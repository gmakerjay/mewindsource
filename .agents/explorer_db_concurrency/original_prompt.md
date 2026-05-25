## 2026-05-25T21:03:38Z
Analyze c:\Users\admin\Documents\EDOTh\Developer\scratch\save_outcomes_to_sql.py (and any other python importer scripts).
Tasks:
1. Locate where decisions are parsed from decisions.jsonl and matches are partitioned. Propose how to detect game restarts to correctly partition matches even if a game ends/restarts on Turn 1.
2. Locate SQLite database write operations. Propose how to wrap database writes with transaction retry loops and timeouts (including WAL mode settings if needed) to support concurrent multi-instance training from different ports without database locking errors.
Write your analysis and proposed code changes to handoff.md in your working directory (.agents/explorer_db_concurrency/).
