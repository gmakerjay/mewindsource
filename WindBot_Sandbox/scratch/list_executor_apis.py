import os
import subprocess

ps_script = """
$dllPath = 'C:\\Users\\admin\\Documents\\EDOTh\\WindBot\\ExecutorBase.dll'
[System.Reflection.Assembly]::LoadFile($dllPath)
$assembly = [System.Reflection.Assembly]::LoadFile($dllPath)

$outputFile = 'C:\\Users\\admin\\Documents\\EDOTh\\WindBot_Sandbox\\scratch\\executor_api_details.txt'
$sb = New-Object System.Text.StringBuilder

[void]$sb.AppendLine("================ TYPE DETAILS ================")

# Filter out only Executor and DefaultExecutor and AIUtil, etc.
foreach ($t in $assembly.GetTypes()) {
    if ($t.FullName -match "Executor" -or $t.FullName -match "GameAI" -or $t.FullName -match "ClientCard" -or $t.FullName -match "Duel") {
        [void]$sb.AppendLine("Type: $($t.FullName) (Base: $($t.BaseType.FullName))")
        
        [void]$sb.AppendLine("  --- Public Methods ---")
        foreach ($m in $t.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::DeclaredOnly)) {
            [void]$sb.AppendLine("    $($m.ReturnType.Name) $($m.Name) ($([System.String]::Join(', ', ($m.GetParameters() | % { "$($_.ParameterType.Name) $($_.Name)" }))))")
        }
        
        [void]$sb.AppendLine("  --- Virtual/Override Methods ---")
        foreach ($m in $t.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)) {
            if ($m.IsVirtual -and !$m.IsFinal) {
                [void]$sb.AppendLine("    Virtual: $($m.ReturnType.Name) $($m.Name) ($([System.String]::Join(', ', ($m.GetParameters() | % { "$($_.ParameterType.Name) $($_.Name)" }))))")
            }
        }
        
        [void]$sb.AppendLine("  --- Properties ---")
        foreach ($p in $t.GetProperties()) {
            [void]$sb.AppendLine("    Prop: $($p.PropertyType.Name) $($p.Name)")
        }
        [void]$sb.AppendLine("================================================")
        [void]$sb.AppendLine("")
    }
}

[System.IO.File]::WriteAllText($outputFile, $sb.ToString())
Write-Host "Output written to $outputFile"
"""

ps_file = "C:\\Users\\admin\\Documents\\EDOTh\\WindBot_Sandbox\\scratch\\list_executor_apis.ps1"
with open(ps_file, "w", encoding="utf-8") as f:
    f.write(ps_script)

res = subprocess.run(["powershell", "-ExecutionPolicy", "Bypass", "-File", ps_file], capture_output=True, text=True)
print(res.stdout)

if os.path.exists(ps_file):
    os.remove(ps_file)
