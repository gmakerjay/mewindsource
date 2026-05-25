# Q-Learning & DB Logging Optimization Plan

## Architecture
- **C# Engine (`BaseCustomExecutor.cs`)**: Logs decision data in JSONL format to `decisions.jsonl`. We need to fix the invalid JSON format where `"danger":F1` is outputted instead of a float.
- **Python DB Importer (`save_outcomes_to_sql.py`)**: Automatically imports matches and decisions from log folders to SQLite `statistics.db`. Fix will allow successful parsing of JSON records.
- **Python Q-Trainer (`q_learning.py`)**: Computes Monte Carlo reinforcement rewards from matches and writes updated weights/Q-values back to `cards_registry_{deck_name}.json`.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | DB & Serialization Audit | Verify C# JSON output, SQLite schema constraints, and identify all formatting bugs. | None | DONE |
| 2 | Code Modifications & Implementation | Fix `danger` formatting in C#, tune `q_learning.py` reward function, implement database wiping, and add priority clamping. | M1 | DONE |
| 3 | Compilation & Integrity Check | Verify the C# project compiles successfully using `compile_ai.bat` and all safeguards are respected. | M2 | DONE |
| 4 | Multi-Match Simulation & Weight Delta Verification | Simulates a bot-vs-bot match, import logs to SQLite, execute Q-learning, and verify before/after registry weights. | M3 | DONE |

## Interface Contracts
### C# (`BaseCustomExecutor.cs`) -> Python (`save_outcomes_to_sql.py` / `q_learning.py`)
- The C# engine must output valid, standard JSON objects in `decisions.jsonl`.
- JSON structure for decision entries:
  - `turn`: integer
  - `card_id`: integer
  - `card_name`: string
  - `action`: string (e.g. "Activate", "Summon")
  - `goal`: string (e.g. "break_board")
  - `score`: float
  - `decision`: boolean
  - `plan`: string
  - `lp_self`: integer
  - `lp_opp`: integer
  - `opponent_threat`: float
  - `bot_monsters`, `opp_monsters`, `opp_spells`, `bot_hand`: arrays of objects with standard JSON-compliant types (no `danger: F1`).
