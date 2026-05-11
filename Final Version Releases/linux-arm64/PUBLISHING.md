# Publishing Guide — Linux ARM64

**Target:** Linux ARM64 (Raspberry Pi 4+, Ampere, AWS Graviton, etc.)  
**Artifact:** Shell launcher + bundled HTML5 game  
**Status:** ✅ READY

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.sh` | Native launcher script |
| `BrickBlast-Linux-arm64.zip` | Complete distributable zip (Git LFS) |
| `BrickBlast.desktop` | Desktop integration file |
| `game/` | HTML5 game assets |

---

## How the Launcher Works

Same as the x64 launcher: checks for an Electron AppImage, falls back to Python HTTP server on `localhost:7777` + `xdg-open`.

---

## Option 1 — itch.io

```bash
butler login
butler push BrickBlast-Linux-arm64.zip teamfasttalk/brickblast:linux-arm64 --userversion 1.0.0
```

Pull from LFS first if needed:
```bash
git lfs pull
```

---

## Option 2 — GitHub Releases

Attach `BrickBlast-Linux-arm64.zip` to release `v1.0.0`.

---

## User Installation Steps

```bash
unzip BrickBlast-Linux-arm64.zip
cd BrickBlast-Linux-arm64
chmod +x BrickBlast.sh
./BrickBlast.sh
```

---

## Testing Before Publish

- [ ] `./BrickBlast.sh` launches on Raspberry Pi OS (64-bit) or Ubuntu ARM
- [ ] Browser opens game at `localhost:7777`
- [ ] Touch controls work on touchscreen devices
- [ ] Performance acceptable (30+ FPS) on Raspberry Pi 4

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| OS | Ubuntu 22.04 ARM / Raspberry Pi OS 64-bit |
| Architecture | aarch64 (ARM64) |
| Dependencies | Python 3 + `xdg-open` |
| Hardware | Raspberry Pi 4 (2 GB RAM+) or equivalent |
