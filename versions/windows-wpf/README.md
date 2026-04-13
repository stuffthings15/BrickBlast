# Brick Blast — Windows WPF Version

## How to Run
**Double-click `RUN_WINDOWS_WPF.bat`** or run `BrickBlast.exe` directly.

This folder is fully self-contained. Zip it and share — no install required.

## Contents
- `BrickBlast.exe` — Self-contained WPF executable (no .NET install needed)
- `RUN_WINDOWS_WPF.bat` — Shortcut to launch the game
- `README.md` — This file

## Requirements
- Windows 10 or later (64-bit)
- No additional software needed — .NET runtime is bundled in the EXE

## Controls
| Key | Action |
|-----|--------|
| ← → / A D | Move paddle |
| SPACE | Start / Resume / Speed boost (2x) |
| P / ESC | Pause |
| F | Speed boost (2x) |
| H / O | Options menu |
| Mouse click | Speed up ball (during gameplay) |

## Platform
- **Type:** WPF + VB.NET (Windows Presentation Foundation)
- **Framework:** .NET 10 (self-contained)
- **Rendering:** `DrawingContext` (WPF Media) instead of GDI+
- **Source:** `anime finder wpf\GameCanvas.vb` in project root
