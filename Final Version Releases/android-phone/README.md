# Brick Blast — Android Phone

**Team Fast Talk** — Capacitor-native package wrapping the canonical HTML5 game source.

This folder is fully self-contained. Zip it and share.

> **⚠️ Source version:** The APK/AAB must be built from `mobile/www/index.html`
> (canonical game, ~65 KB). If your packaged APK was built from an older version,
> **rebuild** using the steps in `PUBLISHING.md`.

## Contents
| File | Purpose |
|------|---------|
| `BrickBlast-release.aab` | **Play Store upload** — Android App Bundle |
| `BrickBlast-release.apk` | Signed APK (sideload / direct install) |
| `BrickBlast-Android.apk` | Debug APK (testing only — do NOT submit to Play) |
| `PLAY_STORE_GUIDE.md` | Step-by-step Google Play Console submission guide |
| `PUBLISHING.md` | Full rebuild + release workflow |
| `README.md` | This file |

## Install APK (Fastest — No Play Store needed)
1. Transfer `BrickBlast-release.apk` to your Android phone (USB, email, or Drive)
2. Open the file on your phone
3. If prompted, enable **"Install unknown apps"** for your file manager in Settings
4. Launch **Brick Blast** from the app drawer

## Google Play Store
See `PLAY_STORE_GUIDE.md` for step-by-step submission using `BrickBlast-release.aab`.

## How This App Is Packaged
Brick Blast for Android uses **Capacitor 6** to wrap the canonical HTML5 Canvas game inside
a native Android WebView container. The game runs entirely in JavaScript/Canvas — no internet
connection is needed after install. The APK is signed and behaves identically to a native app.

## Controls
| Input | Action |
|-------|--------|
| Touch drag | Move paddle |
| Tap | Start / Resume / Speed boost (2×) |
| Gamepad | Full controller support |

## Requirements
- Android 7.0+ (API 24)
- Landscape orientation recommended
- No additional software needed

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
