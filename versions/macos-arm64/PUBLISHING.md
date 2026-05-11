# Publishing Guide — macOS ARM64 / Apple Silicon (versions folder)

**Target:** macOS 12+ Apple Silicon (M1/M2/M3)  
**Status:** 🔒 NEEDS HOST — DMG creation and notarization require a Mac

---

## Quick Reference

For full instructions, see `../../Final Version Releases/macos-arm64/PUBLISHING.md`.

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `BrickBlast.sh` | Shell launcher (Python HTTP server + browser) |
| `BrickBlast.app/` | macOS app bundle skeleton |
| `game/` | HTML5 game assets |

---

## Run Locally (Apple Silicon Mac)

```bash
chmod +x BrickBlast.sh
./BrickBlast.sh
```

---

## Build ARM64 DMG (On Mac)

```bash
cd "../../Final Version Releases/electron-macos"
npm install
npm run build -- --mac --arm64
# Output: dist/BrickBlast-1.0.0-arm64.dmg
```

For a universal binary (Intel + ARM64):
```bash
npm run build -- --mac --universal
```

---

## Publish

| Channel | Command |
|---------|---------|
| itch.io | `butler push BrickBlast-arm64.dmg teamfasttalk/brickblast:osx-arm64 --userversion 1.0.0` |
| GitHub Releases | Upload DMG as a release asset |

---

## Testing Before Publish

- [ ] App launches natively on M-series Mac (no Rosetta)
- [ ] Game renders at full resolution
- [ ] No Gatekeeper block after notarization
- [ ] DMG mounts and app installs to Applications
