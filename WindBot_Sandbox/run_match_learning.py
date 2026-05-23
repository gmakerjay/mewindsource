import os
import sys
import subprocess
import argparse

try:
    sys.stdout.reconfigure(encoding='utf-8')
except AttributeError:
    pass

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

def main():
    parser = argparse.ArgumentParser(description="Unified Ignis Reinforcement Match Learning")
    parser.add_argument("--deck", type=str, required=True, help="Deck name to run learning for")
    args = parser.parse_args()

    print("=== STARTING UNIFIED REINFORCEMENT MATCH LEARNING LOOP ===")
    
    # 1. Run learning_sandbox.py (Supervised Heuristic Updates)
    print("\n[Step 1/2] Running Supervised Heuristics Adjuster...")
    sandbox_script = os.path.join(SCRIPT_DIR, "learning_sandbox.py")
    res1 = subprocess.run([sys.executable, sandbox_script], stdout=sys.stdout, stderr=sys.stderr)
    
    if res1.returncode != 0:
        print("Warning: Heuristics Adjuster returned non-zero exit code.")

    # 2. Run q_learning.py (Q-value updates)
    print("\n[Step 2/2] Running Q-Learning Reinforcement Trainer...")
    q_script = os.path.join(SCRIPT_DIR, "q_learning.py")
    res2 = subprocess.run([sys.executable, q_script, "--deck", args.deck], stdout=sys.stdout, stderr=sys.stderr)
    
    if res2.returncode != 0:
        print("Error: Q-Learning Trainer failed.")
        sys.exit(res2.returncode)

    print("\n=== UNIFIED REINFORCEMENT MATCH LEARNING COMPLETE ===")

if __name__ == "__main__":
    main()
