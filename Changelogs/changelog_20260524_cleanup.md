# Changelog - Junk File Cleanup & Refactor

**Timestamp**: 2026-05-24T00:02:19+07:00  
**Author**: Antigravity AI  

---

## 1. Junk Files & Directories Deleted

The following unused files, temporary logs, test scripts, and old match duels were successfully cleaned up from the workspace:

### Workspace Root (`EDOTh/`)
- `error.log` (5.6 MB engine log file)
- `reflect.exe` (776 KB temporary executable)
- `crashdumps/EDOPro-pid17308-1954703.mdmp` (1.3 MB client dump file)

### WindBot Directory (`EDOTh/WindBot/`)
- `bot1.log` (bot execution log)
- `bot1_err.log` (empty error log)
- `bot2.log` (bot execution log)
- `bot2_err.log` (empty error log)
- `help_output.txt` (command CLI help output)
- `run_test_output.txt` (test match run logs)
- `run_test_output2.txt` (test match run logs)
- `run_test_output3.txt` (test match run logs)
- `run_test_output4.txt` (test match run logs)
- `run_test_output5.txt` (test match run logs)
- `config/cards_registry_2026_Kwtune.json.bak` (redundant backup config)
- `Logs/2026_Invoke_20260523_234052_4f327733` (old match directory)
- `Logs/2026_Kwtune_20260523_234045_04fc7201` (old match directory)

### Sandbox Directory (`EDOTh/WindBot_Sandbox/`)
- `bot_proxy_log.txt` (bot server communication log)
- `run_test_run_output.txt` (test training execution output)
- `card_details.txt` (card dump log from texts DB query)
- `query_deck_ids.py` (unused exploration database query script)
- `query_db.py` (unused exploration database query script)
- `reflect.cs` (unused reflection script)
- `reflect.exe` (unused reflection executable)

---

## 2. Refactor Status

- Verified that the refactored card selection priority sorting logic in [UnifiedIgnisExecutor.cs](file:///c:/Users/admin/Documents/EDOTh/WindBot/UnifiedIgnisExecutor.cs) compiles cleanly using `compile_ai.bat`.
- The sandbox registry weights for the `2026_Kwtune` deck have been optimized and validated using the combo simulator.
