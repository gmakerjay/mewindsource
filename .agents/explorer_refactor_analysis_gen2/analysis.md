# System Analysis Report (Refactor Prep)

## Executive Summary
This report presents an in-depth analysis and concrete design solutions for requirements R1 through R5. The proposed refactoring preserves the safety rules of the bot while enhancing performance monitoring, data pipeline integrity, and preventing edge-case crashes such as SQLite concurrency locks and Fusion Material selection failures.

---

## R1 & R2: OnCardAction Overload and Executor Callback Wrapping

### Analysis
Currently, card actions are registered via:
`AddExecutor(ExecutorType type, int cardId, Func<bool> func)`
When the engine selects an action, the callback `func` is executed. The target is to wrap this execution inside a newly defined overload of `OnCardAction` to perform decision logging and safeguards:
```csharp
public bool OnCardAction(int cardId, ExecutorType type, Func<bool> condition = null)
```

### Proposed Design
1. **Overload Definition in `BaseCustomExecutor.cs`**:
   The new `OnCardAction` overload evaluates the card's action logic. If the `condition` delegate is provided, it is executed. If it evaluates to `true`, the bot logs the decision, tracks that this card was played in the current duel session (`_ourCardsPlayed.Add(cardId)`), and returns `true`.
   
   ```csharp
   public bool OnCardAction(int cardId, ExecutorType type, Func<bool> condition)
   {
       if (condition == null) return false;
       
       // Evaluate condition
       bool condResult = condition();
       if (!condResult) return false;
       
       // Get card metadata for scoring/safeguards
       ClientCard card = null;
       // Locate the card in hand, monster zone, spell zone, or GY
       foreach (var c in Bot.Hand) { if (c != null && c.Id == cardId) { card = c; break; } }
       if (card == null) { foreach (var c in Bot.GetMonsters()) { if (c != null && c.Id == cardId) { card = c; break; } } }
       if (card == null) { foreach (var c in Bot.GetSpells()) { if (c != null && c.Id == cardId) { card = c; break; } } }
       if (card == null) { foreach (var c in Bot.Graveyard) { if (c != null && c.Id == cardId) { card = c; break; } } }
       
       if (card != null)
       {
           CardMetadata meta = GetOrCreateMetadata(card);
           // Evaluate global safeguards or heuristics
           if (!EvaluateCardAction(card, meta, type))
           {
               return false;
           }
       }
       
       // Record execution
       lock (_staticLock)
       {
           if (!_ourCardsPlayed.Contains(cardId))
           {
               _ourCardsPlayed.Add(cardId);
           }
       }
       
       return true;
   }
   ```

2. **Wrapping Executor Registrations in `DreadnoughtExecutor.cs` and `InvokeExecutor.cs`**:
   Update all `AddExecutor` calls. Wrap existing methods in lambda delegates calling `OnCardAction`:
   * **Before**:
     `AddExecutor(ExecutorType.Activate, 14558127, AshBlossomEffect);`
   * **After**:
     `AddExecutor(ExecutorType.Activate, 14558127, () => OnCardAction(14558127, ExecutorType.Activate, AshBlossomEffect));`

---

## R3: Turn Transition Detection & SQLite Concurrency in `save_outcomes_to_sql.py`

### 1. Game Restart Detection Heuristics
Currently, `save_outcomes_to_sql.py` partitions decisions based solely on `turn < last_turn`. This fails when a game ends or restarts on Turn 1 (e.g. `1 < 1` is false), resulting in multiple games being merged into one.

We propose two methods:
* **Option A (Metadata Marker)**:
  In C#, when `ResetDuelState()` or `SetupFolderLogging()` is called, append a unique marker line to `decisions.jsonl`:
  `{"event": "duel_reset", "match_id": "8-char-id"}`
  In the python script, partition the list whenever `dec.get("event") == "duel_reset"`.
  
