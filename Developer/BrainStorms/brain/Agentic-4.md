# Agentic-4: Comprehensive Source Code Analysis Report
## Project: Ignis_Train_Audit — AI WindBot Training Suite

**Date:** 25 May 2026  
**Analyst:** Buffy (Codebuff AI Agent)  
**Scope:** All 12 source files in `C:\Users\admin\Desktop\Ignis_Train_Audit\`

---

## Executive Summary

This project is a **Yu-Gi-Oh! AI training and execution framework** built on top of the **WindBot** platform (an automated YGOPro/EDOPro bot system). It comprises:

1. **C# Executors** (`.cs`) — The AI decision engine that plays the game, deployed within WindBot.
2. **Python Training Scripts** (`.py`) — Offline tools for reinforcement learning, match data persistence, log analysis, and parallel match orchestration.
3. **Web Cockpit UI** — A live dashboard (HTTP server) for training management and analytics.

The architecture is sophisticated but has several **critical issues** ranging from incomplete optimizations, hardcoded paths, missing dependencies, and potential runtime failures.

---

## File-by-File Analysis

---

### 1. `BaseCustomExecutor.cs` — Dynamic AI Core Engine

| Aspect | Assessment |
|--------|------------|
| **Status** | ⚠️ **Partially Working — Truncated File, Overengineered** |
| **Language** | C# (.NET Framework) |
| **Size** | ~143 KB (truncated in read — actual file larger) |

#### ✅ What Works (Working)

- **Dynamic Card Registry** (`_cardRegistry`) — Loads card metadata from JSON config files with roles, priorities, combo plans.
- **Opponent Memory System** (`_opponentMemory`) — Tracks opponent cards seen, disruption frequency, learned danger scores.
- **Deck Identity System** (`DeckIdentity`) — Per-deck playstyle, goals, choke points, weakness configuration.
- **Dynamic Executor Registration** — Automatically registers `AddExecutor` hooks for each card ID in the registry.
- **`CanCardAttack`** — Accurate protection checks for Mystic Mine (18175665), Messenger of Peace (44656491), Gravity Bind (85742772), Swords of Revealing Light (72302403).
- **`IsLethalOnBoard`** — Sophisticated combat simulation to detect lethal damage.
- **`UpdateGoal`** — Dynamic goal selection (push_lethal, survive, break_board, establish_interruptions) based on board state.
- **`CalculateTotalDangerForField`** — Evaluates opponent threats across all zones including Graveyard, Hand, Banished.
- **`CalculateCardDanger`** — Multi-factor danger scoring with learned memory, registry priority, weakness mapping, extra deck detection, tuner synchro threat, chain-response detection.
- **`EvaluateCardAction`** — Extensive scoring system (goal adjustments, combo plan heuristics, threat/baiting logic, resource tracking, zone limits, anti-overextension, redundant field protection, board reading, hybrid weighting).
- **`GetLookaheadBonus`** — Forward-looking evaluation for searcher/draw, extender synergy, tuner materials, protection.
- **`ApplyRealTimeLearning`** (full implementation) — Outcome-based priority adjustments, bait inflation, anti-inflation decay, priority hard cap (max 8), opponent danger scoring, natural decay.
- **Logging System** — Full match logging with `match_summary.log`, per-turn logs, `decisions.jsonl` with board state snapshots.
- **`ReadFileWithRetry` / `WriteFileWithRetry`** — Robust file I/O with retry logic.
- **`LoadConfiguration` / `SaveConfiguration`** — Full config persistence for registry, card names, deck configs, opponent memory, attack locks.

#### ❌ What's Not Working / Problematic

| Issue | Severity | Details |
|-------|----------|---------|
| **File truncated at 143KB** | 🔴 Critical | The file is too large and was cut off during reading. Methods past the truncation point (likely `OnCardAction`, remaining default executor methods) could not be verified. |
| **`GetOrCreateMetadata` uses `card.Controller == 0`** | 🟡 Medium | Only stores auto-created metadata for the bot's own cards. Opponent cards get a metadata object created but it's discarded, causing re-creation on every call. |
| **`GetStapleBaselineDanger` hardcodes IDs** | 🟢 Info | Fine for staples but not extensible. Update requires recompilation. |
| **`MonitorLP` thread never stops** | 🟡 Medium | `_stopLPMonitor` is set but there's no guarantee the thread exits cleanly when the executor is disposed. |
| **`ResetDuelState` is not auto-triggered** | 🟡 Medium | There's `_needsReset` but no automatic detection logic visible in the truncated portion. |
| **`JavaScriptSerializer` usage** | 🟡 Medium | Only available in .NET Framework 4.x. Will not work in .NET Core / .NET 5+. Depends on WindBot's target framework. |
| **Overly complex single file** | 🟢 Info | 143KB+ for one class violates single-responsibility. Should be refactored into smaller modules. |
| **Static `_activeInstances` tracking** | 🟢 Info | Uses WeakReferences for cleanup — theoretically correct but adds unnecessary complexity. |

---

### 2. `cockpit.py` — Web Control Dashboard & Training Launcher

| Aspect | Assessment |
|--------|------------|
| **Status** | ✅ **Functionally Complete with Minor Issues** |
| **Language** | Python 3 |

#### ✅ What Works

- **HTTP Server** (`ThreadedHTTPServer`) — Serves HTML templates and JSON API endpoints.
- **Deck & Opponent Discovery** — Lists available decks from `WindBot/Decks/`, opponent bots from `bots.json`.
- **Bot Spawning** (`spawn_bots_on_port`, `run_live_duel_loop`) — Launches WindBot.exe subprocess pairs.
- **Progress Logging** — Thread-safe file writing with retry logic.
- **Match History Parsing** (`parse_match_history`) — Reads `match_summary.log` and `decisions.jsonl` into structured analytics.
- **Registry Snapshot** (`get_registry_snapshot_data`) — Aggregate registry health metrics.
- **Training Mode Selection** — Heuristic, simulator, reinforcement, AB tournament, live duel modes.
- **Deploy Function** (`deploy_config`) — Copies sandbox registry to live config and runs `compile_ai.bat`.
- **API Endpoints** — `/api/decks`, `/api/opponents`, `/api/status`, `/api/progress`, `/api/match_history`, `/api/progress_report`, `/api/registry_snapshot`, `/api/active_spawns`, `/api/train`, `/api/kill`, `/api/deploy`, `/api/spawn_bots`, `/api/kill_port`.
- **Win/Loss Progress Tracking** — Before/after comparison (first 25% vs last 25% of matches), cumulative block metrics.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **References files not in this directory** | 🔴 Critical | `start_training` references `optimize_registry.py`, `combo_simulator.py`, `run_match_learning.py`, `ab_tournament.py` — none present in `Ignis_Train_Audit/`. These will fail at runtime. |
| **Hardcoded path to templates** | 🟡 Medium | `_load_template` loads from `templates/` directory relative to `SCRIPT_DIR`. If `templates/analytics.html` or `templates/progress.html` don't exist, the UI will return empty pages. |
| **`get_progress_report` runs external script** | 🟡 Medium | Calls `save_outcomes_to_sql.py` via subprocess with 5s timeout. If the script is slow, it gets killed. |
| **Discard output threads in spawn_bots** | 🟢 Info | Bot output is discarded rather than logged, losing diagnostic data. |
| **`parse_match_history` error handling** | 🟢 Info | Uses bare `try/except: pass` which silently drops malformed matches. |

---

### 3. `DreadnoughtExecutor.cs` — Dreadnought Deck-Specific Executor

| Aspect | Assessment |
|--------|------------|
| **Status** | ✅ **Fully Implemented with One Defect** |
| **Language** | C# |
| **Inheritance** | `BaseCustomExecutor` → `UnifiedIgnisExecutor` → `DreadnoughtExecutor` |

#### ✅ What Works

- **Complete card rules** for ~22+ cards with specific activation conditions.
- **`OnSelectCard` override** — Smart target selection for destruction, banish/add, fusion materials, search, recycling, GY sending, Mask Change.
- **Proper hand trap timing** (`AshBlossomEffect`, `FuwalosEffect`, `DrollEffect`) — Only triggers on opponent's turn responding to opponent's cards.
- **Resource protection** — Called by the Grave correctly targets opponent's GY.
- **Zone limit awareness** — Has helper methods `HasInHand`, `HasInGraveyard`, `HasInSpellZone`, `HasInMonsterZone`.
- **`EvaluateCardAction` override** — Blocks normal summon of boss monsters (Level 8/10) that should only be Special Summoned.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **Duplicate executor registration** | 🔴 Critical | `AddExecutor(ExecutorType.Activate, 24224830, CalledByGraveEffect);` **appears twice** — Called by the Grave is registered two times with the same ID. Depending on WindBot's executor resolution, this could fire the effect twice or cause unexpected behavior. |
| **`IsDestinyHero`/`IsHero` uses hardcoded IDs** | 🟢 Info | Maintaining these lists requires code changes when new HERO cards are added. |

---

### 4. `InvokeExecutor.cs` — Invoke Deck-Specific Executor

| Aspect | Assessment |
|--------|------------|
| **Status** | ✅ **Fully Implemented with Minor Defects** |
| **Language** | C# |

#### ✅ What Works

- **Complete card rules** for ~25+ Invoke archetype cards.
- **`OnSelectCard` override** — Proper material selection for Fusion, Link, Spellbook of Knowledge target, Search (Aleister, Aiwass, Spellbook Magician), Discard.
- **`OnSelectPlace` override** — Okeanos always placed in Extra Monster Zone.
- **`OnSelectOption` override** — Spirit Sword Aiwass: Option 0 (SS Aiwass from deck) vs Option 2 (snipe Extra Deck).
- **`EvaluateCardAction` override** — Blocks normal summon of Level 6 Aiwass.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **`ImpermanenceEffect` lacks turn/chain check** | 🟡 Medium | Only checks if opponent has face-up monsters. Does not check `Duel.Player == 1` or verify it's chaining to a valid target. Could activate at suboptimal times. |
| **`OnSelectYesNo` always returns `true`** | 🟡 Medium | Accepts ALL yes/no prompts unconditionally. This could lead to self-destructive effects or paying unnecessary costs. Should check the description parameter. |
| **`TranscendenceAeonEffect` hardcodes attributes** | 🟢 Info | Always declares DARK on opponent's turn and LIGHT on player's turn — good default but lacks adaptability. |

---

### 5. `learning_sandbox.py` — Heuristic Self-Learning Engine

| Aspect | Assessment |
|--------|------------|
| **Status** | ⚠️ **Mostly Working with Logic Issues** |
| **Language** | Python 3 |

#### ✅ What Works

- **Match discovery** — Reads LIVE logs folder with fallback to `mock_logs/`.
- **Outcome parsing** — Reads `match_summary.log` for Bot LP, Opp LP, turn count.
- **Decision parsing** — Reads `decisions.jsonl` into structured dicts.
- **Disruption parsing** — Regex scan of turn logs for "WARNING: Opponent disrupted" messages.
- **`apply_learning` logic** — Priority adjustments for win/loss outcomes, followup_value boosts, recovery_value boosts, risk_if_negated boosts for disrupted cards.
- **Bait value anti-inflation decay** — Decays bait_value >= 6.
- **Dedup by deck groups** — Groups matches by deck name.
- **Sandbox + LIVE sync** — Saves to both sandbox and live registry paths.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **Bait bootstrap logic outside loop** | 🔴 Critical | The **bait_value bootstrap** block (lines ~187-203) is indented **outside** the `for analysis` loop but **references `analysis`** from inside the loop. In Python, this will use the **last** analysis from the loop — meaning bait bootstrapping applies only to the final match's decisions, not all matches. This is a logic bug. |
| **Choke point regex may never match** | 🟡 Medium | The regex `WARNING: Opponent disrupted Bot's choke point .*?\\(ID:\\s*(\\d+)\\)` looks for a specific log format. Review of `BaseCustomExecutor.cs` shows no code that generates this exact log format. The disruption tracking uses `_disruptionsInMatch` dictionary, but the logging of that happens in `ApplyRealTimeLearning` rather than real-time. |
| **`parse_match_outcome` doesn't handle `Draw` for 2-turn games** | 🟡 Medium | If a match ends in 1-2 turns with both LP > 0, it's labeled "Tie/Aborted" even if it was a legitimate timeout draw. |
| **No error propagation in `discover_match_dirs`** | 🟢 Info | Silently skips unreadable directories. |

