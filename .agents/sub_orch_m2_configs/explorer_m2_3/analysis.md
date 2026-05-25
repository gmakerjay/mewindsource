# Deck Analysis and Registry Comparison Report

## Executive Summary
This report presents the findings of our read-only exploration of deck lists, registry files, and deck configurations for the 10 target decks in the Project Ignis WindBot AI engine.
1. **YDK vs Registry Comparison**: Contrary to initial assumptions of card omissions, we verified that **no card IDs specified in the YDK files for the four bricked decks (Goldlord, Invoke, Kwtune, Labrynth) are missing from their corresponding live deck-specific registries**. 
2. **Sandbox Registries & Script Analysis**: Sandbox registries are more complete and tuned, containing reinforcement learning parameters (`q_values`) and optimized weights. The utility script `auto_role_detector.py` has a critical limitation: it parses only main-deck cards and entirely ignores `#extra` and `!side` deck sections due to a bug in `shared_utils.py:load_ydk_main_deck`. Live registries are populated via the cockpit deployment mechanism (which copies sandbox registries to the live config directory and executes `compile_ai.bat` to build the dll).
3. **Deck Config & Playstyle Summaries**: We mapped the playstyles, goals, weaknesses, and choke points for all 10 active decks. 
4. **Configuration Discrepancies**: We discovered critical discrepancies where configured choke points in `2026_Goldlord.json` (Eldorado Adelantado - `95825075`) and `2026_Labrynth.json` (Labrynth Coelophys - `23440079`) are completely missing from their respective YDK deck files, and `2026_Kwtune.json` is missing configured choke points entirely.

---

## 1. YDK Card ID Comparison Against Live Registries
We systematically compared the unique card IDs found in the YDK files (`WindBot/Decks/2026_<deck>.ydk`) for the four bricked decks against their live registries under `WindBot/config/cards_registry_2026_<deck>.json`.

### Method of Loading Configuration in C# Code
In `WindBot/BaseCustomExecutor.cs` (lines 539–544), configuration loading is structured as follows:
- The bot attempts to load the deck-specific registry file: `cards_registry_2026_<deck_name>.json`.
- If the deck-specific registry does not exist, it falls back to the general `cards_registry.json`.
- If the deck-specific registry exists, it is loaded completely. Thus, any card missing from the global registry is not an issue as long as it exists in the deck-specific registry.

### Registry Completeness Verification
For the four bricked decks, **all** unique card IDs inside the YDK files (including Main and Extra decks) are successfully present in their deck-specific registries:

* **2026_Goldlord.ydk**: Contains 24 unique card IDs (19 in main deck, 5 in extra deck). All 24 IDs are present in `cards_registry_2026_Goldlord.json`.
* **2026_Invoke.ydk**: Contains 37 unique card IDs (17 in main deck, 12 in extra deck, 8 unique to side deck). All 37 IDs are present in `cards_registry_2026_Invoke.json`.
* **2026_Kwtune.ydk**: Contains 42 unique card IDs. All 42 IDs are present in `cards_registry_2026_Kwtune.json`.
* **2026_Labrynth.ydk**: Contains 45 unique card IDs (29 in main, 15 in extra, 7 unique to side). All 45 IDs are present in `cards_registry_2026_Labrynth.json`.

---

## 2. Evaluation of Sandbox Registries & Sandbox Scripts

### Sandbox vs. Live Registries
We compared the contents of the files in `WindBot_Sandbox/` against their active counterparts in `WindBot/config/`.
- **Optimization Parameters**: The sandbox registry files contain tuned heuristics (priority, risk, bait, recovery, and followup values) and learned parameters (`q_values` from reinforcement learning match sessions) that are either unoptimized or empty in the live folder.
- **File Structure**: Sandbox JSON files are pretty-printed (using 2-space indentation), resulting in larger file sizes (e.g. 44–49 KB), whereas live config files are typically minified onto a single line (29–32 KB), though they share the same card entries.
- **Kwtune Synchronization**: The Kwtune registry file (`cards_registry_2026_Kwtune.json`) is already identical in both folders (47,178 bytes), indicating it was previously successfully deployed.

