# KAOSDUO: VB NEXT — native VB.NET engine preview

A new **Windows Forms / System.Drawing game engine implemented in Visual Basic .NET**. This does **not** embed HTML, JavaScript, Chromium or WebView2. It is a native Windows port and a new codebase, not a claim of exact v5.0 feature parity.

## Build on Windows

1. Install **.NET 10 SDK**.
2. Unzip the project and double-click `BUILD.bat`.
3. Run `publish/KAOSDUO.VBNext.exe` with the `publish/Assets` folder alongside it.

It is currently a **source preview**. The authoring environment does not have the .NET SDK or a Windows runtime, so Windows compilation/gameplay cannot yet be verified. Report the exact compiler errors if any.

## Features in the VB.NET code

- Solo only; Blå / Metal selectable; four difficulties
- Mouse aiming, WASD movement, jump, dodge and reloadless gunfire
- Original character art, animated walk cycles and recoil, visible hand-held gun and muzzle-origin bullets
- 8 weapon types, unlocks with coins, persistent shop; AI enemies and boss encounters
- 10 combat waves + 2D neon portal platforming, flying section and pseudo-3D road section
- Player movement during the portal miniboss shooting encounter
- Save/load using JSON via `System.Text.Json`
- Original MP3 loops using Windows MCI for Metal and Blå

## Controls

`A / D` move, `W / Space` jump or fly, `Mouse` aim, `Left mouse / J` shoot, `Q / E` change weapons, `1–8` select unlocked weapons, `Shift` dash, `B` shop, `F5` save, `F9` load, `M` mute, `Esc` pause.

In the car portal use `A / D` to switch lanes and `Shift` for nitro. During portal miniboss use `WASD` to move and mouse / left click to aim and fire.

## Scope

The published browser v5.0 is unchanged. This project starts the native engine migration. Do not treat it as a finished 1:1 replacement before a successful Windows build and playtest.
