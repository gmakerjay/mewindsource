# C# AI Engine Hooks & Safeguards Audit Analysis

## 1. Executive Summary
This report presents an architectural audit of the C# AI Engine (`BaseCustomExecutor.cs` and `UnifiedIgnisExecutor.cs`), detailing critical issues in lifecycle hooks, thread safety of the process exit handlers, and real-time learning persistence during unexpected terminations (timeout/disconnect). Precise, production-grade solutions are proposed to resolve compiler warnings, null-reference crashes, multi-instance data loss, and race conditions during file persistence.

---

## 2. Audited Files & Structures
- **`BaseCustomExecutor.cs`**: Abstract base executor class managing match state, configuration loading/saving, and the learning engine.
- **`UnifiedIgnisExecutor.cs`**: Unified implementation inheriting from `BaseCustomExecutor.cs`.
- **`PureYummyExecutor.cs` & `InvokeExecutor.cs`**: Concrete deck-specific implementations compiling against `ExecutorBase.dll`.
- **`ExecutorBase.dll` / `DefaultExecutor`**: Binary interface providing the core deck logic engine and game state contracts.

---

## 3. Diagnosed Issues & Technical Vulnerabilities

### Issue 3.1: Unsafe Lifecycle Hook Delegation & Null Reference Crashes
**Observation**: Multiple lifecycle hooks read variables from `Duel` and `Duel.Fields` without prior null-checks. 
*   In `OnNewTurn()`, line 2362 performs a null-check, but lines 2370 and 2372 access `Duel.Turn` and `Duel.Player` directly. If the engine is in teardown, this raises a `NullReferenceException`.
*   In `OnBattle()` (lines 2541-2619) and `OnSelectAttackTarget()` (lines 2621-2723), `Duel.Fields[1]` is accessed without verifying that `Duel` or `Duel.Fields` is non-null.
*   `OnDraw(int player)` is completely absent from `BaseCustomExecutor.cs`, meaning it cannot intercept draw events to log information or update state.

**Impact**: High-frequency crashes on disconnects and match teardowns, leading to thread aborts and incomplete log outputs.

---

### Issue 3.2: Thread Safety Risks & Multi-Instance Exit Data Loss
**Observation**: 
*   `BaseCustomExecutor` uses a single static `_currentInstance` to handle process exit hooks:
    ```csharp
    _currentInstance = this;
    if (!_processExitRegistered)
    {
        AppDomain.CurrentDomain.ProcessExit += StaticOnProcessExit;
        _processExitRegistered = true;
    }
    ```
*   `StaticOnProcessExit` is invoked by `AppDomain.CurrentDomain.ProcessExit` on a separate thread, while `Dispose` is invoked on the main client thread.

**Vulnerabilities**:
1.  **Concurrency / Race Conditions**: Both `Dispose()` and `StaticOnProcessExit()` call `ApplyRealTimeLearning()` without locking. Since `_learningApplied` is not volatile and is accessed without synchronizing, both threads can execute the file-saving operations concurrently, corrupting the JSON configuration database.
2.  **Multi-Instance Override**: In a concurrent environment (e.g. running multiple matches in parallel), `_currentInstance` is overwritten by the latest instantiated executor. Only the last executor will save its learning data on process exit; all other active executors will experience complete data loss.
3.  **Domain Unload Missing**: Sandbox/plugin environments may unload the AppDomain without terminating the process, bypassing `ProcessExit` completely.

---

### Issue 3.3: Precondition Blocks in `ApplyRealTimeLearning` on Timeout/Disconnect
**Observation**: `ApplyRealTimeLearning()` starts with a strict precondition check:
```csharp
if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
{
    return;
}
```
**Vulnerabilities**:
1.  **Data Loss on Disconnect/Timeout**: When a match is aborted by a timeout or network disconnect, the windbot engine clears the `Duel` reference or tears down its fields. The hard block returns early, completely discarding all match memory (disruptions, cards played, seen cards) accumulated during the duel.
2.  **Anti-Inflation Decay Bug**: If a game aborts before any cards are played (`_ourCardsPlayed.Count == 0`), the unplayed decay logic (line 927) still executes, decaying the priority of all starter/payoff cards with priority >= 8 from `8` to `7`. This penalizes valid cards for aborted games.

---

## 4. Proposes Safe Fixes & Diffs

