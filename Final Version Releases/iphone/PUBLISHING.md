# Publishing Guide — iPhone (App Store + TestFlight)

**Target:** iPhone — Apple App Store and TestFlight  
**Artifact:** IPA archive via Xcode + Capacitor  
**Status:** 🔒 NEEDS HOST — requires Mac with Xcode 15+

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `game/` | HTML5 game assets |
| `README.md` | iPhone release overview |

The Xcode project and Capacitor config are shared with the iPad build in `../../versions/ipad/`. The same app binary runs on both iPhone and iPad (Universal app).

---

## Build Steps

The iPhone build uses the same Capacitor/Xcode project as iPad:

```bash
cd "../../versions/ipad"
chmod +x BUILD_IOS.sh
./BUILD_IOS.sh
```

The `xcodebuild` command targets `generic/platform=iOS`, which covers both iPhone and iPad in a single archive.

---

## Making the App Universal (iPhone + iPad)

In Xcode:
1. Open `versions/ipad/xcode-project/App/App.xcworkspace`
2. Select the **App** target → **General**
3. Under **Deployment Info**, check both **iPhone** and **iPad**
4. Set **Requires full screen: Off** (supports Split View on iPad, landscape on iPhone)

This produces one IPA that Apple distributes to both device types.

---

## Export and Distribute

Same as iPad — see `../ipad/PUBLISHING.md` for:
- TestFlight beta distribution
- App Store submission steps
- Ad Hoc / Development distribution

The only difference is screenshot requirements:

### iPhone Screenshot Sizes Required by App Store

| Device | Resolution |
|--------|------------|
| iPhone 6.7" (14 Pro Max) | 1290×2796 px |
| iPhone 6.5" (11 Pro Max) | 1242×2688 px |
| iPhone 5.5" (8 Plus) | 1242×2208 px |

Capture screenshots on a simulator or device at the required sizes. Resize game window screenshots from `docs/Screenshots/` if needed.

---

## App Store Listing Quick Reference

| Field | Value |
|-------|-------|
| App Name | Brick Blast |
| Bundle ID | `com.teamfasttalk.brickblast` |
| Version | 1.0.0 |
| Category | Games → Arcade |
| Age Rating | 4+ |
| Price | Free |
| Supported Devices | iPhone 6s or later (iOS 13+) |

---

## Testing Before Publish

- [ ] Archive builds for `generic/platform=iOS` successfully
- [ ] App installs on physical iPhone via Xcode
- [ ] Game loads in WebView
- [ ] Portrait mode: game fits screen, no horizontal overflow
- [ ] Landscape mode: game uses full width
- [ ] Touch controls: drag to move paddle, tap to launch
- [ ] No crash on first launch
- [ ] App icon correct on home screen
- [ ] Notch / Dynamic Island area not obstructed by game UI

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| iOS | 13.0 or later |
| iPhone | iPhone 6s or later |
| Build host | Mac with Xcode 15+ |
| Developer account | Apple Developer ($99/year for App Store) |
