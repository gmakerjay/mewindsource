# BRIEFING — 2026-05-25T09:23:00+07:00

## Mission
Investigate and diagnose C# codebase issues in lifecycle hooks, process exit handlers, and ApplyRealTimeLearning preconditions, proposing safe fixes.

## 🔒 My Identity
- Archetype: Explorer
- Roles: Teamwork Explorer
- Working directory: c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_1
- Original parent: d980c172-ff62-451b-8d02-f6321a68df98
- Milestone: Milestone 1 C# Executor Fixes

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Analyze codebase and identify issues in lifecycle hooks, process exit handlers, and ApplyRealTimeLearning outcomes

## Current Parent
- Conversation ID: d980c172-ff62-451b-8d02-f6321a68df98
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `BaseCustomExecutor.cs` (lifecycle hooks, process exit, learning preconditions, dispose, static fields)
  - `UnifiedIgnisExecutor.cs`
  - `PureYummyExecutor.cs`
  - `InvokeExecutor.cs`
  - `ExecutorBase.dll` (via `dll_types.txt` reflections)
  - `DefaultExecutor.cs` (in windbot-master)
- **Key findings**:
  - `OnDraw` hook is completely missing in `BaseCustomExecutor.cs`.
  - Null reference vulnerabilities in `OnNewTurn()`, `OnBattle()`, `OnSelectAttackTarget()`, `OnChaining()`, `OnChainEnd()`.
  - Process exit handlers suffer from race conditions (no locks on file writing) and data loss for all but the last active executor instance.
  - `ApplyRealTimeLearning` fails entirely on disconnects/timeouts due to strict null validation checks on `Duel` object.
  - Priority decay corrupts config if game is aborted before start.
- **Unexplored areas**: None. The scope of C# hooks and safeguards audit is completed.

## Key Decisions Made
- Use static locks and a multi-instance list of active executors in the process exit and domain unload handlers to ensure thread safety and avoid data loss.
- Track last known LP and Turn dynamically in memory as a fallback, enabling learning persistence on network disconnects.
- Return early from `ApplyRealTimeLearning` if `_ourCardsPlayed.Count == 0` to prevent decay of starter cards on early aborted matches.

## Artifact Index
- c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_1\original_prompt.md — Dispatch prompt and updates archive.
- c:\Users\admin\Documents\EDOTh\.agents\explorer_m1_1\analysis.md — Detailed audit analysis of C# AI engine.
