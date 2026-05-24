import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$type = $assembly.GetType("WindBot.Game.AI.DefaultExecutor")
$method = $type.GetMethod("OnSelectAttackTarget", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)
$module = $method.Module

# Let's resolve token 167772231 (0x0a000047)
$token = 167772231
try {
    $member = $module.ResolveMember($token)
    Write-Host "Resolved member: $($member.DeclaringType.FullName).$($member.Name)"
} catch {
    Write-Host "Failed: $($_.Exception.Message)"
}
"""

with open("test_resolve.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "test_resolve.ps1"], capture_output=True, text=True)
print(res.stdout)

if os.path.exists("test_resolve.ps1"):
    os.remove("test_resolve.ps1")
