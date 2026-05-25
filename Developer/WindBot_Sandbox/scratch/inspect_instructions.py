import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')

# A helper to resolve token to MemberInfo
$type = $assembly.GetType("WindBot.Game.AI.DefaultExecutor")
$method = $type.GetMethod("OnSelectAttackTarget", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)

$module = $method.Module
$body = $method.GetMethodBody()
$il = $body.GetILAsByteArray()

# Simple IL parser to find all call/callvirt/newobj tokens
$i = 0
while ($i -lt $il.Length) {
    $opByte = $il[$i]
    $opName = "0x" + $opByte.ToString("X2")
    
    # We can map standard opcodes here
    if ($opByte -eq 0x28 -or $opByte -eq 0x6f -or $opByte -eq 0x73) { # call (0x28), callvirt (0x6F), newobj (0x73)
        $token = $il[$i+1] -bor ($il[$i+2] -shl 8) -bor ($il[$i+3] -shl 16) -bor ($il[$i+4] -shl 24)
        $resolved = "unknown"
        try {
            $member = $module.ResolveMember($token)
            $resolved = "$($member.DeclaringType.FullName).$($member.Name)"
        } catch {
            try {
                $member = $module.ResolveMethod($token)
                $resolved = "$($member.DeclaringType.FullName).$($member.Name)"
            } catch {}
        }
        Write-Host "$($i): $($opByte.ToString('X2')) ($resolved) token: $($token)"
        $i += 5
    } elseif ($opByte -eq 0x7b -or $opByte -eq 0x7d) { # ldfld (0x7B), stfld (0x7D)
        $token = $il[$i+1] -bor ($il[$i+2] -shl 8) -bor ($il[$i+3] -shl 16) -bor ($il[$i+4] -shl 24)
        $resolved = "unknown"
        try {
            $member = $module.ResolveField($token)
            $resolved = "$($member.DeclaringType.FullName).$($member.Name)"
        } catch {}
        Write-Host "$($i): $($opByte.ToString('X2')) field: $($resolved)"
        $i += 5
    } else {
        $i += 1
    }
}
"""

with open("inspect_instructions.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "inspect_instructions.ps1"], capture_output=True, text=True)
print(res.stdout)

if os.path.exists("inspect_instructions.ps1"):
    os.remove("inspect_instructions.ps1")