### Propose Fix 4.1: Fields & Tracker Setup in `BaseCustomExecutor.cs`
Introduce a static lock, a static list for multi-instance tracking, and fallback state trackers for LP and Turn:

```csharp
        // Thread safety and instance tracking locks
        protected static readonly object _learningLock = new object();
        protected static readonly List<BaseCustomExecutor> _activeInstances = new List<BaseCustomExecutor>();
        protected static readonly object _instancesLock = new object();

        // Fallback variables for timeout/disconnect persistence
        protected int _lastBotLP = 8000;
        protected int _lastOppLP = 8000;
        
        // Helper to update LP states safely
        protected void UpdateLastKnownLP()
        {
            if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
            {
                _lastBotLP = Duel.Fields[0].LifePoints;
                _lastOppLP = Duel.Fields[1].LifePoints;
            }
        }
```

### Propose Fix 4.2: Constructor Registration Multi-Instance Handling
Refactor the registration logic to handle multiple active instances and listen to both `ProcessExit` and `DomainUnload`:

```csharp
            // Register ProcessExit and DomainUnload handler to save learning data safely on exit for all active instances
            lock (_instancesLock)
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

### Propose Fix 4.3: Thread-Safe Multi-Instance Static Exit Handler
Rewrite `StaticOnProcessExit` to iterate through all active instances and save them safely:

```csharp
        private static void StaticOnProcessExit(object sender, EventArgs e)
        {
            List<BaseCustomExecutor> targets;
            lock (_instancesLock)
            {
                targets = new List<BaseCustomExecutor>(_activeInstances);
            }
            foreach (var instance in targets)
            {
                try
                {
                    instance.ApplyRealTimeLearning();
                }
                catch (Exception ex)
                {
                    // Ensure exit process itself does not crash
                }
            }
        }
```

### Propose Fix 4.4: Thread-Safe Dispose Cleanup
Update `Dispose` to clean up the instance from the active registry:

```csharp
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                ApplyRealTimeLearning();
                
                lock (_instancesLock)
                {
                    _activeInstances.Remove(this);
                }
                
                try
                {
                    LogToMatch("=== Duel Session Finished ===");
                    if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
                    {
                        LogToMatch("Final Bot LP: " + Duel.Fields[0].LifePoints);
                        LogToMatch("Final Opponent LP: " + Duel.Fields[1].LifePoints);
                    }
                    else
                    {
                        LogToMatch("Final Bot LP: " + _lastBotLP + " (Fallback)");
                        LogToMatch("Final Opponent LP: " + _lastOppLP + " (Fallback)");
                    }
                    LogToMatch("Finished Time: " + DateTime.Now.ToString());
                }
                catch {}

                _disposed = true;
            }
        }
```

### Propose Fix 4.5: Robust Real-Time Learning with Fallbacks
Remove the hard block, fallback to `_lastBotLP` and `_lastOppLP`, ensure locks are acquired, and guard against empty card matches:

```csharp
        protected void ApplyRealTimeLearning()
        {
            lock (_learningLock)
            {
                if (_learningApplied) return;
                
                // If nothing was played, do not apply learning to avoid corruption on immediately aborted matches
                if (_ourCardsPlayed.Count == 0)
                {
                    return;
                }

                _learningApplied = true;

                try
                {
                    int botLP = _lastBotLP;
                    int oppLP = _lastOppLP;
                    
                    if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
                    {
                        botLP = Duel.Fields[0].LifePoints;
                        oppLP = Duel.Fields[1].LifePoints;
                    }
                    
                    string outcome = "Unknown";
                    if (botLP == 0 && oppLP > 0) outcome = "Loss";
                    else if (oppLP == 0 && botLP > 0) outcome = "Win";
                    else
                    {
                        // Match ended due to timeout, disconnect, or other reasons with LP > 0
                        if (botLP > oppLP + 3000) outcome = "WeakWin";
                        else if (oppLP > botLP + 3000) outcome = "WeakLoss";
                        else outcome = "Draw";
                    }
                    
                    LogToMatch(string.Format("Applying Real-time Learning: Outcome is {0} (Bot LP: {1}, Opp LP: {2}, Turns: {3})", outcome, botLP, oppLP, _turnCount));
                    
                    // ... [Rest of original adjustments logic, which safely uses outcome]
                    
                    SaveConfiguration();
                }
                catch (Exception ex)
                {
                    LogToMatch("Error applying real-time learning: " + ex.Message);
                }
            }
        }
