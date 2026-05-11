# Publishing Guide — Android Phone (Google Play + APK)

**Target:** Android 7.0+ (API 24) smartphones  
**Package type:** Capacitor 6 native container wrapping canonical HTML5 game  
**Source:** `mobile/www/index.html` (must be the canonical 65 KB version from `web/index.html`)

---

## Step 1 — Sync Canonical Game Source into Mobile Project

The `mobile/` folder contains the Capacitor Android project. The game files live in `mobile/www/`.

```powershell
# From project root — copy canonical HTML5 source into mobile www/
Copy-Item "web\index.html" "mobile\www\index.html" -Force
Copy-Item "web\manifest.json" "mobile\www\manifest.json" -Force
Copy-Item "web\icons" "mobile\www\icons" -Recurse -Force
```

Verify: `mobile\www\index.html` should be **~65 KB** (the canonical version with Store, music, and all upgrades).

---

## Step 2 — Sync Assets into Android Project

```bash
cd mobile
npm install
npx cap sync android
```

---

## Step 3 — Build Signed APK and AAB

### Option A — Android Studio (recommended)
1. Open Android Studio → **Open** → select `mobile\android\`
2. **Build → Generate Signed Bundle / APK**
3. Choose **Android App Bundle** → use `brickblast-release.keystore` in `mobile\`
4. Output: `mobile\android\app\build\outputs\bundle\release\app-release.aab`

### Option B — Command line
```bash
cd mobile/android
./gradlew bundleRelease assembleRelease
```

### Copy artifacts
```powershell
Copy-Item "mobile\android\app\build\outputs\bundle\release\app-release.aab" "versions\android-phone\BrickBlast-release.aab" -Force
Copy-Item "mobile\android\app\build\outputs\apk\release\app-release.apk" "versions\android-phone\BrickBlast-release.apk" -Force
```

---

## Step 4 — Test Before Submission

- [ ] `BrickBlast-release.apk` installs on Android 7.0+ phone
- [ ] App launches in landscape orientation
- [ ] Store opens, coins earn, skins equip, and persists after restart
- [ ] All 10 music tracks play
- [ ] All 5 SFX packs function
- [ ] Power-ups drop and activate
- [ ] Daily Challenge and Endless Mode accessible
- [ ] Touch drag controls paddle, tap starts ball
- [ ] Back button exits cleanly (no crash)
- [ ] Works fully offline (no internet required)

---

## Step 5 — Submit to Google Play

See `PLAY_STORE_GUIDE.md` for the full step-by-step Play Console submission process.

**Quick reference:**
1. Go to https://play.google.com/console
2. Create app → complete store listing with the content from `PLAY_STORE_GUIDE.md`
3. Production → Create release → upload `BrickBlast-release.aab`
4. Complete all checklist items → Send for review (3–7 days for new apps)

**Play Console registration:** one-time **$25 USD**

---

## Key Files

| File | Purpose |
|------|---------|
| `BrickBlast-release.aab` | Play Store upload artifact |
| `BrickBlast-release.apk` | Signed APK for sideloading / beta testing |
| `BrickBlast-Android.apk` | Debug APK for development testing only |
| `PLAY_STORE_GUIDE.md` | Full store listing copy and submission walkthrough |

---

## Notes
- The keystore is at `mobile\brickblast-release.keystore` — **never commit this to a public repo**.
- Package name: `com.teamfasttalk.brickblast`
- Min SDK: 24 (Android 7.0) | Target SDK: 34
