# Brick Blast — Linux Version

## How to Run

**Option A — Shell launcher (recommended)**
```bash
chmod +x RUN_LINUX.sh
./RUN_LINUX.sh
```

**Option B — Open directly in your browser**
```bash
xdg-open index.html
```

This folder is fully self-contained. Zip it and share — works on any Linux system with a browser.

## Contents
| File | Purpose |
|------|---------|
| `index.html` | Complete single-file game (HTML5 / Canvas / JavaScript) |
| `manifest.json` | PWA manifest — install as a desktop app via Chrome/Edge |
| `RUN_LINUX.sh` | Shell script to open the game in the default browser |
| `BrickBlast.desktop` | Freedesktop `.desktop` entry for app-menu integration |
| `assets/` | Game icons and supplemental assets |
| `README.md` | This file |

## Requirements
- Any modern browser: **Firefox**, **Chromium/Chrome**, **Edge**, **Brave**
- No install, no runtime, no root access needed
- Works on: Ubuntu, Debian, Fedora, Arch, openSUSE, Mint, Pop!_OS, and any other distro

## Install as Desktop App (optional)

### PWA via Chromium/Chrome
1. Open `index.html` in Chromium or Chrome.
2. Click the **Install** icon (⊕) in the address bar.
3. BrickBlast appears in your application launcher.

### Freedesktop `.desktop` entry
```bash
# Copy to your local applications menu
cp BrickBlast.desktop ~/.local/share/applications/
# Update icon path if you move the folder
# Right-click the app menu → refresh, or run:
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
- **Type:** Single-file HTML5/Canvas/JavaScript
- **Source:** `web/index.html` in repository root
- **Tested on:** Ubuntu 24.04 LTS (Firefox 126, Chromium 125), Arch Linux (Firefox 126)
- Runs at native **1200 × 867** logical resolution; scales to fit the browser window

## Build / Update from Source
If you want to rebuild from the latest source code:
```bash
# From the repository root
cp web/index.html versions/linux/index.html
cp web/manifest.json versions/linux/manifest.json
```
No compilation step is needed — the game is pure JavaScript.

## Troubleshooting
| Issue | Fix |
|-------|-----|
| `RUN_LINUX.sh` not executable | `chmod +x RUN_LINUX.sh` |
| No browser opens | Install Firefox: `sudo apt install firefox` |
| Blank screen | Open browser console (F12) and check for path errors |
| Sound not working | Check browser audio permissions; click the page once to enable |
