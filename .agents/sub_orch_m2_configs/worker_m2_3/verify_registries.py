import os
import json
import glob

def verify():
    config_dir = r"c:\Users\admin\Documents\EDOTh\WindBot\config"
    pattern = os.path.join(config_dir, "cards_registry_2026_*.json")
    files = glob.glob(pattern)
    
    all_ok = True
    for f in files:
        filename = os.path.basename(f)
        try:
            with open(f, "r", encoding="utf-8-sig") as fh:
                data = json.load(fh)
        except Exception as e:
            print(f"[{filename}] Error loading JSON: {e}")
            all_ok = False
            continue
            
        print(f"[{filename}] Checking {len(data)} cards...")
        for i, card in enumerate(data):
            card_id = card.get("id")
            roles = card.get("roles")
            priority = card.get("priority")
            
            # Check ID
            if card_id == 0 or card_id == "0":
                print(f"  [ERROR] Card at index {i} has ID {card_id}")
                all_ok = False
                
            # Check roles
            if not roles or len(roles) == 0:
                print(f"  [ERROR] Card {card_id} has empty/missing roles: {roles}")
                all_ok = False
                
            # Check priority limit
            if priority is not None and priority > 8:
                print(f"  [ERROR] Card {card_id} has priority {priority} (> 8)")
                all_ok = False
                
    if all_ok:
        print("All registries are VALID!")
    else:
        print("Validation FAILED!")

if __name__ == "__main__":
    verify()
