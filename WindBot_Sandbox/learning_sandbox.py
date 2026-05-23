import os
import json
import re
import sys

from shared_utils import (
    configure_utf8, SCRIPT_DIR, PROJECT_ROOT,
    get_registry_paths, get_available_decks,
    load_registry_list, save_registry_list, LIVE_LOGS_DIR,
)

configure_utf8()

MOCK_LOGS_DIR = os.path.join(SCRIPT_DIR, "mock_logs")

def extract_deck_name(dir_name):
    available = get_available_decks(ai_only=False)
    for d in sorted(available, key=len, reverse=True):
        if dir_name.startswith(d + "_") or dir_name == d:
            return d
    parts = dir_name.split("_")
    if len(parts) >= 3:
        return "_".join(parts[:-2])
    return "UnifiedIgnis" # Fallback

def discover_match_dirs(logs_dir, latest_n=None):
    if not os.path.exists(logs_dir):
        return []
    
    match_dirs = []
    for entry in os.listdir(logs_dir):
        full_path = os.path.join(logs_dir, entry)
        if os.path.isdir(full_path):
            if os.path.exists(os.path.join(full_path, "match_summary.log")):
                match_dirs.append(full_path)
    
    match_dirs.sort(reverse=True)
    if latest_n is not None:
        match_dirs = match_dirs[:latest_n]
    return match_dirs

def parse_match_outcome(match_dir, min_turns_for_draw=3):
    summary_path = os.path.join(match_dir, "match_summary.log")
    if not os.path.exists(summary_path):
        return "Unknown", 0, 0, 0
    
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
                # Timeout/draw — use LP diff as proxy
                if bot_lp > opp_lp + 3000:
                    outcome = "WeakWin"
                elif opp_lp > bot_lp + 3000:
                    outcome = "WeakLoss"
                else:
                    outcome = "Draw"
            else:
                outcome = "Tie/Aborted"
    return outcome, bot_lp, opp_lp, max_turn

