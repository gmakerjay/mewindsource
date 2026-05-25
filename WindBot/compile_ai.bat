@echo off
cd /d "%~dp0"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library /r:System.Web.Extensions.dll /r:ExecutorBase.dll /out:Executors\UnifiedIgnisExecutor.dll BaseCustomExecutor.cs UnifiedIgnisExecutor.cs PureYummyExecutor.cs InvokeExecutor.cs DreadnoughtExecutor.cs
if %errorlevel% neq 0 (
    echo Compilation FAILED!
    exit /b %errorlevel%
)
echo Compilation SUCCESSFUL!
