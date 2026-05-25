# Project plan for Dreadnought AI Deck Implementation

This plan outlines the milestones, dependencies, and verification criteria for implementing and training the 2026_Dreadnought AI deck.

## Milestones

| Milestone | Name | Scope | Dependencies | Status |
|---|---|---|---|---|
| M1 | C# Executor Implementation | Implement `DreadnoughtExecutor.cs` with combo lines and safeguards. Compile and verify. | None | PLANNED |
| M2 | Configs & Registration | Register bot in `bots.json`, create `2026_Dreadnought.json` playstyle config, and generate `cards_registry_2026_Dreadnought.json` and its sandbox counterpart. | M1 | PLANNED |
| M3 | Pipeline Training & Verification | Run multi-instance bot-vs-bot simulation to train Q-values and verify weight updates. | M1, M2 | PLANNED |

## Detailed Steps

### Milestone 1: C# Executor Implementation & Safeguards
- **Objective**: Create `DreadnoughtExecutor.cs` in the `WindBot` directory.
- **Card Safeguards**:
  - `Destiny HERO - Doom Liege` (101402022): Cost-send D-HERO to GY -> search field spell `101402062`.
  - `Clock Tower Prison City - Dark City` (101402062): Active turn search, and trigger special summon on destruction.
  - `Destiny HERO - Dreadnought Servant` (101402023): Special summon from hand -> destroy field spell -> search Polymerization.
  - `Destiny HERO - Dreadnought` (101402037): Alternative summon by sending Dreadmaster to GY -> search 2 cards on summon.
  - `Destiny HERO - Death Dogma` (101402021): GY banish summon, burn damage, and Quick Fusion chain reaction.
  - Supporting cards: `D - Burst` (100456010), `Masked HERO Dusk Crow` (10808715), `Masked HERO Furnace` (58288218), `Masked HERO Fountain` (66206748).
- **Verification**: Run `compile_ai.bat` via a Worker to ensure clean compilation.

### Milestone 2: Configuration & Bot Registration
- **Objective**: Register `2026_Dreadnought` in `WindBot/bots.json`. Create the playstyle config `WindBot/config/decks/2026_Dreadnought.json`. Generate cards registry `WindBot/config/cards_registry_2026_Dreadnought.json` and its sandbox counterpart `WindBot_Sandbox/cards_registry_2026_Dreadnought.json`.
- **Registry Constraints**: Roles categorized (starter, extender, handtrap, etc.), priority capped at 8 (Iron Rule #5).
- **Verification**: Run a validation check on config existence and structural correctness.

### Milestone 3: Pipeline Training & Performance Verification
- **Objective**: Run simulated training rounds using `verify_pipeline.py` or `run_multi_iterations.py`. Verify `statistics.db` update and weights adjustment in `cards_registry_2026_Dreadnought.json`.
- **Verification**: Output before-and-after Q-values to prove learning.

## Interface Contracts
- **WindBot ↔ Python Sandbox**:
  - Match outcomes logged thread-safely to `statistics.db` (`matches` and `decisions` tables).
  - Decision logs stored in `decisions.jsonl`.
  - Python Q-learning reads `decisions.jsonl` and writes back trained Q-values to `cards_registry_2026_Dreadnought.json`.
