# Scope: Milestone 1: C# Hooks & Safeguards Audit

## Architecture
- Executable/project layout containing `UnifiedIgnisExecutor.cs` and `BaseCustomExecutor.cs`.
- Compilation verification via `compile_ai.bat`.
- Lifecycles/Hooks mapping to base classes/interfaces for executor behavior.
- Real-time learning outcomes saving on game exit/match timeout/disconnects.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | C# Hooks & Safeguards Audit | Audit/fix lifecycle hooks, process exit static flags, ApplyRealTimeLearning preconditions, and verify compilation | None | DONE |

## Interface Contracts
### UnifiedIgnisExecutor ↔ BaseCustomExecutor
- Handlers for: `OnNewTurn`, `OnNewPhase`, `OnSelectHand`, `OnBattle`, `OnSelectAttackTarget`, `OnSelectCard`, `OnChaining`, `OnChainEnd`, and `OnDraw`.
- Hooks must execute safeguards and delegate properly to custom executor logic without crashes or resource leaks.
