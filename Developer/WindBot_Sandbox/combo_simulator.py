import os
import random
import sys
import argparse

from shared_utils import (
    configure_utf8, get_registry_paths, get_available_decks,
    load_ydk_main_deck, load_registry_dict, save_registry_list,
)

configure_utf8()

def run_simulation(deck, registry, deck_name, handtrap_chance=0.35, num_simulations=100000):
    success_plan_a = 0
    success_plan_b = 0
    success_recovery = 0
    fail_no_starter = 0
    fail_disrupted = 0
    
    card_starter_hits = {}
    card_brick_hits = {}
    card_rescue_hits = {}
    
    deck_metadata = []
    for card_id in deck:
        meta = registry.get(card_id, {
            "roles": [],
            "priority": 5,
            "bait_value": 0,
            "risk_if_negated": 0,
            "followup_value": 5,
            "recovery_value": 5
        })
        deck_metadata.append((card_id, meta))
        
    for _ in range(num_simulations):
        hand = random.sample(deck_metadata, min(5, len(deck_metadata)))
        
        has_starter = False
        has_extender = False
        has_bait = False
        has_called_by = False
        has_followup = False
        has_recovery = False
        
        for card_id, meta in hand:
            roles = meta.get("roles", [])
            if "starter" in roles:
                has_starter = True
            if "extender" in roles:
                has_extender = True
            if meta.get("bait_value", 0) >= 3:
                has_bait = True
            if card_id == 24224830:
                has_called_by = True
            if meta.get("followup_value", 0) > 5:
                has_followup = True
            if meta.get("recovery_value", 0) > 5:
                has_recovery = True
                
        opp_has_disruption = random.random() < handtrap_chance
        hand_ids = [cid for cid, _ in hand]
        
        if not has_starter:
            fail_no_starter += 1
            for cid in hand_ids:
                card_brick_hits[cid] = card_brick_hits.get(cid, 0) + 1
            if has_recovery and has_extender:
                success_recovery += 1
                for cid, m in hand:
                    if m.get("recovery_value", 0) > 5:
                        card_rescue_hits[cid] = card_rescue_hits.get(cid, 0) + 1
        else:
            if opp_has_disruption:
                starters_count = len([c for c, m in hand if "starter" in m.get("roles", [])])
                if has_called_by or (has_bait and starters_count > 1):
                    success_plan_a += 1
                    for cid in hand_ids:
                        card_starter_hits[cid] = card_starter_hits.get(cid, 0) + 1
                else:
                    if has_extender:
                        success_plan_b += 1
                        for cid, m in hand:
                            if "extender" in m.get("roles", []):
                                card_rescue_hits[cid] = card_rescue_hits.get(cid, 0) + 1
                    elif has_followup:
                        success_plan_b += 1
                        for cid, m in hand:
                            if m.get("followup_value", 0) > 5:
                                card_rescue_hits[cid] = card_rescue_hits.get(cid, 0) + 1
                    elif has_recovery:
                        success_recovery += 1
                        for cid, m in hand:
                            if m.get("recovery_value", 0) > 5:
                                card_rescue_hits[cid] = card_rescue_hits.get(cid, 0) + 1
                    else:
                        fail_disrupted += 1
                        for cid in hand_ids:
                            card_brick_hits[cid] = card_brick_hits.get(cid, 0) + 1
            else:
                success_plan_a += 1
                for cid in hand_ids:
                    custom_hits = card_starter_hits.get(cid, 0)
                    card_starter_hits[cid] = custom_hits + 1
                
    pct_plan_a = (success_plan_a / num_simulations) * 100
    pct_plan_b = (success_plan_b / num_simulations) * 100
    pct_recovery = (success_recovery / num_simulations) * 100
    pct_no_starter = (fail_no_starter / num_simulations) * 100
    pct_disrupted = (fail_disrupted / num_simulations) * 100
    pct_total_success = pct_plan_a + pct_plan_b + pct_recovery
    
    return {
        "deck_name": deck_name,
        "plan_a": pct_plan_a,
        "plan_b": pct_plan_b,
        "recovery": pct_recovery,
        "no_starter": pct_no_starter,
        "disrupted": pct_disrupted,
        "overall_success": pct_total_success,
        "card_starter_hits": card_starter_hits,
        "card_brick_hits": card_brick_hits,
        "card_rescue_hits": card_rescue_hits
    }

def apply_optimization(registry, all_results):
    changes = []
    total_starter_hits = {}
    total_brick_hits = {}
    total_rescue_hits = {}
    
    for r in all_results:
        for cid, count in r["card_starter_hits"].items():
            total_starter_hits[cid] = total_starter_hits.get(cid, 0) + count
        for cid, count in r["card_brick_hits"].items():
            total_brick_hits[cid] = total_brick_hits.get(cid, 0) + count
        for cid, count in r["card_rescue_hits"].items():
            total_rescue_hits[cid] = total_rescue_hits.get(cid, 0) + count
    
    for card_id, card in registry.items():
        brick = total_brick_hits.get(card_id, 0)
        starter = total_starter_hits.get(card_id, 0)
        rescue = total_rescue_hits.get(card_id, 0)
        
        old_priority = card.get("priority", 5)
        old_bait = card.get("bait_value", 0)
        
        if brick > 0 and starter > 0:
            brick_ratio = brick / (brick + starter)
            if brick_ratio > 0.65 and old_priority > 1:
                card["priority"] = old_priority - 1
                changes.append(f"  Card {card_id}: priority {old_priority} -> {old_priority - 1} (brick ratio: {brick_ratio:.2f})")
        
        if rescue > starter * 0.1 and rescue > 500:
            new_bait = min(8, old_bait + 1)  # Capped at 8 (not 10) to prevent bait inflation
            if new_bait != old_bait:
                card["bait_value"] = new_bait
                changes.append(f"  Card {card_id}: bait_value {old_bait} -> {new_bait} (rescue count: {rescue})")
    
    return changes


