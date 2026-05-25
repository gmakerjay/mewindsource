import os
import subprocess

ps_script = """
[System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$assembly = [System.Reflection.Assembly]::LoadFile('C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll')
$type = $assembly.GetType("WindBot.Game.AI.DefaultExecutor")
$method = $type.GetMethod("OnSelectAttackTarget", [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)
$module = $method.Module
$body = $method.GetMethodBody()
$il = $body.GetILAsByteArray()

# Define opcodes
$OPCODES = @{
    0x00 = "nop"
    0x02 = "ldarg.0"
    0x03 = "ldarg.1"
    0x04 = "ldarg.2"
    0x05 = "ldarg.3"
    0x06 = "ldloc.0"
    0x07 = "ldloc.1"
    0x08 = "ldloc.2"
    0x09 = "ldloc.3"
    0x0a = "stloc.0"
    0x0b = "stloc.1"
    0x0c = "stloc.2"
    0x0d = "stloc.3"
    0x14 = "ldnull"
    0x16 = "ldc.i4.0"
    0x17 = "ldc.i4.1"
    0x18 = "ldc.i4.2"
    0x19 = "ldc.i4.3"
    0x1f = "ldc.i4.s"
    0x20 = "ldc.i4"
    0x25 = "dup"
    0x26 = "pop"
    0x2a = "ret"
    0x2b = "br.s"
    0x2c = "brfalse.s"
    0x2d = "brtrue.s"
    0x2e = "beq.s"
    0x2f = "bge.s"
    0x30 = "bgt.s"
    0x31 = "ble.s"
    0x32 = "blt.s"
    0x3b = "ldc.i4.8"
    0x28 = "call"
    0x6f = "callvirt"
    0x73 = "newobj"
    0x7b = "ldfld"
    0x7d = "stfld"
    0x8d = "ldlen"
    0x8e = "ldelem.i4"
    0xa2 = "stelem.i4"
    0xde = "castclass"
}

$i = 0
while ($i -lt $il.Length) {
    $op = $il[$i]
    $opName = if ($OPCODES.ContainsKey($op)) { $OPCODES[$op] } else { "0x" + $op.ToString("X2") }
    
    if ($op -eq 0x28 -or $op -eq 0x6f -or $op -eq 0x73) { # call, callvirt, newobj
        $token = [int]$il[$i+1] -bor ([int]$il[$i+2] -shl 8) -bor ([int]$il[$i+3] -shl 16) -bor ([int]$il[$i+4] -shl 24)
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
        Write-Host "$($i.ToString('D4')): $($opName) $($resolved)"
        $i += 5
    } elseif ($op -eq 0x7b -or $op -eq 0x7d) { # ldfld, stfld
        $token = [int]$il[$i+1] -bor ([int]$il[$i+2] -shl 8) -bor ([int]$il[$i+3] -shl 16) -bor ([int]$il[$i+4] -shl 24)
        $resolved = "unknown"
        try {
            $member = $module.ResolveField($token)
            $resolved = "$($member.DeclaringType.FullName).$($member.Name)"
        } catch {}
        Write-Host "$($i.ToString('D4')): $($opName) $($resolved)"
        $i += 5
    } elseif ($op -eq 0x2b -or $op -eq 0x2c -or $op -eq 0x2d -or $op -eq 0x2e -or $op -eq 0x2f -or $op -eq 0x30 -or $op -eq 0x31 -or $op -eq 0x32 -or $op -eq 0x1f) { # branch.s / ldc.i4.s
        $val = [int]$il[$i+1]
        if ($val -gt 127) { $val -= 256 }
        $target = $i + 2 + $val
        Write-Host "$($i.ToString('D4')): $($opName) target: $($target.ToString('D4')) (offset: $($val))"
        $i += 2
    } else {
        Write-Host "$($i.ToString('D4')): $($opName)"
        $i += 1
    }
}
"""

with open("resolve_all.ps1", "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", "resolve_all.ps1"], capture_output=True, text=True)
print(res.stdout)

if os.path.exists("resolve_all.ps1"):
    os.remove("resolve_all.ps1")
