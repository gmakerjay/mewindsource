# Changelog - Critical Bug Fixes & Dead Code Cleanup

**Timestamp**: 2026-05-24T00:25:00+07:00  
**Author**: Antigravity AI  

---

## 1. Dead Code & Unused Files Cleaned Up

- **C# Engine (`UnifiedIgnisExecutor.cs`)**:
  - Removed unused local variable `delta` from `ApplyRealTimeLearning()` win/loss block.
  - Simplified redundant conditions (e.g. `strength >= 0.5` and `meta.priority < 10` are redundant inside `WeakWin` block).
- **Python Sandbox**:
  - Removed unused function `forward_bot_logs()` in `cockpit.py`.
  - Removed unused constant `REGISTRY_PATH` from `cockpit.py`.
  - Cleaned up unused library imports: `import re` (`auto_role_detector.py`), `import json` (`ab_tournament.py`, `optimize_registry.py`), and `import glob` (`learning_sandbox.py`).
- **Project Structure**:
  - Deleted obsolete v1 rules documentation `Docs/IGNIS_AgenticSkill_and_IronRules.md`.
  - Deleted the 5 unused language subdirectories under `config/languages/`: `Deutsch`, `Español`, `Français`, `Italiano`, and `Português`.
  - Deleted 12 extra card database files (`.cdb`) under `expansions/` that are not part of the main `cards.cdb` database.

---

## 2. Critical & High-Severity Bug Fixes

- **Bricked Decks Resolved**:
  - Registered all 44 missing card IDs from the `.ydk` deck lists into both `WindBot/config/` and `WindBot_Sandbox/` JSON registries for `2026_Goldlord`, `2026_Invoke`, `2026_Kwtune`, and `2026_Labrynth`. All unique cards are now fully operational.
- **Roles & Combo Plans Deserialization**:
  - Replaced type-specific casting (`as ArrayList`) with robust `(IEnumerable)` casting, resolving the issue where `roles` and `combo_plans` were always null on Mono and custom CLR runtimes.
- **Order of Capping & Decay in Learning**:
  - Swapped the learning logic order in `ApplyRealTimeLearning()` so that the **Anti-Inflation Decay** runs **before** the **Hard Cap**, enabling decay to successfully pull down priority values.
- **Cross-contamination of Deck Logs**:
  - Added a `--deck` filter argument to `learning_sandbox.py` to target only the specified deck and linked it inside `run_match_learning.py`.
- **AB Tournament Crashes & Compiler Errors**:
  - Fixed `parse_match_outcome()` in `ab_tournament.py` to always return 4 values (preventing unpacking crashes).
  - Prefixed dynamic dynamic subclasses generation with `Deck_` to prevent invalid C# class names starting with digits.
- **Threat Detection Improvements**:
  - Updated `CalculateTotalDangerForField()` to scan the opponent's Graveyard and revealed Hand, factoring them into the danger metrics.
- **Combo Summon checks**:
  - Factored `payoff` card roles into combo checks and summons.

---

## 3. Launcher Path Update

- Updated `รันระบบควบคุม_Cockpit.bat` path slash formatting: `python WindBot_Sandbox\cockpit.py`.

---

## 4. Verification

- Verified successful C# executable compilation using `compile_ai.bat`.
- Verified error-free Python scripts syntax checks.
- Verified 0 missing cards remaining across all registries.
