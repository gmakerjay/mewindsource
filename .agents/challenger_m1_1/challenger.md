# Adversarial Review of BaseCustomExecutor.cs

This report evaluates the robustness of the safeguards, thread-safety, and potential runtime errors in `BaseCustomExecutor.cs`.

## Challenge Summary

**Overall risk assessment**: HIGH

While the class contains many custom safeguards for Yu-Gi-Oh! gameplay logic, several critical thread-safety, concurrency, null reference, and logic correctness bugs exist that can lead to crashes, state corruption, or suboptimal plays under stress.

---

## Challenges

### [High] Challenge 1: Lost Update Concurrency on Shared JSON Config
- **Assumption challenged**: Locking `_staticLock` makes configuration saving safe.
- **Attack scenario**: In multi-instance or server-side setups where multiple WindBot processes run concurrently, they share the same physical config files (like `opponent_memory.json`). The `_staticLock` is only in-process. When Process A and Process B finish their duels around the same time, both read the file, merge their local memory, and write it back. Process B will overwrite Process A's updates, causing a "lost update" race condition.
- **Blast radius**: State corruption and loss of learned opponent card statistics over time.
- **Mitigation**: Use OS-level file locking (e.g., using `FileStream` with `FileShare.None`) during the entire read-merge-write sequence, rather than only during the individual read/write operations.

### [High] Challenge 2: Null Reference Risk on Empty/Corrupted JSON Files
- **Assumption challenged**: Configuration files are always populated with valid JSON.
- **Attack scenario**: If `cards_registry.json`, `card_names.json`, `precise_attack_locks.json`, or a deck-specific configuration file becomes empty (0 bytes) or corrupted, `JavaScriptSerializer.Deserialize` will return `null`. The code directly accesses the deserialized object in `foreach` loops (e.g., line 570, line 637, line 706, line 735) without checking for null.
- **Blast radius**: Complete crash of the bot process during initialization (`LoadConfiguration`).
- **Mitigation**: Add null checks immediately after deserialization (e.g., `if (rawList == null) rawList = new List<Dictionary<string, object>>();`).

### [Medium] Challenge 3: Unchecked Null References for `Util` and `Duel.Fields`
- **Assumption challenged**: Properties like `Util`, `Bot`, and `Enemy` are always non-null.
- **Attack scenario**:
  - In `CalculateCardDanger` (line 1377), `Util.GetLastChainCard()` is called without checking if `Util` is null (unlike `OnChaining` which guards with `if (Util != null)`).
  - In `IsLethalOnBoard` (line 280), `Duel.Phase` is checked, and `Bot` and `Enemy` are accessed. If the duel is cleaning up or not fully initialized, this throws a `NullReferenceException`.
- **Blast radius**: Unhandled crashes mid-duel, leading to abrupt disconnects.
- **Mitigation**: Implement strict null checking for `Util`, `Bot`, `Enemy`, and `Duel.Fields` inside all evaluation methods.

### [Medium] Challenge 4: Incomplete Nibiru Summon Count Safeguard
- **Assumption challenged**: The safeguard correctly verifies that the opponent summoned 5+ monsters.
- **Attack scenario**: The code comment states *"Nibiru, the Primal Being (ID: 27204311) — Only activate if opponent summoned 5+ monsters this turn"*, but the actual code only checks `if (Duel.Player == 0) return false;`. It does not verify the summon count. If the opponent has only summoned 1-4 monsters, the AI will still attempt to activate Nibiru.
- **Blast radius**: Illegal activation attempts, which are rejected by the duel engine, causing the AI to waste resources or crash/hang.
- **Mitigation**: Track the number of opponent summons per turn and block Nibiru if the count is less than 5.

### [Medium] Challenge 5: Incomplete Bystial GY Coverage
- **Assumption challenged**: Druiswurm and Magnamhut are the only Bystials requiring GY target validation.
- **Attack scenario**: Other Bystials (e.g., Baldrake, Saronir) have the same summon conditions requiring a LIGHT/DARK monster in either GY to banish. However, they are missing from the specific safeguard check (line 1653). The AI may attempt to activate them when GYs have no valid targets.
- **Blast radius**: Illegal card activations leading to runtime crashes.
- **Mitigation**: Add Saronir (ID: 98501258) and Baldrake (ID: 85034608) to the Bystial GY check.

### [Low] Challenge 6: Flawed PSY-Framegear Gamma Check
- **Assumption challenged**: Gamma can only respond to monster effects.
- **Attack scenario**: The safeguard checks: `if (lastChainCard == null || lastChainCard.Controller != 1 || !lastChainCard.IsMonster()) return false;`. This blocks Gamma if the chained card is a Spell/Trap card, despite Gamma's legal ability to negate Spell/Trap card activations.
- **Blast radius**: AI fails to negate critical opponent Spells/Traps.
- **Mitigation**: Relax the condition to allow negation of opponent's Spells and Traps if they meet Gamma's activation requirements.

### [Low] Challenge 7: Redundant Field Spell Protection and Array Index Risk
- **Assumption challenged**: `Bot.SpellZone` always contains 6+ elements, and duplicating field spells is always redundant.
- **Attack scenario**:
  - `Bot.SpellZone[5]` is accessed without verifying the array length, potentially causing an `IndexOutOfRangeException` if the zone is smaller.
  - Many Field Spells have search-on-activation effects. Overwriting a current field spell is a valid play to search another card, but the AI is penalized by `-500.0`, blocking this play.
- **Blast radius**: Suboptimal play for search-heavy decks and potential crashes.
- **Mitigation**: Verify `Bot.SpellZone.Length > 5` before indexing, and reduce the penalty if the field spell has search-on-activation roles.

### [Low] Challenge 8: Non-Thread-Safe Static `System.Random`
- **Assumption challenged**: Static `_random` is thread-safe for retry loops.
- **Attack scenario**: `System.Random` is not thread-safe. Concurrent calls to `_random.Next()` across multiple bot threads in `ReadFileWithRetry` and `WriteFileWithRetry` can corrupt the generator state.
- **Blast radius**: State corruption leading to 0 return values or infinite loops.
- **Mitigation**: Put `lock (_random)` around `_random.Next()` calls or use thread-local `Random` instances.

---

## Stress Test Results

- **Empty/corrupted opponent_memory.json on startup** → `LoadConfiguration` parses null dictionary → `foreach` over null → **FAIL** (Throws NullReferenceException and crashes).
- **Process exit during active gameplay thread writing to `_ourCardsPlayed`** → `StaticOnProcessExit` calls `ApplyRealTimeLearning` which iterates over `_ourCardsPlayed` on the exit thread while the gameplay thread modifies it → **FAIL** (Throws InvalidOperationException / collection modified).
- **Opponent summons 1 monster and activates a card; bot holds Nibiru** → Safeguard doesn't check summon count, proceeds to evaluate playability → **FAIL** (Attempts illegal play).
- **Opponent activates a Field Spell; bot controls no monsters and holds PSY-Framegear Gamma** → Safeguard blocks Gamma because the chained card is not a monster → **FAIL** (Blocks legal play).
- **Bot has 5 monsters on field and wants to Tribute Summon a Level 6 monster** → Safeguard rejects summon because `selfMonsters >= 5` → **FAIL** (Blocks legal play).

---

## Unchallenged Areas

- **Card priority formulas and Q-value calculations** — These are domain-specific heuristics and did not exhibit direct runtime safety risks.
