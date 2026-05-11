# Publishing Guide — Mobile Capacitor (Android + iOS)

**Target:** Android and iOS via Capacitor 6  
**Artifact:** Native mobile app wrapping the HTML5 game  
**Status:** 🔒 NEEDS HOST — Android requires Android Studio; iOS requires Mac + Xcode

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `package.json` | Capacitor project manifest |
| `capacitor.config.json` | Capacitor configuration |
| `www/` | Web assets synced into native projects |
| `android/` | Android native project |
| `ios/` | iOS native project |
| `BUILD_GUIDE.md` | Detailed build instructions |

---

## Prerequisites

### Android
- [Android Studio](https://developer.android.com/studio) (latest stable)
- Android SDK 34 (installed via Android Studio SDK Manager)
- Java 17
- A device or emulator with Android 7.0+ (API 24+)

### iOS
- Mac with Xcode 15+
- Apple Developer account (free for device testing; $99/year for App Store)
- CocoaPods: `sudo gem install cocoapods`

---

## Build — Android

```bash
cd "Final Version Releases/mobile-capacitor"
npm install

# Sync web assets into Android project
npx cap sync android

# Open in Android Studio
npx cap open android
```

In Android Studio:
1. Wait for Gradle sync to complete
2. Select **Run → Run 'app'** or build APK via **Build → Build Bundle(s)/APK(s)**

**Release APK/AAB:**
```bash
cd android
./gradlew bundleRelease    # AAB for Play Store
./gradlew assembleRelease  # APK for direct distribution
```

---

## Build — iOS

```bash
cd "Final Version Releases/mobile-capacitor"
npm install

# Sync web assets
npx cap sync ios

# Open in Xcode
npx cap open ios
```

In Xcode: select your device → Product → Run, or Product → Archive for distribution.

---

## Publish — Google Play Store

1. Sign the AAB (see `android/app/build.gradle` signing config)
2. Go to https://play.google.com/console
3. Create app: **Brick Blast** | **Games** | **Arcade**
4. Complete store listing (description from `docs/Submission/StoreListingCopy.md`)
5. Upload screenshots from `docs/Screenshots/`
6. Upload AAB in **Production** track
7. Submit for review (2–7 days)

**Registration:** $25 one-time fee

---

## Publish — Apple App Store

See `../../versions/ipad/APP_STORE_GUIDE.md` for full steps.

**Registration:** $99/year Apple Developer Program

---

## Testing Before Publish

- [ ] App installs on Android device (API 24+)
- [ ] App installs on iOS device (iOS 13+)
- [ ] Game loads in WebView
- [ ] Touch controls work (drag paddle, tap to launch)
- [ ] Back button handled on Android
- [ ] Portrait/landscape orientation handled
- [ ] Splash screen displays correctly
