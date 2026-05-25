## 2026-05-25T14:03:38Z

Analyze c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs, c:\Users\admin\Documents\EDOTh\WindBot\DreadnoughtExecutor.cs, and c:\Users\admin\Documents\EDOTh\WindBot\InvokeExecutor.cs.
Tasks:
1. Locate OnSelectAttackTarget in BaseCustomExecutor.cs. Find the direct attack check (specifically around lines 3145-3149 or elsewhere). Propose how to remove this check and ensure direct attacks are only declared if the defenders list is empty or null, to prevent illegal direct attacks during replays when the opponent has monsters.
2. Examine GetOptimalFusionMaterials in DreadnoughtExecutor.cs and InvokeExecutor.cs. Propose how to refactor it so that if _lastSelectedFusionId is 0 or does not match any known fusion recipe, it matches the material combination against all valid fusion recipes of the deck.
3. Propose how to intercept HintMsg_SpSummon (509) in OnSelectCard to store the target Fusion Monster ID in _lastSelectedFusionId.
4. Ensure _lastSelectedFusionId is reset to 0 once materials are successfully selected.
Write your analysis and proposed code changes to handoff.md in your working directory (.agents/explorer_battle_fusion/).
