# Publishing Guide — Linux x64

**Target:** Linux x64 (Ubuntu, Debian, Fedora, Arch, etc.)  
**Artifact:** Shell launcher + bundled HTML5 game (browser-based)  
**Status:** ✅ READY

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.sh` | Native launcher script |
| `BrickBlast-Linux-x64.zip` | Complete distributable zip (Git LFS) |
| `BrickBlast.desktop` | Desktop integration file |
| `game/` | HTML5 game assets served locally |

---

## How the Launcher Works

`BrickBlast.sh` checks for a prebuilt Electron AppImage first. If not found, it serves `game/` on `localhost:7777` via Python's HTTP server and opens the URL with `xdg-open` (your default browser).

---

## Option 1 — itch.io (Recommended)

```bash
butler login
butler push BrickBlast-Linux-x64.zip teamfasttalk/brickblast:linux-x64 --userversion 1.0.0
```

The zip is already in Git LFS — pull it first if missing:
```bash
git lfs pull
```

---

## Option 2 — GitHub Releases

Attach `BrickBlast-Linux-x64.zip` to release `v1.0.0` on GitHub.  
> The file is stored in Git LFS. GitHub Releases serves LFS files correctly.

---

## Option 3 — Direct Tarball

```bash
tar -czf BrickBlast-Linux-x64.tar.gz BrickBlast.sh BrickBlast.desktop game/ icons/
```
Host on any static server or share directly.

---

## Desktop Integration (Optional)

```bash
# Install desktop entry so app appears in application launcher
cp BrickBlast.desktop ~/.local/share/applications/
update-desktop-database ~/.local/share/applications/
```

---

## User Installation Steps

```bash
unzip BrickBlast-Linux-x64.zip
cd BrickBlast-Linux-x64
chmod +x BrickBlast.sh
./BrickBlast.sh
```

---

## Testing Before Publish

- [ ] `./BrickBlast.sh` launches on Ubuntu 20.04
- [ ] Browser opens to `localhost:7777` with game
- [ ] Game plays correctly
- [ ] Script exits cleanly when browser is closed
- [ ] Desktop integration file installs correctly

---

## System Requirements

| Requirement | Minimum |
|-------------|---------|
| OS | Ubuntu 20.04 / Debian 11 or equivalent |
| Architecture | x86_64 |
| Dependencies | Python 3 + `xdg-open` (standard on most distros) |
| Browser | Any modern browser (Chrome, Firefox, Chromium) |
