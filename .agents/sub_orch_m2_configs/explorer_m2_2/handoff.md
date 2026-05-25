# Handoff Report - Explorer 2

## 1. Observation
*   **Deck registry files loading:** In `WindBot/BaseCustomExecutor.cs` (lines 539–544):
    ```csharp
    string deckRegistryName = "cards_registry_" + _resolvedDeckName + ".json";
    string registryPath = Path.Combine(baseDir, "config", deckRegistryName);
    if (!File.Exists(registryPath))
    {
        registryPath = Path.Combine(baseDir, "config", "cards_registry.json");
    }
    ```
*   **Deck registry contents for the 4 bricked decks:** Inspecting `WindBot/config/cards_registry_2026_Labrynth.json` (line 1, columns showing `"id":` entries matching all YDK card IDs like `81497285`, `2347656`, `73355772`, etc.) shows all IDs are present in the JSON file.
*   **Script implementation for YDK parsing:** In `WindBot_Sandbox/shared_utils.py` (lines 120–137):
    ```python
    main_deck = []
    in_main = False
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#created"):
                continue
            if line == "#main":
                in_main = True
                continue
            if line.startswith("#extra") or line.startswith("!side"):
                in_main = False
                continue
            if in_main:
                try:
                    main_deck.append(int(line))
                except ValueError:
                    pass
    ```
*   **Deck configurations:**
    *   `WindBot/config/decks/2026_Goldlord.json` specifies `"choke_points": [95825075]`. Eldorado Adelantado (`95825075`) is not in `Decks/2026_Goldlord.ydk`.
    *   `WindBot/config/decks/2026_Labrynth.json` specifies `"choke_points": [23440079]`. Labrynth Coelophys (`23440079`) is not in `Decks/2026_Labrynth.ydk`.
    *   `WindBot/config/decks/2026_Kwtune.json` specifies `"choke_points": []`.

## 2. Logic Chain
1.  **Registry Matching:** By reading the `.ydk` files for the 4 bricked decks and scanning their corresponding `cards_registry_2026_<deck>.json` files in `WindBot/config/`, we observed that all unique card IDs are successfully registered. Since the dynamic AI loader maps card actions to dynamic executors based on the registered IDs (loaded from the resolved path), the bricking issue is not caused by missing card IDs in these deck-specific registries.
2.  **Script Limitation:** In `shared_utils.py`, the `load_ydk_main_deck` function parses the YDK but strictly stops parsing when it encounters `#extra` or `!side`. The role-detector script `auto_role_detector.py` uses this helper to get the deck card list. Therefore, any unique cards located in the Extra Deck or Side Deck will be ignored and won't have default/detected roles generated, potentially leading to incomplete role assignments when registries are regenerated using the script.
3.  **Config Discrepancies:** By checking `2026_Goldlord.json` and `2026_Labrynth.json`, we found card IDs listed as choke points that are completely absent from the actual YDK decks. This indicates that the JSON configurations are out of sync with the actual YDK lists, leading to logical gaps where the AI is configured to protect or disrupt cards that cannot be drawn or played.

## 3. Caveats
*   The actual dynamic behavior of the AI engine during execution was not runtime-profiled as we only have read-only access and `run_command` is disabled due to environment-level timeout constraints.
*   We assume that the SQLite database `expansions/cards.cdb` is accurate and complete, containing all cards.

## 4. Conclusion
*   There are no card IDs missing in the active deck-specific JSON registries for the 4 bricked decks.
*   The sandbox registries match the live registries in card coverage, but retain trained RL `q_values` which are blanked out in the live versions.
*   `auto_role_detector.py` suffers from a significant limitation: it fails to read or assign roles for Extra Deck and Side Deck cards because `shared_utils.load_ydk_main_deck` ignores non-main deck card sections.
*   Choke point configurations for Goldlord and Labrynth contain obsolete card IDs (`95825075` and `23440079` respectively) that are not present in their YDK decks. Kwtune has no choke points defined.

## 5. Verification Method
*   Inspect `WindBot_Sandbox/shared_utils.py` lines 106–140 to verify that the parser stops reading card IDs when reaching `#extra` or `!side`.
*   Inspect `WindBot/config/decks/2026_Goldlord.json` and match card `95825075` against the list in `WindBot/Decks/2026_Goldlord.ydk` to verify it is missing.
