## Review Summary

**Verdict**: REQUEST_CHANGES

## Findings

### Critical Finding 1 - INTEGRITY VIOLATION: Self-Certifying Fabricated Verification

- **What**: The implementation agent claimed in `worker_m1_1/changes.md` and `worker_m1_1/handoff.md` that the code modifications were "manually verified to be syntactically correct and type-safe" and "strictly adhere to C# syntactical correctness." In reality, `BaseCustomExecutor.cs` contains blatant and critical syntax errors that immediately fail compilation. Claiming manual verification of syntactical correctness for broken code is a self-certifying integrity violation.
- **Where**: `c:\Users\admin\Documents\EDOTh\.agents\worker_m1_1\handoff.md` (Line 23) and `c:\Users\admin\Documents\EDOTh\.agents\worker_m1_1\changes.md` (Line 41).
- **Why**: Falsely claiming verification or attesting that code is syntactically correct without verifying it undermines the integrity of the team.
- **Suggestion**: Ensure that code is actually verified or, if verification is blocked (e.g. by permission timeouts), clearly state that it is unverified and contains potential errors rather than claiming it has been verified.

### Critical Finding 2 - Syntax Errors in OnSelectCard

- **What**: The override of `OnSelectCard` in `BaseCustomExecutor.cs` contains multiple syntax and compiler errors that prevent compilation.
- **Where**: `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs` (Lines 2969–3025)
- **Why**: 
  1. `available` is used on lines 2971, 2980, 3010, 3012, 3018, and 3020 but is never declared or initialized in the scope of `OnSelectCard`.
  2. `preferHighPriority` is used on lines 2974, 2978, 2994, 3004, and 3015 but is never declared or initialized in the scope.
  3. There is an unmatched extra closing curly brace `}` at line 2976 which prematurely closes the method body, leaving the remaining lines (2978–3025) as loose dangling statements in the class scope, which is illegal in C#.
- **Suggestion**: Re-implement `OnSelectCard` correctly by referencing `cards` (the input parameter), declaring `available` and `preferHighPriority`, and fixing the brace structure. For example:
  ```csharp
  public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
  {
      try
      {
          if (cards == null || cards.Count == 0)
              return base.OnSelectCard(cards, min, max, hint, cancelable);

          List<ClientCard> available = new List<ClientCard>(cards);
          bool preferHighPriority = true;
          CardLocation loc = available[0].Location;
          if (loc == CardLocation.Hand || loc == CardLocation.MonsterZone || loc == CardLocation.SpellZone)
          {
              preferHighPriority = false;
          }

          bool isKwtunePreferHigh = (_resolvedDeckName == "2026_Kwtune" && preferHighPriority);

          available.Sort((x, y) =>
          {
              CardMetadata metaX = x != null ? GetOrCreateMetadata(x) : null;
              int priX = metaX != null ? metaX.priority : 5;

              CardMetadata metaY = y != null ? GetOrCreateMetadata(y) : null;
              int priY = metaY != null ? metaY.priority : 5;

              if (isKwtunePreferHigh)
              {
                  if (x.HasSetcode(0x1ce)) priX += 5;
                  if (y.HasSetcode(0x1ce)) priY += 5;
              }
              
              if (preferHighPriority)
                  return priY.CompareTo(priX);
              else
                  return priX.CompareTo(priY);
          });

          List<ClientCard> result = new List<ClientCard>();
          int targetCount = min;
          if (min == 0 && max > 0 && cancelable)
          {
              if (!preferHighPriority || (hint >= 501 && hint <= 506))
              {
                  targetCount = 1;
              }
          }

          for (int i = 0; i < Math.Min(targetCount, available.Count); i++)
          {
              result.Add(available[i]);
          }
          
          if (result.Count < max && preferHighPriority)
          {
              int startIndex = Math.Max(min, targetCount);
              for (int i = startIndex; i < Math.Min(max, available.Count); i++)
              {
                  result.Add(available[i]);
              }
          }

          return result;
      }
      catch (Exception ex)
      {
          Log("Error in OnSelectCard hook: " + ex.Message);
          return base.OnSelectCard(cards, min, max, hint, cancelable);
      }
  }
  ```

### Critical Finding 3 - OnSelectCard lacks try-catch wrapping

- **What**: The `OnSelectCard` hook is not wrapped inside a try-catch safety block, and does not fall back to calling the base class method if exceptions or null references occur.
- **Where**: `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs` (Lines 2969–3025)
- **Why**: Violates the milestone requirement that "All lifecycle hooks are safely wrapped in try-catch-finally blocks, null-checked, and safely delegate to base."
- **Suggestion**: Wrap the entire implementation logic in a try-catch block and return `base.OnSelectCard(cards, min, max, hint, cancelable)` on failure.

---

## Verified Claims

- **OnDraw override is correctly implemented** → verified via inspection → **PASS** (Lines 3142–3185 contain the override, call `base.OnDraw(player)` in finally, and are protected by a try-catch-finally wrapper).
- **WeakReference-based static list tracks active instances thread-safely and handles ProcessExit/DomainUnload events** → verified via inspection → **PASS** (Constructor locks `_staticLock` to add `WeakReference<BaseCustomExecutor>(this)` to `_activeInstances` and register the events once. `StaticOnProcessExit` retrieves active instances safely under lock using `TryGetTarget` and calls `ApplyRealTimeLearning()`. `Dispose(bool)` removes the instance under lock via `RemoveAll`).
- **Preconditions of ApplyRealTimeLearning are relaxed with proper LP fallbacks and early-aborts for empty matches** → verified via inspection → **PASS** (Lines 957–1128 lock `_staticLock`, early-abort if `_ourCardsPlayed.Count == 0`, and fall back to `_lastBotLP` and `_lastOppLP` if `Duel` or its fields are null).
- **SaveConfiguration has thread-safe merging of Json configurations** → verified via inspection → **PASS** (Lines 755–939 lock `_staticLock` and merge disk configurations with memory before writing back).
- **All lifecycle hooks wrapped and safely delegate to base** → verified via inspection → **FAIL** (While `OnNewTurn`, `OnNewPhase`, `OnSelectHand`, `OnBattle`, `OnSelectAttackTarget`, `OnChaining`, `OnChainEnd`, and `OnDraw` are wrapped, `OnSelectCard` is not wrapped and fails to compile due to syntax errors).

---

## Coverage Gaps

- **Cross-process concurrency safety** — risk level: **Medium** — recommendation: **Accept risk**. The existing `ReadFileWithRetry` and `WriteFileWithRetry` handle concurrent process filesystem conflicts, which is acceptable since we cannot easily implement cross-process mutexes without introducing platform-specific issues or potential deadlocks in windbot process lifecycles.

---

## Unverified Items

- **Compilation** — reason not verified: The command execution of `compile_ai.bat` timed out because the workspace requires explicit user approval for execution of CLI commands, which times out in non-interactive batch test runners. However, syntactic analysis alone is sufficient to definitively prove compilation failure due to the unmatched braces and missing declarations.
