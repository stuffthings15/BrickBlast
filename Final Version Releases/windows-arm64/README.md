# BrickBlast — Windows ARM64 Release

Self-contained Windows 11 ARM64 executable for Snapdragon-powered PCs, Surface Pro X, and ARM64 developer boards.

## Files
| Path | Purpose |
|------|---------|
| `BrickBlast.exe` | Main executable (ARM64 native) |
| `BrickBlast.runtimeconfig.json` | Runtime binding configuration |
| `Assets/` | Music, sprites, and audio assets loaded at runtime |
| `*.dll` | Self-contained .NET 10 ARM64 runtime and native dependencies |

## Running

Double-click `BrickBlast.exe` on any ARM64 Windows device, or:

```powershell
.\BrickBlast.exe
```

Save data is stored under `%AppData%\BrickBlast\` — identical path to the x64 build, so profiles roam seamlessly.

## System Requirements
| Requirement | Minimum |
|-------------|---------|
| OS | Windows 11 (ARM64) |
| Architecture | AArch64 |
| RAM | 256 MB |
| .NET | Bundled — no separate install needed |

## Tested Hardware
- Microsoft Surface Pro X (SQ1/SQ2)
- Microsoft Surface Pro 9 (Snapdragon® 8cx Gen 3)
- Qualcomm reference development boards

## Build Reproduction

```powershell
dotnet publish "anime finder.vbproj" `
  -c Release `
  -r win-arm64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -o "Final Version Releases/windows-arm64"
```
