# Brick Blast — Google Play Store Submission Guide

## Artifacts
| File | Purpose |
|------|---------|
| `BrickBlast-release.aab` | **Upload this to Play Console** (Android App Bundle — required for Play Store) |
| `BrickBlast-release.apk` | Signed APK (for direct sideload / testing only) |
| `BrickBlast-Android.apk` | Debug APK (testing only — do NOT submit to Play) |

## Step-by-Step: Submit to Google Play

### 1. Create a Google Play Developer account
- Go to https://play.google.com/console
- Sign in → pay one-time **$25 USD** registration fee
- Complete identity verification (takes 1-2 days)

### 2. Create the app
- Play Console → **"Create app"**
- App name: **Brick Blast**
- Default language: **English (United States)**
- App or game: **Game**
- Free or paid: **Free**
- Accept policies → **"Create app"**

### 3. Set up store listing
**Main store listing:**
| Field | Value |
|-------|-------|
| App name | Brick Blast |
| Short description (80 chars) | Fast-paced brick-breaking arcade. Smash bricks, grab power-ups, top the score! |
| Full description (4000 chars) | Brick Blast is a high-energy brick-breaking arcade game. Destroy colorful bricks with a bouncing ball, snag power-ups (multi-ball, fire ball, slow-mo, paddle grow/shrink, extra life, mega ball), and dominate the leaderboard. Compete for the top score across 60 action-packed levels. Features: 6 original music tracks, smooth 60fps gameplay, landscape orientation, gamepad support, and multiple ball types. No ads. No in-app purchases. Pure arcade action. |
| App icon | Use `assets/icons/icon-512.png` (512×512 PNG, no alpha on corners) |
| Feature graphic | 1024×500 JPG/PNG — create a banner showing gameplay |
| Screenshots (phone) | Min 2, max 8 — 16:9 landscape at 1280×720 or 1920×1080 |

### 4. Content rating
- Dashboard → **"App content"** → **"Content rating"**
- Complete the questionnaire:
  - Category: **Games**
  - Violence: **None**
  - Sexual content: **None**
  - Gambling: **None**
  → Expected rating: **ESRB Everyone / PEGI 3**

### 5. Target audience
- App content → **"Target audience"** → select **"13 and over"** (or "All ages" if confirmed appropriate)

### 6. Upload the AAB
- **"Releases"** → **"Production"** → **"Create new release"**
- Upload `BrickBlast-release.aab`
- Release name: `1.0` — Release notes: `Initial release`

### 7. App details
| Setting | Value |
|---------|-------|
| Package name | `com.teamfasttalk.brickblast` |
| Version code | `1` |
| Version name | `1.0` |
| Min Android | 5.1 (API 22) |
| Target SDK | 34 |

### 8. Review and rollout
- Complete all Dashboard checklist items (green checkmarks)
- Click **"Send changes for review"**
- Initial review: **3-7 days** for new apps

## Rebuilding the AAB
If you make code changes, rebuild from the `mobile/` folder:
```bat
cd mobile
npm run build
cd android
.\gradlew bundleRelease
```
Signed AAB will be at:
`mobile\android\app\build\outputs\bundle\release\app-release.aab`
Copy to `versions\android-phone\BrickBlast-release.aab`
