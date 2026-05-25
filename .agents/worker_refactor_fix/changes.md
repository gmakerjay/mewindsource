# Changes Made

## WindBot/BaseCustomExecutor.cs
Modified the non-overloaded virtual `OnCardAction` method to wrap modifications of `_ourCardsPlayed` in a `lock (_staticLock)` block.

### Details:
- **Location**: `WindBot/BaseCustomExecutor.cs` (lines 2442–2449)
- **Before**:
  ```csharp
  if (result)
  {
      if (!_ourCardsPlayed.Contains(cardId))
          _ourCardsPlayed.Add(cardId);
  }
  ```
- **After**:
  ```csharp
  if (result)
  {
      lock (_staticLock)
      {
          if (!_ourCardsPlayed.Contains(cardId))
              _ourCardsPlayed.Add(cardId);
      }
  }
  ```