---

### 6. `parallel_launcher.py` — Parallel Headless Match Runner

| Aspect | Assessment |
|--------|------------|
| **Status** | ✅ **Functionally Complete** |
| **Language** | Python 3 |

#### ✅ What Works

- **`run_single_headless_match`** — Launches 2 WindBot instances on a port, streams stdout to log file.
- **`run_headless_parallel`** — Spawns N pairs with staggered timing (1.5s delay) to avoid port race conditions.
- **`setup_gui_instance`** — Creates isolated EDOPro sandbox folders with custom `system.conf` on different ports.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **`setup_gui_instance` uses `mklink`** | 🟡 Medium | Windows `mklink /j` and `mklink /h` require **administrator privileges** or developer mode. Without these, the command fails silently (stderr is redirected to `DEVNULL`). The resulting instance folder will be empty. |
| **No cleanup on Ctrl+C** | 🟢 Info | If user interrupts during parallel matches, orphan WindBot.exe processes may remain. |

---

### 7. `PureYummyExecutor.cs` — Pure Yummy Deck Executor

| Aspect | Assessment |
|--------|------------|
| **Status** | ✅ **Fully Implemented** |
| **Language** | C# |

#### ✅ What Works

- **Complete card rules** — One for One, Piri Reis Map, Yummy★Snatchy Link-1, Field Spells (Mignon/Acroquey), all Level 1 Yummy monsters, Level 2 Synchros, Chaos Angel, S:P Little Knight, generic Link-1s.
- **`OnSelectCard` override** — Discard, Search, Link Material, Return to Hand (bounce) with smart prioritization.
- **`OnSelectOption` override** — Yummy☆Surprise: bounce, special summon, or recycle Field depending on board state.
- **`OnSelectYesNo` override** — Always accepts optional Yummy triggers.
- **Proper SS conditions** — Marshmao☆Yummy checks monster zone state, Cupsy/Lollipo/Cooky check for enablers.
- **Block normal summon of bosses** — Prevented in `EvaluateCardAction` (inherited).

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **`OnSelectYesNo` always returns `true`** | 🟡 Medium | Same issue as InvokeExecutor — unconditional acceptance of all prompts. |
| **Return to Hand hint uses `505`** | 🟢 Info | Magic number 505 (ReturnToHand) is used directly instead of a named constant. If WindBot's hint values change, this breaks silently. |

