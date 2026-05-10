# BrickBlast: Velocity Market — Final Version Releases
## Team Fast Talk | CS-120 | .NET 10 / HTML5

This directory contains production-ready release artifacts for every target
platform.  All builds originate from the single canonical WinForms source
(`Form1.vb`) or the HTML5 canvas game (`web/`).

---

## Directory Map

```
Final Version Releases/
├── windows-x64/          ← Windows 10/11 x64 self-contained .exe
├── windows-arm64/        ← Windows 11 ARM64 self-contained .exe
├── windows-store/        ← MSIX package for the Microsoft Store
│   ├── payload/          ← Published app binaries
│   ├── Assets/           ← Store logo/tile images
│   ├── Package.appxmanifest
│   └── Build-MSIX.ps1    ← Run from VS Dev Prompt to create .msix
│
├── assembly/             ← Native x86 Win32 MASM launcher stub
│   ├── BrickBlast.asm
│   └── Build-Assembly.bat
│
├── html/                 ← Browser / itch.io HTML5 build (193 files)
│
├── itch.io/              ← itch.io release (HTML copy + Butler script)
│   └── push-itchio.sh    ← `bash push-itchio.sh` to deploy via Butler
│
├── react/                ← React SPA wrapping the HTML5 game
│   ├── public/game/      ← Copied HTML5 assets
│   ├── src/App.jsx       ← Iframe wrapper with fullscreen toggle
│   └── package.json      ← `npm install && npm start` / `npm run build`
│
├── react-native/         ← React Native (Android + iOS)
│   ├── src/App.jsx       ← WebView wrapper
│   ├── android/…/assets/game/  ← Bundled HTML5 game for WebView
│   └── package.json
│
├── mobile-capacitor/     ← Capacitor 6 — all four mobile targets
│   ├── www/              ← HTML5 game (web dir for Capacitor)
│   ├── capacitor.config.json
│   ├── BUILD_GUIDE.md    ← Step-by-step Android/iOS/store guide
│   └── BUILD_GUIDE_FINAL.md
│
├── android-phone/        ← Placeholder (built from mobile-capacitor)
├── android-tablet/       ← Placeholder (built from mobile-capacitor)
├── iphone/               ← Placeholder (built from mobile-capacitor)
├── ipad/                 ← Placeholder (built from mobile-capacitor)
│
├── electron-windows/     ← Electron desktop — Windows (x64 + ARM64)
│   ├── main.js
│   ├── game/             ← HTML5 game assets
│   └── package.json      ← `npm i && npm run build` → NSIS installer
│
├── electron-linux/       ← Electron desktop — Linux (x64 + ARM64)
│   ├── main.js
│   ├── game/
│   └── package.json      ← `npm i && npm run build` → AppImage
│
├── electron-macos/       ← Electron desktop — macOS (x64 + Apple Silicon)
│   ├── main.js
│   ├── game/
│   └── package.json      ← `npm i && npm run build` → DMG
│
├── linux-x64/            ← Linux native launcher (Python fallback + .sh)
│   ├── BrickBlast.sh
│   ├── brickblast.desktop
│   └── game/
│
├── linux-arm64/          ← Linux ARM64 launcher (same structure)
│
├── macos/                ← macOS launcher + .app stub
│   ├── BrickBlast.sh
│   ├── BrickBlast.app/Contents/Info.plist
│   └── game/
│
└── macos-arm64/          ← Apple Silicon macOS launcher + .app stub
```

---

## Platform Build Quick-Reference

| Target | How to Build / Ship |
|--------|---------------------|
| **Windows x64** | Ready — run `windows-x64\BrickBlast.exe` |
| **Windows ARM64** | Ready — run `windows-arm64\BrickBlast.exe` |
| **Windows Store (MSIX)** | Open VS Dev Prompt → `cd windows-store` → `.\Build-MSIX.ps1` → upload to Partner Center |
| **Win32 Assembly** | Open x86 VS Dev Prompt → `cd assembly` → `Build-Assembly.bat` |
| **HTML / Browser** | Upload `html/` folder contents to any static host |
| **itch.io** | Install Butler → `bash itch.io/push-itchio.sh` |
| **React web** | `cd react && npm install && npm run build` → deploy `build/` |
| **React Native** | See `react-native/package.json`; requires Android Studio / Xcode |
| **Android Phone/Tablet** | `cd mobile-capacitor && npm i && npx cap add android && npx cap sync` → Android Studio |
| **iPhone / iPad** | `cd mobile-capacitor && npx cap add ios && npx cap sync` → Xcode (macOS only) |
| **Google Play Store** | Build signed AAB from Android Studio → upload to Play Console |
| **Apple App Store** | Archive from Xcode → App Store Connect |
| **Electron Windows** | `cd electron-windows && npm i && npm run build` → produces NSIS `.exe` |
| **Electron Linux x64** | `cd electron-linux && npm i && npm run build` → produces `.AppImage` |
| **Electron Linux ARM64** | Same as above; ARM64 target included in `package.json` |
| **Electron macOS** | `cd electron-macos && npm i && npm run build` → produces `.dmg` |
| **Linux native x64** | Copy `linux-x64/` to target machine → `bash BrickBlast.sh` |
| **Linux native ARM64** | Copy `linux-arm64/` to target machine → `bash BrickBlast.sh` |
| **macOS native** | Copy `macos/` → `bash BrickBlast.sh` (or open `.app` after signing) |
| **macOS Apple Silicon** | Copy `macos-arm64/` → `bash BrickBlast.sh` |

---

## Store Submission Checklist

### Microsoft Store
1. Run `Build-MSIX.ps1` to create `BrickBlast.msix`
2. Sign with a trusted certificate (`signtool sign ...`)
3. Upload to https://partner.microsoft.com

### Google Play Store
1. Open `mobile-capacitor/` in Android Studio
2. Build → Generate Signed Bundle/APK → Android App Bundle
3. Upload AAB to https://play.google.com/console

### Apple App Store
1. Open `mobile-capacitor/ios/` in Xcode (macOS required)
2. Set your Team, bump build number, Product → Archive
3. Distribute → App Store Connect → Submit for review

### itch.io
```bash
# Install Butler: https://itch.io/docs/butler/
butler login
bash "Final Version Releases/itch.io/push-itchio.sh"
```

---

## Notes
- The WinForms project (`Form1.vb`) cannot compile natively on Linux/macOS —
  all non-Windows targets use the HTML5 canvas game as the runtime.
- The HTML5 game is a full feature-parity port already present in `web/`.
- Music (`Assets/Audio/*.mp3`) must be bundled manually into mobile builds;
  the Capacitor `www/` sync step handles this automatically.
- ARM64 Windows runs the native .NET 10 self-contained build directly.
- ARM64 Linux/macOS run the HTML5 game via the Electron AppImage/DMG.
