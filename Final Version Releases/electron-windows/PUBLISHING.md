# Publishing Guide — Electron (Windows)

**Target:** Windows 10/11 — Electron desktop app wrapper  
**Artifact:** Windows installer `.exe` and portable `.zip`  
**Status:** ✅ BUILT

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `dist/` | Built installers and zips |
| `dist/*.exe` | NSIS Windows installer |
| `dist/*.zip` | Portable zip (no install needed) |
| `main.js` | Electron main process |
| `package.json` | Electron Builder configuration |
| `game/` | HTML5 game assets served by Electron |

---

## Rebuild (if source changed)

```bash
cd "Final Version Releases/electron-windows"
npm install
npm run build
# Output appears in dist/
```

---

## Option 1 — itch.io

```bash
butler login
butler push dist/BrickBlast-Setup-1.0.0.exe teamfasttalk/brickblast:windows-electron --userversion 1.0.0
```

---

## Option 2 — GitHub Releases

1. Go to https://github.com/stuffthings15/BrickBlast/releases
2. Edit release `v1.0.0`
3. Upload `dist/BrickBlast-Setup-1.0.0.exe` and `dist/BrickBlast-1.0.0-win.zip`
4. Label them clearly in release notes

---

## Option 3 — Direct Download (Website)

Host the installer on any static file host (S3, Cloudflare R2, etc.) and link from your website or itch.io page.

---

## Code Signing (Optional but Recommended)

Unsigned Electron apps trigger Windows SmartScreen. For production:
1. Purchase a code-signing certificate (DigiCert, Sectigo, ~$200/year)
2. Configure in `package.json` under `win.certificateFile` and `win.certificatePassword`
3. Rebuild — SmartScreen warning disappears

---

## Testing Before Publish

- [ ] Run installer — installs to Program Files
- [ ] App appears in Start menu as "Brick Blast"
- [ ] Game loads and plays correctly
- [ ] Window title is "Brick Blast"
- [ ] Close button works
- [ ] Uninstaller appears in Settings → Apps
- [ ] Portable zip runs without install
