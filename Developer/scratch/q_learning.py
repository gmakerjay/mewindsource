import os
import sys
import json
import re
import argparse

from shared_utils import (
    configure_utf8, get_registry_paths, save_registry_list,
    SCRIPT_DIR, LIVE_LOGS_DIR,
)

configure_utf8()


def discover_deck_matches(deck_name):
    if not os.path.exists(LIVE_LOGS_DIR):
        return []
    
    match_dirs = []
    prefix = f"{deck_name}_" if deck_name and deck_name != "all" else ""
    
    for entry in os.listdir(LIVE_LOGS_DIR):
        full_path = os.path.join(LIVE_LOGS_DIR, entry)
        if os.path.isdir(full_path) and entry.startswith(prefix):
            summary_path = os.path.join(full_path, "match_summary.log")
            decisions_path = os.path.join(full_path, "decisions.jsonl")
            if os.path.exists(summary_path) and os.path.exists(decisions_path):
                match_dirs.append(full_path)
                
    return match_dirs

def parse_match_outcome(match_dir, min_turns_for_draw=3):
    summary_path = os.path.join(match_dir, "match_summary.log")
    outcome = "Unknown"
    bot_lp = 0
    opp_lp = 0
    max_turn = 0
    
    # Count turns from turn_N.log files
    for filename in os.listdir(match_dir):
        if filename.startswith("turn_") and filename.endswith(".log"):
            try:
                turn_num = int(filename[5:-4])
                max_turn = max(max_turn, turn_num)
            except ValueError:
                pass
    
    with open(summary_path, "r", encoding="utf-8") as f:
        content = f.read()
        bot_lp_match = re.search(r"Final Bot LP:\s*(\d+)", content)
        opp_lp_match = re.search(r"Final Opponent LP:\s*(\d+)", content)
        if bot_lp_match and opp_lp_match:
            bot_lp = int(bot_lp_match.group(1))
            opp_lp = int(opp_lp_match.group(1))
            if bot_lp == 0 and opp_lp > 0:
                outcome = "Loss"
            elif opp_lp == 0 and bot_lp > 0:
                outcome = "Win"
            elif max_turn >= min_turns_for_draw:
                if bot_lp > opp_lp + 3000:
                    outcome = "WeakWin"
                elif opp_lp > bot_lp + 3000:
                    outcome = "WeakLoss"
                else:
                    outcome = "Draw"
            else:
                outcome = "Tie"
    return outcome, bot_lp, opp_lp, max_turn

def parse_decisions(match_dir):
    decisions_path = os.path.join(match_dir, "decisions.jsonl")
    decisions = []
    with open(decisions_path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                decisions.append(json.loads(line))
            except:
                pass
    return decisions

def main():
    parser = argparse.ArgumentParser(description="Q-Learning Reinforcement Trainer")
    parser.add_argument("--deck", type=str, required=True, help="Deck name to run Q-learning training for")
    parser.add_argument("--alpha", type=float, default=0.1, help="Learning rate (alpha)")
    parser.add_argument("--gamma", type=float, default=0.9, help="Discount factor (gamma)")
    args = parser.parse_args()

    print(f"=== STARTING Q-LEARNING REINFORCEMENT LEARNING ===")
    print(f"Deck: {args.deck}")
    print(f"Learning Rate (alpha): {args.alpha}")
    print(f"Discount Factor (gamma): {args.gamma}")

    match_dirs = discover_deck_matches(args.deck)
    print(f"Discovered {len(match_dirs)} matches with decisions logs for {args.deck}.")
    
    if not match_dirs:
        print("No log matches found. Please play some matches with the bot first to gather log data.")
        sys.exit(0)

    sandbox_path, live_path = get_registry_paths(args.deck)
    if not os.path.exists(sandbox_path):
        print(f"Error: Registry not found at {sandbox_path}")
        sys.exit(1)

    with open(sandbox_path, "r", encoding="utf-8-sig") as f:
        registry = json.load(f)

    # Convert registry list to dict for fast lookup
    reg_dict = {card["id"]: card for card in registry}

    # Track Q-value updates
    update_count = 0
    total_decisions_processed = 0

    for match_dir in match_dirs:
        outcome, bot_lp, opp_lp, turns = parse_match_outcome(match_dir)
        
        # Outcome Reward mapping
        reward_map = {
            "Win": 1.0,
            "WeakWin": 0.5,
            "Draw": 0.0,
            "WeakLoss": -0.5,
            "Loss": -1.0,
            "Tie": 0.0,
        }
        
        if outcome not in reward_map:
            continue
        
        base_reward = reward_map[outcome]
        reward = base_reward + (bot_lp - opp_lp) / 8000.0 * 0.2 - turns * 0.01

        decisions = parse_decisions(match_dir)
        total_decisions_processed += len(decisions)
        
        # Episodic Monte Carlo Q-value update
        T = len(decisions)
        for t, dec in enumerate(decisions):
            card_id = dec["card_id"]
            goal = dec["goal"] # e.g. "establish_interruptions", "survive", "push_lethal"
            decision_made = dec["decision"] # boolean: whether we chose to activate/summon
            
            if not decision_made:
                continue # Only update Q-values for actions the bot chose to take
                
            if card_id not in reg_dict:
                # Add default registry entry for unregistered cards if played
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
                
            card_meta = reg_dict[card_id]
            if "q_values" not in card_meta:
                card_meta["q_values"] = {}
                
            # Current Q-value
            q_values = card_meta["q_values"]
            current_q = q_values.get(goal, 0.0)
            
            # Discounted future return (MC return)
            # Early decisions get higher return (discounted less relative to the start)
            steps_from_start = t
            G_t = reward * (args.gamma ** steps_from_start)
            
            # TD update step
            new_q = current_q + args.alpha * (G_t - current_q)
            # Clamp Q-values to [-2.0, 2.0] range to prevent extreme weights
            new_q = max(-2.0, min(2.0, new_q))
            
            q_values[goal] = round(new_q, 4)
            update_count += 1

    # Ensure all priorities are capped at 8 (Iron Rule #5)
    for card in reg_dict.values():
        if "priority" in card and card["priority"] > 8:
            card["priority"] = 8

    # Save registry
    save_registry_list(reg_dict, sandbox_path)
    live_dir = os.path.dirname(live_path)
    if os.path.exists(live_dir):
        save_registry_list(reg_dict, live_path)

    print(f"\n✅ Q-Learning training completed.")
    print(f"Processed {len(match_dirs)} matches, {total_decisions_processed} decision steps.")
    print(f"Performed {update_count} Q-value updates in registry: {sandbox_path}")

if __name__ == "__main__":
    main()
