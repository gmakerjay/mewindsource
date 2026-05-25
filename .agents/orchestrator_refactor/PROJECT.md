# Project: EDOTh WindBot Refactor & Enhancement
# Scope: EDOTh WindBot System

## Architecture
- `BaseCustomExecutor.cs`: Core C# executor base class. Controls registry updates, decision logging, LP monitor, learning heuristic.
- `DreadnoughtExecutor.cs`, `InvokeExecutor.cs`: Custom deck-specific executors.
- `save_outcomes_to_sql.py`: Python script importing outcomes and decision logs to SQLite database.
- `cockpit.py`: WindBot training manager.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | C# Core Refactor | Overload OnCardAction in BaseCustomExecutor, wrap executor callbacks in Dreadnought & Invoke | None | DONE |
| 2 | Python Importer Fix | Split turn restarts, add concurrency retries in save_outcomes_to_sql.py | None | DONE |
| 3 | Auto-Deployment Pipeline | Sync JSON registries, compile executor headlessly on LP=0 | M1 | DONE |
| 4 | Fusion Stability Fix | Store lastSelectedFusionId, validate materials (DPE, Dreadnought, Dystopia, Dangerous, Trinity, Chaos, Invoked) in OnSelectCard | M1 | DONE |
| 5 | Integrated E2E Verification | Validate compile, run training, check SQLite database & Q-values | M1, M2, M3, M4 | DONE |

## Interface Contracts
### BaseCustomExecutor ↔ Custom Executors
- `OnCardAction(int cardId, ExecutorType type, Func<bool> condition)`: Overloaded decision hook. First evaluates condition delegate, then calls EvaluateCardAction, then updates `_ourCardsPlayed` and logs.
- `_lastSelectedFusionId`: Private field in executor classes.
