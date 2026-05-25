# ProjectIgnis Self-Learning Bot System: Audit & Repair Plan

This master plan document compiles, analyzes, and synthesizes the findings from the audit reports (`Agentic-1.txt` through `agentic-5.txt`, `MASTER_PROMPT_WindBotAI.md`) and performs a codebase audit of C# files in `Developer/WindBot_Sources` and Python files in `Developer/WindBot_Sandbox`. It establishes clear development guidelines and a step-by-step repair pathway to resolve architecture, correctness, and concurrency bottlenecks.

---

## 1. Document Analysis & Commonalities Identification (Milestone 1)

A cross-analysis of the brainstorm logs and the master guidelines reveals strong alignment on structural defects, though there are key conflicts and distinct recommendations.

### 1.1 Consensus Findings (Agreement)
All analyzed documents agree on the following system vulnerabilities:
*   **Multi-Instance Write Concurrency Race:** In a parallel self-play setup (running up to 20 headless client instances concurrently), the C# process-local `lock (_staticLock)` fails to prevent file write collisions across processes on `cards_registry_*.json` and `opponent_memory.json`. This causes files to truncate or corrupt.
*   **Hardcoded Absolute Path Barriers:** The codebase contains non-portable hardcoded absolute paths pointing to `c:\Users\admin\Documents\EDOTh\`, preventing execution on other systems.
*   **Executor Monolithic Bloat:** `BaseCustomExecutor.cs` contains over 3,400 lines of code. It acts as a monolithic engine violating the Single Responsibility Principle, making maintenance difficult.

### 1.2 Discrepancies and Conflicting Recommendations
*   **Monte Carlo Q-learning Return Direction:**
    *   *Conflict:* `Agentic-2.txt` and `agentic-5.txt` accepted the mathematical discounting return formula $G_t = \text{reward} \times \gamma^{T-1-t}$ as correct.
    *   *Resolution:* Only `Agentic-3.md` correctly identified that this formulation is inverted. It discounts early moves (opening combos) heavily and gives the final attack move (trivial action) a discount factor of $1.0$. This prevents the system from solving the credit assignment problem for Turn 1/2 opening boards.
*   **C# Callback Logging Wrapper:**
    *   *Conflict:* Previous discussions proposed overcomplicating subclass executors by overloading `OnCardAction` to support conditional delegates.
    *   *Resolution:* `Agentic-1.txt` and `MASTER_PROMPT_WindBotAI.md` established a cleaner wrapper method `ExecuteWithLog(cardId, type, condition)` in `BaseCustomExecutor` that encapsulates the callback execution, evaluates priorities, logs the decision, and returns the result without overloading delegate signatures.

### 1.3 Architectural Vision: Centralized Actor-Learner Pattern
The reports outline a major architectural shift from client-side file mutations to a distributed training framework:
1.  **Actors (Headless WindBot Clients):** Run in read-only mode. They load immutable snapshots of the card registry (`registry_vN.json`) at start, evaluate priorities, and stream game decisions/state logs directly to an external store (SQLite database in WAL mode or a message queue).
2.  **Learner (Central Training Script):** A single Python process reads decision records from the database, filters out poor-quality games (bricks, OTKs, surrenders) via a Match Quality Filter, computes Q-value updates, and writes a new registry version (`registry_v(N+1).json`).

---

## 2. Codebase Consistency Audit Matrix (Milestone 2)

This matrix maps documented issues directly to verified locations in the active codebase:

| Issue | Target File | Line Numbers | Code Element / Method | Verified Behavior | Corrective Action / Fix Proposal |
|---|---|---|---|---|---|
| **MonitorLP Race Condition** | `BaseCustomExecutor.cs` | 191–219 | `MonitorLP()` method | Polling loop reads `BotLP`, `OpponentLP`, and `CurrentPlayer` asynchronously on background thread `MonitorThread` without thread-safety locks. Calls file-writing updates (`ApplyHeuristicAdjustments` / `ApplyQLearningUpdates`) concurrently with the main thread. | Wrap access to variables and execution of adjustments in thread-safe locks using `lock (_staticLock)` or a dedicated mutex. Ensure G7 (500ms sleep) is not modified. |
| **Inverted Q-learning Discount** | `q_learning.py` | 174–176 | Discounting block in Monte Carlo return calculation | Calculates return using $G_t = \text{reward} \times \gamma^{T-1-t}$ (`steps_from_end = T - 1 - t`), heavily discounting early actions. | Invert discount direction to value early decisions more relative to the start: `steps_from_start = t`, $G_t = \text{reward} \times \gamma^t$. |
| **Hardcoded Paths** | `find_field_locks.py`<br>`save_outcomes_to_sql.py`<br>`run_multi_iterations.py` | 6<br>155, 169<br>11 | `PROJECT_ROOT`, `db_path`, `logs_root` variables | Hardcoded strings pointing to `c:\Users\admin\Documents\EDOTh` break portability on other developer machines. | Replace with dynamic folder resolution based on `os.path.dirname(os.path.abspath(__file__))` relative to the project root. Centralize paths to `shared_utils.py` (P6). |
| **Direct Callback Bypass (Placebo)** | `DreadnoughtExecutor.cs`<br>`InvokeExecutor.cs` | Subclass actions | Custom card handlers (e.g. `DoomLiegeEffect`, `MechabaEffect`) | Card effects registered in subclass executors return raw `bool` directly, bypassing base `EvaluateCardAction()` and skipping decision logging. | Wrap all custom card callbacks in the base class's `ExecuteWithLog` helper. |
| **Stubs vs Implementation** | `UnifiedIgnisExecutor.cs` | 21–76 | 9 empty class stubs | `AzaYummyExecutor`, `BrElfnoteExecutor`, etc. are empty stubs, playing solely via flat default heuristics. | Implement rule-based decision sequences in C# or extend JSON registries to support simple conditional priority schemas. |
| **Duplicate Registration** | `DreadnoughtExecutor.cs` | 26–27 / 40-41 | Card registry mapping | Card ID `24224830` (Called by the Grave) is registered twice. | Remove the duplicate registration line. |
| **Blind Option Selection** | `InvokeExecutor.cs`<br>`PureYummyExecutor.cs` | 737–740<br>433–438 | `OnSelectYesNo` | Returns `true` blindly for all prompt options without checking card state. | Add conditional checks verifying if the prompted card effect is beneficial before returning `true`. |
| **Game Turn Boundary Split** | `save_outcomes_to_sql.py` | 79–82 | Game boundary logic | Turn reset splits games using `turn < last_turn`. Merges games incorrectly if they restart on turn 1. | Implement `turn < last_turn and last_turn > 0` or split by a distinct `game_id` field. |

---

## 3. Strict Design Guidelines & Code Patterns (Milestone 3)

All future modifications must adhere to the following rules, which compile the Iron Rules of `Rules.md` and `MASTER_PROMPT_WindBotAI.md` with Karpathy's clean-coding practices in `SKILL.md`.

### 3.1 C# Code Guidelines
*   **G1 (Thread Safety):** All file I/O operations targeting `cards_registry_*.json` or `opponent_memory.json` must be wrapped in `lock (_staticLock)` blocks.
*   **G2 (Atomic Writes):** Never call `File.WriteAllText` directly on live files. Write to a temporary file in the same directory, copy the live file to backup, and replace the live file atomically.
*   **G3 (Hard Cap Priority):** Enforce `if (card.priority > 8) card.priority = 8;` immediately after updates.
*   **G4 (_learningApplied Guard):** Set `_learningApplied = true` before executing `SaveConfiguration()` to prevent double-save operations across thread transitions.
*   **G5 (ExecuteWithLog Wrapper):** All custom subclass executor callbacks must be wrapped to ensure they are logged:
    ```csharp
    // Correct Pattern
    AddExecutor(ExecutorType.Activate, CARD_ID, () => ExecuteWithLog(CARD_ID, ExecutorType.Activate, CustomEffect));
    ```
*   **G6 (Catch-All Fallbacks):** Subclass executors that clear standard handlers (`Executors.Clear()`) must register default fallbacks (e.g. `OnDefaultActivate`, `OnDefaultSummon`) to avoid playing unregistered cards blindly.
*   **G7 (MonitorLP Sleep):** Never change the `Thread.Sleep(500)` loop interval in `MonitorLP()`.
*   **G8 (Folder Logging in Reset):** Ensure `SetupFolderLogging()` is called inside `ResetDuelState()` to preserve new-game boundary markers.

### 3.2 Python Pipeline Guidelines
*   **P1 (Game Partitioning):** Partition duel logs using a unique `game_id` if present, or split via:
    ```python
    if turn < last_turn and last_turn > 0:
        # Correctly split game boundaries
    ```
*   **P2 (Atomic Registry Write):** Write JSON updates via temporary files using `tempfile.NamedTemporaryFile` + `os.replace` to prevent file truncation during active reads:
    ```python
    with tempfile.NamedTemporaryFile("w", dir=dir, delete=False, suffix=".json") as tmp:
        json.dump(data, tmp)
    os.replace(tmp.name, live_path)
    ```
*   **P3 (Non-Overwrite Merge):** Merge parallel instances using combining rules rather than raw overwrite (Last-Write-Wins):
    *   `priority`, `risk_if_negated`, `bait_value`, `followup_value` $\rightarrow \max(a, b)$
    *   `q_values[goal]` $\rightarrow \text{average}(a, b)$
*   **P4 (Safe database wipes):** Never call `parse_and_save(..., wipe=True)` unless triggered explicitly by the `--wipe` CLI flag.
*   **P5 (Registry Hard Cap):** Always enforce `card["priority"] = 8` in `save_registry_list()` before writing.
*   **P6 (Centralized Path Constants):** Import path constants (`SCRIPT_DIR`, `LIVE_CONFIG_DIR`, etc.) from `shared_utils.py`.

---

## 4. Development Pathways & Repair Plan

To systematically resolve the identified bottlenecks, implementation should follow these three sequential phases:

### Phase 1: Correctness & Portability (Immediate Fixes)
1.  **Invert Monte Carlo Discounting:** Modify `q_learning.py` line 174 to discount relative to step $t$ ($G_t = \text{reward} \times \gamma^t$).
2.  **Centralize Paths in Python:** Move all hardcoded absolute path constants to `shared_utils.py` using dynamic path mapping based on `os.path.abspath(__file__)`, and update scripts to import them.
3.  **Fix Game Partitioning:** Implement `turn < last_turn and last_turn > 0` in `save_outcomes_to_sql.py`.

### Phase 2: Concurrency & Stability (Multi-Instance Safety)
1.  **Synchronize MonitorLP:** Wrap shared state access inside `BaseCustomExecutor.cs` `MonitorLP()` using the existing `lock (_staticLock)` lock block.
2.  **Ensure Atomic Writes in C#:** Verify that `WriteFileWithRetry()` is utilized for registry writes, and enforce thread locking on all config updates.
3.  **SQLite WAL Concurrency Retries:** Wrap SQLite database transactions in `save_outcomes_to_sql.py` in retry blocks with random backoffs (exponential retries) to prevent locking errors under parallel writes.

### Phase 3: Functionality & Architectural Evolution
1.  **Refactor Subclass Callbacks:** Modify `DreadnoughtExecutor.cs` and `InvokeExecutor.cs` to wrap special callbacks in the `ExecuteWithLog` structure.
2.  **Address Executor Stubs:** Implement conditional logic parser mapping rules from JSON or write specific subclasses for the remaining 9 decks.
3.  **Centralize Actor-Learner Framework:** Transition the C# engine to read-only mode during training matches, streaming decisions to SQLite database WAL, and execute training exclusively within the Python pipeline.