* **Option B (Robust Python Heuristics)**:
  If a metadata marker is not used, compare consecutive decisions `dec` and `prev_dec` to detect restart when:
  - `turn < last_turn`
  - OR `turn in (1, 2)` and Life Points reset to 8000 (`lp_self == 8000` and `lp_opp == 8000`) while the previous state had cards on the field or different LP.
  - OR the hand size of the bot suddenly increased back to the starting hand size (5 or 6) from a smaller hand size (e.g. <= 4).

### 2. SQLite Concurrency (WAL & Retry Wrapper)
In parallel dueling environments, multiple processes attempt to write to `statistics.db` concurrently, leading to locked database errors.
We propose wrapping database connection and commits in a transaction-level retry function with exponential backoff and randomized jitter:

```python
import time
import random
import sqlite3

def run_db_transaction(db_path, action, max_retries=10, initial_delay=0.1):
    delay = initial_delay
    for attempt in range(max_retries):
        try:
            conn = sqlite3.connect(db_path, timeout=60.0)
            conn.execute("PRAGMA foreign_keys = ON;")
            conn.execute("PRAGMA journal_mode = WAL;")  # Concurrency enhancement
            cursor = conn.cursor()
            
            # Execute the custom database operation callback
            result = action(conn, cursor)
            
            conn.commit()
            conn.close()
            return result
        except sqlite3.OperationalError as e:
            if "locked" in str(e).lower() and attempt < max_retries - 1:
                # Exponential backoff with jitter
                sleep_time = delay * (2.0 ** attempt) + random.uniform(0, 0.05)
                time.sleep(sleep_time)
            else:
                raise
```

---

## R4: Automated Sync & Headless Compilation on LP = 0

### 1. Detecting LP = 0
The monitor thread `MonitorLP` in `BaseCustomExecutor.cs` runs in the background and queries game state:
```csharp
if (_turnCount > 0 && (botLP == 0 || oppLP == 0))
{
    ApplyRealTimeLearning();
}
```

### 2. Auto-Sync and Headless Compilation
In `ApplyRealTimeLearning()`, immediately after `SaveConfiguration()` writes the updated configuration to `WindBot/config/`, call a new method to copy the updated files to the sandbox folder and execute the batch compilation:

```csharp
protected void SyncRegistryToSandboxAndCompile()
{
    try
    {
        string baseDir = !string.IsNullOrEmpty(_resolvedBaseDir) ? _resolvedBaseDir : AppDomain.CurrentDomain.BaseDirectory;
        string liveConfigDir = Path.Combine(baseDir, "config");
        string sandboxDir = Path.Combine(baseDir, "..", "Developer", "WindBot_Sandbox");
        
        string deckRegistryName = "cards_registry_" + _resolvedDeckName + ".json";
        string liveRegistryPath = Path.Combine(liveConfigDir, deckRegistryName);
        string sandboxRegistryPath = Path.Combine(sandboxDir, deckRegistryName);
        
        // 1. Sync cards registry
        if (File.Exists(liveRegistryPath) && Directory.Exists(sandboxDir))
        {
            File.Copy(liveRegistryPath, sandboxRegistryPath, true);
        }
        
        // 2. Sync opponent memory
        string liveOppMemory = Path.Combine(liveConfigDir, "opponent_memory.json");
        string sandboxOppMemory = Path.Combine(sandboxDir, "opponent_memory.json");
        if (File.Exists(liveOppMemory) && Directory.Exists(sandboxDir))
        {
            File.Copy(liveOppMemory, sandboxOppMemory, true);
        }
        
        // 3. Headlessly run compile_ai.bat
        string compileBatPath = Path.Combine(baseDir, "compile_ai.bat");
        if (File.Exists(compileBatPath))
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = compileBatPath;
            psi.WorkingDirectory = baseDir;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            
            System.Diagnostics.Process.Start(psi);
        }
    }
    catch (Exception ex)
    {
        Log("Sync/Compile error: " + ex.Message);
    }
}
```

---

