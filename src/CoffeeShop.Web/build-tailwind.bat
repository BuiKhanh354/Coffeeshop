@echo off
cd /d "%~dp0"
call npx tailwindcss -i wwwroot/css/tailwind.css -o wwwroot/css/output.css
echo Done!
pause
