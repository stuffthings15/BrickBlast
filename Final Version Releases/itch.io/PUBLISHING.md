# Publishing Guide — itch.io

**Target:** itch.io game page — all platforms  
**Artifact:** Butler push script + per-platform distributables  
**Status:** ✅ READY (Butler must be installed on your machine)

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `push-itchio.sh` | Automated Butler upload script |
| `index.html` | HTML5 game (direct itch.io browser play) |
| `BrickBlast-Windows-Setup.exe` | Windows installer (Git LFS) |
| `README.md` | itch.io folder documentation |

Linux zips are in `../linux-x64/` and `../linux-arm64/` (referenced by the script).

---

## Step 1 — Install Butler

Butler is itch.io's command-line publishing tool.

**Windows:**
```powershell
# Download from https://itch.io/docs/butler/installing.html
# Or via Scoop:
scoop install butler
```

**macOS/Linux:**
```bash
curl -L -o butler.zip https://broth.itch.ovh/butler/linux-amd64/LATEST/archive/default
unzip butler.zip
chmod +x butler
sudo mv butler /usr/local/bin/
```

---

## Step 2 — Log In

```bash
butler login
# Opens browser — sign in to your itch.io account
```

---

## Step 3 — Create the itch.io Page (First Time)

1. Go to https://itch.io/game/new
2. Fill in:
   | Field | Value |
   |-------|-------|
   | Title | Brick Blast |
   | URL slug | `brickblast` |
   | Kind | HTML (for browser play) |
   | Classification | Games |
   | Genre | Arcade |
3. Paste description from `docs/Submission/StoreListingCopy.md`
4. Upload screenshots from `docs/Screenshots/`
5. Set price (Free or Pay What You Want)
6. Save as **Draft** first

---

## Step 4 — Push All Channels

```bash
cd "Final Version Releases/itch.io"
chmod +x push-itchio.sh
./push-itchio.sh
```

This pushes:
- `html` channel — browser playable
- `windows` channel — Windows installer
- `linux-x64` channel — Linux x64 zip
- `linux-arm64` channel — Linux ARM64 zip

For macOS, build a DMG on Mac first, then:
```bash
butler push BrickBlast-macOS.dmg teamfasttalk/brickblast:osx --userversion 1.0.0
```

---

## Step 5 — Publish

On your itch.io dashboard:
1. Open the game → **Edit**
2. Set **Visibility** to **Public**
3. Check **This game can be played in the browser** for the HTML channel
4. Save → **Publish**

---

## Updating (Future Releases)

Increment the version in `push-itchio.sh` and re-run:
```bash
--userversion 1.0.1
```

itch.io automatically notifies existing players of updates.

---

## Testing Before Publish

- [ ] `butler login` succeeds
- [ ] `push-itchio.sh` completes with no errors
- [ ] itch.io dashboard shows all channels with correct file sizes
- [ ] Browser play works on the itch.io page
- [ ] Windows download installs and runs correctly
