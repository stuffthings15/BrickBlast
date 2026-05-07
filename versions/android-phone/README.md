# Brick Blast — Android Phone Version

This folder is fully self-contained. Zip it and share.

## Contents
| File | Purpose |
|------|---------|
| `BrickBlast-release.aab` | **Play Store upload** (Android App Bundle) |
| `BrickBlast-release.apk` | Signed APK (sideload / direct install) |
| `BrickBlast-Android.apk` | Debug APK (testing only) |
| `index.html` | PWA version (host on web server, install via Chrome) |
| `manifest.json` | PWA manifest |
| `icons/` | App icons for PWA install |
| `assets/` | Game sprites, tiles, and UI images |
| `PLAY_STORE_GUIDE.md` | Step-by-step Google Play submission guide |
| `RUN_ANDROID_PHONE.bat` | Info launcher |
| `README.md` | This file |

## Option 1: Install APK (Easiest)
1. Transfer `BrickBlast-release.apk` to your Android phone
2. Open the file on your phone to install
3. Enable **"Install unknown apps"** in Settings if prompted
4. Launch from app drawer

## Option 2: PWA Install (No APK)
1. Host `index.html` + `manifest.json` + `icons/` on any HTTPS web server
2. Open the URL on your phone in **Chrome**
3. Tap **⋮ menu → "Add to Home Screen"**
4. Game installs as a full-screen app

## Option 3: Google Play Store
See `PLAY_STORE_GUIDE.md` for full submission instructions using `BrickBlast-release.aab`.

## Controls
| Input | Action |
|-------|--------|
| Touch drag | Move paddle |
| Tap | Start / Resume / Speed boost (2×) |
| Gamepad | Full controller support |

## Requirements
- Android 5.0+ (API 21)
- No additional software needed

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