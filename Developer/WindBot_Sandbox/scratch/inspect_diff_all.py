import subprocess

res = subprocess.run(["git", "show", "2d0d2cd", "--", "WindBot/UnifiedIgnisExecutor.cs"], capture_output=True, text=True)
diff = res.stdout

for line in diff.splitlines():
    if line.startswith("-") and not line.startswith("---"):
        print(line)
