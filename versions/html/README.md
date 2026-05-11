# Brick Blast — HTML5 / Browser / PWA

**Team Fast Talk** — Single-file HTML5/Canvas game. Works in any modern browser.

> **ℹ️ This is the only version of Brick Blast that is intentionally delivered as HTML.**
> All other platform releases (Android, iOS, Windows, Linux, macOS) are native binaries.

## Contents
| File | Purpose |
|------|---------|
| `index.html` | Complete game — single self-contained file (~65 KB) |
| `manifest.json` | PWA manifest (offline install support) |
| `RUN_HTML.bat` | Windows launcher (opens in default browser) |
| `PUBLISHING.md` | Deployment and distribution guide |
| `README.md` | This file |

## Play Instantly
**Windows:** Double-click `RUN_HTML.bat`  
**macOS / Linux:** `open index.html` or drag into any browser tab  
**Mobile browser:** Visit your hosted URL or open the file via Files app

## Install as PWA (Offline)
1. Open `index.html` from a web server (or hosted URL)
2. In Chrome/Edge: address bar → **Install** (⊕) icon
3. In Safari (iOS): Share → **Add to Home Screen**

The game works **100% offline** once installed.

## Host for Free
| Platform | Steps |
|----------|-------|
| **GitHub Pages** | Push to `gh-pages` branch; enable Pages in repo Settings |
| **itch.io** | Upload as HTML game; mark "This file will be played in the browser" |
| **Netlify** | Drag-and-drop the folder at [netlify.com/drop](https://app.netlify.com/drop) |
| **Vercel** | `vercel --prod` from this folder |

## Controls
| Input | Action |
|-------|--------|
| Mouse drag | Move paddle (desktop) |
| Touch drag | Move paddle (mobile) |
| Click / Tap | Start / Resume / Speed boost (2×) |
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
- **Persistent high scores** and **stats** (uses localStorage)
- **Colorblind mode** and **speed boost (2×)** toggle
- **Full gamepad support** (Xbox, PlayStation, Switch, generic)
- **Credits** and **Stats** screens
