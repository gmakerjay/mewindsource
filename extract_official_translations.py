import sqlite3
import json

def cross_reference():
    with open("untranslated_repo_cards.json", "r", encoding="utf-8") as f:
        data = json.load(f)
        
    main_db = r"c:\Users\admin\Documents\EDOTh\cards.cdb"
    conn = sqlite3.connect(main_db)
    cur = conn.cursor()
    
    found_official = {}
    for card_id in data.keys():
        cur.execute("SELECT name, desc FROM texts WHERE id = ?", (card_id,))
        row = cur.fetchone()
        if row:
            name, desc = row
            # Check if it actually contains Thai (meaning it's translated)
            # Thai character range: \u0e00 to \u0e7f
            has_thai = any('\u0e00' <= char <= '\u0e7f' for char in name or '') or \
                       any('\u0e00' <= char <= '\u0e7f' for char in desc or '')
            if has_thai:
                found_official[card_id] = {
                    "name": name,
                    "desc": desc
                }
                
    print(f"Found {len(found_official)} official cards with existing Thai translations in cards.cdb.")
    with open("official_thai_matches.json", "w", encoding="utf-8") as out:
        json.dump(found_official, out, ensure_ascii=False, indent=2)
        
    conn.close()

if __name__ == "__main__":
    cross_reference()
