import os
import json
import sys
import subprocess
import threading
import time
import webbrowser
from http.server import HTTPServer, BaseHTTPRequestHandler
import socketserver
from urllib.parse import parse_qs, urlparse
import sqlite3

from shared_utils import (
    SCRIPT_DIR, PROJECT_ROOT, WINDBOT_DIR, LIVE_LOGS_DIR, OPP_MEMORY_PATH,
    get_available_decks, get_registry_paths,
)

REAL_REGISTRY_PATH = os.path.join(WINDBOT_DIR, "config", "cards_registry.json")

active_process = None
active_bots = []
spawned_sessions = {}

def kill_bots_on_port(port):
    global spawned_sessions
    if port in spawned_sessions:
        session = spawned_sessions[port]
        for p in session["bots"]:
            if p.poll() is None:
                try:
                    p.terminate()
                    p.wait(timeout=1.0)
                except:
                    try:
                        p.kill()
                    except:
                        pass
        del spawned_sessions[port]

def spawn_bots_on_port(deck_a, deck_b, port):
    global spawned_sessions
    kill_bots_on_port(port)
    
    windbot_exe = os.path.join(PROJECT_ROOT, "WindBot", "WindBot.exe")
    windbot_dir = os.path.join(PROJECT_ROOT, "WindBot")
    
    try:
        # Launch Host Bot (Bot A)
        p1 = subprocess.Popen(
            [windbot_exe, f"name=IgnisBot_A", f"deck={deck_a}", f"port={port}", "hostinfo=", "version=720937"],
            cwd=windbot_dir,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT
        )
        time.sleep(1.0)
        
        # Launch Opponent Bot (Bot B)
        p2 = subprocess.Popen(
            [windbot_exe, f"name=IgnisBot_B", f"deck={deck_b}", f"port={port}", "hostinfo=", "version=720937"],
            cwd=windbot_dir,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT
        )
        
        # Store process handles
        spawned_sessions[port] = {
            "bots": [p1, p2],
            "deck_a": deck_a,
            "deck_b": deck_b
        }
        
        # Read outputs in daemon threads to prevent pipe blocking
        def discard_output(proc):
            try:
                for _ in iter(proc.stdout.readline, b''):
                    pass
            except:
                pass
                
        threading.Thread(target=discard_output, args=(p1,), daemon=True).start()
        threading.Thread(target=discard_output, args=(p2,), daemon=True).start()
        
        return True
    except Exception as e:
        print(f"Error spawning bots on port {port}: {e}")
        return False

progress_log_lock = threading.Lock()

def write_progress_log(path, content, mode="a"):
    with progress_log_lock:
        for _ in range(10):
            try:
                with open(path, mode, encoding="utf-8") as f:
                    f.write(content)
                return True
            except PermissionError:
                time.sleep(0.05)
            except Exception:
                break
        return False

def read_progress_log(path):
    with progress_log_lock:
        for _ in range(10):
            try:
                if os.path.exists(path):
                    with open(path, "r", encoding="utf-8") as f:
                        return f.read()
                return ""
            except PermissionError:
                time.sleep(0.05)
            except Exception:
                break
        return ""

def kill_active_process():
    global active_process, active_bots, spawned_sessions
    if active_process and active_process.poll() is None:
        try:
            active_process.terminate()
            active_process.wait(timeout=2.0)
        except Exception:
            try:
                active_process.kill()
            except:
                pass
        active_process = None
        
    for p in active_bots:
        if p.poll() is None:
            try:
                p.terminate()
            except:
                pass
    active_bots = []
    
    # Kill all spawned sessions
    ports = list(spawned_sessions.keys())
    for port in ports:
        kill_bots_on_port(port)

def get_opponent_deck_name(opp_name):
    bots_json_path = os.path.join(PROJECT_ROOT, "WindBot", "bots.json")
    if os.path.exists(bots_json_path):
        try:
            with open(bots_json_path, "r", encoding="utf-8") as f:
                bots = json.load(f)
                for bot in bots:
                    if bot.get("name") == opp_name:
                        return bot.get("deck")
        except:
            pass
    return opp_name

