# Brick Blast — Linux (x64)

**Team Fast Talk** — Native desktop application built with .NET 10 + Avalonia UI.

> **⚠️ Source:** Built from the `anime finder macos` sub-project (Avalonia cross-platform).
> The game binary is self-contained; no browser or internet connection is required.

## Contents
| File / Folder | Purpose |
|---------------|---------|
| `bin/` | Native Linux x64 binary and required shared libraries |
| `assets/` | Game assets (audio, graphics) |
| `RUN_LINUX.sh` | One-click launcher script |
| `BrickBlast.desktop` | Desktop shortcut file |
| `PUBLISHING.md` | Rebuild and distribution guide |
| `README.md` | This file |

## Run
```bash
chmod +x RUN_LINUX.sh
./RUN_LINUX.sh
```
Or directly:
```bash
chmod +x "bin/anime finder macos"
cd versions/linux
"bin/anime finder macos"
```

## Requirements
- Linux x86-64 (Ubuntu 20.04+, Fedora 36+, Arch, Debian 11+, or equivalent)
- `libgles2`, `libfontconfig`, `libX11` (usually pre-installed)
- No browser, JVM, or Wine needed

## Install Desktop Shortcut
```bash
cp BrickBlast.desktop ~/.local/share/applications/
update-desktop-database ~/.local/share/applications/
```

## Controls
| Input | Action |
|-------|--------|
| Mouse drag | Move paddle |
| Left click | Start / Resume / Speed boost (2×) |
| Gamepad | Full controller support |

## Features
- **Store** — buy and equip ball, brick, bonus, and paddle skins with in-game coins
- **13 ball skins** — Classic, Fire, Ice, Plasma, Gold, Rainbow, Lava, Void, Toxic, Neon, Crystal, Shadow, Sakura
- **10 brick palettes** — Classic, Toxic, Sunset, Forest, Ocean, Galaxy, Gold, Obsidian, Sakura, Aurora
- **16 bonus packs** — Classic, Ninja, Space, Candy, Cyberpunk, Medieval, Retro, Robot, Pirate, Galaxy, Festival, Halloween, Golden Age, and more
- **8 paddle skins** — Classic, Fire, Ice, Gold, Neon, Void, Sakura, Rainbow
- **10 chiptune music tracks** — Zelda, Mega Man, Tetris, Pac-Man, Space Invaders, Castlevania, Metroid, Galaga, Contra, Double Dragon
- **5 SFX packs** — Classic, Zelda, Mega Man, Tetris, Retro Arcade
- **Combo system** — chain brick breaks for multiplied score
- **8 brick layout patterns** cycling by level
- **Power-ups** — ball grow/shrink, extra life, multi-ball, wide paddle, slow/fast ball
- **Daily Challenge** — unique level seeded to today's date
- **Endless Mode** — infinite brick layouts
- **Persistent high scores** and **stats** (games played, bricks broken, coins earned)
- **Colorblind mode** and **speed boost (2×)** toggle
- **Full gamepad support** (Xbox, PlayStation, Switch, generic)
- **Credits** and **Stats** screens
