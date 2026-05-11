# Brick Blast — Windows WPF

**Team Fast Talk** — WPF sub-project build (alternate Windows renderer).

> **Note:** The primary Windows release is `versions\windows\BrickBlast.exe` (WinForms).
> This WPF build is provided as an alternative and uses the same game logic.

## How to Run
**Double-click `RUN_WINDOWS_WPF.bat`** or run `BrickBlast.exe` directly.

This folder is fully self-contained. Zip it and share — no install required.

## Contents
| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Self-contained WPF executable (.NET 10, win-x64) |
| `BrickBlast.pdb` | Debug symbols (safe to delete for distribution) |
| `RUN_WINDOWS_WPF.bat` | Convenience launcher |
| `PUBLISHING.md` | Distribution guide |
| `README.md` | This file |

## Requirements
- Windows 10 or later (64-bit)
- No additional software needed — .NET 10 runtime is bundled in the EXE

## Controls
| Key | Action |
|-----|--------|
| ← → / A D | Move paddle |
| SPACE | Start / Resume / Speed boost (2×) |
| P / ESC | Pause |
| F | Speed boost (2×) |
| H / O | Options menu |
| S | Store |
| Mouse click | Speed up ball (during gameplay) |
| Gamepad | Full controller support |

## Platform
- **Type:** WPF + VB.NET (Windows Presentation Foundation)
- **Framework:** .NET 10 (self-contained)
- **Rendering:** `DrawingContext` (WPF Media) instead of GDI+
- **Source:** `anime finder wpf\GameCanvas.vb` in project root

## Rebuild from Source
```powershell
dotnet publish "anime finder wpf\anime finder wpf.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "versions\windows-wpf"
```
