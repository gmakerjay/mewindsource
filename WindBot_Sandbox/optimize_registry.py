import os
import random
import sys
import argparse

from shared_utils import (
    configure_utf8, get_registry_paths, get_available_decks,
    load_ydk_main_deck, load_registry_list, save_registry_list,
)

configure_utf8()

def run_fast_eval(deck, registry_dict, handtrap_chance=0.35, num_simulations=5000):
    success = 0
    deck_metadata = []
    for card_id in deck:
        meta = registry_dict.get(card_id, {
            "roles": [], "priority": 5, "bait_value": 0, "risk_if_negated": 0,
            "followup_value": 5, "recovery_value": 5
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
        
        if not has_starter:
            if has_recovery and has_extender:
                success += 0.5
        else:
            if opp_has_disruption:
                starters_count = len([c for c, m in hand if "starter" in m.get("roles", [])])
                if has_called_by or (has_bait and starters_count > 1):
                    success += 1
                else:
                    if has_extender:
                        success += 1
                    elif has_followup:
                        success += 0.75
                    elif has_recovery:
                        success += 0.5
            else:
                success += 1
                
    return (success / num_simulations) * 100

def get_overall_score(decks_list, registry_list):
    reg_dict = {card["id"]: card for card in registry_list}
    scores = []
    for deck in decks_list:
        scores.append(run_fast_eval(deck, reg_dict, num_simulations=3000))
    return sum(scores) / len(scores) if scores else 0.0

def main():
    parser = argparse.ArgumentParser(description="Heuristic Weight Optimizer")
    parser.add_argument("--deck", type=str, default="all", help="Deck to optimize (e.g. 2026_AzaYummy) or 'all'")
    parser.add_argument("--iterations", type=int, default=300, help="Number of Hill Climbing iterations")
    args = parser.parse_args()
    print("=== STARTING HEURISTIC WEIGHT OPTIMIZER ===")
    print(f"Target Deck: {args.deck}")
    print(f"Iterations: {args.iterations}")

    # Load target deck(s)
    available = get_available_decks()
    if args.deck == "all":
        target_decks = [load_ydk_main_deck(d) for d in available]
        target_decks = [d for d in target_decks if d]
        all_deck_cards = set()
        for d in target_decks:
            all_deck_cards.update(d)
    else:
        deck_cards = load_ydk_main_deck(args.deck)
        if not deck_cards:
            print(f"Error: Deck '{args.deck}' could not be loaded or is empty.")
            sys.exit(1)
        target_decks = [deck_cards]
        all_deck_cards = set(deck_cards)

    sandbox_reg, live_reg = get_registry_paths(args.deck)
    registry_list = load_registry_list(sandbox_reg)
    if not registry_list:
        print("Error: Registry is empty or not found.")
        sys.exit(1)

    best_score = get_overall_score(target_decks, registry_list)
    print(f"Initial Baseline Score: {best_score:.2f}%", flush=True)

    max_iterations = args.iterations
    try:
        for i in range(1, max_iterations + 1):
            cand_cards = [c for c in registry_list if c["id"] in all_deck_cards]
            if not cand_cards:
                break
                
            card = random.choice(cand_cards)
            original_vals = {
                "priority": card.get("priority", 5),
                "risk_if_negated": card.get("risk_if_negated", 0),
                "bait_value": card.get("bait_value", 0),
                "followup_value": card.get("followup_value", 5),
                "recovery_value": card.get("recovery_value", 5)
            }
            
            param_to_mutate = random.choice(["priority", "risk_if_negated", "bait_value", "followup_value", "recovery_value"])
            old_val = card.get(param_to_mutate, 5)
            
            delta = random.choice([-1, 1])
            new_val = max(1, min(10, old_val + delta))
            
            if new_val == old_val:
                continue
                
            card[param_to_mutate] = new_val
            new_score = get_overall_score(target_decks, registry_list)
            
            # Print status of every single iteration for real-time visualization
            print(f"Iteration {i:5d}/{max_iterations}: Testing Card {card['id']} | {param_to_mutate:15s} -> {new_val} | Score: {new_score:.2f}%", end="", flush=True)
            
            if new_score > best_score:
                best_score = new_score
                print(f" (IMPROVED - Accepted)", flush=True)
            else:
                card[param_to_mutate] = old_val
                print(f" (Rejected)", flush=True)
    except KeyboardInterrupt:
        print("\n[WARNING] Optimization interrupted by user. Saving best configuration found so far...", flush=True)

    print("\n" + "="*50)
    print(f"Optimization Finished after {i} iterations.")
    print(f"Final Optimized Score: {best_score:.2f}%")
    print("="*50)
    
    # Save optimized registry to Sandbox
    save_registry_list(registry_list, sandbox_reg)
    print(f"Saved optimized configuration to Sandbox: {sandbox_reg}")
    
    # Copy to REAL WindBot config
    if os.path.exists(os.path.dirname(live_reg)):
        save_registry_list(registry_list, live_reg)
        print(f"Successfully integrated optimized config with REAL WindBot: {live_reg}")
    else:
        print("Warning: Real WindBot config directory not found. Could not integrate.")

if __name__ == "__main__":
    main()
