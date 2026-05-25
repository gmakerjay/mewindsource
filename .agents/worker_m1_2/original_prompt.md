## 2026-05-25T02:32:47Z
You are teamwork_preview_worker (Worker 2).
Your working directory is: c:\Users\admin\Documents\EDOTh\.agents\worker_m1_2\.
Your task is to:
1. Initialize BRIEFING.md and progress.md in your working directory.
2. Read the global SCOPE.md at c:\Users\admin\Documents\EDOTh\.agents\sub_orch_m1_csharp\SCOPE.md.
3. Review the Reviewer findings in:
   - c:\Users\admin\Documents\EDOTh\.agents\reviewer_m1_1\review.md
   - c:\Users\admin\Documents\EDOTh\.agents\reviewer_m1_2\review.md
4. Fix the following code bugs in c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs:
   - **Fix OnSelectCard**:
     Re-implement `OnSelectCard` to declare `available` and `preferHighPriority` properly, correct the brace structure (which is currently prematurely closed at line 2976), wrap it in a try-catch block, and delegate to `base.OnSelectCard(cards, min, max, hint, cancelable)` on failure.
   - **Fix SaveConfiguration Opponent Statistics Merging**:
     In opponent memory merging (around line 909-910), accumulate `times_seen` and `times_disrupted_us` using addition (`+=`) instead of `Math.Max`.
5. Compile the C# project by executing c:\Users\admin\Documents\EDOTh\WindBot\compile_ai.bat using the run_command tool. Ensure compilation succeeds with NO warnings or errors.
6. Verify your implementation by checking for compilation output.
7. Save a report of changes made and verification results to c:\Users\admin\Documents\EDOTh\.agents\worker_m1_2\changes.md.
8. Write handoff.md and send a message back to the parent conversation ID when complete.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT
hardcode test results, create dummy/facade implementations, or
circumvent the intended task. A Forensic Auditor will independently
verify your work. Integrity violations WILL be detected and your
work WILL be rejected.
