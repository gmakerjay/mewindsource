# Original User Request

## Initial Request — 2026-05-25T09:03:02Z

Create and implement the `2026_Dreadnought` AI deck in the EDOTh WindBot system, incorporating C# executor logic, JSON card registry, bots registration, and verification of combo flow correctness.

Working directory: c:\Users\admin\Documents\EDOTh
Integrity mode: development

## Reference Material
- Docs/2026_dreadnought_deck_analysis.md
- Docs/Rules.md
- Docs/SKILL.md

## Requirements

### R1. C# Executor Implementation & Safeguards
Implement C# logic for `2026_Dreadnought` (either in a new file `DreadnoughtExecutor.cs` or within `UnifiedIgnisExecutor.cs`) representing the Destiny HERO Destiny combo lines. The implementation must include safeguards and priority behaviors for:
- `Destiny HERO - Doom Liege` (ID: `101402022`): Cost-send D-HERO to GY -> search field spell `101402062`.
- `Clock Tower Prison City - Dark City` (ID: `101402062`): Active turn search, and trigger special summon on destruction.
- `Destiny HERO - Dreadnought Servant` (ID: `101402023`): Special summon from hand -> destroy field spell -> search `Polymerization`.
- `Destiny HERO - Dreadnought` (ID: `101402037`): Alternative summon by sending `Dreadmaster` to GY -> search 2 cards on summon.
- `Destiny HERO - Death Dogma` (ID: `101402021`): GY banish summon, burn damage, and Quick Fusion chain reaction.
- Supporting cards: `D - Burst` (ID: `100456010`), `Masked HERO Dusk Crow` (`10808715`), `Masked HERO Furnace` (`58288218`), and `Masked HERO Fountain` (`66206748`).

The C# project must compile successfully using `compile_ai.bat`.
You must follow all coding principles and safeguards described in Rules.md and SKILL.md. Do not violate the 8 Iron Rules.

### R2. Configuration & Bot Registration
- Register the new bot in bots.json with name `"2026_Dreadnought"`, deck `"2026_Dreadnought"`, difficulty `3`, and masterRules `[5]`.
- Create a playstyle deck config file at 2026_Dreadnought.json with playstyle set to `"combo"`.
- Generate and populate the cards registry cards_registry_2026_Dreadnought.json and its sandbox counterpart, categorizing roles (starter, extender, handtrap, etc.) for all deck cards, capping heuristic priority at 8.

### R3. Pipeline Training & Performance Verification
Run multi-instance training rounds with EDOTh bot-vs-bot simulation to collect decision logs into `statistics.db` and train Q-values in registries. Validate that the Q-learning pipeline successfully updates Q-values for Dreadnought cards, achieving optimized play. The bot should achieve a high win rate (aiming for 90%+) against standard opponents in training.

## Verification Resources
The implementing team can use:
- compile_ai.bat to verify C# build.
- verify_pipeline.py or run_multi_iterations.py to run simulated games and check Q-value progression.

## Acceptance Criteria

### Compilation & System Registration
- [ ] The C# project builds successfully with compile_ai.bat without compiler errors or warnings.
- [ ] The bot `"2026_Dreadnought"` is active and listed in bots.json and Cockpit deck lists.
- [ ] The configuration files cards_registry_2026_Dreadnought.json and config/decks/2026_Dreadnought.json exist with valid structure.

### Combo & Learning Verification
- [ ] Running a simulated match of 2026_Dreadnought vs opponent populates statistics.db and generates decisions logs.
- [ ] The registry updates card weights based on match outcomes.
- [ ] Card Q-values show positive/negative learning progression representing combo efficiency.

## Follow-up — 2026-05-25T09:29:17Z

UPDATE CONSTRAINTS FROM USER:
1. Stop running simulated matches or EDOPro duels. Skip Milestone 3 (pipeline training via bot-vs-bot simulation). Do not attempt to run headless EDOPro duels as they require GUI.
2. Focus purely on writing the Dreadnought deck, creating DreadnoughtExecutor.cs, registering it in bots.json, and setting up the optimized card registry weights (Heuristics) in `cards_registry_2026_Dreadnought.json` and its sandbox counterpart.
3. Verify that the C# project compiles successfully using `compile_ai.bat`.
4. Provide the final verification report showing C# compilation success and card registry values.

