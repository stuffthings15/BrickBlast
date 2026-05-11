# Publishing Guide — macOS (Intel x64)

**Target:** macOS 12+ on Intel Macs  
**Artifact:** Shell launcher + bundled HTML5 game (DMG requires Mac build)  
**Status:** 🔒 NEEDS HOST — native .app/.dmg requires a Mac

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.sh` | macOS shell launcher (browser fallback) |
| `BrickBlast.app/` | macOS app bundle skeleton |
| `BrickBlast.app/Contents/Info.plist` | App bundle metadata |
| `game/` | HTML5 game assets |

---

## Option A — Shell Launcher (No Mac Required for Prep)

The `BrickBlast.sh` launcher serves the game via Python and opens Safari/Chrome. Users run:

```bash
chmod +x BrickBlast.sh
./BrickBlast.sh
```

Distribute as a `.zip` — users extract and run.

---

## Option B — Native .app Bundle (Requires Mac)

On a Mac, convert the launcher to a proper `.app`:

```bash
# Make executable
chmod +x BrickBlast.app/Contents/MacOS/BrickBlast

# Create a DMG
hdiutil create -volname "Brick Blast" -srcfolder BrickBlast.app \
  -ov -format UDZO BrickBlast-macOS-Intel.dmg
```

---

## Option C — Electron DMG (Recommended — Requires Mac)

Build from `../electron-macos/`:
```bash
cd "../electron-macos"
npm install
npm run build -- --mac --x64
# Output: dist/BrickBlast-1.0.0.dmg
```

See `../electron-macos/PUBLISHING.md` for signing and notarization.

---

## Publish to itch.io

```bash
butler push BrickBlast-macOS-Intel.dmg teamfasttalk/brickblast:osx --userversion 1.0.0
```

---

## Publish to Mac App Store

Requires:
- Apple Developer Program ($99/year)
- Mac with Xcode 15+
- App Sandbox entitlements
- Review by Apple (3–14 days)

See [Apple's submission guide](https://developer.apple.com/distribute/).

---

## Testing Before Publish

- [ ] App launches on macOS 12 (Monterey) Intel
- [ ] No Gatekeeper block (signed) or right-click → Open works (unsigned)
- [ ] Game loads in browser/Electron window
- [ ] Cmd+Q quits cleanly
