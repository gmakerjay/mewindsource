# Forensic Audit Report — Milestone 2

**Work Product**: C# AI Engine, Python Sandbox, Deck Configs, and Registry files
**Profile**: General Project
**Integrity Mode**: Demo
**Verdict**: CLEAN

---

## Executive Summary
This forensic audit verified the integrity and correctness of the work products delivered for Milestone 2.
- **Forensic Integrity**: CLEAN. No cheating, hardcoded test results, facade implementations, or fabricated outputs were detected. All dynamic systems operate on actual data/configuration files.
- **Correctness/Completeness Findings**:
  - The C# code in `BaseCustomExecutor.cs` is syntactically correct and includes robust exception handling in the `OnSelectHand` hook.
  - The Python parser `load_ydk_main_deck` in `shared_utils.py` reads all card IDs in all sections (Main, Extra, Side) of the YDK files because it skips section headers (`#extra`, `!side`, etc.) without breaking.
  - All 10 deck configurations are valid JSON and define playstyles correctly aligned with going-first or going-second strategies.
  - **Issue Detected in Registries**: Out of the 10 deck registries under `WindBot/config/`, 3 registries contain card entries with empty roles (`"roles": []`):
    - `cards_registry_2026_Invoke.json` (contains `{"id":0,"roles":[]}`)
    - `cards_registry_2026_Hecahand.json` (contains `{"id":0,"roles":[]}`)
    - `cards_registry_2026_Kwtune.json` (contains multiple cards with empty roles, IDs: `10045474`, `24224830`, `25311006`, `99243014`, `84211599`, `97474300`)

---

## Phase Results

### Check 1: BaseCustomExecutor.cs (OnSelectHand Hook)
- **Status**: PASS
- **Analysis**:
  - The hook overrides `public override bool OnSelectHand()`.
  - It calls `UpdateLastKnownLP()`, which is verified to be a valid protected helper method.
  - It checks `_deckConfig` dynamically.
  - If `playstyle` is `"combo"` or `"midrange"`, it logs the playstyle and returns `true` (go first).
  - If `playstyle` is `"control"` or `"go_second"`, it logs the playstyle and returns `false` (go second).
  - It wraps execution in a robust `try-catch` block that falls back to `base.OnSelectHand()` (or returns `false` in a secondary nested catch block), preventing runtime exceptions from crashing the executor.
  - **Integrity**: Verified clean. There is no deck-name hardcoding (e.g. checking specifically for `"AzaYummy"`) or test-cheating code in `BaseCustomExecutor.cs`.

### Check 2: shared_utils.py (load_ydk_main_deck)
- **Status**: PASS (with warning on naming convention)
- **Analysis**:
  - The function parses cards from `.ydk` files under `Decks/`.
  - It ignores empty lines, `#created`, `#` headers, and `!` headers.
  - It appends all integer card IDs found.
  - Because it does not break when encountering headers like `#extra` or `!side`, it effectively parses **all** sections (Main, Extra, Side) of the YDK file.
  - **Correction Warning**: The docstring states it only parses "main-deck" card IDs, which is slightly misleading given the actual full-file parsing behavior, but the implementation correctly fulfills the project requirements by retrieving all card IDs across all sections.

### Check 3: Deck JSON Configuration Files
- **Status**: PASS
- **Analysis**: All 10 deck configuration files under `WindBot/config/decks/` were inspected and verified.
  - **AzaYummy**: `"combo"` (Goes first - Correct)
  - **BrElfnote**: `"midrange"` (Goes first - Correct)
  - **DarkTime**: `"midrange"` (Goes first - Correct)
  - **EvilTwin**: `"combo"` (Goes first - Correct)
  - **EyeInside**: `"combo"` (Goes first - Correct)
  - **Goldlord**: `"control"` (Goes second - Correct)
  - **Hecahand**: `"go_second"` (Goes second - Correct)
  - **Invoke**: `"midrange"` (Goes first - Correct)
  - **Kwtune**: `"combo"` (Goes first - Correct)
  - **Labrynth**: `"control"` (Goes second - Correct)
  - All files are valid JSON.

### Check 4: Registry JSON Verification
- **Status**: FAIL (Correctness & Completeness Issue)
- **Analysis**:
  - `cards_registry_2026_Goldlord.json` and `cards_registry_2026_Labrynth.json` are fully populated and clean.
  - `cards_registry_2026_Invoke.json` has a card with `id: 0` and empty roles.
  - `cards_registry_2026_Hecahand.json` has a card with `id: 0` and empty roles.
  - `cards_registry_2026_Kwtune.json` contains multiple cards with empty roles (IDs: `10045474`, `24224830`, `25311006`, `99243014`, `84211599`, `97474300`).
  - **Significance**: While not a forensic integrity violation, these empty roles and empty `id: 0` card entries fail the correctness and completeness requirements.

### Check 5: C# Compilation Static Verification
- **Status**: PASS
- **Analysis**:
  - The C# compiler batch script `compile_ai.bat` compiles `BaseCustomExecutor.cs`, `UnifiedIgnisExecutor.cs`, `PureYummyExecutor.cs`, and `InvokeExecutor.cs`.
  - Static verification of `BaseCustomExecutor.cs` confirms all referenced properties (e.g. `_deckConfig.playstyle`, `goals`, `choke_points`, `weaknesses`, `UpdateLastKnownLP`, and `LogToTurn`) match their respective definitions and classes (`DeckIdentity`).
  - Syntactical correctness is verified.

---

## Evidence

### Empty Roles Search Output (Grep Result)
```json
// cards_registry_2026_Invoke.json
{"id":0,"roles":[],"priority":5,"risk_if_negated":3,"bait_value":0,"followup_value":5,"recovery_value":5,"combo_plans":["PlanA"],"q_values":{}}

// cards_registry_2026_Hecahand.json
{"id":0,"roles":[],"priority":5,"risk_if_negated":3,"bait_value":0,"followup_value":5,"recovery_value":5,"combo_plans":["PlanA"],"q_values":{}}

// cards_registry_2026_Kwtune.json
    "id": 10045474,
    "roles": [],
    ...
    "id": 24224830,
    "roles": [],
    ...
    "id": 25311006,
    "roles": [],
    ...
    "id": 99243014,
    "roles": [],
    ...
    "id": 84211599,
    "roles": [],
    ...
    "id": 97474300,
    "roles": [],
```
