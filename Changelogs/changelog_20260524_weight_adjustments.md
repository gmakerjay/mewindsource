# Changelog - Bot Weight Adjustments and Macro-Decision Refactoring

**Timestamp**: 2026-05-24T01:14:00+07:00  
**Author**: Antigravity AI  

---

## 1. Scoring Weight Adjustments

- **C# Engine (`UnifiedIgnisExecutor.cs`)**:
  - Increased the penalty weight for blocked/dead combo plans in `EvaluateCardAction()` from `-40.0` to `-90.0`. This prevents the bot from blindly playing/extending blocked combos under high threat levels.

---

## 2. Macro-Decision Refactoring Upgrades

- **C# Engine (`UnifiedIgnisExecutor.cs`)**:
  - **Lethal Check (Anti-Overextension)**: Added `IsLethalOnBoard()` helper to check if current on-board attack power is enough to win when the opponent has no monsters. Penalizes combo/extender/starter cards by `-100.0` in `Main1` if lethal is on board.
  - **Redundant Field Spell Protection**: Penalizes duplicate Field Spell activations by `-500.0` if a Field Spell is already face-up on `Bot.SpellZone[5]`.
  - **Anti-Self Harm**: Penalizes negate/removal/interruption cards by `-200.0` if chaining to the bot's own activations.
  - **Smart Trap Setting**: Rewrote `OnDefaultSpellSet()` to apply a `-30.0` penalty for setting Traps and Quick-Play Spells in `Main1` of `Turn > 1` so they are deferred to `Main2` naturally.

---

## 3. Verification

- Verified successful compilation of C# files using `compile_ai.bat` (0 errors, 0 warnings).
