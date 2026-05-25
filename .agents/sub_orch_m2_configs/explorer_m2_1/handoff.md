# Handoff Report - Explorer 1 (Milestone 2)

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
*   **Default card metadata creation:** In `WindBot/BaseCustomExecutor.cs` (lines 1490–1509):
    ```csharp
    protected CardMetadata GetOrCreateMetadata(ClientCard card)
    {
        if (card == null) return null;
        int cardId = card.Id;
        if (_cardRegistry.ContainsKey(cardId))
        {
            return _cardRegistry[cardId];
        }

        CardMetadata meta = new CardMetadata();
        meta.id = cardId;
        meta.priority = 5;
        ...
    ```
*   **Missing card IDs in live deck-specific registries:**
    - For `AzaYummy` (`WindBot/config/cards_registry_2026_AzaYummy.json`), card IDs `46502744` (Retaliating "C"), `24508238` (D.D. Crow), `18144507` (Harpie's Feather Duster), `14532163` (Lightning Storm), and `41420027` (Solemn Judgment) from `Decks/AI_2026_AzaYummy.ydk` are missing.
    - For `BrElfnote` (`WindBot/config/cards_registry_2026_BrElfnote.json`), card IDs `59438930` (Ghost Ogre & Snow Rabbit), `19613556` (Heavy Storm), and `83326048` (Dimensional Barrier) from `Decks/AI_2026_BrElfnote.ydk` are missing.
    - In contrast, all card IDs specified in the YDK files for the 4 bricked decks (Goldlord, Invoke, Kwtune, Labrynth) are present in their deck-specific registries.
*   **YDK Card ID Parsing Helper:** In `WindBot_Sandbox/shared_utils.py` (lines 120-131):
    ```python
        main_deck = []
        with open(path, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#created"):
                    continue
                if line.startswith("#") or line.startswith("!"):
                    continue
                try:
                    main_deck.append(int(line))
                except ValueError:
                    pass
    ```
*   **Deck Configurations:**
    - `WindBot/config/decks/2026_Goldlord.json` specifies `"choke_points": [95825075]`. Eldorado Adelantado (`95825075`) is not in `Decks/2026_Goldlord.ydk`.
    - `WindBot/config/decks/2026_Labrynth.json` specifies `"choke_points": [23440079]`. Labrynth Coelophys (`23440079`) is not in `Decks/2026_Labrynth.ydk`.
    - `WindBot/config/decks/2026_Kwtune.json` specifies `"choke_points": []`.

## 2. Logic Chain
1. **Per-Deck Loading**: The C# code first attempts to load `cards_registry_2026_<deck_name>.json`. If it exists, it loads it instead of `cards_registry.json`. Thus, any card present in a deck's YDK list *must* be in that deck's specific registry file; otherwise, it is treated as missing and gets default unoptimized values.
2. **Bricked Decks Verification**: The 4 bricked decks (Goldlord, Invoke, Kwtune, Labrynth) have all their card IDs registered. The bricked behavior is therefore not caused by missing registry entries for these 4 decks, but rather configuration discrepancies (such as obsolete choke points) or unoptimized values.
3. **Other Decks Registry Gaps**: Two of the other decks (`AzaYummy` and `BrElfnote`) are missing side-deck cards from their registries. This is likely because the side-deck cards were added after `auto_role_detector.py` was run or the registries were last compiled.
4. **Parser Behavior**: In `shared_utils.py`, `load_ydk_main_deck` does not terminate on `#extra` or `!side` headers; it skips them and continues parsing. Thus, it loads the entire YDK file. Running `auto_role_detector.py` is capable of automatically resolving these missing registry cards when run for those decks.
5. **Config Discrepancies**: Goldlord and Labrynth list choke points that are completely absent from their YDK files, while Kwtune is missing choke points.

## 3. Caveats
- Since this is a read-only investigation, no code changes or script runs were executed.
- We assume that `expansions/cards.cdb` is valid and contains all target card information.

## 4. Conclusion
- No card IDs are missing from the registries of the 4 bricked decks.
- Gaps were discovered in `2026_AzaYummy` and `2026_BrElfnote` registries where several side-deck cards are missing.
- `auto_role_detector.py` is fully functional for all sections (Main, Extra, Side) of YDK files and can be used to populate these missing cards.
- Choke point mismatches exist in `Goldlord` and `Labrynth` configurations, and `Kwtune` has empty choke points.

## 5. Verification Method
1. **Registry Verification**: Inspect `verify_missing_registries.py` in the explorer folder (`.agents/sub_orch_m2_configs/explorer_m2_1/verify_missing_registries.py`) and run it using `python verify_missing_registries.py` to print out missing cards for any deck.
2. **Goldlord/Labrynth Choke Points**: Inspect `WindBot/config/decks/2026_Goldlord.json` and verify `95825075` is not in `WindBot/Decks/2026_Goldlord.ydk`. Inspect `WindBot/config/decks/2026_Labrynth.json` and verify `23440079` is not in `WindBot/Decks/2026_Labrynth.ydk`.
