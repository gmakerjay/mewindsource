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
    public class BaseCustomExecutor : DefaultExecutor, IDisposable
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

            public DeckIdentity()
            {
                playstyle = "combo";
                goals = new ArrayList { "survive", "establish_interruptions", "push_lethal" };
                choke_points = new ArrayList();
                weaknesses = new ArrayList { "handtraps" };
            }
        }

        protected Dictionary<int, CardMetadata> _cardRegistry = new Dictionary<int, CardMetadata>();
        protected Dictionary<int, OpponentCardMeta> _opponentMemory = new Dictionary<int, OpponentCardMeta>();
        protected Dictionary<int, string> _cardNames = new Dictionary<int, string>();
        protected Dictionary<int, string> _attackLocks = new Dictionary<int, string>();
        protected DeckIdentity _deckConfig = new DeckIdentity();
        protected string _resolvedDeckName = "";
        protected string _resolvedBaseDir = "";
        protected string _currentGoal = "establish_interruptions";
        protected string _currentPlan = "PlanA";
        protected List<string> _blockedPlans = new List<string>();

        // In-game played cards and disruptions for learning
        protected List<int> _ourCardsPlayed = new List<int>();
        protected Dictionary<int, List<int>> _disruptionsInMatch = new Dictionary<int, List<int>>();
        protected bool _learningApplied = false;
        
        // Logging State Fields
        protected string _matchLogDir = "";
        protected string _generalLogPath = "";
        protected string _decisionsLogPath = "";
        protected int _turnCount = 0;
        protected System.Threading.Thread _lpMonitorThread = null;
        protected volatile bool _stopLPMonitor = false;
        protected bool _needsReset = false;
        
        // Deduplication for decisions.jsonl — prevents logging the same evaluation twice
        protected HashSet<string> _loggedDecisionKeys = new HashSet<string>();

        protected class SimulatedMonster
        {
            public int Value;
            public bool IsAttack;
            public bool IsFacedown;
        }

        protected static readonly object _staticLock = new object();
        protected static readonly List<WeakReference<BaseCustomExecutor>> _activeInstances = new List<WeakReference<BaseCustomExecutor>>();
        protected static bool _processExitRegistered = false;
        protected static readonly Random _random = new Random();

        protected int _lastBotLP = 8000;
        protected int _lastOppLP = 8000;

        protected void UpdateLastKnownLP()
        {
            if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
            {
                _lastBotLP = Duel.Fields[0].LifePoints;
                _lastOppLP = Duel.Fields[1].LifePoints;
            }
        }

        public BaseCustomExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
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
            SetupFolderLogging();

            // Start LP Monitor Thread
            StartLPMonitor();

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
        }

        protected void SetupFolderLogging()
        {
            string baseDir = !string.IsNullOrEmpty(_resolvedBaseDir) ? _resolvedBaseDir : AppDomain.CurrentDomain.BaseDirectory;
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
        }

        protected void StartLPMonitor()
        {
            _stopLPMonitor = false;
            _lpMonitorThread = new System.Threading.Thread(MonitorLP);
            _lpMonitorThread.IsBackground = true;
            _lpMonitorThread.Start();
        }

        protected void MonitorLP()
        {
            while (!_stopLPMonitor)
            {
                try
                {
                    lock (_staticLock)
                    {
                        if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
                        {
                            int botLP = Duel.Fields[0].LifePoints;
                            int oppLP = Duel.Fields[1].LifePoints;
                            
                            if (botLP == 0 || oppLP == 0)
                            {
                                ApplyRealTimeLearning();
                            }
                            else
                            {
                                _lastBotLP = botLP;
                                _lastOppLP = oppLP;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore transient errors during reinitialization
                }
                System.Threading.Thread.Sleep(200);
            }
        }

        protected void ResetDuelState()
        {
            Log("Detected turn reset or new duel session on same TCP connection. Resetting duel state.");
            lock (_staticLock)
            {
                _ourCardsPlayed.Clear();
                _disruptionsInMatch.Clear();
                _learningApplied = false;
                _loggedDecisionKeys.Clear();
                _turnCount = 0;
                _lastBotLP = 8000;
                _lastOppLP = 8000;
                _needsReset = false;
            }
            
            // Re-setup folder logging for the new match
            SetupFolderLogging();
        }

        protected bool CanCardAttack(ClientCard card)
        {
            if (card == null || !card.IsFaceup() || !card.IsAttack() || card.Attacked)
                return false;

            // 1. Mystic Mine Lock: check both Spell Zones
            bool hasMysticMine = false;
            for (int i = 0; i < 2; i++)
            {
                if (Duel.Fields[i] != null && Duel.Fields[i].SpellZone != null)
                {
                    foreach (var s in Duel.Fields[i].SpellZone)
                    {
                        if (s != null && s.IsFaceup() && !s.IsDisabled() && s.Id == 18175665)
                        {
                            hasMysticMine = true;
                            break;
                        }
                    }
                }
                if (hasMysticMine) break;
            }
            if (hasMysticMine)
            {
                int botCount = 0;
                int oppCount = 0;
                if (Duel.Fields[0] != null && Duel.Fields[0].MonsterZone != null)
                {
                    foreach (var m in Duel.Fields[0].MonsterZone) if (m != null) botCount++;
                }
                if (Duel.Fields[1] != null && Duel.Fields[1].MonsterZone != null)
                {
                    foreach (var m in Duel.Fields[1].MonsterZone) if (m != null) oppCount++;
                }
                if (botCount > oppCount)
                {
                    return false;
                }
            }

            // 2. Messenger of Peace Lock: check both Spell Zones
            bool hasMessenger = false;
            for (int i = 0; i < 2; i++)
            {
                if (Duel.Fields[i] != null && Duel.Fields[i].SpellZone != null)
                {
                    foreach (var s in Duel.Fields[i].SpellZone)
                    {
                        if (s != null && s.IsFaceup() && !s.IsDisabled() && s.Id == 44656491)
                        {
                            hasMessenger = true;
                            break;
                        }
                    }
                }
                if (hasMessenger) break;
            }
            if (hasMessenger && card.Attack >= 1500)
            {
                return false;
            }

            // 3. Gravity Bind Lock: check both Spell Zones
            bool hasGravityBind = false;
            for (int i = 0; i < 2; i++)
            {
                if (Duel.Fields[i] != null && Duel.Fields[i].SpellZone != null)
                {
                    foreach (var s in Duel.Fields[i].SpellZone)
                    {
                        if (s != null && s.IsFaceup() && !s.IsDisabled() && s.Id == 85742772)
                        {
                            hasGravityBind = true;
                            break;
                        }
                    }
                }
                if (hasGravityBind) break;
            }
            if (hasGravityBind && card.Level >= 4 && !card.HasType(CardType.Xyz) && !card.HasType(CardType.Link))
            {
                return false;
            }

            // 4. Swords of Revealing Light: check opponent's Spell Zone only
            bool hasSwords = false;
            if (Duel.Fields[1] != null && Duel.Fields[1].SpellZone != null)
            {
                foreach (var s in Duel.Fields[1].SpellZone)
                {
                    if (s != null && s.IsFaceup() && !s.IsDisabled() && s.Id == 72302403)
                    {
                        hasSwords = true;
                        break;
                    }
                }
            }
            if (hasSwords)
            {
                return false;
            }

            return true;
        }

        protected bool IsLethalOnBoard()
        {
            if (Duel.Phase != DuelPhase.Main1 && Duel.Phase != DuelPhase.Battle) return false;

            int enemyLP = Enemy.LifePoints;

            List<int> attackers = new List<int>();
            foreach (var card in Bot.GetMonsters())
            {
                if (CanCardAttack(card))
                {
                    attackers.Add(card.Attack);
                }
            }

            if (Enemy.GetMonsterCount() == 0)
            {
                int totalAtk = 0;
                foreach (int atk in attackers)
                {
                    totalAtk += atk;
                }
                return totalAtk >= enemyLP;
            }

            // Simulate combat against opponent monsters
            List<SimulatedMonster> oppMonsters = new List<SimulatedMonster>();
            foreach (var card in Enemy.GetMonsters())
            {
                if (card == null) continue;
                SimulatedMonster sm = new SimulatedMonster();
                sm.IsFacedown = card.IsFacedown();
                if (sm.IsFacedown)
                {
                    sm.IsAttack = false;
                    sm.Value = card.Defense > 0 ? card.Defense : 0;
                }
                else
                {
                    sm.IsAttack = !card.IsDefense();
                    sm.Value = sm.IsAttack ? card.Attack : card.Defense;
                }
                oppMonsters.Add(sm);
            }

            // Sort opponent monsters: Defense position (including facedown) first, then Attack position.
            // Within each group, sort by value descending.
            oppMonsters.Sort((a, b) =>
            {
                if (a.IsAttack != b.IsAttack)
                {
                    // Defense first (false before true)
                    return a.IsAttack.CompareTo(b.IsAttack);
                }
                return b.Value.CompareTo(a.Value);
            });

            // Sort our attackers ascending so we can easily find the weakest one that can destroy the target
            attackers.Sort();

            int damageDealt = 0;

            foreach (var opp in oppMonsters)
            {
                int chosenIndex = -1;
                bool isCrash = false;

                // 1. Try to find the weakest attacker that can destroy it (ATK > opp.Value)
                for (int i = 0; i < attackers.Count; i++)
                {
                    if (attackers[i] > opp.Value)
                    {
                        chosenIndex = i;
                        break;
                    }
                }

                // 2. If it's in attack position, we can also crash (ATK == opp.Value)
                if (chosenIndex == -1 && opp.IsAttack)
                {
                    for (int i = 0; i < attackers.Count; i++)
                    {
                        if (attackers[i] == opp.Value)
                        {
                            chosenIndex = i;
                            isCrash = true;
                            break;
                        }
                    }
                }

                // If we can't destroy/crash this opponent monster, we cannot clear the board.
                // Thus we cannot achieve direct attacks from remaining monsters, making lethal highly unlikely/unpredictable.
                if (chosenIndex == -1)
                {
                    return false;
                }

                // Apply damage if we destroyed an attack position monster without crashing
                if (opp.IsAttack && !isCrash)
                {
                    damageDealt += (attackers[chosenIndex] - opp.Value);
                }

                // Remove the attacker used
                attackers.RemoveAt(chosenIndex);
            }

            // All opponent monsters destroyed! Any remaining attackers attack directly.
            foreach (int atk in attackers)
            {
                damageDealt += atk;
            }

            if (damageDealt >= enemyLP)
            {
                LogToTurn(string.Format("Lethal on board detected! Simulated damage: {0} >= Opp LP: {1}", damageDealt, enemyLP));
                return true;
            }

            return false;
        }

        protected void Log(string message)
        {
            Console.WriteLine("[IgnisEngine] " + message);
        }

        protected void LogToMatch(string message)
        {
            Log("MATCH: " + message);
            if (string.IsNullOrEmpty(_generalLogPath)) return;
            try
            {
                File.AppendAllText(_generalLogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[IgnisEngine] LogToMatch error: " + ex.Message);
            }
        }

        protected void LogToTurn(string message)
        {
            Log("TURN " + _turnCount + ": " + message);
            if (string.IsNullOrEmpty(_matchLogDir)) return;
            string turnLogPath = Path.Combine(_matchLogDir, "turn_" + _turnCount + ".log");
            try
            {
                File.AppendAllText(turnLogPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[IgnisEngine] LogToTurn error: " + ex.Message);
            }
        }

        protected void LogDecision(int cardId, string action, string goal, double score, bool decision, string plan)
        {
            Log(string.Format("DECISION: Turn {0} | Card: {1} ({2}) | Action: {3} | Goal: {4} | Score: {5:F1} | Plan: {6} | Chosen: {7}", 
                _turnCount, cardId, GetCardName(cardId), action, goal, score, plan, decision));

            if (string.IsNullOrEmpty(_decisionsLogPath)) return;
            try
            {
                // Deduplicate: same turn + card_id + action combination only logged once
                string dedupKey = string.Format("{0}_{1}_{2}", _turnCount, cardId, action);
                if (_loggedDecisionKeys.Contains(dedupKey)) return;
                _loggedDecisionKeys.Add(dedupKey);
                
                int lpSelf = 8000;
                int lpOpp = 8000;
                double opponentThreat = 0.0;
                string botMonstersJson = "[]";
                string oppMonstersJson = "[]";
                string oppSpellsJson = "[]";
                string botHandJson = "[]";

                if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2)
                {
                    if (Duel.Fields[0] != null)
                    {
                        lpSelf = Duel.Fields[0].LifePoints;
                        botMonstersJson = SerializeMonsterZone(Duel.Fields[0].MonsterZone);
                        botHandJson = SerializeHand(Duel.Fields[0].Hand);
                    }
                    if (Duel.Fields[1] != null)
                    {
                        lpOpp = Duel.Fields[1].LifePoints;
                        oppMonstersJson = SerializeMonsterZoneWithDanger(Duel.Fields[1].MonsterZone);
                        oppSpellsJson = SerializeSpellZone(Duel.Fields[1].SpellZone);

                        int opponentHandCount = Duel.Fields[1].Hand != null ? Duel.Fields[1].Hand.Count : 0;
                        double fieldDanger = CalculateTotalDangerForField();
                        opponentThreat = fieldDanger + (opponentHandCount * 8.0);
                    }
                }

                string json = string.Format(
                    "{{\"turn\":{0},\"card_id\":{1},\"card_name\":\"{2}\",\"action\":\"{3}\",\"goal\":\"{4}\",\"score\":{5},\"decision\":{6},\"plan\":\"{7}\",\"lp_self\":{8},\"lp_opp\":{9},\"opponent_threat\":{10},\"bot_monsters\":{11},\"opp_monsters\":{12},\"opp_spells\":{13},\"bot_hand\":{14}}}",
                    _turnCount, cardId, GetCardName(cardId).Replace("\"", "'"), action, goal, score.ToString("F1", System.Globalization.CultureInfo.InvariantCulture),
                    decision ? "true" : "false", plan,
                    lpSelf, lpOpp, opponentThreat.ToString("F1", System.Globalization.CultureInfo.InvariantCulture),
                    botMonstersJson, oppMonstersJson, oppSpellsJson, botHandJson);
                File.AppendAllText(_decisionsLogPath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[IgnisEngine] LogDecision error: " + ex.Message);
            }
        }

        protected string GetCardName(int id)
        {
            if (_cardNames.ContainsKey(id))
                return _cardNames[id];
            return "Unknown Card (" + id + ")";
        }

        protected string ReadFileWithRetry(string filePath)
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

        protected void WriteFileWithRetry(string filePath, string content)
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

        protected void LoadConfiguration()
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
                        if (item == null || !item.ContainsKey("id") || item["id"] == null)
                            continue;

                        var card = new CardMetadata
                        {
                            id = Convert.ToInt32(item["id"]),
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
                    
                    if (rawDict.ContainsKey("playstyle") && rawDict["playstyle"] != null)
                    {
                        _deckConfig.playstyle = rawDict["playstyle"].ToString();
                    }
                    else
                    {
                        _deckConfig.playstyle = "control";
                    }
                    
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

                // 5. Load precise attack locks
                string locksPath = Path.Combine(baseDir, "config", "precise_attack_locks.json");
                if (File.Exists(locksPath))
                {
                    string json = ReadFileWithRetry(locksPath);
                    var serializer = new JavaScriptSerializer();
                    var rawList = serializer.Deserialize<List<Dictionary<string, object>>>(json);
                    foreach (var item in rawList)
                    {
                        if (item != null && item.ContainsKey("id") && item.ContainsKey("lock_type"))
                        {
                            int cardId = Convert.ToInt32(item["id"]);
                            string lockType = item["lock_type"].ToString();
                            _attackLocks[cardId] = lockType;
                        }
                    }
                    Log("Successfully loaded " + _attackLocks.Count + " attack locks from registry.");
                }

                _resolvedBaseDir = baseDir; // Store resolved baseDir! (comment updated)
            }
            catch (Exception ex)
            {
                Log("Error loading configuration: " + ex.Message);
            }
        }

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
                                    diskRegistry[cardId] = card;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogToMatch("Error merging disk cards registry: " + ex.Message);
                        }
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
                    LogToMatch("Saved " + regList.Count + " cards to " + registryPath + " (backup: " + backupPath + ")");

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
                        catch (Exception ex)
                        {
                            LogToMatch("Error merging disk opponent memory: " + ex.Message);
                        }
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
                    LogToMatch("Saved " + oppDict.Count + " opponent cards to " + oppMemoryPath);
                }
                catch (Exception ex)
                {
                    LogToMatch("Error saving configuration: " + ex.Message);
                }
            }
        }

        protected void RecordOpponentCardSeen(int cardId)
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

        protected void ApplyRealTimeLearning()
        {
            lock (_staticLock)
            {
                if (_learningApplied) return;

                bool hasDuelState = (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null);
                int botLP = hasDuelState ? Duel.Fields[0].LifePoints : _lastBotLP;
                int oppLP = hasDuelState ? Duel.Fields[1].LifePoints : _lastOppLP;

                // Prevent learning if _ourCardsPlayed.Count == 0 (aborted matches),
                // EXCEPT if the game actually finished with LP = 0 (e.g. OTK/FTK/loss on turn 1)
                if (_ourCardsPlayed.Count == 0 && botLP != 0 && oppLP != 0)
                {
                    return;
                }

                _learningApplied = true;
                _needsReset = true;

                try
                {
                    string outcome = "Unknown";
                    if (botLP == 0 && oppLP > 0) outcome = "Loss";
                    else if (oppLP == 0 && botLP > 0) outcome = "Win";
                    else
                    {
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
                                    meta.priority = Math.Min(8, meta.priority + delta);
                                }
                                if (_turnCount >= 2 && (meta.roles.Contains("extender") || meta.roles.Contains("combo_piece")))
                                {
                                    int delta = (outcome == "Win") ? 1 : 0;
                                    meta.followup_value = Math.Min(10, meta.followup_value + delta);
                                }
                            }
                            else if (outcome == "Loss" || outcome == "WeakLoss")
                            {
                                int delta = 0;
                                if (outcome == "Loss" && meta.priority > 1) delta = 1;
                                else if (outcome == "WeakLoss" && meta.priority > 2) delta = 1;
                                meta.priority = Math.Max(1, meta.priority - delta);
                                if (_disruptionsInMatch.ContainsKey(cardId) && _disruptionsInMatch[cardId].Count > 0)
                                {
                                    meta.risk_if_negated = Math.Min(10, meta.risk_if_negated + 1);
                                }
                            }

                            if (meta.priority != oldPriority || meta.risk_if_negated != oldRisk || meta.followup_value != oldFollowup || meta.bait_value != oldBait)
                            {
                                LogToMatch(string.Format("  Card {0} ({1}) adjusted: priority {2}->{3}, risk {4}->{5}, followup {6}->{7}, bait {8}->{9}",
                                    cardId, GetCardName(cardId), oldPriority, meta.priority, oldRisk, meta.risk_if_negated, oldFollowup, meta.followup_value, oldBait, meta.bait_value));
                            }
                        }
                    }

                    // Adjust bait values for non-disrupted cards to encourage baiting (run ONCE per match for Loss/WeakLoss)
                    if ((outcome == "Loss" || outcome == "WeakLoss") && _disruptionsInMatch.Count > 0)
                    {
                        foreach (var otherId in _cardRegistry.Keys)
                        {
                            var otherMeta = _cardRegistry[otherId];
                            if (!_ourCardsPlayed.Contains(otherId) && !otherMeta.roles.Contains("starter") && !otherMeta.roles.Contains("payoff"))
                            {
                                if (otherMeta.bait_value > 0 && otherMeta.bait_value < 6)
                                {
                                    otherMeta.bait_value++;
                                    LogToMatch(string.Format("  Bait Value Inflation: Card {0} ({1}) bait_value increased to {2}",
                                        otherId, GetCardName(otherId), otherMeta.bait_value));
                                }
                            }
                        }
                    }

                    // Anti-Inflation Decay: reduce priority for high-priority unplayed cards (run BEFORE hard cap)
                    // Running before hard cap ensures cards at priority 8 get decayed if not played
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

                    // Hard Cap: Prevent any card from exceeding priority 8 via learning (run AFTER decay)
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
                        bool isOurChokePoint = _deckConfig.choke_points != null && _deckConfig.choke_points.Contains(ourCardId);

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
        }

        protected double CalculateTotalDangerForField()
        {
            double total = 0.0;
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[1] == null) return total;
            
            if (Duel.Fields[1].MonsterZone != null)
            {
                foreach (var m in Duel.Fields[1].MonsterZone)
                {
                    if (m != null) total += CalculateCardDanger(m);
                }
            }
            if (Duel.Fields[1].SpellZone != null)
            {
                foreach (var s in Duel.Fields[1].SpellZone)
                {
                    if (s != null) total += CalculateCardDanger(s);
                }
            }
            // Check Graveyard danger
            if (Duel.Fields[1].Graveyard != null)
            {
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
            }
            // Check Hand danger (revealed/visible cards only)
            if (Duel.Fields[1].Hand != null)
            {
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
            }
            // Check Banished danger (revealed/face-up cards only)
            if (Duel.Fields[1].Banished != null)
            {
                foreach (var b in Duel.Fields[1].Banished)
                {
                    if (b != null && b.Id > 0)
                    {
                        double danger = CalculateCardDanger(b);
                        if (_cardRegistry.ContainsKey(b.Id))
                        {
                            var meta = _cardRegistry[b.Id];
                            if (meta.roles.Contains("recovery") || meta.roles.Contains("starter") || meta.roles.Contains("extender") || meta.roles.Contains("payoff"))
                            {
                                total += danger * 0.4;
                            }
                        }
                        else if (GetStapleBaselineDanger(b.Id) > 0)
                        {
                            total += danger * 0.2;
                        }
                    }
                }
            }
            return total;
        }

        protected void UpdateGoal()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null) return;
            
            int selfLP = Duel.Fields[0].LifePoints;
            int enemyLP = Duel.Fields[1].LifePoints;

            int totalAttack = 0;
            if (Duel.Fields[0].MonsterZone != null)
            {
                foreach (var card in Duel.Fields[0].MonsterZone)
                {
                    if (card != null && card.Position == (int)CardPosition.FaceUpAttack)
                    {
                        totalAttack += card.Attack;
                    }
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

        protected bool HasBaitInHand()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 1 || Duel.Fields[0] == null || Duel.Fields[0].Hand == null) return false;
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

        protected int GetZoneCount(IList<ClientCard> zone)
        {
            int count = 0;
            if (zone == null) return count;
            foreach (var card in zone)
            {
                if (card != null) count++;
            }
            return count;
        }

        protected double GetStapleBaselineDanger(int cardId)
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

        protected double CalculateCardDanger(ClientCard enemyCard)
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
            CardMetadata meta = GetOrCreateMetadata(enemyCard);
            if (meta != null)
            {
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

        protected int GetIntOrDefault(Dictionary<string, object> dict, string key, int defaultValue)
        {
            object value;
            if (dict != null && dict.TryGetValue(key, out value) && value != null)
            {
                try
                {
                    return Convert.ToInt32(value);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        protected bool IsFaceUp(ClientCard card)
        {
            if (card == null) return false;
            return card.IsFaceup();
        }

        protected bool IsLightOrDark(ClientCard card)
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

        protected int GetOpponentFaceUpMonsterCount()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[1] == null || Duel.Fields[1].MonsterZone == null)
                return 0;
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

        protected int GetOpponentGraveMonsterCount()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[1] == null || Duel.Fields[1].Graveyard == null)
                return 0;
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

        protected int GetOpponentGraveLightDarkCount()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[1] == null || Duel.Fields[1].Graveyard == null)
                return 0;
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

        protected int GetBotGraveLightDarkCount()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 1 || Duel.Fields[0] == null || Duel.Fields[0].Graveyard == null)
                return 0;
            int count = 0;
            foreach (var botCard in Duel.Fields[0].Graveyard)
            {
                if (botCard != null && botCard.IsMonster() && IsLightOrDark(botCard))
                {
                    count++;
                }
            }
            return count;
        }

        protected CardMetadata GetOrCreateMetadata(ClientCard card)
        {
            if (card == null) return null;
            int cardId = card.Id;
            if (_cardRegistry.ContainsKey(cardId))
            {
                return _cardRegistry[cardId];
            }

            CardMetadata meta = new CardMetadata();
            meta.id = cardId;
            meta.priority = 5;
            meta.risk_if_negated = 3;
            meta.bait_value = 0;
            meta.followup_value = 5;
            meta.recovery_value = 5;
            meta.roles = new ArrayList();
            meta.combo_plans = new ArrayList { "PlanA" };
            meta.q_values = new Dictionary<string, object>();

            if (card.IsMonster())
            {
                bool isExtra = card.HasType(CardType.Fusion) || card.HasType(CardType.Ritual) || card.HasType(CardType.Synchro) || card.HasType(CardType.Xyz) || card.HasType(CardType.Link);
                if (isExtra)
                {
                    meta.roles.Add("payoff");
                    meta.priority = 7;
                }
                else
                {
                    meta.roles.Add("combo_piece");
                }

                if (card.IsTuner())
                {
                    meta.roles.Add("tuner");
                }

                if (card.Attack >= 2500)
                {
                    if (!meta.roles.Contains("payoff")) meta.roles.Add("payoff");
                    meta.priority = Math.Max(meta.priority, 7);
                }

                if (GetStapleBaselineDanger(cardId) > 0 && !isExtra)
                {
                    meta.roles.Add("handtrap");
                    meta.roles.Add("interruption");
                    meta.priority = 8;
                }
            }
            else if (card.IsSpell())
            {
                if (card.HasType(CardType.QuickPlay))
                {
                    meta.roles.Add("interruption");
                    meta.priority = 6;
                }
                else if (card.HasType(CardType.Field) || card.HasType(CardType.Continuous))
                {
                    meta.roles.Add("starter");
                    meta.priority = 6;
                }
                else
                {
                    meta.roles.Add("starter");
                }
            }
            else if (card.IsTrap())
            {
                meta.roles.Add("interruption");
                if (card.HasType(CardType.Continuous))
                {
                    meta.roles.Add("floodgate");
                }
                meta.priority = 6;
            }

            if (card.Controller == 0)
            {
                _cardRegistry[cardId] = meta;
                string rolesStr = "";
                foreach (var r in meta.roles) rolesStr += r.ToString() + " ";
                Log("Dynamically registered unknown Card ID: " + cardId + " (" + GetCardName(cardId) + ") with priority " + meta.priority + " and roles: " + rolesStr.Trim());
            }

            return meta;
        }

        protected virtual bool EvaluateCardAction(ClientCard card, CardMetadata meta, ExecutorType type)
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

                // General Rule: Never chain an interruption / handtrap / negate / removal to our own card activation
                if (lastChainCard != null && lastChainCard.Controller == 0)
                {
                    if (meta.roles.Contains("interruption") || meta.roles.Contains("handtrap") || meta.roles.Contains("disruption") || meta.roles.Contains("negate") || meta.roles.Contains("removal"))
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
                        LogToTurn(string.Format("Block disruptive handtrap {0} on our own turn.", GetCardName(card.Id)));
                        return false;
                    }
                }

                // Specific card safeguards to prevent self-sabotage/illegal activations
                
                // 1. Droll & Lock Bird (ID: 94145021) - Only activate on opponent's turn to avoid locking ourselves
                if (card.Id == 94145021 && Duel.Player == 0)
                {
                    LogToTurn("Block activating Droll & Lock Bird on our own turn.");
                    return false;
                }

                // 2. Effect Veiler (ID: 97268402) - Only activate on opponent's turn and during opponent's Main Phase, and opponent controls face-up monster
                if (card.Id == 97268402)
                {
                    if (Duel.Player == 0 || (Duel.Phase != DuelPhase.Main1 && Duel.Phase != DuelPhase.Main2) || GetOpponentFaceUpMonsterCount() == 0)
                    {
                        LogToTurn("Block activating Effect Veiler (must be opponent's Main Phase with face-up opponent monster).");
                        return false;
                    }
                }

                // 3. Called by the Grave (ID: 24224830) - Only activate if there is a target in the opponent's GY
                if (card.Id == 24224830)
                {
                    if (GetOpponentGraveMonsterCount() == 0)
                    {
                        LogToTurn("Block Called by the Grave: No monsters in opponent's GY to target.");
                        return false;
                    }
                }

                // 4. Bystials: Druiswurm (ID: 6637331) & Magnamhut (ID: 33854624) - Only activate if opponent or bot has LIGHT/DARK monster in GY
                if (card.Id == 6637331 || card.Id == 33854624)
                {
                    if (GetOpponentGraveLightDarkCount() + GetBotGraveLightDarkCount() == 0)
                    {
                        LogToTurn(string.Format("Block Bystial {0}: No LIGHT/DARK monsters in either GY to banish.", GetCardName(card.Id)));
                        return false;
                    }
                }

                // 5. Infinite Impermanence (ID: 10045474) - require target
                if (card.Id == 10045474)
                {
                    if (GetOpponentFaceUpMonsterCount() == 0)
                    {
                        LogToTurn("Block Infinite Impermanence activation: No face-up monsters on opponent's field to target.");
                        return false;
                    }
                }

                // 6. Mulcharmy Fuwalos / Maxx 'C' style: only activate on opponent's turn
                if (card.Id == 42141493 && Duel.Player == 0)
                {
                    LogToTurn("Block Mulcharmy Fuwalos on our own turn.");
                    return false;
                }

                // 7. Nibiru, the Primal Being (ID: 27204311) — Only activate if opponent summoned 5+ monsters this turn
                if (card.Id == 27204311)
                {
                    if (Duel.Player == 0)
                    {
                        LogToTurn("Block Nibiru on our own turn.");
                        return false;
                    }
                }

                // 8. PSY-Framegear Gamma (ID: 38814750) — Only activate if we control no monsters and responding to opponent monster effect
                if (card.Id == 38814750)
                {
                    int ourMonCount = GetZoneCount(Duel.Fields[0].MonsterZone);
                    if (ourMonCount > 0)
                    {
                        LogToTurn("Block PSY-Framegear Gamma: We control a monster.");
                        return false;
                    }
                    if (lastChainCard == null || lastChainCard.Controller != 1 || !lastChainCard.IsMonster())
                    {
                        LogToTurn("Block PSY-Framegear Gamma: Last chain card is null, not controlled by opponent, or not a monster.");
                        return false;
                    }
                }

                // 9. Aleister the Invoker (ID: 86120751) — Only activate hand effect during Battle Phase (outside is waste of ATK boost) (comment updated)
                if (card.Id == 86120751 && card.Location == CardLocation.Hand)
                {
                    if (Duel.Phase != DuelPhase.Battle)
                    {
                        LogToTurn("Block activating Aleister the Invoker hand effect outside of the Battle Phase.");
                        return false;
                    }
                }
            }

            double score = meta.priority * 10.0;
            score += GetLookaheadBonus(card, meta, type);

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

            int selfHandCount = (Duel.Fields != null && Duel.Fields.Length > 0 && Duel.Fields[0] != null && Duel.Fields[0].Hand != null) ? Duel.Fields[0].Hand.Count : 0;
            int opponentHandCount = (Duel.Fields != null && Duel.Fields.Length > 1 && Duel.Fields[1] != null && Duel.Fields[1].Hand != null) ? Duel.Fields[1].Hand.Count : 0;
            int selfMonsters = Bot != null && Bot.MonsterZone != null ? GetZoneCount(Bot.MonsterZone) : 0;
            int opponentMonsters = Enemy != null && Enemy.MonsterZone != null ? GetZoneCount(Enemy.MonsterZone) : 0;
            int selfSpells = Bot != null && Bot.SpellZone != null ? GetZoneCount(Bot.SpellZone) : 0;
            int opponentSpells = Enemy != null && Enemy.SpellZone != null ? GetZoneCount(Enemy.SpellZone) : 0;

            int selfLP = (Duel.Fields != null && Duel.Fields.Length > 0 && Duel.Fields[0] != null) ? Duel.Fields[0].LifePoints : 8000;
            int enemyLP = (Duel.Fields != null && Duel.Fields.Length > 1 && Duel.Fields[1] != null) ? Duel.Fields[1].LifePoints : 8000;

            double fieldDanger = CalculateTotalDangerForField();
            double opponentThreat = fieldDanger + (opponentHandCount * 8.0);

            // 1. Goal adjustments
            if (_currentGoal == "push_lethal")
            {
                if (meta.roles.Contains("starter") || meta.roles.Contains("extender"))
                    score += 25.0;
                if (meta.roles.Contains("payoff"))
                    score += 35.0;
                if (meta.roles.Contains("combo_piece"))
                    score += 20.0;
                if (meta.roles.Contains("tuner"))
                {
                    if (selfMonsters >= 1)
                        score += 20.0;
                    else
                        score -= 10.0;
                }
                if (meta.roles.Contains("searcher"))
                    score += 10.0;
                if (meta.roles.Contains("disruption"))
                    score += 5.0;
            }
            else if (_currentGoal == "survive")
            {
                if (meta.roles.Contains("recovery"))
                    score += 30.0;
                if (meta.roles.Contains("interruption") || meta.roles.Contains("floodgate"))
                    score += 25.0;
                score += meta.recovery_value * 3.0;
                if (meta.roles.Contains("disruption"))
                    score += 20.0;
            }
            else if (_currentGoal == "break_board")
            {
                if (meta.roles.Contains("removal"))
                    score += 35.0;
                if (meta.roles.Contains("interruption") || meta.roles.Contains("disruption"))
                    score += 20.0;
                if (meta.roles.Contains("starter") || meta.roles.Contains("extender"))
                    score += 15.0;
            }
            else // establish_interruptions
            {
                if (meta.roles.Contains("starter") && selfMonsters == 0)
                    score += 20.0;
                if (meta.roles.Contains("interruption"))
                    score += 15.0;
                if (meta.roles.Contains("combo_piece") && selfMonsters < 2)
                    score += 15.0;
                if (meta.roles.Contains("disruption"))
                    score += 20.0;
                if (meta.roles.Contains("tuner"))
                {
                    if (selfMonsters >= 1)
                        score += 15.0;
                    else
                        score -= 10.0;
                }
                if (selfLP < 5000)
                    score += meta.recovery_value * 1.5;
            }

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
                    if (meta.roles.Contains("starter") || meta.roles.Contains("extender") || meta.roles.Contains("combo_piece") || meta.roles.Contains("payoff"))
                    {
                        score -= 90.0;
                        LogToTurn(string.Format("Penalizing dead combo card: {0} because its plan is blocked.", GetCardName(card.Id)));
                    }
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
            int deckCount = Duel.Fields[0].Deck.Count;
            int cardAdvantage = selfHandCount + selfMonsters - (opponentHandCount + Enemy.GetMonsterCount());

            if (deckCount <= 5 && (meta.roles.Contains("draw") || meta.roles.Contains("searcher")))
            {
                score -= 50.0;
                LogToTurn(string.Format("Resource Tracking: Low deck count ({0})! Penalizing draw/search card: {1} (-50.0)", deckCount, GetCardName(card.Id)));
            }

            if (cardAdvantage <= -3)
            {
                if (meta.roles.Contains("recovery") || meta.roles.Contains("searcher"))
                {
                    score += 15.0;
                    LogToTurn(string.Format("Resource Tracking: Card disadvantage detected ({0}). Boosting recovery/search: {1} (+15.0)", cardAdvantage, GetCardName(card.Id)));
                }
            }

            if (selfHandCount <= 2)
            {
                if (meta.roles.Contains("starter") || meta.roles.Contains("recovery"))
                    score += 20.0;
                if (meta.followup_value > 5)
                    score += meta.followup_value * 2.5;
            }
            else if (selfHandCount >= 5 && selfMonsters >= 3)
            {
                if (meta.roles.Contains("starter") || meta.roles.Contains("extender"))
                    score -= 15.0;
            }

            if (Duel.Player == 0 && meta.followup_value > 6)
                score += meta.followup_value * 1.5;

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
                    score += danger;
                    
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
                    score += maxEnemyDanger * 1.5;
                    LogToTurn(string.Format("Removal evaluated. Highest danger target on field: {0} (ID: {1}) | Danger: {2:F1}", 
                        GetCardName(bestTarget.Id), bestTarget.Id, maxEnemyDanger));
                }
                else
                {
                    score -= 30.0;
                }
            }

            // 6. Zone Limit check
            if ((type == ExecutorType.Summon || type == ExecutorType.SpSummon) && selfMonsters >= 5)
            {
                if (type == ExecutorType.SpSummon && card.Location == CardLocation.Extra)
                {
                    // Allow Extra Deck summons
                }
                else
                {
                    LogToTurn(string.Format("Zone Limit reached: selfMonsters = {0}. Summon action rejected.", selfMonsters));
                    LogDecision(card.Id, type.ToString(), _currentGoal, 0.0, false, _currentPlan);
                    return false;
                }
            }

            // 7. Macro-Decision Refactoring Upgrades
            // 7.1 Anti-Overextension / Lethal Check
            if (Duel != null && Duel.Phase == DuelPhase.Main1 && IsLethalOnBoard())
            {
                if (meta.roles.Contains("combo") || meta.roles.Contains("extender") || meta.roles.Contains("starter") || meta.roles.Contains("combo_piece") || meta.roles.Contains("searcher") || meta.roles.Contains("draw"))
                {
                    score -= 1000.0;
                    LogToTurn(string.Format("Lethal on board detected! Penalizing overextending card: {0} (-1000.0)", GetCardName(card.Id)));
                }
            }

            // 7.2 Redundant Field Spell Protection
            if (type == ExecutorType.Activate && card.HasType(CardType.Field))
            {
                if (Bot != null && Bot.SpellZone != null)
                {
                    var currentField = Bot.SpellZone[5];
                    if (currentField != null && IsFaceUp(currentField) && currentField.Id == card.Id && currentField != card && currentField.Location == CardLocation.SpellZone)
                    {
                        score -= 500.0;
                        LogToTurn(string.Format("Redundant Field Spell detected! Penalizing duplicate: {0} (-500.0)", GetCardName(card.Id)));
                    }
                }
            }

            // 7.3 Hand Overflow Safeguard: if hand is full (>= 6) and it's our turn, boost score to encourage play
            if (Duel != null && Duel.Player == 0 && selfHandCount >= 6)
            {
                score += 15.0;
            }

            // --- HYBRID BOARD READING WEIGHTING LAYER ---
            bool hasSkillDrain = false;
            bool hasBagooskaDefense = false;
            bool hasEnemyNegators = false;

            if (Duel != null && Duel.Fields != null && Duel.Fields.Length > 1 && Duel.Fields[1] != null)
            {
                var oppSpellZone = Duel.Fields[1].SpellZone;
                if (oppSpellZone != null)
                {
                    foreach (var s in oppSpellZone)
                    {
                        if (s != null && IsFaceUp(s))
                        {
                            if (s.Id == 82732705) hasSkillDrain = true;
                        }
                    }
                }

                var oppMonsterZone = Duel.Fields[1].MonsterZone;
                if (oppMonsterZone != null)
                {
                    foreach (var m in oppMonsterZone)
                    {
                        if (m != null && IsFaceUp(m))
                        {
                            if (m.Id == 90590303 && m.IsDefense()) hasBagooskaDefense = true;

                            if (CalculateCardDanger(m) >= 45.0 && !m.IsDisabled())
                            {
                                hasEnemyNegators = true;
                            }
                        }
                    }
                }

                var ourSpellZone = Duel.Fields[0].SpellZone;
                if (ourSpellZone != null)
                {
                    foreach (var s in ourSpellZone)
                    {
                        if (s != null && IsFaceUp(s) && s.Id == 82732705) hasSkillDrain = true;
                    }
                }

                var ourMonsterZone = Duel.Fields[0].MonsterZone;
                if (ourMonsterZone != null)
                {
                    foreach (var m in ourMonsterZone)
                    {
                        if (m != null && IsFaceUp(m) && m.Id == 90590303 && m.IsDefense()) hasBagooskaDefense = true;
                    }
                }
            }

            if (hasSkillDrain)
            {
                if (type == ExecutorType.Activate && card.IsMonster() && card.Location == CardLocation.MonsterZone)
                {
                    score -= 40.0;
                    LogToTurn("Skill Drain active! Penalizing on-field monster activation (-40.0)");
                }
                if (meta.roles.Contains("removal"))
                {
                    score += 25.0;
                    LogToTurn("Skill Drain active! Boosting removal card (+25.0)");
                }
            }

            if (hasBagooskaDefense)
            {
                if (type == ExecutorType.Activate && card.IsMonster() && card.Location == CardLocation.MonsterZone && card.IsDefense())
                {
                    score -= 30.0;
                    LogToTurn("Bagooska active! Penalizing defense monster activation (-30.0)");
                }
            }

            if (hasEnemyNegators && Duel != null && Duel.Player == 0)
            {
                if (meta.roles.Contains("bait") || meta.bait_value > 5)
                {
                    score += 20.0;
                    LogToTurn("Enemy negators detected! Boosting bait card (+20.0)");
                }
                else if (meta.roles.Contains("starter") && meta.risk_if_negated > 5)
                {
                    if (!HasBaitInHand())
                    {
                        score -= 20.0;
                        LogToTurn("Enemy negators detected with no bait in hand! Penalizing high-risk starter (-20.0)");
                    }
                }
            }

            if (selfLP < 3000 && opponentThreat > 30.0)
            {
                if (type == ExecutorType.MonsterSet)
                {
                    score += 25.0;
                    LogToTurn("Low LP and high opponent threat! Boosting monster set (+25.0)");
                }
            }

            LogToTurn(string.Format("Analysing Card: {0} (ID: {1}) | Action: {2} | Goal: {3} | Opp Threat: {4:F1} | Score: {5:F1}",
                GetCardName(card.Id), card.Id, type, _currentGoal, opponentThreat, score));

            double threshold = 35.0;
            if (selfLP <= 2000) threshold = 15.0;
            else if (selfLP <= 4000) threshold = 25.0;
            else if (enemyLP <= 2000 && _currentGoal == "push_lethal") threshold = 10.0;
            else if (opponentThreat > 80.0) threshold = 20.0;

            bool decision = score > threshold;
            LogDecision(card.Id, type.ToString(), _currentGoal, score, decision, _currentPlan);
            return decision;
        }

        protected double GetLookaheadBonus(ClientCard card, CardMetadata meta, ExecutorType type)
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null)
                return 0.0;

            double bonus = 0.0;
            int selfHandCount = Duel.Fields[0].Hand.Count;
            int selfMonsters = GetZoneCount(Duel.Fields[0].MonsterZone);

            // 1. Searcher/Draw Lookahead: If we have no starter/extender in hand, draw/search is our only lifeline
            if (meta.roles.Contains("searcher") || meta.roles.Contains("draw"))
            {
                if (!HasStarterOrExtenderInHand())
                {
                    bonus += 25.0;
                    LogToTurn(string.Format("Lookahead: Searcher/Draw {0} is highly valued to find starters.", GetCardName(card.Id)));
                }
                else
                {
                    bonus += 10.0;
                }
            }

            // 2. Extender/Combo Piece Synergy Lookahead:
            if (meta.roles.Contains("extender") || meta.roles.Contains("combo_piece"))
            {
                if (selfMonsters > 0 || HasStarterInHand())
                {
                    bonus += 15.0;
                    LogToTurn(string.Format("Lookahead: Extender/Combo Piece {0} is live or will be enabled.", GetCardName(card.Id)));
                }
                else
                {
                    bonus -= 20.0;
                    LogToTurn(string.Format("Lookahead: Penalizing extender {0} due to no starters/monsters.", GetCardName(card.Id)));
                }
            }

            // 3. Tuner / Synchro / Link Materials Lookahead:
            if (meta.roles.Contains("tuner"))
            {
                bool hasNonTuner = false;
                foreach (var c in Duel.Fields[0].Hand)
                {
                    if (c != null && c.IsMonster() && !c.IsTuner() && c.Id != card.Id)
                    {
                        hasNonTuner = true;
                        break;
                    }
                }
                foreach (var c in Duel.Fields[0].MonsterZone)
                {
                    if (c != null && IsFaceUp(c) && !c.IsTuner())
                    {
                        hasNonTuner = true;
                        break;
                    }
                }

                if (hasNonTuner)
                {
                    bonus += 15.0;
                    LogToTurn(string.Format("Lookahead: Tuner {0} has non-tuner materials available.", GetCardName(card.Id)));
                }
            }

            // 4. Protection Lookahead: If we have a high-risk starter in hand, keep protection/negates active
            if (card.Id == 24224830 || meta.roles.Contains("negate")) // Called by the Grave or negator
            {
                bool hasHighRiskStarter = false;
                foreach (var c in Duel.Fields[0].Hand)
                {
                    if (c != null && _cardRegistry.ContainsKey(c.Id))
                    {
                        var m = _cardRegistry[c.Id];
                        if (m.roles.Contains("starter") && m.risk_if_negated >= 6)
                        {
                            hasHighRiskStarter = true;
                            break;
                        }
                    }
                }

                if (hasHighRiskStarter)
                {
                    bonus += 20.0;
                    LogToTurn(string.Format("Lookahead: Boosting protector/negator {0} to shield high-risk starter.", GetCardName(card.Id)));
                }
            }

            return bonus;
        }

        protected bool HasStarterInHand()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 1 || Duel.Fields[0] == null || Duel.Fields[0].Hand == null)
                return false;
            foreach (var card in Duel.Fields[0].Hand)
            {
                if (card != null && card.IsMonster())
                {
                    var meta = GetOrCreateMetadata(card);
                    if (meta != null && meta.roles.Contains("starter"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool OnCardAction(int cardId, ExecutorType type)
        {
            UpdateGoal();

            ClientCard card = Card;
            if (card == null)
                return false;

            CardMetadata meta = GetOrCreateMetadata(card);
            bool result = EvaluateCardAction(card, meta, type);
            if (result)
            {
                if (!_ourCardsPlayed.Contains(cardId))
                    _ourCardsPlayed.Add(cardId);
            }
            return result;
        }

        protected bool OnDefaultActivate()
        {
            ClientCard card = Card;
            if (card != null)
            {
                CardMetadata meta = GetOrCreateMetadata(card);
                bool decision = EvaluateCardAction(card, meta, ExecutorType.Activate);
                if (decision)
                {
                    LogToTurn(string.Format("{0} Activated [Fallback]: {1} (ID: {2})", _resolvedDeckName, GetCardName(card.Id), card.Id));
                }
                return decision;
            }
            return false;
        }

        protected bool HasStarterOrExtenderInHand()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 1 || Duel.Fields[0] == null || Duel.Fields[0].Hand == null)
                return false;
            foreach (var card in Duel.Fields[0].Hand)
            {
                if (card != null && card.IsMonster())
                {
                    var meta = GetOrCreateMetadata(card);
                    if (meta != null && (meta.roles.Contains("starter") || meta.roles.Contains("extender") || meta.roles.Contains("payoff") || meta.roles.Contains("searcher")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected int GetOpponentTotalAttack()
        {
            if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[1] == null || Duel.Fields[1].MonsterZone == null)
                return 0;
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

        protected bool OnDefaultSummon()
        {
            ClientCard card = Card;
            if (card == null) return false;

            CardMetadata meta = GetOrCreateMetadata(card);

            if (HasStarterOrExtenderInHand())
            {
                if (!meta.roles.Contains("starter") && !meta.roles.Contains("extender") && !meta.roles.Contains("payoff") && !meta.roles.Contains("searcher"))
                {
                    return false;
                }
            }

            if (meta.roles.Contains("handtrap"))
            {
                return false;
            }

            if (card.Attack < 1500 && card.Defense >= card.Attack)
            {
                return false;
            }

            bool decision = EvaluateCardAction(card, meta, ExecutorType.Summon);

            if (decision)
            {
                LogToTurn(string.Format("{0} Summoned [Fallback]: {1} (ID: {2}) | ATK={3} DEF={4}", 
                    _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
            }
            return decision;
        }

        protected bool OnDefaultSpSummon()
        {
            ClientCard card = Card;
            if (card != null)
            {
                CardMetadata meta = GetOrCreateMetadata(card);
                if (meta.roles.Contains("handtrap"))
                {
                    return false;
                }

                bool decision = EvaluateCardAction(card, meta, ExecutorType.SpSummon);
                if (decision)
                {
                    LogToTurn(string.Format("{0} SpSummoned [Fallback]: {1} (ID: {2}) | ATK={3} DEF={4}", 
                        _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
                }
                return decision;
            }
            return false;
        }

        protected bool OnDefaultSpellSet()
        {
            ClientCard card = Card;
            if (card == null) return false;

            CardMetadata meta = GetOrCreateMetadata(card);
            double score = meta.priority * 10.0;

            if ((card.IsTrap() || card.HasType(CardType.QuickPlay)) && Duel.Phase == DuelPhase.Main1 && Duel.Turn > 1)
            {
                score -= 30.0;
                LogToTurn(string.Format("Smart Trap Setting: Penalizing setting {0} in Main 1 (-30.0)", GetCardName(card.Id)));
            }

            int handCount = (Duel.Fields != null && Duel.Fields.Length > 0 && Duel.Fields[0] != null && Duel.Fields[0].Hand != null) ? Duel.Fields[0].Hand.Count : 0;
            if (handCount >= 6)
            {
                score += 100.0;
                LogToTurn(string.Format("Hand overflow ({0} cards)! Boosting set: {1} (+100.0)", handCount, GetCardName(card.Id)));
            }

            bool decision = score > 25.0;
            if (decision)
            {
                LogToTurn(string.Format("{0} Set Spell/Trap: {1} (ID: {2})", _resolvedDeckName, GetCardName(card.Id), card.Id));
            }
            return decision;
        }

        protected bool OnDefaultRepos()
        {
            ClientCard card = Card;
            if (card != null)
            {
                if (card.Attack >= card.Defense)
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
                    if (isAttack && (Duel.Phase == DuelPhase.Main2 || card.Attacked || card.Attack == 0))
                    {
                        LogToTurn(string.Format("{0} Repositioned to Defense: {1} (ID: {2}) | ATK={3} DEF={4}", 
                            _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
                        return true;
                    }
                }
            }
            return false;
        }

        protected bool OnDefaultMonsterSet()
        {
            ClientCard card = Card;
            if (card == null) return false;

            CardMetadata meta = GetOrCreateMetadata(card);

            if (HasStarterOrExtenderInHand())
            {
                return false;
            }

            int selfMonsters = Bot != null && Bot.MonsterZone != null ? GetZoneCount(Bot.MonsterZone) : 0;
            int opponentMonsters = Enemy != null && Enemy.MonsterZone != null ? GetZoneCount(Enemy.MonsterZone) : 0;
            int oppAttack = GetOpponentTotalAttack();
            int selfLP = (Duel.Fields != null && Duel.Fields.Length > 0 && Duel.Fields[0] != null) ? Duel.Fields[0].LifePoints : 8000;
            bool isDesperate = (oppAttack >= selfLP) || (selfLP < 3000);

            if (meta.roles.Contains("handtrap"))
            {
                if (selfMonsters == 0 && opponentMonsters > 0 && isDesperate)
                {
                    LogToTurn(string.Format("{0} Set Handtrap as Defensive Wall [Desperation]: {1} (ID: {2}) | LP={3} OppATK={4}", 
                        _resolvedDeckName, GetCardName(card.Id), card.Id, selfLP, oppAttack));
                    return true;
                }
                return false;
            }

            if (selfMonsters == 0)
            {
                LogToTurn(string.Format("{0} Set Monster in Defense [Fallback Wall]: {1} (ID: {2}) | ATK={3} DEF={4}", 
                    _resolvedDeckName, GetCardName(card.Id), card.Id, card.Attack, card.Defense));
                return true;
            }

            return false;
        }

        protected void LogState()
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

        public override void OnNewTurn()
        {
            UpdateLastKnownLP();
            try
            {
                if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
                {
                    return;
                }

                if (_needsReset || Duel.Turn < _turnCount || (Duel.Turn == 1 && _turnCount > 1))
                {
                    ResetDuelState();
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
            }
            catch (Exception ex)
            {
                Log("Error in OnNewTurn hook: " + ex.Message);
            }
            finally
            {
                try
                {
                    base.OnNewTurn();
                }
                catch (Exception ex)
                {
                    Log("Error calling base.OnNewTurn: " + ex.Message);
                }
            }
        }

        public override void OnNewPhase()
        {
            UpdateLastKnownLP();
            try
            {
                if (Duel == null)
                {
                    return;
                }
                LogToTurn("--- Phase Changed to: " + Duel.Phase.ToString() + " ---");
            }
            catch (Exception ex)
            {
                Log("Error in OnNewPhase hook: " + ex.Message);
            }
            finally
            {
                try
                {
                    base.OnNewPhase();
                }
                catch (Exception ex)
                {
                    Log("Error calling base.OnNewPhase: " + ex.Message);
                }
            }
        }

        public override bool OnSelectHand()
        {
            UpdateLastKnownLP();
            try
            {
                if (_deckConfig != null)
                {
                    if (_deckConfig.playstyle == "combo" || _deckConfig.playstyle == "midrange")
                    {
                        LogToTurn(string.Format("Playstyle is {0}, selecting to go first.", _deckConfig.playstyle));
                        return true;
                    }
                    if (_deckConfig.playstyle == "control" || _deckConfig.playstyle == "go_second")
                    {
                        LogToTurn(string.Format("Playstyle is {0}, selecting to go second.", _deckConfig.playstyle));
                        return false;
                    }
                }
                LogToTurn("Selecting to go second.");
                return false;
            }
            catch (Exception ex)
            {
                Log("Error in OnSelectHand hook: " + ex.Message);
                try
                {
                    return base.OnSelectHand();
                }
                catch
                {
                    return false;
                }
            }
        }

        public override CardPosition OnSelectPosition(int cardId, IList<CardPosition> positions)
        {
            if (positions == null || positions.Count == 0)
                return CardPosition.FaceUpAttack;

            if (positions.Count == 1)
                return positions[0];

            YGOSharp.OCGWrapper.NamedCard cardData = YGOSharp.OCGWrapper.NamedCard.Get(cardId);
            if (cardData != null)
            {
                if (cardData.Attack <= 500)
                {
                    if (positions.Contains(CardPosition.FaceUpDefence))
                    {
                        LogToTurn(string.Format("OnSelectPosition: Selecting FaceUpDefence for low ATK card (ID: {0}, ATK: {1}).", cardId, cardData.Attack));
                        return CardPosition.FaceUpDefence;
                    }
                    if (positions.Contains(CardPosition.FaceDownDefence))
                    {
                        LogToTurn(string.Format("OnSelectPosition: Selecting FaceDownDefence for low ATK card (ID: {0}, ATK: {1}).", cardId, cardData.Attack));
                        return CardPosition.FaceDownDefence;
                    }
                }

                if (Duel.Turn == 1 || Duel.Phase >= DuelPhase.Main2)
                {
                    if (cardData.Attack <= cardData.Defense)
                    {
                        if (positions.Contains(CardPosition.FaceUpDefence))
                        {
                            return CardPosition.FaceUpDefence;
                        }
                    }
                }

                int oppMaxAtk = 0;
                if (Enemy != null && Enemy.MonsterZone != null)
                {
                    foreach (var m in Enemy.MonsterZone)
                    {
                        if (m != null && m.IsFaceup() && m.Attack > oppMaxAtk)
                        {
                            oppMaxAtk = m.Attack;
                        }
                    }
                }

                if (oppMaxAtk > cardData.Attack)
                {
                    if (positions.Contains(CardPosition.FaceUpDefence))
                    {
                        LogToTurn(string.Format("OnSelectPosition: Opponent has higher ATK monster ({0} ATK). Selecting FaceUpDefence for (ID: {1}, ATK: {2}).", oppMaxAtk, cardId, cardData.Attack));
                        return CardPosition.FaceUpDefence;
                    }
                }
            }

            return base.OnSelectPosition(cardId, positions);
        }

        protected bool IsProtectedBySleepingScapegoats(ClientCard card)
        {
            if (card == null) return false;
            
            bool hasToken = false;
            foreach (var monster in Enemy.GetMonsters())
            {
                if (monster != null && (monster.Id == 101402154 || monster.Id == 900000113))
                {
                    hasToken = true;
                    break;
                }
            }
            
            if (!hasToken) return false;
            
            int id = card.Id;
            return id == 101402001 || id == 101402002 || id == 101402003 || 
                   id == 101402004 || id == 101402036 || id == 101402052 || 
                   id == 101402054 || id == 101402070 || id == 101402071 ||
                   id == 900000006 || id == 900000007 || id == 900000008 ||
                   id == 900000009 || id == 900000010 || id == 900000011 ||
                   id == 900000013 || id == 900000014 || id == 900000015;
        }

        protected bool IsSafeAttack(ClientCard attacker, ClientCard defender)
        {
            if (attacker == null) return false;

            if (defender == null)
            {
                return attacker.CanDirectAttack;
            }

            attacker.RealPower = attacker.Attack;
            defender.RealPower = defender.GetDefensePower();

            if (!OnPreBattleBetween(attacker, defender))
                return false;

            if (_opponentMemory.ContainsKey(defender.Id))
            {
                double danger = _opponentMemory[defender.Id].learned_danger;
                if (danger > 80.0 && attacker.RealPower <= defender.RealPower + 1000)
                {
                    return false;
                }
            }

            if (attacker.RealPower == defender.RealPower && IsProtectedBySleepingScapegoats(defender))
            {
                return false;
            }

            if (attacker.RealPower > defender.RealPower)
            {
                return true;
            }

            if (attacker.RealPower == defender.RealPower && attacker.IsLastAttacker && defender.IsAttack())
            {
                return true;
            }

            return false;
        }

        public override BattlePhaseAction OnBattle(IList<ClientCard> attackers, IList<ClientCard> defenders)
        {
            UpdateLastKnownLP();
            try
            {
                if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
                {
                    return base.OnBattle(attackers, defenders);
                }

                if (IsLethalOnBoard())
                {
                    return null;
                }

                bool hasFaceDownBackrow = false;
                if (Duel.Fields[1].SpellZone != null)
                {
                    foreach (var card in Duel.Fields[1].SpellZone)
                    {
                        if (card != null && card.IsFacedown())
                        {
                            hasFaceDownBackrow = true;
                            break;
                        }
                    }
                }

                if (hasFaceDownBackrow)
                {
                    foreach (var kvp in _opponentMemory)
                    {
                        if (kvp.Value.learned_danger > 75.0 && GetStapleBaselineDanger(kvp.Key) > 0)
                        {
                            int cardId = kvp.Key;
                            if (cardId == 44095762 || cardId == 70342110 || cardId == 62279055 || cardId == 15693423)
                            {
                                LogToTurn(string.Format("Battle Phase: Opponent backrow detected with known dangerous battle trap in memory: {0}. Ending battle phase to play safely.", GetCardName(cardId)));
                                return new BattlePhaseAction(BattlePhaseAction.BattleAction.ToMainPhaseTwo);
                            }
                        }
                    }
                }

                foreach (var attacker in attackers)
                {
                    if (attacker == null || attacker.Attacked) continue;

                    if (defenders.Count == 0 || (Enemy != null && Enemy.GetMonsterCount() == 0))
                    {
                        if (IsSafeAttack(attacker, null))
                        {
                            LogToTurn(string.Format("Battle Phase: Declaring direct attack with {0}.", GetCardName(attacker.Id)));
                            return AI.Attack(attacker, null);
                        }
                    }

                    List<ClientCard> sortedDefenders = new List<ClientCard>(defenders);
                    sortedDefenders.Sort((a, b) =>
                    {
                        if (a == null && b == null) return 0;
                        if (a == null) return 1;
                        if (b == null) return -1;

                        bool aIsToken = a.Id == 101402154 || a.Id == 900000113 || (a.Name != null && a.Name.Contains("Token"));
                        bool bIsToken = b.Id == 101402154 || b.Id == 900000113 || (b.Name != null && b.Name.Contains("Token"));
                        if (aIsToken && !bIsToken) return -1;
                        if (!aIsToken && bIsToken) return 1;

                        int aPower = a.GetDefensePower();
                        int bPower = b.GetDefensePower();
                        return aPower.CompareTo(bPower);
                    });

                    foreach (var defender in sortedDefenders)
                    {
                        if (defender == null) continue;

                        if (IsSafeAttack(attacker, defender))
                        {
                            LogToTurn(string.Format("Battle Phase: Declaring attack: {0} (ATK={1}) -> {2} (DEF={3})", 
                                GetCardName(attacker.Id), attacker.Attack, GetCardName(defender.Id), defender.RealPower));
                            return AI.Attack(attacker, defender);
                        }
                    }
                }

                LogToTurn("Battle Phase: No safe attacks found. Transitioning to Main Phase 2.");
                return new BattlePhaseAction(BattlePhaseAction.BattleAction.ToMainPhaseTwo);
            }
            catch (Exception ex)
            {
                Log("Error in OnBattle hook: " + ex.Message);
                try
                {
                    return base.OnBattle(attackers, defenders);
                }
                catch
                {
                    return null;
                }
            }
        }

        public override BattlePhaseAction OnSelectAttackTarget(ClientCard attacker, IList<ClientCard> defenders)
        {
            UpdateLastKnownLP();
            try
            {
                if (Duel == null || Duel.Fields == null || Duel.Fields.Length < 2 || Duel.Fields[0] == null || Duel.Fields[1] == null)
                {
                    return base.OnSelectAttackTarget(attacker, defenders);
                }

                if (defenders == null || defenders.Count == 0)
                {
                    if (attacker != null && attacker.CanDirectAttack)
                    {
                        LogToTurn(string.Format("Battle Phase: {0} attacking directly.", GetCardName(attacker.Id)));
                        return AI.Attack(attacker, null);
                    }
                    return null;
                }

                if (attacker != null && attacker.CanDirectAttack)
                {
                    LogToTurn(string.Format("Battle Phase: {0} attacking directly.", GetCardName(attacker.Id)));
                    return AI.Attack(attacker, null);
                }

                if (attacker == null)
                {
                    return null;
                }

                attacker.RealPower = attacker.Attack;

                ClientCard bestTarget = null;
                double maxScore = -99999999.0;

                List<ClientCard> sortedDefenders = new List<ClientCard>(defenders);
                try
                {
                    sortedDefenders.Sort((a, b) =>
                    {
                        if (a == null && b == null) return 0;
                        if (a == null) return 1;
                        if (b == null) return -1;

                        bool aIsToken = a.Id == 101402154 || a.Id == 900000113 || (a.Name != null && a.Name.Contains("Token"));
                        bool bIsToken = b.Id == 101402154 || b.Id == 900000113 || (b.Name != null && b.Name.Contains("Token"));
                        if (aIsToken && !bIsToken) return -1;
                        if (!aIsToken && bIsToken) return 1;

                        int aPower = a.GetDefensePower();
                        int bPower = b.GetDefensePower();
                        return aPower.CompareTo(bPower);
                    });
                }
                catch (Exception ex)
                {
                    Log("Error sorting defenders: " + ex.Message);
                }

                foreach (var defender in sortedDefenders)
                {
                    if (defender == null) continue;

                    defender.RealPower = defender.GetDefensePower();

                    if (!OnPreBattleBetween(attacker, defender))
                        continue;

                    if (_opponentMemory.ContainsKey(defender.Id))
                    {
                        double danger = _opponentMemory[defender.Id].learned_danger;
                        if (danger > 80.0 && attacker.RealPower <= defender.RealPower + 1000)
                        {
                            LogToTurn(string.Format("Battle Phase: Avoiding highly dangerous defender {0} (Danger: {1:F1})", GetCardName(defender.Id), danger));
                            continue;
                        }
                    }

                    if (attacker.RealPower == defender.RealPower && IsProtectedBySleepingScapegoats(defender))
                    {
                        LogToTurn(string.Format("Battle Phase: Avoiding tie attack on protected defender {0}", GetCardName(defender.Id)));
                        continue;
                    }

                    int diff = attacker.RealPower - defender.RealPower;
                    double defDanger = CalculateCardDanger(defender);
                    double score = defDanger * 10000.0 + diff;

                    if (attacker.RealPower > defender.RealPower)
                    {
                        if (score > maxScore)
                        {
                            bestTarget = defender;
                            maxScore = score;
                        }
                    }
                    else if (attacker.RealPower == defender.RealPower && attacker.IsLastAttacker && defender.IsAttack())
                    {
                        if (score > maxScore)
                        {
                            bestTarget = defender;
                            maxScore = score;
                        }
                    }
                }

                if (bestTarget != null)
                {
                    LogToTurn(string.Format("Battle Phase: {0} (ATK={1}) attacking {2} (DEF={3})", 
                        GetCardName(attacker.Id), attacker.Attack, GetCardName(bestTarget.Id), bestTarget.RealPower));
                    return AI.Attack(attacker, bestTarget);
                }

                return null;
            }
            catch (Exception ex)
            {
                Log("Error in OnSelectAttackTarget hook: " + ex.Message);
                try
                {
                    return base.OnSelectAttackTarget(attacker, defenders);
                }
                catch
                {
                    return null;
                }
            }
        }

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            try
            {
                if (cards == null || cards.Count == 0)
                {
                    return base.OnSelectCard(cards, min, max, hint, cancelable);
                }

                List<ClientCard> available = new List<ClientCard>(cards);
                bool preferHighPriority = true;

                if (available.Count > 0)
                {
                    CardLocation loc = available[0].Location;
                    if (loc == CardLocation.Hand || loc == CardLocation.MonsterZone || loc == CardLocation.SpellZone)
                    {
                        preferHighPriority = false;
                    }
                }

                bool isKwtunePreferHigh = (_resolvedDeckName == "2026_Kwtune" && preferHighPriority);

                available.Sort((x, y) =>
                {
                    CardMetadata metaX = x != null ? GetOrCreateMetadata(x) : null;
                    int priX = metaX != null ? metaX.priority : 5;

                    CardMetadata metaY = y != null ? GetOrCreateMetadata(y) : null;
                    int priY = metaY != null ? metaY.priority : 5;

                    if (isKwtunePreferHigh)
                    {
                        if (x != null && x.HasSetcode(0x1ce)) priX += 5;
                        if (y != null && y.HasSetcode(0x1ce)) priY += 5;
                    }

                    if (preferHighPriority)
                        return priY.CompareTo(priX);
                    else
                        return priX.CompareTo(priY);
                });

                List<ClientCard> result = new List<ClientCard>();
                int targetCount = min;
                if (min == 0 && max > 0 && cancelable)
                {
                    if (!preferHighPriority || (hint >= 501 && hint <= 506))
                    {
                        targetCount = 1;
                    }
                }

                for (int i = 0; i < Math.Min(targetCount, available.Count); i++)
                {
                    result.Add(available[i]);
                }

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
            catch (Exception ex)
            {
                Log("Error in OnSelectCard hook: " + ex.Message);
                try
                {
                    return base.OnSelectCard(cards, min, max, hint, cancelable);
                }
                catch
                {
                    return new List<ClientCard>();
                }
            }
        }

        protected string GetNextPlan(string current)
        {
            if (current == "PlanA") return "PlanB";
            if (current == "PlanB") return "PlanC";
            return "PlanA";
        }

        public override void OnChaining(int player, ClientCard card)
        {
            UpdateLastKnownLP();
            try
            {
                if (Duel == null || Duel.Fields == null)
                {
                    return;
                }

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
            }
            catch (Exception ex)
            {
                Log("Error in OnChaining hook: " + ex.Message);
            }
            finally
            {
                try
                {
                    base.OnChaining(player, card);
                }
                catch (Exception ex)
                {
                    Log("Error calling base.OnChaining: " + ex.Message);
                }
            }
        }

        public override void OnChainEnd()
        {
            UpdateLastKnownLP();
            try
            {
                if (Duel == null || Duel.Fields == null)
                {
                    return;
                }

                LogToTurn("--- Chain resolution finished ---");
                if (Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
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
                try
                {
                    base.OnChainEnd();
                }
                catch (Exception ex)
                {
                    Log("Error calling base.OnChainEnd: " + ex.Message);
                }
            }
        }

        public override void OnDraw(int player)
        {
            UpdateLastKnownLP();
            try
            {
                if (Duel == null)
                {
                    return;
                }
                string drawer = player == 0 ? "Bot" : "Opponent";
                LogToTurn(string.Format("Draw Event: {0} drew a card.", drawer));
            }
            catch (Exception ex)
            {
                Log("Error in OnDraw hook: " + ex.Message);
            }
            finally
            {
                try
                {
                    base.OnDraw(player);
                }
                catch (Exception ex)
                {
                    Log("Error calling base.OnDraw: " + ex.Message);
                }
            }
        }

        private static void StaticOnProcessExit(object sender, EventArgs e)
        {
            List<BaseCustomExecutor> targets = new List<BaseCustomExecutor>();
            lock (_staticLock)
            {
                foreach (var wr in _activeInstances)
                {
                    BaseCustomExecutor target;
                    if (wr.TryGetTarget(out target))
                    {
                        targets.Add(target);
                    }
                }
            }

            foreach (var instance in targets)
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

        private string SerializeMonsterZone(ClientCard[] zone)
        {
            if (zone == null) return "[]";
            List<string> items = new List<string>();
            for (int i = 0; i < zone.Length; i++)
            {
                var card = zone[i];
                if (card != null)
                {
                    items.Add(string.Format("{{\"id\":{0},\"atk\":{1},\"def\":{2},\"pos\":\"{3}\",\"faceup\":{4}}}",
                        card.Id, card.Attack, card.Defense, (CardPosition)card.Position, card.IsFaceup() ? "true" : "false"));
                }
            }
            return "[" + string.Join(",", items.ToArray()) + "]";
        }

        private string SerializeMonsterZoneWithDanger(ClientCard[] zone)
        {
            if (zone == null) return "[]";
            List<string> items = new List<string>();
            for (int i = 0; i < zone.Length; i++)
            {
                var card = zone[i];
                if (card != null)
                {
                    double danger = CalculateCardDanger(card);
                    items.Add(string.Format("{{\"id\":{0},\"atk\":{1},\"def\":{2},\"pos\":\"{3}\",\"faceup\":{4},\"danger\":{5}}}",
                        card.Id, card.Attack, card.Defense, (CardPosition)card.Position, card.IsFaceup() ? "true" : "false", danger.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)));
                }
            }
            return "[" + string.Join(",", items.ToArray()) + "]";
        }

        private string SerializeSpellZone(ClientCard[] zone)
        {
            if (zone == null) return "[]";
            List<string> items = new List<string>();
            for (int i = 0; i < zone.Length; i++)
            {
                var card = zone[i];
                if (card != null)
                {
                    items.Add(string.Format("{{\"id\":{0},\"pos\":\"{1}\",\"faceup\":{2}}}",
                        card.Id, (CardPosition)card.Position, card.IsFaceup() ? "true" : "false"));
                }
            }
            return "[" + string.Join(",", items.ToArray()) + "]";
        }

        private string SerializeHand(IList<ClientCard> hand)
        {
            if (hand == null) return "[]";
            List<string> items = new List<string>();
            foreach (var card in hand)
            {
                if (card != null)
                {
                    items.Add(string.Format("{{\"id\":{0}}}", card.Id));
                }
            }
            return "[" + string.Join(",", items.ToArray()) + "]";
        }

        protected bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _stopLPMonitor = true;
                ApplyRealTimeLearning();

                lock (_staticLock)
                {
                    _activeInstances.RemoveAll(wr =>
                    {
                        BaseCustomExecutor target;
                        return !wr.TryGetTarget(out target) || target == this;
                    });
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

        ~BaseCustomExecutor()
        {
            Dispose(false);
        }
    }
}
