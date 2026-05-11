# Brick Blast — iPad

**Team Fast Talk** — Capacitor-native package wrapping the canonical HTML5 game source.

This folder is fully self-contained. Zip it and share.

> **⚠️ Source version:** The IPA must be built from `mobile/www/index.html`
> (canonical game, ~65 KB). If your archived IPA was built from an older version,
> **rebuild** using the steps in `PUBLISHING.md`.

## Contents
| File / Folder | Purpose |
|---------------|---------|
| `BUILD_IOS.sh` | Automated build script (macOS only) |
| `BUILD_IOS.bat` | Windows helper that prints macOS requirements |
| `APP_STORE_GUIDE.md` | Step-by-step App Store Connect submission guide |
| `PUBLISHING.md` | Full rebuild + release workflow |
| `capacitor.config.json` | Capacitor project configuration |
| `package.json` | npm dependencies for Capacitor CLI |
| `README.md` | This file |

## Requirements
- **Build machine:** macOS 13+ with Xcode 15+, CocoaPods, Node.js 18+
- **Target device:** iPad with iPadOS 15.0+
- **Signing:** Valid Apple Developer account and distribution certificate

## How This App Is Packaged
Brick Blast for iPad uses **Capacitor 6** to wrap the canonical HTML5 Canvas game inside
a native iOS WKWebView container. The game logic runs entirely in JavaScript/Canvas — no internet
connection is needed after install. The IPA is signed and behaves identically to a native app.

## Build (macOS)
```bash
cd "versions/ipad"
bash BUILD_IOS.sh
```
See `PUBLISHING.md` for step-by-step instructions, signing configuration, and App Store submission.

## Controls
| Input | Action |
|-------|--------|
| Touch drag | Move paddle |
| Tap | Start / Resume / Speed boost (2×) |
| Gamepad | MFi controller supported |

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
- **Full MFi gamepad support**
- **Credits** and **Stats** screens
