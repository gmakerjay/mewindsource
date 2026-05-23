import os
import sys
import json
import shutil
import subprocess
import time
import socket
import argparse
import re

from shared_utils import (
    configure_utf8, SCRIPT_DIR, PROJECT_ROOT, WINDBOT_DIR, LIVE_LOGS_DIR,
)

configure_utf8()

def check_port_open(ip, port):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(1.0)
    try:
        s.connect((ip, port))
        s.close()
        return True
    except:
        return False

def get_new_match_log(before_dirs, deck_a_name):
    # Scan for new log directories that start with deck_a_name
    after_dirs = set(os.listdir(LIVE_LOGS_DIR)) if os.path.exists(LIVE_LOGS_DIR) else set()
    new_dirs = after_dirs - before_dirs
    
    target_dirs = []
    for d in new_dirs:
        if d.startswith(deck_a_name + "_"):
            summary_path = os.path.join(LIVE_LOGS_DIR, d, "match_summary.log")
            if os.path.exists(summary_path):
                target_dirs.append(os.path.join(LIVE_LOGS_DIR, d))
                
    if target_dirs:
        # Return the most recently modified directory
        target_dirs.sort(key=os.path.getmtime, reverse=True)
        return target_dirs[0]
    return None

def parse_match_outcome(match_dir):
    summary_path = os.path.join(match_dir, "match_summary.log")
    if not os.path.exists(summary_path):
        return "Unknown", 0, 0
    
    bot_lp = 0
    opp_lp = 0
    with open(summary_path, "r", encoding="utf-8") as f:
        content = f.read()
        bot_lp_match = re.search(r"Final Bot LP:\s*(\d+)", content)
        opp_lp_match = re.search(r"Final Opponent LP:\s*(\d+)", content)
        if bot_lp_match and opp_lp_match:
            bot_lp = int(bot_lp_match.group(1))
            opp_lp = int(opp_lp_match.group(1))
            
    # Also count turns from turn logs
    turns = 0
    for filename in os.listdir(match_dir):
        if filename.startswith("turn_") and filename.endswith(".log"):
            try:
                turn_num = int(filename[5:-4])
                turns = max(turns, turn_num)
            except ValueError:
                pass
                
    if bot_lp == 0 and opp_lp > 0:
        return "Loss", bot_lp, opp_lp, turns
    elif opp_lp == 0 and bot_lp > 0:
        return "Win", bot_lp, opp_lp, turns
    else:
        return "Tie/Aborted", bot_lp, opp_lp, turns

