@echo off
echo กำลังเปิดระบบ Cockpit ควบคุมระบบ IgnisEngine...
cd /d "%~dp0"
start http://localhost:8000
python WindBot_Sandbox\cockpit.py
pause
