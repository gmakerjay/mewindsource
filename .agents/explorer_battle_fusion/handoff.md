# Handoff Report: Fusion and Battle Analysis

## Observation

The following files and locations were investigated:
1. **`c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`**
   - Lines 3135-3143:
     ```csharp
     if (defenders == null || defenders.Count == 0)
     {
         if (attacker != null && attacker.CanDirectAttack)
         {
             LogToTurn(string.Format("Battle Phase: {0} attacking directly.", GetCardName(attacker.Id)));
             return AI.Attack(attacker, null);
         }
         return null;
     }
     ```
   - Lines 3145-3149:
     ```csharp
     if (attacker != null && attacker.CanDirectAttack)
     {
         LogToTurn(string.Format("Battle Phase: {0} attacking directly.", GetCardName(attacker.Id)));
         return AI.Attack(attacker, null);
     }
     ```
2. **`c:\Users\admin\Documents\EDOTh\WindBot\DreadnoughtExecutor.cs`**
   - Lines 981-1022: Contains `GetOptimalFusionMaterials` with the recipe checks (DPE, Dreadnought, Dystopia, Dangerous, Trinity, Contrast HERO Chaos).
   - Lines 1013-1016:
     ```csharp
     else
     {
         isValid = true;
     }
     ```
   - Lines 696-704: Contains `OnSelectCard` intercepting `HintMsg_SpSummon`:
     ```csharp
     public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
     {
         IList<ClientCard> selected = OnSelectCardInternal(cards, min, max, hint, cancelable);
         if (hint == HintMsg_SpSummon && selected != null && selected.Count > 0)
         {
             _lastSelectedFusionId = selected[0].Id;
         }
         return selected;
     }
     ```
   - Lines 876-879:
     ```csharp
     // 4. Fusion Material Selection (Polymerization, Death Dogma, etc.)
     if (hint == HintMsg_FusionMaterial)
     {
         return GetOptimalFusionMaterials(cards, min, max);
     }
     ```
3. **`c:\Users\admin\Documents\EDOTh\WindBot\InvokeExecutor.cs`**
   - Lines 685-741: Contains `GetOptimalFusionMaterials` with the recipe checks (Mechaba, Purgatrio, Sorath, Babalon, Okeanos, Caliga, Raidjin, Magellanica, Augoeides, Elysium, Transcendence Aeon).
   - Lines 737-740:
     ```csharp
     else
     {
         isValid = true;
     }
     ```
   - Lines 423-431: Contains `OnSelectCard` intercepting `HintMsg_SpSummon`:
     ```csharp
     public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
     {
         IList<ClientCard> selected = OnSelectCardInternal(cards, min, max, hint, cancelable);
         if (hint == HintMsg_SpSummon && selected != null && selected.Count > 0)
         {
             _lastSelectedFusionId = selected[0].Id;
         }
         return selected;
     }
     ```
   - Lines 493-496:
     ```csharp
     // 2. Fusion Material Selection (Invocation / Invocation Sword)
     if (hint == HintMsg_FusionMaterial)
     {
         return GetOptimalFusionMaterials(cards, min, max);
     }
     ```

## Logic Chain

1. **Direct Attack logic in `OnSelectAttackTarget`**:
   - At line 3135, if `defenders` list is null or empty, the executor correctly evaluates if the attacker can direct attack, declares it and returns.
   - At line 3145, when `defenders` has items (since the previous condition was bypassed), the executor still declares a direct attack if `CanDirectAttack` is true. This can result in illegal direct attacks if the card does not have an effect allowing direct attacks when opponent has monsters, causing replay desync or game engine errors.
   - Removing the second check at line 3145-3149 and relying exclusively on the null/empty check ensures a direct attack is only declared when the opponent controls no monsters.

2. **Matching Materials against Deck Recipes**:
   - Currently, if `_lastSelectedFusionId` is 0 or doesn't match any known recipe, `isValid` defaults to `true`. This causes `GetOptimalFusionMaterials` to treat any subset of materials of length `min` as valid, leading to illegal material choices for fusion spells.
   - Refactoring the `else` branch to perform logical OR checks across all the deck's specific fusion recipe predicates guarantees that any matched material combination is legal for at least one of the deck's fusion recipes.

3. **Intercepting `HintMsg_SpSummon`**:
   - Inspection of `OnSelectCard` in both `DreadnoughtExecutor.cs` (lines 696-704) and `InvokeExecutor.cs` (lines 423-431) shows they both already correctly intercept `hint == HintMsg_SpSummon` and store the selected Fusion Monster's ID in `_lastSelectedFusionId`. No new interception logic is required for this task.

4. **Resetting `_lastSelectedFusionId`**:
   - Storing the selected materials in a local variable in the `HintMsg_FusionMaterial` block of `OnSelectCard`, setting `_lastSelectedFusionId = 0`, and then returning the materials ensures that `_lastSelectedFusionId` is cleanly reset to 0 once the selection is successful and complete.

## Caveats

- We assume that removing the direct attack check on line 3145-3149 will not affect legitimate direct-attack-capable cards that might attack directly when opponent monsters exist (e.g., Mariner, Ceal) if the AI does not rely on them. If the AI does play such cards and must declare direct attacks over monsters, the engine handles target selection differently or we would need a more sophisticated check. Under current requirements, removing it is the specified way to prevent illegal replays.

## Conclusion

The proposed code changes will prevent illegal direct attacks during replays and ensure legal fusion material selection by checking combinations against all valid fusion recipes of the deck when `_lastSelectedFusionId` is 0 or unmatched.

