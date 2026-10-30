@echo off
setlocal EnableExtensions
cd /d "%~dp0"
chcp 65001 >nul
cls
echo ============================================================
echo            KAOSDUO REFORGED v6.0 / VB.NET
 echo             NATIVE WINDOWS BUILD - NO HTML
 echo ============================================================
where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERROR] .NET SDK was not found.
  echo Install .NET SDK 8 or later from https://dotnet.microsoft.com/download
  pause
  exit /b 1
)
dotnet --version
if not exist "dist" mkdir "dist"
dotnet publish "src\KAOSDUO.VB\KAOSDUO.VB.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=None -o "dist\win-x64"
if errorlevel 1 (
  echo [ERROR] Compilation failed. Copy the error output when asking for assistance.
  pause
  exit /b 1
)
echo.
echo [OK] Game successfully compiled.
echo [OK] EXE: "%CD%\dist\win-x64\KAOSDUO.exe"
echo.
echo Run KAOSDUO.exe to play on Windows 10/11 (64-bit).
pause
endlocal
