# Changelog - WindBot Executable Restoration & Startup Crash Fix

**Timestamp**: 2026-05-24T10:25:00+07:00  
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

## 2. Configuration & Workspace Cleanup

- **bots.json**: Reverted temporary modifications in `WindBot/bots.json` to keep the custom `"dialog"` and `"description"` fields since the restored executable handles names with spaces successfully.
- **Junk/Temp Files Cleaned**:
  - Removed `WindBot/log_args.cs` (temporary logging wrapper)
  - Removed `WindBot/log_args.exe` (compiled logging wrapper)
  - Removed `WindBot/WindBot_Real.exe` (buggy updated version backup)
  - Removed `WindBot/WindBot_Test.exe` (temporary test executable)
  - Removed `WindBot/args_log.txt` (temporary arguments log)

---

## 3. Verification

- Verified that the restored `WindBot.exe` starts up and initializes decks correctly.
- Verified that `git status` is clean of temporary untracked files.
