# Publishing Guide — Electron (macOS)

**Target:** macOS 12+ (Intel x64 and Apple Silicon ARM64)  
**Artifact:** `.dmg` installer and `.zip` app bundle  
**Status:** 🔒 NEEDS HOST — macOS build must be performed on a Mac

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `main.js` | Electron main process |
| `package.json` | Electron Builder config (mac target configured) |
| `game/` | HTML5 game assets |
| `dist/` | Output folder (empty until built on Mac) |

---

## Build on Mac

```bash
cd "Final Version Releases/electron-macos"
npm install

# Intel Mac (x64)
npm run build -- --mac --x64

# Apple Silicon (arm64)
npm run build -- --mac --arm64

# Universal binary (both architectures)
npm run build -- --mac --universal
```

Output appears in `dist/`:
- `BrickBlast-1.0.0.dmg` — installer
- `BrickBlast-1.0.0-mac.zip` — portable app bundle

---

## Code Signing (Required for Gatekeeper)

Unsigned macOS apps are blocked by Gatekeeper. For distribution:
1. Enroll in [Apple Developer Program](https://developer.apple.com) ($99/year)
2. Create a **Developer ID Application** certificate in Keychain Access
3. Configure in `package.json`:
   ```json
   "mac": {
     "identity": "Developer ID Application: Team Fast Talk (TEAMID)"
   }
   ```
4. After build, notarize:
   ```bash
   xcrun notarytool submit dist/BrickBlast-1.0.0.dmg \
     --apple-id "your@email.com" \
     --team-id "TEAMID" \
     --password "app-specific-password" \
     --wait
   xcrun stapler staple dist/BrickBlast-1.0.0.dmg
   ```

---

## Option 1 — itch.io

```bash
butler push dist/BrickBlast-1.0.0.dmg teamfasttalk/brickblast:osx --userversion 1.0.0
```

---

## Option 2 — GitHub Releases

Attach `dist/BrickBlast-1.0.0.dmg` and `dist/BrickBlast-1.0.0-mac.zip` to release `v1.0.0`.

---

## Option 3 — Mac App Store

Mac App Store requires a separate Xcode-based submission flow using `xcodebuild archive`. The Electron app can be submitted but requires additional entitlements for sandboxing.

---

## Testing Before Publish

- [ ] DMG mounts and app drags to Applications
- [ ] App launches without Gatekeeper warning (signed) or with right-click → Open (unsigned)
- [ ] Game loads and plays
- [ ] Cmd+Q quits cleanly
- [ ] App runs on both Intel and Apple Silicon (if universal build)
