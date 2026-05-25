## 2026-05-25T13:01:40Z
You are the Refactor Explorer subagent.
Your identity is teamwork_preview_explorer.
Your working directory is: c:\Users\admin\Documents\EDOTh\.agents\teamwork_preview_explorer_refactor_analysis

Please perform an in-depth analysis of the system to prepare for the following requirements:
1. R1: Overloading OnCardAction in BaseCustomExecutor.cs. Look at existing OnCardAction, EvaluateCardAction, and _ourCardsPlayed. How should we implement the overload?
2. R2: Wrapping custom executor callbacks in DreadnoughtExecutor.cs and InvokeExecutor.cs. Look at how they use AddExecutor and target callbacks.
3. R3: Fixing Decisions Partitioning & Concurrency in save_outcomes_to_sql.py. Look at decisions.jsonl reading, turn comparison, SQLite connection retries/timeouts. How should we implement the robust split and retry?
4. R4: Automatic deployment and compiling on LP = 0. How does cockpit.py and BaseCustomExecutor.cs interact? Where does the LP monitor thread check for LP=0? Can we intercept it in BaseCustomExecutor.cs (e.g. inside ApplyRealTimeLearning or during duel shutdown/reset) to trigger an automated sync and compilation? How should cockpit.py detect or react to this?
5. R5: Fix Fusion Material Selection Crash in DreadnoughtExecutor.cs and InvokeExecutor.cs. Look at OnSelectCard, HintMsg_SpSummon, HintMsg_FusionMaterial. Draft the recipe requirements and prioritization.

Create a detailed report named analysis.md and a handoff.md in your working directory. Then use send_message to report back to your parent conversation (ID: caa92013-e2fd-4b40-8e51-3362e33e2a91) with a summary and the paths to your reports.
