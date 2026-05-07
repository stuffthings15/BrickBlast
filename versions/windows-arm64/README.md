# Brick Blast — Windows ARM64 Version

This folder is fully self-contained. Zip it and share — no install required.

## How to Run
**Double-click `RUN_WINDOWS_ARM64.bat`** or run `BrickBlast.exe` directly.

## Contents
| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Native Windows ARM64 executable |
| `BrickBlast.pdb` | Debug symbols (optional — can delete) |
| `RUN_WINDOWS_ARM64.bat` | Shortcut to launch the game |
| `README.md` | This file |

## Requirements
- Windows 10 or later on **ARM64** hardware (Surface Pro X, Snapdragon laptops, etc.)
- No additional software needed — .NET runtime is bundled in the EXE

## Controls
| Input | Action |
|-------|--------|
| ← → / A D / WASD | Move paddle |
| SPACE | Start / Resume / Speed boost (2×) |
| P / ESC | Pause |
| F | Speed boost (2×) |
| H / O | Options menu |
| Mouse click | Speed up ball (during gameplay) |
| Gamepad | Full controller support |

## Platform
- **Type:** Native Windows app (WinForms + VB.NET)
- **Architecture:** ARM64 (native — not emulated x64)
- **Framework:** .NET 10 (self-contained)
- **For x64 PCs:** Use the `windows/` folder instead

## Rebuild from Source
```bat
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true /p:PublishSingleFile=true -o versions\windows-arm64
```
