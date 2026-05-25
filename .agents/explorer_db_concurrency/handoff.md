# Handoff Report: Explorer DB Concurrency Analysis

## 1. Observation

### Path 1: `c:\Users\admin\Documents\EDOTh\Developer\scratch\save_outcomes_to_sql.py`
* **Partitioning Logic** (lines 126–138):
  ```python
              current_game_decs = []
              last_turn = 0
              for dec in decisions_list:
                  turn = dec.get("turn", 0)
                  if turn < last_turn:
                      if current_game_decs:
                          games_decisions.append(current_game_decs)
                      current_game_decs = [dec]
                  else:
                      current_game_decs.append(dec)
                  last_turn = turn
              if current_game_decs:
                  games_decisions.append(current_game_decs)
  ```
* **Database Connection / SQLite write initialization** (lines 7–12):
  ```python
  def init_db(db_path):
      conn = sqlite3.connect(db_path, timeout=30.0)
      cursor = conn.cursor()
      # Enable foreign keys and WAL mode for concurrency
      cursor.execute("PRAGMA foreign_keys = ON;")
      cursor.execute("PRAGMA journal_mode = WAL;")
  ```
  And database updates are committed at line 293:
  ```python
      conn.commit()
  ```

### Path 2: `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`
* **New Turn Reset Condition** (lines 2784–2787):
  ```csharp
                  if (_needsReset || Duel.Turn < _turnCount || (Duel.Turn == 1 && _turnCount > 1))
                  {
                      ResetDuelState();
                  }
  ```

---

## 2. Logic Chain

### Task 1: Game Restarts on Turn 1
1. Under the current `save_outcomes_to_sql.py` parser, decisions from `decisions.jsonl` are grouped into separate matches by checking if `turn < last_turn` (e.g. transition from turn 4 to turn 1).
2. If Game 1 ends on Turn 1, `last_turn` remains `1`.
3. When Game 2 starts, its first decision has `"turn": 1`.
4. The comparison `turn < last_turn` (1 < 1) evaluates to `False`. Thus, the parser appends the new game's decisions to the previous game's list. They are merged into a single game, leading to corrupt statistics and missing records in the database.
5. In addition, the C# executor (`BaseCustomExecutor.cs`) triggers a folder logging reset using the condition `(Duel.Turn == 1 && _turnCount > 1)`.
6. If Game 1 ends on Turn 1, `_turnCount` is `1`. At the start of Game 2's Turn 1, `Duel.Turn` is `1` and `_turnCount` is `1`. The check `_turnCount > 1` (1 > 1) evaluates to `False`.
7. If no cards were played in Game 1 (e.g., immediate surrender/FTK), `_needsReset` remains `False`, meaning `ResetDuelState()` is not called. Consequently, Game 2 continues logging to the same directory and file path as Game 1, exacerbating the python partitioning bug.

### Task 2: SQLite Write Operations & Lock Prevention
1. `save_outcomes_to_sql.py` opens sqlite3 connections and writes data using default deferred transaction mode.
2. In concurrent training environments (running multiple ports simultaneously), multiple bots complete training iterations at similar times, invoking `save_outcomes_to_sql.py` concurrently.
3. Multiple processes holding read locks that attempt to write can cause promotion deadlocks, causing SQLite to throw `sqlite3.OperationalError: database is locked`.
4. To solve this concurrency bottleneck:
   - Transactions must explicitly begin with `BEGIN IMMEDIATE` to acquire write locks at the start, preventing promotion deadlocks and forcing other writers to queue up safely.
   - Synchronous mode should be set to `NORMAL` in WAL mode. This significantly reduces the write lock hold time, keeping concurrency high.
   - Database writes must be structured to happen in memory first, minimizing the lock hold time to a single short write transaction block.
   - Write operations must be wrapped in a transaction retry loop with exponential backoff and randomized jitter to handle transient write collisions.

---

## 3. Caveats
* **Hand Set Overlap**: The Turn 1 state-based restart logic matches hand overlap. In mirror matches, if the opening hand has exactly the same card IDs, there is an extremely small probability ($\approx 1/658008$ for a 40-card deck) that a restart on Turn 1 might not be detected if both players also start with 8000 LP and an empty board. This is an acceptable minor caveat given the 100% failure rate previously.
* **Write performance**: Although retry loops prevent lock crashes, executing too many concurrent writers will increase write latencies.

---

## 4. Conclusion
* **Game Restarts Fix**: Propose modifying `BaseCustomExecutor.cs` to trigger `ResetDuelState()` if `_turnCount >= 1` instead of `_turnCount > 1`. Propose a state-based restart detection helper `is_game_restart(dec, prev_dec)` in `save_outcomes_to_sql.py` to check for LP resets, board clears, and hand shuffles.
* **SQLite Locking Fix**: Propose a transaction wrapper `execute_write_transaction` in Python that uses `BEGIN IMMEDIATE`, WAL mode, `PRAGMA synchronous = NORMAL`, and an exponential backoff retry loop.

---

## 5. Verification Method

### Step 1: Code Review
* Verify the changes in `.agents/explorer_db_concurrency/proposed_save_outcomes_to_sql.py`.
* Verify the diff patch in `.agents/explorer_db_concurrency/proposed_BaseCustomExecutor.patch`.

### Step 2: Automated Pipeline Verification
* Run the project's automated verification script:
  `python c:\Users\admin\Documents\EDOTh\Developer\Scripts\verify_pipeline.py`
  This script creates mock logs and imports them using `save_outcomes_to_sql.py`, then performs reinforcement training. Running this script ensures that our proposed database writing logic works seamlessly and integrates into the existing test suite without syntax or operational errors.
