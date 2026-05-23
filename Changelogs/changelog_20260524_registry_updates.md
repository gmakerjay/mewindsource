# Changelog - Deck Registries Update & Deployment

**Timestamp**: 2026-05-24T00:50:00+07:00  
**Author**: Senior Developer (Python & C#)

---

## 1. Verification of Critical Bug Fixes
Verified the completion of the 5 requested bug fixes across 3 files:
- **`WindBot_Sandbox/ab_tournament.py`**:
  - `parse_match_outcome` correctly returns 4 variables `(outcome, bot_lp, opp_lp, turns)` on missing logs to prevent unpacking crashes.
  - Subclass names and constructors inside `injected_subclasses` are correctly prefixed with `Deck_` to ensure valid C# classes for decks starting with numbers.
- **`WindBot/UnifiedIgnisExecutor.cs`**:
  - `HasStarterOrExtenderInHand` and normal summon check logic successfully include `"payoff"` and `"searcher"` card roles.
  - `CalculateTotalDangerForField` correctly loops and evaluates card danger for opponent's hand (`Fields[1].Hand`) and graveyard (`Fields[1].Graveyard`).
- **`WindBot_Sandbox/cockpit.py`**:
  - The unused function `forward_bot_logs()` has been completely removed.

## 2. Compilation
- Compiled the C# WindBot executable using `compile_ai.bat` in the C# project directory. The build completed with **SUCCESSFUL** status and zero errors.

## 3. Deck Registry Role Detection & Activation
Ran the Auto Role Detector tool on the 4 targeted decks to identify card roles and update their configurations:
- `python WindBot_Sandbox/auto_role_detector.py --deck 2026_Goldlord`
- `python WindBot_Sandbox/auto_role_detector.py --deck 2026_Invoke`
- `python WindBot_Sandbox/auto_role_detector.py --deck 2026_Kwtune`
- `python WindBot_Sandbox/auto_role_detector.py --deck 2026_Labrynth`

All 4 decks are now fully registered and activated in the AI system.

## 4. Live Deployment & Sync
- Deployed the optimized Sandbox registry configurations for all 4 decks to the live WindBot folder:
  - `WindBot/config/cards_registry_2026_Goldlord.json`
  - `WindBot/config/cards_registry_2026_Invoke.json`
  - `WindBot/config/cards_registry_2026_Kwtune.json`
  - `WindBot/config/cards_registry_2026_Labrynth.json`
  - `WindBot/config/opponent_memory.json`

## 5. GitHub Synchronization
- Staged all updated config and registry files.
- Committed with message: `"Update card registries and opponent memory using auto role detector"`
- Pushed changes successfully to remote repository: `https://github.com/gmakerjay/mewindsource` (branch: `master`).
