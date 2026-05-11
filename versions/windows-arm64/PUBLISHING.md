# Publishing Guide — Windows ARM64 (versions folder)

**Target:** Windows 11 on ARM (Surface Pro X, Snapdragon laptops)  
**Status:** ✅ READY — self-contained .NET executable

---

## Quick Reference

For full instructions, see `../../Final Version Releases/windows-arm64/PUBLISHING.md`.

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Self-contained ARM64 executable |
| `*.dll` | .NET runtime assemblies |
| `Assets/` | Game assets |

---

## Run

Double-click `BrickBlast.exe` on any Windows 11 ARM device. No installation required.

---

## Rebuild

```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true -o "versions\windows-arm64"
```

---

## Distribute

| Channel | Steps |
|---------|-------|
| itch.io | `butler push . teamfasttalk/brickblast:windows-arm64 --userversion 1.0.0` |
| GitHub Releases | Zip folder → upload as release asset |

---

## Testing Before Publish

- [ ] Launches on Windows 11 ARM device
- [ ] Runs natively (Task Manager shows ARM64, not x64 emulated)
- [ ] Full gameplay loop works
- [ ] Save/load persists across restarts
