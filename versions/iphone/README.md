# Brick Blast — iPhone (iOS) Version

## How to Install (PWA — No Mac Required)
The iPhone version runs as a **Progressive Web App** installed through Safari.

### Steps
1. **Deploy** `web/index.html` to GitHub Pages or any HTTPS web server
2. Open the URL on your **iPhone in Safari** (must be Safari, not Chrome)
3. Tap the **Share button** (□↑) at the bottom of Safari
4. Scroll down and tap **"Add to Home Screen"**
5. Tap **"Add"** in the top-right corner
6. The game now appears as an app icon on your home screen
7. Launch it — it runs **full-screen** without browser UI

### GitHub Pages URL
```
https://stuffthings15.github.io/BrickBlast/web/
```

## Alternative: Capacitor iOS Build (Requires Mac + Xcode)
If you need a native App Store build:
1. On a Mac, install Xcode and Node.js
2. Navigate to the `mobile/` folder
3. Run:
   ```bash
   npx cap add ios
   npx cap sync ios
   npx cap open ios
   ```
4. Build and run from Xcode

## Controls (Touch)
| Input | Action |
|-------|--------|
| Touch + drag | Move paddle |
| Tap | Start / Resume / Speed boost |
| Two-finger tap | Toggle speed boost |
| MFi controller | Full gamepad support |

## iOS-Specific Features
- `apple-mobile-web-app-capable` meta tag enables full-screen
- `apple-mobile-web-app-status-bar-style` set to `black-translucent`
- Touch events optimized with `touch-action: none`
- `viewport-fit=cover` for edge-to-edge on notched iPhones

## Platform
- **Type:** PWA (Progressive Web App) via Safari
- **Source:** `web/index.html`
- **Min iOS:** 12.0+
