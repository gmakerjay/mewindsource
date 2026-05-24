import os
import sqlite3
import re

DECK_DIR = r"c:\Users\admin\Documents\EDOTh\deck"
REPO_DIR = r"c:\Users\admin\Documents\EDOTh\repositories\delta-bagooska"
ROOT_DIR = r"c:\Users\admin\Documents\EDOTh"
EXPANSIONS_DIR = r"c:\Users\admin\Documents\EDOTh\expansions"

def get_2026_card_ids():
    card_ids = set()
    for file in os.listdir(DECK_DIR):
        if file.startswith("2026_") and file.endswith(".ydk"):
            path = os.path.join(DECK_DIR, file)
            with open(path, "r", encoding="utf-8") as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith("#") and not line.startswith("!"):
                        try:
                            card_ids.add(int(line))
                        except ValueError:
                            pass
    return card_ids

def inspect_card_in_cdb(cdb_path, card_ids):
    if not os.path.exists(cdb_path) or os.path.getsize(cdb_path) == 0:
        return {}
    
    conn = sqlite3.connect(cdb_path)
    cursor = conn.cursor()
    
    # Check if table texts exists
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='texts'")
    if not cursor.fetchone():
        conn.close()
        return {}
        
    results = {}
    placeholders = ",".join("?" for _ in card_ids)
    try:
        cursor.execute(f"SELECT id, name, desc FROM texts WHERE id IN ({placeholders})", list(card_ids))
        for row in cursor.fetchall():
            results[row[0]] = {"name": row[1], "desc": row[2]}
    except Exception as e:
        print(f"Error reading {cdb_path}: {e}")
    conn.close()
    return results

def main():
    card_ids = get_2026_card_ids()
    print(f"Total unique card IDs in 2026 decks: {len(card_ids)}")
    
    cdb_files = []
    # Collect all CDB files
    for root, dirs, files in os.walk(ROOT_DIR):
        # Exclude git or vscode
        if ".git" in root or ".vscode" in root:
            continue
        for file in files:
            if file.endswith(".cdb"):
                cdb_files.append(os.path.join(root, file))
                
    print(f"Found CDB files: {[os.path.relpath(p, ROOT_DIR) for p in cdb_files]}")
    
    card_db_info = {}
    for cdb in cdb_files:
        info = inspect_card_in_cdb(cdb, card_ids)
        if info:
            card_db_info[cdb] = info
            print(f"CDB {os.path.relpath(cdb, ROOT_DIR)} contains {len(info)} cards of interest.")
            
    # Compile a master list of card info
    # We want to identify cards that:
    # 1. are custom cards (e.g. starting with 101402, 100455, 101305, etc. - or any cards not in official DBs)
    # 2. check if they have English or Thai descriptions
    
    # Let's inspect which cards are present in which CDBs
    all_found_cards = {}
    for cdb, cards in card_db_info.items():
        for cid, data in cards.items():
            if cid not in all_found_cards:
                all_found_cards[cid] = []
            all_found_cards[cid].append((cdb, data))
            
    print(f"\nUnique cards from 2026 decks found in CDBs: {len(all_found_cards)} / {len(card_ids)}")
    
    # Let's dump the info of these cards
    with open("deck_cards_info.json", "w", encoding="utf-8") as f:
        import json
        json.dump({str(k): v for k, v in all_found_cards.items()}, f, indent=2, ensure_ascii=False)
        
if __name__ == "__main__":
    main()
