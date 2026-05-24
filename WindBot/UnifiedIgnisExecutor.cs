using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper.Enums;

namespace ProjectIgnisAI
{
    // Register the deck name and its corresponding .ydk filename
    [Deck("UnifiedIgnis", "AI_CustomIgnis")]
    public class UnifiedIgnisExecutor : DefaultExecutor
    {
        public class CardMetadata
        {
            public int id { get; set; }
            public ArrayList roles { get; set; }
            public int priority { get; set; }
            public int risk_if_negated { get; set; }
            public int bait_value { get; set; }
            public int followup_value { get; set; }
            public int recovery_value { get; set; }
            public ArrayList combo_plans { get; set; }
            public Dictionary<string, object> q_values { get; set; }
        }

        public class OpponentCardMeta
        {
            public string name { get; set; }
            public int times_seen { get; set; }
            public int times_disrupted_us { get; set; }
            public double learned_danger { get; set; }
        }

        public class DeckIdentity
        {
            public string playstyle { get; set; }
            public ArrayList goals { get; set; }
            public ArrayList choke_points { get; set; }
            public ArrayList weaknesses { get; set; }
        }

        private Dictionary<int, CardMetadata> _cardRegistry = new Dictionary<int, CardMetadata>();
        private Dictionary<int, OpponentCardMeta> _opponentMemory = new Dictionary<int, OpponentCardMeta>();
        private Dictionary<int, string> _cardNames = new Dictionary<int, string>();
        private DeckIdentity _deckConfig = new DeckIdentity();
        private string _resolvedDeckName = "";
        private string _currentGoal = "establish_interruptions";
        private string _currentPlan = "PlanA";
        private List<string> _blockedPlans = new List<string>();

        // In-game played cards and disruptions for learning
        private List<int> _ourCardsPlayed = new List<int>();
        private Dictionary<int, List<int>> _disruptionsInMatch = new Dictionary<int, List<int>>();
        private bool _learningApplied = false;
        
        // Logging State Fields
        private string _matchLogDir = "";
        private string _generalLogPath = "";
        private string _currentTurnLogPath = "";
        private string _decisionsLogPath = "";
        private int _turnCount = 0;
        
        // Deduplication for decisions.jsonl — prevents logging the same evaluation twice
        private HashSet<string> _loggedDecisionKeys = new HashSet<string>();

        private bool IsLethalOnBoard()
        {
            if (Duel.Phase != DuelPhase.Main1) return false;

            if (Enemy.GetMonsterCount() == 0)
            {
                int totalAtk = 0;
                foreach (var card in Bot.GetMonsters())
                {
                    if (card != null && card.IsFaceup() && card.IsAttack() && !card.IsDisabled() && !card.Attacked)
                    {
                        totalAtk += card.Attack;
                    }
                }
                return totalAtk >= Enemy.LifePoints;
            }
            return false;
        }

        private static UnifiedIgnisExecutor _currentInstance = null;
        private static bool _processExitRegistered = false;
        private static readonly Random _random = new Random();

        public UnifiedIgnisExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Resolve deck name from attribute
            object[] attrs = this.GetType().GetCustomAttributes(typeof(DeckAttribute), true);
            if (attrs.Length > 0)
            {
                DeckAttribute deckAttr = (DeckAttribute)attrs[0];
                _resolvedDeckName = deckAttr.Name;
            }
            if (string.IsNullOrEmpty(_resolvedDeckName))
            {
                _resolvedDeckName = Deck; // Fallback
            }

            Log("Initializing Dynamic AI Engine for deck: " + _resolvedDeckName);

            // Load Registries, Identity & Card Names
            LoadConfiguration();

            // Set up Folder Logging
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string matchId = Guid.NewGuid().ToString().Substring(0, 8);
            _matchLogDir = Path.Combine(baseDir, "Logs", _resolvedDeckName + "_" + timeStamp + "_" + matchId);
            
            try
            {
                Directory.CreateDirectory(_matchLogDir);
                _generalLogPath = Path.Combine(_matchLogDir, "match_summary.log");
                _decisionsLogPath = Path.Combine(_matchLogDir, "decisions.jsonl");
                LogToMatch("=== Match Started ===");
                LogToMatch("Deck: " + _resolvedDeckName);
                LogToMatch("Playstyle: " + _deckConfig.playstyle);
                LogToMatch("Time: " + DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                Log("Failed to create log directory: " + ex.Message);
            }

            // Register card-specific logic dynamically from metadata registry
            foreach (var cardMeta in _cardRegistry.Values)
            {
                int cardId = cardMeta.id;
                Log("Dynamically registering executors for Card ID: " + cardId + " (Priority: " + cardMeta.priority + ")");

                // Register hooks for Activation, Normal Summon, and Special Summon
                AddExecutor(ExecutorType.Activate, cardId, () => OnCardAction(cardId, ExecutorType.Activate));
                AddExecutor(ExecutorType.Summon, cardId, () => OnCardAction(cardId, ExecutorType.Summon));
                AddExecutor(ExecutorType.SpSummon, cardId, () => OnCardAction(cardId, ExecutorType.SpSummon));
            }

            // Register fallback catch-all executors
            AddExecutor(ExecutorType.Activate, OnDefaultActivate);
            AddExecutor(ExecutorType.Summon, OnDefaultSummon);
            AddExecutor(ExecutorType.SpSummon, OnDefaultSpSummon);
            AddExecutor(ExecutorType.SpellSet, OnDefaultSpellSet);
            AddExecutor(ExecutorType.Repos, OnDefaultRepos);
            AddExecutor(ExecutorType.MonsterSet, OnDefaultMonsterSet);

            // Register ProcessExit handler to save learning data safely on exit
            _currentInstance = this;
            if (!_processExitRegistered)
            {
                AppDomain.CurrentDomain.ProcessExit += StaticOnProcessExit;
                _processExitRegistered = true;
            }
        }

        private void Log(string message)
        {
            Console.WriteLine("[IgnisEngine] " + message);
        }

