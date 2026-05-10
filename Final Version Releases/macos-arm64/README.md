# BrickBlast — macOS ARM64 Release (Apple Silicon)

Launcher-based macOS release for M1/M2/M3/M4 Macs.

## Files
| File | Purpose |
|------|---------|
| `BrickBlast.app/` | macOS application bundle |
| `BrickBlast.app/Contents/MacOS/BrickBlast` | Launch script |
| `BrickBlast.app/Contents/Info.plist` | Bundle metadata |
| `BrickBlast.app/Contents/Resources/www/` | HTML5 game assets |

## Running

```bash
open BrickBlast.app
# or, if Gatekeeper blocks:
xattr -d com.apple.quarantine BrickBlast.app
```

## Distribution
Identical to the Intel release — create a universal `.dmg` covering both architectures with `lipo` if needed.

## Requirements
- macOS 12 Monterey or later (Apple Silicon)
- Any Chromium-based or WebKit browser
