## 2026-05-25T21:07:19Z

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Modify and refactor the EDOTh WindBot system files to implement direct attack replay fix, robust fusion material selection, safe SQL writes with WAL retry transactions, and automatic brain deployment.

Tasks:
1. Modify c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs:
   - In OnSelectAttackTarget, remove the duplicate direct attack check (around lines 3145-3149) that declares direct attacks even if opponent monsters exist. Direct attacks should ONLY be declared if defenders list is empty or null (lines 3135-3143).
   - In the turn reset logic (around lines 2781-2787), change the condition check from `(Duel.Turn == 1 && _turnCount > 1)` to `(Duel.Turn == 1 && _turnCount >= 1)` to ensure the bot resets its logging state if game 1 ends on Turn 1.

2. Modify c:\Users\admin\Documents\EDOTh\WindBot\DreadnoughtExecutor.cs:
   - In GetOptimalFusionMaterials, if _lastSelectedFusionId is 0 or unmatched (the else block of recipe checks), match the combination against all valid recipes of the deck:
     isValid = IsDpeRecipe(combo) || IsDreadnoughtRecipe(combo) || IsDystopiaRecipe(combo) || IsDangerousRecipe(combo) || IsTrinityRecipe(combo) || IsContrastHeroChaosRecipe(combo);
   - In OnSelectCard, when hint is HintMsg_FusionMaterial, store the result of GetOptimalFusionMaterials, set _lastSelectedFusionId = 0, and then return the materials.

3. Modify c:\Users\admin\Documents\EDOTh\WindBot\InvokeExecutor.cs:
   - In GetOptimalFusionMaterials, if _lastSelectedFusionId is 0 or unmatched (the else block of recipe checks), match the combination against all valid recipes of the deck:
     isValid = IsInvokedMechabaRecipe(combo) || IsInvokedPurgatrioRecipe(combo) || IsInvokedSorathRecipe(combo) || IsInvokedBabalonRecipe(combo) || IsInvokedOkeanosRecipe(combo) || IsInvokedCaligaRecipe(combo) || IsInvokedRaidjinRecipe(combo) || IsInvokedMagellanicaRecipe(combo) || IsInvokedAugoeidesRecipe(combo) || IsInvokedElysiumRecipe(combo) || IsInvokedTranscendenceAeonRecipe(combo);
   - In OnSelectCard, when hint is HintMsg_FusionMaterial, store the result of GetOptimalFusionMaterials, set _lastSelectedFusionId = 0, and then return the materials.

4. Replace the contents of c:\Users\admin\Documents\EDOTh\Developer\scratch\save_outcomes_to_sql.py with the content of c:\Users\admin\Documents\EDOTh\.agents\explorer_db_concurrency\proposed_save_outcomes_to_sql.py verbatim.

5. Modify c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sandbox\cockpit.py and c:\Users\admin\Documents\EDOTh\WindBot_Sandbox\cockpit.py (update both copies):
   - In run_live_duel_loop, after each duel match finishes (inside the iterations loop, after the `while p1.poll() is None or p2.poll() is None:` loop terminates):
     - Sync the updated JSON card registry (cards_registry_{deck}.json or cards_registry.json) from WindBot/config/ to WindBot_Sandbox/ (using shutil.copy2).
     - Sync opponent_memory.json from WindBot/config/ to WindBot_Sandbox/.
     - Execute compile_ai.bat inside WindBot/ CW to automatically compile UnifiedIgnisExecutor.dll.
     Log the status of these operations in progress_log.

6. Verification:
   - Run WindBot/compile_ai.bat to verify the DLL compiles with no syntax errors.
   - Run python Developer/Scripts/verify_pipeline.py to verify that the database outcomes partitioning and learning pipeline run successfully.
   Report the verification results.
