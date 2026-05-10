# BrickBlast — iPhone Release

## Overview
Native iPhone build via **Capacitor** and **Xcode** wrapping the HTML5 game.

## Prerequisites
- macOS 13+ with **Xcode 15+**
- Apple Developer account (paid, for App Store submission)
- Node.js 18+
- CocoaPods (`sudo gem install cocoapods`)

## Build Steps

```bash
cd "../mobile-capacitor"
npm install
npx cap sync ios
npx cap open ios
```

In Xcode:
1. Select the **BrickBlast** scheme and an **Any iOS Device** destination
2. **Product → Archive**
3. In Organizer → **Distribute App → App Store Connect**

## Target Specs
| Property | Value |
|----------|-------|
| Minimum iOS | 15.0 |
| Orientation | Landscape Left / Right |
| Form Factor | iPhone (4.7" – 6.7") |

## App Store Requirements
- App icon set: 1024×1024 (App Store) + all required sizes via Xcode asset catalog
- At least 3 iPhone screenshots (6.5" size class)
- Privacy manifest (`PrivacyInfo.xcprivacy`) — add if using any tracked APIs

## Notes
- `capacitor.config.json` — app ID and plugin settings
- `BUILD_GUIDE.md` — full walkthrough including TestFlight beta distribution
