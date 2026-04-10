# Brick Blast — All Platform Versions

Each folder below contains a **launcher shortcut** (.bat) and a **README** with full instructions for that platform.

| Folder | Platform | Launcher | How It Works |
|--------|----------|----------|--------------|
| `windows/` | Windows Desktop | `RUN_WINDOWS.bat` | Native VB.NET WinForms app |
| `html/` | Any Browser | `RUN_HTML.bat` | Opens `web/index.html` in browser |
| `android-phone/` | Android Phone | `RUN_ANDROID_PHONE.bat` | Capacitor APK or PWA install |
| `android-tablet/` | Android Tablet | `RUN_ANDROID_TABLET.bat` | Same APK, auto-scales to tablet |
| `iphone/` | iPhone (iOS) | `RUN_IPHONE.bat` | PWA via Safari "Add to Home Screen" |
| `ipad/` | iPad | `RUN_IPAD.bat` | PWA via Safari "Add to Home Screen" |

## Quick Start

### Windows
Double-click `windows/RUN_WINDOWS.bat`

### Browser (Any Device)
Double-click `html/RUN_HTML.bat`

### Android (Phone or Tablet)
1. Deploy to GitHub Pages: `https://stuffthings15.github.io/BrickBlast/web/`
2. Open on device in Chrome → Menu → "Add to Home Screen"
3. **OR** build APK from `mobile/android/` with Android Studio

### iPhone / iPad
1. Deploy to GitHub Pages: `https://stuffthings15.github.io/BrickBlast/web/`
2. Open on device in Safari → Share → "Add to Home Screen"

## Architecture

```
BrickBlast/
├── Form1.vb                    ← Windows desktop source (VB.NET)
├── web/index.html              ← HTML5 source (all other platforms)
├── mobile/
│   ├── android/                ← Capacitor Android project (APK builds)
│   └── www/                    ← Shared web assets for mobile
└── versions/
    ├── windows/                ← Windows launcher + README
    ├── html/                   ← Browser launcher + README
    ├── android-phone/          ← Android phone launcher + README
    ├── android-tablet/         ← Android tablet launcher + README
    ├── iphone/                 ← iPhone launcher + README
    └── ipad/                   ← iPad launcher + README
```