        private void LogToMatch(string message)
        {
            if (string.IsNullOrEmpty(_generalLogPath)) return;
            try
            {
                File.AppendAllText(_generalLogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
            }
            catch {}
        }

        private void LogToTurn(string message)
        {
            if (string.IsNullOrEmpty(_matchLogDir)) return;
            if (string.IsNullOrEmpty(_currentTurnLogPath))
            {
                _currentTurnLogPath = Path.Combine(_matchLogDir, "turn_" + _turnCount + ".log");
            }
            try
            {
                File.AppendAllText(_currentTurnLogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
            }
            catch {}
        }

        private void LogDecision(int cardId, string action, string goal, double score, bool decision, string plan)
        {
            if (string.IsNullOrEmpty(_decisionsLogPath)) return;
            try
            {
                // Deduplicate: same turn + card_id + action combination only logged once
                string dedupKey = string.Format("{0}_{1}_{2}", _turnCount, cardId, action);
                if (_loggedDecisionKeys.Contains(dedupKey)) return;
                _loggedDecisionKeys.Add(dedupKey);
                
                string json = string.Format(
                    "{{\"turn\":{0},\"card_id\":{1},\"card_name\":\"{2}\",\"action\":\"{3}\",\"goal\":\"{4}\",\"score\":{5:F1},\"decision\":{6},\"plan\":\"{7}\",\"lp_self\":{8},\"lp_opp\":{9}}}",
                    _turnCount, cardId, GetCardName(cardId).Replace("\"", "'"), action, goal, score,
                    decision ? "true" : "false", plan,
                    Duel.Fields[0].LifePoints, Duel.Fields[1].LifePoints);
                File.AppendAllText(_decisionsLogPath, json + Environment.NewLine);
            }
            catch {}
        }

        private string GetCardName(int id)
        {
            if (_cardNames.ContainsKey(id))
                return _cardNames[id];
            return "Unknown Card (" + id + ")";
        }

        private string ReadFileWithRetry(string filePath)
        {
            int retries = 10;
            int delay = 100; // ms
            while (true)
            {
                try
                {
                    return File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                }
                catch (IOException)
                {
                    if (--retries == 0) throw;
                    System.Threading.Thread.Sleep(_random.Next(delay, delay * 2));
                }
            }
        }

        private void WriteFileWithRetry(string filePath, string content)
        {
            int retries = 10;
            int delay = 100; // ms
            while (true)
            {
                try
                {
                    File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
                    return;
                }
                catch (IOException)
                {
                    if (--retries == 0) throw;
                    System.Threading.Thread.Sleep(_random.Next(delay, delay * 2));
                }
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string deckRegistryName = "cards_registry_" + _resolvedDeckName + ".json";
                string registryPath = Path.Combine(baseDir, "config", deckRegistryName);
                if (!File.Exists(registryPath))
                {
                    registryPath = Path.Combine(baseDir, "config", "cards_registry.json");
                }
                
                if (!File.Exists(registryPath))
                {
                    string assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string parentDir = Path.GetDirectoryName(assemblyDir);
                    string deckRegistryPathAssembly = Path.Combine(parentDir, "config", deckRegistryName);
                    if (File.Exists(deckRegistryPathAssembly))
                    {
                        baseDir = parentDir;
                        registryPath = deckRegistryPathAssembly;
                    }
                    else if (File.Exists(Path.Combine(parentDir, "config", "cards_registry.json")))
                    {
                        baseDir = parentDir;
                        registryPath = Path.Combine(parentDir, "config", "cards_registry.json");
                    }
                }
                
                // 1. Load general cards registry
                if (File.Exists(registryPath))
                {
                    string json = ReadFileWithRetry(registryPath);
                    var serializer = new JavaScriptSerializer();
                    var rawList = serializer.Deserialize<List<Dictionary<string, object>>>(json);
                    foreach (var item in rawList)
                    {
                        var card = new CardMetadata
                        {
                            id = (int)item["id"],
                            priority = (int)item["priority"],
                            risk_if_negated = (int)item["risk_if_negated"],
                            bait_value = (int)item["bait_value"],
                            followup_value = (int)item["followup_value"],
                            recovery_value = (int)item["recovery_value"]
                        };
                        
                        card.q_values = new Dictionary<string, object>();
                        if (item.ContainsKey("q_values") && item["q_values"] is Dictionary<string, object>)
                        {
                            var rawQ = item["q_values"] as Dictionary<string, object>;
                            foreach (var kvp in rawQ)
                                card.q_values[kvp.Key] = kvp.Value;
                        }

                        card.roles = new ArrayList();
                        if (item.ContainsKey("roles") && item["roles"] != null)
                        {
                            if (item["roles"] is IEnumerable && !(item["roles"] is string))
                            {
                                foreach (var r in (IEnumerable)item["roles"])
                                    card.roles.Add(r.ToString());
                            }
                        }

                        card.combo_plans = new ArrayList();
                        if (item.ContainsKey("combo_plans") && item["combo_plans"] != null)
                        {
                            if (item["combo_plans"] is IEnumerable && !(item["combo_plans"] is string))
                            {
                                foreach (var p in (IEnumerable)item["combo_plans"])
                                    card.combo_plans.Add(p.ToString());
                            }
                            else
                            {
                                card.combo_plans.Add("PlanA");
                            }
                        }
                        else
                        {
                            card.combo_plans.Add("PlanA");
                        }

                        _cardRegistry[card.id] = card;
                    }
                    Log("Successfully loaded " + _cardRegistry.Count + " cards from registry.");
                }
                else
                {
                    Log("Warning: registry not found at " + registryPath);
                }

                // 2. Load card names
                string namesPath = Path.Combine(baseDir, "config", "card_names.json");
                if (File.Exists(namesPath))
                {
                    string json = ReadFileWithRetry(namesPath);
                    var serializer = new JavaScriptSerializer();
                    var rawNames = serializer.Deserialize<Dictionary<string, object>>(json);
                    foreach (var kvp in rawNames)
                    {
                        int id;
                        if (int.TryParse(kvp.Key, out id))
                        {
                            _cardNames[id] = kvp.Value.ToString();
                        }
                    }
                    Log("Successfully loaded " + _cardNames.Count + " card names.");
                }
                else
                {
                    Log("Warning: card_names.json not found at " + namesPath);
                }

                // 3. Load deck specific config
                string deckConfigPath = Path.Combine(baseDir, "config", "decks", _resolvedDeckName + ".json");
                if (File.Exists(deckConfigPath))
                {
                    string json = ReadFileWithRetry(deckConfigPath);
                    var serializer = new JavaScriptSerializer();
                    var rawDict = serializer.Deserialize<Dictionary<string, object>>(json);
                    
                    _deckConfig.playstyle = rawDict["playstyle"].ToString();
                    
                    _deckConfig.goals = new ArrayList();
                    if (rawDict.ContainsKey("goals") && rawDict["goals"] is IEnumerable && !(rawDict["goals"] is string))
                    {
                        var rawGoals = rawDict["goals"] as IEnumerable;
                        foreach (var g in rawGoals)
                            _deckConfig.goals.Add(g.ToString());
                    }

                    _deckConfig.choke_points = new ArrayList();
                    if (rawDict.ContainsKey("choke_points") && rawDict["choke_points"] is IEnumerable && !(rawDict["choke_points"] is string))
                    {
                        var rawChokes = rawDict["choke_points"] as IEnumerable;
                        foreach (var c in rawChokes)
                            _deckConfig.choke_points.Add(Convert.ToInt32(c));
                    }

                    _deckConfig.weaknesses = new ArrayList();
                    if (rawDict.ContainsKey("weaknesses") && rawDict["weaknesses"] is IEnumerable && !(rawDict["weaknesses"] is string))
                    {
                        var rawWeaknesses = rawDict["weaknesses"] as IEnumerable;
                        foreach (var w in rawWeaknesses)
                            _deckConfig.weaknesses.Add(w.ToString());
                    }

                    Log("Successfully loaded deck config for " + _resolvedDeckName + " (Playstyle: " + _deckConfig.playstyle + ")");
                }
                else
                {
                    Log("Warning: deck config not found at " + deckConfigPath);
                }

                // 4. Load opponent memory
                string oppMemoryPath = Path.Combine(baseDir, "config", "opponent_memory.json");
                if (File.Exists(oppMemoryPath))
                {
                    string json = ReadFileWithRetry(oppMemoryPath);
                    var serializer = new JavaScriptSerializer();
                    var rawDict = serializer.Deserialize<Dictionary<string, object>>(json);
                    foreach (var kvp in rawDict)
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
                                _opponentMemory[id] = oppCard;
                            }
                        }
                    }
                    Log("Successfully loaded " + _opponentMemory.Count + " opponent memory profiles.");
                }
            }
            catch (Exception ex)
            {
                Log("Error loading configuration: " + ex.Message);
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string deckRegistryName = "cards_registry_" + _resolvedDeckName + ".json";
                string registryPath = Path.Combine(baseDir, "config", deckRegistryName);
                string oppMemoryPath = Path.Combine(baseDir, "config", "opponent_memory.json");
                
                if (!File.Exists(registryPath))
                {
                    string assemblyDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string parentDir = Path.GetDirectoryName(assemblyDir);
                    string deckRegistryPathAssembly = Path.Combine(parentDir, "config", deckRegistryName);
                    if (File.Exists(deckRegistryPathAssembly))
                    {
                        baseDir = parentDir;
                        registryPath = deckRegistryPathAssembly;
                        oppMemoryPath = Path.Combine(parentDir, "config", "opponent_memory.json");
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(parentDir, "config", "cards_registry.json")))
                        {
                            baseDir = parentDir;
                        }
                        registryPath = Path.Combine(baseDir, "config", deckRegistryName);
                        oppMemoryPath = Path.Combine(baseDir, "config", "opponent_memory.json");
                    }
                }

                var serializer = new JavaScriptSerializer();

                // 1. Serialize and save cards_registry_{deck}.json
                var regList = new List<Dictionary<string, object>>();
                foreach (var kvp in _cardRegistry)
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

                // Safety Backup: create .bak before overwriting registry
                string backupPath = registryPath + ".bak";
                try { if (File.Exists(registryPath)) File.Copy(registryPath, backupPath, true); } catch {}

                WriteFileWithRetry(registryPath, regJson);
                LogToMatch("Saved " + regList.Count + " cards to " + registryPath + " (backup: " + backupPath + ")");

