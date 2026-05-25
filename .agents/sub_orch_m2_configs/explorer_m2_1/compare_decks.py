import os
import json

working_dir = r"c:\Users\admin\Documents\EDOTh"
windbot_decks = os.path.join(working_dir, "WindBot", "Decks")
windbot_config = os.path.join(working_dir, "WindBot", "config")
sandbox_dir = os.path.join(working_dir, "WindBot_Sandbox")

decks = ["Goldlord", "Invoke", "Kwtune", "Labrynth"]

def parse_ydk(path):
    if not os.path.exists(path):
        return set()
    cards = set()
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#") or line.startswith("!"):
                continue
            try:
                cards.add(int(line))
            except ValueError:
                pass
    return cards

def load_registry(path):
    if not os.path.exists(path):
        return set()
    with open(path, "r", encoding="utf-8-sig") as f:
        try:
            data = json.load(f)
            return {card["id"] for card in data}
        except Exception as e:
            print(f"Error loading {path}: {e}")
            return set()

print("=== DECK COMPARISONS ===")
for deck in decks:
    ydk_name1 = f"2026_{deck}.ydk"
    ydk_name2 = f"AI_2026_{deck}.ydk"
    ydk_path1 = os.path.join(windbot_decks, ydk_name1)
    ydk_path2 = os.path.join(windbot_decks, ydk_name2)
    
    cards1 = parse_ydk(ydk_path1)
    cards2 = parse_ydk(ydk_path2)
    union_cards = cards1.union(cards2)
    
    reg_path = os.path.join(windbot_config, f"cards_registry_2026_{deck}.json")
    reg_cards = load_registry(reg_path)
    
    sandbox_path = os.path.join(sandbox_dir, f"cards_registry_2026_{deck}.json")
    sandbox_cards = load_registry(sandbox_path)
    
    print(f"\nDeck: {deck}")
    print(f"  {ydk_name1} count: {len(cards1)}")
    print(f"  {ydk_name2} count: {len(cards2)}")
    print(f"  Union of YDK files count: {len(union_cards)}")
    print(f"  Live Registry count: {len(reg_cards)}")
    print(f"  Sandbox Registry count: {len(sandbox_cards)}")
    
    missing_in_live = union_cards - reg_cards
    missing_in_sandbox = union_cards - sandbox_cards
    
    print(f"  Missing in Live Registry: {len(missing_in_live)}")
    if missing_in_live:
        print(f"    IDs: {sorted(list(missing_in_live))}")
        
    print(f"  Missing in Sandbox Registry: {len(missing_in_sandbox)}")
    if missing_in_sandbox:
        print(f"    IDs: {sorted(list(missing_in_sandbox))}")
