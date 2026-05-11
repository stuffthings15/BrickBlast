# Publishing Guide — Windows ARM64

**Target:** Windows 10/11 ARM64 (Surface Pro X, Snapdragon-based PCs)  
**Artifact:** Self-contained .NET executable — no runtime install required  
**Status:** ✅ BUILT

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.exe` | Native ARM64 executable |
| `*.dll` | .NET runtime and app assemblies |
| `Assets/` | Game assets (images, audio) |

---

## Option 1 — itch.io

1. Install [Butler](https://itch.io/docs/butler/installing.html)
2. Log in: `butler login`
3. Zip this folder: `Compress-Archive -Path . -DestinationPath BrickBlast-Windows-ARM64.zip`
4. Push:
   ```
   butler push BrickBlast-Windows-ARM64.zip teamfasttalk/brickblast:windows-arm64 --userversion 1.0.0
   ```

---

## Option 2 — GitHub Releases

1. Zip this folder
2. Go to https://github.com/stuffthings15/BrickBlast/releases → Release `v1.0.0`
3. Upload as an additional asset alongside the x64 zip
4. Label clearly: `BrickBlast-Windows-ARM64.zip`

---

## Option 3 — Windows Store (MSIX)

The Windows Store MSIX in `../windows-store/` targets `x64`. For ARM64 Store submission, a separate MSIX must be built with target `arm64`. See `../windows-store/PUBLISHING.md`.

---

## Testing Before Publish

- [ ] Run on an ARM64 device (Surface Pro X, Snapdragon PC)
- [ ] Confirm no x64 emulation warning
- [ ] Full gameplay loop — menu → game → results → store
- [ ] Save/load persistence verified

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| OS | Windows 10 on ARM, version 1903+ |
| Architecture | ARM64 native |
| RAM | 512 MB |
| Storage | 150 MB |
| .NET runtime | Bundled (self-contained) |
