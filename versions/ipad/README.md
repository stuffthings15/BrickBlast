# Brick Blast — iPad Version

## How to Install (PWA — No Mac Required)
Same process as iPhone — install via Safari as a Progressive Web App.

### Steps
1. **Deploy** `web/index.html` to GitHub Pages or any HTTPS web server
2. Open the URL on your **iPad in Safari**
3. Tap the **Share button** (□↑)
4. Tap **"Add to Home Screen"**
5. Tap **"Add"**
6. Launch from home screen — runs full-screen in landscape

### GitHub Pages URL
```
https://stuffthings15.github.io/BrickBlast/web/
```

## iPad-Specific Notes
- The larger screen makes touch controls very comfortable
- Game renders at 1200×867 logical pixels, scales perfectly to iPad
- Landscape mode recommended (auto-locks when added to home screen)
- Apple Pencil taps work for gameplay

## Controls
| Input | Action |
|-------|--------|
| Touch + drag | Move paddle |
| Tap | Start / Resume |
| Bluetooth keyboard | Full keyboard controls |
| Bluetooth gamepad / MFi controller | Full controller support |
| Apple Pencil tap | Speed boost |

## Alternative: Capacitor iOS Build
See `versions/iphone/README.md` for native Xcode build instructions.
The same iOS build works on both iPhone and iPad.

## Platform
- **Type:** PWA (Progressive Web App) via Safari
- **Source:** `web/index.html`
- **Min iPadOS:** 13.0+
