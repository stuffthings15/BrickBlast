# BrickBlast — Linux x64 Release

Launcher-based Linux release (x86_64).

## Files
| File | Purpose |
|------|---------|
| `BrickBlast.sh` | Main launcher script |
| `BrickBlast.desktop` | XDG desktop integration entry |
| `www/` | HTML5 game assets served by the launcher |

## Running

```bash
chmod +x BrickBlast.sh
./BrickBlast.sh
```

The script opens the game in the system default browser. For a kiosk-style launch, install Electron then run from the `electron-linux` release instead.

## Desktop Integration

```bash
cp BrickBlast.desktop ~/.local/share/applications/
update-desktop-database ~/.local/share/applications/
```

## Requirements
- Any modern Linux distro (Ubuntu 20.04+, Fedora 36+, Arch)
- Chromium, Firefox, or any browser supporting Canvas / Web Audio API
- `.NET 10` runtime is **not** required for this wrapper release
