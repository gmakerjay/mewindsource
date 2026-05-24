import os
import sqlite3
import json

DECK_DIR = r"c:\Users\admin\Documents\EDOTh\deck"
ROOT_DIR = r"c:\Users\admin\Documents\EDOTh"

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

def has_thai(text):
    if not text:
        return False
    # Thai range is U+0E00 to U+0E7F
    return any(ord(char) >= 0x0e00 and ord(char) <= 0x0e7f for char in text)

def inspect_untranslated_cards(cdb_path, card_ids):
    if not os.path.exists(cdb_path) or os.path.getsize(cdb_path) == 0:
        return {}
    
    conn = sqlite3.connect(cdb_path)
    cursor = conn.cursor()
    
    cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='texts'")
    if not cursor.fetchone():
        conn.close()
        return {}
        
    untranslated = {}
    placeholders = ",".join("?" for _ in card_ids)
    try:
        cursor.execute(f"SELECT id, name, desc FROM texts WHERE id IN ({placeholders})", list(card_ids))
        for row in cursor.fetchall():
            cid, name, desc = row
            # If name or desc doesn't have Thai, it is considered untranslated
            if not has_thai(name) or not has_thai(desc):
                untranslated[cid] = {"name": name, "desc": desc}
    except Exception as e:
        print(f"Error reading {cdb_path}: {e}")
    conn.close()
    return untranslated

def main():
    card_ids = get_2026_card_ids()
    print(f"Total unique card IDs in 2026 decks: {len(card_ids)}")
    
    cdb_files = []
    for root, dirs, files in os.walk(ROOT_DIR):
        if ".git" in root or ".vscode" in root:
            continue
        for file in files:
            if file.endswith(".cdb"):
                cdb_files.append(os.path.join(root, file))
                
    untranslated_by_cdb = {}
    for cdb in cdb_files:
        info = inspect_untranslated_cards(cdb, card_ids)
        if info:
            untranslated_by_cdb[cdb] = info
            print(f"CDB {os.path.relpath(cdb, ROOT_DIR)} has {len(info)} untranslated cards.")
            
    # Print the master list of untranslated cards
    master_untranslated = {}
    for cdb, cards in untranslated_by_cdb.items():
        # Exclude root duplicate copies (like WindBot/cards.cdb or extensions/cards.cdb)
        # We only want to classify unique IDs and their details
        for cid, data in cards.items():
            if cid not in master_untranslated:
                master_untranslated[cid] = []
            master_untranslated[cid].append((cdb, data))
            
    print(f"\nUnique untranslated cards: {len(master_untranslated)}")
    for cid, items in sorted(master_untranslated.items()):
        # Print the first details found
        cdb_name = os.path.relpath(items[0][0], ROOT_DIR)
        name = items[0][1]['name']
        print(f"ID: {cid} | Name: {name} | Found in: {cdb_name}")

    with open("untranslated_cards.json", "w", encoding="utf-8") as f:
        # Save a simplified representation
        simple = {str(cid): {"name": items[0][1]['name'], "desc": items[0][1]['desc'], "cdbs": [os.path.relpath(i[0], ROOT_DIR) for i in items]} for cid, items in master_untranslated.items()}
        json.dump(simple, f, indent=2, ensure_ascii=False)

if __name__ == "__main__":
    main()
