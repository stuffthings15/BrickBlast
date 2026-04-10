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

## App Details
- **Bundle ID:** `com.teamfasttalk.brickblast`
- **Orientation:** All orientations (landscape recommended)
- **Min iPadOS:** 13.0+
- **Framework:** Capacitor 6 (Swift)
