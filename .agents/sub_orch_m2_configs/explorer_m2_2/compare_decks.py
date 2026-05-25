import os
import json
import re

decks = ["2026_Goldlord", "2026_Invoke", "2026_Kwtune", "2026_Labrynth"]
decks_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\Decks"
config_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\config"

def get_all_ydk_cards(ydk_path):
    if not os.path.exists(ydk_path):
        return set()
    cards = set()
    with open(ydk_path, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith('#') or line.startswith('!'):
                continue
            try:
                cards.add(int(line))
            except ValueError:
                pass
    return cards

def get_registry_cards(registry_path):
    if not os.path.exists(registry_path):
        return set()
    with open(registry_path, 'r', encoding='utf-8-sig') as f:
        data = json.load(f)
    return {card["id"] for card in data}

for deck in decks:
    ydk_path1 = os.path.join(decks_dir, f"{deck}.ydk")
    ydk_path2 = os.path.join(decks_dir, f"AI_{deck}.ydk")
    
    cards1 = get_all_ydk_cards(ydk_path1)
    cards2 = get_all_ydk_cards(ydk_path2)
    
    registry_path = os.path.join(config_dir, f"cards_registry_{deck}.json")
    registry_cards = get_registry_cards(registry_path)
    
    print(f"=== Deck: {deck} ===")
    print(f"  {deck}.ydk cards count: {len(cards1)}")
    print(f"  AI_{deck}.ydk cards count: {len(cards2)}")
    print(f"  Registry cards count: {len(registry_cards)}")
    
    # Check if cards1 and cards2 differ
    diff_ydk = cards1.symmetric_difference(cards2)
    if diff_ydk:
        print(f"  Differences between {deck}.ydk and AI_{deck}.ydk: {diff_ydk}")
    else:
        print(f"  {deck}.ydk and AI_{deck}.ydk are identical in card IDs.")
        
    union_ydk = cards1.union(cards2)
    missing = union_ydk - registry_cards
    extra_in_reg = registry_cards - union_ydk
    print(f"  Missing from Registry: {missing}")
    print(f"  Extra in Registry (not in YDK): {extra_in_reg}")
    print()
