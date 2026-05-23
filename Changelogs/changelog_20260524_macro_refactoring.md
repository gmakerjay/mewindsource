# Changelog - Macro-Decision Making Refactoring

**Timestamp**: 2026-05-24T01:14:00+07:00  
**Author**: Antigravity AI  

---

## 1. Macro-Decision Refactoring Upgrades

- **C# Engine (`UnifiedIgnisExecutor.cs`)**:
  - Added `IsLethalOnBoard()` helper to check if current on-board attack power is enough to win when the opponent has no monsters.
  - Implemented **Anti-Overextension / Lethal Check**: Penalizes combo/extender/starter cards by `-100.0` in `Main1` if lethal is on board.
  - Implemented **Redundant Field Spell Protection**: Penalizes Duplicate Field Spell activations by `-500.0` if already face-up on `Bot.SpellZone[5]`.
  - Implemented **Anti-Self Harm**: Penalizes negate/removal/interruption cards by `-200.0` if chaining to the bot's own activations.
  - Implemented **Smart Trap Setting**: Rewrote `OnDefaultSpellSet()` to apply a `-30.0` penalty for setting Traps and Quick-Play Spells in `Main1` of `Turn > 1` so they are deferred to `Main2` naturally.

---

## 2. Verification

- Verified successful compilation of C# files using `compile_ai.bat`.
