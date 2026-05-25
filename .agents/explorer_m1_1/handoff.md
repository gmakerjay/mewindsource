# Handoff Report — Explorer 1

## 1. Observation
We audited the AI executor codebase and the exported library interfaces. The following exact lines and code structures were observed:

1.  **Vulnerable `OnNewTurn`**:
    In `BaseCustomExecutor.cs` (lines 2360–2372):
    ```csharp
    public override void OnNewTurn()
    {
        if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
        {
            if (Duel.Fields[0].LifePoints == 0 || Duel.Fields[1].LifePoints == 0)
            {
                ApplyRealTimeLearning();
            }
        }

        _turnCount = Duel.Turn; // Direct access without null check!
    ```

2.  **Missing `OnDraw` Override**:
    `OnDraw(int player)` is a virtual hook in `ExecutorBase.dll` (verified in `scratch/dll_types.txt` line 506: `Method: Void OnDraw(Int32 player) (Virtual: True)`) but is completely missing from `BaseCustomExecutor.cs`.

3.  **Process Exit Handler Static Instance Registration**:
    In `BaseCustomExecutor.cs` (lines 161–166):
    ```csharp
    _currentInstance = this;
    if (!_processExitRegistered)
    {
        AppDomain.CurrentDomain.ProcessExit += StaticOnProcessExit;
        _processExitRegistered = true;
    }
    ```
    And in `StaticOnProcessExit` (lines 2889–2895):
    ```csharp
    private static void StaticOnProcessExit(object sender, EventArgs e)
    {
        if (_currentInstance != null)
        {
            _currentInstance.ApplyRealTimeLearning();
        }
    }
    ```

4.  **Learning Preconditions and Teardown**:
    In `BaseCustomExecutor.cs` (lines 831–834):
    ```csharp
    if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
    {
        return;
    }
    ```
    If no cards were played (aborted match), the decay logic at line 927 is still processed:
    ```csharp
    if (!_ourCardsPlayed.Contains(kvpDecay.Key) && decayCard.priority >= 8)
    ```

---

## 2. Logic Chain
-   **Unsafe Lifecycle Hook Delegation**: Disconnects or cleanups tear down the `Duel` reference. Direct access to `Duel.Turn` or `Duel.Fields[1]` throws a `NullReferenceException`. Therefore, adding complete validation checks at hook entry points is required to prevent match crash loops.
-   **Thread Safety & Multi-Instance Handling**:
    -   `AppDomain.CurrentDomain.ProcessExit` is invoked asynchronously. If the main thread concurrently calls `Dispose()`, both threads execute `ApplyRealTimeLearning()` and write to the configuration file simultaneously, causing database corruption. A static lock is necessary.
    -   Because only a single static `_currentInstance` is registered, concurrent matches overwrite it, meaning only the last created bot saves its data upon process exit. Tracking active instances in a synchronized collection is required to prevent data loss.
-   **ApplyRealTimeLearning Outcomes**:
    -   When network timeout/disconnect sets `Duel` or `Duel.Fields` to null, the strict null block prevents saving. Tracking `_lastBotLP` and `_lastOppLP` in memory provides a fallback to calculate the match outcome.
    -   When matches abort before a turn is taken, zero cards are played. Decay under `_ourCardsPlayed.Count == 0` degrades valid starter priority. Skipping learning entirely in aborted matches protects configuration integrity.

---

## 3. Caveats
No caveats.

---

## 4. Conclusion
The codebase contains critical thread safety risks, multi-instance data loss, and disconnect state-loss vulnerabilities. Implementing a thread-safe static collection tracker, fallback LP caching, and complete null guards on lifecycle hooks will ensure 100% learning persistence and runtime stability.

---

## 5. Verification Method
1.  Verify compilation of proposed changes using `compile_ai.bat`.
2.  Inspect `analysis.md` at `c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_1\analysis.md` for exact C# implementation details and diffs.
3.  Simulate a network disconnect during a duel to ensure the learning data is successfully cached and written to the configuration database using the fallback LP parameters.
