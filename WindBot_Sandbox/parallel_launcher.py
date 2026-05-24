import os
import sys
import json
import time
import subprocess
import threading
import argparse

# Resolve paths relative to this script
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(SCRIPT_DIR)
WINDBOT_DIR = os.path.join(PROJECT_ROOT, "WindBot")
WINDBOT_EXE = os.path.join(WINDBOT_DIR, "WindBot.exe")
ORIGINAL_SYSTEM_CONF = os.path.join(PROJECT_ROOT, "config", "system.conf")

def run_single_headless_match(deck, opponent, opp_deck, port, match_id, logs_dir):
    """Runs a single headless duel between two bots on a specific port"""
    print(f"[Match {match_id}] Starting on Port {port} ({deck} vs {opponent})...")
    log_file = os.path.join(logs_dir, f"match_{match_id}_port_{port}.log")
    
    try:
        # Launch Host Bot (IgnisBot or Player Bot)
        p1 = subprocess.Popen(
            [WINDBOT_EXE, f"name=IgnisBot", f"deck={deck}", f"port={port}", "hostinfo=", "version=720937"],
            cwd=WINDBOT_DIR,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT
        )
        time.sleep(1.0)
        
        # Launch Opponent Bot
        p2 = subprocess.Popen(
            [WINDBOT_EXE, f"name={opponent}", f"deck={opp_deck}", f"port={port}", "hostinfo=", "version=720937"],
            cwd=WINDBOT_DIR,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT
        )
        
        def log_stream(proc, bot_name):
            for line in iter(proc.stdout.readline, b''):
                decoded = line.decode('utf-8', errors='replace')
                with open(log_file, "a", encoding="utf-8") as f:
                    f.write(f"[{bot_name}] {decoded}")
        
        # Start logging threads
        t1 = threading.Thread(target=log_stream, args=(p1, "IgnisBot"), daemon=True)
        t2 = threading.Thread(target=log_stream, args=(p2, opponent), daemon=True)
        t1.start()
        t2.start()
        
        # Wait for both bots to finish
        while p1.poll() is None or p2.poll() is None:
            time.sleep(0.5)
            
        print(f"[Match {match_id}] Finished on Port {port}.")
    except Exception as e:
        print(f"[Match {match_id}] Error on Port {port}: {str(e)}")

def run_headless_parallel(deck, opponent, opp_deck, instances, start_port):
    """Spins up multiple headless matches in parallel"""
    print(f"=== STARTING HEADLESS PARALLEL MATCHES ({instances} pairs) ===")
    threads = []
    logs_dir = os.path.join(PROJECT_ROOT, "WindBot", "Logs", "ParallelMatches")
    os.makedirs(logs_dir, exist_ok=True)
    
    for i in range(1, instances + 1):
        port = start_port + i - 1
        t = threading.Thread(
            target=run_single_headless_match, 
            args=(deck, opponent, opp_deck, port, i, logs_dir),
            daemon=True
        )
        threads.append(t)
        t.start()
        # Brief pause between pairs to avoid socket bind race conditions
        time.sleep(1.5)
        
    # Wait for all matches to complete
    for t in threads:
        t.join()
        
    print(f"\n=== ALL PARALLEL MATCHES FINISHED ===")
    print(f"Match log output saved to: {logs_dir}")

def setup_gui_instance(instance_id, port):
    """Creates a sandbox environment for EDOPro GUI with isolated system.conf port"""
    instance_dir = os.path.join(PROJECT_ROOT, f"instance_{instance_id}")
    print(f"\nSetting up GUI Instance {instance_id} in {instance_dir} on Port {port}...")
    
    os.makedirs(instance_dir, exist_ok=True)
    os.makedirs(os.path.join(instance_dir, "config"), exist_ok=True)
    
    # 1. Write custom system.conf with isolated port
    if os.path.exists(ORIGINAL_SYSTEM_CONF):
        with open(ORIGINAL_SYSTEM_CONF, "r", encoding="utf-8") as f:
            lines = f.readlines()
        
        new_lines = []
        for line in lines:
            if line.startswith("serverport ="):
                new_lines.append(f"serverport = {port}\n")
            elif line.startswith("lastport ="):
                new_lines.append(f"lastport = {port}\n")
            else:
                new_lines.append(line)
                
        with open(os.path.join(instance_dir, "config", "system.conf"), "w", encoding="utf-8") as f:
            f.writelines(new_lines)
    else:
        print("Warning: original system.conf not found. Skipping config setup.")

    # List of directories/files to link to the instance folder
    entries = os.listdir(PROJECT_ROOT)
    for entry in entries:
        if entry.startswith("instance_") or entry in ["config", ".git", "WindBot", ".vscode", "__pycache__", "WindBot_Sandbox"]:
            continue
        
        src_path = os.path.join(PROJECT_ROOT, entry)
        dest_path = os.path.join(instance_dir, entry)
        
        # Clean up old links if they exist
        if os.path.exists(dest_path):
            try:
                if os.path.isdir(dest_path):
                    os.rmdir(dest_path)
                else:
                    os.remove(dest_path)
            except:
                pass
                
        # Link directories/files using Windows mklink commands
        if os.path.isdir(src_path):
            subprocess.run(f"mklink /j \"{dest_path}\" \"{src_path}\"", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        else:
            subprocess.run(f"mklink /h \"{dest_path}\" \"{src_path}\"", shell=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    print(f"Instance {instance_id} successfully created.")
    print(f"To launch: double-click {os.path.join(instance_dir, 'EDOPro.exe')}")

def main():
    parser = argparse.ArgumentParser(description="WindBot & EDOPro Parallel Duel Launcher")
    parser.add_argument("--mode", type=str, choices=["headless", "gui-setup"], default="headless",
                        help="headless (parallel background duels) or gui-setup (create EDOPro sandbox folders)")
    parser.add_argument("--deck", type=str, default="2026_AzaYummy", help="Deck name for IgnisBot")
    parser.add_argument("--opponent", type=str, default="AzaYummy_VerA", help="Opponent bot name")
    parser.add_argument("--opp_deck", type=str, default="2026_AzaYummy", help="Opponent deck name")
    parser.add_argument("--instances", type=int, default=3, help="Number of parallel instances to setup or run")
    parser.add_argument("--start-port", type=int, default=7911, help="Starting port number")
    
    args = parser.parse_args()
    
    if args.mode == "headless":
        run_headless_parallel(args.deck, args.opponent, args.opp_deck, args.instances, args.start_port)
    elif args.mode == "gui-setup":
        for i in range(1, args.instances + 1):
            port = args.start_port + i - 1
            setup_gui_instance(i, port)
        print(f"\nCreated {args.instances} isolated EDOPro instances.")
        print("You can launch their EDOPro.exe files and they will host LAN rooms on separate ports!")

if __name__ == "__main__":
    main()
