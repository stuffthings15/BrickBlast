# BrickBlast — Android Tablet Release

## Overview
Native Android tablet build via **Capacitor** wrapping the HTML5 game.
The game's 1200×867 logical canvas maps naturally to landscape tablet viewports.

## Prerequisites
- Node.js 18+
- Android Studio (Flamingo or later) with SDK 33+
- Java 17+

## Build Steps

```bash
cd "../mobile-capacitor"
npm install
npx cap sync android
npx cap open android
```

In Android Studio:
- Open `android/app/src/main/res/xml/config.xml` and confirm orientation is `landscape`
- **Build → Generate Signed Bundle / APK → Android App Bundle**

## Target Specs
| Property | Value |
|----------|-------|
| Target SDK | API 33 (Android 13) |
| Min SDK | API 24 (Android 7) |
| Orientation | Landscape |
| Form Factor | Tablet (7" – 12") |

## Distribution
Upload `.aab` to Google Play Console under a **Tablet** release track.
Provide at least one tablet screenshot (≥ 1280×800) in the store listing.

## Notes
- For tablet-only release, set `requires.smallScreens=false` in `AndroidManifest.xml` supports-screens element.
