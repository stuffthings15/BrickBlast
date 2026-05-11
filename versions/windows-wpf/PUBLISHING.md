# Publishing Guide — Windows WPF

**Target:** Windows 10/11 (x64) — WPF sub-project  
**Source:** `anime finder wpf\anime finder wpf.csproj`  
**⚠️ Sub-project:** The canonical game is WinForms (`Form1.vb`). Only build and distribute this if explicitly targeting the WPF variant.

---

## Step 1 — Rebuild

From the project root:

```powershell
dotnet publish "anime finder wpf\anime finder wpf.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "versions\windows-wpf"
```

---

## Step 2 — Test Before Distribution

- [ ] `BrickBlast.exe` launches without errors
- [ ] Game renders correctly (WPF DrawingContext, not GDI+)
- [ ] Store, music, power-ups, and save all function identically to WinForms build
- [ ] No external runtime dependencies
- [ ] Verified as WPF process (not the WinForms canonical build)

---

## Step 3 — Distribute

| Channel | Steps |
|---------|-------|
| itch.io | Zip folder → upload as alternate Windows download, tagged "WPF" |
| GitHub Releases | Attach `BrickBlast-Windows-WPF.zip` to release, clearly labelled |

---

## Key Files

| File | Purpose |
|------|---------|
| `BrickBlast.exe` | WPF game executable (self-contained, .NET 10, win-x64) |
| `BrickBlast.pdb` | Debug symbols (safe to delete for distribution) |
| `RUN_WINDOWS_WPF.bat` | Convenience launcher |

---

## Notes
- Prefer distributing the WinForms build (`versions\windows\`) for the primary Windows release.
- This WPF build is provided as an alternative for users who experience issues with WinForms rendering.
