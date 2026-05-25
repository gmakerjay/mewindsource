# Deck Analysis and Registry Comparison Report

## Executive Summary
This report presents the findings of our read-only exploration of deck lists, registry files, and deck configurations for the 10 target decks in the Project Ignis WindBot AI engine.

1. **YDK vs Registry Comparison**: 
   - We verified that **no card IDs specified in the YDK files for the four primary bricked decks (Goldlord, Invoke, Kwtune, Labrynth) are missing** from their corresponding live deck-specific registries.
   - However, our extended check on the remaining six decks revealed that **`2026_AzaYummy` and `2026_BrElfnote` have several side-deck card IDs completely missing** from both their live and sandbox registries. These missing cards will receive default unoptimized heuristics at runtime.
2. **Sandbox Registries & Script Analysis**: 
   - Sandbox registries contain reinforcement learning parameters (`q_values`) and optimized weights.
   - Live registries are copied from the sandbox registries via the cockpit utility (`cockpit.py`), which copies the registries to `WindBot/config/` and compiles the dynamic link library `UnifiedIgnisExecutor.dll` by running `compile_ai.bat`.
   - The helper script `shared_utils.py:load_ydk_main_deck` does not terminate parsing on `#extra` or `!side`; it loops through the entire file and returns all card IDs (Main, Extra, and Side). Therefore, `auto_role_detector.py` can be re-run on a deck to automatically populate any missing cards.
3. **Deck Config & Playstyle Summaries**: We mapped the playstyles, goals, weaknesses, and choke points for all 10 active decks.
4. **Configuration Discrepancies**: We identified critical discrepancies where configured choke points in `2026_Goldlord.json` (Eldorado Adelantado - `95825075`) and `2026_Labrynth.json` (Labrynth Coelophys - `23440079`) are completely missing from their respective YDK deck files, and `2026_Kwtune.json` is missing configured choke points entirely.

---

## 1. YDK Card ID Comparison Against Live Registries
We systematically compared the unique card IDs found in the YDK files (`WindBot/Decks/AI_2026_<deck>.ydk` or `2026_<deck>.ydk`) against their live registries under `WindBot/config/cards_registry_2026_<deck>.json`.

### Method of Loading Configuration in C# Code
In `WindBot/BaseCustomExecutor.cs` (lines 539–544), configuration loading is structured as follows:
- The bot attempts to load the deck-specific registry file: `cards_registry_2026_<deck_name>.json`.
- If the deck-specific registry does not exist, it falls back to the general `cards_registry.json`.
- If the deck-specific registry exists, it is loaded completely. Thus, any card missing from the global registry is not an issue as long as it exists in the deck-specific registry.

### Registry Completeness Verification (4 Bricked Decks)
For the four bricked decks, **all** unique card IDs inside the YDK files (including Main and Extra decks) are successfully present in their deck-specific registries:
* **2026_Goldlord.ydk**: Contains 24 unique card IDs (19 in main deck, 5 in extra deck). All 24 IDs are present in `cards_registry_2026_Goldlord.json`.
* **2026_Invoke.ydk**: Contains 37 unique card IDs (17 in main deck, 12 in extra deck, 8 unique to side deck). All 37 IDs are present in `cards_registry_2026_Invoke.json`.
* **2026_Kwtune.ydk**: Contains 42 unique card IDs. All 42 IDs are present in `cards_registry_2026_Kwtune.json`.
* **2026_Labrynth.ydk**: Contains 45 unique card IDs (29 in main, 15 in extra, 7 unique to side). All 45 IDs are present in `cards_registry_2026_Labrynth.json`.

### Registry Discrepancy Discovery (Other 6 Decks)
While the 4 bricked decks and 4 of the other decks are fully registered, we discovered missing side-deck card IDs in `2026_AzaYummy` and `2026_BrElfnote`:

#### 1. AzaYummy Registry Gaps
The following card IDs present in `Decks/AI_2026_AzaYummy.ydk` (specifically the `!side` section) are **entirely missing** from `WindBot/config/cards_registry_2026_AzaYummy.json` and `WindBot_Sandbox/cards_registry_2026_AzaYummy.json`:
- **46502744**: Retaliating "C"
- **24508238**: D.D. Crow
- **18144507**: Harpie's Feather Duster
- **14532163**: Lightning Storm
- **41420027**: Solemn Judgment

#### 2. BrElfnote Registry Gaps
The following card IDs present in `Decks/AI_2026_BrElfnote.ydk` (specifically the `!side` section) are **entirely missing** from `WindBot/config/cards_registry_2026_BrElfnote.json` and `WindBot_Sandbox/cards_registry_2026_BrElfnote.json`:
- **59438930**: Ghost Ogre & Snow Rabbit
- **19613556**: Heavy Storm
- **83326048**: Dimensional Barrier

*Impact*: At runtime, when the bot attempts to get metadata for these missing cards, `BaseCustomExecutor.GetOrCreateMetadata` (lines 1490–1510) creates default metadata with baseline values (priority = 5, risk = 3, bait = 0, followup = 5, recovery = 5, and empty roles). This causes suboptimal bot play for these cards (e.g. failing to use handtraps or board wipes with appropriate urgency or priority).

