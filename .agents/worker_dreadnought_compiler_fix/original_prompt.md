## 2026-05-25T09:30:41Z
You are the worker agent teamwork_preview_worker.
Your working directory is: c:\Users\admin\Documents\EDOTh\.agents\worker_dreadnought_compiler_fix

Your tasks are:
1. Open `c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sources\DreadnoughtExecutor.cs` and replace all occurrences of `CardLocation.Graveyard` with `CardLocation.Grave`.
2. Run `compile_ai.bat` from `c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sources`. Verify that the compilation completes successfully and prints "Compilation SUCCESSFUL!" and has no errors.
3. If any other compiler errors or warnings arise, resolve them within `DreadnoughtExecutor.cs` until the build is completely successful.
4. Verify that the files `c:\Users\admin\Documents\EDOTh\WindBot\config\cards_registry_2026_Dreadnought.json` and `c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sandbox\cards_registry_2026_Dreadnought.json` exist, are non-empty, and that all heuristic priority values are strictly capped at 8 (under no circumstances should any priority value be 9 or greater).
5. Write your findings, compilation logs, and validation results to your handoff.md in your working directory and notify the orchestrator (conversation ID: bf8461fc-41d6-4865-aeff-4e1495fe08be).

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.
