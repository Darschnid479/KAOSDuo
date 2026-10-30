# KAOSDUO: REFORGED v6.0.0 — Full VB.NET Engine Rewrite

This major release replaces the previous HTML/JavaScript game engine with a **native Visual Basic .NET Windows desktop application**.

### Gameplay
- Animated Blue and Metal heroes (stride / jump / idle / weapon recoil), using the original pixel-art graphics.
- Eight visible guns that are actually held, aim with the hero, and fire from their muzzle points; magazine ammo and reloading.
- Portal Sentinel boss fights now allow the character to move **freely in two dimensions (WASD)** while shooting and dodging.
- Ten monster waves, boss waves, 8-weapon coin armory, automatic between-wave shop offer, and progression upgrades.
- Nine intermissions with independent 50% probability of Neon Highway (cars, three lanes, nitro) or Geometry Portal (cube jump, flight, gravity flip).
- On geometry portal journeys: 25% chance of a surprise mid-run shooting miniboss.
- Portal failures lose a special ability but **do not lower HP**; unlimited retries.
- Original character-select soundtracks: heavy Metal riffs for Metal and cozy Blue theme for Blue.
- Saves, autosaves, screenshots (F12), combo bonus, fever mode and more.

### Downloads
- `KAOSDUO_REFORGED_v6.0.0_WIN64.zip`: self-contained Windows executable, embedded art/audio, no separate .NET installation required.
- `KAOSDUO_REFORGED_v6.0.0_SOURCE.zip`: editable VB.NET source code and original media assets.
- `SHA256SUMS_v6.0.0.txt`: download checksums.

### Limitations
- Windows 10/11 64-bit only. Previous browser saves are not compatible with the VB.NET save format.
- The rendering engine is native WinForms/GDI+ instead of a browser. The available character poses are dynamically composed from their original pixel-art image parts; there are no prerendered multi-frame animation sheets.
