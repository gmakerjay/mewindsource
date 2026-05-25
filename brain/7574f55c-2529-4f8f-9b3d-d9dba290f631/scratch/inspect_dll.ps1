$assemblyPath = "c:\Users\admin\Documents\EDOTh\WindBot\ExecutorBase.dll"
$assembly = [System.Reflection.Assembly]::LoadFrom($assemblyPath)
$type = $assembly.GetTypes() | Where-Object { $_.Name -eq "Executor" }
if ($type) {
    $methods = $type.GetMethods()
    foreach ($m in $methods) {
        $params = $m.GetParameters() | ForEach-Object { $_.ParameterType.ToString() + " " + $_.Name }
        $paramsStr = [string]::Join(", ", $params)
        Write-Output "$($m.ReturnType.ToString()) $($m.Name)($paramsStr)"
    }
} else {
    Write-Output "Type Executor not found!"
}
