# BrickBlast — Android Phone Release

## Overview
Native Android phone build via **Capacitor** wrapping the HTML5 game.

## Prerequisites
- Node.js 18+
- Android Studio (Flamingo or later) with SDK 33+
- Java 17+

## Build Steps

```bash
# 1. Navigate to the Capacitor project root
cd "../mobile-capacitor"

# 2. Install dependencies
npm install

# 3. Sync web assets into the Android project
npx cap sync android

# 4. Open in Android Studio
npx cap open android
```

In Android Studio:
- Select **Build → Generate Signed Bundle / APK**
- Choose **Android App Bundle (.aab)** for Google Play submission
- Sign with your keystore

## Target Specs
| Property | Value |
|----------|-------|
| Target SDK | API 33 (Android 13) |
| Min SDK | API 24 (Android 7) |
| Orientation | Landscape |
| Form Factor | Phone |

## Distribution
Upload the `.aab` to [Google Play Console](https://play.google.com/console) under a **Phone** release track.

## Notes
- `capacitor.config.json` — app ID, web dir, plugin settings
- `BUILD_GUIDE.md` — full step-by-step walkthrough
