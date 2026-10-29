<div align="center">

# ⚡ KAOSDUO: PORTALPANIKK — OVERDRIVE

### VERSION 2.0.0 · SOLO EDITION · FREE BROWSER GAME

**ONE HERO. TEN WAVES. THREE BOSSES. ABSOLUTE CHAOS.**

[![Play Now](https://img.shields.io/badge/PLAY-OVERDRIVE-FFE17A?style=for-the-badge&labelColor=21142E)](https://darschnid479.github.io/KAOSDuo/play/)
[![Latest Release](https://img.shields.io/badge/DOWNLOAD-v2.0.0-FF74A3?style=for-the-badge&labelColor=21142E)](https://github.com/Darschnid479/KAOSDuo/releases/tag/v2.0.0)
[![Platform](https://img.shields.io/badge/PLATFORM-WEB%20%7C%20OFFLINE_HTML-73F1FC?style=for-the-badge&labelColor=21142E)](#download--play)

![KAOSDUO Overdrive menu](assets/menu.webp)

</div>

## 🎮 ABOUT THE GAME

**KAOSDUO: PORTALPANIKK — OVERDRIVE** is a fast, ridiculous 2D action-survival game. Pick **one** of two original pixel-art fighters and battle your way through a world bursting with monsters, projectiles, lasers, and completely unreasonable bosses.

- **Single-player only**. Choose **Blue** ❄️ or **Metal** 🎸 — never two simultaneous characters or AI sidekicks.
- **10 escalating waves**, with **three multi-phase boss encounters**: Glitch Warden (wave 4), Crystal Titan (wave 7), and Portal King (wave 10).
- **4 difficulty modes**: Lett, Medium, Vanskelig, and Drit Vanskelig.
- **Double jump, turbo-dash, attack animations, and special attacks**.
- **OVERDRIVE rage mode** powered up by fighting enemies.
- **17 upgrade choices** including chain lightning, critical hits, orbiting auto-turret, explosive shots, lifesteal, and movement boosts.
- Dynamic arenas, warning markers for dangerous attacks, particle bursts, combo scoring, and original synthesized retro music.
- High scores stored locally in your browser. No registration, login, account, or online server required.

## 📸 GAMEPLAY

![The Portal King boss battle during OVERDRIVE](assets/gameplay.webp)

## ❄️ CHOOSE YOUR HERO

| Blue — The Frost Fighter | Metal — The Riff Machine |
| --- | --- |
| Snowball projectiles, freezing attacks | Musical projectiles, chaining splash attacks |
| **SPECIAL: ABSOLUTE ZERO** | **SPECIAL: APOCALYPSE SOLO** |
| Excellent crowd control | Excellent damage and explosive effects |

The game always has one playable character. There is no multiplayer or companion AI.

## 🎮 CONTROLS

| Action | Keyboard | Touch (supported touch browser) |
| --- | --- | --- |
| Move | `A` / `D` or `←` / `→` | Left/right arrows |
| Jump / double-jump | `W` / `↑` / `Space`, tap again | Jump button (release + press again) |
| Turbo-dash | `Shift` or `Q` | Dash button |
| Shoot | `F` or `K` (hold) | Fire button |
| Super attack | `G` or `L` (100% charge) | Super button |
| Pause | `P` or `Escape` | Pause button |
| Toggle music/sound | `M` | Sound button |
| Choose upgrade | `1`, `2`, `3` or click | Tap a card |

## 🔥 FOUR DIFFICULTIES

| Mode | Starting HP | Enemy pressure |
| --- | ---: | --- |
| **Lett** 😎 | 150 | Lower |
| **Medium** ⚔️ | 110 | Balanced |
| **Vanskelig** 🔥 | 90 | Intense |
| **Drit Vanskelig** 💀 | 75 | Extreme |

## 📦 DOWNLOAD & PLAY

**In browser:** [Play OVERDRIVE](https://darschnid479.github.io/KAOSDuo/play/) when GitHub Pages is enabled in repository settings.

**Offline:** Open the downloadable [`KAOSDUO_OVERDRIVE_v2.0.0.html`](dist/KAOSDUO_OVERDRIVE_v2.0.0.html) locally in Chrome, Edge, or Firefox. The complete game and graphics are embedded and work without a network connection.

**Release:** [v2.0.0 download / release notes](https://github.com/Darschnid479/KAOSDuo/releases/tag/v2.0.0).

**Android:** The old v1.1 Android beta is **not** an OVERDRIVE v2.0 port. Its launch behavior remains unverified. No Android APK is included in v2.0.0.

## 🗂️ PROJECT STRUCTURE

```text
KAOSDuo/
├── index.html                       # Official game landing page
├── play/index.html                  # Actual playable browser game
├── assets/                          # Character art, screenshots, web style
├── dist/
│   ├── KAOSDUO_OVERDRIVE_v2.0.0.html        # One-file offline game
│   └── KAOSDUO_OVERDRIVE_v2.0.0_OFFLINE.zip # Offline download bundle
├── CHANGELOG.md
├── RELEASE_NOTES.md
└── .github/workflows/               # Pages + release automation
```

## 🛠️ TECHNICAL DETAILS

- HTML5 Canvas and vanilla JavaScript, no runtime dependencies.
- Pixel-art character assets with animated effects and procedural arena graphics.
- Generated Web Audio soundtrack and sound effects (no externally hosted audio).
- Persistent local best score (browser local storage when allowed).
- Responsive interface including touch controls; designed primarily for landscape displays.

## ✨ DEVELOPMENT

Created by [Darschnid](https://github.com/Darschnid479). Bug reports and suggestions are welcome through repository issues.

See [CHANGELOG.md](CHANGELOG.md) for the OVERDRIVE feature list and [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidance.