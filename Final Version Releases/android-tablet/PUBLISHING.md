# Publishing Guide — Android Tablet (Google Play + APK)

**Target:** Android 7.0+ (API 24) tablets — 7" and larger  
**Package type:** Capacitor 6 native container wrapping canonical HTML5 game  
**Source:** `mobile/www/index.html` (must be the canonical ~65 KB version from `web/index.html`)

---

## Step 1 — Sync Canonical Game Source

```powershell
# From project root
Copy-Item "web\index.html"   "mobile\www\index.html"   -Force
Copy-Item "web\manifest.json" "mobile\www\manifest.json" -Force
Copy-Item "web\icons"        "mobile\www\icons"         -Recurse -Force
```

Verify: `mobile\www\index.html` should be **~65 KB**.

---

## Step 2 — Sync into Android Project

```bash
cd mobile
npm install
npx cap sync android
```

---

## Step 3 — Build Signed APK and AAB

### Option A — Android Studio
1. Open `mobile\android\` in Android Studio
2. **Build → Generate Signed Bundle / APK**
3. Choose **Android App Bundle** → use `mobile\brickblast-release.keystore`
4. Output: `mobile\android\app\build\outputs\bundle\release\app-release.aab`

### Option B — Command line
```bash
cd mobile/android
./gradlew bundleRelease assembleRelease
```

### Copy artifacts to this folder
```powershell
Copy-Item "mobile\android\app\build\outputs\bundle\release\app-release.aab" "versions\android-tablet\BrickBlast-release.aab" -Force
Copy-Item "mobile\android\app\build\outputs\apk\release\app-release.apk"    "versions\android-tablet\BrickBlast-release.apk" -Force
```

---

## Step 4 — Test Before Submission

- [ ] APK installs on Android 7.0+ tablet
- [ ] Launches in landscape; canvas fills screen correctly
- [ ] Store, music, power-ups, and save all work correctly
- [ ] Daily Challenge and Endless Mode accessible
- [ ] Touch controls work; back button exits cleanly
- [ ] Works fully offline

---

## Step 5 — Submit to Google Play

The tablet APK/AAB uses the **same package ID** as the phone version (`com.teamfasttalk.brickblast`).
Use the same Play Console submission — Google Play delivers the correct binary to phones and tablets
automatically. See `PLAY_STORE_GUIDE.md` for the full listing and submission checklist.

---

## Key Files

| File | Purpose |
|------|---------|
| `BrickBlast-release.aab` | Play Store upload artifact |
| `BrickBlast-release.apk` | Signed APK for sideloading |
| `BrickBlast-Android.apk` | Debug APK for testing only |
| `PLAY_STORE_GUIDE.md` | Store listing and submission walkthrough |
