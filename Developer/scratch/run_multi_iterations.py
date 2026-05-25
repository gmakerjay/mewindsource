import os
import sys
import time
import subprocess
import argparse
import shutil
import glob
import sqlite3

# Paths
PROJECT_ROOT = r"c:\Users\admin\Documents\EDOTh"
WINDBOT_DIR = os.path.join(PROJECT_ROOT, "WindBot")
LOGS_DIR = os.path.join(WINDBOT_DIR, "Logs")
PARALLEL_LOGS_DIR = os.path.join(LOGS_DIR, "ParallelMatches")
ARCHIVE_DIR = os.path.join(LOGS_DIR, "ArchivedMatches")
DB_PATH = os.path.join(PROJECT_ROOT, "Developer", "scratch", "statistics.db")

def run_round(deck, opponent, opp_deck, instances, start_port):
    """Launches one round of parallel matches using parallel_launcher.py"""
    launcher_path = os.path.join(PROJECT_ROOT, "Developer", "WindBot_Sandbox", "parallel_launcher.py")
    
    cmd = [
        sys.executable,
        launcher_path,
        "--mode", "headless",
        "--deck", deck,
        "--opponent", opponent,
        "--opp-deck", opp_deck,
        "--instances", str(instances),
        "--start-port", str(start_port)
    ]
    
    # Run the parallel matches and monitor progress live
    p = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    
    # Monitor using our own monitoring screen loop
    monitor_script = os.path.join(PROJECT_ROOT, "Developer", "scratch", "monitor_progress.py")
    
    while p.poll() is None:
        # Clear screen and show current status of ports
        subprocess.run("cls", shell=True)
        print("=== LIVE MATCH SIMULATION MONITOR ===")
        print(f"Status: Running {instances} pairs of {deck} vs {opponent} on Ports {start_port}-{start_port+instances-1}")
        print("-" * 90)
        
        # Execute monitor_progress.py directly to print the table
        try:
            res = subprocess.run([sys.executable, monitor_script], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, encoding="utf-8")
            # Skip the standard headers to avoid redundant titles
            lines = res.stdout.splitlines()
            for line in lines:
                if "No parallel" in line or "=====" in line or "Match  | Port" in line or "------" in line:
                    continue
                print(line)
        except Exception as e:
            print(f"Error displaying progress: {e}")
            
        print("-" * 90)
        print("Press Ctrl+C in terminal if you need to abort.")
        time.sleep(2.0)
        
    p.wait()
    
    # Show final status of this round
    subprocess.run("cls", shell=True)
    print("=== LIVE MATCH SIMULATION MONITOR ===")
    print(f"Status: Round Completed. Final outcomes for Ports {start_port}-{start_port+instances-1}:")
    print("-" * 90)
    res = subprocess.run([sys.executable, monitor_script], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, encoding="utf-8")
    for line in res.stdout.splitlines():
        if "No parallel" in line or "=====" in line or "Match  | Port" in line or "------" in line:
            continue
        print(line)
    print("-" * 90)

def archive_and_clean_logs(deck_name):
    """Moves session logs to ArchivedMatches to prevent Logs/ folder bloat"""
    os.makedirs(ARCHIVE_DIR, exist_ok=True)
    
    # Move matching logs
    prefix = f"{deck_name}_"
    count = 0
    for entry in os.listdir(LOGS_DIR):
        full_path = os.path.join(LOGS_DIR, entry)
        if os.path.isdir(full_path) and entry.startswith(prefix) and entry != "ArchivedMatches" and entry != "ParallelMatches":
            dest_path = os.path.join(ARCHIVE_DIR, entry)
            # Remove existing archive folder if there's a collision
            if os.path.exists(dest_path):
                shutil.rmtree(dest_path)
            shutil.move(full_path, dest_path)
            count += 1
            
    # Clean ParallelMatches folder logs
    if os.path.exists(PARALLEL_LOGS_DIR):
        for f in os.listdir(PARALLEL_LOGS_DIR):
            f_path = os.path.join(PARALLEL_LOGS_DIR, f)
            if os.path.isfile(f_path):
                os.remove(f_path)
                
    return count

