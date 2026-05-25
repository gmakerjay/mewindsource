import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
foreach ($type in $assembly.GetTypes()) {
    if ($type.FullName -like "*BattlePhaseAction*" -or $type.FullName -like "*BattleAction*") {
        Write-Host "Type: $($type.FullName)"
        foreach ($constructor in $type.GetConstructors()) {
            Write-Host "  Constructor: $($constructor.ToString())"
        }
        foreach ($method in $type.GetMethods()) {
            Write-Host "  Method: $($method.ToString())"
        }
        foreach ($field in $type.GetFields()) {
            Write-Host "  Field: $($field.ToString())"
        }
        foreach ($prop in $type.GetProperties()) {
            Write-Host "  Property: $($prop.ToString())"
        }
    }
}
"""

with open("inspect_bp.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "inspect_bp.ps1"], capture_output=True, text=True)
print(res.stdout)

if os.path.exists("inspect_bp.ps1"):
    os.remove("inspect_bp.ps1")
