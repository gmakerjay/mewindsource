# Handoff Report — Explorer 2 (Milestone 1 C# safeguards audit)

## 1. Observation
- **Lifecycle Hooks**: `OnNewTurn` (Lines 2360–2389), `OnNewPhase` (Lines 2391–2395), `OnSelectHand` (Lines 2397–2411), `OnBattle` (Lines 2541–2619), `OnSelectAttackTarget` (Lines 2621–2723), `OnSelectCard` (Lines 2725–2805), `OnChaining` (Lines 2814–2874), and `OnChainEnd` (Lines 2876–2887) do not implement try-catch protection. Several hooks dereference `Duel` or `Duel.Fields` directly (e.g. `_turnCount = Duel.Turn` or `Duel.Fields[0].LifePoints == 0`), which will throw a `NullReferenceException` if the match is closing or disconnected. `OnDraw` is missing entirely in `BaseCustomExecutor.cs`.
- **Process Exit Handler**: `BaseCustomExecutor` constructor (Lines 161–166) registers a static `StaticOnProcessExit` handler. It tracks only a single active instance in `_currentInstance` (Line 161), overwriting previous instances (Line 89). On process termination/unload, only the last-created instance executes `ApplyRealTimeLearning()`. It also causes a memory leak because the static variable prevents GC of the instance.
- **ApplyRealTimeLearning Preconditions**: Has a hard block `if (Duel == null || Duel.Fields == null || ...)` (Lines 831–834) that halts execution immediately on disconnects or timeouts when the match engine tears down the `Duel` object. This causes complete data loss of match learning outcomes, even though internal lists like `_ourCardsPlayed` are valid.
- **Compilation**: `compile_ai.bat` compiles the executors using:
  ```bat
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library /r:System.Web.Extensions.dll /r:ExecutorBase.dll /out:Executors\UnifiedIgnisExecutor.dll BaseCustomExecutor.cs UnifiedIgnisExecutor.cs PureYummyExecutor.cs InvokeExecutor.cs
  ```

## 2. Logic Chain
1. Dereferencing members of a null `Duel` or `Duel.Fields` throws uncaught `NullReferenceException` in lifecycle hooks. Safely wrapping them in `try-catch` blocks and adding null check guards ensures that any hook crash falls back to default engine behaviors rather than killing the server thread.
2. Storing active executors in a single static field `_currentInstance` results in overwrites when multiple games run concurrently, losing exit-learning data for all but the last-created game. Transitioning to a thread-safe static list (`List<BaseCustomExecutor>`) with lock guards allows us to save learning outcomes for all active games upon process exit/domain unload, and deregistering in `Dispose()` avoids memory leaks.
3. Aborting `ApplyRealTimeLearning()` because `Duel` is null prevents saving card statistics from disconnected/timed out games. Checking `hasDuelState` and defaulting outcome to `"Draw"` allows the bot to safely serialize its card priorities and update `opponent_memory.json` / `cards_registry_{deck}.json` anyway.
4. Adding `lock (_configLock)` coordinates concurrent reads and writes to configuration JSON files between threads inside the same process, preventing file-sharing exceptions.

## 3. Caveats
- Static locking coordinates threads within the *same* process but does not coordinate across different OS processes. The existing retry-delay file IO helper (`ReadFileWithRetry` and `WriteFileWithRetry`) remains necessary to handle cross-process concurrency.
- The `OnDraw` hook override must forward calls to `base.OnDraw(player)` to preserve underlying default behaviors.

## 4. Conclusion
Centralizing safety wrappers, thread-safe instance tracking, and relaxed learning preconditions in `BaseCustomExecutor.cs` (which all Ignis executors inherit) will eliminate runtime crashes, thread/process-exit resource leaks, and data loss.

For detailed recommendations and exact code snippets, see:
`c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_2\analysis.md`

## 5. Verification Method
1. **Compilation**: Execute `compile_ai.bat` in the workspace to verify the modified code compiles with legacy CSC.
2. **Concurrency**: Run multiple bot instances concurrently. Check that each match creates a unique folder under `Logs/` and reads/writes configurations thread-safely.
3. **Exit Handling**: Terminate a multi-session process mid-game. Verify that `ApplyRealTimeLearning` runs and saves for all active instances.
4. **Teardown**: Simulate a disconnect and verify learning is saved as a "Draw" instead of aborting.