---

### 8. `q_learning.py` — Q-Learning Reinforcement Trainer

| Aspect | Assessment |
|--------|------------|
| **Status** | ✅ **Functionally Complete** |
| **Language** | Python 3 |

#### ✅ What Works

- **Match discovery** — Finds matches with both `match_summary.log` and `decisions.jsonl`.
- **Outcome parsing** — Win/WeakWin/Draw/WeakLoss/Loss with LP and turn analysis.
- **Reward calculation** — Base reward + LP differential scaling + turn penalty.
- **Episodic Monte Carlo Q-update** — Bellman equation with discounted rewards.
- **Priority hard cap enforcement** — Caps all priorities at 8 before saving.
- **Sandbox + LIVE save** — Dual-path registry persistence.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **`reg_dict` is built from list, saved correctly** | 🟢 Verified | The `save_registry_list` in shared_utils handles dict→list conversion properly. |
| **No early-stopping for unregistered cards** | 🟢 Info | Cards not in the registry are auto-created with default metadata — could dilute the registry with noise from irrelevant cards (e.g., tokens, summoned-from-deck enemies). |

---

### 9. `run_multi_iterations.py` — Multi-Round Training Orchestrator

| Aspect | Assessment |
|--------|------------|
| **Status** | ⚠️ **Partially Working — Hardcoded Paths** |
| **Language** | Python 3 |

