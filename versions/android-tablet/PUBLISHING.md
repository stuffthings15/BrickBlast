# Publishing Guide — Android Tablet (versions folder)

**Target:** Android tablets 7"+ — Google Play Store  
**Status:** 🔒 NEEDS HOST — requires Android Studio

---

## Quick Reference

For full instructions, see `../../Final Version Releases/android-tablet/PUBLISHING.md`.

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `game/` | HTML5 game assets |
| `README.md` | Tablet platform notes |

The Android native project lives in `../mobile-capacitor/android/` and the same APK/AAB runs on both phone and tablet.

---

## Build

```bash
cd "../mobile-capacitor"
npm install
npx cap sync android
npx cap open android
```

Then: **Build → Build Bundle(s)/APK(s)** in Android Studio.

---

## Tablet Screenshots Required for Play Store

Upload screenshots at 7" (1024×600) and 10" (1280×800) in Play Console under the Tablet tab to qualify for the tablet optimization badge.

---

## Testing Before Publish

- [ ] App renders landscape on 7"+ tablet
- [ ] No UI elements clipped at edges
- [ ] Controls work at tablet scale
- [ ] Multi-window does not crash
