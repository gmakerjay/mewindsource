import os
import re
import glob
import sqlite3
import json
import time
import random

def execute_write_transaction(db_path, write_func, max_retries=10, initial_delay=0.1):
    """
    Executes a database write function within a transaction, retrying on locking errors
    with exponential backoff and random jitter.
    """
    delay = initial_delay
    for attempt in range(max_retries):
        conn = None
        try:
            conn = sqlite3.connect(db_path, timeout=30.0)
            # Enable WAL mode, NORMAL synchronous, and foreign keys
            conn.execute("PRAGMA journal_mode = WAL;")
            conn.execute("PRAGMA synchronous = NORMAL;")
            conn.execute("PRAGMA foreign_keys = ON;")
            
            # Start transaction immediately with write lock to prevent promotion deadlocks
            conn.execute("BEGIN IMMEDIATE;")
            
            # Run the write operations
            result = write_func(conn)
            
            # Commit the transaction
            conn.commit()
            return result
        except sqlite3.OperationalError as e:
            if "locked" in str(e).lower():
                if conn:
                    try:
                        conn.rollback()
                    except:
                        pass
                sleep_time = delay * (1.5 ** attempt) + random.uniform(0, 0.1)
                time.sleep(sleep_time)
            else:
                if conn:
                    try:
                        conn.rollback()
                    except:
                        pass
                raise e
        except Exception as e:
            if conn:
                try:
                    conn.rollback()
                except:
                    pass
            raise e
        finally:
            if conn:
                conn.close()
    raise sqlite3.OperationalError("Database locked. Maximum retries exceeded.")