def main():
    parser = argparse.ArgumentParser(description="IgnisEngine A/B Tournament Tester")
    parser.add_argument("--deck", type=str, required=True, help="Base deck name (e.g. 2026_AzaYummy)")
    parser.add_argument("--regA", type=str, required=True, help="Registry A JSON path or filename in Sandbox")
    parser.add_argument("--regB", type=str, required=True, help="Registry B JSON path or filename in Sandbox")
    parser.add_argument("--matches", type=int, default=50, help="Number of matches to play")
    parser.add_argument("--port", type=int, default=7911, help="Port of YGOPro server")
    parser.add_argument("--timeout", type=int, default=180, help="Timeout in seconds for a single match")
    args = parser.parse_args()

    print("=== IGNIS ENGINE A/B TOURNAMENT TESTER ===")
    print(f"Base Deck: {args.deck}")
    print(f"Registry A (Baseline): {args.regA}")
    print(f"Registry B (Challenger): {args.regB}")
    print(f"Total Matches: {args.matches}")
    print(f"Target Port: {args.port}")
    
    # Check if server is running
    if not check_port_open("127.0.0.1", args.port):
        print(f"\n⚠️ WARNING: No YGOPro server listening on port {args.port}!")
        print("Please launch EDOPro, click 'LAN Mode' or 'LAN + AI', and Host a room.")
        print("The tournament tester requires a running server to pair the bots.")
        sys.exit(1)
        
    # Resolve registry paths
    reg_a_path = args.regA if os.path.isabs(args.regA) else os.path.join(SCRIPT_DIR, args.regA)
    reg_b_path = args.regB if os.path.isabs(args.regB) else os.path.join(SCRIPT_DIR, args.regB)
    
    if not os.path.exists(reg_a_path) or not os.path.exists(reg_b_path):
        print("Error: One or both registry files do not exist.")
        sys.exit(1)
        
    # Define A/B deck names and configuration file paths
    deck_a_name = f"{args.deck}_VerA"
    deck_b_name = f"{args.deck}_VerB"
    
    # 1. Copy YDK files
    decks_dir = os.path.join(WINDBOT_DIR, "Decks")
    src_ydk = os.path.join(decks_dir, f"AI_{args.deck}.ydk")
    if not os.path.exists(src_ydk):
        src_ydk = os.path.join(decks_dir, f"{args.deck}.ydk")
    if not os.path.exists(src_ydk):
        print(f"Error: Base deck YDK file not found for {args.deck}")
        sys.exit(1)
        
    dst_ydk_a = os.path.join(decks_dir, f"AI_{deck_a_name}.ydk")
    dst_ydk_b = os.path.join(decks_dir, f"AI_{deck_b_name}.ydk")
    
    shutil.copy2(src_ydk, dst_ydk_a)
    shutil.copy2(src_ydk, dst_ydk_b)
    
    # Also copy Player copies just in case
    shutil.copy2(src_ydk, os.path.join(decks_dir, f"{deck_a_name}.ydk"))
    shutil.copy2(src_ydk, os.path.join(decks_dir, f"{deck_b_name}.ydk"))
    
    # 2. Copy Registry config files to LIVE config directory
    config_dir = os.path.join(WINDBOT_DIR, "config")
    live_reg_a = os.path.join(config_dir, f"cards_registry_{deck_a_name}.json")
    live_reg_b = os.path.join(config_dir, f"cards_registry_{deck_b_name}.json")
    
    shutil.copy2(reg_a_path, live_reg_a)
    shutil.copy2(reg_b_path, live_reg_b)
    
    # Copy Sandbox registry configs
    shutil.copy2(reg_a_path, os.path.join(SCRIPT_DIR, f"cards_registry_{deck_a_name}.json"))
    shutil.copy2(reg_b_path, os.path.join(SCRIPT_DIR, f"cards_registry_{deck_b_name}.json"))
    
    # 3. Copy Deck profile JSON configs
    src_deck_json = os.path.join(config_dir, "decks", f"{args.deck}.json")
    if os.path.exists(src_deck_json):
        shutil.copy2(src_deck_json, os.path.join(config_dir, "decks", f"{deck_a_name}.json"))
        shutil.copy2(src_deck_json, os.path.join(config_dir, "decks", f"{deck_b_name}.json"))
        
    print("\n✅ Set up environment for A/B tournament successfully.")
    
    # Compile C# executable to register the new dynamic executors
    # Wait, our UnifiedIgnisExecutor.cs automatically resolves any deck name by matching or dynamically!
    # Let's verify that A/B sub-classes aren't needed because C# has dynamic fallback if no subclass exists:
    # Actually, does UnifiedIgnisExecutor fallback to base if there is no subclass?
    # Let's check:
    # UnifiedIgnisExecutor has [Deck("UnifiedIgnis", "AI_CustomIgnis")].
    # But wait, if we run WindBot with deck=2026_AzaYummy_VerA, will it find a subclass?
    # No, but wait, WindBot's ExecutorManager loads the class annotated with [Deck("2026_AzaYummy_VerA")].
    # Ah! WindBot loads executors by matching the deck parameter to the [Deck] attribute!
    # If there is no subclass for "2026_AzaYummy_VerA", WindBot will fail to load the executor!
    # Oh! Let's check this. If it's true, we must add the subclass in C# or copy an existing one!
    # Yes! Let's look at bots.json or ExecutorManager.
    # In WindBot, if a deck has no executor class with that [Deck] attribute, it falls back to the default or fails.
    # Let's check bots.json to see how it works.
    
    # Let's clean up function
    def cleanup():
        print("\n🧹 Cleaning up tournament temporary files...")
        for p in [dst_ydk_a, dst_ydk_b, 
                  os.path.join(decks_dir, f"{deck_a_name}.ydk"),
                  os.path.join(decks_dir, f"{deck_b_name}.ydk"),
                  live_reg_a, live_reg_b,
                  os.path.join(SCRIPT_DIR, f"cards_registry_{deck_a_name}.json"),
                  os.path.join(SCRIPT_DIR, f"cards_registry_{deck_b_name}.json"),
                  os.path.join(config_dir, "decks", f"{deck_a_name}.json"),
                  os.path.join(config_dir, "decks", f"{deck_b_name}.json")]:
            if os.path.exists(p):
                try: os.remove(p)
                except: pass
                
    # Check if subclass is needed. Let's look at UnifiedIgnisExecutor.cs bottom classes:
    # We can append subclasses for 2026_AzaYummy_VerA and 2026_AzaYummy_VerB to C# file dynamically!
    # Wait, yes! We can add:
    # [Deck("2026_AzaYummy_VerA", "2026_AzaYummy_VerA")]
    # public class AzaYummyVerAExecutor : UnifiedIgnisExecutor { ... }
    # Let's modify C# to support VerA and VerB dynamically, or we can just append them!
    # Better yet, let's write a python function to inject them, compile, run, and then restore the C# file on cleanup!
    # That is extremely smart and 100% robust!
    
    cs_file_path = os.path.join(WINDBOT_DIR, "UnifiedIgnisExecutor.cs")
    with open(cs_file_path, "r", encoding="utf-8") as f:
        original_cs_content = f.read()
        
    try:
        # Inject the A/B subclasses at the end of the file before the namespace closing curly brace
        # Usually the last brace is the namespace closing brace.
        # Let's find the last '}' in the C# file
        last_brace_idx = original_cs_content.rfind('}')
        if last_brace_idx == -1:
            raise Exception("Invalid C# file format")
            
        injected_subclasses = f"""
[Deck("{deck_a_name}", "{deck_a_name}")]
public class {args.deck}VerAExecutor : UnifiedIgnisExecutor
{{
    public {args.deck}VerAExecutor(GameAI ai, Duel duel) : base(ai, duel) {{}}
}}

[Deck("{deck_b_name}", "{deck_b_name}")]
public class {args.deck}VerBExecutor : UnifiedIgnisExecutor
{{
    public {args.deck}VerBExecutor(GameAI ai, Duel duel) : base(ai, duel) {{}}
}}
"""
        new_cs_content = original_cs_content[:last_brace_idx] + injected_subclasses + "\n}"
        with open(cs_file_path, "w", encoding="utf-8") as f:
            f.write(new_cs_content)
            
        print("⚙️ Injected temporary C# subclasses. Compiling WindBot AI...")
        compile_bat = os.path.join(WINDBOT_DIR, "compile_ai.bat")
        res = subprocess.run([compile_bat], cwd=WINDBOT_DIR, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, shell=True)
        if res.returncode != 0:
            print("Error compiling C# AI after subclass injection:")
            print(res.stdout.decode('utf-8', errors='replace'))
            cleanup()
            # Restore CS file
            with open(cs_file_path, "w", encoding="utf-8") as frestore:
                frestore.write(original_cs_content)
            sys.exit(1)
        print("✅ Compilation successful!")
        
        # Start Tournament Loop
        wins_a = 0
        wins_b = 0
        ties = 0
        total_turns = 0
        lp_diffs = []
        
        windbot_exe = os.path.join(WINDBOT_DIR, "WindBot.exe")
        
        print(f"\n🚀 Starting tournament: {args.matches} games...")
        for i in range(1, args.matches + 1):
            before_dirs = set(os.listdir(LIVE_LOGS_DIR)) if os.path.exists(LIVE_LOGS_DIR) else set()
            
            # Start processes
            p_a = subprocess.Popen([windbot_exe, f"name=Ver_A", f"deck={deck_a_name}", f"port={args.port}", "hostinfo=", "version=720937"], cwd=WINDBOT_DIR, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            # Give a tiny headstart to player A
            time.sleep(0.5)
            p_b = subprocess.Popen([windbot_exe, f"name=Ver_B", f"deck={deck_b_name}", f"port={args.port}", "hostinfo=", "version=720937"], cwd=WINDBOT_DIR, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            
            # Wait for processes to exit
            start_time = time.time()
            match_finished = False
            while time.time() - start_time < args.timeout:
                if p_a.poll() is not None and p_b.poll() is not None:
                    match_finished = True
                    break
                time.sleep(0.5)
                
            # If timed out, terminate processes
            if not match_finished:
                print(f"Match {i} timed out. Terminating processes.")
                p_a.terminate()
                p_b.terminate()
                
            # Scan for the new log directory for deck A
            time.sleep(1.0) # wait for logs to write
            new_log_dir = get_new_match_log(before_dirs, deck_a_name)
            
            if new_log_dir:
                outcome, bot_lp, opp_lp, turns = parse_match_outcome(new_log_dir)
                total_turns += turns
                diff = bot_lp - opp_lp
                lp_diffs.append(diff)
                
                if outcome == "Win":
                    wins_a += 1
                    status_str = "Baseline (A) WON"
                elif outcome == "Loss":
                    wins_b += 1
                    status_str = "Challenger (B) WON"
                else:
                    ties += 1
                    status_str = "TIE/ABORTED"
                    
                print(f"  Match {i:3d}/{args.matches}: {status_str} | Turns: {turns:2d} | LP A: {bot_lp:5d} | LP B: {opp_lp:5d} (Diff: {diff:+6d})")
            else:
                ties += 1
                print(f"  Match {i:3d}/{args.matches}: TIE/ABORTED (No logs generated)")
                
        # Tournament Summary
        played = wins_a + wins_b
        win_rate_a = (wins_a / played * 100) if played > 0 else 0
        win_rate_b = (wins_b / played * 100) if played > 0 else 0
        avg_turns = total_turns / max(played + ties, 1)
        avg_lp_diff = sum(lp_diffs) / len(lp_diffs) if lp_diffs else 0
        
        print("\n" + "="*60)
        print("🏆 TOURNAMENT FINAL RESULTS")
        print("="*60)
        print(f"Baseline (A):   {wins_a} wins ({win_rate_a:.1f}% Win Rate)")
        print(f"Challenger (B): {wins_b} wins ({win_rate_b:.1f}% Win Rate)")
        print(f"Ties/Aborted:   {ties}")
        print(f"Average Turns:  {avg_turns:.2f}")
        print(f"Average LP Diff: {avg_lp_diff:+.1f} (favoring A if positive)")
        print("="*60)
        
    except KeyboardInterrupt:
        print("\nTournament interrupted by user.")
    finally:
        # Restore C# code
        with open(cs_file_path, "w", encoding="utf-8") as frestore:
            frestore.write(original_cs_content)
        # Recompile to restore WindBot executable state
        subprocess.run([compile_bat], cwd=WINDBOT_DIR, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, shell=True)
        cleanup()

if __name__ == "__main__":
    main()
