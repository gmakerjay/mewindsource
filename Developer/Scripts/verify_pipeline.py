import os
import sys
import json
import sqlite3
import shutil
import subprocess

try:
    sys.stdout.reconfigure(encoding='utf-8')
except AttributeError:
    pass


def load_card_registry():
    path = r"c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sandbox\cards_registry_2026_EvilTwin.json"
    with open(path, "r", encoding="utf-8-sig") as f:
        data = json.load(f)
    return data

def find_card(registry, card_id):
    for card in registry:
        if card["id"] == card_id:
            return card
    return None

def main():
    print("=== STARTING AUTOMATED PIPELINE VERIFICATION ===")
    
    # 1. Print current Q-values of Bystial Druiswurm
    print("\n--- Reading current Bystial Druiswurm (6637331) state in Sandbox Registry ---")
    reg_before = load_card_registry()
    card_before = find_card(reg_before, 6637331)
    if card_before:
        print(f"Before: priority={card_before.get('priority')}, q_values={card_before.get('q_values')}")
    else:
        print("Error: Card 6637331 not found in registry!")
        sys.exit(1)

    # 2. Wipe database
    print("\n--- Step 1: Wiping the database statistics.db ---")
    save_sql_path = r"c:\Users\admin\Documents\EDOTh\Developer\scratch\save_outcomes_to_sql.py"
    subprocess.run([sys.executable, save_sql_path, "--wipe"], check=True)
    
    db_path = r"c:\Users\admin\Documents\EDOTh\Developer\scratch\statistics.db"
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    cursor.execute("SELECT COUNT(*) FROM matches")
    matches_count = cursor.fetchone()[0]
    cursor.execute("SELECT COUNT(*) FROM decisions")
    decisions_count = cursor.fetchone()[0]
    print(f"Verified: matches count = {matches_count}, decisions count = {decisions_count}")
    conn.close()

    # 3. Create mock log folder
    print("\n--- Step 2: Creating mock log folder ---")
    mock_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\Logs\2026_EvilTwin_MockWin_20260525_120000_12345678"
    os.makedirs(mock_dir, exist_ok=True)
    
    # match_summary.log
    summary_content = (
        "Applying Real-time Learning: Outcome is Win (Bot LP: 8000, Opp LP: 0, Turns: 4)\n"
        "Deck: 2026_EvilTwin\n"
        "Final Bot LP: 8000\n"
        "Final Opponent LP: 0\n"
    )
    with open(os.path.join(mock_dir, "match_summary.log"), "w", encoding="utf-8") as f:
        f.write(summary_content)
        
    # decisions.jsonl
    decision_line = (
        '{"turn":1,"card_id":6637331,"card_name":"Bystial Druiswurm","action":"Activate","goal":"break_board",'
        '"score":176.0,"decision":true,"plan":"PlanA","lp_self":8000,"lp_opp":8000,"opponent_threat":189.0,'
        '"bot_monsters":[],"opp_monsters":[{"id":59581480,"atk":2400,"def":1800,"pos":"FaceUpAttack","faceup":true,"danger":45.0}],'
        '"opp_spells":[],"bot_hand":[]}\n'
    )
    with open(os.path.join(mock_dir, "decisions.jsonl"), "w", encoding="utf-8") as f:
        f.write(decision_line)
        
    # turn_4.log to simulate turn count
    with open(os.path.join(mock_dir, "turn_4.log"), "w", encoding="utf-8") as f:
        f.write("")
        
    print(f"Created mock log folder at: {mock_dir}")

    # 4. Import mock log into DB
    print("\n--- Step 3: Running save_outcomes_to_sql.py to import mock log ---")
    subprocess.run([sys.executable, save_sql_path], check=True)

    # 5. Run learning sandbox and Q-learning trainer via run_match_learning.py
    print("\n--- Step 4: Running run_match_learning.py for deck 2026_EvilTwin ---")
    run_learning_path = r"c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sandbox\run_match_learning.py"
    subprocess.run([sys.executable, run_learning_path, "--deck", "2026_EvilTwin"], check=True)

    # 6. Check database contents
    print("\n--- Step 5: Querying database records ---")
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    print("\n--- Matches Table Records: ---")
    cursor.execute("SELECT * FROM matches")
    matches = cursor.fetchall()
    for row in matches:
        print(row)
        
    print("\n--- Decisions Table Records: ---")
    cursor.execute("SELECT * FROM decisions")
    decisions = cursor.fetchall()
    for row in decisions:
        print(row)
        
    conn.close()

    # 7. Print the updated Q-values
    print("\n--- Step 6: Reading updated Bystial Druiswurm state in Sandbox Registry ---")
    reg_after = load_card_registry()
    card_after = find_card(reg_after, 6637331)
    if card_after:
        print(f"After: priority={card_after.get('priority')}, q_values={card_after.get('q_values')}")
    else:
        print("Error: Card 6637331 not found in registry after training!")

    # 8. Clean up mock log folder
    print("\n--- Step 7: Cleaning up mock log folder ---")
    if os.path.exists(mock_dir):
        shutil.rmtree(mock_dir)
        print("Mock log folder successfully deleted.")
    
    print("\n=== PIPELINE VERIFICATION COMPLETE ===")

if __name__ == "__main__":
    main()
