import os
import json

WINDBOT_DIR = r"c:\Users\admin\Documents\EDOTh\WindBot"
SANDBOX_DIR = r"c:\Users\admin\Documents\EDOTh\WindBot_Sandbox"

bricked_decks = ["2026_Goldlord", "2026_Invoke", "2026_Kwtune", "2026_Labrynth"]
all_10_decks = ["AzaYummy", "BrElfnote", "DarkTime", "EvilTwin", "EyeInside", "Goldlord", "Hecahand", "Invoke", "Kwtune", "Labrynth"]

def parse_ydk_all_ids(ydk_path):
    if not os.path.exists(ydk_path):
        return set()
    ids = set()
    with open(ydk_path, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith('#') or line.startswith('!'):
                continue
            try:
                ids.add(int(line))
            except ValueError:
                pass
    return ids

def load_registry_ids(registry_path):
    if not os.path.exists(registry_path):
        return set()
    with open(registry_path, 'r', encoding='utf-8-sig') as f:
        try:
            data = json.load(f)
            return {card["id"] for card in data}
        except Exception as e:
            print(f"Error loading {registry_path}: {e}")
            return set()

print("--- COMPARING YDK FILES AGAINST LIVE REGISTRIES (WindBot/config/) ---")
for deck in bricked_decks:
    ydk_path = os.path.join(WINDBOT_DIR, "Decks", f"{deck}.ydk")
    ai_ydk_path = os.path.join(WINDBOT_DIR, "Decks", f"AI_{deck}.ydk")
    
    ydk_ids = parse_ydk_all_ids(ydk_path)
    ai_ydk_ids = parse_ydk_all_ids(ai_ydk_path)
    
    combined_ydk_ids = ydk_ids.union(ai_ydk_ids)
    
    live_registry_path = os.path.join(WINDBOT_DIR, "config", f"cards_registry_{deck}.json")
    live_ids = load_registry_ids(live_registry_path)
    
    missing_in_live = combined_ydk_ids - live_ids
    
    print(f"\nDeck: {deck}")
    print(f"  YDK files found: {os.path.exists(ydk_path)} (regular), {os.path.exists(ai_ydk_path)} (AI)")
    print(f"  Total unique IDs in YDK: {len(combined_ydk_ids)}")
    print(f"  Total IDs in live registry: {len(live_ids)}")
    print(f"  Missing in live registry ({len(missing_in_live)}): {sorted(list(missing_in_live))}")

print("\n--- COMPARING LIVE REGISTRIES AGAINST SANDBOX REGISTRIES ---")
for deck in bricked_decks:
    live_registry_path = os.path.join(WINDBOT_DIR, "config", f"cards_registry_{deck}.json")
    sandbox_registry_path = os.path.join(SANDBOX_DIR, f"cards_registry_{deck}.json")
    
    live_ids = load_registry_ids(live_registry_path)
    sandbox_ids = load_registry_ids(sandbox_registry_path)
    
    ydk_path = os.path.join(WINDBOT_DIR, "Decks", f"{deck}.ydk")
    ai_ydk_path = os.path.join(WINDBOT_DIR, "Decks", f"AI_{deck}.ydk")
    combined_ydk_ids = parse_ydk_all_ids(ydk_path).union(parse_ydk_all_ids(ai_ydk_path))
    
    missing_in_sandbox = combined_ydk_ids - sandbox_ids
    
    print(f"\nDeck: {deck}")
    print(f"  Total IDs in sandbox registry: {len(sandbox_ids)}")
    print(f"  Missing in sandbox registry ({len(missing_in_sandbox)}): {sorted(list(missing_in_sandbox))}")
    print(f"  Is sandbox registry complete? {combined_ydk_ids.issubset(sandbox_ids)}")
