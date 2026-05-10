# BrickBlast — Linux ARM64 Release

Launcher-based Linux release (AArch64 / Raspberry Pi 4+, NVIDIA Jetson, etc.).

## Files
| File | Purpose |
|------|---------|
| `BrickBlast.sh` | Main launcher script |
| `BrickBlast.desktop` | XDG desktop integration entry |
| `www/` | HTML5 game assets |

## Running

```bash
chmod +x BrickBlast.sh
./BrickBlast.sh
```

## Desktop Integration

```bash
cp BrickBlast.desktop ~/.local/share/applications/
update-desktop-database ~/.local/share/applications/
```

## Tested On
- Raspberry Pi 4 / 5 (64-bit Raspberry Pi OS)
- NVIDIA Jetson Orin (Ubuntu 22.04 ARM64)
- AWS Graviton EC2 instances with desktop environment