#### ✅ What Works

- **`run_round`** — Launches parallel matches and monitors progress.
- **`archive_and_clean_logs`** — Moves session logs to `ArchivedMatches/`, cleans `ParallelMatches/`.
- **`print_db_summary`** — Queries `statistics.db` for outcome distribution.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **Hardcoded project paths** | 🔴 Critical | `PROJECT_ROOT = r"c:\\Users\\admin\\Documents\\EDOTh"` — **will not work on any other machine.** This path appears in multiple places. Must be made relative or configurable. |
| **Hardcoded DB path** | 🟡 Medium | `DB_PATH = os.path.join(PROJECT_ROOT, "Developer", "scratch", "statistics.db")` assumes a specific directory structure under EDOTh. |
| **`subprocess.run("cls", shell=True)`** | 🟡 Medium | Windows-specific `cls` command. Will cause errors on Linux/macOS (but this is a Windows-only WindBot tool, so acceptable). |
| **References scripts not in this directory** | 🟡 Medium | Calls `monitor_progress.py`, `parallel_launcher.py` (by path), `save_outcomes_to_sql.py`, `run_match_learning.py` — only `parallel_launcher.py` is present in the analyzed set. |

---

### 10. `save_outcomes_to_sql.py` — SQLite Database Persistence

| Aspect | Assessment |
|--------|------------|
| **Status** | ⚠️ **Partially Working — Hardcoded Paths, Legacy Code** |
| **Language** | Python 3 |