### Script Evaluation: `auto_role_detector.py`
The script `auto_role_detector.py` is designed to scan YDK decks, query `expansions/cards.cdb` (SQLite database) for card names/descriptions, detect roles (e.g., `handtrap`, `starter`, `extender`, `payoff`, `disruption`, `recovery`, `floodgate`) using regex heuristics, and write them to the sandbox registry.

#### Critical Bug/Limitation in `shared_utils.py`
A crucial bug is located in the card-loading helper function:
- File: `WindBot_Sandbox/shared_utils.py` (lines 106–141)
- Function: `load_ydk_main_deck(deck_name, unique=False)`
- The function loops through the YDK file but explicitly disables parsing when it encounters `#extra` or `!side` headers:
  ```python
  if line.startswith("#extra") or line.startswith("!side"):
      in_main = False
      continue
  ```
- **Impact**: The parser only returns cards in the `#main` section. As a result, `auto_role_detector.py` completely ignores all cards in the Extra and Side decks. 
- **Consequence**: Running `auto_role_detector.py` to populate registries will leave any Extra Deck boss monsters or Side Deck cards completely unpopulated and un-role-detected, causing them to defaults (e.g., empty roles or generic `combo_piece` default roles), which can degrade bot gameplay.

### Registry Synchronization & Live Deployment
Live registries are updated from the sandbox via the cockpit deployment utility:
- File: `WindBot_Sandbox/cockpit.py`
- Method: `deploy_config(self, deck="")` (called via `/api/deploy` endpoint)
- **Deployment Process**:
  1. The script identifies the target sandbox file (`WindBot_Sandbox/cards_registry_2026_<deck>.json`).
  2. It copies the file to the live config directory (`WindBot/config/cards_registry_2026_<deck>.json`).
  3. It executes the C# AI build script `WindBot/compile_ai.bat`.
  4. `compile_ai.bat` compiles the C# source files using `csc.exe` into a new `UnifiedIgnisExecutor.dll` dynamic link library.

---

## 3. Deck Configuration Profiles (All 10 Decks)
We inspected the JSON files under `WindBot/config/decks/` for all 10 target decks. Below is a comprehensive profile of their configurations:

| Deck Name | Playstyle | Goals | Choke Points (Card IDs) | Weaknesses |
| :--- | :--- | :--- | :--- | :--- |
| **AzaYummy** | combo | `survive`, `establish_interruptions`, `push_lethal` | `61980241`, `72270339`, `31425736` | `handtraps`, `boardwipes` |
| **BrElfnote** | midrange | `survive`, `establish_interruptions`, `push_lethal` | `13597785`, `56651978`, `70088809`, `68468459` | `handtraps`, `lockouts` |
| **DarkTime** | midrange | `survive`, `establish_interruptions`, `push_lethal` | `101402052`, `101402001` | `handtraps`, `backrow` |
| **EvilTwin** | combo | `survive`, `establish_interruptions`, `push_lethal` | `60764609`, `36326160`, `73810864` | `handtraps`, `graveyard_hate` |
| **EyeInside** | combo | `survive`, `establish_interruptions`, `push_lethal` | `28954097`, `92565383`, `95365081` | `handtraps`, `boardwipes` |
| **Goldlord** | control | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `95825075` *(Eldorado Adelantado)* | `graveyard_hate`, `backrow_removal` |
| **Hecahand** | go_second | `survive`, `establish_interruptions`, `push_lethal` | `95365081`, `20415050` | `floodgate`, `handtraps` |
| **Invoke** | midrange | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `86120751` *(Aleister)*, `74063034` *(Invocation)* | `handtraps`, `negation` |
| **Kwtune** | combo | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | *(None defined: `[]`)* | `handtraps`, `boardwipes` |
| **Labrynth** | control | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `23440079` *(Labrynth Coelophys)* | `backrow_removal`, `graveyard_hate` |

---

