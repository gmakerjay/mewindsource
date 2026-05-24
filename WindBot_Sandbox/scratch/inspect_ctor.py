import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$type = $assembly.GetType("WindBot.Game.BattlePhaseAction")
foreach ($ctor in $type.GetConstructors()) {
    Write-Host "Ctor: $($ctor.ToString())"
    foreach ($param in $ctor.GetParameters()) {
        Write-Host "  Param: $($param.ParameterType.FullName) $($param.Name)"
    }
}
$methods = $type.GetMethods()
foreach ($m in $methods) {
    if ($m.Name -eq "ToValue") {
        Write-Host "Method: $($m.ToString())"
    }
}
"""

with open("inspect_ctor.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "inspect_ctor.ps1"], capture_output=True, text=True)
print(res.stdout)

if os.path.exists("inspect_ctor.ps1"):
    os.remove("inspect_ctor.ps1")