def read_process_output(proc, log_file_path):
    try:
        for line in iter(proc.stdout.readline, b''):
            decoded_line = line.decode('utf-8', errors='replace')
            write_progress_log(log_file_path, decoded_line)
        proc.wait()
        write_progress_log(log_file_path, "\n==================================================\nการทำงานเสร็จสมบูรณ์\n==================================================\n")
    except Exception as e:
        write_progress_log(log_file_path, f"\nเกิดข้อผิดพลาดในการอ่านข้อมูล: {str(e)}\n")

def consume_stream(proc, name, log_file_path):
    try:
        for line in iter(proc.stdout.readline, b''):
            decoded = line.decode('utf-8', errors='replace')
            if "[IgnisEngine]" in decoded:
                write_progress_log(log_file_path, f"[{name}] {decoded}")
    except Exception as e:
        write_progress_log(log_file_path, f"[{name}] เกิดข้อผิดพลาดในการอ่านลอก: {str(e)}\n")


def run_live_duel_loop(deck, opponent, opp_deck, iterations, progress_log, port=7911, bot_count=2):
    global active_bots
    windbot_exe = os.path.join(PROJECT_ROOT, "WindBot", "WindBot.exe")
    windbot_dir = os.path.join(PROJECT_ROOT, "WindBot")
    
    for i in range(1, iterations + 1):
        write_progress_log(progress_log, f"\n==================================================\nเริ่มต้นรอบที่ {i} / {iterations} บนพอร์ต {port}\n==================================================\n")
        try:
            p1 = subprocess.Popen(
                [windbot_exe, f"name=IgnisBot", f"deck={deck}", f"port={port}", "hostinfo=", "version=720937"],
                cwd=windbot_dir,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT
            )
            
            p2 = None
            if bot_count == 2:
                time.sleep(1.0)
                p2 = subprocess.Popen(
                    [windbot_exe, f"name={opponent}", f"deck={opp_deck}", f"port={port}", "hostinfo=", "version=720937"],
                    cwd=windbot_dir,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.STDOUT
                )
                active_bots = [p1, p2]
                
                t1 = threading.Thread(target=consume_stream, args=(p1, "IgnisBot", progress_log), daemon=True)
                t2 = threading.Thread(target=consume_stream, args=(p2, opponent, progress_log), daemon=True)
                t1.start()
                t2.start()
                
                while p1.poll() is None or p2.poll() is None:
                    time.sleep(1.0)
                    if not active_bots:
                        break
            else:
                active_bots = [p1]
                t1 = threading.Thread(target=consume_stream, args=(p1, "IgnisBot", progress_log), daemon=True)
                t1.start()
                
                while p1.poll() is None:
                    time.sleep(1.0)
                    if not active_bots:
                        break
            
            try:
                import shutil
                reg_name = f"cards_registry_{deck}.json" if deck else "cards_registry.json"
                src_reg = os.path.join(PROJECT_ROOT, "WindBot", "config", reg_name)
                if not os.path.exists(src_reg):
                    reg_name = "cards_registry.json"
                    src_reg = os.path.join(PROJECT_ROOT, "WindBot", "config", reg_name)
                
                dst_reg = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", reg_name)
                
                if os.path.exists(src_reg):
                    shutil.copy2(src_reg, dst_reg)
                    write_progress_log(progress_log, f"Synced card registry {reg_name} to sandbox successfully.\n")
                else:
                    write_progress_log(progress_log, f"Card registry not found at {src_reg}\n")
                
                opp_mem_src = os.path.join(PROJECT_ROOT, "WindBot", "config", "opponent_memory.json")
                opp_mem_dst = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", "opponent_memory.json")
                if os.path.exists(opp_mem_src):
                    shutil.copy2(opp_mem_src, opp_mem_dst)
                    write_progress_log(progress_log, "Synced opponent_memory.json to sandbox successfully.\n")
                else:
                    write_progress_log(progress_log, f"Opponent memory not found at {opp_mem_src}\n")
                
                compile_bat = os.path.join(PROJECT_ROOT, "WindBot", "compile_ai.bat")
                if os.path.exists(compile_bat):
                    write_progress_log(progress_log, "Executing compile_ai.bat...\n")
                    res = subprocess.run(
                        [compile_bat],
                        cwd=os.path.join(PROJECT_ROOT, "WindBot"),
                        stdout=subprocess.PIPE,
                        stderr=subprocess.STDOUT,
                        shell=True
                    )
                    compile_output = res.stdout.decode('utf-8', errors='replace')
                    if res.returncode == 0:
                        write_progress_log(progress_log, "compile_ai.bat executed successfully.\n")
                    else:
                        write_progress_log(progress_log, f"compile_ai.bat returned error code {res.returncode}. Output:\n{compile_output}\n")
                else:
                    write_progress_log(progress_log, f"compile_ai.bat not found at {compile_bat}\n")
            except Exception as ex:
                write_progress_log(progress_log, f"Error in post-duel sync/compile: {ex}\n")
            
            if not active_bots:
                break
                
            time.sleep(1.0)
        except Exception as e:
            write_progress_log(progress_log, f"เกิดข้อผิดพลาดในการรันรอบที่ {i}: {str(e)}\n")
            break
            
    active_bots = []
    write_progress_log(progress_log, "\n==================================================\nการจำลองการดวลจริงทั้งหมดเสร็จสิ้นลงแล้ว\n==================================================\n")

