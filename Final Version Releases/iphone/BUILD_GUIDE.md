# BrickBlast: Velocity Market — Mobile Build Guide
# Covers: Android Phone, Android Tablet, iPhone, iPad (iOS)
# Technology: Capacitor 6 wrapping the HTML5 canvas game
# ──────────────────────────────────────────────────────────────────────────────

## Prerequisites
| Tool                | Version  | Install                                         |
|---------------------|----------|-------------------------------------------------|
| Node.js             | ≥ 20     | https://nodejs.org                              |
| npm                 | ≥ 10     | bundled with Node                               |
| Android Studio      | Hedgehog+| https://developer.android.com/studio            |
| Xcode               | ≥ 15     | macOS App Store (macOS required for iOS)        |
| JDK                 | 17       | https://adoptium.net                            |
| CocoaPods           | latest   | `sudo gem install cocoapods`                    |
| @capacitor/cli      | 6        | installed via npm below                         |

## 1. Install dependencies
```bash
cd "Final Version Releases/mobile-capacitor"
npm install
```

## 2. Add platforms (first time only)
```bash
npx cap add android
npx cap add ios
```

## 3. Sync web assets to native projects
```bash
npx cap sync
```

## 4. Android Phone — debug build
```bash
npx cap open android
# In Android Studio: Run > Run 'app'  (select a phone AVD)
```

## 4a. Android Tablet — same project, different AVD
```
In Android Studio: AVD Manager > Create Virtual Device > Tablet > Pixel Tablet
Run > Run 'app'
```

## 4b. Android — release (Google Play Store)
```
In Android Studio: Build > Generate Signed Bundle/APK
  → Android App Bundle (.aab)  ← required by Play Store
Keystore: use the existing brickblast-release.keystore from the mobile/ folder
Alias:    brickblast-key
```

## 5. iPhone — debug build
```bash
npx cap open ios
# In Xcode: select "iPhone 15 Pro" simulator > ▶ Run
```

## 5a. iPad — same project, different simulator
```
In Xcode: select "iPad Pro (M4)" simulator > ▶ Run
```

## 5b. iOS — release (Apple App Store)
```
In Xcode:
  1. Set Team to your Apple Developer account (requires $99/year program)
  2. Product > Archive
  3. Distribute App > App Store Connect
  4. In App Store Connect (appstoreconnect.apple.com):
     - Create a new app record
     - Upload the archive
     - Fill metadata and submit for review
```

## 6. Google Play Store submission
1. Sign in to Google Play Console (https://play.google.com/console)
2. Create application > "BrickBlast: Velocity Market"
3. Upload the signed .aab from step 4b
4. Fill store listing, screenshots, and content rating
5. Submit for review

## 7. Apple App Store submission
- Done from Xcode Organizer after archiving (step 5b)
- App Store Connect review typically takes 24–48 hours

## Store listing notes
- App category:   Games > Arcade
- Target audience: Everyone (no ESRB/PEGI rating required for this content)
- IAPs:           None (coins are earned in-game, no real-money purchases)
- Privacy policy: Required — no personal data is collected beyond local save files
