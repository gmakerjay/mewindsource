import json

def list_cards():
    with open("untranslated_repo_cards.json", "r", encoding="utf-8") as f:
        data = json.load(f)
        
    out_lines = [f"Total cards: {len(data)}"]
    for idx, (card_id, details) in enumerate(data.items()):
        detail = details[0]
        out_lines.append(f"{idx+1}. ID: {card_id} | Name: {detail['name']} | CDB: {detail['cdb']}")
        
    with open("list_cards_output.txt", "w", encoding="utf-8") as out:
        out.write("\n".join(out_lines))
        
if __name__ == "__main__":
    list_cards()