```

### Propose Fix 4.6: Safer Lifecycle Hook Overrides & Delegations
Update the lifecycle hooks to update LP trackers, ensure null-safety on `Duel`, and include the missing `OnDraw` hook:

```csharp
        public override void OnNewTurn()
        {
            UpdateLastKnownLP();
            if (Duel == null) return;

            if (Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
            {
                if (Duel.Fields[0].LifePoints == 0 || Duel.Fields[1].LifePoints == 0)
                {
                    ApplyRealTimeLearning();
                }
            }

            _turnCount = Duel.Turn;

            LogToTurn(string.Format("=== Turn {0} Started (Active Player: {1}) ===", _turnCount, Duel.Player == 0 ? "Bot" : "Opponent"));
            
            if (Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
            {
                LogToTurn(string.Format("Bot LP: {0} | Opponent LP: {1}", Duel.Fields[0].LifePoints, Duel.Fields[1].LifePoints));
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
            base.OnNewTurn();
        }

        public override void OnNewPhase()
        {
            UpdateLastKnownLP();
            if (Duel != null)
            {
                LogToTurn("--- Phase Changed to: " + Duel.Phase.ToString() + " ---");
            }
            base.OnNewPhase();
        }

        public override void OnChaining(int player, ClientCard card)
        {
            UpdateLastKnownLP();
            if (card != null)
            {
                string activator = player == 0 ? "Bot" : "Opponent";
                string cardName = GetCardName(card.Id);
                LogToTurn(string.Format("Chain Event: {0} activated {1} (ID: {2})", activator, cardName, card.Id));

                if (player == 1)
                {
                    RecordOpponentCardSeen(card.Id);
                }

                if (Util != null)
                {
                    ClientCard lastChain = Util.GetLastChainCard();
                    if (lastChain != null && lastChain.Controller == 0)
                    {
                        if (player == 1)
                        {
                            if (!_disruptionsInMatch.ContainsKey(lastChain.Id))
                            {
                                _disruptionsInMatch[lastChain.Id] = new List<int>();
                            }
                            if (!_disruptionsInMatch[lastChain.Id].Contains(card.Id))
                            {
                                _disruptionsInMatch[lastChain.Id].Add(card.Id);
                            }

                            if (_deckConfig.choke_points != null && _deckConfig.choke_points.Contains(lastChain.Id))
                            {
                                LogToTurn(string.Format("WARNING: Opponent disrupted Bot's choke point [{0}] (ID: {1}) with [{2}] (ID: {3})!",
                                    GetCardName(lastChain.Id), lastChain.Id, cardName, card.Id));
                            }

                            double danger = CalculateCardDanger(card);
                            if (danger > 30.0)
                            {
                                if (_cardRegistry.ContainsKey(lastChain.Id))
                                {
                                    var meta = _cardRegistry[lastChain.Id];
                                    foreach (string plan in meta.combo_plans)
                                    {
                                        if (plan == _currentPlan)
                                        {
                                            if (!_blockedPlans.Contains(_currentPlan))
                                            {
                                                _blockedPlans.Add(_currentPlan);
                                                string nextPlan = GetNextPlan(_currentPlan);
                                                LogToTurn(string.Format("DISRUPTION DETECTED: Opponent disrupted our {0} using {1}. Shifting Combo Plan: {2} -> {3}!",
                                                    _currentPlan, GetCardName(card.Id), _currentPlan, nextPlan));
                                                _currentPlan = nextPlan;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            base.OnChaining(player, card);
        }

        public override void OnChainEnd()
        {
            UpdateLastKnownLP();
            LogToTurn("--- Chain resolution finished ---");
            if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
            {
                if (Duel.Fields[0].LifePoints == 0 || Duel.Fields[1].LifePoints == 0)
                {
                    ApplyRealTimeLearning();
                }
            }
            base.OnChainEnd();
        }

        public override void OnDraw(int player)
        {
            UpdateLastKnownLP();
            string drawer = player == 0 ? "Bot" : "Opponent";
            LogToTurn(string.Format("Draw Event: {0} drew a card.", drawer));
            base.OnDraw(player);
        }
```
