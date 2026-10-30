@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title KAOSDUO REFORGED - Startup Diagnostics
set "GAME=%CD%\KAOSDUO.exe"
if not exist "%GAME%" set "GAME=%CD%\dist\win-x64\KAOSDUO.exe"
if not exist "%GAME%" (
  echo ERROR: Could not find KAOSDUO.exe.
  echo Place this .bat next to the EXE, or run BUILD_WINDOWS.bat first.
  pause
  exit /b 1
)
echo Starting "%GAME%"
echo If the game crashes, its error report is located at:
echo %LOCALAPPDATA%\KAOSDUO\logs\startup.log
echo.
"%GAME%"
set "RESULT=%ERRORLEVEL%"
echo.
echo Game exited with code: %RESULT%
if exist "%LOCALAPPDATA%\KAOSDUO\logs\startup.log" (
  echo An error log was found. Showing latest information:
  powershell.exe -NoProfile -Command "Get-Content -LiteralPath '%LOCALAPPDATA%\KAOSDUO\logs\startup.log' -Tail 35"
) else (
  echo No startup error log was written.
)
echo.
pause
exit /b %RESULT%
