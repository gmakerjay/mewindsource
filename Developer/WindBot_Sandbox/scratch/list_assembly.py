import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\Mono.Data.Sqlite.dll')

$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
foreach ($type in $assembly.GetTypes()) {
    if ($type.Name -eq "Executor" -or $type.Name -eq "DefaultExecutor") {
        Write-Host "Type: $($type.FullName)"
        foreach ($method in $type.GetMethods()) {
            if ($method.IsVirtual -and $method.DeclaringType.FullName -eq $type.FullName) {
                Write-Host "  Virtual Method: $($method.ToString())"
            }
        }
    }
}
"""

with open("run_ps.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "run_ps.ps1"], capture_output=True, text=True)
print("STDOUT:")
print(res.stdout)

# Cleanup
if os.path.exists("run_ps.ps1"):
    os.remove("run_ps.ps1")
