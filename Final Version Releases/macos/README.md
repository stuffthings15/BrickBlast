# BrickBlast — macOS Release (Intel x64)

Launcher-based macOS release for Intel Macs.

## Files
| File | Purpose |
|------|---------|
| `BrickBlast.app/` | macOS application bundle |
| `BrickBlast.app/Contents/MacOS/BrickBlast` | Launch script |
| `BrickBlast.app/Contents/Info.plist` | Bundle metadata |
| `BrickBlast.app/Contents/Resources/www/` | HTML5 game assets |

## Running

Double-click **BrickBlast.app** in Finder, or:

```bash
open BrickBlast.app
```

If Gatekeeper blocks it (unsigned app):

```bash
xattr -d com.apple.quarantine BrickBlast.app
```

## Distribution
- Drag `BrickBlast.app` into a disk image with `hdiutil create` for a `.dmg`
- For Mac App Store submission, notarize with `xcrun notarytool`

## Requirements
- macOS 12 Monterey or later (Intel)
- Safari, Chrome, or any Chromium browser (opened by the launcher)
