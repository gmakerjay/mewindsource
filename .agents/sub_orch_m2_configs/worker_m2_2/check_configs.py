import os
import json

decks_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\Decks"
configs_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\config\decks"

decks = [
    "AzaYummy", "BrElfnote", "DarkTime", "EvilTwin", "EyeInside",
    "Goldlord", "Hecahand", "Invoke", "Kwtune", "Labrynth"
]

for deck in decks:
    ydk_path = os.path.join(decks_dir, f"AI_2026_{deck}.ydk")
    if not os.path.exists(ydk_path):
        ydk_path = os.path.join(decks_dir, f"2026_{deck}.ydk")
        if not os.path.exists(ydk_path):
            print(f"[ERROR] YDK file for {deck} not found!")
            continue

    # Load YDK card IDs
    ydk_cards = set()
    with open(ydk_path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#") or line.startswith("!"):
                continue
            try:
                ydk_cards.add(int(line))
            except ValueError:
                pass

    json_path = os.path.join(configs_dir, f"2026_{deck}.json")
    if not os.path.exists(json_path):
        print(f"[ERROR] JSON file for {deck} not found!")
        continue

    with open(json_path, "r", encoding="utf-8") as f:
        config = json.load(f)

    playstyle = config.get("playstyle", "unknown")
    choke_points = config.get("choke_points", [])

    print(f"Deck: {deck}")
    print(f"  Playstyle: {playstyle}")
    print(f"  Choke points: {choke_points}")
    
    missing_chokes = [cp for cp in choke_points if cp not in ydk_cards]
    if missing_chokes:
        print(f"  [WARNING] Choke points not in YDK: {missing_chokes}")
    else:
        print(f"  [OK] All choke points present in YDK.")