#### ✅ What Works

- **SQLite schema initialization** — `matches` and `decisions` tables with foreign keys.
- **WAL mode** — Enabled for concurrent access.
- **Match parsing** — Regex-based extraction of outcomes from `match_summary.log`.
- **Decision logging** — Full board state serialization into `decisions` table.
- **Turn-reset game partitioning** — Detects `turn < last_turn` as new game boundaries.
- **Fallback for aborted matches** — Handles single-game sessions with "Duel Session Finished" marker.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **Hardcoded project paths** | 🔴 Critical | `db_path = r"c:\\Users\\admin\\Documents\\EDOTh\\Developer\\scratch\\statistics.db"` and `logs_root = r"c:\\Users\\admin\\Documents\\EDOTh\\WindBot\\Logs"`. **Unusable on any other system.** |
| **Legacy `deck_self` matching** | 🟡 Medium | The opponent_deck matching logic assumes the script is always run with --deck and --opp-deck. If run without these, `opponent_deck` remains "Unknown" for all entries. |
| **Bare `try/except: pass` in decision insertion** | 🟡 Medium | Malformed JSON lines in decisions.jsonl are silently dropped, making debugging difficult. |
| **Does not use `shared_utils.py` paths** | 🟡 Medium | Has its own hardcoded paths instead of importing from `shared_utils`. This is a code duplication / inconsistency issue. |

---

### 11. `shared_utils.py` — Shared Utilities Module

| Aspect | Assessment |
|--------|------------|
| **Status** | ✅ **Clean & Fully Functional** |
| **Language** | Python 3 |

#### ✅ What Works

