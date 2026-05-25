import os
import sqlite3
import json
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
    import os
    import sqlite3
    
    db_paths = [
        os.path.join(PROJECT_ROOT, "expansions", "cards.cdb"),
        os.path.join(PROJECT_ROOT, "cards.cdb"),
        os.path.join(PROJECT_ROOT, "WindBot", "cards.cdb"),
        os.path.join(PROJECT_ROOT, "WindBot", "cards.delta.cdb"),
        os.path.join(PROJECT_ROOT, "repositories", "delta-bagooska", "cards.delta.cdb"),
        os.path.join(PROJECT_ROOT, "repositories", "delta-bagooska", "cards-unofficial.delta.cdb"),
    ]
    
    results = {}
    placeholders = ",".join("?" for _ in card_ids)
    query = f"""
    SELECT d.id, d.type, d.atk, d.def, t.name, t.desc 
    FROM datas d
    JOIN texts t ON d.id = t.id
    WHERE d.id IN ({placeholders})
    """
    
    for db_path in db_paths:
        if not os.path.exists(db_path):
            continue
        try:
            conn = sqlite3.connect(db_path)
            cursor = conn.cursor()
            cursor.execute(query, card_ids)
            for row in cursor.fetchall():
                card_id = row[0]
                # If already found, don't overwrite unless we have a richer/newer description or it's a delta db
                if card_id not in results or (row[5] and len(row[5]) > len(results[card_id]["desc"])):
                    results[card_id] = {
                        "id": card_id,
                        "type": row[1],
                        "atk": row[2],
                        "def": row[3],
                        "name": row[4],
                        "desc": row[5]
                    }
            conn.close()
        except Exception as e:
            pass
            
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
    
    # Check for Quick Effect (English or Thai)
    is_quick_effect = ("quick effect" in desc 
                       or "during either player's turn" in desc 
                       or "เอฟเฟกต์ความเร็วสูง" in desc 
                       or "เอฟเฟกต์ฉับพลัน" in desc 
                       or "ช่วงเทิร์นของฝ่ายตรงข้าม" in desc 
                       or "เทิร์นของใครก็ตาม" in desc
                       or "เทิร์นของฝ่ายตรงข้าม" in desc
                       or "ในเทิร์นของฝ่ายตรงข้าม" in desc)
                       
    # Check for from hand (English or Thai)
    from_hand = ("from your hand" in desc or "จากมือ" in desc or "จากบนมือ" in desc or "จากมือของคุณ" in desc)
    
    # Check for special summon (English or Thai)
    spec_summon = ("special summon" in desc or "อัญเชิญแบบพิเศษ" in desc or "อัญเชิญพิเศษ" in desc)
    
    # Check for from deck (English or Thai)
    from_deck = ("from your deck" in desc or "จากเด็ค" in desc or "จากในเด็ค" in desc)
    
    # Check for normal summoned (English or Thai)
    norm_summoned = ("normal summoned" in desc or "อัญเชิญแบบปกติ" in desc or "อัญเชิญปกติ" in desc)
    
    # Check for add to hand (English or Thai)
    add_to_hand = (("add 1" in desc or "นำการ์ด 1 ใบ" in desc or "นำการ์ด" in desc or "หยิบ" in desc) 
                   and ("to your hand" in desc or "ขึ้นมือ" in desc or "กลับขึ้นมือ" in desc or "สู่มือ" in desc))
                   
    # Check for negate/destroy/banish/send to gy (English or Thai)
    negate = ("negate" in desc or "ทำให้ผลเป็นโมฆะ" in desc or "ทำให้เป็นโมฆะ" in desc or "ยกเลิก" in desc or "ขัดขวาง" in desc or "ยกเลิกเอฟเฟกต์" in desc)
    destroy = ("destroy" in desc or "ทำลาย" in desc)
    banish = ("banish" in desc or "นำออกนอกเกม" in desc or "นำออกจากเกม" in desc or "รีมูฟ" in desc)
    send_to_gy = ("send to the gy" in desc or "ส่งลงสุสาน" in desc or "ส่งไปยังสุสาน" in desc or "ส่งสุสาน" in desc)
    
    # 1. Handtrap
    if (is_monster and is_quick_effect and from_hand) or (is_trap and from_hand):
        roles.append("handtrap")
        
    # 2. Starter
    if add_to_hand and from_deck:
        roles.append("starter")
    elif spec_summon and from_deck:
        roles.append("starter")
    elif is_monster and not is_extra and norm_summoned:
        roles.append("starter")
        
    # 3. Extender
    if ("special summon this card" in desc 
            or "อัญเชิญแบบพิเศษการ์ดใบนี้" in desc 
            or "อัญเชิญพิเศษการ์ดใบนี้" in desc 
            or "อัญเชิญแบบพิเศษตัวเอง" in desc
            or "อัญเชิญแบบพิเศษการ์ดนี้" in desc
            or "อัญเชิญพิเศษการ์ดนี้" in desc):
        roles.append("extender")
    elif spec_summon and (from_hand or "from your gy" in desc or "จากสุสาน" in desc or "จากในสุสาน" in desc):
        roles.append("extender")
        
    # 4. Payoff / Boss
    if is_extra:
        roles.append("payoff")
    elif is_monster and atk >= 2500 and (negate or destroy or banish):
        roles.append("payoff")
        
    # 5. Disruption / Interruption
    if (negate or destroy or banish or send_to_gy) and (is_quick_effect or is_trap):
        roles.append("disruption")
        
    # 6. Recovery
    if add_to_hand and ("from your gy" in desc or "จากสุสาน" in desc or "จากในสุสาน" in desc):
        roles.append("recovery")
    elif spec_summon and ("from your gy" in desc or "จากสุสาน" in desc or "จากในสุสาน" in desc) and "extender" not in roles:
        roles.append("recovery")
    elif ("draw" in desc or "จั่ว" in desc) and ("card" in desc or "การ์ด" in desc):
        roles.append("recovery")
        
    # 7. Floodgate
    if ("neither player can" in desc or "ผู้เล่นทั้งสองฝ่ายไม่สามารถ" in desc or "ผู้เล่นแต่ละฝ่ายไม่สามารถ" in desc
            or "ผู้เล่นทั้งสองไม่สามารถ" in desc or "ผู้เล่นแต่ละคนไม่สามารถ" in desc
            or "cannot special summon" in desc or "ไม่สามารถอัญเชิญแบบพิเศษ" in desc or "ไม่สามารถอัญเชิญพิเศษ" in desc
            or "effects are negated" in desc or "ผลจะถูกทำให้เป็นโมฆะ" in desc or "เอฟเฟกต์จะถูกทำให้เป็นโมฆะ" in desc
            or "ถูกทำให้เอฟเฟกต์เป็นโมฆะ" in desc or "ถูกยกเลิก" in desc):
        roles.append("floodgate")
        
    # Clean duplicates & default combo_piece if it's monster, disruption for traps, extender for spells
    roles = list(set(roles))
    if not roles:
        if is_monster:
            roles.append("combo_piece")
        elif is_trap:
            roles.append("disruption")
        elif is_spell:
            roles.append("extender")
            
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
                    
    # Clean keys 0 and "0", and assign default roles if empty or missing
    if 0 in reg_dict:
        del reg_dict[0]
    if "0" in reg_dict:
        del reg_dict["0"]
    for entry in reg_dict.values():
        if not entry.get("roles") or len(entry.get("roles", [])) == 0:
            entry["roles"] = ["combo_piece"]

    # Save back
    save_registry_list(reg_dict, reg_path)
        
    print(f"\n✅ Completed Auto Role Detection. Updated {detected_count} cards in registry: {reg_path}")

if __name__ == "__main__":
    main()
