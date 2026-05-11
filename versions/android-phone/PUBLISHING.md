# Publishing Guide — Android Phone (versions folder)

**Target:** Android smartphones — Google Play Store + direct APK  
**Status:** 🔒 NEEDS HOST — requires Android Studio

---

## Quick Reference

For full instructions, see `../../Final Version Releases/android-phone/PUBLISHING.md`.

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `game/` | HTML5 game assets |
| `README.md` | Platform notes |

The Android native project lives in `../mobile-capacitor/android/`.

---

## Build APK

```bash
cd "../mobile-capacitor"
npm install
npx cap sync android
npx cap open android
```

In Android Studio → **Build → Build Bundle(s)/APK(s) → Build APK(s)**

---

## Publish to Google Play

1. Build signed AAB: `./gradlew bundleRelease`
2. Go to https://play.google.com/console
3. Upload AAB to **Production** track
4. Submit for review (2–7 days)

**Fee:** $25 one-time registration

---

## Testing Before Publish

- [ ] Installs on Android 7.0+ (API 24)
- [ ] Game loads in WebView
- [ ] Touch drag moves paddle
- [ ] Tap launches ball
- [ ] Back button exits cleanly
