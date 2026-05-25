# System Context and Reference Indices

## Directories and Important Files
- **C# WindBot Project**: `c:\Users\admin\Documents\EDOTh\WindBot\`
  - Executors: `WindBot/UnifiedIgnisExecutor.cs`, `WindBot/InvokeExecutor.cs`
  - Base Executor: `WindBot/BaseCustomExecutor.cs`
  - Bots registration: `WindBot/bots.json`
  - Decks configuration: `WindBot/config/decks/`
  - Cards registry configuration: `WindBot/config/cards_registry_{deck}.json`
- **Python Sandbox**: `c:\Users\admin\Documents\EDOTh\WindBot_Sandbox\`
  - Sandbox cards registry: `WindBot_Sandbox/cards_registry_{deck}.json`
- **Logs**: `Logs/` directory (created dynamically during duels)
- **Database**: `statistics.db`
- **AI Compilation script**: `compile_ai.bat`

## Key Reference Docs
- `Docs/2026_dreadnought_deck_analysis.md`: Detailed HERO combo analysis, card roles, and combo lines.
- `Docs/Rules.md`: IGNIS WindBot system rules, safeguards, and the 8 Iron Rules.
- `Docs/SKILL.md`: Karpathy programming guidelines (simplicity, surgical changes, thinking first).

## Guidelines & Constraints
- Iron Rules:
  1. Handtraps are blocked on our own turn.
  2. Do not chain to negate our own cards.
  3. Called by the Grave, Bystials, and Impermanence must check target availability.
  4. Default fallbacks in `OnDefaultActivate`, `OnDefaultSummon`, `OnDefaultSpSummon` must return `false`.
  5. Hardcap card registry priorities at 8.
  6. OnChaining controller directions must not be swapped.
  7. `GetNextPlan()` resets to `PlanA`.
  8. `_learningApplied` guard in `ApplyRealTimeLearning()` to prevent double-saving.
