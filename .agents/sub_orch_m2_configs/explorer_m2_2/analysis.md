# Deck Analysis and Registry Comparison Report

## Executive Summary
This report analyzes the deck lists, card registries, and deck configurations for the 10 active AI decks in the Project Ignis WindBot engine. We focused on investigating four "bricked" decks (Goldlord, Invoke, Kwtune, and Labrynth) to compare their YDK card lists against the live card registries. We also evaluated the differences between live registries and sandbox registries, assessed the `auto_role_detector.py` utility, and summarized the playstyles, goals, and weaknesses for all 10 decks.

---

## 1. YDK Cards vs. Live Registry Comparison
We compared the unique card IDs inside the YDK files for the four bricked decks (`2026_Goldlord`, `2026_Invoke`, `2026_Kwtune`, and `2026_Labrynth`) against their deck-specific registry files (`WindBot/config/cards_registry_2026_<deck>.json`).

### Findings
Contrary to initial assumptions, **no card IDs specified in the YDK files are missing from their corresponding deck-specific live registries.** Every card in the Main, Extra, and Side decks of these four files has a matching JSON entry in `cards_registry_2026_<deck>.json`.

*   **Deck-Specific Registry Priority:** In `BaseCustomExecutor.LoadConfiguration` (lines 539–544), the bot resolves `cards_registry_2026_<deck>.json` first. If it exists, it loads it. It only falls back to the global `cards_registry.json` if the deck-specific file is missing. Therefore, missing cards in the global registry do not affect execution if the deck-specific registry exists and is complete.
*   **Identical YDKs:** The standard user deck (`2026_<deck>.ydk`) and the AI deck (`AI_2026_<deck>.ydk`) are identical in card IDs for all four decks.

### Complete Card ID Coverage Check

#### A. 2026_Goldlord
*   **Total unique card IDs in YDK:** 24
*   **All IDs in Registry:** Yes. All 24 IDs are defined in `cards_registry_2026_Goldlord.json` (size: 29,335 bytes).

#### B. 2026_Invoke
*   **Total unique card IDs in YDK:** 44
*   **All IDs in Registry:** Yes. All 44 IDs are defined in `cards_registry_2026_Invoke.json` (size: 47,425 bytes).

#### C. 2026_Kwtune
*   **Total unique card IDs in YDK:** 42
*   **All IDs in Registry:** Yes. All 42 IDs are defined in `cards_registry_2026_Kwtune.json` (size: 47,175 bytes).

#### D. 2026_Labrynth
*   **Total unique card IDs in YDK:** 45
*   **All IDs in Registry:** Yes. All 45 IDs are defined in `cards_registry_2026_Labrynth.json` (size: 32,667 bytes).

---

## 2. Sandbox Registry Differences & `auto_role_detector.py` Analysis

### Sandbox vs. Live Registries
We compared files under `WindBot_Sandbox/` against their active counterparts in `WindBot/config/`.
*   **Content Differences:** The registries are structural duplicates in terms of card IDs, but the sandbox version retains Reinforcement Learning training data in `q_values` (e.g. `"q_values": {"establish_interruptions": -0.0011}`) which is sometimes empty (`{}`) in live configs.
*   **File Sizes:** Sandbox registries are generally pretty-printed, leading to larger file sizes (e.g., Labrynth registry is 49,131 bytes in sandbox vs. 32,667 bytes minified/differently spaced in live config, but contains the same data).

### Script Analysis: `auto_role_detector.py`
The utility `auto_role_detector.py` automates role detection by querying `expansions/cards.cdb` (SQLite database) for card texts and applying regex heuristics to assign roles (e.g., `handtrap`, `starter`, `extender`, `payoff`, `disruption`, `recovery`, `floodgate`).

#### Critical Script Limitation
A major bug/limitation was found in the card-loading helper function:
*   In `WindBot_Sandbox/shared_utils.py` (lines 106–140), `load_ydk_main_deck` parses card IDs from the YDK file.
*   It explicitly filters and **only** returns cards listed between `#main` and the next section header (`#extra` or `!side`).
*   **Impact:** The script completely ignores all cards in the `#extra` (Extra Deck) and `!side` (Side Deck) sections of the YDK.
*   Consequently, if `auto_role_detector.py` is run to populate a deck's registry, it will **never** detect, register, or update roles for any unique Extra Deck boss monsters or Side Deck options unless they are already present in the Main Deck.

---

## 3. Deck Configuration Inspection (All 10 Decks)
We inspected the JSON files under `WindBot/config/decks/` for all 10 decks. Below is a comprehensive profile of their configurations:

