# Publishing Guide — Windows Desktop x64

**Target:** Windows 10/11 (64-bit x64) — native WinForms, self-contained .NET 10  
**Source:** `Form1.vb` + `anime finder.vbproj` at project root  
**This is the canonical Windows release.**

---

## Step 1 — Rebuild (if needed)

Run from the project root:

```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "versions\windows"
```

Output: `versions\windows\BrickBlast.exe` (~142 MB, includes .NET 10 runtime)

---

## Step 2 — Test Before Distribution

- [ ] `BrickBlast.exe` launches without errors on a clean machine or VM
- [ ] Main menu, Store, Stats, Daily Challenge, and Endless Mode all render
- [ ] All 10 music tracks play (chiptune cycling)
- [ ] All 5 SFX packs function
- [ ] Coin economy: earn coins, open Store, buy a skin, equip it, restart — persists
- [ ] Power-ups drop and activate correctly
- [ ] High score saves and survives restart
- [ ] No UAC prompts (should not require admin)
- [ ] Dark mode follows system theme on Windows 11
- [ ] Gamepad connects and controls work (Xbox controller recommended for test)

---

## Step 3 — Distribution Options

### Option A — Direct / itch.io (Recommended for indie release)
1. Zip the entire `versions\windows\` folder → `BrickBlast-Windows-x64.zip`
2. Go to https://itch.io/game/new
3. Upload the zip, set Kind: **Executable**, Platform: **Windows**
4. Set price, description, screenshots, then publish

### Option B — GitHub Releases
1. Tag the release: `git tag v1.0.0 && git push origin v1.0.0`
2. Go to https://github.com/stuffthings15/BrickBlast/releases/new
3. Select the tag, add release notes, upload `BrickBlast-Windows-x64.zip`
4. Publish release

### Option C — Windows Store (MSIX)
See `versions\windows-store\PUBLISHING.md` for the full MSIX packaging and Partner Center submission guide.

---

## Key Files

| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Native WinForms game executable (self-contained, ~142 MB) |
| `Assets/` | Audio (MP3), sprites, and UI images |
| `RUN_WINDOWS.bat` | Convenience launcher |

---

## Notes
- The EXE is built with `-p:PublishSingleFile=true` so the entire .NET runtime is embedded — no separate installer needed.
- Save data is written to `%APPDATA%\BrickBlast\` — does not require admin rights.
- For ARM64 Surface/Copilot+ PCs use `versions\windows-arm64\BrickBlast.exe`.