# get_available_decks() is now imported from shared_utils

def get_opponent_bots():
    bots_json_path = os.path.join(PROJECT_ROOT, "WindBot", "bots.json")
    if not os.path.exists(bots_json_path):
        return []
    try:
        with open(bots_json_path, "r", encoding="utf-8") as f:
            bots = json.load(f)
            return sorted([bot.get("name") for bot in bots if bot.get("name")])
    except:
        return []

def get_match_logs_count():
    if not os.path.exists(LIVE_LOGS_DIR):
        return 0
    count = 0
    for entry in os.listdir(LIVE_LOGS_DIR):
        full_path = os.path.join(LIVE_LOGS_DIR, entry)
        if os.path.isdir(full_path) and os.path.exists(os.path.join(full_path, "match_summary.log")):
            count += 1
    return count

def get_registry_card_count(path):
    if not os.path.exists(path):
        return 0
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
            return len(data)
    except:
        return 0

def get_opponent_memory_count():
    if not os.path.exists(OPP_MEMORY_PATH):
        return 0
    try:
        with open(OPP_MEMORY_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)
            return len(data)
    except:
        return 0

def parse_match_history():
    """Parse all match log folders into structured data for analytics"""
    matches = []
    if not os.path.exists(LIVE_LOGS_DIR):
        return matches
    for entry in sorted(os.listdir(LIVE_LOGS_DIR)):
        match_dir = os.path.join(LIVE_LOGS_DIR, entry)
        if not os.path.isdir(match_dir):
            continue
        summary_file = os.path.join(match_dir, "match_summary.log")
        decisions_file = os.path.join(match_dir, "decisions.jsonl")
        match_data = {
            "id": entry, "deck": "", "timestamp": "", "playstyle": "",
            "bot_lp": 8000, "opp_lp": 8000, "lp_diff": 0,
            "turns": 0, "total_decisions": 0, "avg_score": 0,
            "dangers_learned": [], "goals_used": {}
        }
        # Parse match_summary.log
        if os.path.exists(summary_file):
            try:
                with open(summary_file, "r", encoding="utf-8") as f:
                    for line in f:
                        line = line.strip()
                        if "] Deck:" in line:
                            match_data["deck"] = line.split("Deck:")[1].strip()
                        elif "] Playstyle:" in line:
                            match_data["playstyle"] = line.split("Playstyle:")[1].strip()
                        elif "Final Bot LP:" in line:
                            try: match_data["bot_lp"] = int(line.split("Final Bot LP:")[1].strip())
                            except: pass
                        elif "Final Opponent LP:" in line:
                            try: match_data["opp_lp"] = int(line.split("Final Opponent LP:")[1].strip())
                            except: pass
                        elif "marked dangerous" in line:
                            try:
                                name_part = line.split("(")[1].split(")")[0]
                                danger_part = line.split("danger ")[1]
                                vals = danger_part.split("->")
                                match_data["dangers_learned"].append({
                                    "name": name_part,
                                    "before": float(vals[0]),
                                    "after": float(vals[1])
                                })
                            except: pass
            except: pass
        match_data["lp_diff"] = match_data["bot_lp"] - match_data["opp_lp"]
        # Parse decisions.jsonl (deduplicated)
        if os.path.exists(decisions_file):
            seen = set()
            scores = []
            max_turn = 0
            goals = {}
            try:
                with open(decisions_file, "r", encoding="utf-8") as f:
                    for line in f:
                        line = line.strip()
                        if not line:
                            continue
                        try:
                            d = json.loads(line)
                            key = (d.get("turn"), d.get("card_id"), d.get("action"))
                            if key in seen:
                                continue
                            seen.add(key)
                            scores.append(d.get("score", 0))
                            max_turn = max(max_turn, d.get("turn", 0))
                            g = d.get("goal", "unknown")
                            goals[g] = goals.get(g, 0) + 1
                        except: pass
            except: pass
            match_data["total_decisions"] = len(scores)
            match_data["avg_score"] = round(sum(scores) / max(len(scores), 1), 1)
            match_data["turns"] = max_turn
            match_data["goals_used"] = goals
        # Extract timestamp from folder name
        parts = entry.split("_")
        if len(parts) >= 4:
            try:
                date_s = parts[-3]
                time_s = parts[-2]
                match_data["timestamp"] = f"{date_s[:4]}-{date_s[4:6]}-{date_s[6:8]} {time_s[:2]}:{time_s[2:4]}:{time_s[4:6]}"
            except: pass
        matches.append(match_data)
    matches.sort(key=lambda m: m.get("timestamp", ""))
    return matches


