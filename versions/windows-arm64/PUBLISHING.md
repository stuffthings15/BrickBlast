# Publishing Guide — Windows ARM64

**Target:** Windows 10/11 on ARM64 (Surface Pro X, Snapdragon Elite/X Plus, Copilot+ PCs)  
**Source:** `Form1.vb` + `anime finder.vbproj` at project root  
**Type:** Native WinForms, self-contained .NET 10

---

## Step 1 — Rebuild (if needed)

```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -o "versions\windows-arm64"
```

Output: `versions\windows-arm64\BrickBlast.exe` (~155 MB, .NET 10 runtime embedded)

---

## Step 2 — Test Before Distribution

- [ ] Launches on a Windows ARM64 device without errors
- [ ] Task Manager shows process as **ARM64** (not "x64 emulated")
- [ ] Store, Stats, Daily Challenge, Endless Mode all open correctly
- [ ] All 10 music tracks play; all 5 SFX packs work
- [ ] Coin economy persists across restarts
- [ ] Power-ups activate correctly during gameplay
- [ ] High score saves across sessions
- [ ] No UAC prompt on launch
- [ ] Gamepad works (if available on device)

---

## Step 3 — Distribution Options

### Option A — itch.io
1. Zip `versions\windows-arm64\` → `BrickBlast-Windows-ARM64.zip`
2. Upload to your itch.io game page as a second Windows download
3. Set Kind: **Executable**, Platform: **Windows**, Tag: **ARM64**

### Option B — GitHub Releases
1. Attach `BrickBlast-Windows-ARM64.zip` to the same release as the x64 build
2. Label clearly in release notes: *"ARM64 native — Surface, Snapdragon, Copilot+ PCs"*

### Option C — Windows Store (MSIX)
See `versions\windows-store\PUBLISHING.md` — the MSIX package can bundle both x64 and arm64 in a single upload using a MSIX bundle.

---

## Key Files

| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Native ARM64 game executable (~155 MB) |
| `Assets/` | Audio (MP3), sprites, UI images |
| `RUN_WINDOWS_ARM64.bat` | Convenience launcher |

---

## Notes
- Running the x64 EXE on ARM64 Windows works via emulation, but native ARM64 delivers better battery life and performance.
- Save data is written to `%APPDATA%\BrickBlast\` on the user's device.