## R5: Fusion Material Selection Crash Prevention

### 1. Tracking Selection State
To perform material validation during the separate card selection calls, we must store the ID of the selected Fusion Monster.
1. Define a private/protected field in `BaseCustomExecutor.cs`:
   `protected int _lastSelectedFusionId = 0;`
2. In `OnSelectCard` hooks:
   Whenever `hint == HintMsg_SpSummon` (509) and the bot selects a fusion monster from the Extra Deck (which is standard during Fusion spells), capture its ID:
   `_lastSelectedFusionId = result[0].Id;`

### 2. Card Prioritization Score Heuristic
We score available cards to favor recycling materials while keeping vital resources:
* **Priority Boost (+40 to +50)**: Destiny HERO - Malicious (9411399), Destiny HERO - Denier (16605586), Destiny HERO - Dreadnought Servant (101402023).
* **Invoker Boost (+25 to +30)**: Aleister the Invoker (86120751), Aleister the Reminiscent (101305015).
* **Location Graveyard Bonus (+100)**: Strongly favor using GY cards over hand or field cards.
* **Staple Penalty (-100)**: Avoid using Handtraps / Staples (Ash Blossom, Infinite Impermanence, Called by the Grave, etc.) from Hand.
* **Virakam Penalty (-50)**: Avoid using Virakam the Artificial Spirit (101305017) to keep its negate active.

### 3. Strict Material Validation Recipes
When `hint == HintMsg_FusionMaterial` (511), evaluate combinations using these strict recipe rules:

| Fusion Target | ID | Recipe Requirements |
|---|---|---|
| **DPE** | `60461804` | 1 Level 6 or higher HERO monster + 1 Destiny HERO monster |
| **Dreadnought** | `101402037` | 2 Level 5 or higher Destiny HERO monsters |
| **Dystopia** | `90579153` | 2 Destiny HERO monsters |
| **Dangerous** | `30757127` | 1 Destiny HERO monster + 1 DARK Effect Monster |
| **Trinity** | `46759931` | 3 HERO monsters |
| **Contrast HERO Chaos** | `23204029` | 2 Masked HERO monsters |
| **Invoked Mechaba** | `75286621` | Aleister monster + 1 LIGHT monster |
| **Invoked Elysium** | `12307878` | 1 Invoked monster + 1 Extra Deck Summoned monster |
| **Invoked Caliga** | `97973962` | Aleister monster + 1 DARK monster |
| **Invoked Raidjin** | `49513164` | Aleister monster + 1 WIND monster |
| **Invoked Purgatrio** | `13529466` | Aleister monster + 1 FIRE monster |
| **Invoked Magellanica** | `23656668` | Aleister monster + 1 EARTH monster |
| **Invoked Augoeides** | `38423248` | Aleister monster + 1 Fusion monster |
| **Invoked Sorath** | `101305030` | Aleister monster + 1 FIRE or WIND monster |
| **Invoked Babalon** | `101305031` | Aleister monster + 1 LIGHT or EARTH monster |
| **Invoked Okeanos** | `101305032` | Aleister monster + 1 DARK or WATER monster |
| **Invoked Transcendence Aeon** | `101305033` | 2+ Fusion Monsters with different Attributes |

### 4. Implementation Algorithm (`GetOptimalFusionMaterials`)
```csharp
public IList<ClientCard> GetOptimalFusionMaterials(IList<ClientCard> cards, int targetFusionId, int min, int max)
{
    // 1. Iterate combinations (pairs for size 2, triplets for size 3, etc.)
    // 2. Filter combinations satisfying IsValidRecipe(combination, targetFusionId)
    // 3. For each valid combination, calculate total score = Sum(GetMaterialScore(card))
    // 4. Select the combination with the highest total score
    // 5. Fallback: If no valid combination satisfies the recipe, sort cards by score and take top min/max.
}
```
This algorithm ensures the AI always picks legal and optimized materials, eliminating engine crashes during Fusion Summons.