def parse_decisions_jsonl(match_dir):
    decisions_path = os.path.join(match_dir, "decisions.jsonl")
    decisions = []
    if not os.path.exists(decisions_path):
        return decisions
    
    with open(decisions_path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                decisions.append(json.loads(line))
            except json.JSONDecodeError:
                continue
    return decisions

def parse_disruptions_from_logs(match_dir):
    disrupted_card_ids = []
    choke_point_regex = re.compile(r"WARNING: Opponent disrupted Bot's choke point .*?\(ID:\s*(\d+)\)")
    
    for filename in os.listdir(match_dir):
        if filename.startswith("turn_") and filename.endswith(".log"):
            filepath = os.path.join(match_dir, filename)
            with open(filepath, "r", encoding="utf-8") as f:
                for line in f:
                    choke_match = choke_point_regex.search(line)
                    if choke_match:
                        card_id = int(choke_match.group(1))
                        disrupted_card_ids.append(card_id)
    return list(set(disrupted_card_ids))

def analyze_single_match(match_dir):
    dir_name = os.path.basename(match_dir)
    print(f"\n{'─'*50}")
    print(f"📂 Analyzing: {dir_name}")
    
    outcome, bot_lp, opp_lp, turns = parse_match_outcome(match_dir)
    print(f"   Outcome: {outcome} | Bot LP: {bot_lp} | Opp LP: {opp_lp} | Turns: {turns}")
    
    decisions = parse_decisions_jsonl(match_dir)
    disrupted_ids = parse_disruptions_from_logs(match_dir)
    
    if decisions:
        print(f"   Decisions loaded: {len(decisions)} entries from decisions.jsonl")
    else:
        print(f"   No decisions.jsonl found — using turn log regex fallback")
    
    if disrupted_ids:
        print(f"   Choke point disruptions detected: {len(disrupted_ids)} cards")
    
    return {
        "dir": match_dir,
        "dir_name": dir_name,
        "outcome": outcome,
        "bot_lp": bot_lp,
        "opp_lp": opp_lp,
        "decisions": decisions,
        "disrupted_ids": disrupted_ids
    }

def apply_learning(registry, all_match_analyses):
    if not registry or not all_match_analyses:
        return False
    
    reg_dict = {card["id"]: card for card in registry}
    changes = []
    
    WIN_OUTCOMES = ("Win", "WeakWin")
    LOSS_OUTCOMES = ("Loss", "WeakLoss")
    DRAW_OUTCOMES = ("Draw",)
    
    for analysis in all_match_analyses:
        outcome = analysis["outcome"]
        decisions = analysis["decisions"]
        disrupted_ids = analysis["disrupted_ids"]
        dir_name = analysis["dir_name"]
        
        if decisions:
            for d in decisions:
                card_id = d.get("card_id", 0)
                if card_id not in reg_dict:
                    continue
                
                card = reg_dict[card_id]
                score = d.get("score", 0)
                decision = d.get("decision", False)
                goal = d.get("goal", "")
                lp_self = d.get("lp_self", 8000)
                
                if outcome in WIN_OUTCOMES and decision and score > 150:
                    delta = 1 if outcome == "Win" else 0
                    old_p = card.get("priority", 5)
                    new_p = min(10, old_p + delta)
                    if new_p != old_p:
                        card["priority"] = new_p
                        changes.append(f"  [{dir_name}] Card {card_id} ({d.get('card_name', '?')}): priority {old_p} -> {new_p} ({outcome} + high score {score:.0f})")
                
                if outcome in LOSS_OUTCOMES and decision and score > 100:
                    old_p = card.get("priority", 5)
                    decrease = 1 if outcome == "Loss" else (1 if old_p >= 4 else 0)
                    new_p = max(1, old_p - decrease)
                    if new_p != old_p:
                        card["priority"] = new_p
                        changes.append(f"  [{dir_name}] Card {card_id} ({d.get('card_name', '?')}): priority {old_p} -> {new_p} ({outcome} despite high score {score:.0f})")
                
                if outcome in DRAW_OUTCOMES and decision and score > 100 and card.get("priority", 5) >= 9:
                    old_p = card.get("priority", 5)
                    new_p = max(6, old_p - 1)
                    if new_p != old_p:
                        card["priority"] = new_p
                        changes.append(f"  [{dir_name}] Card {card_id} ({d.get('card_name', '?')}): priority {old_p} -> {new_p} (Draw anti-inflation decay)")
                
                if decision and goal == "push_lethal" and d.get("turn", 0) >= 2:
                    old_fv = card.get("followup_value", 5)
                    if outcome in WIN_OUTCOMES and old_fv < 10:
                        card["followup_value"] = old_fv + 1
                        changes.append(f"  [{dir_name}] Card {card_id} ({d.get('card_name', '?')}): followup_value {old_fv} -> {old_fv + 1} (helped combo continuation in {outcome})")
                
                if decision and lp_self < 3000 and outcome not in LOSS_OUTCOMES:
                    old_rv = card.get("recovery_value", 5)
                    if old_rv < 10:
                        card["recovery_value"] = old_rv + 1
                        changes.append(f"  [{dir_name}] Card {card_id} ({d.get('card_name', '?')}): recovery_value {old_rv} -> {old_rv + 1} (used at low LP {lp_self} and survived)")
        
        for card_id in disrupted_ids:
            if card_id not in reg_dict:
                continue
            card = reg_dict[card_id]
            
            if outcome in LOSS_OUTCOMES:
                old_risk = card.get("risk_if_negated", 0)
                new_risk = min(10, old_risk + 1)
                if new_risk != old_risk:
                    card["risk_if_negated"] = new_risk
                    changes.append(f"  [{dir_name}] Card {card_id}: risk_if_negated {old_risk} -> {new_risk} (choke point disrupted in {outcome})")
                
                for other_card in registry:
                    if other_card["id"] != card_id:
                        roles = other_card.get("roles", [])
                        if "starter" not in roles and "payoff" not in roles:
                            old_bait = other_card.get("bait_value", 0)
                            if old_bait < 6 and old_bait > 0:
                                other_card["bait_value"] = old_bait + 1
    # Bait Value Anti-Inflation: decay high bait values and bootstrap zero-bait cards
    for card in registry:
        card_id = card["id"]
        old_bait = card.get("bait_value", 0)
        
        # Decay: if bait >= 6, reduce by 1 to prevent runaway inflation
        if old_bait >= 6:
            card["bait_value"] = old_bait - 1
            changes.append(f"  [Decay] Card {card_id}: bait_value {old_bait} -> {old_bait - 1} (anti-inflation decay)")
        
        # Bootstrap: if bait == 0 and card was played in a win, set to 1
        was_played_in_win = False
        for analysis in all_match_analyses:
            if analysis["outcome"] == "Win":
                for d in analysis.get("decisions", []):
                    if d.get("card_id") == card_id and d.get("decision", False):
                        was_played_in_win = True
                        break
            if was_played_in_win:
                break
        if was_played_in_win and old_bait == 0:
            card["bait_value"] = 1
            changes.append(f"  [Bootstrap] Card {card_id}: bait_value 0 -> 1 (played in win, bootstrap)")

    return changes

def main():
    import argparse
    parser = argparse.ArgumentParser(description="Self-Learning Engine Heuristics Adjuster")
    parser.add_argument("--deck", type=str, default=None, help="Specific deck name to process learning for")
    args = parser.parse_args()

    print("=== SELF-LEARNING ENGINE v2.0 ===")
    if args.deck:
        print(f"Filtering updates for deck: {args.deck}")
    
    match_dirs = discover_match_dirs(LIVE_LOGS_DIR)
    source = "LIVE"
    
    if not match_dirs:
        print(f"\nNo LIVE logs found at {LIVE_LOGS_DIR}, falling back to mock_logs/")
        match_dirs = discover_match_dirs(MOCK_LOGS_DIR)
        source = "MOCK"
    
    if not match_dirs:
        print("Error: No match logs found anywhere!")
        return
    
    print(f"\n📁 Found {len(match_dirs)} match(es) from {source} logs:")
    for md in match_dirs:
        print(f"   - {os.path.basename(md)}")
    
    all_analyses = []
    for match_dir in match_dirs:
        analysis = analyze_single_match(match_dir)
        all_analyses.append(analysis)
    
    # Group analyses by deck name
    deck_groups = {}
    for analysis in all_analyses:
        deck_name = extract_deck_name(analysis["dir_name"])
        if deck_name not in deck_groups:
            deck_groups[deck_name] = []
        deck_groups[deck_name].append(analysis)
        
    for deck_name, matches in deck_groups.items():
        if args.deck and deck_name != args.deck:
            continue
        print(f"\n🧠 Processing learning updates for Deck: {deck_name} ({len(matches)} matches)")
        sandbox_reg, live_reg = get_registry_paths(deck_name)
        registry = load_registry_list(sandbox_reg)
        if not registry:
            print(f"⚠️ Could not load registry for deck {deck_name}. Skipping.")
            continue
            
        wins = sum(1 for a in matches if a["outcome"] in ("Win", "WeakWin"))
        losses = sum(1 for a in matches if a["outcome"] in ("Loss", "WeakLoss"))
        draws = sum(1 for a in matches if a["outcome"] == "Draw")
        ties = sum(1 for a in matches if a["outcome"] == "Tie/Aborted")
        unknowns = sum(1 for a in matches if a["outcome"] == "Unknown")
        
        print(f"📊 Deck Summary: Wins: {wins} | Losses: {losses} | Draws: {draws} | Ties: {ties} | Unknown: {unknowns}")
        
        changes = apply_learning(registry, matches)
        
        if changes:
            print(f"📝 Total adjustments: {len(changes)}")
            for c in changes[:15]:
                print(c)
            if len(changes) > 15:
                print(f"  ... and {len(changes) - 15} more adjustments")
            
            save_registry_list(registry, sandbox_reg)
            print(f"✅ Saved updated registry to Sandbox: {sandbox_reg}")
            
            if os.path.exists(os.path.dirname(live_reg)):
                save_registry_list(registry, live_reg)
                print(f"✅ Synced updated registry to LIVE WindBot: {live_reg}")
            else:
                print("⚠️  Warning: Real WindBot config directory not found. Could not sync.")
        else:
            print("✅ No adjustments needed — current weights are optimal based on match data.")
            
    print("\n=== SELF-LEARNING ENGINE COMPLETE ===")

if __name__ == "__main__":
    main()