| Deck Name | Playstyle | Goals | Choke Points (Card IDs) | Weaknesses |
| :--- | :--- | :--- | :--- | :--- |
| **AzaYummy** | Combo | `survive`, `establish_interruptions`, `push_lethal` | `61980241`, `72270339`, `31425736` | `handtraps`, `boardwipes` |
| **BrElfnote** | Midrange | `survive`, `establish_interruptions`, `push_lethal` | `13597785`, `56651978`, `70088809`, `68468459` | `handtraps`, `lockouts` |
| **DarkTime** | Midrange | `survive`, `establish_interruptions`, `push_lethal` | `101402052`, `101402001` | `handtraps`, `backrow` |
| **EvilTwin** | Combo | `survive`, `establish_interruptions`, `push_lethal` | `60764609`, `36326160`, `73810864` | `handtraps`, `graveyard_hate` |
| **EyeInside** | Combo | `survive`, `establish_interruptions`, `push_lethal` | `28954097`, `92565383`, `95365081` | `handtraps`, `boardwipes` |
| **Goldlord** | Control | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `95825075` *(Eldorado Adelantado - missing in YDK)* | `graveyard_hate`, `backrow_removal` |
| **Hecahand** | Go Second | `survive`, `establish_interruptions`, `push_lethal` | `95365081`, `20415050` | `floodgate`, `handtraps` |
| **Invoke** | Midrange | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `86120751`, `74063034` | `handtraps`, `negation` |
| **Kwtune** | Combo | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | *(None defined: `[]`)* | `handtraps`, `boardwipes` |
| **Labrynth** | Control | `survive`, `establish_interruptions`, `push_lethal`, `break_board` | `23440079` *(Labrynth Coelophys - missing in YDK)* | `backrow_removal`, `graveyard_hate` |

### Key Configuration Discrepancies
1.  **Goldlord Choke Point:** The configured choke point is card `95825075` (Eldorado Adelantado). However, this card is **not present** in the active `2026_Goldlord.ydk` deck list.
2.  **Labrynth Choke Point:** The configured choke point is card `23440079` (Labrynth Coelophys). Similarly, this card is **not present** in the active `2026_Labrynth.ydk` deck list.
3.  **Kwtune Choke Points:** No choke points are configured (`[]`) for this combo deck, meaning the AI does not recognize any of its own or the opponent's cards as high-risk targets for disruption.

---

## 4. Deck Playstyles, Goals, and Weaknesses

1.  **AzaYummy (Combo):**
    *   *Playstyle:* Combo going first. Establishes board presence with multiple Level 1 Yummy monsters and quick-synchro summon setups.
    *   *Goals:* Set up interruption boards (e.g. Snatchy, Level 2 Synchros) and push for lethal on the following turn.
    *   *Weaknesses:* Vulnerable to handtraps (Ash, Impermanence) and board clearing effects (Raigeki, Evenly Matched).
2.  **BrElfnote (Midrange):**
    *   *Playstyle:* Balanced going first or second. Relies on recurrable resources and moderate disruption.
    *   *Goals:* Maintain resource loops, deny opponent extensions, and grind down opponents.
    *   *Weaknesses:* Lockout floodgates and high-tempo combos that bypass midrange grinding.
3.  **DarkTime (Midrange):**
    *   *Playstyle:* Midrange deck focused on tempo and graveyard setups.
    *   *Goals:* Survive early game, establish quick interactions, and close out matches.
    *   *Weaknesses:* Handtraps on initial searches, heavy backrow setups that limit monster-based responses.
4.  **EvilTwin (Combo):**
    *   *Playstyle:* Combo going first. Link climbs using the Live☆Twin / Evil★Twin archetype.
    *   *Goals:* Summon Kisikil and Lilla, draw cards, pop cards on opponent's turn, and push lethal.
    *   *Weaknesses:* Crucially weak to handtraps targeting the normal summon starter, and graveyard banishing/hate.
5.  **EyeInside (Combo):**
    *   *Playstyle:* High-ceiling combo going first.
    *   *Goals:* Establish multiple negates/interruptions.
    *   *Weaknesses:* Handtraps and massive board wipes that clear their established negates.
6.  **Goldlord (Control):**
    *   *Playstyle:* Trap-heavy control deck (Eldlich-based).
    *   *Goals:* Out-resource the opponent using graveyard recursion and trap cards, then summon Golden Lord for high ATK pressure.
    *   *Weaknesses:* Graveyard banishing (Called by the Grave, Bystials) and heavy backrow removal (Harpie's Feather Duster).
7.  **Hecahand (Go Second):**
    *   *Playstyle:* Dedicated OTK/Board-breaker going second.
    *   *Goals:* Break opponent boards via high-ATK pushes, survive disruption, and win on Turn 2.
    *   *Weaknesses:* Floodgates that restrict special summons or attacks, and well-timed handtraps.
8.  **Invoke (Midrange):**
    *   *Playstyle:* Midrange fusion deck (Aleister the Invoker engine).
    *   *Goals:* Establish a Mechaba negate on Turn 1, loop Invocation, and grind down the opponent.
    *   *Weaknesses:* Handtraps on Aleister's normal summon search, and negations on key Fusion spells.
9.  **Kwtune (Combo):**
    *   *Playstyle:* Fast-paced combo setup.
    *   *Goals:* Sync-summon boss monsters, establish disruptions, and push lethal.
    *   *Weaknesses:* Handtraps, boardwipes.
10. **Labrynth (Control):**
    *   *Playstyle:* Trap-based control.
    *   *Goals:* Chain normal trap cards to trigger Labrynth monster effects, loop resources, and dismantle opponent strategies.
    *   *Weaknesses:* Backrow removal (Cosmic Cyclone, Feather Duster), graveyard control/hate.
