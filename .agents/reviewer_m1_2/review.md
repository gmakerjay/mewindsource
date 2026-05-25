## Review Summary

**Verdict**: REQUEST_CHANGES

The C# AI Engine implementation in `BaseCustomExecutor.cs` contains a critical syntax error inside the `OnSelectCard` hook override, which completely prevents the project from compiling. Additionally, some lifecycle hooks do not delegate to base under normal execution, and the configuration merging logic has major inaccuracies in counting card statistics.

---

## Findings

### [Critical] Finding 1: Syntax Error and Undeclared Identifiers in `OnSelectCard`

- **What**: The override of `OnSelectCard` contains syntax errors and undeclared identifiers:
  - `available` is referenced on line 2971 but is never declared.
  - `preferHighPriority` is modified and referenced but never declared.
  - A misplaced closing brace `}` on line 2976 prematurely closes the method body of `OnSelectCard`, leaving lines 2978 to 3025 floating outside of any method within the class body.
  - The hook is not wrapped in `try-catch-finally` blocks and does not delegate to base when an exception is thrown.
- **Where**: `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`, lines 2969-2977.
- **Why**: This prevents compilation of the entire library (`UnifiedIgnisExecutor.dll`).
- **Suggestion**: Replace `OnSelectCard` with a safely wrapped implementation:
  ```csharp
  public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
  {
      try
      {
          if (cards == null || cards.Count == 0)
          {
              return base.OnSelectCard(cards, min, max, hint, cancelable);
          }

          List<ClientCard> available = new List<ClientCard>(cards);
          bool preferHighPriority = true;
          
          if (available.Count > 0)
          {
              CardLocation loc = available[0].Location;
              if (loc == CardLocation.Hand || loc == CardLocation.MonsterZone || loc == CardLocation.SpellZone)
              {
                  preferHighPriority = false;
              }
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
          try
          {
              return base.OnSelectCard(cards, min, max, hint, cancelable);
          }
          catch
          {
              return new List<ClientCard>();
          }
      }
  }
  ```

### [Major] Finding 2: Inaccurate Merging of Opponent Memory Statistics in `SaveConfiguration`

- **What**: When merging opponent memory from concurrent instances, the seen counts and disruptions are merged using `Math.Max` instead of accumulation:
  ```csharp
  diskMeta.times_seen = Math.Max(diskMeta.times_seen, ourMeta.times_seen);
  diskMeta.times_disrupted_us = Math.Max(diskMeta.times_disrupted_us, ourMeta.times_disrupted_us);
  ```
- **Where**: `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`, lines 909-910.
- **Why**: Seen and disruption counts from parallel matches will be under-counted and stats will be skewed.
- **Suggestion**: Accumulate the counts using addition:
  ```csharp
  diskMeta.times_seen += ourMeta.times_seen;
  diskMeta.times_disrupted_us += ourMeta.times_disrupted_us;
  ```

### [Major] Finding 3: `OnSelectHand` Lacks Base Delegation on Successful Path

- **What**: `OnSelectHand` only delegates to base inside the `catch` block. The normal path returns `true` or `false` directly.
- **Where**: `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`, lines 2572-2605.
- **Why**: While returning a custom value is correct, this bypasses any base class state changes that may occur during `OnSelectHand()`.
- **Suggestion**: Verify if base class handles any state we should not bypass, or explicitly document this design decision.

---

## Verified Claims

- **Preconditions of ApplyRealTimeLearning are relaxed with proper LP fallbacks and early-aborts for empty matches** → verified via source code analysis of `ApplyRealTimeLearning()` lines 957-976 → **PASS** (uses `_lastBotLP` and `_lastOppLP` when `Duel` state is unavailable, and aborts immediately when `_ourCardsPlayed.Count == 0`).
- **WeakReference-based static list tracks active instances thread-safely and handles ProcessExit/DomainUnload events** → verified via inspection of static locks, AppDomain hook registrations, and destructor/disposal removal loops → **PASS** (correctly locks `_staticLock` and registers/unregisters using `WeakReference`).
- **OnDraw override is correctly implemented** → verified via inspection of lines 3158-3185 → **PASS** (safely checks `Duel`, wraps inside try-catch-finally, and delegates to `base.OnDraw`).
- **SaveConfiguration has thread-safe merging of Json configurations** → verified via presence of `lock (_staticLock)` block surrounding configuration read/write operations → **PASS** (file accesses are serialized thread-safely).

---

## Coverage Gaps

- **Compilation Verification** — risk level: HIGH — recommendation: The compilation could not be verified in practice due to the `run_command` permission prompt timing out waiting for a user response. The syntax errors in `OnSelectCard` make compilation failure 100% certain.

---

## Unverified Items

- **Physical execution of `compile_ai.bat`** — reason not verified: permission prompt timed out.
