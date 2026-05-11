# Publishing Guide — iPad Native Build (versions folder)

**Target:** iPad — Apple App Store via Capacitor + Xcode  
**This folder is the source of truth for the iPad native build.**

---

## Quick Start

Run on a Mac with Xcode and Node installed:

```bash
chmod +x BUILD_IOS.sh
./BUILD_IOS.sh
```

This script:
1. Installs Capacitor npm packages (`npm install`)
2. Stages web assets into `www/` from `index.html`, `manifest.json`, and `icons/`
3. Syncs web assets into the Xcode project (`npx cap sync ios`)
4. Installs CocoaPods if needed
5. Builds the Xcode archive → `xcode-project/App/build/BrickBlast.xcarchive`

---

## Prerequisites (Mac Required)

| Tool | Install |
|------|---------|
| Xcode 15+ | Mac App Store |
| Node.js 18+ | https://nodejs.org |
| CocoaPods | `sudo gem install cocoapods` (script installs automatically) |
| Apple Developer account | https://developer.apple.com ($99/year for App Store) |

---

## After the Build

1. Open `xcode-project/App/App.xcworkspace` in Xcode
2. Go to **Window → Organizer**
3. Select the `BrickBlast` archive
4. Click **Distribute App**
5. Choose: **App Store Connect** (for submission) or **Ad Hoc** (for device testing)

---

## App Store Submission

For full App Store submission steps and listing content, see:
- `../../Final Version Releases/ipad/PUBLISHING.md` — complete submission guide
- `../../docs/Submission/StoreListingCopy.md` — store listing copy
- `../../docs/Screenshots/` — screenshots

---

## Key Files in This Folder

| File | Purpose |
|------|---------|
| `BUILD_IOS.sh` | Main build automation script |
| `BUILD_IOS.bat` | Windows helper (opens Mac SSH instructions) |
| `RUN_IPAD.bat` | Launches browser preview on Windows |
| `capacitor.config.json` | Capacitor app config (appId, webDir, iOS path) |
| `package.json` | Capacitor npm dependencies |
| `index.html` | The complete HTML5 game |
| `manifest.json` | PWA manifest |
| `icons/` | All app icons |
| `xcode-project/App/` | Xcode/Capacitor iOS native project |

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `npx cap` not found | Run `npm install` first |
| Pod install fails | Run `sudo gem install cocoapods` then retry |
| Xcode says web assets missing | Run `npx cap sync ios` manually |
| `www/` folder has old content | Script does `rm -rf www` on each run — this is expected |
| Simulator works but device doesn't | Check provisioning profile and signing in Xcode → Signing & Capabilities |
