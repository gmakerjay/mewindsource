import os
import json

WINDBOT_DIR = r"c:\Users\admin\Documents\EDOTh\WindBot"
SANDBOX_DIR = r"c:\Users\admin\Documents\EDOTh\WindBot_Sandbox"
DECKS_DIR = os.path.join(WINDBOT_DIR, "Decks")
CONFIG_DIR = os.path.join(WINDBOT_DIR, "config")
OUTPUT_FILE = r"c:\Users\admin\Documents\EDOTh\.agents\sub_orch_m2_configs\explorer_m2_3\analysis_results.txt"

def load_ydk_cards(ydk_path):
    if not os.path.exists(ydk_path):
        return set()
    cards = set()
    with open(ydk_path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#") or line.startswith("!"):
                continue
            try:
                cards.add(int(line))
            except ValueError:
                pass
    return cards

def load_registry_ids(reg_path):
    if not os.path.exists(reg_path):
        return set()
    try:
        with open(reg_path, "r", encoding="utf-8") as f:
            data = json.load(f)
            return {item["id"] for item in data}
    except Exception as e:
        print(f"Error loading {reg_path}: {e}")
        return set()

def main():
    decks = ["2026_Goldlord", "2026_Invoke", "2026_Kwtune", "2026_Labrynth"]
    results = []
    
    for deck in decks:
        ydk_path = os.path.join(DECKS_DIR, f"{deck}.ydk")
        live_reg_path = os.path.join(CONFIG_DIR, f"cards_registry_{deck}.json")
        sandbox_reg_path = os.path.join(SANDBOX_DIR, f"cards_registry_{deck}.json")
        
        ydk_cards = load_ydk_cards(ydk_path)
        live_cards = load_registry_ids(live_reg_path)
        sandbox_cards = load_registry_ids(sandbox_reg_path)
        
        missing_live = ydk_cards - live_cards
        missing_sandbox = ydk_cards - sandbox_cards
        
        results.append(f"Deck: {deck}")
        results.append(f"  YDK unique cards count: {len(ydk_cards)}")
        results.append(f"  Live Registry cards count: {len(live_cards)}")
        results.append(f"  Sandbox Registry cards count: {len(sandbox_cards)}")
        results.append(f"  Missing from Live Registry: {sorted(list(missing_live))}")
        results.append(f"  Missing from Sandbox Registry: {sorted(list(missing_sandbox))}")
        results.append("")
        
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        f.write("\n".join(results))
    print("Done")

if __name__ == "__main__":
    main()
