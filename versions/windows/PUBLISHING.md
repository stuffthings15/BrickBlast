# Publishing Guide — Windows (versions folder)

**Target:** Windows 10/11 — self-contained .NET executable  
**This is the canonical Windows release of the game.**

---

## Quick Start

Double-click `BrickBlast.exe` to launch. No installation required.

---

## Distributing This Build

This folder contains the canonical WinForms executable built from `Form1.vb`.

For publishing to stores and platforms, see the detailed platform-specific guides:

| Platform | Guide |
|----------|-------|
| itch.io | `../../Final Version Releases/itch.io/PUBLISHING.md` |
| Windows Store (MSIX) | `../../Final Version Releases/windows-store/PUBLISHING.md` |
| GitHub Releases | `../../Final Version Releases/windows-x64/PUBLISHING.md` |
| Windows ARM64 | `../../Final Version Releases/windows-arm64/PUBLISHING.md` |

---

## Rebuilding

From the solution root (requires Visual Studio or .NET SDK 10):

```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -o "versions\windows"
```

---

## Key Files

| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Main game executable |
| `*.dll` | .NET runtime assemblies |
| `Assets/` | Game assets |

---

## Testing Before Distribute

- [ ] `BrickBlast.exe` launches without errors
- [ ] Main menu renders correctly
- [ ] Full gameplay loop works
- [ ] Save/load persists across restarts
- [ ] No UAC prompts
