# KAOSDUO: REFORGED v6.0.1 — Startup & Source Build Repair

This maintenance release specifically addresses two reports from v6.0.0: the Windows executable closing on launch, and the downloadable source not compiling as packaged.

## Fixes

- Clean source ZIP now includes **`src/KAOSDUO.VB/KAOSDUO.VB.vbproj`** in the expected directory, together with **`BUILD_WINDOWS.bat`**. It does **not** bundle stale `bin` or `obj` output.
- Upgraded the VB.NET project to **.NET 10**, so it builds with the .NET 10 SDK. The Windows EXE is still self-contained.
- Embedded sprites and soundtracks now resolve by actual manifest resource name rather than relying on an assumed assembly prefix.
- Startup and UI exceptions are logged to `%LOCALAPPDATA%\KAOSDUO\logs\startup.log`, and startup errors show a message rather than simply disappearing.
- A missing optional EXE icon can no longer prevent the game window from opening.
- `RUN_KAOSDUO_DEBUG.bat` gives a straightforward way to retrieve the startup log.
- GitHub Actions now verifies the native menu, combat, shop and portal screens, tests the **published EXE**, and **rebuilds from the extracted source ZIP** before publishing any release files.

## Download and run

1. Download `KAOSDUO_REFORGED_v6.0.1_WIN64.zip`.
2. Extract the ZIP completely, then run `KAOSDUO.exe`. Windows 10/11 x64 required.
3. If it closes immediately, run `RUN_KAOSDUO_DEBUG.bat` in the extracted folder and share the diagnostic log.

## Build from source

Extract `KAOSDUO_REFORGED_v6.0.1_SOURCE.zip` and run `BUILD_WINDOWS.bat`. This requires the **.NET 10 SDK** (not just the .NET Runtime). A full compiler log is saved as `BUILD_LOG.txt`.

This build is native VB.NET WinForms/GDI+ and does not embed the previous HTML game. Older v6.0.0 saves remain in the same JSON format.
