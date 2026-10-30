@echo off
setlocal EnableExtensions
cd /d "%~dp0"
title KAOSDUO REFORGED 6.0.1 - Native VB.NET Build
chcp 65001 >nul
cls
echo ============================================================
echo     KAOSDUO: REFORGED v6.0.1 - VB.NET BUILD FIX
echo ============================================================
echo.
where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: .NET SDK is missing.
  echo Please install .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
  pause
  exit /b 1
)
echo Installed SDK:
dotnet --version
echo.
set "PROJECT=%CD%\src\KAOSDUO.VB\KAOSDUO.VB.vbproj"
if not exist "%PROJECT%" (
  if exist "%CD%\KAOSDUO.VB\KAOSDUO.VB.vbproj" (
    echo NOTE: This is the old v6.0.0 ZIP folder layout. Using compatibility path.
    set "PROJECT=%CD%\KAOSDUO.VB\KAOSDUO.VB.vbproj"
  ) else (
    echo ERROR: Cannot find the VB.NET project.
    echo Expected: src\KAOSDUO.VB\KAOSDUO.VB.vbproj
    echo Extract the ENTIRE SOURCE ZIP before launching BUILD_WINDOWS.bat.
    pause
    exit /b 2
  )
)
if not exist "dist\win-x64" mkdir "dist\win-x64"
echo Building native Windows x64 game... output also saved to BUILD_LOG.txt
echo.
dotnet publish "%PROJECT%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=None -p:DebugSymbols=false -o "%CD%\dist\win-x64" > "%CD%\BUILD_LOG.txt" 2>&1
set "RESULT=%ERRORLEVEL%"
type "%CD%\BUILD_LOG.txt"
if not "%RESULT%"=="0" (
  echo.
  echo ERROR: Build failed. Please send BUILD_LOG.txt for troubleshooting.
  pause
  exit /b %RESULT%
)
if not exist "dist\win-x64\KAOSDUO.exe" (
  echo ERROR: Build completed but KAOSDUO.exe was not produced.
  pause
  exit /b 3
)
echo.
echo SUCCESS: dist\win-x64\KAOSDUO.exe
echo Run KAOSDUO.exe directly or use RUN_KAOSDUO_DEBUG.bat.
pause
exit /b 0
