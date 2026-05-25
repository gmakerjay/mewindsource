# C# AI Engine Safeguards Audit Analysis

## Core Summary
This report analyzes potential runtime crashes, thread-safety bugs, multi-instance data loss, and resource leaks in the C# AI Engine (`BaseCustomExecutor.cs` and `UnifiedIgnisExecutor.cs`). We identify specific vulnerabilities in lifecycle hook implementations, process exit handlers, and learning outcome saving, and propose safe, backwards-compatible fixes.

---

## 1. Observation

### A. Lifecycle Hooks
We examined the lifecycle hooks in `BaseCustomExecutor.cs`:
1. **`OnNewTurn()` (Lines 2360–2389)**:
   - Accesses `Duel.Turn`, `Duel.Player`, `Duel.Fields[0].LifePoints`, and `Duel.Fields[1].LifePoints` without null checks on `Duel` or `Duel.Fields`.
   - Periodically calls `SaveConfiguration()`.
2. **`OnNewPhase()` (Lines 2391–2395)**:
   - Accesses `Duel.Phase` without null-checking `Duel`.
3. **`OnSelectHand()` (Lines 2397–2411)**:
   - Checks `_deckConfig.playstyle` but has no validation for whether `_deckConfig` itself is initialized.
4. **`OnBattle()` (Lines 2541–2619)** and **`OnSelectAttackTarget()` (Lines 2621–2723)**:
   - Implement complex search algorithms with multiple dereferences of `Duel`, `Enemy`, and `Bot` zones. If any of these fields are partially unitialized (e.g., during startup/teardown phases or disconnects), they throw uncaught exceptions.
5. **`OnSelectCard()` (Lines 2725–2805)**:
   - Sorts cards using metadata-based priorities, dereferencing `Card` and list items.
6. **`OnChaining()` (Lines 2814–2874)** and **`OnChainEnd()` (Lines 2876–2887)**:
   - Access `Duel` and `Duel.Fields` without safety checks, and lack exception protection.
7. **`OnDraw()`**:
   - `BaseCustomExecutor` has **no override** for `OnDraw(int player)`. The base class `DefaultExecutor` (referenced from `ExecutorBase.dll`) defines a virtual `OnDraw` method, which is left unhandled here.

### B. Process Exit Handlers & Static Flags
1. **Registration & Thread Safety (Lines 161–166)**:
   ```csharp
   _currentInstance = this;
   if (!_processExitRegistered)
   {
       AppDomain.CurrentDomain.ProcessExit += StaticOnProcessExit;
       _processExitRegistered = true;
   }
   ```
   - Modifies static fields `_currentInstance` and `_processExitRegistered` concurrently without any synchronization lock.
2. **Overwriting in Constructor (Lines 89–100)**:
   ```csharp
   if (_currentInstance != null)
   {
       try
       {
           _currentInstance.ApplyRealTimeLearning();
       }
       catch (Exception ex) ...
   }
   ```
   - Only the single *last-instantiated* executor is kept in `_currentInstance`. All other concurrent instances are overwritten.
3. **Process Exit Handler (Lines 2889–2895)**:
   ```csharp
   private static void StaticOnProcessExit(object sender, EventArgs e)
   {
       if (_currentInstance != null)
       {
           _currentInstance.ApplyRealTimeLearning();
       }
   }
   ```
   - When the process exits, learning is saved only for `_currentInstance`. If multiple matches were running concurrently in the same process, the other instances' learning data is completely lost.
   - `_currentInstance` is a static GC root keeping the last bot instance alive in memory forever (causing a memory leak).
   - Registration does not listen to `AppDomain.CurrentDomain.DomainUnload`, causing data loss if the host unloads the AppDomain instead of terminating the process.