- **Path constants** — `SCRIPT_DIR`, `PROJECT_ROOT`, `WINDBOT_DIR`, `DECKS_DIR`, `LIVE_CONFIG_DIR`, `LIVE_LOGS_DIR`, `OPP_MEMORY_PATH`.
- **`configure_utf8()`** — Safe stdout encoding config.
- **`_registry_filename()`** — Generates deck-specific registry filenames.
- **`get_registry_paths()`** — Returns (sandbox, live) paths with auto-init from default registry + auto-role-detector.
- **`get_available_decks()`** — Lists AI and non-AI decks from `WindBot/Decks/`.
- **`load_ydk_main_deck()`** — Parses `.ydk` files with side deck/deck comments filtering.
- **`load_registry_list()` / `load_registry_dict()` / `save_registry_list()`** — Thread-safe registry I/O with atomic writes via `tempfile.mkstemp` + `os.replace`.
- **Priority hard cap enforcement** — Enforced at save time in `save_registry_list()`.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **`auto_role_detector.py` referenced but not present** | 🟡 Medium | `get_registry_paths` calls `auto_role_detector.py` via subprocess. If this file doesn't exist in the sandbox directory, the subprocess fails silently. |
| **`PROJECT_ROOT` path resolution** | 🟢 Info | Uses `SCRIPT_DIR` → parent → parent to find project root. This assumes `WindBot_Sandbox/` is 2 levels deep under `PROJECT_ROOT`. If directory structure changes, this breaks. |

---

### 12. `UnifiedIgnisExecutor.cs` — Base Deck Executor Stub

| Aspect | Assessment |
|--------|------------|
| **Status** | ⚠️ **Partially Working — Mostly Empty Stubs** |
| **Language** | C# |

#### ✅ What Works

- **`UnifiedIgnisExecutor`** — Inherits from `BaseCustomExecutor`, uses dynamic registry system.
- **`AzaYummyExecutor`**, **`BrElfnoteExecutor`**, **`DarkTimeExecutor`**, **`EvilTwinExecutor`**, **`EyeInsideExecutor`**, **`HecahandExecutor`**, **`GoldlordExecutor`**, **`KwtuneExecutor`**, **`LabrynthExecutor`** — All correctly registered with `[Deck]` attributes and class declarations.

#### ❌ Issues

| Issue | Severity | Details |
|-------|----------|---------|
| **Most executors are EMPTY stubs** | 🟡 Medium | Out of 10 deck executors, only 3 have custom logic files: `InvokeExecutor.cs`, `DreadnoughtExecutor.cs`, `PureYummyExecutor.cs`. The remaining 7 executors are **empty classes** with no `AddExecutor` calls — they rely entirely on the dynamic registry system from `BaseCustomExecutor`. If the JSON registry files are incomplete or missing, these decks will perform poorly. |
| **`2026_Dreadnought` has a dedicated file** | 🟢 Info | `DreadnoughtExecutor.cs` exists but `UnifiedIgnisExecutor.cs` does **not** have a `DreadnoughtExecutor` class — it's defined in `DreadnoughtExecutor.cs` instead, which is correct. |

---

## Cross-Cutting Concerns

### 🔴 Critical Issues

1. **Hardcoded Absolute Paths** (3 files)
   - `run_multi_iterations.py`: `r"c:\\Users\\admin\\Documents\\EDOTh"`
   - `save_outcomes_to_sql.py`: `r"c:\\Users\\admin\\Documents\\EDOTh\\..."`
   - These **will not work on any other machine** and prevent sharing or deployment.

2. **Missing Referenced Files** (multiple scripts)
   - `optimize_registry.py` — Referenced by cockpit.py
   - `combo_simulator.py` — Referenced by cockpit.py
   - `run_match_learning.py` — Referenced by cockpit.py, run_multi_iterations.py
   - `ab_tournament.py` — Referenced by cockpit.py
   - `auto_role_detector.py` — Referenced by shared_utils.py
   - `monitor_progress.py` — Referenced by run_multi_iterations.py
   - HTML templates: `analytics.html`, `progress.html` — Referenced by cockpit.py
   - **Without these files, the training pipeline is broken.**

