# Project: EDOTh WindBot System Refactoring & Enhancements

## Architecture
- **WindBot Execution Environment (C#)**: Custom executors (`DreadnoughtExecutor.cs`, `InvokeExecutor.cs`, and `PureYummyExecutor.cs` inheriting from `BaseCustomExecutor.cs`) interact with OCGCore using C# events.
- **Data logging and DB pipeline (Python)**: Bot execution records matches/decisions to `decisions.jsonl` in logs. The python script `save_outcomes_to_sql.py` parses these and imports into SQLite (`statistics.db`).
- **Cockpit Training Controller (Python/HTML)**: Controls simulation runs, monitors state, syncs configurations, and recompiles dlls dynamically on completion.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Explore & Codebase Audit | Analyze requirements, identify target file lines, verify environment setup. | None | DONE |
| 2 | Fix Direct Attack Replay Crash | Update `BaseCustomExecutor.cs` battle selection checks to prevent direct attacks when opponent has monsters. | M1 | DONE |
| 3 | Robust Fusion Material Selection | Refactor `DreadnoughtExecutor` and `InvokeExecutor` to match material combos against valid recipes if target fusion ID is lost/bypassed, and reset selected ID. | M1 | DONE |
| 4 | Safe DB Writes & Partitioning | Implement game restart split and WAL transaction retries in `save_outcomes_to_sql.py`. | M1 | DONE |
| 5 | Automatic Brain Deployment | Implement LP=0 registry sync and headless dll compilation in C# executors and `cockpit.py`. | M1 | DONE |
| 6 | System verification and Pipeline Testing | Run compiler, build tests, run python tests to verify whole integration. | M2, M3, M4, M5 | DONE |

## Interface Contracts
### C# Executors ↔ OCGCore
- `OnSelectAttackTarget`: returns the index of the selected target. Must not declare direct attack (return -1 or similar invalid defender representation) if opponent has monsters.
- `OnSelectCard`: returns the indices of chosen cards for summoning or fusion material.
- `GetOptimalFusionMaterials`: takes target fusion ID (if any) and a list of cards, returns the sub-list of optimal fusion materials.
