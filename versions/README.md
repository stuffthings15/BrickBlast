# Brick Blast — All Platform Versions

**Each folder is fully self-contained** — zip any folder and share it independently.

| Folder | Platform | Run With | Key File |
|--------|----------|----------|----------|
| `windows/` | Windows x64 (WinForms) | Double-click | `BrickBlast.exe` |
| `windows-arm64/` | Windows ARM64 (WinForms) | Double-click | `BrickBlast.exe` |
| `windows-wpf/` | Windows x64 (WPF) | Double-click | `BrickBlast.exe` |
| `windows-store/` | Windows Store (x64 + ARM64) | Double-click to sideload | `BrickBlast.msixbundle` |
| `macos/osx-x64/` | macOS Intel | Terminal | `anime finder macos` |
| `macos/osx-arm64/` | macOS Apple Silicon | Terminal | `anime finder macos` |
| `linux/` | Linux x64 | `RUN_LINUX.sh` | `bin/anime finder macos` |
| `html/` | Any Browser | `RUN_HTML.bat` | `index.html` |
| `android-phone/` | Android Phone | Install APK | `BrickBlast-Android.apk` |
| `android-tablet/` | Android Tablet | Install APK | `BrickBlast-Android.apk` |
| `iphone/` | iPhone (iOS) | Host + Safari PWA | `index.html` |
| `ipad/` | iPad | Host + Safari PWA | `index.html` |

## How to Run

- **Windows** → Double-click `BrickBlast.exe` — no install, no .NET runtime required
- **Windows Store** → Double-click `BrickBlast.msixbundle` to sideload (requires Developer Mode), or submit to Partner Center for Store distribution
- **macOS** → `chmod +x "anime finder macos" && ./"anime finder macos"` — requires macOS 12+
- **Linux** → `chmod +x RUN_LINUX.sh && ./RUN_LINUX.sh` — requires Ubuntu 20.04+ or equivalent
- **Android** → Transfer APK to phone/tablet, enable "Install unknown apps", tap to install
- **iPhone/iPad** → Host folder on HTTPS server, open in Safari, tap "Add to Home Screen"
- **Browser** → Open `index.html` in any modern browser

## Store Submission

| Store | Guide |
|-------|-------|
| Microsoft Store | `windows-store/README.md` |
| Google Play | `android-phone/PLAY_STORE_GUIDE.md` |
| Apple App Store (iPhone) | `iphone/APP_STORE_GUIDE.md` |
| Apple App Store (iPad) | `ipad/APP_STORE_GUIDE.md` |