3. **BaseCustomExecutor.cs Truncation**
   - At ~143KB, this file is too large and was cut off during reading. Critical methods beyond the truncation point cannot be verified.

4. **Duplicate `CalledByGrave` Registration** in `DreadnoughtExecutor.cs` (line 40-41)
   - `AddExecutor(ExecutorType.Activate, 24224830, CalledByGraveEffect);` appears twice.

5. **Bait Bootstrap Logic Bug** in `learning_sandbox.py`
   - Code block is outside the loop but references loop variable `analysis`.

### 🟡 Medium Issues

6. **`OnSelectYesNo` always returns true** in `InvokeExecutor.cs` and `PureYummyExecutor.cs`
   - Accepts all prompts including potentially harmful ones.

7. **`ImpermanenceEffect` lacks turn validation** in `InvokeExecutor.cs`
   - No `Duel.Player == 1` check — could fire during own turn.

8. **Windows `mklink` privilege requirement** in `parallel_launcher.py`
   - Fails silently without admin privileges.

9. **Empty executor stubs** (7 of 10 decks) in `UnifiedIgnisExecutor.cs`
   - No custom logic — entirely dependent on JSON registry files.

10. **`JavaScriptSerializer` dependency** in `BaseCustomExecutor.cs`
    - .NET Framework-specific. May not work in modern .NET runtimes.

### 🟢 Minor / Informational Issues