                // 2. Serialize and save opponent_memory.json
                var oppDict = new Dictionary<string, object>();
                foreach (var kvp in _opponentMemory)
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
                LogToMatch("Saved " + oppDict.Count + " opponent cards to " + oppMemoryPath);
            }
            catch (Exception ex)
            {
                LogToMatch("Error saving configuration: " + ex.Message);
            }
        }

        private void RecordOpponentCardSeen(int cardId)
        {
            if (cardId <= 0) return;
            if (!_opponentMemory.ContainsKey(cardId))
            {
                _opponentMemory[cardId] = new OpponentCardMeta
                {
                    name = GetCardName(cardId),
                    times_seen = 0,
                    times_disrupted_us = 0,
                    learned_danger = 10.0 // Default initial learned danger
                };
            }
            _opponentMemory[cardId].times_seen++;
        }

        private void ApplyRealTimeLearning()
        {
            if (_learningApplied) return;
            _learningApplied = true;

            try
            {
                int botLP = Duel.Fields[0].LifePoints;
                int oppLP = Duel.Fields[1].LifePoints;
                
                string outcome = "Unknown";
                if (botLP == 0 && oppLP > 0) outcome = "Loss";
                else if (oppLP == 0 && botLP > 0) outcome = "Win";
                else if (_turnCount >= 3 && _ourCardsPlayed.Count > 0)
                {
                    // Match likely timed out or disconnected — apply partial learning
                    // based on LP difference as a proxy for performance
                    if (botLP > oppLP + 3000) outcome = "WeakWin";
                    else if (oppLP > botLP + 3000) outcome = "WeakLoss";
                    else outcome = "Draw";
                }
                
                LogToMatch(string.Format("Applying Real-time Learning: Outcome is {0} (Bot LP: {1}, Opp LP: {2}, Turns: {3})", outcome, botLP, oppLP, _turnCount));
                
                // Adjust our cards
                foreach (int cardId in _ourCardsPlayed)
                {
                    if (_cardRegistry.ContainsKey(cardId))
                    {
                        var meta = _cardRegistry[cardId];
                        int oldPriority = meta.priority;
                        int oldRisk = meta.risk_if_negated;
                        int oldBait = meta.bait_value;
                        int oldFollowup = meta.followup_value;
                        
                        if (outcome == "Win" || outcome == "WeakWin")
                        {
                            // Smart Reward: only boost priority for key contributors (starters, payoffs, searchers)
                            // This prevents all-card inflation where every played card drifts to priority 10
                            if (meta.roles.Contains("starter") || meta.roles.Contains("payoff") || meta.roles.Contains("searcher"))
                            {
                                int delta = (outcome == "Win") ? 1 : 0;
                                if (outcome == "WeakWin" && meta.priority < 8) delta = 1;
                                meta.priority = Math.Min(10, meta.priority + delta);
                            }
                            if (_turnCount >= 2 && (meta.roles.Contains("extender") || meta.roles.Contains("combo_piece")))
                            {
                                int delta = (outcome == "Win") ? 1 : 0;
                                meta.followup_value = Math.Min(10, meta.followup_value + delta);
                            }
                        }
                        else if (outcome == "Loss" || outcome == "WeakLoss")
                        {
                            int delta = (outcome == "Loss") ? 1 : 0;
                            if (meta.priority > 1 && (outcome == "WeakLoss" && meta.priority > 3)) delta = 1;
                            meta.priority = Math.Max(1, meta.priority - delta);
                            if (_disruptionsInMatch.ContainsKey(cardId) && _disruptionsInMatch[cardId].Count > 0)
                            {
                                meta.risk_if_negated = Math.Min(10, meta.risk_if_negated + 1);
                                foreach (var otherId in _cardRegistry.Keys)
                                {
                                    if (otherId != cardId)
                                    {
                                        var otherMeta = _cardRegistry[otherId];
                                        if (!otherMeta.roles.Contains("starter") && !otherMeta.roles.Contains("payoff"))
                                        {
                                            if (otherMeta.bait_value > 0 && otherMeta.bait_value < 6)
                                            {
                                                otherMeta.bait_value++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (outcome == "Draw")
                        {
                            // Draw: mild decay on high-priority cards not played, no boost
                            // This prevents inflation from drawn-out matches
                            if (meta.priority >= 9)
                            {
                                meta.priority = Math.Max(6, meta.priority - 1);
                            }
                        }

                        if (meta.priority != oldPriority || meta.risk_if_negated != oldRisk || meta.followup_value != oldFollowup || meta.bait_value != oldBait)
                        {
                            LogToMatch(string.Format("  Card {0} ({1}) adjusted: priority {2}->{3}, risk {4}->{5}, followup {6}->{7}, bait {8}->{9}",
                                cardId, GetCardName(cardId), oldPriority, meta.priority, oldRisk, meta.risk_if_negated, oldFollowup, meta.followup_value, oldBait, meta.bait_value));
                        }
                    }
                }

                // Anti-Inflation Decay: reduce priority for high-priority cards NOT played in this match
                // This counteracts the natural upward drift from repeated wins
                foreach (var kvpDecay in _cardRegistry)
                {
                    var decayCard = kvpDecay.Value;
                    if (!_ourCardsPlayed.Contains(kvpDecay.Key) && decayCard.priority >= 8)
                    {
                        int oldP = decayCard.priority;
                        decayCard.priority = Math.Max(5, decayCard.priority - 1);
                        if (oldP != decayCard.priority)
                        {
                            LogToMatch(string.Format("  Decay: Card {0} ({1}) priority {2}->{3} (not played, anti-inflation)",
                                kvpDecay.Key, GetCardName(kvpDecay.Key), oldP, decayCard.priority));
                        }
                    }
                }

                // Hard Cap: Prevent any card from exceeding priority 8 via learning
                foreach (var kvpCap in _cardRegistry)
                {
                    if (kvpCap.Value.priority > 8)
                    {
                        LogToMatch(string.Format("  Hard Cap: Card {0} ({1}) priority capped from {2} to 8",
                            kvpCap.Key, GetCardName(kvpCap.Key), kvpCap.Value.priority));
                        kvpCap.Value.priority = 8;
                    }
                }

                // Adjust opponent memory based on disruptions
                foreach (var kvp in _disruptionsInMatch)
                {
                    int ourCardId = kvp.Key;
                    List<int> oppCardIds = kvp.Value;
                    bool isOurChokePoint = _deckConfig.choke_points.Contains(ourCardId);
                    
                    foreach (int oppId in oppCardIds)
                    {
                        RecordOpponentCardSeen(oppId);
                        var oppMeta = _opponentMemory[oppId];
                        oppMeta.times_disrupted_us++;
                        
                        double oldDanger = oppMeta.learned_danger;
                        double dangerInc = 5.0;
                        if (isOurChokePoint) dangerInc += 10.0;
                        if (outcome == "Loss" || outcome == "WeakLoss") dangerInc += 15.0;
                        if (outcome == "Draw") dangerInc += 5.0; // Draws still teach us something
                        
                        oppMeta.learned_danger = Math.Min(100.0, oppMeta.learned_danger + dangerInc);
                        
                        LogToMatch(string.Format("  Opponent card {0} ({1}) marked dangerous: times_disrupted {2}, danger {3:F1}->{4:F1}",
                            oppId, oppMeta.name, oppMeta.times_disrupted_us, oldDanger, oppMeta.learned_danger));
                    }
                }

                // Natural Decay: gradually reduce learned_danger for ALL opponent cards
                // This prevents danger scores from accumulating infinitely over many matches
                foreach (var kvpOppDecay in _opponentMemory)
                {
                    double prevDanger = kvpOppDecay.Value.learned_danger;
                    kvpOppDecay.Value.learned_danger = Math.Max(5.0, kvpOppDecay.Value.learned_danger * 0.95);
                    if (prevDanger - kvpOppDecay.Value.learned_danger > 0.5)
                    {
                        LogToMatch(string.Format("  Decay: Opponent card {0} ({1}) danger {2:F1}->{3:F1} (natural decay)",
                            kvpOppDecay.Key, kvpOppDecay.Value.name, prevDanger, kvpOppDecay.Value.learned_danger));
                    }
                }

                SaveConfiguration();
            }
            catch (Exception ex)
            {
                LogToMatch("Error applying real-time learning: " + ex.Message);
            }
        }

        // --- Core Decision Engine ---
        private double CalculateTotalDangerForField()
        {
            double total = 0.0;
            foreach (var m in Duel.Fields[1].MonsterZone)
            {
                if (m != null) total += CalculateCardDanger(m);
            }
            foreach (var s in Duel.Fields[1].SpellZone)
            {
                if (s != null) total += CalculateCardDanger(s);
            }
            // Check Graveyard danger
            foreach (var g in Duel.Fields[1].Graveyard)
            {
                if (g != null)
                {
                    double danger = CalculateCardDanger(g);
                    if (_cardRegistry.ContainsKey(g.Id))
                    {
                        var meta = _cardRegistry[g.Id];
                        if (meta.roles.Contains("recovery") || meta.roles.Contains("starter") || meta.roles.Contains("extender") || meta.roles.Contains("payoff"))
                        {
                            total += danger * 0.5;
                        }
                    }
                    else if (GetStapleBaselineDanger(g.Id) > 0)
                    {
                        total += danger * 0.3;
                    }
                }
            }
            // Check Hand danger (revealed/visible cards only)
            foreach (var h in Duel.Fields[1].Hand)
            {
                if (h != null && h.Id > 0)
                {
                    double danger = CalculateCardDanger(h);
                    if (_cardRegistry.ContainsKey(h.Id))
                    {
                        var meta = _cardRegistry[h.Id];
                        if (meta.roles.Contains("handtrap") || meta.roles.Contains("interruption") || meta.roles.Contains("disruption"))
                        {
                            total += danger;
                        }
                    }
                    else if (GetStapleBaselineDanger(h.Id) > 0)
                    {
                        total += danger;
                    }
                }
            }
            return total;
        }

        private void UpdateGoal()
        {
            int selfLP = Duel.Fields[0].LifePoints;
            int enemyLP = Duel.Fields[1].LifePoints;

            int totalAttack = 0;
            foreach (var card in Duel.Fields[0].MonsterZone)
            {
                if (card != null && card.Position == (int)CardPosition.FaceUpAttack)
                {
                    totalAttack += card.Attack;
                }
            }

            double enemyDanger = CalculateTotalDangerForField();

            string oldGoal = _currentGoal;
            if (totalAttack >= enemyLP && enemyDanger < 40.0) // Lethal safeguard: don't push lethal when opponent board is highly threatening
            {
                _currentGoal = "push_lethal";
            }
            else if (selfLP < 3000)
            {
                _currentGoal = "survive";
            }
            else if (enemyDanger >= 40.0)
            {
                _currentGoal = "break_board";
            }
            else
            {
                _currentGoal = "establish_interruptions";
            }

            if (oldGoal != _currentGoal)
            {
                LogToTurn(string.Format("Goal shifted: {0} -> {1} (Self LP: {2}, Enemy LP: {3}, Total ATK: {4}, Opp Danger: {5:F1})", 
                    oldGoal, _currentGoal, selfLP, enemyLP, totalAttack, enemyDanger));
            }
        }

        private bool HasBaitInHand()
        {
            foreach (var card in Duel.Fields[0].Hand)
            {
                if (card != null && _cardRegistry.ContainsKey(card.Id))
                {
                    if (_cardRegistry[card.Id].bait_value > 5)
                        return true;
                }
            }
            return false;
        }

        private int GetZoneCount(IList<ClientCard> zone)
        {
            int count = 0;
            foreach (var card in zone)
            {
                if (card != null) count++;
            }
            return count;
        }

        private double GetStapleBaselineDanger(int cardId)
        {
            switch (cardId)
            {
                case 23434538: return 80.0; // Maxx "C"
                case 42141493: return 75.0; // Mulcharmy Fuwalos
                case 14558127:
                case 14558128: return 55.0; // Ash Blossom & Joyous Spring
                case 94145021: return 50.0; // Droll & Lock Bird
                case 10045474: return 45.0; // Infinite Impermanence
                case 97268402: return 45.0; // Effect Veiler
                case 29301450: return 50.0; // S:P Little Knight
                case 24224830: return 40.0; // Called by the Grave
                case 73642296: return 40.0; // Ghost Belle & Haunted Mansion
                case 52038441: return 35.0; // Ghost Mourner & Moonlit Chill
                default: return 0.0;
            }
        }

        private double CalculateCardDanger(ClientCard enemyCard)
        {
            if (enemyCard == null) return 0.0;
            if (enemyCard.IsDisabled()) return 0.0; // Negation Check

            double danger = 0.0;

            // 0. Opponent Memory learned danger override/bonus with baseline protection
            double learnedDanger = 0.0;
            if (_opponentMemory.ContainsKey(enemyCard.Id))
            {
                learnedDanger = _opponentMemory[enemyCard.Id].learned_danger;
            }
            double baselineDanger = GetStapleBaselineDanger(enemyCard.Id);
            danger += Math.Max(learnedDanger, baselineDanger);

            // 1. Check if the card is in the registry and check priority
            if (_cardRegistry.ContainsKey(enemyCard.Id))
            {
                var meta = _cardRegistry[enemyCard.Id];
                danger += meta.priority * 8.0; // Base danger from priority weight

                // 2. Weakness mapping (Does the card match this deck's specific weaknesses?)
                if (_deckConfig.weaknesses != null)
                {
                    foreach (string weakness in _deckConfig.weaknesses)
                    {
                        if (weakness == "handtraps" && enemyCard.Location == CardLocation.Hand)
                        {
                            danger += 25.0; // Handtraps are dangerous if our deck is vulnerable to them
                        }
                        if (weakness == "backrow" && (enemyCard.IsSpell() || enemyCard.IsTrap()))
                        {
                            danger += 20.0;
                        }
                        if (weakness == "graveyard_hate" && (meta.roles.Contains("graveyard_hate") || enemyCard.Id == 24224830)) // Called by the Grave ID is 24224830
                        {
                            danger += 30.0;
                        }
                    }
                }
            }
            else
            {
                // Unregistered card fallback danger calculation
                if (enemyCard.IsMonster())
                {
                    if (enemyCard.Attack >= 2500) danger += 20.0; // High ATK threat
                    if (enemyCard.Level >= 8) danger += 15.0;     // Boss monster threat
                }
                else if (enemyCard.IsSpell() || enemyCard.IsTrap())
                {
                    danger += 10.0; // Generic spell/trap
                }
            }

            // 3. Situational/Type-based Danger
            // Extra Deck detection
            if (enemyCard.IsMonster() && enemyCard.IsExtraCard())
            {
                danger += 25.0;
            }

            // Face-up S/T continuous/field threats
            if (enemyCard.IsSpell() || enemyCard.IsTrap())
            {
                if (enemyCard.IsFaceup() && (enemyCard.HasType(CardType.Continuous) || enemyCard.HasType(CardType.Field)))
                {
                    danger += 15.0;
                }
            }

            // Material Detection (Tuner + 1 other monster)
            if (enemyCard.IsMonster() && enemyCard.IsTuner() && GetOpponentFaceUpMonsterCount() >= 2)
            {
                danger += 20.0;
            }

            // Is it responding to our starter/key card in chain?
            ClientCard lastBotCard = Util.GetLastChainCard(); // If opponent chains, lastBotCard is our card they chained to
            if (lastBotCard != null && lastBotCard.Controller == 0) // It is our card!
            {
                if (_cardRegistry.ContainsKey(lastBotCard.Id))
                {
                    var ourMeta = _cardRegistry[lastBotCard.Id];
                    if (ourMeta.roles.Contains("starter") || ourMeta.roles.Contains("payoff"))
                    {
                        danger += 35.0; // Extremely high danger because they are chaining to our starter or payoff card!
                    }
                }
            }

            return danger;
        }

        private bool IsFaceUp(ClientCard card)
        {
            if (card == null) return false;
            return card.Position == (int)CardPosition.FaceUpAttack || card.Position == (int)CardPosition.FaceUpDefence;
        }

        private bool IsLightOrDark(ClientCard card)
        {
            if (card == null) return false;
            try
            {
                int attr = (int)card.Attribute;
                return (attr & (int)CardAttribute.Light) != 0 || (attr & (int)CardAttribute.Dark) != 0;
            }
            catch
            {
                return false;
            }
        }

        private int GetOpponentFaceUpMonsterCount()
        {
            int count = 0;
            foreach (var enemyCard in Duel.Fields[1].MonsterZone)
            {
                if (enemyCard != null && IsFaceUp(enemyCard))
                {
                    count++;
                }
            }
            return count;
        }

        private int GetOpponentGraveMonsterCount()
        {
            int count = 0;
            foreach (var enemyCard in Duel.Fields[1].Graveyard)
            {
                if (enemyCard != null && enemyCard.IsMonster())
                {
                    count++;
                }
            }
            return count;
        }

        private int GetOpponentGraveLightDarkCount()
        {
            int count = 0;
            foreach (var enemyCard in Duel.Fields[1].Graveyard)
            {
                if (enemyCard != null && enemyCard.IsMonster() && IsLightOrDark(enemyCard))
                {
                    count++;
                }
            }
            return count;
        }

        private bool EvaluateCardAction(ClientCard card, CardMetadata meta, ExecutorType type)
        {
            // Block summoning handtraps or low-ATK walls in Attack position
            if (type == ExecutorType.Summon || type == ExecutorType.SpSummon)
            {
                if (meta.roles.Contains("handtrap"))
                {
                    return false;
                }
                if (card.Attack < 1500 && card.Defense >= card.Attack)
                {
                    if (!meta.roles.Contains("starter") && !meta.roles.Contains("extender") && !meta.roles.Contains("payoff"))
                    {
                        return false;
                    }
                }
            }

            if (type == ExecutorType.Activate)
            {
                ClientCard lastChainCard = Util.GetLastChainCard();

                // General Rule: Never chain an interruption / handtrap / negate to our own card activation
                if (lastChainCard != null && lastChainCard.Controller == 0)
                {
                    if (meta.roles.Contains("interruption") || meta.roles.Contains("handtrap") || meta.roles.Contains("disruption"))
                    {
                        LogToTurn(string.Format("Block chaining self-hurt: {0} (ID: {1}) responding to our own card: {2} (ID: {3})",
                            GetCardName(card.Id), card.Id, GetCardName(lastChainCard.Id), lastChainCard.Id));
                        return false;
                    }
                }

                // GLOBAL RULE: If it is our turn and the card is a disruptive handtrap (handtrap + disruption/interruption),
                // never activate it as chain link 1 on our own turn — it belongs in the opponent's turn.
                if (Duel.Player == 0 && meta.roles.Contains("handtrap"))
                {
                    if (meta.roles.Contains("disruption") || meta.roles.Contains("interruption"))
                    {
                        if (lastChainCard != null && lastChainCard.Controller == 1)
                        {
                            // Allow reacting to opponent's activations (e.g. chaining Ash to negate Maxx "C")
                        }
                        else
                        {
                            LogToTurn(string.Format("Block disruptive handtrap {0} on our own turn.", GetCardName(card.Id)));
                            return false;
                        }
                    }
                }

                // Specific card safeguards to prevent self-sabotage/illegal activations
                
                // 1. Droll & Lock Bird (ID: 94145021) - Only activate on opponent's turn to avoid locking ourselves
                if (card.Id == 94145021 && Duel.Player == 0)
                {
                    LogToTurn("Block activating Droll & Lock Bird on our own turn.");
                    return false;
                }

                // 2. Effect Veiler (ID: 97268402) - Only activate on opponent's turn and during opponent's Main Phase
                if (card.Id == 97268402)
                {
                    if (Duel.Player == 0 || (Duel.Phase != DuelPhase.Main1 && Duel.Phase != DuelPhase.Main2))
                    {
                        LogToTurn("Block activating Effect Veiler (must be opponent's Main Phase only).");
                        return false;
                    }
                }

                // 3. Called by the Grave (ID: 24224830) - Only activate if starting a chain and there is a target in the opponent's GY
                if (card.Id == 24224830)
                {
                    if (lastChainCard == null && GetOpponentGraveMonsterCount() == 0)
                    {
                        LogToTurn("Block Called by the Grave: No monsters in opponent's GY to target.");
                        return false;
                    }
                }

                // 4. Bystials: Druiswurm (ID: 6637331) & Magnamhut (ID: 33854624) - Only activate if opponent has LIGHT/DARK monster in GY
                if (card.Id == 6637331 || card.Id == 33854624)
                {
                    if (GetOpponentGraveLightDarkCount() == 0)
                    {
                        LogToTurn(string.Format("Block Bystial {0}: No LIGHT/DARK monsters in opponent's GY to banish.", GetCardName(card.Id)));
                        return false;
                    }
                }

                // 5. Infinite Impermanence (ID: 10045474) - If starting a chain, require target
                if (card.Id == 10045474)
                {
                    if (lastChainCard == null && GetOpponentFaceUpMonsterCount() == 0)
                    {
                        LogToTurn("Block Infinite Impermanence: No face-up monsters on opponent's field to target.");
                        return false;
                    }
                }

                // 6. Mulcharmy Fuwalos / Maxx 'C' style: only activate on opponent's turn
                if (card.Id == 42141493 && Duel.Player == 0)
                {
                    LogToTurn("Block Mulcharmy Fuwalos on our own turn.");
                    return false;
                }
            }

            double score = meta.priority * 10.0;

            // Factor in Q-value if available for current goal
            if (meta.q_values != null && meta.q_values.ContainsKey(_currentGoal))
            {
                try
                {
                    double qVal = Convert.ToDouble(meta.q_values[_currentGoal]);
                    score += qVal * 10.0; // scaled to match priority factor
                }
                catch {}
            }

            int selfHandCount = Duel.Fields[0].Hand.Count;
            int opponentHandCount = Duel.Fields[1].Hand.Count;
            int selfMonsters = GetZoneCount(Duel.Fields[0].MonsterZone);
            int opponentMonsters = GetZoneCount(Duel.Fields[1].MonsterZone);
            int selfSpells = GetZoneCount(Duel.Fields[0].SpellZone);
            int opponentSpells = GetZoneCount(Duel.Fields[1].SpellZone);

            int selfLP = Duel.Fields[0].LifePoints;
            int enemyLP = Duel.Fields[1].LifePoints;

            double fieldDanger = CalculateTotalDangerForField();
            double opponentThreat = fieldDanger + (opponentHandCount * 8.0);

            // 1. Goal adjustments
            if (_currentGoal == "push_lethal")
            {
                if (meta.roles.Contains("starter") || meta.roles.Contains("extender"))
                    score += 25.0;
                if (meta.roles.Contains("payoff"))
                    score += 35.0;
                // NEW: combo_piece bonus for push_lethal
                if (meta.roles.Contains("combo_piece"))
                    score += 20.0;
                // NEW: tuner bonus when material available
                if (meta.roles.Contains("tuner"))
                {
                    if (selfMonsters >= 1)
                        score += 20.0;
                    else
                        score -= 10.0; // No material to pair with
                }
                // NEW: searcher helps find missing payoff
                if (meta.roles.Contains("searcher"))
                    score += 10.0;
                // NEW: disruption minor bonus during push
                if (meta.roles.Contains("disruption"))
                    score += 5.0;
            }
            else if (_currentGoal == "survive")
            {
                if (meta.roles.Contains("recovery"))
                    score += 30.0;
                if (meta.roles.Contains("interruption") || meta.roles.Contains("floodgate"))
                    score += 25.0;
                // NEW: recovery_value scaling for survive goal
                score += meta.recovery_value * 3.0;
                // NEW: disruption bonus for survive
                if (meta.roles.Contains("disruption"))
                    score += 20.0;
            }
            else if (_currentGoal == "break_board")
            {
                if (meta.roles.Contains("removal"))
                    score += 35.0; // Prioritize removing threats
                if (meta.roles.Contains("interruption") || meta.roles.Contains("disruption"))
                    score += 20.0; // Control development
                if (meta.roles.Contains("starter") || meta.roles.Contains("extender"))
                    score += 15.0; // Extenders to break board
            }
            else // establish_interruptions
            {
                if (meta.roles.Contains("starter") && selfMonsters == 0)
                    score += 20.0;
                if (meta.roles.Contains("interruption"))
                    score += 15.0;
                // NEW: combo_piece bonus when building board
                if (meta.roles.Contains("combo_piece") && selfMonsters < 2)
                    score += 15.0;
                // NEW: disruption bonus for interruption goal
                if (meta.roles.Contains("disruption"))
                    score += 20.0;
                // NEW: tuner bonus when material available
                if (meta.roles.Contains("tuner"))
                {
                    if (selfMonsters >= 1)
                        score += 15.0;
                    else
                        score -= 10.0;
                }
                // NEW: recovery_value scaling when LP is getting low
                if (selfLP < 5000)
                    score += meta.recovery_value * 1.5;
            }

            // 1.1 NEW: Searcher bonus — hand advantage when resources are thin
            if (meta.roles.Contains("searcher") && selfHandCount <= 3)
                score += 15.0;

            // 1.5. Combo Plan Heuristics (Branching / Backup Plans)
            if (meta.combo_plans.Contains(_currentPlan))
            {
                score += 30.0;
            }
            else
            {
                // Penalize playing dead/blocked combo lines
                bool isBlocked = false;
                foreach (string plan in meta.combo_plans)
                {
                    if (_blockedPlans.Contains(plan))
                    {
                        isBlocked = true;
                        break;
                    }
                }
                if (isBlocked)
                {
                    score -= 90.0;
                    LogToTurn(string.Format("Penalizing dead combo card: {0} because its plan is blocked.", GetCardName(card.Id)));
                }
            }

            // 2. Threat & Baiting logic
            if (opponentThreat > 25.0)
            {
                if (meta.bait_value > 5)
                {
                    score += (meta.bait_value * 4.0);
                }
                if (meta.risk_if_negated > 5 && HasBaitInHand())
                {
                    score -= (meta.risk_if_negated * 3.0);
                }
            }

            // 3. Resource Advantage Adjustments
            if (selfHandCount <= 2)
            {
                if (meta.roles.Contains("starter") || meta.roles.Contains("recovery"))
                    score += 20.0;
                // NEW: followup_value bonus when hand is thin
                if (meta.followup_value > 5)
                    score += meta.followup_value * 2.5;
            }
            else if (selfHandCount >= 5 && selfMonsters >= 3)
            {
                if (meta.roles.Contains("starter") || meta.roles.Contains("extender"))
                    score -= 15.0; // Avoid overextending
            }

            // 3.1 NEW: followup_value combo continuation bonus on our turn
            if (Duel.Player == 0 && meta.followup_value > 6)
                score += meta.followup_value * 1.5;

            // 3.2 NEW: recovery_value GY synergy bonus
            int gyCount = Duel.Fields[0].Graveyard.Count;
            if (gyCount >= 3 && meta.recovery_value > 5)
                score += 10.0;

            // 4. Special Negation/Interruption Heuristics
            if (type == ExecutorType.Activate && meta.roles.Contains("interruption"))
            {
                ClientCard enemyCard = Util.GetLastChainCard();
                if (enemyCard != null && enemyCard.Controller == 1)
                {
                    double danger = CalculateCardDanger(enemyCard);
                    score += danger; // Directly add the danger score to our negation utility!
                    
                    LogToTurn(string.Format("Negation target is opponent's card: {0} (ID: {1}) | Danger: {2:F1}", 
                        GetCardName(enemyCard.Id), enemyCard.Id, danger));
                }
            }

            // 5. Destruction/Removal Heuristics
            if (meta.roles.Contains("removal"))
            {
                double maxEnemyDanger = 0.0;
                ClientCard bestTarget = null;
                
                foreach (var enemyMon in Duel.Fields[1].MonsterZone)
                {
                    if (enemyMon != null)
                    {
                        double d = CalculateCardDanger(enemyMon);
                        if (d > maxEnemyDanger)
                        {
                            maxEnemyDanger = d;
                            bestTarget = enemyMon;
                        }
                    }
                }
                
                foreach (var enemySpell in Duel.Fields[1].SpellZone)
                {
                    if (enemySpell != null)
                    {
                        double d = CalculateCardDanger(enemySpell);
                        if (d > maxEnemyDanger)
                        {
                            maxEnemyDanger = d;
                            bestTarget = enemySpell;
                        }
                    }
                }

                if (bestTarget != null)
                {
                    score += maxEnemyDanger * 1.5; // Scale score based on target danger
                    LogToTurn(string.Format("Removal evaluated. Highest danger target on field: {0} (ID: {1}) | Danger: {2:F1}", 
                        GetCardName(bestTarget.Id), bestTarget.Id, maxEnemyDanger));
                }
                else
                {
                    score -= 30.0; // Do not waste removal if no threats
                }
            }

            // 6. Zone Limit check
            if ((type == ExecutorType.Summon || type == ExecutorType.SpSummon) && selfMonsters >= 5)
            {
                score = 0;
            }

            // 7. Macro-Decision Refactoring Upgrades
            // 7.1 Anti-Overextension / Lethal Check
            if (Duel.Phase == DuelPhase.Main1 && IsLethalOnBoard())
            {
                if (meta.roles.Contains("combo") || meta.roles.Contains("extender") || meta.roles.Contains("starter") || meta.roles.Contains("combo_piece"))
                {
                    score -= 100.0;
                    LogToTurn(string.Format("Lethal on board detected! Penalizing overextending card: {0} (-100.0)", GetCardName(card.Id)));
                }
            }

            // 7.2 Redundant Field Spell Protection
            if (type == ExecutorType.Activate && card.HasType(CardType.Field))
            {
                var currentField = Bot.SpellZone[5];
                if (currentField != null && IsFaceUp(currentField) && currentField.Id == card.Id)
                {
                    score -= 500.0;
                    LogToTurn(string.Format("Redundant Field Spell detected! Penalizing duplicate: {0} (-500.0)", GetCardName(card.Id)));
                }
            }

            // 7.3 Anti-Self Harm Check
            if (Duel.CurrentChain.Count > 0 && Duel.LastChainPlayer == 0)
            {
                if (meta.roles.Contains("negate") || meta.roles.Contains("removal") || meta.roles.Contains("interruption") || meta.roles.Contains("disruption"))
                {
                    score -= 200.0;
                    LogToTurn(string.Format("Self-chain prevention: Penalizing disruptive card: {0} responding to our own chain link (-200.0)", GetCardName(card.Id)));
                }
            }

            LogToTurn(string.Format("Analysing Card: {0} (ID: {1}) | Action: {2} | Goal: {3} | Opp Threat: {4:F1} | Score: {5:F1}",
                GetCardName(card.Id), card.Id, type, _currentGoal, opponentThreat, score));

            bool decision = score > 35.0;
            LogDecision(card.Id, type.ToString(), _currentGoal, score, decision, _currentPlan);
            return decision;
        }

        private bool OnCardAction(int cardId, ExecutorType type)
        {
            UpdateGoal();

            if (!_cardRegistry.ContainsKey(cardId))
                return false;

            ClientCard card = Card;
            if (card == null)
                return false;

            var meta = _cardRegistry[cardId];
            bool result = EvaluateCardAction(card, meta, type);
            if (result)
            {
                if (!_ourCardsPlayed.Contains(cardId))
                    _ourCardsPlayed.Add(cardId);
            }
            return result;
        }

        // --- Fallback Handlers ---
        private bool OnDefaultActivate()
        {
            ClientCard card = Card;
            if (card != null)
            {
                bool decision = false;
                if (_cardRegistry.ContainsKey(card.Id))
                {
                    decision = EvaluateCardAction(card, _cardRegistry[card.Id], ExecutorType.Activate);
                }
                else
                {
                    decision = false; // Unknown card — safe default, do not play blindly
                }
                if (decision)
                {
                    LogToTurn(string.Format("{0} Activated [Fallback]: {1} (ID: {2})", _resolvedDeckName, GetCardName(card.Id), card.Id));
                }
                return decision;
            }
            return false;
        }

        private bool HasStarterOrExtenderInHand()
        {
            foreach (var card in Duel.Fields[0].Hand)
            {
                if (card != null && _cardRegistry.ContainsKey(card.Id))
                {
                    var meta = _cardRegistry[card.Id];
                    if (meta.roles.Contains("starter") || meta.roles.Contains("extender") || meta.roles.Contains("payoff") || meta.roles.Contains("searcher"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int GetOpponentTotalAttack()
        {
            int total = 0;
            foreach (var card in Duel.Fields[1].MonsterZone)
            {
                if (card != null && card.Position == (int)CardPosition.FaceUpAttack)
                {
                    total += card.Attack;
                }
            }
            return total;
        }

        private bool OnDefaultSummon()
        {
            ClientCard card = Card;
            if (card == null) return false;

            // 1. If we have any starter or extender in hand, save our normal summon for them!
            if (HasStarterOrExtenderInHand())
            {
                if (_cardRegistry.ContainsKey(card.Id))
                {
                    var meta = _cardRegistry[card.Id];
                    if (!meta.roles.Contains("starter") && !meta.roles.Contains("extender") && !meta.roles.Contains("payoff") && !meta.roles.Contains("searcher"))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            // 2. If it is a handtrap, NEVER summon it face-up.
            if (_cardRegistry.ContainsKey(card.Id))
            {
                var meta = _cardRegistry[card.Id];
                if (meta.roles.Contains("handtrap"))
                {
                    return false;
                }
            }

            // 3. For low-ATK monsters (ATK < 1500), prefer setting them instead of summoning in attack position.
            if (card.Attack < 1500 && card.Defense >= card.Attack)
            {
                return false; // Let the MonsterSet handler take care of it as a face-down set
            }

            // 4. For high-ATK monsters (ATK >= 1500), evaluate summoning them face-up.
            bool decision = false;
            if (_cardRegistry.ContainsKey(card.Id))
            {
                decision = EvaluateCardAction(card, _cardRegistry[card.Id], ExecutorType.Summon);
            }
            else
            {
                decision = false; // Unknown card — safe default, do not play blindly
            }

            if (decision)
            {
                LogToTurn(string.Format("{0} Summoned [Fallback]: {1} (ID: {2}) | ATK={3} DEF={4}", 
                    _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
            }
            return decision;
        }

        private bool OnDefaultSpSummon()
        {
            ClientCard card = Card;
            if (card != null)
            {
                // Prevent special summoning handtraps in fallback
                if (_cardRegistry.ContainsKey(card.Id))
                {
                    var meta = _cardRegistry[card.Id];
                    if (meta.roles.Contains("handtrap"))
                    {
                        return false;
                    }
                }

                bool decision = false;
                if (_cardRegistry.ContainsKey(card.Id))
                {
                    decision = EvaluateCardAction(card, _cardRegistry[card.Id], ExecutorType.SpSummon);
                }
                else
                {
                    decision = false; // Unknown card — safe default, do not play blindly
                }
                if (decision)
                {
                    LogToTurn(string.Format("{0} SpSummoned [Fallback]: {1} (ID: {2}) | ATK={3} DEF={4}", 
                        _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
                }
                return decision;
            }
            return false;
        }

        private bool OnDefaultSpellSet()
        {
            ClientCard card = Card;
            if (card == null) return false;

            double score = 50.0; // Base score for setting

            if (_cardRegistry.ContainsKey(card.Id))
            {
                var meta = _cardRegistry[card.Id];
                score = meta.priority * 10.0;
            }

            if ((card.IsTrap() || card.HasType(CardType.QuickPlay)) && Duel.Phase == DuelPhase.Main1 && Duel.Turn > 1)
            {
                score -= 30.0; // Penalty for setting in Main 1
                LogToTurn(string.Format("Smart Trap Setting: Penalizing setting {0} in Main 1 (-30.0)", GetCardName(card.Id)));
            }

            bool decision = score > 35.0;
            if (decision)
            {
                LogToTurn(string.Format("{0} Set Spell/Trap: {1} (ID: {2})", _resolvedDeckName, GetCardName(card.Id), card.Id));
            }
            return decision;
        }

        private bool OnDefaultRepos()
        {
            ClientCard card = Card;
            if (card != null)
            {
                if (card.Attack > card.Defense)
                {
                    bool isDefense = card.Position == (int)CardPosition.FaceDownDefence || card.Position == (int)CardPosition.FaceUpDefence;
                    if (isDefense)
                    {
                        LogToTurn(string.Format("{0} Repositioned to Attack: {1} (ID: {2}) | ATK={3} DEF={4}", 
                            _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
                        return true;
                    }
                }
                else if (card.Defense > card.Attack)
                {
                    bool isAttack = card.Position == (int)CardPosition.FaceDownAttack || card.Position == (int)CardPosition.FaceUpAttack;
                    if (isAttack)
                    {
                        LogToTurn(string.Format("{0} Repositioned to Defense: {1} (ID: {2}) | ATK={3} DEF={4}", 
                            _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
                        return true;
                    }
                }
            }
            return false;
        }

        private bool OnDefaultMonsterSet()
        {
            ClientCard card = Card;
            if (card == null) return false;

            // 1. If we have other starters or extenders in hand, save the action for them.
            if (HasStarterOrExtenderInHand())
            {
                return false;
            }

            // 2. Desperation check: LP is low or opponent has lethal threat on board
            int selfMonsters = GetZoneCount(Duel.Fields[0].MonsterZone);
            int opponentMonsters = GetZoneCount(Duel.Fields[1].MonsterZone);
            int oppAttack = GetOpponentTotalAttack();
            int selfLP = Duel.Fields[0].LifePoints;
            bool isDesperate = (oppAttack >= selfLP) || (selfLP < 3000);

            // 3. Handtrap handling
            if (_cardRegistry.ContainsKey(card.Id))
            {
                var meta = _cardRegistry[card.Id];
                if (meta.roles.Contains("handtrap"))
                {
                    // Only Set a handtrap face-down if we have absolutely no monsters on board,
                    // the opponent has active threats, and we are in desperation (OTK threat or low LP).
                    if (selfMonsters == 0 && opponentMonsters > 0 && isDesperate)
                    {
                        LogToTurn(string.Format("{0} Set Handtrap as Defensive Wall [Desperation]: {1} (ID: {2}) | LP={3} OppATK={4}", 
                            _resolvedDeckName, GetCardName(card.Id), card.Id, selfLP, oppAttack));
                        return true;
                    }
                    return false; // Keep handtrap active in hand
                }
            }

            // 4. Non-handtrap monster handling (e.g., small tuners or combo pieces)
            if (selfMonsters == 0)
            {
                LogToTurn(string.Format("{0} Set Monster in Defense [Fallback Wall]: {1} (ID: {2}) | ATK={3} DEF={4}", 
                    _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
                return true;
            }

            return false;
        }

        private void LogState()
        {
            try
            {
                LogToTurn("--- Hand State: ---");
                foreach (var card in Duel.Fields[0].Hand)
                {
                    if (card != null)
                        LogToTurn(string.Format("  - Hand: {0} (ID: {1})", GetCardName(card.Id), card.Id));
                }

                LogToTurn("--- Monster Zone State: ---");
                for (int i = 0; i < 7; i++)
                {
                    var card = Duel.Fields[0].MonsterZone[i];
                    if (card != null)
                    {
                        LogToTurn(string.Format("  - Monster[{0}]: {1} (ID: {2}) | ATK={3} DEF={4} Pos={5}", 
                            i, GetCardName(card.Id), card.Id, card.Attack, card.Defense, (CardPosition)card.Position));
                    }
                }

                LogToTurn("--- Spell/Trap Zone State: ---");
                for (int i = 0; i < 8; i++)
                {
                    var card = Duel.Fields[0].SpellZone[i];
                    if (card != null)
                    {
                        LogToTurn(string.Format("  - SpellTrap[{0}]: {1} (ID: {2}) | Pos={3}", 
                            i, GetCardName(card.Id), card.Id, (CardPosition)card.Position));
                    }
                }

                LogToTurn("--- Graveyard State: ---");
                foreach (var card in Duel.Fields[0].Graveyard)
                {
                    if (card != null)
                        LogToTurn(string.Format("  - GY: {0} (ID: {1})", GetCardName(card.Id), card.Id));
                }

                LogToTurn("--- Banished State: ---");
                foreach (var card in Duel.Fields[0].Banished)
                {
                    if (card != null)
                        LogToTurn(string.Format("  - Banished: {0} (ID: {1})", GetCardName(card.Id), card.Id));
                }
            }
            catch (Exception ex)
            {
                LogToTurn("Error logging state: " + ex.Message);
            }
        }

        // --- Lifecycle Hooks ---
        public override void OnNewTurn()
        {
            _turnCount = Duel.Turn;
            _currentTurnLogPath = Path.Combine(_matchLogDir, "turn_" + _turnCount + ".log");

            LogToTurn(string.Format("=== Turn {0} Started (Active Player: {1}) ===", _turnCount, Duel.Player == 0 ? "Bot" : "Opponent"));
            LogToTurn(string.Format("Bot LP: {0} | Opponent LP: {1}", Duel.Fields[0].LifePoints, Duel.Fields[1].LifePoints));
            
            // Reset combo path at start of each turn
            _currentPlan = "PlanA";
            _blockedPlans.Clear();
            LogToTurn("Combo Plan initialized to PlanA");

            // Log full state at the start of each turn
            LogState();

            // Safety: periodic save every 3 turns to prevent data loss if destructor is not called
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
            LogToTurn("--- Phase Changed to: " + Duel.Phase.ToString() + " ---");
            base.OnNewPhase();
        }

        public override bool OnSelectHand()
        {
            if (_deckConfig.playstyle == "control" || _deckConfig.playstyle == "midrange")
            {
                LogToTurn("Playstyle is control/midrange, selecting to go first.");
                return true;
            }
            LogToTurn("Selecting to go second.");
            return false;
        }

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            LogToTurn(string.Format("OnSelectCard called: count={0}, min={1}, max={2}, hint={3}, cancelable={4}", 
                cards != null ? cards.Count : 0, min, max, hint, cancelable));

            if (cards == null || cards.Count == 0)
                return base.OnSelectCard(cards, min, max, hint, cancelable);

            // Filter out the card currently being evaluated to prevent self-discard/tribute
            List<ClientCard> available = new List<ClientCard>();
            foreach (var c in cards)
            {
                if (Card != null && c.Id == Card.Id && cards.Count > min)
                    continue;
                available.Add(c);
            }
            if (available.Count < min)
                available = new List<ClientCard>(cards);

            // Determine if we should prefer high priority or low priority
            bool preferHighPriority = true;
            if (available.Count > 0)
            {
                CardLocation loc = available[0].Location;
                if (loc == CardLocation.Hand || loc == CardLocation.MonsterZone || loc == CardLocation.SpellZone)
                {
                    // Discarding, tributing, or destroying our own cards on field/hand -> prefer lowest priority
                    preferHighPriority = false;
                }
            }

            bool isKwtunePreferHigh = (_resolvedDeckName == "2026_Kwtune" && preferHighPriority);

            // Sort available cards based on registry priority
            available.Sort((x, y) =>
            {
                int priX = 5;
                CardMetadata metaX;
                if (_cardRegistry.TryGetValue(x.Id, out metaX))
                {
                    priX = metaX.priority;
                }

                int priY = 5;
                CardMetadata metaY;
                if (_cardRegistry.TryGetValue(y.Id, out metaY))
                {
                    priY = metaY.priority;
                }

                // Scoped archetype priority boost for Kewl Tune to prevent handtrap hijacking
                if (isKwtunePreferHigh)
                {
                    if (x.HasSetcode(0x1ce)) priX += 5;
                    if (y.HasSetcode(0x1ce)) priY += 5;
                }
                
                if (preferHighPriority)
                    return priY.CompareTo(priX); // Descending (highest first)
                else
                    return priX.CompareTo(priY); // Ascending (lowest first)
            });

            // Select the required number of cards
            List<ClientCard> result = new List<ClientCard>();
            int targetCount = min;
            if (min == 0 && max > 0 && cancelable)
            {
                // If it is an optional/cancelable choice of our own cards (e.g. discard, tribute, send to grave),
                // select at least 1 card to avoid cancelling our own action and causing an infinite loop.
                if (!preferHighPriority || (hint >= 501 && hint <= 506))
                {
                    targetCount = 1;
                }
            }

            for (int i = 0; i < Math.Min(targetCount, available.Count); i++)
            {
                result.Add(available[i]);
            }
            
            // If max > min, we can optionally add more if they are beneficial (for preferHighPriority)
            if (result.Count < max && preferHighPriority)
            {
                int startIndex = Math.Max(min, targetCount);
                for (int i = startIndex; i < Math.Min(max, available.Count); i++)
                {
                    result.Add(available[i]);
                }
            }

            return result;
        }

        private string GetNextPlan(string current)
        {
            if (current == "PlanA") return "PlanB";
            if (current == "PlanB") return "PlanC";
            return "PlanA"; // Reset to PlanA when all plans exhausted
        }

        public override void OnChaining(int player, ClientCard card)
        {
            if (card != null)
            {
                string activator = player == 0 ? "Bot" : "Opponent";
                string cardName = GetCardName(card.Id);
                LogToTurn(string.Format("Chain Event: {0} activated {1} (ID: {2})", activator, cardName, card.Id));

                if (player == 1) // Opponent activated
                {
                    RecordOpponentCardSeen(card.Id);
                }

                // Check for negate / disruption response
                ClientCard lastChain = Util.GetLastChainCard();
                if (lastChain != null && lastChain.Controller == 0) // Our card was in the chain before
                {
                    // Track opponent card seen
                    RecordOpponentCardSeen(card.Id);

                    if (player == 1) // Opponent is the one chaining into us
                    {
                        // Track disruption relationship
                        if (!_disruptionsInMatch.ContainsKey(lastChain.Id))
                        {
                            _disruptionsInMatch[lastChain.Id] = new List<int>();
                        }
                        if (!_disruptionsInMatch[lastChain.Id].Contains(card.Id))
                        {
                            _disruptionsInMatch[lastChain.Id].Add(card.Id);
                        }

                        if (_deckConfig.choke_points.Contains(lastChain.Id))
                        {
                            LogToTurn(string.Format("WARNING: Opponent disrupted Bot's choke point [{0}] (ID: {1}) with [{2}] (ID: {3})!",
                                GetCardName(lastChain.Id), lastChain.Id, cardName, card.Id));
                        }

                        // Determine if our current plan is disrupted
                        double danger = CalculateCardDanger(card);
                        if (danger > 30.0) // Significant threat
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
            base.OnChaining(player, card);
        }

        public override void OnChainEnd()
        {
            LogToTurn("--- Chain resolution finished ---");
            base.OnChainEnd();
        }

        private static void StaticOnProcessExit(object sender, EventArgs e)
        {
            if (_currentInstance != null)
            {
                _currentInstance.ApplyRealTimeLearning();
            }
        }

        ~UnifiedIgnisExecutor()
        {
            ApplyRealTimeLearning();
            LogToMatch("=== Duel Session Finished ===");
            LogToMatch("Final Bot LP: " + Duel.Fields[0].LifePoints);
            LogToMatch("Final Opponent LP: " + Duel.Fields[1].LifePoints);
            LogToMatch("Finished Time: " + DateTime.Now.ToString());
        }
    }

    [Deck("2026_AzaYummy", "2026_AzaYummy")]
    public class AzaYummyExecutor : UnifiedIgnisExecutor
    {
        public AzaYummyExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_BrElfnote", "2026_BrElfnote")]
    public class BrElfnoteExecutor : UnifiedIgnisExecutor
    {
        public BrElfnoteExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_DarkTime", "2026_DarkTime")]
    public class DarkTimeExecutor : UnifiedIgnisExecutor
    {
        public DarkTimeExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_EvilTwin", "2026_EvilTwin")]
    public class EvilTwinExecutor : UnifiedIgnisExecutor
    {
        public EvilTwinExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_EyeInside", "2026_EyeInside")]
    public class EyeInsideExecutor : UnifiedIgnisExecutor
    {
        public EyeInsideExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Hecahand", "2026_Hecahand")]
    public class HecahandExecutor : UnifiedIgnisExecutor
    {
        public HecahandExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Goldlord", "2026_Goldlord")]
    public class GoldlordExecutor : UnifiedIgnisExecutor
    {
        public GoldlordExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Invoke", "2026_Invoke")]
    public class InvokeExecutor : UnifiedIgnisExecutor
    {
        public InvokeExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Kwtune", "2026_Kwtune")]
    public class KwtuneExecutor : UnifiedIgnisExecutor
    {
        public KwtuneExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_Labrynth", "2026_Labrynth")]
    public class LabrynthExecutor : UnifiedIgnisExecutor
    {
        public LabrynthExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }

    [Deck("2026_PureYummy", "2026_PureYummy")]
    public class PureYummyExecutor : UnifiedIgnisExecutor
    {
        public PureYummyExecutor(GameAI ai, Duel duel) : base(ai, duel) {}
    }
}
