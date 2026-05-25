# Milestone 1 Audit & Analysis: C# Hooks and Safeguards

## 1. Executive Summary
This analysis details the results of a code audit of `BaseCustomExecutor.cs`, `UnifiedIgnisExecutor.cs`, and their compilation script `compile_ai.bat`. Multiple issues have been identified in lifecycle hook execution (null dereferences), process exit handlers (thread safety, memory leaks, lost updates), and real-time learning preconditions (failing to save learning on disconnects/timeouts). Safe, thread-safe, leak-free proposed fixes are provided below.

---

## 2. File Index and Observations

### Located Files & compilation:
1. **`c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`**: Contains the base AI logic, lifecycle hooks, and ApplyRealTimeLearning.
2. **`c:\Users\admin\Documents\EDOTh\WindBot\UnifiedIgnisExecutor.cs`**: Defines the custom executor class mappings inheriting from `BaseCustomExecutor`.
3. **`c:\Users\admin\Documents\EDOTh\WindBot\compile_ai.bat`**: Uses the .NET Framework C# compiler (`csc.exe`) referencing `System.Web.Extensions.dll` and `ExecutorBase.dll` to build `Executors\UnifiedIgnisExecutor.dll`.

---

## 3. Diagnosed Issues & Analysis

### Issue A: Lifecycle Hook Crash Risks & Missing `OnDraw` Hook
- **Problem**: The lifecycle hooks overridden in `BaseCustomExecutor.cs` (`OnNewTurn`, `OnNewPhase`, `OnSelectHand`, `OnBattle`, `OnSelectAttackTarget`, `OnSelectCard`, `OnChaining`, `OnChainEnd`) access properties of `Duel` and `Duel.Fields` directly without null/bounds validation. If the duel session is uninitialized, aborted, or disconnected, a `NullReferenceException` is thrown, causing the engine to crash.
- **Evidence**:
  - `OnNewTurn` (line 2370): `_turnCount = Duel.Turn;` and (line 2373) accessing `Duel.Fields[0].LifePoints` without validating if `Duel` or `Fields` is null.
  - `OnNewPhase` (line 2393): `Duel.Phase.ToString()` accessed without check.
- **Missing Hook**: The `OnDraw` hook (defined in the `Executor` base class as `public virtual void OnDraw(int player)`) is not overridden or safely wrapped in `BaseCustomExecutor.cs`, creating inconsistencies in hook logging and safeguards.

### Issue B: Process Exit Handler Thread-Safety, Memory Leaks, and Instance Loss
- **Problem**:
  1. **Thread-Safety**: Accessing and setting `_processExitRegistered` is not thread-safe. Concurrent initialization can cause multiple event registrations.
  2. **Memory Leak**: The static variable `_currentInstance` holds a strong reference to `BaseCustomExecutor`, preventing garbage collection of the entire AI engine/duel structure after the duel finishes.
  3. **Multi-Instance Loss**: Only the last created instance is tracked. If multiple duels run concurrently in the same process or sequentially without disposal, earlier instances will not have `ApplyRealTimeLearning()` executed when the process exits.

### Issue C: Preconditions in `ApplyRealTimeLearning()` Blocking Saves
- **Problem**: In `ApplyRealTimeLearning()`, a strict null-check on `Duel` and `Fields` returns immediately:
  ```csharp
  if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
  {
      return;
  }
  ```
  During match timeouts, sudden terminations, or disconnects, the duel object or fields might be cleaned up and set to null. Returning immediately discards all opponent card memory, disruptions, and learning data accumulated during that match.

### Issue D: File Concurrency and Lost Updates
- **Problem**: When multiple concurrent duel threads call `SaveConfiguration()`, they write to the same files (`opponent_memory.json`, `cards_registry_{deck}.json`). They overwrite each other's changes because they overwrite the file with their own private in-memory list without merging the latest contents from the disk.

---

## 4. Proposed Fixes & Implementation Details

To address the diagnosed issues safely, we propose the following modifications:

### Fix A: Safe Lifecycle Hooks wrapping
Introduce robust try-catch-finally wrappers for all lifecycle hooks and implement a safe `OnDraw` wrapper.