### C. ApplyRealTimeLearning() Preconditions
1. **Hard Blocking Check (Lines 831–834)**:
   ```csharp
   if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
   {
       return;
   }
   ```
   - In case of a timeout or disconnect, the duel engine may nullify or dispose of `Duel` and `Duel.Fields`. Under these conditions, the method aborts instantly.
   - Any learning accumulated during the match (in `_ourCardsPlayed` or `_disruptionsInMatch`) is never saved, even though these collections do not require a valid `Duel` object to serialize.
2. **Shared Config Overwrite**:
   - `SaveConfiguration()` writes to a single shared configuration file `cards_registry_{deck}.json` and `opponent_memory.json`.
   - Concurrent instances running in different threads can read/write the same file at the same time, leading to sharing violations or corrupted JSON files.

### D. Compile Script
We examined `compile_ai.bat` (Lines 1–9):
```bat
@echo off
cd /d "%~dp0"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library /r:System.Web.Extensions.dll /r:ExecutorBase.dll /out:Executors\UnifiedIgnisExecutor.dll BaseCustomExecutor.cs UnifiedIgnisExecutor.cs PureYummyExecutor.cs InvokeExecutor.cs
```
- It compiles all four executors into a single library: `Executors\UnifiedIgnisExecutor.dll`.
- Uses the legacy C# compiler from .NET Framework 4.0.
- Does not create the `Executors` subdirectory; if the folder is missing, compilation will fail.

---

## 2. Logic Chain

1. **Uncaught exceptions inside lifecycle hooks** (due to null `Duel` or `Fields`) cause the duel server thread to abort, terminating the match abruptly. Thus, we need robust `try-catch` blocks and safety null-checks.
2. **A single static variable `_currentInstance`** is insufficient for multi-instance scenarios. It causes:
   - Data loss: Concurrent instances have their references overwritten, so they never write learning data on exit.
   - Memory leak: The static reference prevents Garbage Collection of the last instance.
   - Solution: Use a thread-safe `List<BaseCustomExecutor>` registered with lock guards and deregister instances inside `Dispose(bool)`.
3. **If a match times out/disconnects**, the `Duel` object is teardown by the engine. But the bot's played card data (`_ourCardsPlayed` and `_disruptionsInMatch`) remains in RAM. If we remove the rigid null block at the start of `ApplyRealTimeLearning()`, we can treat these cases as a `"Draw"` or `"Unknown"` outcome and still successfully save configuration data.
4. **File locking issues** arise when multiple instances access config files concurrently. Using a `static object` lock guarantees that only one thread in the process can access files in `LoadConfiguration()` and `SaveConfiguration()` at any time.

---

## 3. Caveats
- **Process Boundaries**: While static locks (`lock (_configLock)`) prevent thread conflicts within the *same* process, they do not coordinate between *separate* OS processes. The filesystem `ReadFileWithRetry` and `WriteFileWithRetry` must remain in place to handle multi-process conflicts.
- **`OnDraw` override**: Adding `OnDraw` logging is a nice-to-have wrapper, but we must call `base.OnDraw(player)` to maintain core engine behaviors.

---

## 4. Conclusion & Proposed Fixes

We propose the following code modifications to resolve all diagnosed issues:

### Fix A: Safely Delegate and Wrap Lifecycle Hooks
We propose wrapping all lifecycle hooks in `BaseCustomExecutor.cs` with `try-catch` and `finally` blocks, and adding null-checks:

```csharp
        public override void OnNewTurn()
        {
            try
            {
                if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
                {
                    if (Duel.Fields[0].LifePoints == 0 || Duel.Fields[1].LifePoints == 0)
                    {
                        ApplyRealTimeLearning();
                    }
                }

                if (Duel != null)
                {
                    _turnCount = Duel.Turn;
                    LogToTurn(string.Format("=== Turn {0} Started (Active Player: {1}) ===", _turnCount, Duel.Player == 0 ? "Bot" : "Opponent"));
                    if (Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
                    {
                        LogToTurn(string.Format("Bot LP: {0} | Opponent LP: {1}", Duel.Fields[0].LifePoints, Duel.Fields[1].LifePoints));
                    }
                }

                _currentPlan = "PlanA";
                _blockedPlans.Clear();
                LogToTurn("Combo Plan initialized to PlanA");

                LogState();

                if (_turnCount > 0 && _turnCount % 3 == 0)
                {
                    try { SaveConfiguration(); }
                    catch (Exception ex) { LogToTurn("Periodic save failed: " + ex.Message); }
                }

                UpdateGoal();
            }
            catch (Exception ex)
            {
                Log("Error in OnNewTurn hook: " + ex.Message);
            }
            finally
            {
                base.OnNewTurn();
            }
        }

        public override void OnNewPhase()
        {
            try
            {
                if (Duel != null)
                {
                    LogToTurn("--- Phase Changed to: " + Duel.Phase.ToString() + " ---");
                }
            }
            catch (Exception ex)
            {
                Log("Error in OnNewPhase hook: " + ex.Message);
            }
            finally
            {
                base.OnNewPhase();
            }
        }

        public override bool OnSelectHand()
        {
            try
            {
                if (_deckConfig != null)
                {
                    if (_deckConfig.playstyle == "control" || _deckConfig.playstyle == "combo" || _deckConfig.playstyle == "midrange")
                    {
                        LogToTurn(string.Format("Playstyle is {0}, selecting to go first.", _deckConfig.playstyle));
                        return true;
                    }
                    if (_deckConfig.playstyle == "go_second")
                    {
                        LogToTurn("Playstyle is go_second, selecting to go second.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error in OnSelectHand hook: " + ex.Message);
            }
            return false;
        }

        public override BattlePhaseAction OnBattle(IList<ClientCard> attackers, IList<ClientCard> defenders)
        {
            try
            {
                // Core OnBattle logic...
                // (Omitted for brevity, wrapped in try-catch)
            }
            catch (Exception ex)
            {
                Log("Error in OnBattle hook: " + ex.Message);
                return null;
            }
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            try
            {
                // Core OnSelectAttackTarget logic...
            }
            catch (Exception ex)
            {
                Log("Error in OnSelectAttackTarget hook: " + ex.Message);
                return null;
            }
        }

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            try
            {
                // Core OnSelectCard logic...
            }
            catch (Exception ex)
            {
                Log("Error in OnSelectCard hook: " + ex.Message);
                return base.OnSelectCard(cards, min, max, hint, cancelable);
            }
        }

        public override void OnChaining(int player, ClientCard card)
        {
            try
            {
                // Core OnChaining logic...
            }
            catch (Exception ex)
            {
                Log("Error in OnChaining: " + ex.Message);
            }
            finally
            {
                base.OnChaining(player, card);
            }
        }

        public override void OnChainEnd()
        {
            try
            {
                LogToTurn("--- Chain resolution finished ---");
                if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
                {
                    if (Duel.Fields[0].LifePoints == 0 || Duel.Fields[1].LifePoints == 0)
                    {
                        ApplyRealTimeLearning();
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error in OnChainEnd hook: " + ex.Message);
            }
            finally
            {
                base.OnChainEnd();
            }
        }

        public override void OnDraw(int player)
        {
            try
            {
                LogToTurn(string.Format("Player {0} drew a card.", player));
            }
            catch (Exception ex)
            {
                Log("Error in OnDraw hook: " + ex.Message);
            }
            finally
            {
                base.OnDraw(player);
            }
        }
```

### Fix B: Thread-Safe and Multi-Instance Process Exit Handler
Introduce a list to track all active instances and manage registration/deregistration safely:

```csharp
        protected static readonly List<BaseCustomExecutor> _activeInstances = new List<BaseCustomExecutor>();
        protected static readonly object _instanceLock = new object();
        protected static bool _processExitRegistered = false;
        protected static readonly object _configLock = new object();
```

