# Publishing Guide — Android Phone

**Target:** Android smartphones (portrait + landscape)  
**Artifact:** APK / AAB via Capacitor or React Native  
**Status:** 🔒 NEEDS HOST — requires Android Studio

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `game/` | HTML5 game assets |
| `README.md` | Platform notes |

The Android native project lives in `../mobile-capacitor/android/`.

---

## Build APK (Android Studio Required)

```bash
cd "../mobile-capacitor"
npm install
npx cap sync android
npx cap open android
```

In Android Studio → **Build → Build Bundle(s)/APK(s) → Build APK(s)**

Output: `android/app/build/outputs/apk/debug/app-debug.apk`

---

## Release Build (Signed)

```bash
cd "../mobile-capacitor/android"
./gradlew bundleRelease
# Output: app/build/outputs/bundle/release/app-release.aab
```

Configure signing key in `android/app/build.gradle`:
```groovy
signingConfigs {
    release {
        storeFile file("../brickblast.keystore")
        storePassword "yourpassword"
        keyAlias "brickblast"
        keyPassword "yourpassword"
    }
}
```

Generate keystore:
```bash
keytool -genkey -v -keystore brickblast.keystore -alias brickblast \
  -keyalg RSA -keysize 2048 -validity 10000
```

---

## Publish — Google Play Store

1. Go to https://play.google.com/console → **Create app**
2. **App name:** Brick Blast | **Category:** Games → Arcade
3. **Store listing:**
   - Short description (80 chars): from `docs/Submission/StoreListingCopy.md`
   - Full description (4000 chars): from `docs/Submission/StoreListingCopy.md`
   - Screenshots: upload from `docs/Screenshots/` (phone-sized screenshots needed)
   - Feature graphic: use `Assets/UI/titlecard.png` (resize to 1024×500)
4. Upload AAB to **Production** track
5. Complete content rating questionnaire (IARC)
6. Set price: Free
7. Submit — review takes 2–7 days

**Registration fee:** $25 one-time

---

## Publish — Direct APK (Sideload / itch.io)

```bash
butler push app-debug.apk teamfasttalk/brickblast:android --userversion 1.0.0
```

Or share APK directly — users enable "Install from unknown sources" in Android settings.

---

## Testing Before Publish

- [ ] APK installs on Android 7.0+ (API 24)
- [ ] App icon appears on home screen
- [ ] Game loads — no blank screen
- [ ] Touch drag moves paddle
- [ ] Tap launches ball
- [ ] Landscape orientation locks correctly
- [ ] Back button exits cleanly (no crash)
- [ ] Audio plays (if device not muted)

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| Android | 7.0 (API 24) |
| RAM | 2 GB |
| Storage | 100 MB |
| Screen | 360dp width minimum |
