# Brick Blast — Linux Version

## How to Run

**Option A — Native binary (best performance)**
```bash
chmod +x "bin/anime finder macos"
./bin/"anime finder macos"
```
> The native binary requires no browser. It runs as a standalone desktop app using .NET 10 + Avalonia UI.

**Option B — Shell launcher (browser version)**
```bash
chmod +x RUN_LINUX.sh
./RUN_LINUX.sh
```

**Option C — Open directly in your browser**
```bash
xdg-open index.html
```

This folder is fully self-contained. Zip it and share — works on any Linux system.

## Contents
| File / Folder | Purpose |
|---------------|---------|
| `bin/` | Native Linux x64 executable + required `.so` libraries |
| `bin/anime finder macos` | Native Linux binary (ELF x64 — .NET 10 / Avalonia UI) |
| `bin/libSkiaSharp.so` | 2D graphics library |
| `bin/libHarfBuzzSharp.so` | Text shaping library |
| `index.html` | HTML5 browser version (fallback — same full game) |
| `manifest.json` | PWA manifest — install as desktop app via Chrome/Edge |
| `RUN_LINUX.sh` | Shell script to open the game in the default browser |
| `BrickBlast.desktop` | Freedesktop `.desktop` entry for app-menu integration |
| `assets/` | Game sprites, tiles, and UI images |
| `icons/` | App icons for PWA and desktop entry |
| `README.md` | This file |

## Requirements
- **Native binary:** Linux x64, glibc 2.17+, no browser needed
- **Browser version:** Any modern browser (Firefox, Chromium, Chrome, Edge, Brave)
- Works on: Ubuntu, Debian, Fedora, Arch, openSUSE, Mint, Pop!_OS, and any other distro

## Install as Desktop App (optional)

### PWA via Chromium/Chrome
1. Open `index.html` in Chromium or Chrome.
2. Click the **Install** icon (⊕) in the address bar.
3. BrickBlast appears in your application launcher.

### Freedesktop `.desktop` entry
```bash
cp BrickBlast.desktop ~/.local/share/applications/
update-desktop-database ~/.local/share/applications/
```

## Controls
| Input | Action |
|-------|--------|
| ← → / A D / WASD | Move paddle |
| SPACE / Click | Start / Resume / Speed boost (2×) |
| P / ESC | Pause |
| F | Speed boost (2×) |
| H / O | Options menu |
| Mouse click | Speed up ball during gameplay |

## Platform Notes
- **Native binary:** .NET 10, Avalonia UI, self-contained (no .NET install needed)
- **Browser version:** HTML5 / Canvas / JavaScript — identical game logic
- Tested on: Ubuntu 24.04 LTS, Arch Linux

## Rebuild Native Binary from Source
```bash
# From the repo root (requires .NET 10 SDK)
dotnet publish "anime finder macos/anime finder macos.csproj" -r linux-x64 -c Release --self-contained true -o versions/linux/bin
```

## Troubleshooting
| Issue | Fix |
|-------|-----|
| `Permission denied` on binary | `chmod +x "bin/anime finder macos"` |
| `RUN_LINUX.sh` not executable | `chmod +x RUN_LINUX.sh` |
| No browser opens | `sudo apt install firefox` |
| Blank screen in browser | Open browser console (F12) and check for path errors |
| Sound not working | Click the page once to enable browser audio |


## Features
- **Store** — buy and equip ball, brick, bonus, and paddle skins with in-game coins
- **13 ball skins** — Classic, Fire, Ice, Plasma, Gold, Rainbow, Lava, Void, Toxic, Neon, Crystal, Shadow, Sakura
- **10 brick palettes** — Classic, Toxic, Sunset, Forest, Ocean, Galaxy, Gold, Obsidian, Sakura, Aurora
- **16 bonus packs** — Classic, Ninja, Space, Candy, Cyberpunk, Medieval, Retro, Robot, Pirate, Galaxy, Festival, Halloween, Golden Age, and more
- **8 paddle skins** — Classic, Fire, Ice, Gold, Neon, Void, Sakura, Rainbow
- **10 chiptune music tracks** cycling automatically (Zelda, Mega Man, Tetris, Pac-Man, Space Invaders, Castlevania, Metroid, Galaga, Contra, Double Dragon)
- **5 SFX packs** — Classic, Zelda, Mega Man, Tetris, Retro Arcade
- **Combo system** — chain brick breaks for multiplied score
- **8 brick layout patterns** cycling by level
- **Power-ups** — ball grow/shrink, extra life, multi-ball, wide paddle, slow/fast ball
- **Persistent high scores** and **stats** (games played, bricks broken, coins earned)
- **Colorblind mode** and **speed boost (2×)** toggle
- **Full gamepad support** (Xbox, PlayStation, Switch, generic)
- **Credits** and **Stats** screens