```csharp
        public override void OnNewTurn()
        {
            try
            {
                if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
                {
                    Log("OnNewTurn: Duel or Fields is null/incomplete. Skipping turn initialization.");
                    return;
                }

                if (Duel.Fields[0].LifePoints == 0 || Duel.Fields[1].LifePoints == 0)
                {
                    ApplyRealTimeLearning();
                }

                _turnCount = Duel.Turn;

                LogToTurn(string.Format("=== Turn {0} Started (Active Player: {1}) ===", _turnCount, Duel.Player == 0 ? "Bot" : "Opponent"));
                LogToTurn(string.Format("Bot LP: {0} | Opponent LP: {1}", Duel.Fields[0].LifePoints, Duel.Fields[1].LifePoints));
                
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
            catch (Exception ex)
            {
                Log("Error in OnNewTurn hook: " + ex.Message);
            }
        }

        public override void OnNewPhase()
        {
            try
            {
                if (Duel == null) return;
                LogToTurn("--- Phase Changed to: " + Duel.Phase.ToString() + " ---");
                base.OnNewPhase();
            }
            catch (Exception ex)
            {
                Log("Error in OnNewPhase hook: " + ex.Message);
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
                LogToTurn("Selecting to go second.");
                return false;
            }
            catch (Exception ex)
            {
                Log("Error in OnSelectHand hook: " + ex.Message);
                return base.OnSelectHand();
            }
        }

        public override void OnDraw(int player)
        {
            try
            {
                if (Duel == null) return;
                string drawer = player == 0 ? "Bot" : "Opponent";
                LogToTurn(string.Format("OnDraw: {0} drew a card.", drawer));
                base.OnDraw(player);
            }
            catch (Exception ex)
            {
                Log("Error in OnDraw hook: " + ex.Message);
            }
        }
```

### Fix B: Thread-Safe, Leak-Free, Multi-Instance Process Exit Handler
Replace the strong static `_currentInstance` with a thread-safe list of `WeakReference<BaseCustomExecutor>` to prevent memory leaks and handle concurrent instances.

#### In the Static Fields Section of `BaseCustomExecutor.cs`:
```csharp
        protected static readonly List<WeakReference<BaseCustomExecutor>> _activeInstances = new List<WeakReference<BaseCustomExecutor>>();
        protected static readonly object _staticLock = new object();
        protected static bool _processExitRegistered = false;
```

#### In the Constructor:
```csharp
            // Register active instance thread-safely
            lock (_staticLock)
            {
                _activeInstances.Add(new WeakReference<BaseCustomExecutor>(this));
                if (!_processExitRegistered)
                {
                    AppDomain.CurrentDomain.ProcessExit += StaticOnProcessExit;
                    AppDomain.CurrentDomain.DomainUnload += StaticOnProcessExit;
                    _processExitRegistered = true;
                }
            }
```

#### In `Dispose(bool disposing)`:
```csharp
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                ApplyRealTimeLearning();
                
                lock (_staticLock)
                {
                    _activeInstances.RemoveAll(wr => {
                        BaseCustomExecutor target;
                        return !wr.TryGetTarget(out target) || target == this;
                    });
                }
                ...
```

