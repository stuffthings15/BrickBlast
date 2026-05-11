# Publishing Guide — iPhone (versions folder)

**Target:** iPhone — Apple App Store via Capacitor + Xcode  
**The iPhone build shares the same Xcode project as iPad.**

---

## Quick Start

Run on a Mac with Xcode and Node installed:

```bash
cd ../ipad
chmod +x BUILD_IOS.sh
./BUILD_IOS.sh
```

The archive produced targets `generic/platform=iOS`, which covers both iPhone and iPad.

---

## Making the Build Universal (iPhone + iPad)

1. Open `../ipad/xcode-project/App/App.xcworkspace` in Xcode
2. Click the **App** target → **General** tab
3. Under **Deployment Info**, ensure **iPhone** and **iPad** are both checked
4. Set minimum deployment: **iOS 13.0**

One archive → one IPA → distributes to all iOS devices.

---

## Screenshot Requirements for iPhone

Apple requires screenshots at these sizes for App Store:

| Size | Device example |
|------|---------------|
| 6.7" (1290×2796) | iPhone 15 Pro Max |
| 6.5" (1242×2688) | iPhone 11 Pro Max |
| 5.5" (1242×2208) | iPhone 8 Plus |

Tip: Use Xcode Simulator at the required device size and take screenshots there.

---

## Distribution Options

| Method | Use Case |
|--------|----------|
| App Store Connect | Public release via App Store |
| TestFlight | Beta testing (up to 10,000 testers) |
| Ad Hoc | Up to 100 registered UDIDs |
| Development | Registered team devices only |

---

## App Store Submission Quick Reference

For full steps, see `../../Final Version Releases/iphone/PUBLISHING.md`.

| Field | Value |
|-------|-------|
| Bundle ID | `com.teamfasttalk.brickblast` |
| Version | 1.0.0 |
| Category | Games → Arcade |
| Age Rating | 4+ |
| Price | Free |

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Build error: no signing | Set team in Xcode → Signing & Capabilities |
| App crashes on launch | Check WebView URL in `capacitor.config.json` |
| Icons missing | Run `npx cap sync ios` from `../ipad/` |
| Landscape locks in portrait | Set orientation in `capacitor.config.json` ios section |