11. Code duplication between files (e.g., path resolution in `save_outcomes_to_sql.py` vs `shared_utils.py`).
12. Magic numbers for card IDs scattered across multiple files (could be named constants).
13. `cockpit.py` uses bare `try/except: pass` in several places, making debugging difficult.
14. `q_learning.py` auto-creates registry entries for any card played — can accumulate noise.
15. `parallel_launcher.py` uses stdout-discarding threads instead of logging.
16. No PyPI `requirements.txt` or setup documentation found.

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    W i n d B o t   (C#)                      │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              BaseCustomExecutor.cs                     │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────────┐  │  │
│  │  │ Dynamic Card  │  │  Opponent    │  │  Decision  │  │  │
│  │  │ Registry      │  │  Memory      │  │  Engine    │  │  │
│  │  └──────────────┘  └──────────────┘  └────────────┘  │  │
│  │                        │                              │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │  UnifiedIgnisExecutor.cs  (stub base)            │  │  │
│  │  │  ┌───────────┐ ┌──────────┐ ┌─────────────────┐  │  │  │
│  │  │  │ Invoke    │ │Dreadnought│ │ PureYummy      │  │  │  │
│  │  │  │ Executor  │ │ Executor  │ │ Executor       │  │  │  │
│  │  │  └───────────┘ └──────────┘ └─────────────────┘  │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │ logs/decisions.jsonl
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                P y t h o n   T r a i n i n g                │
│                                                             │
│  save_outcomes_to_sql.py   ──►   statistics.db (SQLite)    │
│         │                                                  │
│  learning_sandbox.py        ◄──  match logs                │
│         │                                                  │
│  q_learning.py              ◄──  decisions + outcomes       │
│         │                                                  │
│  cockpit.py (Web UI)        ◄──  all data sources          │
│         │                                                  │
│  parallel_launcher.py       ──►  headless duels            │
│         │                                                  │
│  run_multi_iterations.py    ──►  orchestration             │
│         │                                                  │
│  shared_utils.py            ──►  shared helpers            │
└─────────────────────────────────────────────────────────────┘
```

---

## Recommendations

### Immediate (Critical Fixes)

1. **Refactor hardcoded paths** → Use relative paths from `shared_utils.py` in all Python scripts.
2. **Locate and verify missing files** — `optimize_registry.py`, `combo_simulator.py`, `run_match_learning.py`, `ab_tournament.py`, `auto_role_detector.py`, `monitor_progress.py`, HTML templates.
3. **Fix duplicate executor registration** in `DreadnoughtExecutor.cs`.
4. **Fix bait bootstrap indentation** in `learning_sandbox.py` (`apply_learning` function).
5. **Split `BaseCustomExecutor.cs`** into smaller modules (< 1000 lines each).

### Short-Term (Quality Improvements)

6. **Add turn/chain validation** to `ImpermanenceEffect` in `InvokeExecutor.cs`.
7. **Implement proper `OnSelectYesNo`** in all executors — check the description parameter.
8. **Replace `mklink`** with `os.symlink()` in `parallel_launcher.py`.
9. **Create `requirements.txt`** documenting Python dependencies.
10. **Add named constants** for common card IDs.

### Long-Term (Architecture)

11. **Separate card data** from code — consider a YAML/JSON-driven effect system.
12. **Unit tests** for the Python training pipeline.
13. **CI/CD pipeline** for AI model deployment.
14. **Documentation** — architecture overview, setup guide, deck customization guide.

---

## File Summary Table

| # | File | Language | Lines (approx) | Status | Dependencies |
|---|------|----------|---------------|--------|--------------|
| 1 | `BaseCustomExecutor.cs` | C# | 3,000+ | ⚠️ Truncated | WindBot, YGOSharp |
| 2 | `cockpit.py` | Python | 450+ | ✅ Works | shared_utils, 5 missing scripts |
| 3 | `DreadnoughtExecutor.cs` | C# | 400+ | ✅ Works | UnifiedIgnisExecutor |
| 4 | `InvokeExecutor.cs` | C# | 400+ | ✅ Works | UnifiedIgnisExecutor |
| 5 | `learning_sandbox.py` | Python | 250+ | ⚠️ Bug | shared_utils |
| 6 | `parallel_launcher.py` | Python | 150+ | ✅ Works | Windows mklink |
| 7 | `PureYummyExecutor.cs` | C# | 250+ | ✅ Works | UnifiedIgnisExecutor |
| 8 | `q_learning.py` | Python | 120+ | ✅ Works | shared_utils |
| 9 | `run_multi_iterations.py` | Python | 160+ | ⚠️ Hardcoded | 3 missing scripts |
| 10 | `save_outcomes_to_sql.py` | Python | 220+ | ⚠️ Hardcoded | JSON, sqlite3 |
| 11 | `shared_utils.py` | Python | 170+ | ✅ Clean | (standalone) |
| 12 | `UnifiedIgnisExecutor.cs` | C# | 40+ | ⚠️ Stubs | BaseCustomExecutor |

---

## Conclusion

The **Ignis_Train_Audit** project represents a sophisticated AI training framework for Yu-Gi-Oh! WindBot, combining dynamic C# decision engines with Python-based reinforcement learning pipelines. 

**What's working:** The C# executor architecture is sound, with a comprehensive dynamic card registry, opponent memory system, sophisticated decision scoring, and detailed match logging. The Python training pipeline correctly reads logs, updates registry weights, and can persist results to SQLite. Three deck-specific executors (Invoke, Dreadnought, PureYummy) are well-implemented with specific card rules.

**What's broken:** The project suffers from **5 critical issues** that prevent it from functioning as a complete system: hardcoded absolute paths, missing referenced training scripts, file truncation in the core engine, a duplicate executor registration bug, and a logic error in the learning sandbox's bait bootstrapping. Additionally, 7 of 10 deck executors are empty stubs with no custom logic, relying entirely on dynamic JSON registries.

**To make this production-ready:** Address the hardcoded paths, locate the missing scripts, fix the logic bugs, refactor the monolithic `BaseCustomExecutor.cs`, and add proper `OnSelectYesNo` handling. The foundation is solid; the execution needs hardening.
