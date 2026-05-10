# BrickBlast — Windows x64 Release

Self-contained Windows 10/11 x64 executable. No .NET runtime installation required.

## Files
| Path | Purpose |
|------|---------|
| `BrickBlast.exe` | Main executable |
| `BrickBlast.runtimeconfig.json` | Runtime binding configuration |
| `Assets/` | Music, sprites, and audio assets loaded at runtime |
| `*.dll` | Self-contained .NET 10 runtime and native dependencies |

## Running

Double-click `BrickBlast.exe`, or from PowerShell:

```powershell
.\BrickBlast.exe
```

No installer required. The game stores save data under `%AppData%\BrickBlast\`.

## System Requirements
| Requirement | Minimum |
|-------------|---------|
| OS | Windows 10 version 1903 (build 18362) |
| Architecture | x86_64 |
| RAM | 256 MB |
| GPU | Any DirectX 9+ capable GPU (GDI+ fallback available) |
| .NET | Bundled — no separate install needed |

## Distribution
Zip the entire folder and distribute as-is, or package with the Windows Store MSIX (see `windows-store/`).

## Build Reproduction

```powershell
# From repo root
dotnet publish "anime finder.vbproj" `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -o "Final Version Releases/windows-x64"
```