Inside the constructor:
```csharp
            lock (_instanceLock)
            {
                _activeInstances.Add(this);
                if (!_processExitRegistered)
                {
                    AppDomain.CurrentDomain.ProcessExit += StaticOnProcessExit;
                    AppDomain.CurrentDomain.DomainUnload += StaticOnProcessExit;
                    _processExitRegistered = true;
                }
            }
```

Inside `Dispose(bool disposing)`:
```csharp
            if (!_disposed)
            {
                ApplyRealTimeLearning();
                
                try
                {
                    LogToMatch("=== Duel Session Finished ===");
                    // (Logging duel outcome)
                }
                catch {}

                // Remove instance from active list to prevent memory leak
                lock (_instanceLock)
                {
                    _activeInstances.Remove(this);
                }

                _disposed = true;
            }
```

The process exit handler:
```csharp
        private static void StaticOnProcessExit(object sender, EventArgs e)
        {
            BaseCustomExecutor[] instancesCopy;
            lock (_instanceLock)
            {
                instancesCopy = _activeInstances.ToArray();
            }

            foreach (var instance in instancesCopy)
            {
                try
                {
                    instance.ApplyRealTimeLearning();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[IgnisEngine] Error during process exit ApplyRealTimeLearning: " + ex.Message);
                }
            }
        }
```

Additionally, wrap the contents of `LoadConfiguration()` and `SaveConfiguration()` with `lock (_configLock)` to guarantee thread safety of configuration access within the same process.

### Fix C: Relax ApplyRealTimeLearning() Preconditions
Rewrite `ApplyRealTimeLearning()` to work without requiring a valid `Duel` object:

```csharp
        protected void ApplyRealTimeLearning()
        {
            if (_learningApplied) return;
            _learningApplied = true;

            try
            {
                int botLP = 8000;
                int oppLP = 8000;
                bool hasDuelState = (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null);

                if (hasDuelState)
                {
                    botLP = Duel.Fields[0].LifePoints;
                    oppLP = Duel.Fields[1].LifePoints;
                }

                string outcome = "Unknown";
                if (hasDuelState && botLP == 0 && oppLP > 0) 
                    outcome = "Loss";
                else if (hasDuelState && oppLP == 0 && botLP > 0) 
                    outcome = "Win";
                else if (_ourCardsPlayed.Count > 0)
                {
                    if (hasDuelState)
                    {
                        if (botLP > oppLP + 3000) outcome = "WeakWin";
                        else if (oppLP > botLP + 3000) outcome = "WeakLoss";
                        else outcome = "Draw";
                    }
                    else
                    {
                        outcome = "Draw";
                    }
                }
                else
                {
                    return;
                }

                LogToMatch(string.Format("Applying Real-time Learning: Outcome is {0} (Bot LP: {1}, Opp LP: {2}, Turns: {3})", outcome, botLP, oppLP, _turnCount));
                
                // (Remainder of Adjustments and Decay Logic operates safely on collections)
                ...
                
                SaveConfiguration();
            }
            catch (Exception ex)
            {
                LogToMatch("Error applying real-time learning: " + ex.Message);
            }
        }
```

---

## 5. Verification Method

1. **Verify compilation**:
   - Run `compile_ai.bat` in command line. It must succeed with output `Compilation SUCCESSFUL!`.
2. **Verify multi-instance behavior**:
   - Launch multiple bot sessions concurrently. Check memory and log output. Each instance must generate its own log folder under `Logs/` and write to the global configuration files without locks or crashes.
3. **Verify process exit handling**:
   - Terminate the bot process midway. The terminal log or `match_summary.log` must show `Applying Real-time Learning` call for all active matches.
4. **Verify disconnect handling**:
   - Simulate a disconnect (tearing down the connection). Even if the engine sets `Duel` to null, `Dispose()` must successfully apply real-time learning (with outcome `Draw`) and save files.
