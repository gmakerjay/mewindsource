import os
import json

sandbox_dir = r"c:\Users\admin\Documents\EDOTh\WindBot_Sandbox"
live_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\config"
decks = [
    "2026_AzaYummy", "2026_BrElfnote", "2026_DarkTime", "2026_EvilTwin",
    "2026_EyeInside", "2026_Goldlord", "2026_Hecahand", "2026_Invoke",
    "2026_Kwtune", "2026_Labrynth"
]

print("=== REGISTRY COMPARISON ===")
for deck in decks:
    sandbox_path = os.path.join(sandbox_dir, f"cards_registry_{deck}.json")
    live_path = os.path.join(live_dir, f"cards_registry_{deck}.json")
    
    if not os.path.exists(sandbox_path):
        print(f"{deck}: Sandbox registry missing!")
        continue
    if not os.path.exists(live_path):
        print(f"{deck}: Live registry missing!")
        continue
        
    with open(sandbox_path, "r", encoding="utf-8-sig") as f:
        sandbox_data = json.load(f)
    with open(live_path, "r", encoding="utf-8-sig") as f:
        live_data = json.load(f)
        
    sandbox_ids = {c["id"] for c in sandbox_data}
    live_ids = {c["id"] for c in live_data}
    
    sandbox_only = sandbox_ids - live_ids
    live_only = live_ids - sandbox_ids
    
    print(f"\nDeck: {deck}")
    print(f"  Sandbox card count: {len(sandbox_ids)}")
    print(f"  Live card count: {len(live_ids)}")
    if sandbox_only:
        print(f"  Only in Sandbox ({len(sandbox_only)} cards): {sorted(list(sandbox_only))}")
    if live_only:
        print(f"  Only in Live ({len(live_only)} cards): {sorted(list(live_only))}")
    if not sandbox_only and not live_only:
        print("  Identical IDs!")