def main():
    parser = argparse.ArgumentParser(description="Dual Deck Combo Simulator")
    parser.add_argument("--deck", type=str, default="all", help="Deck to simulate (e.g. 2026_AzaYummy) or 'all'")
    parser.add_argument("--simulations", type=int, default=100000, help="Number of simulations per deck")
    parser.add_argument("--optimize", action="store_true", help="Optimize weights after simulation")
    args = parser.parse_args()
    
    print("=== DUAL DECK COMBO SIMULATOR START ===")
    if args.optimize:
        print("MODE: Optimize & Sync (will write changes back)")
    else:
        print("MODE: Report Only (read-only)")
    
    sandbox_reg, live_reg = get_registry_paths(args.deck)
    registry = load_registry_dict(sandbox_reg)
    print(f"Loaded Card Registry: {len(registry)} profiles.")
    
    available = get_available_decks()
    if args.deck == "all":
        decks_to_simulate = available
    else:
        if args.deck in available:
            decks_to_simulate = [args.deck]
        else:
            print(f"Error: Deck '{args.deck}' not found in Decks directory.")
            sys.exit(1)
            
    results = []
    for deck_name in decks_to_simulate:
        deck = load_ydk_main_deck(deck_name)
        if not deck:
            print(f"Skipping {deck_name} because deck file was not found.")
            continue
            
        print(f"\nSimulating {deck_name} ({len(deck)} cards)...")
        res = run_simulation(deck, registry, deck_name, handtrap_chance=0.35, num_simulations=args.simulations)
        results.append(res)
        
    print("\n" + "="*72)
    print("                      SUMMARY DECK COMPARISON")
    print("="*72)
    print(f"{'Deck Name':<16} | {'Plan A':<8} | {'Plan B':<8} | {'Recovery':<8} | {'No Start':<8} | {'Disrupt':<7} | {'OVERALL':<7}")
    print("-"*72)
    for r in results:
        print(f"{r['deck_name']:<16} | {r['plan_a']:>6.2f}% | {r['plan_b']:>6.2f}% | {r['recovery']:>6.2f}% | {r['no_starter']:>6.2f}% | {r['disrupted']:>5.2f}% | {r['overall_success']:>5.2f}%")
    print("="*72)
    
    if len(results) >= 2:
        aza = results[0]
        elf = results[1]
        print("\n📊 การวิเคราะห์ผลเปรียบเทียบความคุ้มค่า (Analysis Commentary):")
        if aza["no_starter"] < elf["no_starter"]:
            print(f" * ความเสถียรของมือเปิด: {aza['deck_name']} จั่วเปิดเล่นได้ดีกว่า (มีอัตราจั่วเน่าจาก Starter {aza['no_starter']:.2f}% vs {elf['no_starter']:.2f}%)")
        else:
            print(f" * ความเสถียรของมือเปิด: {elf['deck_name']} จั่วเปิดเล่นได้ดีกว่า (มีอัตราจั่วเน่าจาก Starter {elf['no_starter']:.2f}% vs {aza['no_starter']:.2f}%)")
            
        if aza["disrupted"] < elf["disrupted"]:
            print(f" * การทนทานต่อแฮนด์แทรป: {aza['deck_name']} รับมือการขัดจังหวะได้ดีกว่า (โดนขัดแล้วจอดสนิทเพียง {aza['disrupted']:.2f}% vs {elf['disrupted']:.2f}%)")
        else:
            print(f" * การทนทานต่อแฮนด์แทรป: {elf['deck_name']} รับมือการขัดจังหวะได้ดีกว่า (โดนขัดแล้วจอดสนิทเพียง {elf['disrupted']:.2f}% vs {aza['disrupted']:.2f}%)")

        if aza["recovery"] > elf["recovery"]:
            print(f" * ความสามารถกู้คืน: {aza['deck_name']} กู้คืนจาก brick/disruption ได้ดีกว่า ({aza['recovery']:.2f}% vs {elf['recovery']:.2f}%)")
        else:
            print(f" * ความสามารถกู้คืน: {elf['deck_name']} กู้คืนจาก brick/disruption ได้ดีกว่า ({elf['recovery']:.2f}% vs {aza['recovery']:.2f}%)")
            
        if aza["overall_success"] > elf["overall_success"]:
            print(f" * สรุปเด็คที่เล่นจบคอมโบได้สม่ำเสมอที่สุด: {aza['deck_name']} ({aza['overall_success']:.2f}%)")
        else:
            print(f" * สรุปเด็คที่เล่นจบคอมโบได้สม่ำเสมอที่สุด: {elf['deck_name']} ({elf['overall_success']:.2f}%)")

    if args.optimize and results:
        print("\n" + "="*50)
        print("  OPTIMIZATION: Analyzing simulation data...")
        print("="*50)
        
        changes = apply_optimization(registry, results)
        if changes:
            print(f"\n📝 Weight adjustments ({len(changes)} changes):")
            for c in changes:
                print(c)
            
            save_registry_list(registry, sandbox_reg)
            print(f"\n✅ Saved optimized config to Sandbox: {sandbox_reg}")
            
            if os.path.exists(os.path.dirname(live_reg)):
                save_registry_list(registry, live_reg)
                print(f"✅ Synced optimized config to LIVE WindBot: {live_reg}")
            else:
                print("⚠️ Warning: Real WindBot config directory not found. Could not sync.")
        else:
            print("\n✅ No weight adjustments needed — current weights are already optimal for this simulation.")

if __name__ == "__main__":
    main()