def get_registry_snapshot_data(registry_path=None):
    """Get combined registry + opponent memory analysis for analytics"""
    if registry_path is None:
        registry_path = REAL_REGISTRY_PATH
    result = {"cards": [], "opponents": [], "health": {}}
    if os.path.exists(registry_path):
        try:
            with open(registry_path, "r", encoding="utf-8-sig") as f:
                cards = json.load(f)
                result["cards"] = cards
                total = len(cards)
                if total > 0:
                    high_pri = sum(1 for c in cards if c.get("priority", 0) >= 9)
                    max_pri = sum(1 for c in cards if c.get("priority", 0) >= 10)
                    high_bait = sum(1 for c in cards if c.get("bait_value", 0) >= 6)
                    avg_pri = sum(c.get("priority", 0) for c in cards) / total
                    avg_bait = sum(c.get("bait_value", 0) for c in cards) / total
                    roles = {}
                    for c in cards:
                        for role in c.get("roles", []):
                            roles[role] = roles.get(role, 0) + 1
                    pri_dist = {}
                    for c in cards:
                        p = str(c.get("priority", 0))
                        pri_dist[p] = pri_dist.get(p, 0) + 1
                    result["health"] = {
                        "total_cards": total,
                        "high_priority_count": high_pri,
                        "max_priority_count": max_pri,
                        "high_priority_pct": round(high_pri / total * 100, 1),
                        "high_bait_count": high_bait,
                        "avg_priority": round(avg_pri, 2),
                        "avg_bait": round(avg_bait, 2),
                        "role_distribution": roles,
                        "priority_distribution": pri_dist
                    }
        except: pass
    if os.path.exists(OPP_MEMORY_PATH):
        try:
            with open(OPP_MEMORY_PATH, "r", encoding="utf-8-sig") as f:
                opp_data = json.load(f)
                opponents = []
                for cid, data in opp_data.items():
                    opponents.append({
                        "id": int(cid),
                        "name": data.get("name", f"Card #{cid}"),
                        "times_seen": data.get("times_seen", 0),
                        "times_disrupted": data.get("times_disrupted_us", 0),
                        "danger": data.get("learned_danger", 0)
                    })
                opponents.sort(key=lambda x: x["danger"], reverse=True)
                result["opponents"] = opponents
        except: pass
    return result


def _load_template(name):
    """Load an HTML template from the templates/ directory."""
    path = os.path.join(SCRIPT_DIR, "templates", name)
    with open(path, "r", encoding="utf-8") as f:
        return f.read()

HTML_TEMPLATE = _load_template("analytics.html")


ANALYTICS_TEMPLATE = _load_template("analytics.html")
PROGRESS_TEMPLATE = _load_template("progress.html")

