# Publishing Guide — React Native

**Target:** Android (APK/AAB) and iOS (IPA) via React Native  
**Artifact:** Native mobile app wrapping the HTML5 game via `react-native-webview`  
**Status:** 🔒 NEEDS HOST — requires Android Studio (Android) or Xcode on Mac (iOS)

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `package.json` | React Native project manifest |
| `App.js` | Main React Native component (WebView wrapper) |
| `android/` | Android native project (requires Android Studio) |
| `ios/` | iOS native project (requires Mac + Xcode) |
| `assets/` | Bundled HTML5 game assets |

---

## Prerequisites

### Android
- [Android Studio](https://developer.android.com/studio) with Android SDK 34
- Java 17 (bundled with Android Studio)
- USB debugging enabled on your device

### iOS
- Mac with [Xcode 15+](https://developer.apple.com/xcode/)
- Apple Developer account ($99/year for App Store, free for device testing)
- CocoaPods: `sudo gem install cocoapods`

---

## Build — Android

```bash
cd "Final Version Releases/react-native"
npm install

# Debug APK (for testing)
npx react-native run-android

# Release AAB (for Google Play)
cd android
./gradlew bundleRelease
# Output: android/app/build/outputs/bundle/release/app-release.aab
```

---

## Build — iOS

```bash
cd "Final Version Releases/react-native"
npm install
cd ios && pod install && cd ..

# Run on device
npx react-native run-ios --device

# Archive for App Store (Xcode)
# Open ios/BrickBlast.xcworkspace → Product → Archive
```

---

## Publish — Google Play Store

1. Sign the AAB:
   ```
   keytool -genkey -v -keystore brickblast.keystore -alias brickblast -keyalg RSA -keysize 2048 -validity 10000
   ```
2. Configure signing in `android/app/build.gradle`
3. Rebuild release AAB: `./gradlew bundleRelease`
4. Go to https://play.google.com/console
5. Create app → **Brick Blast** → fill listing
6. Upload AAB in **Production** → **Create new release**
7. Set version: `1.0` (versionCode: 1)

---

## Publish — Apple App Store

See `../ipad/PUBLISHING.md` for full iOS App Store steps — the process is identical for iPhone/iPad.

---

## Testing Before Publish

- [ ] App launches on Android emulator or device
- [ ] WebView loads the game
- [ ] Touch controls work (drag paddle, tap to launch)
- [ ] Game runs at acceptable framerate (30+ FPS)
- [ ] Back button handled gracefully (no crash)
