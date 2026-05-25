# Handoff Report — Explorer 3 (Preview Explorer)

This handoff report summarizes the findings of the C# codebase investigation under Milestone 1, focusing on lifecycle hooks, process exit handlers, and `ApplyRealTimeLearning()` preconditions.

## 1. Observation

Direct observations made in the codebase:
- **File Paths and Lines**:
  - `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`:
    - Line 83-84: `protected static BaseCustomExecutor _currentInstance = null;` and `protected static bool _processExitRegistered = false;`
    - Line 87-101 and 160-167: Constructor registers `AppDomain.CurrentDomain.ProcessExit += StaticOnProcessExit` without a lock.
    - Line 826-836: `ApplyRealTimeLearning()` checks:
      ```csharp
      if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
      {
          return;
      }
      ```
    - Line 2370: `_turnCount = Duel.Turn;` and line 2373: accessing `Duel.Fields[0].LifePoints` inside `OnNewTurn()` without verifying if `Duel` or its fields are null.
    - Line 2393: `Duel.Phase.ToString()` accessed directly.
    - Line 2889-2895: `StaticOnProcessExit` triggers `ApplyRealTimeLearning()` on `_currentInstance`.
  - `c:\Users\admin\Documents\EDOTh\BrainStroms\windbot-master\Game\AI\Executor.cs`:
    - Line 119: Defines `public virtual void OnDraw(int player)`. `BaseCustomExecutor.cs` does not override this method.
  - `c:\Users\admin\Documents\EDOTh\WindBot\compile_ai.bat`:
    - Compilation script uses `csc.exe` to compile custom executors referencing `ExecutorBase.dll`.

## 2. Logic Chain

1. **Crash Risk in Lifecycle Hooks**: Because lifecycle hooks like `OnNewTurn` and `OnNewPhase` directly access properties of `Duel` and `Duel.Fields` (Observations A1/A2) without verifying if `Duel` or `Duel.Fields` is null/empty, any sudden uninitialization or early disconnect will result in a `NullReferenceException` which will crash the engine thread.
2. **Missing Hook `OnDraw`**: Because `Executor.cs` defines `OnDraw` (Observation B1) but `BaseCustomExecutor.cs` does not override it, any draw event occurs without the executor's custom logging and safety wrappers, leaving a gap in tracing game state.
3. **Thread Safety & Registration Issues in Process Exit**: Because process exit registration (Observation A3) does not use a lock and depends on a single static variable `_currentInstance` (Observation A1), multiple instances initializing concurrently can cause race conditions. Furthermore, only the last created instance is saved.
4. **Memory Leak**: Because `_currentInstance` is a static field referencing the custom executor (Observation A1), it keeps a strong reference to it, preventing the GC from reclaiming the memory of the finished duel session.
5. **Early Return in learning**: Because `ApplyRealTimeLearning()` returns early if `Duel` or its fields are null (Observation A4), matches ending via sudden client disconnects or server timeouts cannot save their in-memory opponent card memory or priority updates, leading to data loss.
6. **Lost Updates**: Because concurrent duels write to the same files in `SaveConfiguration` without merging disk contents, whoever saves last overwrites the other's changes.

## 3. Caveats

- We assumed that multiple processes do not write to the same configuration files simultaneously, though we added a thread-safe in-memory lock (`_staticLock`) to handle multi-threaded concurrency inside the same process. For multi-process safety, a global Mutex could be used if necessary.
- We did not compile or run the binary via the user command prompt as the permission prompt timed out, but verified the API signatures against `executor_api_details.txt`.

## 4. Conclusion

- Robust null safety checks and try-catch-finally wrappers must be introduced into all lifecycle hooks, and a wrapped override of `OnDraw` should be added.
- The process exit registration should use `lock (_staticLock)` and track instances via a list of `WeakReference<BaseCustomExecutor>` to prevent memory leaks and handle concurrent duels correctly.
- `ApplyRealTimeLearning` should decouple save/decay logic from the presence of `Duel` and `Fields` so it can write results even on disconnects/timeouts.
- `SaveConfiguration` should load and merge disk configs before writing to prevent lost updates under concurrency.

## 5. Verification Method

- **Files to Inspect**: Verify that `BaseCustomExecutor.cs` contains the updated lifecycle hooks, process exit logic using `WeakReference`, and the refactored `ApplyRealTimeLearning`/`SaveConfiguration`.
- **Build/Test Command**: Execute `compile_ai.bat` in `c:\Users\admin\Documents\EDOTh\WindBot\` to verify compilation passes without error:
  `cmd.exe /c compile_ai.bat`
- **Invalidation Conditions**: If compiling `compile_ai.bat` throws any syntax errors or type signature mismatches, or if `System.Web.Extensions.dll` is missing, the solution must be corrected to match the referenced assembly signatures.
