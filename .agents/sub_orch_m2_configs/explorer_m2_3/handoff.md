# Handoff Report — Explorer 3 (Milestone 2)

## 1. Observation
- **Deck Config Files**: View of JSON configs under `WindBot/config/decks/` showed that:
  - `2026_Goldlord.json` (line 10) defines `"choke_points": [ 95825075 ]`.
  - `2026_Labrynth.json` (line 10) defines `"choke_points": [ 23440079 ]`.
  - `2026_Kwtune.json` (line 9) defines `"choke_points": []`.
- **YDK Files**: View of `WindBot/Decks/2026_Goldlord.ydk` and `WindBot/Decks/2026_Labrynth.ydk` confirmed:
  - Eldorado Adelantado (`95825075`) is not present in `2026_Goldlord.ydk`.
  - Labrynth Coelophys (`23440079`) is not present in `2026_Labrynth.ydk`.
- **Registry Completeness**: Manual comparison and analysis of YDK lists and `WindBot/config/cards_registry_2026_<deck>.json` confirmed that all YDK card IDs exist in their corresponding registry files.
- **Sandbox Scripts**:
  - `WindBot_Sandbox/shared_utils.py` (lines 127–132):
    ```python
    if line.startswith("#extra") or line.startswith("!side"):
        in_main = False
        continue
    ```
    This function `load_ydk_main_deck` stops parsing when it hits `#extra` or `!side`, meaning that `auto_role_detector.py` (which uses this function) completely ignores Extra and Side decks.
  - `WindBot_Sandbox/cockpit.py` (lines 708–742):
    The `deploy_config` method copies the registry files from `WindBot_Sandbox/` to `WindBot/config/` and executes `compile_ai.bat` to rebuild the execution DLL.

---

## 2. Logic Chain
1. *Observation*: The C# configuration loading in `BaseCustomExecutor.cs` (lines 539–544) loads `cards_registry_2026_<deck>.json` first and only falls back to the general `cards_registry.json` if it's missing.
2. *Observation*: All card IDs in YDK files for the 4 bricked decks are present in their deck-specific registries.
3. *Inference*: Therefore, the C# AI bot is not "bricked" due to missing card IDs in the active deck-specific JSON registries.
4. *Observation*: `2026_Goldlord.json` and `2026_Labrynth.json` reference obsolete/missing choke point card IDs (`95825075`, `23440079`) which are absent from their YDK decks.
5. *Observation*: `2026_Kwtune.json` lists no choke points.
6. *Inference*: The bot is likely underperforming or behaving sub-optimally ("bricked") because of incorrect choke point config parameters and unoptimized/outdated heuristics.
7. *Observation*: `shared_utils.py` contains `load_ydk_main_deck` which terminates parsing when reaching `#extra` or `!side`.
8. *Inference*: Running `auto_role_detector.py` will not register or update card roles for Extra or Side decks, leaving those sections unoptimized.

---

## 3. Caveats
- We did not execute live duels or optimization scripts because our role is strictly read-only and `run_command` timed out.
- We assume the card database `cards.cdb` is structurally intact, as we verified details using grep and sandbox script logs.

---

## 4. Conclusion
- **YDK vs Registry**: No card IDs are missing from the deck-specific live registries.
- **Auto Role Bug**: A bug in `shared_utils.py:load_ydk_main_deck` restricts auto-role detection to the main deck, ignoring extra and side deck cards.
- **Config Discrepancies**:
  - `Goldlord` choke point `95825075` is missing from `2026_Goldlord.ydk`.
  - `Labrynth` choke point `23440079` is missing from `2026_Labrynth.ydk`.
  - `Kwtune` has no choke points.
- **Deployment**: Live configs can be updated from sandbox via cockpit's `/api/deploy` endpoint, which copies registries and rebuilds `UnifiedIgnisExecutor.dll`.

---

## 5. Verification Method
1. **File Checks**:
   - Inspect `WindBot/config/decks/2026_Goldlord.json` (line 10) and verify that `95825075` is not in `WindBot/Decks/2026_Goldlord.ydk`.
   - Inspect `WindBot/config/decks/2026_Labrynth.json` (line 10) and verify that `23440079` is not in `WindBot/Decks/2026_Labrynth.ydk`.
2. **Code Checks**:
   - View `WindBot_Sandbox/shared_utils.py` (lines 127–132) to confirm that `load_ydk_main_deck` disables parsing for `#extra` and `!side` sections.
