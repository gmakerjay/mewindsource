# Project Plan: IGNIS WindBot System Update

## Architecture
- **C# AI Engine** (`UnifiedIgnisExecutor.cs`, `BaseCustomExecutor.cs`): Decisions, scoring, battle AI, and hook management. Writes `match_summary.log` and `decisions.jsonl` in local match directories.
- **Python Sandbox** (`WindBot_Sandbox/`): Runs learning processes, detects card roles, runs A/B tests, and manages the dashboard.
- **Database Layer** (`statistics.db`): SQLite database capturing match and decision logs populated by `save_outcomes_to_sql.py`.
- **Registry / Config** (`WindBot/config/`): JSON files defining card priorities (registries) and playstyles (deck configs).

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | C# Hooks & Safeguards Audit | Audit lifecycle hooks, safeguards, process exit, and LP completion checks in `UnifiedIgnisExecutor.cs` and `BaseCustomExecutor.cs`. | None | DONE |
| 2 | Registries & Deck Configs | Populate bricked registries (Goldlord, Invoke, Kwtune, Labrynth) and create JSON deck configs for all 10 decks under `WindBot/config/decks/`. | None | IN_PROGRESS (Conv: bb7dcb26-dc23-4fca-91fd-bb97ea430319) |
| 3 | Concurrency & Safe Learning | Make SQLite writes in `save_outcomes_to_sql.py` thread-safe. Fix outcomes persistence during disconnects/timeouts. | M1 | PLANNED |
| 4 | Verification & Compilation | Run `compile_ai.bat` to verify compilation. Validate registry files and configurations. (Simulation duels and runs are skipped per updated instructions). | M1, M2, M3 | PLANNED |

## Interface Contracts
### C# Engine ↔ Python Sandbox (Match Summary File)
- **File**: `Logs/match_summary.log` (or within local instance folders)
- **Format**: Structured text or JSON line indicating match results.
- **Fields**: Deck names, outcome status, final LPs, turn counts.

### C# Engine ↔ Python Sandbox (Decisions Log File)
- **File**: `decisions.jsonl`
- **Format**: JSON Lines format.
- **Fields**: `card_id`, `card_name`, `decision`, `plan`, `satisfaction_score`, `opponent_board` (JSON object detailing visible opponent cards, danger scores, etc.).

### Python Sandbox ↔ SQLite Database (`statistics.db`)
- **Tables**:
  - `matches`: `id`, `session_name`, `deck_self`, `opponent_deck`, `outcome` (Win/Loss/Draw/WeakWin/WeakLoss), `bot_lp`, `opp_lp`, `turns`.
  - `decisions`: decision events mapping to the match session.
- **Concurrency**: SQLite writes must handle file locks when multiple instances finish and trigger logging.
