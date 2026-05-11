# Brick Blast — macOS (Apple Silicon ARM64)

**Team Fast Talk** — Native desktop application built with .NET 10 + Avalonia UI.

> **⚠️ Source:** Built from the `anime finder macos` sub-project (Avalonia cross-platform).
> For Intel Macs, use the `macos/` folder instead.

## Contents
| File / Folder | Purpose |
|---------------|---------|
| `anime finder macos` | Native macOS ARM64 executable |
| `anime finder macos.pdb` | Debug symbols |
| `libAvaloniaNative.dylib` | Avalonia native rendering library |
| `libHarfBuzzSharp.dylib` | Text shaping library |
| `libSkiaSharp.dylib` | Skia rendering library |
| `Assets/` | Game assets (audio, graphics) |
| `RUN_MACOS_ARM64.sh` | One-click launcher script |
| `PUBLISHING.md` | Rebuild and distribution guide |
| `README.md` | This file |

## Run
```bash
chmod +x RUN_MACOS_ARM64.sh
./RUN_MACOS_ARM64.sh
```
Or directly:
```bash
chmod +x "anime finder macos"
./"anime finder macos"
```

> **Gatekeeper:** On first launch, right-click the binary → **Open** → **Open** to bypass the
> unsigned-app warning. Or run: `xattr -d com.apple.quarantine "anime finder macos"`

## Requirements
- macOS 11 Big Sur or later (Apple Silicon M1/M2/M3/M4)
- No browser, JVM, or additional runtime needed

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
