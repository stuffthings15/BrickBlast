# Publishing Guide — Linux (versions folder)

**Target:** Linux x64 and ARM64 — direct distribution  
**Status:** ✅ READY

---

## Quick Reference

For full instructions see:
- `../../Final Version Releases/linux-x64/PUBLISHING.md`
- `../../Final Version Releases/linux-arm64/PUBLISHING.md`

---

## Run Locally

```bash
chmod +x BrickBlast.sh
./BrickBlast.sh
```

---

## Distribute

| Channel | Command |
|---------|---------|
| itch.io (x64) | `butler push BrickBlast-Linux-x64.zip teamfasttalk/brickblast:linux --userversion 1.0.0` |
| itch.io (ARM64) | `butler push BrickBlast-Linux-arm64.zip teamfasttalk/brickblast:linux-arm64 --userversion 1.0.0` |
| GitHub Releases | Upload zips as release assets |

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `BrickBlast.sh` | Shell launcher |
| `BrickBlast.desktop` | Desktop integration file |
| `game/` | HTML5 game assets |
| `icons/` | App icons |
