# Brick Blast — Android Phone Version

## Method 1: PWA Install (Easiest — No Build Required)
1. Host `web/index.html` on a web server or GitHub Pages
2. Open the URL on your Android phone in **Chrome**
3. Tap the **⋮ menu → "Add to Home Screen"**
4. The game installs as a full-screen app with an icon

## Method 2: Native APK via Capacitor
The `mobile/android/` folder contains a pre-configured Capacitor Android project.

### Prerequisites
- [Android Studio](https://developer.android.com/studio) installed
- Android SDK 33+

### Build Steps
1. Open `mobile/android/` in Android Studio
2. Wait for Gradle sync to complete
3. Click **Build → Build Bundle(s) / APK(s) → Build APK(s)**
4. The APK will be at `mobile/android/app/build/outputs/apk/debug/app-debug.apk`
5. Transfer to your phone and install (enable "Unknown Sources" in Settings)

### Update Game Code
If `web/index.html` was updated, sync it to the Android project:
```
copy web\index.html mobile\www\index.html
copy web\index.html mobile\android\app\src\main\assets\public\index.html
```

## Controls (Touch)
| Input | Action |
|-------|--------|
| Touch + drag | Move paddle |
| Tap | Start / Resume / Speed boost |
| Two-finger tap | Toggle speed boost |

## Platform
- **Type:** Capacitor-wrapped HTML5 app / PWA
- **Source:** `mobile/android/` + `web/index.html`
- **Min Android:** 5.0 (API 21)