Proposed changes are detailed below.

### Proposed Code Changes

#### 1. Remove Direct Attack Check when Opponent Has Monsters
**File**: `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs`

Before:
```csharp
                if (defenders == null || defenders.Count == 0)
                {
                    if (attacker != null && attacker.CanDirectAttack)
                    {
                        LogToTurn(string.Format("Battle Phase: {0} attacking directly.", GetCardName(attacker.Id)));
                        return AI.Attack(attacker, null);
                    }
                    return null;
                }

                if (attacker != null && attacker.CanDirectAttack)
                {
                    LogToTurn(string.Format("Battle Phase: {0} attacking directly.", GetCardName(attacker.Id)));
                    return AI.Attack(attacker, null);
                }

                if (attacker == null)
```

After:
```csharp
                if (defenders == null || defenders.Count == 0)
                {
                    if (attacker != null && attacker.CanDirectAttack)
                    {
                        LogToTurn(string.Format("Battle Phase: {0} attacking directly.", GetCardName(attacker.Id)));
                        return AI.Attack(attacker, null);
                    }
                    return null;
                }

                if (attacker == null)
```

---

#### 2. Fallback Recipe Matching and Resetting `_lastSelectedFusionId` in `DreadnoughtExecutor.cs`
**File**: `c:\Users\admin\Documents\EDOTh\WindBot\DreadnoughtExecutor.cs`

Before (Recipe Selection):
```csharp
                else if (_lastSelectedFusionId == 23204029) // Contrast HERO Chaos
                {
                    isValid = IsContrastHeroChaosRecipe(combo);
                }
                else
                {
                    isValid = true;
                }

                if (isValid)
```

After (Recipe Selection):
```csharp
                else if (_lastSelectedFusionId == 23204029) // Contrast HERO Chaos
                {
                    isValid = IsContrastHeroChaosRecipe(combo);
                }
                else
                {
                    isValid = IsDpeRecipe(combo)
                           || IsDreadnoughtRecipe(combo)
                           || IsDystopiaRecipe(combo)
                           || IsDangerousRecipe(combo)
                           || IsTrinityRecipe(combo)
                           || IsContrastHeroChaosRecipe(combo);
                }

                if (isValid)
```

Before (Material Selection reset):
```csharp
            // 4. Fusion Material Selection (Polymerization, Death Dogma, etc.)
            if (hint == HintMsg_FusionMaterial)
            {
                return GetOptimalFusionMaterials(cards, min, max);
            }
```

After (Material Selection reset):
```csharp
            // 4. Fusion Material Selection (Polymerization, Death Dogma, etc.)
            if (hint == HintMsg_FusionMaterial)
            {
                IList<ClientCard> materials = GetOptimalFusionMaterials(cards, min, max);
                _lastSelectedFusionId = 0;
                return materials;
            }
```

---

#### 3. Fallback Recipe Matching and Resetting `_lastSelectedFusionId` in `InvokeExecutor.cs`
**File**: `c:\Users\admin\Documents\EDOTh\WindBot\InvokeExecutor.cs`

Before (Recipe Selection):
```csharp
                else if (_lastSelectedFusionId == 101305033) // Transcendence Aeon
                {
                    isValid = IsInvokedTranscendenceAeonRecipe(combo);
                }
                else
                {
                    isValid = true;
                }

                if (isValid)
```

After (Recipe Selection):
```csharp
                else if (_lastSelectedFusionId == 101305033) // Transcendence Aeon
                {
                    isValid = IsInvokedTranscendenceAeonRecipe(combo);
                }
                else
                {
                    isValid = IsInvokedMechabaRecipe(combo)
                           || IsInvokedPurgatrioRecipe(combo)
                           || IsInvokedSorathRecipe(combo)
                           || IsInvokedBabalonRecipe(combo)
                           || IsInvokedOkeanosRecipe(combo)
                           || IsInvokedCaligaRecipe(combo)
                           || IsInvokedRaidjinRecipe(combo)
                           || IsInvokedMagellanicaRecipe(combo)
                           || IsInvokedAugoeidesRecipe(combo)
                           || IsInvokedElysiumRecipe(combo)
                           || IsInvokedTranscendenceAeonRecipe(combo);
                }

                if (isValid)
```

Before (Material Selection reset):
```csharp
            // 2. Fusion Material Selection (Invocation / Invocation Sword)
            if (hint == HintMsg_FusionMaterial)
            {
                return GetOptimalFusionMaterials(cards, min, max);
            }
```

After (Material Selection reset):
```csharp
            // 2. Fusion Material Selection (Invocation / Invocation Sword)
            if (hint == HintMsg_FusionMaterial)
            {
                IList<ClientCard> materials = GetOptimalFusionMaterials(cards, min, max);
                _lastSelectedFusionId = 0;
                return materials;
            }
```

## Verification Method

1. **Verify OnSelectAttackTarget removal**: Inspect `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs` at line 3145 to ensure the check is completely removed and that the remaining flow handles standard targeting correctly.
2. **Verify Recipe Fallback**: Run compilation and/or tests (e.g. `dotnet build` or project test scripts) to ensure there are no syntax/reference errors in the refactored boolean chains.
3. **Verify Material Selection Reset**: Inspect lines 876-879 in `DreadnoughtExecutor.cs` and lines 493-496 in `InvokeExecutor.cs` to confirm that `_lastSelectedFusionId` is set to 0 before the returned material list is returned.
