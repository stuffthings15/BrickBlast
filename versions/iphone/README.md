# Brick Blast — iPhone (iOS) Native Version

This folder is fully self-contained with both native and PWA options.

## Contents
- `xcode-project/` — Complete Xcode project (native iOS app)
- `BUILD_IOS.sh` — Automated build script (run on Mac)
- `BUILD_IOS.bat` — Build instructions (Windows)
- `index.html` — PWA version (fallback / web hosting)
- `manifest.json` — PWA manifest
- `icons/` — App icons
- `RUN_IPHONE.bat` — Launcher
- `README.md` — This file

## Option 1: Native App via Xcode (Recommended)
### Requirements
- Mac with **Xcode 15+** installed
- Free Apple Developer account (for device testing)
- CocoaPods (`sudo gem install cocoapods`)

### Steps
1. Copy `xcode-project/` folder to a Mac
2. Open Terminal in `xcode-project/App/`
3. Run `pod install`
4. Open `App.xcworkspace` in Xcode (NOT App.xcodeproj)
5. Set your **Team** in Signing & Capabilities
6. Select your iPhone as the run target
7. Press **⌘R** (Play) to build and install

### Automated Build
```bash
chmod +x BUILD_IOS.sh
./BUILD_IOS.sh
```

## Option 2: PWA Install (No Mac Required)
1. Host `index.html` + `manifest.json` + `icons/` on HTTPS server
2. Open URL in **Safari** on iPhone
3. Tap **Share → Add to Home Screen**

## Controls
| Input | Action |
|-------|--------|
| Touch drag | Move paddle |
| Tap | Start / Resume / Speed boost (2×) |
| Two-finger tap | Pause |
| Tilt (if enabled) | Move paddle |
| Gamepad | Full controller support (MFi) |

## Requirements
- iPhone with iOS 13.0 or later
- Safari for PWA install (no App Store needed)
- Mac + Xcode 15+ for native build

## App Details
- **Bundle ID:** `com.teamfasttalk.brickblast`
- **Orientation:** Landscape only
- **Min iOS:** 13.0+
- **Framework:** Capacitor 6 (Swift)

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