def print_db_summary():
    """Prints a clean summary of all outcomes stored in statistics.db"""
    if not os.path.exists(DB_PATH):
        return
    try:
        conn = sqlite3.connect(DB_PATH, timeout=30.0)
        cursor = conn.cursor()
        
        cursor.execute("SELECT outcome, COUNT(*) FROM matches GROUP BY outcome")
        rows = cursor.fetchall()
        
        total = sum(r[1] for r in rows)
        if total == 0:
            return
            
        print("\n[Stats] CUMULATIVE SIMULATION STATISTICS (FROM DATABASE):")
        print("=" * 60)
        print(f"{'Outcome':<15} | {'Count':<10} | {'Percentage':<10}")
        print("-" * 60)
        for outcome, count in rows:
            pct = (count / total) * 100.0
            print(f"{outcome:<15} | {count:<10} | {pct:>8.1f}%")
        print("-" * 60)
        print(f"{'Total Games':<15} | {total:<10} | 100.0%")
        print("=" * 60)
        conn.close()
    except Exception as e:
        print(f"Error querying database summary: {e}")

def main():
    parser = argparse.ArgumentParser(description="Multi-Instance Headless Parallel Simulator Coordinator")
    parser.add_argument("--deck", type=str, default="2026_EvilTwin", help="Deck name for our bot")
    parser.add_argument("--opponent", type=str, default="ABC_DragonBuster", help="Opponent bot name")
    parser.add_argument("--opp-deck", type=str, default="ABC-Dragon Buster", help="Opponent deck name")
    parser.add_argument("--instances", type=int, default=10, help="Number of parallel matches (e.g. 10 or 20)")
    parser.add_argument("--rounds", type=int, default=1, help="Number of rounds to run sequentially")
    parser.add_argument("--start-port", type=int, default=7911, help="Starting port")
    
    args = parser.parse_args()
    
    # Cap instances at 20 to prevent CPU starvation
    instances = min(20, max(1, args.instances))
    
    print(f"=== WIND-BOT PARALLEL TRAINING ORCHESTRATOR ===")
    print(f"Main Bot Deck: {args.deck}")
    print(f"Opponent Bot:  {args.opponent} ({args.opp_deck})")
    print(f"Parallel Pairs: {instances} instances")
    print(f"Total Rounds:   {args.rounds} rounds (Total {instances * args.rounds} matches)")
    print(f"Ports:          {args.start_port} to {args.start_port + instances - 1}")
    print("=" * 60)
    time.sleep(2.0)
    
    opp_is_custom = args.opp_deck.startswith("2026_") and args.opp_deck != args.deck

    for r in range(1, args.rounds + 1):
        print(f"\n[Round] Running Round {r} of {args.rounds}...")
        
        # 1. Run the headless parallel duels
        run_round(args.deck, args.opponent, args.opp_deck, instances, args.start_port)
        
        # 2. Bulk import all match logs and decision stats to SQL
        print("\n[Save] Importing match results and decision details into SQLite Database...")
        sql_script = os.path.join(PROJECT_ROOT, "Developer", "scratch", "save_outcomes_to_sql.py")
        subprocess.run([sys.executable, sql_script, "--deck", args.deck, "--opp-deck", args.opp_deck])
        
        # 3. Trigger reinforcement learning (Heuristics Optimization + Q-learning)
        print("\n[Learn] Training reinforcement learning models based on new results...")
        learning_script = os.path.join(PROJECT_ROOT, "Developer", "WindBot_Sandbox", "run_match_learning.py")
        subprocess.run([sys.executable, learning_script, "--deck", args.deck])
        if opp_is_custom:
            print(f"\n[Learn] Training reinforcement learning models for opponent deck: {args.opp_deck}...")
            subprocess.run([sys.executable, learning_script, "--deck", args.opp_deck])
        
        # 4. Move session logs to ArchivedMatches and clear temporary logs to prevent folder bloat
        archived_count = archive_and_clean_logs(args.deck)
        if opp_is_custom:
            opp_archived_count = archive_and_clean_logs(args.opp_deck)
            archived_count += opp_archived_count
        print(f"[Clean] Cleaned up temporary logs. Archived {archived_count} session folders to WindBot/Logs/ArchivedMatches/")
        
        # 5. Print database summary
        print_db_summary()
        
    print("\nALL SIMULATION ROUNDS COMPLETED SUCCESSFULY!")

if __name__ == "__main__":
    main()