#### Refactored Process Exit Handler:
```csharp
        private static void StaticOnProcessExit(object sender, EventArgs e)
        {
            List<BaseCustomExecutor> instancesToSave = new List<BaseCustomExecutor>();
            lock (_staticLock)
            {
                foreach (var wr in _activeInstances)
                {
                    BaseCustomExecutor target;
                    if (wr.TryGetTarget(out target))
                    {
                        instancesToSave.Add(target);
                    }
                }
            }

            foreach (var instance in instancesToSave)
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

### Fix C: Relax Preconditions in `ApplyRealTimeLearning()` and Concurrency Merge
Enable saving when `Duel` is null by decoupling the save and decay logic from active duel objects, and perform thread-safe merging of json logs with disk configuration.

#### Refactored `ApplyRealTimeLearning()`:
```csharp
        protected void ApplyRealTimeLearning()
        {
            if (_learningApplied) return;
            _learningApplied = true;

            try
            {
                bool hasDuelInfo = (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null);
                int botLP = hasDuelInfo ? Duel.Fields[0].LifePoints : 8000;
                int oppLP = hasDuelInfo ? Duel.Fields[1].LifePoints : 8000;
                
                string outcome = "Unknown";
                if (hasDuelInfo)
                {
                    if (botLP == 0 && oppLP > 0) outcome = "Loss";
                    else if (oppLP == 0 && botLP > 0) outcome = "Win";
                    else if (_ourCardsPlayed.Count > 0)
                    {
                        if (botLP > oppLP + 3000) outcome = "WeakWin";
                        else if (oppLP > botLP + 3000) outcome = "WeakLoss";
                        else outcome = "Draw";
                    }
                }
                else
                {
                    outcome = "Draw"; // Disconnect/sudden close
                }
                
                LogToMatch(string.Format("Applying Real-time Learning: Outcome is {0} (Bot LP: {1}, Opp LP: {2}, Turns: {3})", outcome, botLP, oppLP, _turnCount));
                
                // [Learning adjustment loops for _ourCardsPlayed, decay, and hard capping remain here]
                // ...

                SaveConfiguration();
            }
            catch (Exception ex)
            {
                LogToMatch("Error applying real-time learning: " + ex.Message);
            }
        }
