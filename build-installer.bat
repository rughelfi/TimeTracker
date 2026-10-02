@echo off
setlocal
echo Starting TimeTracker Installer Build...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Installer\build-installer.ps1" %*
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed!
    pause
    exit /b %ERRORLEVEL%
)
echo.
echo [DONE] Installer built successfully!
pause