## 4. Playstyles, Goals, and Weaknesses

### 1. AzaYummy (Combo)
- **Playstyle**: Combo going first. Rapidly summons Level 1 monsters and prepares quick synchros.
- **Goals**: Survive the first turn, establish multiple interruptions, and push for lethal on the next turn.
- **Weaknesses**: Handtraps (Ash Blossom, Infinite Impermanence) and generic board wipes (Raigeki, Evenly Matched).

### 2. BrElfnote (Midrange)
- **Playstyle**: Midrange going first/second. Relies on consistent resource generation and moderate disruption.
- **Goals**: Establish resource loops, maintain card advantage, and gradually wear down the opponent.
- **Weaknesses**: Lockout floodgates and faster combo decks that can overpower their resource game.

### 3. DarkTime (Midrange)
- **Playstyle**: Midrange tempo deck with graveyard utilization.
- **Goals**: Survive early-game pressure, set up graveyard triggers, and establish moderate interruption.
- **Weaknesses**: Direct handtraps on key searchers and backrow cards that freeze monster effects.

### 4. EvilTwin (Combo)
- **Playstyle**: Combo going first. Focuses on Link-summoning using Live☆Twin and Evil★Twin engines.
- **Goals**: Link climb to Lilla and Kisikil, draw cards, pop cards on the opponent's turn, and finish.
- **Weaknesses**: Extremely vulnerable to handtraps targeting the Normal Summon starter, and graveyard banishing effects.

### 5. EyeInside (Combo)
- **Playstyle**: High-ceiling combo going first.
- **Goals**: Build a board with multiple monster negates and interruptions.
- **Weaknesses**: Handtraps and massive board wipes that clear their negates.

### 6. Goldlord (Control)
- **Playstyle**: Trap-heavy control deck utilizing Eldlich the Golden Lord engines.
- **Goals**: Control the board via normal traps and recur Eldlich cards from the GY to break boards.
- **Weaknesses**: Graveyard banishment (Bystials, Called by the Grave) and heavy backrow removal (Harpie's Feather Duster).

### 7. Hecahand (Go Second)
- **Playstyle**: Go-second OTK board breaker.
- **Goals**: Clear opponent's field, survive disruptions, and push high-damage attacks to win on Turn 2.
- **Weaknesses**: Lockouts/floodgates that restrict special summons or attacks, and well-timed handtraps.

### 8. Invoke (Midrange)
- **Playstyle**: Midrange fusion deck using the Aleister the Invoker engine.
- **Goals**: Summon Invoked Mechaba on Turn 1 to negate opponent's key actions, loop Invocation, and grind down the opponent.
- **Weaknesses**: Handtraps targeting Aleister's normal summon search, and negations on key Fusion spells.

### 9. Kwtune (Combo)
- **Playstyle**: Tuner/Non-tuner synchro combo deck.
- **Goals**: Sync-summon boss monsters, establish interruptions, and push lethal.
- **Weaknesses**: Handtraps and boardwipes.

### 10. Labrynth (Control)
- **Playstyle**: Control deck revolving around Normal Trap activations and Labrynth monster triggers.
- **Goals**: Activating traps to trigger Labrynth castle and maid effects, looping traps from the GY, and breaking opponent resources.
- **Weaknesses**: Backrow removal (Cosmic Cyclone, Feather Duster) and graveyard hate.

---

## 5. Critical Configuration Discrepancies
1. **Goldlord Choke Point**: `WindBot/config/decks/2026_Goldlord.json` lists `95825075` (Eldorado Adelantado) as its choke point, but this card is **not present** in `Decks/2026_Goldlord.ydk`.
2. **Labrynth Choke Point**: `WindBot/config/decks/2026_Labrynth.json` lists `23440079` (Labrynth Coelophys) as its choke point, but this card is **not present** in `Decks/2026_Labrynth.ydk`.
3. **Kwtune Choke Points**: `WindBot/config/decks/2026_Kwtune.json` lists no choke points (`[]`), leaving the AI unable to prioritize which cards to negate or protect.
