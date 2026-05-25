import os
import subprocess
import time
import sys

def main():
    project_root = r"c:\Users\admin\Documents\EDOTh"
    windbot_exe = os.path.join(project_root, "WindBot", "WindBot.exe")
    windbot_dir = os.path.join(project_root, "WindBot")
    port = 7912  # Use a different port to avoid conflict
    
    print("Launching Bot A (Host)...")
    p1 = subprocess.Popen(
        [windbot_exe, "name=IgnisBot_A", "deck=2026_Dreadnought", f"port={port}", "hostinfo=", "version=720937"],
        cwd=windbot_dir,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        encoding="utf-8",
        errors="ignore"
    )
    
    time.sleep(1.5)
    
    print("Launching Bot B (Client)...")
    p2 = subprocess.Popen(
        [windbot_exe, "name=IgnisBot_B", "deck=2026_Dreadnought", f"port={port}", "hostinfo=", "version=720937"],
        cwd=windbot_dir,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        encoding="utf-8",
        errors="ignore"
    )
    
    print("Waiting for match to run...")
    
    # Read stdout and stderr of both processes asynchronously
    # For debugging, we will just poll and read their output after they exit or timeout after 45 seconds
    try:
        p1.wait(timeout=45.0)
        p2.wait(timeout=45.0)
        print("Both bots finished execution.")
    except subprocess.TimeoutExpired:
        print("Timeout expired! Terminating processes...")
        p1.terminate()
        p2.terminate()
        
    out1, err1 = p1.communicate()
    out2, err2 = p2.communicate()
    
    print("\n=== BOT A STDOUT ===")
    print(out1[-2000:])
    print("\n=== BOT A STDERR ===")
    print(err1)
    
    print("\n=== BOT B STDOUT ===")
    print(out2[-2000:])
    print("\n=== BOT B STDERR ===")
    print(err2)
    
    print(f"Bot A exit code: {p1.returncode}")
    print(f"Bot B exit code: {p2.returncode}")

if __name__ == "__main__":
    main()
