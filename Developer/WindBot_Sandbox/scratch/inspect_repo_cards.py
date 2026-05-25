import os
import sqlite3
import json

DECK_DIR = r"c:\Users\admin\Documents\EDOTh\deck"
REPO_DIR = r"c:\Users\admin\Documents\EDOTh\repositories\delta-bagooska"

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
    return any(ord(char) >= 0x0e00 and ord(char) <= 0x0e7f for char in text)

def main():
    card_ids = get_2026_card_ids()
    print(f"Total unique card IDs in 2026 decks: {len(card_ids)}")
    
    repo_cdbs = [
        "cards.delta.cdb",
        "cards-unofficial.delta.cdb",
        "prerelease-betb.cdb",
        "prerelease-cori.cdb",
        "release-blzd.cdb",
        "prerelease-lpg2.cdb"
    ]
    
    custom_cards_info = {}
    
    for cdb_name in repo_cdbs:
        path = os.path.join(REPO_DIR, cdb_name)
        if not os.path.exists(path):
            continue
        
        conn = sqlite3.connect(path)
        cursor = conn.cursor()
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name='texts'")
        if not cursor.fetchone():
            conn.close()
            continue
            
        cursor.execute("SELECT id, name, desc FROM texts")
        for row in cursor.fetchall():
            cid, name, desc = row
            # We are interested if it's in the 2026 decks
            if cid in card_ids:
                if cid not in custom_cards_info:
                    custom_cards_info[cid] = []
                custom_cards_info[cid].append({
                    "cdb": cdb_name,
                    "name": name,
                    "desc": desc,
                    "has_thai": has_thai(name) or has_thai(desc)
                })
        conn.close()
        
    print(f"Found {len(custom_cards_info)} custom/prerelease cards from 2026 decks in repo databases.")
    
    # Untranslated custom cards (no Thai name AND no Thai description in any of the repo occurrences)
    untranslated = {}
    translated = {}
    
    for cid, occurrences in custom_cards_info.items():
        # Check if any occurrence has Thai translation
        has_t = any(occ["has_thai"] for occ in occurrences)
        if not has_t:
            untranslated[cid] = occurrences
        else:
            translated[cid] = occurrences
            
    print(f"Untranslated: {len(untranslated)}")
    print(f"Translated: {len(translated)}")
    
    # Save the untranslated cards to inspect
    with open("untranslated_repo_cards.json", "w", encoding="utf-8") as f:
        json.dump(untranslated, f, indent=2, ensure_ascii=False)
        
    # Also save the translated ones for comparison
    with open("translated_repo_cards.json", "w", encoding="utf-8") as f:
        json.dump(translated, f, indent=2, ensure_ascii=False)

if __name__ == "__main__":
    main()
