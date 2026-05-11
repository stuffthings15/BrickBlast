# Publishing Guide — macOS (versions folder)

**Target:** macOS 12+ Intel  
**Status:** 🔒 NEEDS HOST — .app / DMG creation requires a Mac

---

## Quick Reference

For full instructions, see `../../Final Version Releases/macos/PUBLISHING.md`.

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `BrickBlast.sh` | Shell launcher (Python HTTP server + browser) |
| `BrickBlast.app/` | macOS app bundle skeleton |
| `game/` | HTML5 game assets |

---

## Run Locally (macOS)

```bash
chmod +x BrickBlast.sh
./BrickBlast.sh
```

---

## Create DMG (On Mac)

```bash
hdiutil create -volname "Brick Blast" -srcfolder BrickBlast.app \
  -ov -format UDZO BrickBlast-macOS-Intel.dmg
```

For a signed, notarized DMG, use the Electron build:
```bash
cd "../../Final Version Releases/electron-macos"
npm run build -- --mac --x64
```

---

## Publish

See `../../Final Version Releases/macos/PUBLISHING.md` for itch.io, GitHub Releases, and Mac App Store steps.
