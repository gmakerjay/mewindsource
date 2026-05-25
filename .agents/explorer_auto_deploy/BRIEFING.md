# BRIEFING — 2026-05-25T21:08:00+07:00

## Mission
Analyze training system compilation and deployment flow in cockpit.py and C# WindBot shutdown hooks, proposing headless auto-sync and compilation.

## 🔒 My Identity
- Archetype: Teamwork explorer
- Roles: Read-only investigator
- Working directory: c:\Users\admin\Documents\EDOTh\.agents\explorer_auto_deploy\
- Original parent: e07b25b1-018f-4ee8-88c1-50de17279a3f
- Milestone: Auto-deploy and shutdown analysis

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- CODE_ONLY network mode (no external network, curl, wget, etc.)

## Current Parent
- Conversation ID: e07b25b1-018f-4ee8-88c1-50de17279a3f
- Updated: 2026-05-25T21:08:00+07:00

## Investigation State
- **Explored paths**:
  - `c:\Users\admin\Documents\EDOTh\WindBot\BaseCustomExecutor.cs` - Core learning and config management
  - `c:\Users\admin\Documents\EDOTh\Developer\BrainStorms\windbot-master\Game\GameBehavior.cs` - Packet handling and match termination
  - `c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sandbox\cockpit.py` - Training and process management UI/API
  - `c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sandbox\shared_utils.py` - Path config and registry helpers
- **Key findings**:
  - C# WindBot has a background thread `MonitorLP` in `BaseCustomExecutor` that detects when LP hits 0 and triggers `ApplyRealTimeLearning()`.
  - `ApplyRealTimeLearning()` saves the configuration.
  - `SaveConfiguration()` calls `SyncRegistryToSandboxAndCompile()` if `target_lp` is 0.
  - Recompiling `UnifiedIgnisExecutor.dll` while the bots are running causes file lock sharing violations in Windows.
  - Proposed two approaches to headless auto-deploy: C# triggered (delayed detached process) and Python/Cockpit triggered (after-process-exit hook).
- **Unexplored areas**:
  - None. Both C# and python codebases have been fully analyzed.

## Key Decisions Made
- Offer both C# (standalone) and Python (Cockpit-specific) integration strategies to give complete architectural freedom.

## Artifact Index
- c:\Users\admin\Documents\EDOTh\.agents\explorer_auto_deploy\original_prompt.md — Original instructions for this subagent
- c:\Users\admin\Documents\EDOTh\.agents\explorer_auto_deploy\BRIEFING.md — Persistent briefing and status
- c:\Users\admin\Documents\EDOTh\.agents\explorer_auto_deploy\progress.md — Heartbeat progress log