def get_progress_report(deck_name):
    # Automatically sync latest finished matches to statistics.db
    try:
        sync_script = os.path.join(PROJECT_ROOT, "Developer", "scratch", "save_outcomes_to_sql.py")
        if os.path.exists(sync_script):
            subprocess.run([sys.executable, sync_script], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=5.0)
    except Exception as e:
        print(f"Error auto-syncing database: {e}")
        
    db_path = os.path.join(PROJECT_ROOT, "Developer", "scratch", "statistics.db")
    if not os.path.exists(db_path):
        return {"error": "ไม่พบไฟล์ฐานข้อมูล (Database file not found)"}
        
    try:
        conn = sqlite3.connect(db_path, timeout=30.0)
        cursor = conn.cursor()
        
        # Query matches for this deck, ordered by ID ascending (chronological)
        cursor.execute("""
            SELECT id, outcome, bot_lp, opp_lp, turns 
            FROM matches 
            WHERE deck_self = ? 
            ORDER BY id ASC
        """, (deck_name,))
        rows = cursor.fetchall()
        
        if not rows:
            conn.close()
            return {"error": f"ไม่พบข้อมูลแมตช์ของเด็ค {deck_name} ในฐานข้อมูล"}
            
        num_matches = len(rows)
        
        # Calculate Before vs After (first 25% of matches vs last 25% of matches)
        split_size = max(1, num_matches // 4)
        
        first_chunk = rows[:split_size]
        last_chunk = rows[-split_size:]
        
        def get_chunk_metrics(chunk):
            wins = sum(1 for r in chunk if r[1] in ("Win", "WeakWin"))
            total = len(chunk)
            win_rate = round((wins / total) * 100.0, 1) if total > 0 else 0.0
            
            turns_list = []
            for r in chunk:
                try:
                    t = int(r[4])
                    if t > 0:
                        turns_list.append(t)
                except:
                    pass
            avg_turns = round(sum(turns_list) / len(turns_list), 1) if turns_list else 0.0
            avg_bot_lp = round(sum(r[2] for r in chunk) / total, 1) if total > 0 else 0.0
            avg_opp_lp = round(sum(r[3] for r in chunk) / total, 1) if total > 0 else 0.0
            
            chunk_match_ids = [r[0] for r in chunk]
            placeholders = ",".join("?" for _ in chunk_match_ids)
            cursor.execute(f"""
                SELECT AVG(score) 
                FROM decisions 
                WHERE match_id IN ({placeholders}) AND decision = 1
            """, chunk_match_ids)
            avg_score_row = cursor.fetchone()
            avg_score = round(avg_score_row[0], 2) if avg_score_row and avg_score_row[0] is not None else 0.0
            
            return {
                "win_rate": win_rate,
                "avg_turns": avg_turns,
                "avg_bot_lp": avg_bot_lp,
                "avg_opp_lp": avg_opp_lp,
                "avg_score": avg_score
            }
            
        before = get_chunk_metrics(first_chunk)
        after = get_chunk_metrics(last_chunk)
        
        comparison = {
            "win_rate_before": before["win_rate"],
            "win_rate_after": after["win_rate"],
            "win_rate_delta": round(after["win_rate"] - before["win_rate"], 1),
            
            "score_before": before["avg_score"],
            "score_after": after["avg_score"],
            "score_delta": round(after["avg_score"] - before["avg_score"], 2),
            
            "turns_before": before["avg_turns"],
            "turns_after": after["avg_turns"],
            "turns_delta": round(after["avg_turns"] - before["avg_turns"], 1),
            
            "bot_lp_before": before["avg_bot_lp"],
            "bot_lp_after": after["avg_bot_lp"],
            
            "opp_lp_before": before["avg_opp_lp"],
            "opp_lp_after": after["avg_opp_lp"]
        }
        
        # Calculate cumulative metrics for each step.
        # To avoid cluttering the line chart, downsample to maximum 30 points.
        step = max(1, num_matches // 30)
        blocks = []
        
        for idx in range(0, num_matches, step):
            sub_rows = rows[:idx+1]
            total_sub = len(sub_rows)
            wins_sub = sum(1 for r in sub_rows if r[1] in ("Win", "WeakWin"))
            losses_sub = sum(1 for r in sub_rows if r[1] in ("Loss", "WeakLoss"))
            
            win_rate = round((wins_sub / total_sub) * 100.0, 1)
            
            turns_list = []
            for r in sub_rows:
                try:
                    t = int(r[4])
                    if t > 0:
                        turns_list.append(t)
                except:
                    pass
            avg_turns = round(sum(turns_list) / len(turns_list), 1) if turns_list else 0.0
            
            sub_ids = [r[0] for r in sub_rows]
            placeholders = ",".join("?" for _ in sub_ids)
            cursor.execute(f"""
                SELECT AVG(score) 
                FROM decisions 
                WHERE match_id IN ({placeholders}) AND decision = 1
            """, sub_ids)
            avg_score_row = cursor.fetchone()
            avg_score = round(avg_score_row[0], 2) if avg_score_row and avg_score_row[0] is not None else 0.0
            
            blocks.append({
                "block_index": len(blocks) + 1,
                "matches_count": total_sub,
                "win_rate": win_rate,
                "avg_turns": avg_turns,
                "avg_bot_lp": round(sum(r[2] for r in sub_rows) / total_sub, 1),
                "avg_opp_lp": round(sum(r[3] for r in sub_rows) / total_sub, 1),
                "avg_score": avg_score,
                "wins": wins_sub,
                "losses": losses_sub,
                "draws": total_sub - wins_sub - losses_sub
            })
            
        conn.close()
        
        return {
            "deck": deck_name,
            "total_matches": num_matches,
            "blocks": blocks,
            "comparison": comparison
        }
    except Exception as e:
        return {"error": f"เกิดข้อผิดพลาดในการดึงข้อมูลฐานข้อมูล: {str(e)}"}

class CockpitHandler(BaseHTTPRequestHandler):
    def log_message(self, format, *args):
        pass # Suppress logging request noise to console
        
    def do_GET(self):
        url = urlparse(self.path)
        if url.path in ('/', '/index.html'):
            self.send_response(200)
            self.send_header('Content-Type', 'text/html; charset=utf-8')
            self.end_headers()
            self.wfile.write(HTML_TEMPLATE.encode('utf-8'))
            
        elif url.path == '/api/decks':
            decks = get_available_decks(ai_only=True)
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(decks).encode('utf-8'))
            
        elif url.path == '/api/opponents':
            opps = get_opponent_bots()
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(opps).encode('utf-8'))
            
        elif url.path == '/api/status':
            query = parse_qs(url.query)
            deck = query.get('deck', [''])[0]
            if deck and deck != "all":
                reg_file = f"cards_registry_{deck}.json"
            else:
                reg_file = "cards_registry.json"
                
            sandbox_reg = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", reg_file)
            live_reg = os.path.join(PROJECT_ROOT, "WindBot", "config", reg_file)
            
            stats = {
                "registry_count": get_registry_card_count(sandbox_reg),
                "live_registry_count": get_registry_card_count(live_reg),
                "match_logs_count": get_match_logs_count(),
                "opponent_memory_count": get_opponent_memory_count(),
                "is_training": (active_process is not None and active_process.poll() is None) or len(active_bots) > 0
            }
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(stats).encode('utf-8'))
            
        elif url.path == '/api/progress':
            progress_log = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", "training_progress.log")
            content = read_progress_log(progress_log)
            self.send_response(200)
            self.send_header('Content-Type', 'text/plain; charset=utf-8')
            self.end_headers()
            self.wfile.write(content.encode('utf-8'))
            
        elif url.path == '/analytics':
            self.send_response(200)
            self.send_header('Content-Type', 'text/html; charset=utf-8')
            self.end_headers()
            self.wfile.write(ANALYTICS_TEMPLATE.encode('utf-8'))
            
        elif url.path == '/progress':
            self.send_response(200)
            self.send_header('Content-Type', 'text/html; charset=utf-8')
            self.end_headers()
            self.wfile.write(PROGRESS_TEMPLATE.encode('utf-8'))
            
        elif url.path == '/api/match_history':
            data = parse_match_history()
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(data).encode('utf-8'))
            
        elif url.path == '/api/progress_report':
            query = parse_qs(url.query)
            deck = query.get('deck', [''])[0]
            
            data = get_progress_report(deck)
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(data).encode('utf-8'))
            
        elif url.path == '/api/registry_snapshot':
            query = parse_qs(url.query)
            deck = query.get('deck', [''])[0]
            if deck and deck != "all":
                reg_file = f"cards_registry_{deck}.json"
            else:
                reg_file = "cards_registry.json"
            live_reg = os.path.join(PROJECT_ROOT, "WindBot", "config", reg_file)
            
            data = get_registry_snapshot_data(live_reg)
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(data).encode('utf-8'))
            
        elif url.path == '/api/active_spawns':
            spawns_list = []
            for p_port, session in spawned_sessions.items():
                spawns_list.append({
                    "port": p_port,
                    "deck_a": session["deck_a"],
                    "deck_b": session["deck_b"]
                })
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps(spawns_list).encode('utf-8'))
            
        else:
            self.send_response(404)
            self.end_headers()

    def do_POST(self):
        url = urlparse(self.path)
        if url.path == '/api/train':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length).decode('utf-8')
            params = json.loads(post_data)
            
            deck = params.get('deck', '')
            opponent = params.get('opponent', '')
            mode = params.get('mode', 'heuristic')
            iterations = int(params.get('iterations', 300))
            port = int(params.get('port', 7911))
            bot_count = int(params.get('bot_count', 2))
            
            success = self.start_training(deck, opponent, mode, iterations, port, bot_count)
            
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({"success": success}).encode('utf-8'))
            
        elif url.path == '/api/kill':
            kill_active_process()
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({"success": True}).encode('utf-8'))
            
        elif url.path == '/api/deploy':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length).decode('utf-8') if content_length > 0 else "{}"
            params = json.loads(post_data) if post_data else {}
            deck = params.get('deck', '')
            
            success, output = self.deploy_config(deck)
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({"success": success, "output": output}).encode('utf-8'))
            
        elif url.path == '/api/spawn_bots':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length).decode('utf-8')
            params = json.loads(post_data)
            
            deck_a = params.get('deck_a', '')
            deck_b = params.get('deck_b', '')
            port = int(params.get('port', 7911))
            
            success = spawn_bots_on_port(deck_a, deck_b, port)
            
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({"success": success}).encode('utf-8'))
            
        elif url.path == '/api/kill_port':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length).decode('utf-8')
            params = json.loads(post_data)
            
            port = int(params.get('port', 7911))
            kill_bots_on_port(port)
            
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.end_headers()
            self.wfile.write(json.dumps({"success": True}).encode('utf-8'))
            
        else:
            self.send_response(404)
            self.end_headers()

    def start_training(self, deck, opponent, mode, iterations, port=7911, bot_count=2):
        global active_process, active_bots
        kill_active_process()
        
        progress_log = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", "training_progress.log")
        os.makedirs(os.path.dirname(progress_log), exist_ok=True)
        
        write_progress_log(progress_log, "ระบบดำเนินการ: เริ่มต้นระบบการประมวลผล...\n", "w")
        
        sandbox_dir = os.path.join(PROJECT_ROOT, "WindBot_Sandbox")
        cmd = []
        
        if mode == 'heuristic':
            cmd = [
                sys.executable,
                os.path.join(sandbox_dir, "optimize_registry.py"),
                "--deck", deck,
                "--iterations", str(iterations)
            ]
            log_msg = f"คำสั่งระบบ: ปรับจูนน้ำหนัก Heuristic (เด็ค: {deck}, รอบ: {iterations})\n"
        elif mode == 'simulator':
            cmd = [
                sys.executable,
                os.path.join(sandbox_dir, "combo_simulator.py"),
                "--deck", deck,
                "--simulations", str(iterations),
                "--optimize"
            ]
            log_msg = f"คำสั่งระบบ: รันจำลองผลการจั่วการ์ด (เด็ค: {deck}, รอบจำลอง: {iterations})\n"
        elif mode == 'real_match':
            cmd = [
                sys.executable,
                os.path.join(sandbox_dir, "run_match_learning.py"),
                "--deck", deck
            ]
            log_msg = f"คำสั่งระบบ: ปรับปรุงความสามารถบอทด้วยการเรียนรู้เสริมกำลังย้อนหลัง (Reinforcement Learning) (เด็ค: {deck})\n"
        elif mode == 'ab_tournament':
            reg_a = os.path.join(PROJECT_ROOT, "WindBot", "config", f"cards_registry_{deck}.json" if deck else "cards_registry.json")
            reg_b = os.path.join(sandbox_dir, f"cards_registry_{deck}.json" if deck else "cards_registry.json")
            
            # Make sure LIVE config exists, fallback to default if not
            if not os.path.exists(reg_a):
                reg_a = os.path.join(PROJECT_ROOT, "WindBot", "config", "cards_registry.json")
            if not os.path.exists(reg_b):
                reg_b = os.path.join(sandbox_dir, "cards_registry.json")
                
            cmd = [
                sys.executable,
                os.path.join(sandbox_dir, "ab_tournament.py"),
                "--deck", deck,
                "--regA", reg_a,
                "--regB", reg_b,
                "--matches", str(iterations)
            ]
            log_msg = f"คำสั่งระบบ: เริ่มต้นทัวร์นาเมนต์ A/B ทดสอบอัตราชนะ (เด็ค: {deck}, จำนวนแมตช์: {iterations})\n"
        elif mode == 'live_duel':
            log_msg = f"คำสั่งระบบ: เปิดจำลองการแข่งจริง {iterations} รอบ บนพอร์ต {port} (จำนวนบอท: {bot_count}) (เด็คหลักเรา: {deck} ปะทะ คู่ซ้อม: {opponent})\n"
            write_progress_log(progress_log, log_msg + f"โปรดตรวจสอบ: ตรวจสอบห้องและเซิร์ฟเวอร์ YGOPro/EDOPro บนพอร์ต {port} ในเครื่องของท่าน\n")
                
            opp_deck = get_opponent_deck_name(opponent)
            
            try:
                threading.Thread(
                    target=run_live_duel_loop,
                    args=(deck, opponent, opp_deck, iterations, progress_log, port, bot_count),
                    daemon=True
                ).start()
                return True
            except Exception as e:
                write_progress_log(progress_log, f"เกิดข้อผิดพลาดในการเริ่มต้นรอบจำลอง: {str(e)}\n")
                return False

        if cmd:
            write_progress_log(progress_log, log_msg)
            try:
                active_process = subprocess.Popen(
                    cmd,
                    cwd=sandbox_dir,
                    stdout=subprocess.PIPE,
                    stderr=subprocess.STDOUT
                )
                threading.Thread(target=read_process_output, args=(active_process, progress_log), daemon=True).start()
                return True
            except Exception as e:
                write_progress_log(progress_log, f"เกิดข้อผิดพลาดในการเปิดรันกระบวนการ: {str(e)}\n")
                return False
        return False

    def deploy_config(self, deck=""):
        if deck and deck != "all":
            reg_file = f"cards_registry_{deck}.json"
        else:
            reg_file = "cards_registry.json"
            
        sandbox_reg = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", reg_file)
        live_reg = os.path.join(PROJECT_ROOT, "WindBot", "config", reg_file)
        
        if os.path.exists(sandbox_reg):
            try:
                import shutil
                shutil.copy2(sandbox_reg, live_reg)
            except Exception as e:
                return False, f"ไม่สามารถคัดลอกไฟล์การตั้งค่า: {str(e)}"
        else:
            return False, f"ไม่พบไฟล์ {reg_file} ใน Sandbox"
            
        compile_bat = os.path.join(PROJECT_ROOT, "WindBot", "compile_ai.bat")
        if os.path.exists(compile_bat):
            try:
                res = subprocess.run(
                    [compile_bat],
                    cwd=os.path.join(PROJECT_ROOT, "WindBot"),
                    stdout=subprocess.PIPE,
                    stderr=subprocess.STDOUT,
                    shell=True
                )
                output = res.stdout.decode('utf-8', errors='replace')
                success = res.returncode == 0
                return success, output
            except Exception as e:
                return False, f"ข้อผิดพลาดระหว่างรันคอมไพล์: {str(e)}"
        else:
            return False, "ไม่พบไฟล์ compile_ai.bat ในโฟลเดอร์รันบอท"

def run_server(port=8000):
    server_address = ('', port)
    httpd = ThreadedHTTPServer(server_address, CockpitHandler)
    print(f"ระบบ Cockpit เริ่มการทำวานบนพอร์ต: {port}")
    print(f"โปรแกรมกำลังเปิดหน้าเว็บเบราว์เซอร์อัตโนมัติ...")
    webbrowser.open(f"http://localhost:{port}")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nกำลังปิดระบบ...")
        kill_active_process()
        sys.exit(0)

class ThreadedHTTPServer(socketserver.ThreadingMixIn, HTTPServer):
    allow_reuse_address = True

if __name__ == "__main__":
    port = 8000
    if len(sys.argv) > 1:
        try:
            port = int(sys.argv[1])
        except ValueError:
            pass
    run_server(port)
