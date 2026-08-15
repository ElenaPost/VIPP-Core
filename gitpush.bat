@echo off
set /p msg=Enter message:
git status
git add .
git commit -m "%msg%"
git push origin main
pause
