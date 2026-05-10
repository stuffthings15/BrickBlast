# Brick Blast - Mobile Build Guide

## Overview
This creates native **Android APK** and **iOS IPA** from the HTML5 web game using [Capacitor](https://capacitorjs.com/).

### Compatibility
- **Android**: API 24+ (Android 7.0+) — covers Samsung Galaxy S8 and newer, all tablets from 2019+
- **iOS**: iOS 14+ — covers iPhone 8 and newer, iPad 6th gen and newer

---

## Prerequisites

### For Android (APK)
1. **Node.js 18+** — https://nodejs.org
2. **Android Studio** — https://developer.android.com/studio
   - During install, ensure these are checked:
     - Android SDK (API 34)
     - Android SDK Build-Tools
     - Android Emulator
     - Android SDK Platform-Tools
3. **Java JDK 17** — Android Studio bundles this

### For iOS (IPA) — requires a Mac
1. **Node.js 18+**
2. **Xcode 15+** — from Mac App Store
3. **CocoaPods** — `sudo gem install cocoapods`
4. **Apple Developer Account** — https://developer.apple.com (free for testing on your device, $99/yr for App Store)

---

## Quick Setup

Open a terminal in the `mobile/` directory:

```bash
# 1. Install dependencies
npm install

# 2. Copy web game files to www/
npm run build:web

# 3. Generate app icons
node scripts/generate-icons.js

# 4. Add Android platform
npx cap add android

# 5. Add iOS platform (Mac only)
npx cap add ios

# 6. Sync web files to native projects
npx cap sync
```

---

## Building Android APK

### Debug APK (for testing)
```bash
# Sync latest web changes
npm run build:android

# Open in Android Studio
npx cap open android
```

In Android Studio:
1. Wait for Gradle sync to complete
2. **Build → Build Bundle(s) / APK(s) → Build APK(s)**
3. APK is at: `android/app/build/outputs/apk/debug/app-debug.apk`

### Install on device
```bash
# Connect phone via USB with USB Debugging enabled
adb install android/app/build/outputs/apk/debug/app-debug.apk
```

### Signed Release APK (for distribution)
1. In Android Studio: **Build → Generate Signed Bundle / APK**
2. Choose **APK**
3. Create new keystore or use existing
4. Select **release** build type
5. APK at: `android/app/build/outputs/apk/release/app-release.apk`

### Command-line release build
```bash
cd android
./gradlew assembleRelease
```

---

## Building iOS (Mac only)

```bash
# Sync latest web changes
npm run build:ios

# Open in Xcode
npx cap open ios
```

In Xcode:
1. Select your Team in **Signing & Capabilities**
2. Connect iPhone/iPad via USB
3. Select your device as build target
4. **Product → Run** (⌘R)

For App Store / TestFlight:
1. **Product → Archive**
2. **Distribute App → App Store Connect**

---

## Testing on Devices

### Android
1. On your Samsung/Android phone: **Settings → Developer Options → USB Debugging → ON**
   - (Tap "Build Number" 7 times in Settings → About Phone to unlock Developer Options)
2. Connect via USB
3. Run from Android Studio or use `adb install`

### Android Emulator
1. In Android Studio: **Tools → Device Manager → Create Device**
2. Choose a phone (e.g., Pixel 7) or tablet (e.g., Pixel Tablet)
3. Download a system image (API 34)
4. Run the emulator, then build

### iOS Simulator (Mac)
```bash
npx cap run ios --target "iPhone 15"
```

### iOS Device
1. Connect iPhone/iPad via USB
2. Trust the computer on the device
3. In Xcode, select your device and run

---

## Updating the Game

After making changes to `web/index.html`:

```bash
# Re-copy web files and sync to native projects
npm run build:web
npx cap sync
```

Then rebuild in Android Studio / Xcode.

---

## Project Structure

```
mobile/
├── package.json              # Node dependencies (Capacitor)
├── capacitor.config.json     # Capacitor configuration
├── www/                      # Web game files (copied from ../web/)
│   ├── index.html           # Game with mobile integration
│   ├── manifest.json        # Web app manifest
│   └── icons/               # App icons
├── android/                  # Android project (generated)
│   ├── app/
│   │   ├── src/main/
│   │   │   ├── AndroidManifest.xml
│   │   │   ├── java/.../MainActivity.java
│   │   │   └── res/         # Android resources/icons
│   │   └── build.gradle
│   └── build.gradle
├── ios/                      # iOS project (generated)
│   └── App/
│       ├── App/
│       │   ├── Info.plist
│       │   └── AppDelegate.swift
│       └── App.xcworkspace
├── resources/               # Source icons/splash screens
│   ├── android/
│   └── ios/
└── scripts/
    ├── copy-web.js          # Copies web game + injects mobile code
    └── generate-icons.js    # Generates app icons
```

---

## Supported Devices (Last 5 Years)

### Samsung / Android
| Device | Android Version | Supported |
|--------|----------------|-----------|
| Galaxy S20/S21/S22/S23/S24 | 10-14 | ✅ |
| Galaxy A50/A51/A52/A53/A54 | 9-14 | ✅ |
| Galaxy Tab S6/S7/S8/S9 | 9-14 | ✅ |
| Google Pixel 4/5/6/7/8 | 10-14 | ✅ |
| OnePlus 8/9/10/11/12 | 10-14 | ✅ |

### Apple / iOS
| Device | iOS Version | Supported |
|--------|------------|-----------|
| iPhone 8/X/XR/XS | 14-17 | ✅ |
| iPhone 11/12/13/14/15 | 14-17 | ✅ |
| iPad 6th-10th gen | 14-17 | ✅ |
| iPad Air 3/4/5 | 14-17 | ✅ |
| iPad Pro (2018+) | 14-17 | ✅ |
| iPad Mini 5/6 | 14-17 | ✅ |

---

## Troubleshooting

### "SDK location not found"
Set `ANDROID_HOME` environment variable:
```bash
# Windows
setx ANDROID_HOME "%LOCALAPPDATA%\Android\Sdk"
# Mac
export ANDROID_HOME=~/Library/Android/sdk
```

### Gradle build fails
```bash
cd android
./gradlew clean
cd ..
npx cap sync android
```

### iOS pods fail
```bash
cd ios/App
pod install --repo-update
```

### Game doesn't fill screen
The game auto-scales to fill the viewport. If you see black bars, the aspect ratio (1200:867) may not match the device. This is normal — the game centers itself.

### Touch not working
Ensure the web game's touch handlers are working in the browser first. The Capacitor WebView passes all touch events through to the web content.
