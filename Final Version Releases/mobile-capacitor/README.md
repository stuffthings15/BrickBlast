# BrickBlast — Capacitor Mobile (Shared Base)

This folder is the **shared Capacitor project** that backs the Android and iOS device releases.

## Folder Role
The four device-specific folders (`android-phone`, `android-tablet`, `iphone`, `ipad`) each copy from this base and add a `www/` game-asset folder. The actual native project (`android/` or `ios/`) is generated here by Capacitor CLI.

## Quick Start

```bash
npm install
npx cap add android
npx cap add ios
npx cap sync
```

Then open the platform project:

```bash
npx cap open android   # Android Studio
npx cap open ios       # Xcode
```

## Files
| File | Purpose |
|------|---------|
| `capacitor.config.json` | App ID, web dir, plugin config |
| `BUILD_GUIDE_FINAL.md` | Detailed step-by-step build walkthrough |
| `www/` | HTML5 game assets (copied from `web/`) |

## Shared Config
App ID: `com.teamfasttalk.brickblast`
Web Dir: `www`
