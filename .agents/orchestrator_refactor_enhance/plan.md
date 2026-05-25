# Implementation Plan: EDOTh WindBot System Refactoring & Enhancements

## Architecture & Scope
The EDOTh WindBot system contains an AI engine (C# executors under `WindBot/`), a database synchronization layer (Python script `Developer/scratch/save_outcomes_to_sql.py` importing decision logs into `statistics.db`), and a control dashboard/training launcher (`Developer/WindBot_Sandbox/cockpit.py`).

Our mission is to address issues across all three layers to improve stability, robust fusion summoning, concurrent database operations, and automatic brain deployment.

## Milestones & Decomposition

| Milestone | Name | Objective | Target Files | Status |
|-----------|------|-----------|--------------|--------|
| M1 | Analyze & Explore | Investigate the current state of the codebase to identify exact locations for direct attack check, fusion recipe matching, database concurrency, and registry synchronization. | All targets | PLANNED |
| M2 | Fix Direct Attack Replay Crash | Remove premature direct attack checks and restrict direct attacks in `BaseCustomExecutor.cs` when opponent monsters exist. | `WindBot/BaseCustomExecutor.cs` | PLANNED |
| M3 | Robust Fusion Material Selection | Refactor `GetOptimalFusionMaterials` in custom executors. Fallback to matching against all valid recipes if `_lastSelectedFusionId` is invalid, and reset `_lastSelectedFusionId` to 0. | `WindBot/DreadnoughtExecutor.cs`, `WindBot/InvokeExecutor.cs` | PLANNED |
| M4 | Safe DB Writes & Partitioning | Detect turn resets to partition matches in `decisions.jsonl`. Wrap SQLite writes in transaction retry loops with timeouts in `save_outcomes_to_sql.py`. | `Developer/scratch/save_outcomes_to_sql.py` | PLANNED |
| M5 | Automatic Brain Deployment | Update the C# executor shutdown hook and `cockpit.py` to sync JSON registries and recompile `UnifiedIgnisExecutor.dll` automatically on LP = 0. | `WindBot/...`, `Developer/WindBot_Sandbox/cockpit.py` | PLANNED |
| M6 | Verification & Pipeline Testing | Compile the project and run verification tests (e.g., pipeline test, simulated matches) to ensure zero OCGCore crashes and successful learning. | All targets | PLANNED |

## Detailed Steps & Verification Criteria

### Milestone 1: Analyze & Explore
- **Task**: Spawn 3 Explorer agents in parallel or sequentially to explore:
  - Explorer A: BaseCustomExecutor.cs (attack logic) and DreadnoughtExecutor/InvokeExecutor (fusion logic).
  - Explorer B: save_outcomes_to_sql.py (database write locks, turn transitions, partitioning).
  - Explorer C: C# executor shutdown hooks & cockpit.py registry sync and compilation integration.
- **Verification**: Handoff analysis report outlining exact lines, structures, and recommended changes.

### Milestone 2: Fix Direct Attack Replay Crash
- **Task**: Spawn a Worker to modify `BaseCustomExecutor.cs`:
  - Locate `OnSelectAttackTarget`.
  - Remove direct attack check before defender evaluation (around lines 3145-3149).
  - Force direct attacks only if the opponent's monster list is empty or null.
- **Verification**: Spawn Reviewer to check correctness. Compile DLL and verify no compiler errors.

### Milestone 3: Robust Fusion Material Selection
- **Task**: Spawn a Worker to modify `DreadnoughtExecutor.cs` and `InvokeExecutor.cs`:
  - Declare `private int _lastSelectedFusionId = 0;`.
  - Update `OnSelectCard` to intercept `HintMsg_SpSummon` (509) to store fusion ID.
  - Implement/refactor `GetOptimalFusionMaterials` to fallback to checking all valid recipes of the deck when `_lastSelectedFusionId == 0` or is unrecognized.
  - Reset `_lastSelectedFusionId = 0` upon successful material selection.
- **Verification**: Spawn Reviewer and Challenger to verify fusion summons run without crashes/resets and select valid materials.

### Milestone 4: Safe DB Writes & Partitioning
- **Task**: Spawn a Worker to modify `save_outcomes_to_sql.py`:
  - Implement restart detection (e.g. Turn 1 restart, health reset) to split matches correctly in `decisions.jsonl`.
  - Wrap SQLite writes in transaction retry loops with timeouts to handle parallel training.
- **Verification**: Run `verify_pipeline.py` and parallel training test cases. Ensure no write-locks are raised.

### Milestone 5: Automatic Brain Deployment
- **Task**: Spawn a Worker to implement shutdown hooks and update `cockpit.py`:
  - C# shutdown hook: detect LP = 0 on either side, trigger registry synchronization, and run auto-compilation of DLL.
  - `cockpit.py`: implement auto-compilation execution and configuration deployment.
- **Verification**: Verify that when match ends at LP = 0, the JSON files are synced and DLL is rebuilt automatically.

### Milestone 6: Verification & Pipeline Testing
- **Task**: Execute compile and run pipeline tests. Ensure all acceptance criteria are met.
