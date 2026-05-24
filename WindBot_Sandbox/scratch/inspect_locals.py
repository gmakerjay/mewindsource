import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$type = $assembly.GetType("WindBot.Game.AI.DefaultExecutor")
$method = $type.GetMethod("OnSelectAttackTarget", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)

if ($method -ne $null) {
    $body = $method.GetMethodBody()
    Write-Host "Local variables:"
    foreach ($local in $body.LocalVariables) {
        Write-Host "  $($local.LocalType.FullName) (Index: $($local.LocalIndex))"
    }
}
"""

with open("inspect_locals.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "inspect_locals.ps1"], capture_output=True, text=True)
print(res.stdout)

if os.path.exists("inspect_locals.ps1"):
    os.remove("inspect_locals.ps1")