## Follow-up — 2026-05-25T09:34:40Z

Please restore and copy all C# source files (`BaseCustomExecutor.cs`, `UnifiedIgnisExecutor.cs`, `PureYummyExecutor.cs`, `InvokeExecutor.cs`, and `DreadnoughtExecutor.cs`) and `compile_ai.bat` back from `Developer/WindBot_Sources/` to the live `WindBot/` directory. Make sure the `compile_ai.bat` inside `WindBot/` compiles locally and outputs the DLL to `Executors/UnifiedIgnisExecutor.dll`. Run `compile_ai.bat` in `WindBot/` to verify compilation succeeds in the live directory.

## Follow-up — 2026-05-25T12:59:28Z

Audit, refactor, and enhance the EDOTh WindBot system: correct the "placebo" learning defect, resolve SQL partitioning/concurrency issues during multi-port training, implement auto-deployment of Brain on LP = 0, and fix the fusion material selection crash in Dreadnought and Invoke decks.

Working directory: c:\Users\admin\Documents\EDOTh
Integrity mode: development

## Requirements

### R1. Overload OnCardAction in BaseCustomExecutor.cs
Implement an overloaded `OnCardAction(int cardId, ExecutorType type, Func<bool> condition)` in BaseCustomExecutor.cs that:
1. First evaluates the condition delegate (if provided). If false, returns false immediately.
2. Otherwise, executes the `EvaluateCardAction` logic to log the decision, calculate weights, and return the decision.
3. Updates `_ourCardsPlayed` with the played card IDs.

### R2. Refactor Custom Executors to Wrap Callbacks
Modify DreadnoughtExecutor.cs and InvokeExecutor.cs to wrap all specific executor callbacks (like `AshBlossomEffect`, `DoomLiegeEffect`, `AleisterSummonEffect`) in the overloaded `OnCardAction` delegate, ensuring they are logged and trained in the Q-learning loop.

### R3. Fix Decisions Partitioning & Concurrency in Python Importer
Modify save_outcomes_to_sql.py to:
1. Correctly split games in `decisions.jsonl` when turn restarts (even if ending on turn 1).
2. Wrap SQLite database writes with concurrency retries/timeouts (WAL mode is already enabled, but retry logic is needed) to support concurrent multi-instance training from different ports.