---

## 2. Evaluation of Sandbox Registries & Sandbox Scripts

### Sandbox vs. Live Registries
We compared the contents of the files in `WindBot_Sandbox/` against their active counterparts in `WindBot/config/`.
- **Optimization Parameters**: The sandbox registry files contain tuned heuristics (priority, risk, bait, recovery, and followup values) and learned parameters (`q_values` from reinforcement learning match sessions) that are either unoptimized or empty in the live folder.
- **File Structure**: Sandbox JSON files are pretty-printed (using 2-space indentation), resulting in larger file sizes (e.g. 44–49 KB), whereas live config files are typically minified onto a single line (29–32 KB), though they share the same card entries.
- **Kwtune Synchronization**: The Kwtune registry file (`cards_registry_2026_Kwtune.json`) is already identical in both folders (47,178 bytes), indicating it was previously successfully deployed.

### Script Evaluation: `auto_role_detector.py`
The script `auto_role_detector.py` scans YDK decks, queries `expansions/cards.cdb` (SQLite database) for card names/descriptions, detects roles using regex heuristics, and merges them into the sandbox registry.

#### Clarification on `shared_utils.py` Parsing
Contrary to previous assertions that `load_ydk_main_deck` ignores `#extra` and `!side` headers, an inspection of `WindBot_Sandbox/shared_utils.py` (lines 120-131) reveals:
```python
    main_deck = []
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#created"):
                continue
            if line.startswith("#") or line.startswith("!"):
                continue
            try:
                main_deck.append(int(line))
            except ValueError:
                pass
```
The script skips only comment/header lines starting with `#` or `!` (such as `#main`, `#extra`, and `!side`), but **continues to loop through and parse all card IDs in all sections**. Thus, the entire YDK card list is returned. 

*Recommendation*: Running `python auto_role_detector.py --deck 2026_AzaYummy` and `python auto_role_detector.py --deck 2026_BrElfnote` is a fully viable way to populate the missing card IDs in those registries.

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
| **AzaYummy** | combo | `survive`, `establish_interruptions`, `push_lethal` | `61980241` *(Dyna Mondo)*, `72270339` *(Diviner of the Herald)*, `31425736` *(Nouvelles Restaurant Atable)* | `handtraps`, `boardwipes` |
| **BrElfnote** | midrange | `survive`, `establish_interruptions`, `push_lethal` | `13597785` *(Branded Fusion)*, `56651978` *(Aluber the Jester of Despia)*, `70088809` *(Ash Blossom & Joyous Spring)*, `68468459` *(Lubellion the Searing Dragon)* | `handtraps`, `lockouts` |
| **DarkTime** | midrange | `survive`, `establish_interruptions`, `push_lethal` | `101402052` *(Tearlaments Kitkallos)*, `101402001` *(Tearlaments Havnis)* | `handtraps`, `backrow` |
| **EvilTwin** | combo | `survive`, `establish_interruptions`, `push_lethal` | `60764609` *(Live☆Twin Ki-sikil)*, `36326160` *(Live☆Twin Lil-la)*, `73810864` *(Evil★Twin Ki-sikil)* | `handtraps`, `graveyard_hate` |
| **EyeInside** | combo | `survive`, `establish_interruptions`, `push_lethal` | `28954097` *(Evil Eye of Selene)*, `92565383` *(Serziel, Watcher of the Evil Eye)*, `95365081` *(Basilicock, Glimmer of the Evil Eye)* | `handtraps`, `boardwipes` |
| **Goldlord** | control | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `95825075` *(Eldorado Adelantado)* | `graveyard_hate`, `backrow_removal` |
| **Hecahand** | go_second | `survive`, `establish_interruptions`, `push_lethal` | `95365081` *(Basilicock)*, `20415050` *(Evil Eye Reemergence)* | `floodgate`, `handtraps` |
| **Invoke** | midrange | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `86120751` *(Aleister the Invoker)*, `74063034` *(Invocation)* | `handtraps`, `negation` |
| **Kwtune** | combo | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | *(None defined: `[]`)* | `handtraps`, `boardwipes` |
| **Labrynth** | control | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `23440079` *(Labrynth Coelophys)* | `backrow_removal`, `graveyard_hate` |

---

## 4. Playstyles, Goals, and Weaknesses

### 1. AzaYummy (Combo)
- **Playstyle**: Combo going first. Rapidly summons Level 1 monsters and prepares quick synchros/tribute plays.
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
1. **Goldlord Choke Point**: `WindBot/config/decks/2026_Goldlord.json` lists `95825075` (Eldorado Adelantado) as its choke point, but this card is **not present** in `Decks/2026_Goldlord.ydk` (neither Main nor Extra).
2. **Labrynth Choke Point**: `WindBot/config/decks/2026_Labrynth.json` lists `23440079` (Labrynth Coelophys) as its choke point, but this card is **not present** in `Decks/2026_Labrynth.ydk` (neither Main nor Extra).
3. **Kwtune Choke Points**: `WindBot/config/decks/2026_Kwtune.json` lists no choke points (`[]`), leaving the AI unable to prioritize which cards to negate or protect.