def init_db_tables(conn):
    """Initializes the database schema if tables do not exist."""
    cursor = conn.cursor()
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS matches (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            session_name TEXT UNIQUE,
            deck_self TEXT,
            opponent_deck TEXT,
            outcome TEXT,
            bot_lp INTEGER,
            opp_lp INTEGER,
            turns INTEGER
        )
    """)
    # Check if opponent_deck column exists, if not, add it
    cursor.execute("PRAGMA table_info(matches)")
    columns = [col[1] for col in cursor.fetchall()]
    if "opponent_deck" not in columns:
        cursor.execute("ALTER TABLE matches ADD COLUMN opponent_deck TEXT DEFAULT 'Unknown'")
    cursor.execute("""
        CREATE TABLE IF NOT EXISTS decisions (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            match_id INTEGER,
            turn INTEGER,
            card_id INTEGER,
            card_name TEXT,
            action TEXT,
            goal TEXT,
            score REAL,
            decision INTEGER,
            plan TEXT,
            lp_self INTEGER,
            lp_opp INTEGER,
            opponent_threat REAL,
            bot_monsters TEXT,
            opp_monsters TEXT,
            opp_spells TEXT,
            bot_hand TEXT,
            FOREIGN KEY(match_id) REFERENCES matches(id) ON DELETE CASCADE
        )
    """)

def is_game_restart(dec, prev_dec):
    """
    Detects if the current decision indicates the start of a new game,
    even if the turn number remains 1.
    """
    if not prev_dec:
        return False
        
    turn = dec.get("turn", 0)
    prev_turn = prev_dec.get("turn", 0)
    
    # 1. Turn number went down (standard reset)
    if turn < prev_turn:
        return True
        
    # 2. Turn number is 1, and the previous turn was also 1 (potential restart on Turn 1)
    if turn == 1 and prev_turn == 1:
        # Check LP reset to 8000/8000 from a different value
        lp_self = dec.get("lp_self", 8000)
        lp_opp = dec.get("lp_opp", 8000)
        prev_lp_self = prev_dec.get("lp_self", 8000)
        prev_lp_opp = prev_dec.get("lp_opp", 8000)
        
        if (lp_self == 8000 and lp_opp == 8000) and (prev_lp_self != 8000 or prev_lp_opp != 8000):
            return True
            
        # Check board clearing
        bot_monsters = dec.get("bot_monsters", [])
        opp_monsters = dec.get("opp_monsters", [])
        opp_spells = dec.get("opp_spells", [])
        
        prev_bot_monsters = prev_dec.get("bot_monsters", [])
        prev_opp_monsters = prev_dec.get("opp_monsters", [])
        prev_opp_spells = prev_dec.get("opp_spells", [])
        
        # If board was not empty previously, but is now empty
        if (prev_bot_monsters or prev_opp_monsters or prev_opp_spells) and not (bot_monsters or opp_monsters or opp_spells):
            return True
            
        # Check hand reset (disjoint set of cards in hand)
        hand = {c.get("id") for c in dec.get("bot_hand", []) if isinstance(c, dict) and c.get("id")}
        prev_hand = {c.get("id") for c in prev_dec.get("bot_hand", []) if isinstance(c, dict) and c.get("id")}
        
        if hand and prev_hand:
            # If the hand set has zero overlap and is normal size, it's a restart
            if not (hand & prev_hand):
                return True
                
    return False

def parse_and_save(deck=None, opp_deck=None, wipe=False):
    db_path = r"c:\Users\admin\Documents\EDOTh\Developer\scratch\statistics.db"
    
    # 1. Handle wipe if requested
    if wipe:
        def do_wipe(conn):
            cursor = conn.cursor()
            cursor.execute("DROP TABLE IF EXISTS decisions;")
            cursor.execute("DROP TABLE IF EXISTS matches;")
        execute_write_transaction(db_path, do_wipe)
        
    # 2. Ensure tables are initialized safely
    execute_write_transaction(db_path, init_db_tables)
    
    # 3. Read and parse log directories in memory (read-only, no locks)
    logs_root = r"c:\Users\admin\Documents\EDOTh\WindBot\Logs"
    log_dirs = []
    if os.path.exists(logs_root):
        for entry in os.listdir(logs_root):
            full_path = os.path.join(logs_root, entry)
            if os.path.isdir(full_path) and entry.startswith("2026_"):
                log_dirs.append(full_path)
                
    # Pattern to find outcome lines in match_summary.log
    outcome_pat = re.compile(
        r"Applying Real-time Learning:\s+Outcome is\s+(\w+)\s+\(Bot LP:\s+(\d+),\s+Opp LP:\s+(\d+),\s+Turns:\s+(\d+)\)"
    )
    
    parsed_matches = []
    
    for log_dir in log_dirs:
        session_name = os.path.basename(log_dir)
        summary_path = os.path.join(log_dir, "match_summary.log")
        decisions_path = os.path.join(log_dir, "decisions.jsonl")
        
        if not os.path.exists(summary_path):
            continue
            
        with open(summary_path, "r", encoding="utf-8", errors="ignore") as f:
            content = f.read()
            
        matches_found = list(outcome_pat.finditer(content))
        
        # Determine deck self
        deck_self = "Unknown"
        deck_match = re.search(r"Deck:\s*([^\r\n]+)", content)
        if deck_match:
            deck_self = deck_match.group(1).strip()
            
        opponent_deck = "Unknown"
        if deck and opp_deck:
            if deck_self == deck:
                opponent_deck = opp_deck
            elif deck_self == opp_deck:
                opponent_deck = deck
                
        # Parse decisions list
        decisions_list = []
        if os.path.exists(decisions_path):
            with open(decisions_path, "r", encoding="utf-8", errors="ignore") as f:
                for line in f:
                    line = line.strip()
                    if not line:
                        continue
                    try:
                        decisions_list.append(json.loads(line))
                    except:
                        pass
                        
        # Partition decisions list using robust restart detection
        games_decisions = []
        current_game_decs = []
        for dec in decisions_list:
            prev_dec = current_game_decs[-1] if current_game_decs else None
            if is_game_restart(dec, prev_dec):
                if current_game_decs:
                    games_decisions.append(current_game_decs)
                current_game_decs = [dec]
            else:
                current_game_decs.append(dec)
        if current_game_decs:
            games_decisions.append(current_game_decs)
            
        # Match parsed decisions to summary outcomes
        if len(matches_found) > 0:
            for idx, match_m in enumerate(matches_found):
                session_name_g = f"{session_name}_g{idx+1}"
                outcome = match_m.group(1)
                bot_lp = int(match_m.group(2))
                opp_lp = int(match_m.group(3))
                turns = int(match_m.group(4))
                
                game_decs = games_decisions[idx] if idx < len(games_decisions) else []
                parsed_matches.append({
                    "session_name_g": session_name_g,
                    "deck_self": deck_self,
                    "opponent_deck": opponent_deck,
                    "outcome": outcome,
                    "bot_lp": bot_lp,
                    "opp_lp": opp_lp,
                    "turns": turns,
                    "decisions": game_decs
                })
        else:
            # Fallback for single aborted match only if session is finished
            if "=== Duel Session Finished ===" in content:
                bot_lp_match = re.search(r"Final Bot LP:\s*(\d+)", content)
                opp_lp_match = re.search(r"Final Opponent LP:\s*(\d+)", content)
                
                bot_lp = int(bot_lp_match.group(1)) if bot_lp_match else 8000
                opp_lp = int(opp_lp_match.group(1)) if opp_lp_match else 8000
                
                turns = 0
                for f_name in os.listdir(log_dir):
                    if f_name.startswith("turn_") and f_name.endswith(".log"):
                        try:
                            t_num = int(f_name[5:-4])
                            turns = max(turns, t_num)
                        except:
                            pass
                            
                outcome = "Draw"
                if bot_lp == 0 and opp_lp > 0:
                    outcome = "Loss"
                elif opp_lp == 0 and bot_lp > 0:
                    outcome = "Win"
                elif bot_lp > opp_lp + 3000:
                    outcome = "WeakWin"
                elif opp_lp > bot_lp + 3000:
                    outcome = "WeakLoss"
                    
                game_decs = games_decisions[0] if len(games_decisions) > 0 else []
                parsed_matches.append({
                    "session_name_g": session_name,
                    "deck_self": deck_self,
                    "opponent_deck": opponent_deck,
                    "outcome": outcome,
                    "bot_lp": bot_lp,
                    "opp_lp": opp_lp,
                    "turns": turns,
                    "decisions": game_decs
                })
                
    # 4. Save all parsed data within a single write transaction using retry loop
    def do_insert(conn):
        cursor = conn.cursor()
        inserted_matches = 0
        inserted_decisions = 0
        
        for m_data in parsed_matches:
            session_name_g = m_data["session_name_g"]
            deck_self = m_data["deck_self"]
            opponent_deck = m_data["opponent_deck"]
            outcome = m_data["outcome"]
            bot_lp = m_data["bot_lp"]
            opp_lp = m_data["opp_lp"]
            turns = m_data["turns"]
            game_decs = m_data["decisions"]
            
            # Check if this specific game is already in DB
            cursor.execute("SELECT id FROM matches WHERE session_name = ?", (session_name_g,))
            match_row = cursor.fetchone()
            
            match_id = None
            if match_row:
                match_id = match_row[0]
            else:
                try:
                    cursor.execute("""
                        INSERT INTO matches (session_name, deck_self, opponent_deck, outcome, bot_lp, opp_lp, turns)
                        VALUES (?, ?, ?, ?, ?, ?, ?)
                    """, (session_name_g, deck_self, opponent_deck, outcome, bot_lp, opp_lp, turns))
                    match_id = cursor.lastrowid
                    inserted_matches += 1
                except sqlite3.IntegrityError:
                    cursor.execute("SELECT id FROM matches WHERE session_name = ?", (session_name_g,))
                    row = cursor.fetchone()
                    if row:
                        match_id = row[0]
                        
            # Insert decisions for this game if not already present
            if match_id and game_decs:
                cursor.execute("SELECT COUNT(*) FROM decisions WHERE match_id = ?", (match_id,))
                dec_count = cursor.fetchone()[0]
                if dec_count == 0:
                    for dec in game_decs:
                        try:
                            turn = dec.get("turn", 0)
                            card_id = dec.get("card_id", 0)
                            card_name = dec.get("card_name", "Unknown")
                            action = dec.get("action", "")
                            goal = dec.get("goal", "")
                            score = dec.get("score", 0.0)
                            decision_val = 1 if dec.get("decision", False) else 0
                            plan = dec.get("plan", "")
                            lp_self = dec.get("lp_self", 8000)
                            lp_opp = dec.get("lp_opp", 8000)
                            opponent_threat = dec.get("opponent_threat", 0.0)
                            
                            bot_monsters = json.dumps(dec.get("bot_monsters", []))
                            opp_monsters = json.dumps(dec.get("opp_monsters", []))
                            opp_spells = json.dumps(dec.get("opp_spells", []))
                            bot_hand = json.dumps(dec.get("bot_hand", []))
                            
                            cursor.execute("""
                                INSERT INTO decisions (
                                    match_id, turn, card_id, card_name, action, goal, score, decision, plan,
                                    lp_self, lp_opp, opponent_threat, bot_monsters, opp_monsters, opp_spells, bot_hand
                                )
                                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                            """, (
                                match_id, turn, card_id, card_name, action, goal, score, decision_val, plan,
                                lp_self, lp_opp, opponent_threat, bot_monsters, opp_monsters, opp_spells, bot_hand
                            ))
                            inserted_decisions += 1
                        except:
                            pass
        return inserted_matches, inserted_decisions
        
    inserted_matches, inserted_decisions = execute_write_transaction(db_path, do_insert)
    
    # 5. Query summary status (read-only, does not block WAL)
    conn = sqlite3.connect(db_path, timeout=30.0)
    cursor = conn.cursor()
    cursor.execute("SELECT COUNT(*) FROM matches")
    total_matches = cursor.fetchone()[0]
    cursor.execute("SELECT COUNT(*) FROM decisions")
    total_decisions = cursor.fetchone()[0]
    
    print(f"Database successfully updated.")
    print(f"Added {inserted_matches} new match records (Total matches: {total_matches}).")
    print(f"Added {inserted_decisions} new decision logs (Total decision states: {total_decisions}).")
    
    # Query latest outcomes
    cursor.execute("SELECT id, session_name, deck_self, opponent_deck, outcome, bot_lp, opp_lp, turns FROM matches ORDER BY id DESC LIMIT 10")
    rows = cursor.fetchall()
    print("=" * 135)
    print(f"{'ID':<4} | {'Deck (Self)':<16} | {'Opponent':<16} | {'Session Name':<42} | {'Outcome':<8} | {'Bot LP':<6} | {'Opp LP':<6} | {'Turns':<5}")
    print("-" * 135)
    for r in rows:
        print(f"{r[0]:<4} | {r[2]:<16} | {r[3]:<16} | {r[1]:<42} | {r[4]:<8} | {r[5]:<6} | {r[6]:<6} | {r[7]:<5}")
    print("=" * 135)
    
    conn.close()

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description="Save WindBot match outcomes to SQLite")
    parser.add_argument("--deck", type=str, default=None, help="Main deck name")
    parser.add_argument("--opp-deck", type=str, default=None, help="Opponent deck name")
    parser.add_argument("--wipe", action="store_true", help="Wipe database tables before running")
    args = parser.parse_args()
    parse_and_save(deck=args.deck, opp_deck=args.opp_deck, wipe=args.wipe)
