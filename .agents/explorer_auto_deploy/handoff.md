# Handoff Report — explorer_auto_deploy

## 1. Observation

Direct observations made in the workspace `c:\Users\admin\Documents\EDOTh`:

### C# WindBot Game-End and Shutdown Hooks
1. **Game Client Disconnection:** In `Developer/BrainStorms/windbot-master/Game/GameBehavior.cs` at lines 288-292, receipt of the `DuelEnd` packet triggers client disconnection:
   ```csharp
   private void OnDuelEnd(BinaryReader packet)
   {
       Thread.Sleep(500);
       Connection.Close();
   }
   ```
   Closing the connection exits the main client thread loop in `Program.cs` (lines 175-192), allowing the bot process to terminate.

2. **LP-0 Detection (Background Monitor):** In `WindBot/BaseCustomExecutor.cs` at lines 194-222, a background thread monitors LP and triggers real-time learning as soon as either side reaches 0 LP:
   ```csharp
   protected void MonitorLP()
   {
       while (!_stopLPMonitor)
       {
           try
           {
               if (Duel != null && Duel.Fields != null && Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
               {
                   int botLP = Duel.Fields[0].LifePoints;
                   int oppLP = Duel.Fields[1].LifePoints;
                   
                   if (_turnCount > 0 && (botLP == 0 || oppLP == 0))
                   {
                       ApplyRealTimeLearning();
                   }
                   ...
   ```

3. **LP-0 Detection (Chain End):** In `WindBot/BaseCustomExecutor.cs` at lines 3444-3450, `OnChainEnd` checks LP and applies learning:
   ```csharp
   if (Duel.Fields.Length >= 2 && Duel.Fields[0] != null && Duel.Fields[1] != null)
   {
       if (Duel.Fields[0].LifePoints == 0 || Duel.Fields[1].LifePoints == 0)
       {
           ApplyRealTimeLearning();
       }
   }
   ```

4. **Trigger for Sync and Recompilation:** `ApplyRealTimeLearning()` invokes `SaveConfiguration()` (line 1334). Within `SaveConfiguration()` (lines 1048-1052), syncing/compiling is gated behind target LP checking:
   ```csharp
   if (targetLpIsZero || _deckConfig.target_lp == 0)
   {
       LogToMatch("Target LP is 0. Training concluded. Syncing registry and compiling brain...");
       SyncRegistryToSandboxAndCompile();
   }
   ```

### cockpit.py Process Spawning and Registry Syncing
1. **Bot Launching:** `cockpit.py` spawns bot processes using `subprocess.Popen` in `spawn_bots_on_port` (lines 49-63) and `run_live_duel_loop` (lines 184-199).
2. **Registry Syncing (Sandbox -> Live):** `deploy_config(deck)` (lines 868-874) copies settings from Sandbox to Live config:
   ```python
   sandbox_reg = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", reg_file)
   live_reg = os.path.join(PROJECT_ROOT, "WindBot", "config", reg_file)
   if os.path.exists(sandbox_reg):
       shutil.copy2(sandbox_reg, live_reg)
   ```
3. **Triggering Compilation:** `deploy_config` triggers compilation (lines 880-890) by running `compile_ai.bat`:
   ```python
   compile_bat = os.path.join(PROJECT_ROOT, "WindBot", "compile_ai.bat")
   if os.path.exists(compile_bat):
       res = subprocess.run(
           [compile_bat],
           cwd=os.path.join(PROJECT_ROOT, "WindBot"),
           ...
       )
   ```

---

## 2. Logic Chain

