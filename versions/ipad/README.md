# Brick Blast — iPad Native Version

This folder is fully self-contained with both native and PWA options.

## Contents
- `xcode-project/` — Complete Xcode project (native iOS app)
- `BUILD_IOS.sh` — Automated build script (run on Mac)
- `BUILD_IOS.bat` — Build instructions (Windows)
- `index.html` — PWA version (fallback / web hosting)
- `manifest.json` — PWA manifest
- `icons/` — App icons
- `RUN_IPAD.bat` — Launcher
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
6. Select your iPad as the run target
7. Press **⌘R** (Play) to build and install

### Automated Build
```bash
chmod +x BUILD_IOS.sh
./BUILD_IOS.sh
```

## Option 2: PWA Install (No Mac Required)
1. Host `index.html` + `manifest.json` + `icons/` on HTTPS server
2. Open URL in **Safari** on iPad
3. Tap **Share → Add to Home Screen**

## Controls
| Input | Action |
|-------|--------|
| Touch drag | Move paddle |
| Tap | Start / Resume / Speed boost (2×) |
| Two-finger tap | Pause |
| Keyboard ← → / A D | Move paddle (when keyboard attached) |
| Keyboard SPACE | Start / Resume |
| Gamepad | Full controller support (MFi) |

## Requirements
- iPad with iPadOS 13.0 or later
- Safari for PWA install (no App Store needed)
- Mac + Xcode 15+ for native build

## App Details
- **Bundle ID:** `com.teamfasttalk.brickblast`
- **Orientation:** All orientations (landscape recommended)
- **Min iPadOS:** 13.0+
- **Framework:** Capacitor 6 (Swift)
