import json

with open("untranslated_repo_cards.json", "r", encoding="utf-8") as f:
    cards = json.load(f)
    
print(f"Loaded {len(cards)} untranslated cards.")

with open("untranslated_list.txt", "w", encoding="utf-8") as f:
    for cid, occurrences in sorted(cards.items(), key=lambda x: int(x[0])):
        f.write(f"==================================================\n")
        f.write(f"ID: {cid}\n")
        for occ in occurrences:
            f.write(f"CDB: {occ['cdb']}\n")
            f.write(f"Name: {occ['name']}\n")
            f.write(f"Description:\n{occ['desc']}\n")
        f.write(f"==================================================\n\n")

print("Written untranslated_list.txt successfully.")
