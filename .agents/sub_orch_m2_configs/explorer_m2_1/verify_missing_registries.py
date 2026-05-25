import os
import json

DECKS_DIR = os.path.join("..", "..", "..", "WindBot", "Decks")
CONFIG_DIR = os.path.join("..", "..", "..", "WindBot", "config")

DECKS = [
    "AzaYummy", "BrElfnote", "DarkTime", "EvilTwin", "EyeInside",
    "Goldlord", "Hecahand", "Invoke", "Kwtune", "Labrynth"
]

def load_ydk_cards(deck_name):
    path = os.path.join(DECKS_DIR, f"AI_2026_{deck_name}.ydk")
    if not os.path.exists(path):
        path = os.path.join(DECKS_DIR, f"2026_{deck_name}.ydk")
    if not os.path.exists(path):
        return []
    
    cards = []
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#") or line.startswith("!"):
                continue
            try:
                cards.append(int(line))
            except ValueError:
                pass
    return list(set(cards))

def load_registry_ids(deck_name):
    path = os.path.join(CONFIG_DIR, f"cards_registry_2026_{deck_name}.json")
    if not os.path.exists(path):
        return []
    with open(path, "r", encoding="utf-8") as f:
        try:
            data = json.load(f)
            return [card["id"] for card in data]
        except Exception:
            return []

def main():
    print("=== Target Decks Registry Card Verification ===")
    for deck in DECKS:
        ydk_cards = load_ydk_cards(deck)
        registry_cards = load_registry_ids(deck)
        
        missing = [cid for cid in ydk_cards if cid not in registry_cards]
        if missing:
            print(f"Deck: {deck} - Missing {len(missing)} cards in registry:")
            for cid in missing:
                print(f"  - {cid}")
        else:
            print(f"Deck: {deck} - All {len(ydk_cards)} cards present in registry.")

if __name__ == "__main__":
    main()
