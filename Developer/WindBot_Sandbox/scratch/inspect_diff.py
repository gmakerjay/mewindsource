import subprocess
import re

res = subprocess.run(["git", "show", "2d0d2cd", "--", "WindBot/UnifiedIgnisExecutor.cs"], capture_output=True, text=True)
diff = res.stdout

# Print lines that were deleted (starting with '-') that are related to Battle Phase
deleted_lines = []
in_deleted_block = False
for line in diff.splitlines():
    if line.startswith("---") or line.startswith("+++"):
        continue
    if line.startswith("-"):
        deleted_lines.append(line)

print("\\n".join(deleted_lines[:150]))
