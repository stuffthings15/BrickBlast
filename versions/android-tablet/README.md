# Brick Blast — Android Tablet Version

## How to Run
The Android Tablet version uses the **same APK** as the phone version.
The game automatically scales to fill any screen size.

## Method 1: PWA Install (Easiest)
1. Host `web/index.html` on a web server or GitHub Pages
2. Open the URL on your Android tablet in **Chrome**
3. Tap **⋮ menu → "Add to Home Screen"**
4. Game installs as a full-screen landscape app

## Method 2: Native APK
See `versions/android-phone/README.md` for full APK build instructions.
The same APK works on tablets — no changes needed.

## Tablet-Specific Notes
- The game renders at **1200×867 logical resolution** and scales to fit
- Landscape orientation is recommended for best experience
- Touch controls work the same as phone version
- Bluetooth gamepad/keyboard fully supported

## Controls
| Input | Action |
|-------|--------|
| Touch + drag | Move paddle |
| Tap | Start / Resume |
| Bluetooth keyboard | Full keyboard controls |
| Bluetooth gamepad | Full controller support |

## Platform
- **Type:** Capacitor-wrapped HTML5 / PWA
- **Source:** `mobile/android/` + `web/index.html`
