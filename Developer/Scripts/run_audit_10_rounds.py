import os
import sys
import time
import socket
import argparse
import subprocess

PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
WINDBOT_DIR = os.path.join(PROJECT_ROOT, "WindBot")
WINDBOT_EXE = os.path.join(WINDBOT_DIR, "WindBot.exe")

def is_server_listening(ip, port):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(0.5)
    try:
        s.connect((ip, port))
        s.close()
        return True
    except:
        return False

def main():
    parser = argparse.ArgumentParser(description="IgnisEngine Sequential 10-Round GUI LAN Auditor")
    parser.add_argument("--deck1", type=str, default="2026_PureYummy", help="Deck name for IgnisBot")
    parser.add_argument("--deck2", type=str, default="2026_AzaYummy", help="Deck name for Opponent (AzaYummy_VerA)")
    parser.add_argument("--port", type=int, default=7911, help="EDOPro LAN port")
    parser.add_argument("--rounds", type=int, default=10, help="Number of audit rounds")
    parser.add_argument("--delay", type=float, default=2.0, help="Delay between rounds (seconds)")
    
    args = parser.parse_args()
    
    print("="*60)
    print("  IGNIS AI 10-ROUND LAN AUDIT CONTROLLER")
    print("="*60)
    print(f"  IgnisBot Deck:  {args.deck1}")
    print(f"  Opponent Deck:  {args.deck2}")
    print(f"  Target Port:    {args.port}")
    print(f"  Total Rounds:   {args.rounds}")
    print("="*60)
    print("\n[INFO] Please open EDOPro, navigate to LAN Mode, select Port 7911, and host a room.")
    print("[INFO] This script will automatically detect the server and launch the bots sequentially.\n")
    
    for round_num in range(1, args.rounds + 1):
        print(f"\n[Round {round_num}/{args.rounds}] Checking for EDOPro LAN Server on port {args.port}...")
        
        # Wait for server to become active
        dots = 0
        while not is_server_listening("127.0.0.1", args.port):
            sys.stdout.write(f"\rWaiting for EDOPro server to start on port {args.port}" + "." * (dots % 4) + "    ")
            sys.stdout.flush()
            dots += 1
            time.sleep(1.0)
        sys.stdout.write(f"\rEDOPro Server detected on port {args.port}! Starting duel...\n")
        sys.stdout.flush()
        
        # Launch Host Bot (IgnisBot)
        print(f"[Round {round_num}] Launching player 1: IgnisBot ({args.deck1})...")
        p1 = subprocess.Popen(
            [WINDBOT_EXE, "name=IgnisBot", f"deck={args.deck1}", f"port={args.port}", "hostinfo=", "version=720937"],
            cwd=WINDBOT_DIR,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT
        )
        
        # Wait 1.5 seconds for player 1 to enter the room before launching opponent
        time.sleep(1.5)
        
        # Launch Opponent Bot (AzaYummy_VerA)
        print(f"[Round {round_num}] Launching player 2: AzaYummy_VerA ({args.deck2})...")
        p2 = subprocess.Popen(
            [WINDBOT_EXE, "name=AzaYummy_VerA", f"deck={args.deck2}", f"port={args.port}", "hostinfo=", "version=720937"],
            cwd=WINDBOT_DIR,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT
        )
        
        print(f"[Round {round_num}] Duel is in progress. Waiting for bots to finish match...")
        
        # Wait for both bots to exit
        while p1.poll() is None or p2.poll() is None:
            time.sleep(1.0)
            
        print(f"[Round {round_num}] Duel finished.")
        
        # Delay before starting next round to allow EDOPro server room cleanup
        if round_num < args.rounds:
            print(f"Waiting {args.delay} seconds before initializing next round...")
            time.sleep(args.delay)
            
    print("\n" + "="*60)
    print("  10-ROUND LAN AUDIT SUCCESSFULLY COMPLETED!")
    print("="*60)

if __name__ == "__main__":
    main()
