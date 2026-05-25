## 2026-05-25T21:03:38Z
Analyze the training system compilation and deployment flow in c:\Users\admin\Documents\EDOTh\Developer\WindBot_Sandbox\cockpit.py and C# WindBot shutdown hooks.
Tasks:
1. Locate how WindBot handles shutdown or game end in C#. Look for hooks that detect when a match finishes (e.g. LP of either side reaches 0).
2. Look at how cockpit.py launches processes, handles registry syncing between the live directory (WindBot/config/) and sandbox directory (WindBot_Sandbox/), and how it triggers compile_ai.bat to rebuild UnifiedIgnisExecutor.dll.
3. Propose how to integrate these so that once a match finishes and LP reaches 0 on either side, the system automatically syncs JSON registries and recompiles UnifiedIgnisExecutor.dll headlessly without human intervention.
Write your analysis and proposed code changes to handoff.md in your working directory (.agents/explorer_auto_deploy/).
