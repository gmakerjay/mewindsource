import subprocess

res = subprocess.run(["git", "show", "2d0d2cd", "--", "WindBot/UnifiedIgnisExecutor.cs"], capture_output=True, text=True)
diff = res.stdout

# Look for deleted lines related to Battle Phase override methods
for line in diff.splitlines():
    if line.startswith("-") and not line.startswith("---"):
        if any(keyword in line for keyword in ["OnBattle", "OnSelectAttackTarget", "BattlePhaseAction", "AttackTarget", "BattleTrap", "HandTrap"]):
            print(line)