1. When a match finishes by either side reaching 0 LP, `MonitorLP` (line 205) or `OnChainEnd` (line 3446) triggers `ApplyRealTimeLearning()`.
2. `ApplyRealTimeLearning()` updates the weights of the cards and writes the new registry values to `WindBot/config/cards_registry_{deck}.json` via `SaveConfiguration()` (line 1334).
3. If we want this to automatically sync the updated weights back to the sandbox directory (`WindBot_Sandbox/`) and recompile the assembly DLL (`UnifiedIgnisExecutor.dll`) without human intervention, we must remove the `target_lp == 0` constraint.
4. **The File-Locking Caveat:** Recompiling `UnifiedIgnisExecutor.dll` while `WindBot.exe` is still running is impossible because Windows locks the loaded assembly.
5. Therefore, we can implement two alternative solutions:
   - **Option A (C# Standalone):** Let C# detect LP reaching 0, write the configuration, and spawn a detached/delayed batch compile job using `cmd.exe /c timeout /t 2 /nobreak && compile_ai.bat` which waits for the `WindBot` process to close and unlock the DLL before executing the csc compiler.
   - **Option B (Python / Cockpit-Driven):** Let Cockpit's `run_live_duel_loop` monitor the subprocesses. When they exit naturally, Cockpit copies `cards_registry_{deck}.json` and `opponent_memory.json` from the live config to the sandbox directory, then triggers `compile_ai.bat`. This guarantees no file locks because the processes have already exited.

---

## 3. Caveats

- **Windows File Locks:** Recompiling `UnifiedIgnisExecutor.dll` immediately inside the active C# thread will fail due to write restrictions on locked files. Detaching/delaying the execution is required.
- **Decks Scope:** The proposed changes assume registry filenames match the format `cards_registry_{deck}.json` or default to `cards_registry.json`.

---

## 4. Conclusion

Integrating headless auto-deployment is highly feasible and can be done either from the C# executor itself or the Python cockpit manager. Option B (Python side) is the most robust since it completely side-steps Windows file lock issues by running post-termination, while Option A is best if WindBot needs to compile independently of the Python UI.

### Proposed Code Changes (Option A: C# Standalone)

1. Modify `SaveConfiguration` signature in `WindBot/BaseCustomExecutor.cs` to receive optional LPs:
```csharp
// Change line 842 of WindBot/BaseCustomExecutor.cs
protected void SaveConfiguration(int botLP = -1, int oppLP = -1)
```

2. Inside `ApplyRealTimeLearning()` (line 1334), pass `botLP` and `oppLP`:
```csharp
// Before:
SaveConfiguration();
// After:
SaveConfiguration(botLP, oppLP);
```

3. Update the execution check and launch a delayed compilation inside `SaveConfiguration()`:
```csharp
// Modify lines 1048-1052 of WindBot/BaseCustomExecutor.cs:
if (targetLpIsZero || _deckConfig.target_lp == 0 || botLP == 0 || oppLP == 0)
{
    LogToMatch(string.Format("Triggering headless sync & compile (Bot LP: {0}, Opp LP: {1})", botLP, oppLP));
    SyncRegistryToSandboxAndCompile();
}
```

4. Modify `SyncRegistryToSandboxAndCompile()` to execute delayed and redirect outputs:
```csharp
// Replace compile part (lines 1117-1139 of WindBot/BaseCustomExecutor.cs):
string batPath = Path.Combine(windBotDir, "compile_ai.bat");
if (File.Exists(batPath))
{
    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
    psi.FileName = "cmd.exe";
    // Wait 2 seconds for process to exit and unlock DLL, then compile and log output
    psi.Arguments = "/c timeout /t 2 /nobreak && compile_ai.bat > compile.log 2>&1";
    psi.WorkingDirectory = windBotDir;
    psi.CreateNoWindow = true;
    psi.UseShellExecute = false;
    
    System.Diagnostics.Process.Start(psi);
    Log("Headless detached compilation scheduled. Rebuild will occur in 2 seconds after process exit.");
}
```

### Proposed Code Changes (Option B: Python / Cockpit-Driven)

Modify `run_live_duel_loop` in `Developer/WindBot_Sandbox/cockpit.py` to auto-sync and deploy once bot processes exit:

```python
# Insert after loop ends in cockpit.py run_live_duel_loop (line 229)
            if not active_bots:
                break
                
            time.sleep(1.0)
        except Exception as e:
            write_progress_log(progress_log, f"เกิดข้อผิดพลาดในการรันรอบที่ {i}: {str(e)}\n")
            break
            
    # --- AUTO-SYNC AND RECOMPILE SECTION ---
    if deck and deck != "all":
        reg_file = f"cards_registry_{deck}.json"
    else:
        reg_file = "cards_registry.json"
        
    live_reg = os.path.join(PROJECT_ROOT, "WindBot", "config", reg_file)
    sandbox_reg = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", reg_file)
    live_memory = os.path.join(PROJECT_ROOT, "WindBot", "config", "opponent_memory.json")
    sandbox_memory = os.path.join(PROJECT_ROOT, "WindBot_Sandbox", "opponent_memory.json")
    
    # Sync weights back to Sandbox from Live config
    if os.path.exists(live_reg):
        try:
            import shutil
            shutil.copy2(live_reg, sandbox_reg)
            write_progress_log(progress_log, f"ระบบ: ซิงค์น้ำหนักการ์ดล่าสุดกลับมายัง Sandbox ({reg_file})\n")
        except Exception as e:
            write_progress_log(progress_log, f"ข้อผิดพลาดในการซิงค์น้ำหนัก: {e}\n")
            
    # Sync opponent memory back to Sandbox from Live config
    if os.path.exists(live_memory):
        try:
            import shutil
            shutil.copy2(live_memory, sandbox_memory)
            write_progress_log(progress_log, "ระบบ: ซิงค์หน่วยความจำคู่แข่งล่าสุดกลับมายัง Sandbox\n")
        except Exception as e:
            write_progress_log(progress_log, f"ข้อผิดพลาดในการซิงค์หน่วยความจำ: {e}\n")

    # Headless Recompilation
    compile_bat = os.path.join(PROJECT_ROOT, "WindBot", "compile_ai.bat")
    if os.path.exists(compile_bat):
        try:
            write_progress_log(progress_log, "ระบบ: กำลังเริ่มต้นคอมไพล์ UnifiedIgnisExecutor.dll Headlessly...\n")
            res = subprocess.run(
                [compile_bat],
                cwd=os.path.join(PROJECT_ROOT, "WindBot"),
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                shell=True
            )
            output = res.stdout.decode('utf-8', errors='replace')
            if res.returncode == 0:
                write_progress_log(progress_log, "ระบบ: คอมไพล์ UnifiedIgnisExecutor.dll สำเร็จ!\n")
            else:
                write_progress_log(progress_log, f"ระบบ: คอมไพล์ผิดพลาด!\n{output}\n")
        except Exception as e:
            write_progress_log(progress_log, f"ระบบ: ข้อผิดพลาดระหว่างสั่งคอมไพล์: {e}\n")
            
    active_bots = []
```

---

## 5. Verification Method

To verify the integration:
1. Run a match using `cockpit.py` or command line, ensuring the bot wins or loses (LP of either side reaches 0).
2. For Option A: Verify that after the bot closes, a `compile.log` is generated in the `WindBot` directory, containing `Compilation SUCCESSFUL!`. Also verify `WindBot_Sandbox/cards_registry_{deck}.json` has the updated weights.
3. For Option B: Verify that the output logs of the cockpit training log `training_progress.log` show the compilation messages and results.
4. Verify that the file modification timestamp of `WindBot/Executors/UnifiedIgnisExecutor.dll` has updated.
