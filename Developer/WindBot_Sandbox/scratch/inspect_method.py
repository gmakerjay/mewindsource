import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$type = $assembly.GetType("WindBot.Game.AI.DefaultExecutor")
$method = $type.GetMethod("OnSelectAttackTarget", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)

if ($method -ne $null) {
    Write-Host "Method found: $($method.ToString())"
    $body = $method.GetMethodBody()
    $bytes = $body.GetILAsByteArray()
    Write-Host "IL Bytes count: $($bytes.Length)"
    Write-Host "Bytes: $([System.String]::Join(', ', $bytes))"
} else {
    Write-Host "Method not found"
}
"""

with open("inspect_method.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "inspect_method.ps1"], capture_output=True, text=True)
print(res.stdout)

if os.path.exists("inspect_method.ps1"):
    os.remove("inspect_method.ps1")
