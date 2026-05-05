# Brick Blast — Apple App Store Submission Guide

## ⚠️ REQUIREMENT: A Mac with Xcode is required to submit to the App Store
iOS apps MUST be built and submitted from macOS. These steps require a Mac.

## Artifacts in this folder
| Path | Purpose |
|------|---------|
| `xcode-project/` | Complete Xcode/Capacitor project — open this on your Mac |
| `icons/` | App icons in all required sizes |
| `BUILD_IOS.sh` | Automated build script for Mac |

## Step-by-Step: Submit to Apple App Store

### 1. Prerequisites (do once)
- **Mac** running macOS 13+ (Ventura or later recommended)
- **Xcode 15+** — install free from the Mac App Store
- **Apple Developer account** — https://developer.apple.com — **$99/year**
- **CocoaPods** — run: `sudo gem install cocoapods`

### 2. Enroll in Apple Developer Program
- Go to https://developer.apple.com/account
- Sign in → **"Enroll"** → Individual or Organization ($99/year)
- Verification takes 24-48 hours

### 3. Create App ID in Apple Developer Portal
- https://developer.apple.com → Certificates, IDs & Profiles → **"Identifiers"**
- **"+"** → **"App IDs"** → **"App"**
- Bundle ID: `com.teamfasttalk.brickblast` (Explicit)
- Capabilities: check **"Game Center"** (optional)

### 4. Create the app in App Store Connect
- Go to https://appstoreconnect.apple.com
- **"My Apps"** → **"+"** → **"New App"**
| Field | Value |
|-------|-------|
| Platform | iOS |
| Name | Brick Blast |
| Primary language | English (U.S.) |
| Bundle ID | `com.teamfasttalk.brickblast` |
| SKU | `brickblast-ios-001` |

### 5. Transfer xcode-project to Mac and build

On your Mac, from Terminal:
```bash
# Copy xcode-project folder to Mac (USB, AirDrop, or git)
cd path/to/xcode-project/App
pod install
open App.xcworkspace
```

In Xcode:
- Select the **App** target → **"Signing & Capabilities"**
- Set your **Team** to your Apple Developer account
- Set **Bundle Identifier** to `com.teamfasttalk.brickblast`
- Set **Version** to `1.0`, **Build** to `1`

### 6. Archive and upload
In Xcode:
1. Set scheme destination to **"Any iOS Device (arm64)"**
2. **Product → Archive**
3. In the Organizer window → **"Distribute App"**
4. Choose **"App Store Connect"** → **"Upload"**
5. Follow the wizard — Xcode handles signing automatically

### 7. Complete App Store listing
In App Store Connect → your app → **"App Store"** tab:

| Field | Value |
|-------|-------|
| App name | Brick Blast |
| Subtitle | Classic Arcade Brick Breaker |
| Description | Fast-paced brick-breaking arcade action. Destroy colorful bricks, collect power-ups, and compete for the top score. Features 6 original music tracks, 60fps gameplay, and landscape-only orientation. No ads. No in-app purchases. |
| Keywords (100 chars) | brick,breaker,arcade,ball,paddle,breakout,classic,retro,game,action |
| Support URL | https://github.com/stuffthings15/BrickBlast |
| Category | Games → Action |
| Content Rights | You own or have rights to all content |

**Age Rating:** Complete the questionnaire → expected **4+**

**Screenshots required (iPhone):**
- 6.9" display: 1320×2868 or 1290×2796 px (landscape: 2796×1290)
- 6.5" display: 1242×2688 or 1284×2778 px
- At least 3 screenshots minimum

**Screenshots required (iPad):**
- 12.9" display: 2048×2732 px (landscape: 2732×2048)

### 8. Submit for review
- Select the uploaded build under **"Build"** section
- Complete all required fields
- Click **"Add for Review"** → **"Submit to App Review"**
- First review: **1-3 days** (can be longer)

## App Store Rejection Prevention Checklist
- ✅ Landscape orientation only — declared in Info.plist
- ✅ No private API usage
- ✅ No login required to play core functionality
- ✅ App works offline (no internet required to play)
- ✅ Accurate screenshots (must match actual app UI)
- ✅ Privacy policy URL if collecting any user data
- ✅ App icon has no transparency/alpha channel on corners
