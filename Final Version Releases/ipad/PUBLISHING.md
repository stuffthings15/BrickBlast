# Publishing Guide — iPad (App Store + TestFlight)

**Target:** iPad — Apple App Store and TestFlight  
**Artifact:** IPA archive via Xcode + Capacitor  
**Status:** 🔒 NEEDS HOST — requires Mac with Xcode 15+

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.ipa` | App archive (if pre-built and exported) |
| `game/` | HTML5 game source used for the WebView |
| `README.md` | iPad release overview |

The Xcode project and Capacitor config live in `../../versions/ipad/`.

---

## Full Build Steps

> Run these on a Mac. The `BUILD_IOS.sh` script handles all steps automatically.

```bash
cd "../../versions/ipad"
chmod +x BUILD_IOS.sh
./BUILD_IOS.sh
```

This script:
1. Installs Capacitor npm dependencies
2. Stages web assets into `www/`
3. Runs `npx cap sync ios`
4. Installs CocoaPods dependencies
5. Archives with `xcodebuild` → `xcode-project/App/build/BrickBlast.xcarchive`

---

## Export IPA from Archive

After `BUILD_IOS.sh` completes:

1. Open `versions/ipad/xcode-project/App/App.xcworkspace` in Xcode
2. Go to **Window → Organizer**
3. Select the `BrickBlast` archive
4. Click **Distribute App**
5. Choose distribution method:
   - **App Store Connect** — for App Store and TestFlight
   - **Ad Hoc** — for direct device install (no App Store)
   - **Development** — for team testing

---

## TestFlight (Beta Testing)

1. In Xcode Organizer → **Distribute App → App Store Connect**
2. Upload the build to App Store Connect
3. Go to https://appstoreconnect.apple.com
4. Select the app → **TestFlight** tab
5. Add testers by email or make a public TestFlight link
6. Users install via TestFlight app — no App Store approval required

---

## App Store Submission

1. Go to https://appstoreconnect.apple.com
2. **My Apps → New App**
   - Platform: iOS
   - Name: Brick Blast
   - Bundle ID: `com.teamfasttalk.brickblast`
   - SKU: `brickblast-001`
3. Fill in store listing:
   - **Description:** from `docs/Submission/StoreListingCopy.md`
   - **Keywords:** brick, blast, arcade, ball, paddle, breakout
   - **Category:** Games → Arcade
   - **Age Rating:** 4+
4. Upload screenshots:
   - Required sizes: 12.9" iPad Pro, 11" iPad Pro
   - Use `docs/Screenshots/` or capture from the game
5. Set pricing: Free
6. Upload build (from TestFlight or direct upload)
7. **Submit for Review** — Apple reviews in 1–3 days

**Registration:** $99/year Apple Developer Program

---

## Ad Hoc Distribution (No App Store)

1. Register tester UDIDs in Apple Developer Portal
2. Export IPA with **Ad Hoc** provisioning profile
3. Share IPA via email, AirDrop, or Diawi (https://www.diawi.com)
4. Users install via Diawi link or Xcode

---

## App Store Listing Quick Reference

| Field | Value |
|-------|-------|
| App Name | Brick Blast |
| Bundle ID | `com.teamfasttalk.brickblast` |
| Version | 1.0.0 |
| Build | 1 |
| Category | Games → Arcade |
| Age Rating | 4+ |
| Price | Free |
| Supported Devices | iPad (all models with iPadOS 13+) |

---

## Testing Before Publish

- [ ] Archive builds without errors on Mac
- [ ] IPA exports with Ad Hoc provisioning
- [ ] App installs on physical iPad via Xcode
- [ ] WebView loads — game appears
- [ ] Touch drag moves paddle smoothly
- [ ] Tap launches ball
- [ ] Landscape orientation works
- [ ] App icon appears correctly on home screen
- [ ] Splash screen displays then dismisses
- [ ] No crash on first launch or after background/foreground

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| iPadOS | 13.0 or later |
| iPad | All iPad models from 2017 (iPad 5th gen) onward |
| Build host | Mac with Xcode 15+ |
| Developer account | Apple Developer ($99/year for App Store) |