```

#### Refactored `SaveConfiguration()` with Concurrency Merge:
```csharp
        protected void SaveConfiguration()
        {
            lock (_staticLock)
            {
                try
                {
                    string baseDir = !string.IsNullOrEmpty(_resolvedBaseDir) ? _resolvedBaseDir : AppDomain.CurrentDomain.BaseDirectory;
                    string deckRegistryName = "cards_registry_" + _resolvedDeckName + ".json";
                    string registryPath = Path.Combine(baseDir, "config", deckRegistryName);
                    string oppMemoryPath = Path.Combine(baseDir, "config", "opponent_memory.json");

                    var serializer = new JavaScriptSerializer();

                    // Load & Merge cards_registry_{deck}.json from disk
                    var diskRegistry = new Dictionary<int, CardMetadata>();
                    if (File.Exists(registryPath))
                    {
                        try
                        {
                            string diskRegJson = ReadFileWithRetry(registryPath);
                            var rawListDisk = serializer.Deserialize<List<Dictionary<string, object>>>(diskRegJson);
                            if (rawListDisk != null)
                            {
                                foreach (var item in rawListDisk)
                                {
                                    if (item == null || !item.ContainsKey("id")) continue;
                                    int cardId = Convert.ToInt32(item["id"]);
                                    var card = new CardMetadata
                                    {
                                        id = cardId,
                                        priority = GetIntOrDefault(item, "priority", 5),
                                        risk_if_negated = GetIntOrDefault(item, "risk_if_negated", 0),
                                        bait_value = GetIntOrDefault(item, "bait_value", 0),
                                        followup_value = GetIntOrDefault(item, "followup_value", 0),
                                        recovery_value = GetIntOrDefault(item, "recovery_value", 0)
                                    };
                                    card.q_values = new Dictionary<string, object>();
                                    if (item.ContainsKey("q_values") && item["q_values"] is Dictionary<string, object>)
                                    {
                                        var rawQ = item["q_values"] as Dictionary<string, object>;
                                        foreach (var kvp in rawQ)
                                            card.q_values[kvp.Key] = kvp.Value;
                                    }
                                    card.roles = new ArrayList();
                                    if (item.ContainsKey("roles") && item["roles"] is IEnumerable && !(item["roles"] is string))
                                    {
                                        foreach (var r in (IEnumerable)item["roles"])
                                            card.roles.Add(r.ToString());
                                    }
                                    card.combo_plans = new ArrayList();
                                    if (item.ContainsKey("combo_plans") && item["combo_plans"] is IEnumerable && !(item["combo_plans"] is string))
                                    {
                                        foreach (var p in (IEnumerable)item["combo_plans"])
                                            card.combo_plans.Add(p.ToString());
                                    }
                                    diskRegistry[cardId] = card;
                                }
                            }
                        }
                        catch (Exception ex) { LogToMatch("Error merging disk cards registry: " + ex.Message); }
                    }

                    // Update loaded list with our in-memory data
                    foreach (var kvp in _cardRegistry)
                    {
                        diskRegistry[kvp.Key] = kvp.Value;
                    }

                    var regList = new List<Dictionary<string, object>>();
                    foreach (var kvp in diskRegistry)
                    {
                        var card = kvp.Value;
                        var dict = new Dictionary<string, object>();
                        dict["id"] = card.id;
                        dict["roles"] = card.roles;
                        dict["priority"] = card.priority;
                        dict["risk_if_negated"] = card.risk_if_negated;
                        dict["bait_value"] = card.bait_value;
                        dict["followup_value"] = card.followup_value;
                        dict["recovery_value"] = card.recovery_value;
                        dict["combo_plans"] = card.combo_plans;
                        dict["q_values"] = card.q_values != null ? card.q_values : new Dictionary<string, object>();
                        regList.Add(dict);
                    }
                    string regJson = serializer.Serialize(regList);

                    // Safety Backup
                    string backupPath = registryPath + ".bak";
                    try { if (File.Exists(registryPath)) File.Copy(registryPath, backupPath, true); } catch {}

                    WriteFileWithRetry(registryPath, regJson);

                    // Load & Merge opponent_memory.json from disk
                    var diskOppMemory = new Dictionary<int, OpponentCardMeta>();
                    if (File.Exists(oppMemoryPath))
                    {
                        try
                        {
                            string diskOppJson = ReadFileWithRetry(oppMemoryPath);
                            var rawDictDisk = serializer.Deserialize<Dictionary<string, object>>(diskOppJson);
                            if (rawDictDisk != null)
                            {
                                foreach (var kvp in rawDictDisk)
                                {
                                    int id;
                                    if (int.TryParse(kvp.Key, out id))
                                    {
                                        var metaDict = kvp.Value as Dictionary<string, object>;
                                        if (metaDict != null)
                                        {
                                            var oppCard = new OpponentCardMeta
                                            {
                                                name = metaDict.ContainsKey("name") ? metaDict["name"].ToString() : "Unknown Card",
                                                times_seen = metaDict.ContainsKey("times_seen") ? Convert.ToInt32(metaDict["times_seen"]) : 0,
                                                times_disrupted_us = metaDict.ContainsKey("times_disrupted_us") ? Convert.ToInt32(metaDict["times_disrupted_us"]) : 0,
                                                learned_danger = metaDict.ContainsKey("learned_danger") ? Convert.ToDouble(metaDict["learned_danger"]) : 0.0
                                            };
                                            diskOppMemory[id] = oppCard;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { LogToMatch("Error merging disk opponent memory: " + ex.Message); }
                    }

                    // Merge our current opponent memory
                    foreach (var kvp in _opponentMemory)
                    {
                        if (diskOppMemory.ContainsKey(kvp.Key))
                        {
                            var diskMeta = diskOppMemory[kvp.Key];
                            var ourMeta = kvp.Value;
                            diskMeta.times_seen += ourMeta.times_seen;
                            diskMeta.times_disrupted_us += ourMeta.times_disrupted_us;
                            diskMeta.learned_danger = Math.Max(diskMeta.learned_danger, ourMeta.learned_danger);
                        }
                        else
                        {
                            diskOppMemory[kvp.Key] = kvp.Value;
                        }
                    }

                    var oppDict = new Dictionary<string, object>();
                    foreach (var kvp in diskOppMemory)
                    {
                        var oppMeta = kvp.Value;
                        var dict = new Dictionary<string, object>();
                        dict["name"] = oppMeta.name;
                        dict["times_seen"] = oppMeta.times_seen;
                        dict["times_disrupted_us"] = oppMeta.times_disrupted_us;
                        dict["learned_danger"] = oppMeta.learned_danger;
                        oppDict[kvp.Key.ToString()] = dict;
                    }
                    string oppJson = serializer.Serialize(oppDict);
                    WriteFileWithRetry(oppMemoryPath, oppJson);
                }
                catch (Exception ex)
                {
                    LogToMatch("Error saving configuration: " + ex.Message);
                }
            }
        }
```
