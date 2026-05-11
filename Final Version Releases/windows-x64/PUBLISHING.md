# Publishing Guide — Windows x64

**Target:** Windows 10/11 x64 (64-bit Intel/AMD)  
**Artifact:** Self-contained .NET executable — no runtime install required  
**Status:** ✅ BUILT

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.exe` | Main executable |
| `*.dll` | .NET runtime and app assemblies |
| `Assets/` | Game assets (images, audio) |
| `appsettings.json` | Runtime configuration |

---

## Option 1 — itch.io (Recommended for Public Release)

1. Install [Butler](https://itch.io/docs/butler/installing.html)
2. Log in: `butler login`
3. Zip this folder: `Compress-Archive -Path . -DestinationPath BrickBlast-Windows-x64.zip`
4. Push:
   ```
   butler push BrickBlast-Windows-x64.zip teamfasttalk/brickblast:windows --userversion 1.0.0
   ```
5. Go to https://teamfasttalk.itch.io/brickblast → Edit → mark channel as **Windows**

---

## Option 2 — Direct Distribution (GitHub Releases)

1. Zip this folder
2. Go to https://github.com/stuffthings15/BrickBlast/releases
3. Click **Draft a new release** → tag `v1.0.0`
4. Upload the zip as a release asset
5. Publish

---

## Option 3 — Windows Store (MSIX)

See `../windows-store/PUBLISHING.md` for full MSIX packaging and Store submission steps.

---

## Testing Before Publish

- [ ] Launch `BrickBlast.exe` — main menu appears
- [ ] Start a game — ball launches, bricks break
- [ ] Complete a level — results screen appears
- [ ] Open Store — items display, purchase/equip works
- [ ] Close and relaunch — purchased items persist
- [ ] Check no console error window appears

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| OS | Windows 10 version 1903 or later |
| Architecture | x64 (64-bit) |
| RAM | 512 MB |
| Storage | 150 MB |
| .NET runtime | Bundled (self-contained) |
