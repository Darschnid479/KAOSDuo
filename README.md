<div align="center">

# ⚡ KAOSDUO: REFORGED

### v6.0.0 • 100% native Visual Basic .NET game engine • Windows

**The browser engine is gone. A real VB.NET game has taken its place.**

[![GitHub Release](https://img.shields.io/badge/DOWNLOAD-WINDOWS%20RELEASE-ffdf8c?style=for-the-badge)](https://github.com/Darschnid479/KAOSDuo/releases/tag/v6.0.0)
[![Visual Basic .NET](https://img.shields.io/badge/ENGINE-VISUAL%20BASIC%20.NET-9467f5?style=for-the-badge)](src/KAOSDUO.VB/)
[![Windows](https://img.shields.io/badge/PLATFORM-WINDOWS%2010%2F11-72eafa?style=for-the-badge)](BUILD_WINDOWS.bat)

</div>

## What's new in the full native rewrite

The **KAOSDUO: REFORGED** Windows game is rendered and simulated using Windows Forms/GDI+ in **Visual Basic .NET**. No WebView, browser, HTML game engine, JavaScript or C# gameplay code is used.

- **Animated original pixel-art heroes:** separate animated legs, idle breathing, walking sway, jump, damage flashing, and firing recoil.
- **Visible held weapons:** eight distinct guns are drawn in the hands, rotate toward the target, and **bullets spawn from the actual muzzle**.
- **Free movement in the mid-portal miniboss fight:** WASD/arrows to move in both axes; mouse/F to aim and shoot, with three portal shields and no HP loss.
- **10 combat waves** with five enemy families and big bosses.
- **8 buyable weapons**, including shotgun, railgun, chain lightning, rockets, ice repeater, minigun and prism weapon.
- **Coin shop after every combat wave** and optional shop while fighting; weapon purchases persist.
- **Nine between-wave journeys:** each is independently **50% Neon Highway** or **50% Geometry Portal**.
- Geometry stages mix jump cube, fly ship and gravity flips; **25% of geometry portals** spawn a surprise mid-run shooting miniboss.
- Car stages use 3 lanes, traffic hazards, collectibles and nitro.
- Portal failure permanently removes one remaining ability for the current run, **never combat HP**. Unlimited retries.
- Automatic and manual **save/load**, including portal and boss positions, shop purchases, difficulty and weapons.
- Different **original MP3 soundtracks** for Blue and Metal, included as embedded game resources. Toggle with `M`.
- Extra features: OVERDRIVE, kill combo, loot-based economy, ammo magazines/reload, weapon recoil, boss shield patterns and screenshot capture.

## Download / Run

Download **[v6.0.0](https://github.com/Darschnid479/KAOSDuo/releases/tag/v6.0.0)**, extract the `WIN64.zip` file and run `KAOSDUO.exe` on a 64-bit Windows 10/11 PC. The build is self-contained; players don't need to install .NET separately.

**Developers:** install .NET SDK 8 or newer and double-click `BUILD_WINDOWS.bat`, or run:

```powershell
dotnet publish src/KAOSDUO.VB/KAOSDUO.VB.vbproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=false -o dist/win-x64
```

## Controls

| Action | Controls |
| --- | --- |
| Move hero | `A/D`, arrow keys |
| Jump / double-jump | `W`, `Space`, `Up` |
| Aim / fire | Hold left mouse, or `F` for auto-target |
| Dash | `Shift` |
| Character super | `G` |
| OVERDRIVE fever | `Q` when charged |
| Reload | `R` |
| Equip bought weapons | `1`–`8`, mouse wheel, `Tab` |
| Shop | `B` (and after every wave) |
| In portal miniboss battle | **WASD / arrows: freely move; mouse/F: shoot** |
| Geometry portal | `Space/W`: jump, hold to fly, tap to flip gravity |
| Neon Highway | `A/D`: change lanes; hold `Space`: nitro |
| Save / load | `F5` / `F9` |
| Screenshot | `F12` (Pictures/KAOSDUO) |
| Pause / audio / fullscreen | `P`/`Esc`, `M`, `F11` |

## Save file

Saved locally as `%APPDATA%\KAOSDUO\save_v6.json`. The new native save format is not interchangeable with the old browser `localStorage` format. Your old HTML releases remain archived as previous releases.

## Tech / project structure

- `src/KAOSDUO.VB/GameData.vb`: gameplay and save models, weapons and probability rules.
- `src/KAOSDUO.VB/GameForm.vb`: native WinForms game loop, input, menus, save/load.
- `src/KAOSDUO.VB/Simulation.vb`: native VB.NET combat and portal physics.
- `src/KAOSDUO.VB/RenderHelpers.vb` and `RenderScenes.vb`: native GDI+ visuals and character animations.
- `src/KAOSDUO.VB/MusicPlayer.vb`: MP3 music playback using built-in Windows multimedia APIs.
- `src/KAOSDUO.VB/Assets`: original character illustrations and original character themes embedded in the executable.
- `BUILD_WINDOWS.bat`: Windows build command.

## Platform

**Windows only** for this native WinForms edition. An Android port of the VB.NET rewrite would require a separate platform UI and renderer; the old experimental Android APK is **not** v6.0 and is not part of this release.

The Geometry-style portal mode is an original, independent implementation; no assets from Geometry Dash are included.
