# Execution Plan

## Steps:
1. **Analyze Codebase**: Spawn an explorer to deeply analyze `BaseCustomExecutor.cs`, `DreadnoughtExecutor.cs`, `InvokeExecutor.cs`, `save_outcomes_to_sql.py`, and `cockpit.py` to draft precise modification plans.
2. **Milestone 1 (C# Core Refactor)**: Spawn a worker to implement overloaded `OnCardAction` in `BaseCustomExecutor.cs` and refactor the custom callback hooks in `DreadnoughtExecutor.cs` and `InvokeExecutor.cs`. Verify compilation.
3. **Milestone 2 (Python Importer Fix)**: Spawn a worker to fix turn-restart splitting and add concurrency retries/timeouts in `save_outcomes_to_sql.py`.
4. **Milestone 3 (Auto-Deployment)**: Spawn a worker to implement automatic JSON registry syncing and headless DLL recompilation on LP=0.
5. **Milestone 4 (Fusion Stability)**: Spawn a worker to implement target fusion selection tracking and material validation recipes in `DreadnoughtExecutor.cs` and `InvokeExecutor.cs` to eliminate OCGCore crashes.
6. **Milestone 5 (Verification & Audit)**: Spawn reviewers, challengers, and a forensic auditor to run checks and confirm that all requirements are met with 100% integrity.
