import json

def compare_translations():
    with open("untranslated_repo_cards.json", "r", encoding="utf-8") as f:
        untranslated = json.load(f)
        
    from apply_translations import CUSTOM_TRANSLATIONS
    
    output = []
    for card_id, records in untranslated.items():
        if card_id not in CUSTOM_TRANSLATIONS:
            continue
        eng_desc = records[0]["desc"]
        thai_desc = CUSTOM_TRANSLATIONS[card_id]["desc"]
        cdb_name = records[0]["cdb"]
        
        output.append(f"ID: {card_id} | Name: {records[0]['name']} | CDB: {cdb_name}")
        output.append(f"ENG:\n{eng_desc}")
        output.append(f"THAI:\n{thai_desc}")
        output.append("=" * 60)
        output.append("")
        
    with open("compare_output.txt", "w", encoding="utf-8") as out:
        out.write("\n".join(output))

if __name__ == "__main__":
    compare_translations()
