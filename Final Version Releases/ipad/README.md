# BrickBlast — iPad Release

## Overview
Native iPad build via **Capacitor** and **Xcode** wrapping the HTML5 game.
The 1200×867 logical canvas is ideal for 10"–13" iPad landscapes.

## Prerequisites
- macOS 13+ with **Xcode 15+**
- Apple Developer account
- Node.js 18+
- CocoaPods

## Build Steps

```bash
cd "../mobile-capacitor"
npm install
npx cap sync ios
npx cap open ios
```

In Xcode:
1. Set **Supported Devices** to include iPad
2. Confirm orientation: **Landscape Left** and **Landscape Right** only
3. **Product → Archive → Distribute App → App Store Connect**

## Target Specs
| Property | Value |
|----------|-------|
| Minimum iPadOS | 15.0 |
| Orientation | Landscape |
| Form Factor | iPad (9.7" – 13") |

## App Store Requirements
- iPad screenshots: 12.9" size class (2732×2048 or 2048×2732)
- Universal binary covers both iPhone and iPad from a single submission

## iPad-Specific Tips
- In Xcode's General → Deployment Info, ensure **iPad** checkbox is checked
- The WebView renders at native resolution; the game's canvas will scale automatically

## Notes
- `capacitor.config.json` — base configuration shared with phone builds
- `BUILD_GUIDE.md` — shared Capacitor walkthrough
