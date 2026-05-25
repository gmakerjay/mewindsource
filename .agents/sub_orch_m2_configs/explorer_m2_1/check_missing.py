import os
import json
import re

agent_dir = r"c:\Users\admin\Documents\EDOTh\.agents\sub_orch_m2_configs\explorer_m2_1"
project_root = r"c:\Users\admin\Documents\EDOTh"

decks = ["2026_Goldlord", "2026_Invoke", "2026_Kwtune", "2026_Labrynth"]

def get_ydk_cards(deck_name):
    ydk_path1 = os.path.join(project_root, "WindBot", "Decks", f"AI_{deck_name}.ydk")
    ydk_path2 = os.path.join(project_root, "WindBot", "Decks", f"{deck_name}.ydk")
    
    paths = []
    if os.path.exists(ydk_path1):
        paths.append(ydk_path1)
    if os.path.exists(ydk_path2):
        paths.append(ydk_path2)
        
    card_ids = set()
    for path in paths:
        with open(path, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#") or line.startswith("!"):
                    continue
                try:
                    card_ids.add(int(line))
                except ValueError:
                    pass
    return card_ids

def load_registry(deck_name, folder="WindBot/config"):
    filename = f"cards_registry.json" if deck_name == "all" else f"cards_registry_{deck_name}.json"
    path = os.path.join(project_root, folder, filename)
    if not os.path.exists(path):
        return set()
    with open(path, "r", encoding="utf-8-sig") as f:
        data = json.load(f)
    return {card["id"] for card in data}

print("=== STARTING CARD ID COMPARISON ===")
for deck in decks:
    ydk_ids = get_ydk_cards(deck)
    live_reg_ids = load_registry(deck, "WindBot/config")
    sandbox_reg_ids = load_registry(deck, "WindBot_Sandbox")
    default_reg_ids = load_registry("all", "WindBot/config")
    
    missing_live = ydk_ids - live_reg_ids
    missing_sandbox = ydk_ids - sandbox_reg_ids
    missing_default = ydk_ids - default_reg_ids
    
    print(f"\nDeck: {deck}")
    print(f"  Unique YDK cards: {len(ydk_ids)}")
    print(f"  Live Registry cards: {len(live_reg_ids)}")
    print(f"  Sandbox Registry cards: {len(sandbox_reg_ids)}")
    print(f"  Missing in Live Registry: {missing_live}")
    print(f"  Missing in Sandbox Registry: {missing_sandbox}")
    print(f"  Missing in Default Registry: {missing_default}")
