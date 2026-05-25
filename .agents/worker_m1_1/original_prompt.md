## 2026-05-25T02:23:46Z
You are teamwork_preview_worker.
Your working directory is: c:\Users\admin\Documents\EDOTh\.agents\worker_m1_1\.
Your task is to:
1. Initialize BRIEFING.md and progress.md in your working directory.
2. Read the global SCOPE.md at c:\Users\admin\Documents\EDOTh\.agents\sub_orch_m1_csharp\SCOPE.md.
3. Review the Explorer findings and proposed fixes located in:
   - c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_1\analysis.md
   - c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_2\analysis.md
   - c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_3\analysis.md
4. Implement the following changes in c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs:
   - **Lifecycle Hooks**:
     Wrap all overridden lifecycle hooks (OnNewTurn, OnNewPhase, OnSelectHand, OnBattle, OnSelectAttackTarget, OnSelectCard, OnChaining, OnChainEnd) with safety null-checks for `Duel` and `Duel.Fields`, and execute them inside `try-catch` blocks.
     Ensure base class calls (like `base.OnNewTurn()`) are safely executed (e.g. in `finally` blocks or at the end).
     Implement an override for `OnDraw(int player)` with logging/safeguards, delegating back to `base.OnDraw(player)`.
   - **Process Exit Handlers & Memory Leaks**:
     Define a static lock (`_staticLock`) and a static `List<WeakReference<BaseCustomExecutor>>` to track active instances thread-safely.
     In the constructor, register the instance in the list thread-safely, and register `StaticOnProcessExit` to both `ProcessExit` and `DomainUnload` events if not already registered.
     In `Dispose(bool disposing)`, safely remove the instance from the list and apply real-time learning.
     In `StaticOnProcessExit`, safely retrieve all active targets using `TryGetTarget` and call `ApplyRealTimeLearning()` on each.
   - **ApplyRealTimeLearning Preconditions & Concurrency locks**:
     Keep track of `_lastBotLP` and `_lastOppLP` in active gameplay hooks as fallbacks.
     Relax `ApplyRealTimeLearning()` preconditions so it does not block when `Duel` or `Duel.Fields` is null. Instead, fall back to last-known LP values, and determine the outcome (e.g., Draw/Loss/Win) accordingly.
     Prevent learning if `_ourCardsPlayed.Count == 0` (aborted matches).
     In `SaveConfiguration()`, implement thread-safe file merging. Load the existing file from disk first, deserialize it, merge it with the current instance's in-memory data, and then serialize and write it back under the static config lock.
5. Compile the C# project using `c:\Users\admin\Documents\EDOTh\WindBot\compile_ai.bat` by running it via run_command tool. Ensure compilation completes with NO errors.
6. Verify your implementation by checking for compilation output.
7. Save a report of changes made and verification results to `c:\Users\admin\Documents\EDOTh\.agents\worker_m1_1\changes.md`.
8. Write `handoff.md` and send a message back to the parent conversation ID when complete.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT
hardcode test results, create dummy/facade implementations, or
circumvent the intended task. A Forensic Auditor will independently
verify your work. Integrity violations WILL be detected and your
work WILL be rejected.
