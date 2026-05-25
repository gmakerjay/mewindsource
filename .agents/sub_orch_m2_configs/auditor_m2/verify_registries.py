import os
import json

decks = [
    "2026_AzaYummy", "2026_BrElfnote", "2026_DarkTime", "2026_EvilTwin", "2026_EyeInside",
    "2026_Goldlord", "2026_Hecahand", "2026_Invoke", "2026_Kwtune", "2026_Labrynth"
]

config_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\config"

issues = []

for deck in decks:
    filename = f"cards_registry_{deck}.json"
    filepath = os.path.join(config_dir, filename)
    print(f"Checking {filename}...")
    if not os.path.exists(filepath):
        issues.append(f"File not found: {filepath}")
        continue
    
    try:
        with open(filepath, "r", encoding="utf-8") as f:
            data = json.load(f)
    except Exception as e:
        issues.append(f"JSON parsing error in {filename}: {e}")
        continue
        
    if not isinstance(data, list):
        issues.append(f"Registry is not a list in {filename}")
        continue
        
    if len(data) == 0:
        issues.append(f"Registry is empty in {filename}")
        continue
        
    print(f"  {filename} contains {len(data)} cards.")
    
    empty_roles_cards = []
    missing_fields_cards = []
    
    for card in data:
        card_id = card.get("id")
        if "roles" not in card:
            missing_fields_cards.append((card_id, "missing roles field"))
            continue
            
        roles = card["roles"]
        if not isinstance(roles, list) or len(roles) == 0:
            empty_roles_cards.append(card_id)
            
    if empty_roles_cards:
        issues.append(f"{filename} has cards with empty roles: {empty_roles_cards}")
    if missing_fields_cards:
        issues.append(f"{filename} has cards with missing roles field: {missing_fields_cards}")

if issues:
    print("\n--- ISSUES DETECTED ---")
    for issue in issues:
        print(issue)
else:
    print("\n--- ALL REGISTRIES ARE VALID AND FULLY POPULATED ---")
