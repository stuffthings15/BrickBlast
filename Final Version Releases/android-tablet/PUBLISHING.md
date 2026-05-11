# Publishing Guide — Android Tablet

**Target:** Android tablets (landscape-first layout, 7"+ screens)  
**Artifact:** APK / AAB via Capacitor  
**Status:** 🔒 NEEDS HOST — requires Android Studio

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `game/` | HTML5 game assets |
| `README.md` | Tablet-specific notes |

The Android native project lives in `../mobile-capacitor/android/`.

---

## Tablet-Specific Differences

The HTML5 game scales responsively based on viewport. Tablets benefit from:
- Wider canvas → more game area visible
- Larger hit targets
- Landscape orientation as default

In `capacitor.config.json`, orientation is set to `landscape`. No code change needed.

---

## Build (Same as Android Phone)

```bash
cd "../mobile-capacitor"
npm install
npx cap sync android
npx cap open android
```

Build APK or AAB — same process as phone. The same binary runs on both phone and tablet.

---

## Publish — Google Play Store (Tablet Optimization)

Google Play separates phone and tablet listings via screenshots. To qualify for tablet optimization badge:

1. Upload **tablet screenshots** (7" and 10") alongside phone screenshots
2. In Play Console: **Main store listing → Phone/7" tablet/10" tablet** tabs
3. Upload screenshots (minimum 1024×600 landscape for 7"; 1280×800 for 10")
4. The game is the same APK/AAB — just add the extra screenshots

---

## Tablet Screenshot Guidance

Capture at 1280×800 or similar resolution showing landscape gameplay:
- Main menu
- Gameplay (full level with bricks)
- Marketplace store
- Results screen

Use the Windows exe in a 1280×800 window and press F12 to auto-generate screenshots, then crop/resize.

---

## Testing Before Publish

- [ ] App installs on a 7"+ tablet
- [ ] Game renders in landscape correctly
- [ ] Controls work at tablet scale (wider paddle range)
- [ ] No UI elements cut off at edges
- [ ] Store and results screens render correctly at larger resolution
- [ ] Multi-window / split-screen does not crash

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| Android | 7.0 (API 24) |
| Screen | 7" minimum, 1024×600 |
| RAM | 2 GB |
| Storage | 100 MB |
