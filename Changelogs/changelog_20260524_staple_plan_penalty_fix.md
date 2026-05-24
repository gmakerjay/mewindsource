# Changelog - Staple Combo Plan Penalty Bug Fix

**Timestamp**: 2026-05-24T10:39:00+07:00  
**Author**: Antigravity AI  

---

## 1. Description of Change

- **C# Scoring Engine (`UnifiedIgnisExecutor.cs`)**:
  - Modified `EvaluateCardAction()` to check roles before applying the `-90.0` penalty for cards whose combo plans are blocked (`isBlocked`).
  - Previously, general staples (e.g. *Triple Tactics Talent*, *Triple Tactics Thrust*) containing default/PlanA configurations suffered this penalty whenever the combo plan branched/fallback occurred (e.g., due to opponent's Ash Blossom).
  - Now, the `-90.0` penalty is exclusively applied to cards with deck-specific combo roles (`starter`, `extender`, `combo_piece`, or `payoff`). Staple and generic utility cards will not be penalized, allowing the bot to correctly evaluate and play them to recover or resolve threats.

---

## 2. Code Difference

```diff
                 if (isBlocked)
                 {
-                    score -= 90.0;
-                    LogToTurn(string.Format("Penalizing dead combo card: {0} because its plan is blocked.", GetCardName(card.Id)));
+                    // Only penalize if the card has deck-specific combo roles (starter, extender, combo_piece, payoff)
+                    if (meta.roles.Contains("starter") || meta.roles.Contains("extender") || meta.roles.Contains("combo_piece") || meta.roles.Contains("payoff"))
+                    {
+                        score -= 90.0;
+                        LogToTurn(string.Format("Penalizing dead combo card: {0} because its plan is blocked.", GetCardName(card.Id)));
+                    }
                 }
```

---

## 3. Verification

- Ran `compile_ai.bat` in `WindBot` directory.
- Compilation finished successfully with **0 errors** and **0 warnings**.
- The updated dll `Executors/UnifiedIgnisExecutor.dll` was generated.
