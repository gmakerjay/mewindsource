# Changelog - Scoring Weight Adjustments

**Timestamp**: 2026-05-24T01:05:00+07:00  
**Author**: Antigravity AI  

---

## 1. Scoring Weight Adjustments

- **C# Engine (`UnifiedIgnisExecutor.cs`)**:
  - Increased the penalty weight for blocked/dead combo plans in `EvaluateCardAction()` from `-40.0` to `-90.0`. This prevents the bot from blindly playing/extending blocked combos under high threat levels.

---

## 2. Verification

- Compiled `UnifiedIgnisExecutor.cs` using `compile_ai.bat` to verify syntactical correctness and successful compilation.
