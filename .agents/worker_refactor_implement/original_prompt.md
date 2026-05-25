## 2026-05-25T13:09:28Z
You are the Refactor Worker subagent.
Your identity is teamwork_preview_worker.
Your working directory is: c:\Users\admin\Documents\EDOTh\.agents\worker_refactor_implement\

Please implement the refactoring, enhancements, and stability fixes for the EDOTh WindBot system as detailed below.

### MANDATORY INTEGRITY WARNING (ZERO TOLERANCE)
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

---

### Implementation Instructions:

#### R1 & R2: OnCardAction Overload and Executor Callback Wrapping
1. **In `WindBot/BaseCustomExecutor.cs`**:
   - Add a new overloaded method `public bool OnCardAction(int cardId, ExecutorType type, Func<bool> condition)`:
     - If `condition` is `null`, return `false`.
     - First execute `condition()`. If it returns `false`, return `false` immediately.
     - Look up the `ClientCard` matching `cardId` in hand, monster zone, spell zone, or graveyard. If not found, use `Card` if `Card.Id == cardId`.
     - If the card is found:
       - Get or create its `CardMetadata` using `GetOrCreateMetadata(card)`.
       - Call `EvaluateCardAction(card, meta, type)`. If it returns `true`, ensure `cardId` is added to `_ourCardsPlayed` (using `lock(_staticLock)` to be safe), and return `true`. Otherwise return `false`.
       - If the card is NOT found (fallback), return the result of `condition()`.
2. **In `WindBot/DreadnoughtExecutor.cs` and `WindBot/InvokeExecutor.cs`**:
   - Wrap all deck-specific callbacks registered via `AddExecutor` in the overloaded `OnCardAction`.
   - For example:
     - Ash Blossom:
       `AddExecutor(ExecutorType.Activate, 14558127, () => OnCardAction(14558127, ExecutorType.Activate, AshBlossomEffect));`
     - Doom Liege (Banish/Search):
       `AddExecutor(ExecutorType.Activate, 101402022, () => OnCardAction(101402022, ExecutorType.Activate, DoomLiegeEffect));`
     - Aleister Summon:
       `AddExecutor(ExecutorType.Summon, 86120751, () => OnCardAction(86120751, ExecutorType.Summon, AleisterSummonEffect));`
     - Keep standard fallbacks (like `AddExecutor(ExecutorType.Activate, OnDefaultActivate);`) as they are.

#### R3: Fix Decisions Partitioning & Concurrency in save_outcomes_to_sql.py
1. **In `Developer/scratch/save_outcomes_to_sql.py`**:
   - Improve the turn transition logic to detect restarts even when a game starts and ends on turn 1:
     - Standard: `turn < last_turn`.
     - Robust heuristics: Detect reset when LP resets to 8000 for both players (`lp_self == 8000` and `lp_opp == 8000`) and the previous record had different LP or had cards in monster/spell zones, OR when turn is 1 and the previous record had a larger turn number.
   - Wrap SQLite database operations (such as insert and commit blocks) with concurrency retry logic:
     - Use SQLite WAL mode (`PRAGMA journal_mode = WAL;`) and enable foreign keys.
     - Implement exponential backoff with randomized jitter on `sqlite3.OperationalError` (e.g. up to 10 retries, start with 0.1s delay, sleep `delay * (2.0 ** attempt) + random.uniform(0, 0.05)`).

#### R4: Automatic Brain Deployment and Compiling on LP = 0
1. **In `WindBot/BaseCustomExecutor.cs`**:
   - Create a method `protected void SyncRegistryToSandboxAndCompile()`:
     - Extract `_resolvedDeckName`. The cards registry name is `cards_registry_" + _resolvedDeckName + ".json`.
     - Copy the updated registry file from `WindBot/config/` to `Developer/WindBot_Sandbox/` (replacing the existing file there).
     - Copy `WindBot/config/opponent_memory.json` to `Developer/WindBot_Sandbox/opponent_memory.json`.
     - Headlessly execute `compile_ai.bat` from `WindBot/compile_ai.bat` using `System.Diagnostics.Process` in a background non-blocking manner.
   - Call this method at the end of `SaveConfiguration()`. This ensures that whenever the duel ends (LP reaches 0) and configuration is updated, the changes are automatically synchronized back to the sandbox and recompilation is triggered.

#### R5: Fix Fusion Material Selection Crash
1. **In `WindBot/DreadnoughtExecutor.cs` and `WindBot/InvokeExecutor.cs`**:
   - Define a private/protected field: `protected int _lastSelectedFusionId = 0;` (you can declare it in `BaseCustomExecutor.cs` or in both executor classes).
   - In `OnSelectCard`:
     - Intercept `hint == HintMsg_SpSummon` (509) to store the target Fusion Monster ID in `_lastSelectedFusionId`. You can intercept the return value of `OnSelectCard` or `base.OnSelectCard` when `hint == HintMsg_SpSummon` to capture whichever ID is selected.
     - Intercept `hint == HintMsg_FusionMaterial` (511) and implement a strict material validation method `GetOptimalFusionMaterials` with priority scoring matching the target fusion's requirements:
       - **DPE (60461804):** Ensure exactly 1 Level 6+ HERO and 1 Destiny HERO are selected.
       - **Dreadnought (101402037):** Ensure exactly 2 Level 5+ Destiny HERO monsters are selected.
       - **Dystopia (90579153):** Ensure exactly 2 Destiny HERO monsters are selected.
       - **Dangerous (30757127):** Ensure exactly 1 Destiny HERO and 1 DARK Effect monster are selected.
       - **Trinity (46759931):** Ensure exactly 3 HERO monsters are selected.
       - **Contrast HERO Chaos (23204029):** Ensure exactly 2 Masked HERO monsters are selected.
       - **Invoked Monsters:** Ensure exactly 1 Aleister (86120751 or 101305015) and 1 monster of the correct attribute/level:
         - Mechaba (75286621): LIGHT
         - Purgatrio (13529466): FIRE
         - Sorath (101305030): FIRE or WIND
         - Babalon (101305031): LIGHT or EARTH
         - Okeanos (101305032): DARK or WATER
         - Caliga (97973962): DARK
         - Raidjin (49513164): WIND
         - Magellanica (23656668): EARTH
         - Augoeides (38423248): 1 Fusion monster
         - Elysium (12307878): 1 Invoked monster + 1 Extra Deck Summoned monster
         - Transcendence Aeon (101305033): 2+ Fusion Monsters with different Attributes
     - **Scoring Prioritization**:
       - Favor recycling/using materials from Graveyard or hand that are `Malicious` (9411399), `Denier` (16605586), and `Servant` (101402023).
       - Banish/use `Aleister` from GY/field if possible.
       - Penalize using handtraps/staples (Ash, Veiler, Impermanence, Called by the Grave) from hand.
       - Penalize using `Virakam` (101305017) to keep its negate active.
       - Find all possible combinations of size `min` from `cards`. Filter those that satisfy the target recipe. Calculate a score for each valid combination based on the cards' priorities. Choose the combination with the highest score. If none satisfies the recipe, fall back to sorting by priority and selecting top `min` cards.

---

### Verification:
1. Run `compile_ai.bat` inside the `WindBot` directory to verify that the C# files compile successfully and output `Executors/UnifiedIgnisExecutor.dll`.
2. Document the compilation results and changes in your handoff report.

Write `changes.md` and `handoff.md` in your working directory and message your parent conversation (ID: caa92013-e2fd-4b40-8e51-3362e33e2a91) with the paths to these files.
