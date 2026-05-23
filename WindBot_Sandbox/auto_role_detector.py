import os
import sqlite3
import json
import re
import argparse
import sys

from shared_utils import (
    configure_utf8, PROJECT_ROOT,
    get_sandbox_registry_path, load_ydk_main_deck, save_registry_list,
)

configure_utf8()

# YGOPro Card Type Constants
TYPE_MONSTER = 0x1
TYPE_SPELL = 0x2
TYPE_TRAP = 0x4
TYPE_FUSION = 0x40
TYPE_SYNCHRO = 0x2000
TYPE_XYZ = 0x800000
TYPE_LINK = 0x4000000

CDB_PATH = os.path.join(PROJECT_ROOT, "expansions", "cards.cdb")

def query_card_details(card_ids):
    if not os.path.exists(CDB_PATH):
        print(f"Error: card database cards.cdb not found at {CDB_PATH}")
        return {}
        
    conn = sqlite3.connect(CDB_PATH)
    cursor = conn.cursor()
    
    placeholders = ",".join("?" for _ in card_ids)
    query = f"""
    SELECT d.id, d.type, d.atk, d.def, t.name, t.desc 
    FROM datas d
    JOIN texts t ON d.id = t.id
    WHERE d.id IN ({placeholders})
    """
    
    results = {}
    try:
        cursor.execute(query, card_ids)
        for row in cursor.fetchall():
            results[row[0]] = {
                "id": row[0],
                "type": row[1],
                "atk": row[2],
                "def": row[3],
                "name": row[4],
                "desc": row[5]
            }
    except Exception as e:
        print(f"Database query error: {e}")
    finally:
        conn.close()
        
    return results

def detect_roles(card):
    roles = []
    desc = card["desc"].lower()
    ctype = card["type"]
    atk = card["atk"]
    
    is_monster = bool(ctype & TYPE_MONSTER)
    is_spell = bool(ctype & TYPE_SPELL)
    is_trap = bool(ctype & TYPE_TRAP)
    is_extra = bool(ctype & (TYPE_FUSION | TYPE_SYNCHRO | TYPE_XYZ | TYPE_LINK))
    
    # 1. Handtrap
    if is_monster and ("quick effect" in desc or "during either player's turn" in desc) and "from your hand" in desc:
        roles.append("handtrap")
        
    # 2. Starter
    if "add 1" in desc and "from your deck to your hand" in desc:
        roles.append("starter")
    elif "special summon" in desc and "from your deck" in desc:
        roles.append("starter")
    elif is_monster and not is_extra and "normal summoned" in desc:
        # Many starters trigger when normal/special summoned
        roles.append("starter")
        
    # 3. Extender
    if "special summon this card" in desc:
        roles.append("extender")
    elif "special summon 1" in desc and ("from your hand" in desc or "from your gy" in desc):
        roles.append("extender")
        
    # 4. Payoff / Boss
    if is_extra:
        roles.append("payoff")
    elif is_monster and atk >= 2500 and ("negate" in desc or "destroy" in desc or "banish" in desc):
        roles.append("payoff")
        
    # 5. Disruption / Interruption
    if ("negate" in desc or "destroy" in desc or "banish" in desc or "send to the gy" in desc) and ("quick effect" in desc or "during either player's turn" in desc or is_trap):
        roles.append("disruption")
        
    # 6. Recovery
    if "add 1" in desc and "from your gy to your hand" in desc:
        roles.append("recovery")
    elif "special summon" in desc and "from your gy" in desc and "extender" not in roles:
        roles.append("recovery")
    elif "draw" in desc and "card" in desc:
        roles.append("recovery")
        
    # 7. Floodgate
    if "neither player can" in desc or "cannot special summon" in desc or "effects are negated" in desc:
        roles.append("floodgate")
        
    # Clean duplicates & default combo_piece if it's monster
    roles = list(set(roles))
    if not roles and is_monster:
        roles.append("combo_piece")
        
    return roles

def main():
    parser = argparse.ArgumentParser(description="Auto Role Detection Engine")
    parser.add_argument("--deck", type=str, required=True, help="Deck name to detect roles for")
    parser.add_argument("--overwrite", action="store_true", help="Completely overwrite manual roles")
    args = parser.parse_args()
    
    print(f"=== AUTO ROLE DETECTION FOR DECK: {args.deck} ===")
    
    deck_cards = load_ydk_main_deck(args.deck, unique=True)
    if not deck_cards:
        print(f"Error: Deck '{args.deck}' not found or has no cards.")
        sys.exit(1)
        
    print(f"Loaded {len(deck_cards)} unique card IDs from YDK file.")
    
    card_details = query_card_details(deck_cards)
    print(f"Found details for {len(card_details)} cards in database expansions/cards.cdb.")
    
    reg_path = get_sandbox_registry_path(args.deck)
    if not os.path.exists(reg_path):
        import shutil
        from shared_utils import SCRIPT_DIR
        default_reg = os.path.join(SCRIPT_DIR, "cards_registry.json")
        if os.path.exists(default_reg):
            shutil.copy2(default_reg, reg_path)
            print(f"Initialized registry for {args.deck} from cards_registry.json")
            
    with open(reg_path, "r", encoding="utf-8-sig") as f:
        registry = json.load(f)
        
    reg_dict = {card["id"]: card for card in registry}
    
    detected_count = 0
    for card_id in deck_cards:
        if card_id not in card_details:
            if card_id not in reg_dict:
                reg_dict[card_id] = {
                    "id": card_id,
                    "roles": ["combo_piece"],
                    "priority": 5,
                    "risk_if_negated": 3,
                    "bait_value": 0,
                    "followup_value": 5,
                    "recovery_value": 5,
                    "combo_plans": ["PlanA"],
                    "q_values": {}
                }
                detected_count += 1
                print(f"  [NEW] [CDB Missing Card] ({card_id}) -> Roles: ['combo_piece'] (Default)")
            continue
            
        details = card_details[card_id]
        roles = detect_roles(details)
        
        if card_id not in reg_dict:
            reg_dict[card_id] = {
                "id": card_id,
                "roles": roles,
                "priority": 5,
                "risk_if_negated": 3,
                "bait_value": 0,
                "followup_value": 5,
                "recovery_value": 5,
                "combo_plans": ["PlanA"],
                "q_values": {}
            }
            detected_count += 1
            print(f"  [NEW] {details['name']} ({card_id}) -> Roles: {roles}")
        else:
            card = reg_dict[card_id]
            old_roles = card.get("roles", [])
            
            if args.overwrite:
                card["roles"] = roles
                if set(old_roles) != set(roles):
                    detected_count += 1
                    print(f"  [OVERWRITE] {details['name']} ({card_id}) -> Roles: {roles}")
            else:
                # Merge
                merged = list(set(old_roles + roles))
                card["roles"] = merged
                if set(old_roles) != set(merged):
                    detected_count += 1
                    print(f"  [MERGE] {details['name']} ({card_id}) -> Roles: {merged} (added: {list(set(merged) - set(old_roles))})")
                    
    # Save back
    save_registry_list(reg_dict, reg_path)
        
    print(f"\n✅ Completed Auto Role Detection. Updated {detected_count} cards in registry: {reg_path}")

if __name__ == "__main__":
    main()
