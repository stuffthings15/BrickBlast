# Publishing Guide — macOS ARM64 (Apple Silicon)

**Target:** macOS 12+ on Apple Silicon (M1/M2/M3/M4)  
**Artifact:** Shell launcher + bundled HTML5 game (DMG requires Mac build)  
**Status:** 🔒 NEEDS HOST — native .app/.dmg requires a Mac

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.sh` | macOS shell launcher |
| `BrickBlast.app/` | macOS app bundle skeleton |
| `game/` | HTML5 game assets |

---

## Build Native ARM64 App (On Mac)

```bash
# From electron-macos folder
cd "../electron-macos"
npm install
npm run build -- --mac --arm64
# Output: dist/BrickBlast-1.0.0-arm64.dmg
```

For a Universal binary that runs on both Intel and Apple Silicon:
```bash
npm run build -- --mac --universal
```

---

## Notarize for Gatekeeper

Apple Silicon Macs enforce Gatekeeper strictly. Unsigned apps cannot run without right-click → Open.

```bash
# Submit for notarization
xcrun notarytool submit dist/BrickBlast-1.0.0-arm64.dmg \
  --apple-id "your@email.com" \
  --team-id "TEAMID" \
  --password "app-specific-password" \
  --wait

# Staple the ticket
xcrun stapler staple dist/BrickBlast-1.0.0-arm64.dmg
```

---

## Publish to itch.io

```bash
butler push dist/BrickBlast-1.0.0-arm64.dmg teamfasttalk/brickblast:osx-arm64 --userversion 1.0.0
```

Or publish the universal DMG to the general `osx` channel to serve all Mac users from one artifact.

---

## Testing Before Publish

- [ ] Runs natively on M1/M2/M3 (Activity Monitor shows "Apple" architecture)
- [ ] No Rosetta 2 translation (native ARM64 binary)
- [ ] Game loads and plays at full performance
- [ ] Metal/GPU rendering works correctly

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| OS | macOS 12 Monterey |
| Chip | Apple Silicon (M1 or later) |
| RAM | 512 MB |
| Storage | 250 MB |
