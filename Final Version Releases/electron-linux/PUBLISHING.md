# Publishing Guide — Electron (Linux)

**Target:** Linux x64 and ARM64 — Electron desktop app wrapper  
**Artifact:** `.zip` archives (AppImage requires Linux host)  
**Status:** ✅ BUILT (zip distributables ready; AppImage requires Linux to package)

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `dist/BrickBlast-Linux-x64.zip` | Linux x64 portable zip (Git LFS) |
| `dist/BrickBlast-Linux-arm64.zip` | Linux ARM64 portable zip (Git LFS) |
| `main.js` | Electron main process |
| `package.json` | Electron Builder configuration |
| `game/` | HTML5 game assets |

---

## Rebuild (on Linux host)

```bash
cd "Final Version Releases/electron-linux"
npm install

# Build x64
npm run build -- --linux --x64

# Build ARM64
npm run build -- --linux --arm64
```

> **Note:** AppImage packaging requires `mksquashfs` which is only available on Linux. The zip fallback was used for the Windows build.

---

## Convert Zip to AppImage (on Linux)

If you have a Linux machine:
```bash
# Extract the zip
unzip BrickBlast-Linux-x64.zip -d BrickBlast-AppDir

# Use appimagetool
wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
chmod +x appimagetool-x86_64.AppImage
./appimagetool-x86_64.AppImage BrickBlast-AppDir BrickBlast-1.0.0.AppImage
```

---

## Option 1 — itch.io

The Butler script in `../itch.io/` references these files:
```bash
cd "../itch.io"
./push-itchio.sh
```

Or manually:
```bash
butler push ../linux-x64/BrickBlast-Linux-x64.zip teamfasttalk/brickblast:linux-x64 --userversion 1.0.0
butler push ../linux-arm64/BrickBlast-Linux-arm64.zip teamfasttalk/brickblast:linux-arm64 --userversion 1.0.0
```

---

## Option 2 — GitHub Releases

The zips are stored in Git LFS. Download them:
```bash
git lfs pull
```
Then attach to GitHub release `v1.0.0`.

---

## Testing Before Publish

- [ ] Extract zip on Linux — `BrickBlast` binary exists
- [ ] `chmod +x BrickBlast && ./BrickBlast` — app launches
- [ ] Game loads in Electron window
- [ ] Controls work (keyboard/mouse)
- [ ] App closes cleanly

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| OS | Ubuntu 20.04 / Debian 11 / any glibc 2.31+ distro |
| Architecture | x64 or ARM64 |
| RAM | 512 MB |
| Storage | 250 MB (extracted) |
| Display | X11 or Wayland |
