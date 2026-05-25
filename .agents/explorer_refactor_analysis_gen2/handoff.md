# Handoff Report

## 1. Observation

- **`BaseCustomExecutor.cs` (OnSelectCard)**:
  Line 3044: `public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)` handles sorting cards by priority.
- **`DreadnoughtExecutor.cs` (Executor Registrations)**:
  Line 25: `AddExecutor(ExecutorType.Activate, 14558127, AshBlossomEffect);`
- **`InvokeExecutor.cs` (Fusion Material Selection)**:
  Line 482: `if (hint == HintMsg_FusionMaterial)` handles filtering Aleister and other light/dark elements.
- **`c101305030.lua` (Invoked Sorath)**:
  Line 7: `--Fusion Materials: 1 "Aleister" monster + 1 FIRE or WIND monster`
  Line 8: `Fusion.AddProcMix(c,true,true,aux.FilterBoolFunctionEx(Card.IsSetCard,SET_ALEISTER),aux.FilterBoolFunctionEx(Card.IsAttribute,ATTRIBUTE_FIRE|ATTRIBUTE_WIND))`
- **`c101305031.lua` (Invoked Babalon)**:
  Line 7: `--Fusion Materials: 1 "Aleister" monster + 1 LIGHT or EARTH monster`
  Line 8: `Fusion.AddProcMix(c,true,true,aux.FilterBoolFunctionEx(Card.IsSetCard,SET_ALEISTER),aux.FilterBoolFunctionEx(Card.IsAttribute,ATTRIBUTE_LIGHT|ATTRIBUTE_EARTH))`
- **`c101305032.lua` (Invoked Okeanos)**:
  Line 7: `--Fusion Naterials: 1 "Aleister" monster + 1 DARK or WATER monster`
  Line 8: `Fusion.AddProcMix(c,true,true,aux.FilterBoolFunctionEx(Card.IsSetCard,SET_ALEISTER),aux.FilterBoolFunctionEx(Card.IsAttribute,ATTRIBUTE_DARK|ATTRIBUTE_WATER))`
- **`c101305033.lua` (Invoked Transcendence Aeon)**:
  Line 7: `--Fusion Materials: 2+ Fusion Monsters with different Attributes`
  Line 8: `Fusion.AddProcMixRep(c,true,true,s.matfilter,2,99)`
- **`ORIGINAL_REQUEST.md` (Dreadnought fusion materials)**:
  Line 100: `Ensure 2 Level 5+ Destiny HERO monsters are selected.`

---

## 2. Logic Chain

1. **R1/R2**: Because `AddExecutor` registers a parameterless callback (`Func<bool>`), we can wrap this in a new `OnCardAction` overload by providing the cardId, executor type, and the original callback delegate inside a lambda. This allows the base executor to log the decision, evaluate card-specific safeguards, and mark `_ourCardsPlayed` in a unified manner.
2. **R3 (Transitions)**: Because `save_outcomes_to_sql.py` parses `decisions.jsonl` sequentially, a transition marker or board state comparison (like LP or hand size reset) is needed to reliably partition games when turn counts restart.
3. **R3 (Concurrency)**: Because multiple parallel instances of WindBot write outcomes simultaneously, SQLite connections will raise database locks. Specifying WAL mode (`journal_mode = WAL`) and wrapping execute/commit blocks in a retry loop with exponential backoff and jitter is the standard way to resolve SQLite write concurrency issues.
4. **R4**: Because the C# game engine has direct access to the files and process start APIs, when `MonitorLP` detects `LP == 0`, we can sync the registries using `File.Copy` and launch `compile_ai.bat` headlessly using C# `System.Diagnostics.Process` in a non-blocking background thread.
5. **R5**: Because fusion material selection (`HintMsg_FusionMaterial`) is requested in a separate engine callback from the fusion summon target selection (`HintMsg_SpSummon`), storing the target ID in a state variable `_lastSelectedFusionId` allows the material selection routine to look up the exact fusion recipe, score materials based on strategic resource values (e.g. prioritizing Malicious, Denier, and Servant over handtraps), and validate combinations to prevent illegal selections and crashes.

---

## 3. Caveats

- We assume the base executor is single-threaded per duel instance; however, concurrent learning/writing uses a global lock (`_staticLock`) to avoid overlapping registry updates.
- If multiple fusion spells are chained or resolved in an unexpected sequence, `_lastSelectedFusionId` must be cleared or managed to prevent matching wrong recipes.

---

## 4. Conclusion

The system analysis is complete and concrete designs for requirements R1-R5 are ready. The next agent (implementer) should proceed directly with implementing these modifications inside `BaseCustomExecutor.cs`, `DreadnoughtExecutor.cs`, `InvokeExecutor.cs`, and `save_outcomes_to_sql.py` as detailed in `analysis.md`.

---

## 5. Verification Method

- **Compilation**: Run `compile_ai.bat` in a Windows Command Prompt/PowerShell to verify successful compilation without errors.
- **Unit/Simulation test**: Execute a test duel simulation using the custom decks `2026_Dreadnought` and `2026_Invoke` to trigger fusion summons, turn resets, and verify the correct database records are populated in `statistics.db` under write concurrency.
