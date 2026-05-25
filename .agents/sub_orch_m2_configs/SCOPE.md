# Scope: Milestone 2: Registries & Deck Configs

## Architecture
- WindBot configuration files directory: `WindBot/config/`
- Target registry files: `cards_registry_2026_<DeckName>.json` (Goldlord, Invoke, Kwtune, Labrynth)
- Target deck config JSON files: `WindBot/config/decks/2026_<DeckName>.json` (10 decks)
- Tool for registry generation/optimization: `WindBot_Sandbox/auto_role_detector.py` and `WindBot_Sandbox/optimize_registry.py`

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Populate Registries | Populate key cards for the 4 bricked decks in their card registries. | None | DONE |
| 2 | Configure Playstyles | Create/update JSON configurations for all 10 target decks with proper playstyle settings. | None | DONE |
| 3 | Verification | Check no deck has empty registry, all JSON configs exist and have proper playstyle settings. | M1, M2 | IN_PROGRESS |

## Interface Contracts
- Registries must be valid JSON objects mapping card IDs to role/information structures.
- Deck configs must be JSON files containing appropriate settings (e.g., `"PlayStyle"`: `"first"` or `"second"` or similar config keys).