### R4. Automatic Brain Deployment and Compiling on LP = 0
Enhance the training system (C# executor shutdown hook and cockpit.py) so that once a match finishes and LP reaches 0:
1. The updated JSON card registries are synced between the live directory `WindBot/config/` and sandbox `WindBot_Sandbox/` directory.
2. An automatic trigger deploys the configuration and executes `compile_ai.bat` to rebuild `UnifiedIgnisExecutor.dll` headlessly without human intervention.

### R5. Fix Fusion Material Selection Crash
Resolve the OCGCore protocol crash caused by selecting invalid material combinations:
1. Declare a private field `private int _lastSelectedFusionId = 0;` in both DreadnoughtExecutor.cs and InvokeExecutor.cs.
2. Intercept `HintMsg_SpSummon` (509) in `OnSelectCard` to store the target Fusion Monster ID in `_lastSelectedFusionId`.
3. Intercept `HintMsg_FusionMaterial` (511) in `OnSelectCard` and implement a strict material validation method (`GetOptimalFusionMaterials` with priority scoring) matching the target fusion's requirements:
   - **DPE (60461804):** Ensure 1 Level 6+ HERO and 1 Destiny HERO are selected.
   - **Dreadnought (101402037):** Ensure 2 Level 5+ Destiny HERO monsters are selected.
   - **Dystopia (90579153):** Ensure 2 Destiny HERO monsters are selected.
   - **Dangerous (30757127):** Ensure 1 Destiny HERO and 1 DARK Effect monster are selected.
   - **Trinity (46759931):** Ensure 3 HERO monsters are selected.
   - **Contrast HERO Chaos (23204029):** Ensure 2 Masked HERO monsters are selected.
   - **Invoked Monsters:** Ensure 1 Aleister (86120751 or 101305015) and 1 monster of the correct attribute/level (LIGHT for Mechaba, FIRE for Purgatrio, level 10 for Transcendence Aeon, etc.) are selected.
4. Rank valid combinations using a custom priority method (favoring `Malicious`, `Denier`, and `Servant` to optimize graveyard setup) and return the optimal valid material list.

## Acceptance Criteria

### Learning and Logging Correction
- [ ] Running a match of `2026_Dreadnought` logs decisions for core cards (like `Doom Liege` ID `101402022`) in `decisions.jsonl` and SQL `decisions` table.
- [ ] Card registry Q-values (`q_values`) for core cards in `cards_registry_2026_Dreadnought.json` update correctly with trained values (not empty `{}`).

### Concurrency and Robustness
- [ ] Running parallel duels on multiple ports writes to `statistics.db` without locking errors.
- [ ] Game decisions are split correctly in database matches even if a game ends on turn 1.

### Automatic Deployment
- [ ] When a match ends (LP of either side reaches 0), the system automatically syncs JSON registries and recompiles `UnifiedIgnisExecutor.dll` successfully.

### Fusion Summon Stability
- [ ] Activating `Fusion Destiny` or `Invocation` selects only valid materials and executes to completion without causing OCGCore protocol crashes or sudden duel resets.

## Follow-up — 2026-05-25T14:02:05Z

Audit, refactor, and enhance the EDOTh WindBot system to fix the direct attack replay crash, make fusion material selection robust against stale/bypassed IDs, and ensure database stability during parallel training.

Working directory: c:\Users\admin\Documents\EDOTh
Integrity mode: development

## Requirements

### R1. Fix Direct Attack Replay Crash
In BaseCustomExecutor.cs, modify OnSelectAttackTarget to resolve the direct attack crash during replays:
1. Remove the direct attack check that executes before evaluating defenders (lines 3145-3149).
2. Ensure that direct attacks are only declared if defenders list is empty or null, preventing illegal direct attack declarations when the opponent has monsters.

### R2. Robust Fusion Material Selection & Recipe Matching
Refactor GetOptimalFusionMaterials in DreadnoughtExecutor.cs and InvokeExecutor.cs:
1. If _lastSelectedFusionId is 0 or does not match any known fusion recipe for the deck, match the material combination against all valid fusion recipes of the deck.
2. Once fusion materials are successfully selected, reset _lastSelectedFusionId to 0 to prevent stale ID pollution in subsequent turns.

### R3. Safe Database Writes & Outcomes Partitioning
Ensure save_outcomes_to_sql.py correctly splits game records on turn resets and handles concurrent database writes to statistics.db during multi-port training:
1. Detect game restarts to correctly partition matches in decisions.jsonl.
2. Wrap SQLite database writes in transaction retry loops with timeouts to prevent locking/concurrency errors.

### R4. Automatic Brain Deployment
Enhance the C# executor shutdown hook and cockpit.py to sync JSON registries and recompile UnifiedIgnisExecutor.dll automatically once a match finishes and LP reaches 0 on either side.

## Acceptance Criteria

### Battle & Fusion Stability
- Running a match of 2026_Dreadnought or 2026_Invoke completes without any OCGCore protocol crashes or sudden duel resets.
- Invoking fusion spells (e.g. Fusion Destiny, Invocation) selects valid materials and resolves to completion even when the Extra Deck target selection prompt is bypassed.
- Replay events in the Battle Phase (e.g., when the opponent summons tokens during attack declaration) resolve correctly without triggering invalid direct attacks.

### Training & Deployment
- Duel decisions are logged correctly in decisions.jsonl and successfully written to SQL database.
- Recompilation of UnifiedIgnisExecutor.dll compiles successfully with no syntax errors.
