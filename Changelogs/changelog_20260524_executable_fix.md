# Changelog - WindBot Fixes (Startup Crash & AI Decision Upgrades)

**Timestamp**: 2026-05-24T10:30:00+07:00  
**Author**: Antigravity AI  

---

## 1. Critical Startup Crash Resolved

- **Issue**: Selecting any deck/bot caused WindBot to fail to start and throw the following exception:
  ```
  Unhandled Exception: System.Exception: Invalid argument '<DeckName>': no key/value separator
     at WindBot.Config.LoadArgs(String[] args)
     at WindBot.Config.Load(String[] args)
     at WindBot.Program.Main(String[] args)
  ```
- **Root Cause**: The updated `WindBot.exe` (1.43 MB) modified during the morning update had a command line parsing bug. It split the command line string by spaces regardless of quotes, causing `"name=[AI] 2026_Goldlord"` (which has a space) to be parsed as two arguments: `name=[AI]` and `2026_Goldlord`. The latter had no `=` separator, throwing the exception.
- **Resolution**: Restored the original working `WindBot.exe` (733 KB) from the cached root file (`._cache_WindBot.exe`) into `WindBot/WindBot.exe`. We verified that it correctly parses arguments containing spaces and boots up successfully.

---

## 2. AI Stupidity / Decision Logic Upgrades

We resolved two critical logic flaws in `WindBot/UnifiedIgnisExecutor.cs` where key disruption cards were unnecessarily blocked from activating on our own turn:

- **Handtrap Reaction Logic Fix**:
  * *Bug*: A global safeguard blocked all disruptive handtraps (like `Ash Blossom & Joyous Spring`) on our own turn. This prevented the bot from chaining `Ash Blossom` to negate opponent handtraps (like `Maxx "C"`) activated during our turn.
  * *Fix*: Modified the check so that handtraps are only blocked if the bot is attempting to initiate a chain (Chain Link 1). If the bot is reacting to an opponent's card activation (`lastChainCard.Controller == 1`), the handtrap activation is permitted.
- **Infinite Impermanence Activation Fix**:
  * *Bug*: `Infinite Impermanence` (ID: `10045474`) was blocked completely on our own turn. This made it impossible to use Impermanence to negate opponent monsters (such as continuous floodgates/interruption boards) when going second.
  * *Fix*: Removed the `Duel.Player == 0` check so that Impermanence can be activated normally on our turn (e.g. from hand if we control no cards, or if set in the S/T zone). The existing safeguard still correctly ensures there is a face-up opponent monster to target.

---

## 3. Configuration & Workspace Cleanup

- **bots.json**: Reverted temporary modifications in `WindBot/bots.json` to keep the custom `"dialog"` and `"description"` fields since the restored executable handles names with spaces successfully.
- **Junk/Temp Files Cleaned**:
  - Removed `WindBot/log_args.cs` (temporary logging wrapper)
  - Removed `WindBot/log_args.exe` (compiled logging wrapper)
  - Removed `WindBot/WindBot_Real.exe` (buggy updated version backup)
  - Removed `WindBot/WindBot_Test.exe` (temporary test executable)
  - Removed `WindBot/args_log.txt` (temporary arguments log)

---

## 4. Verification

- Verified that the restored `WindBot.exe` starts up and initializes decks correctly.
- Verified that `UnifiedIgnisExecutor.cs` compiles cleanly using `compile_ai.bat`.
- Verified that `git status` is clean of temporary untracked files.
