# Brick Blast — Windows Desktop (x64)

**Team Fast Talk** — Native WinForms build from the canonical VB.NET source.

## How to Run
**Double-click `RUN_WINDOWS.bat`** or run `BrickBlast.exe` directly.

This folder is fully self-contained. Zip it and share — no install required.

## Contents
| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Self-contained native WinForms executable (.NET 10, win-x64) |
| `RUN_WINDOWS.bat` | Shortcut to launch the game |
| `Assets/` | Audio, sprites, and UI images used by the game |
| `PUBLISHING.md` | Distribution and store submission guide |
| `README.md` | This file |

## Requirements
- Windows 10 version 1903 or later (64-bit x64)
- No additional software needed — .NET 10 runtime is bundled in the EXE

## Controls
| Key / Input | Action |
|-------------|--------|
| ← → / A D / WASD | Move paddle |
| SPACE / Click | Start · Resume · Speed boost (2×) |
| P / ESC | Pause |
| F | Speed boost (2×) toggle |
| H / O | Options menu |
| S | Open Store (from menu) |
| Gamepad | Full controller support (A=confirm, B=back, Start=options) |

## Platform
- **Type:** Native Windows Forms (VB.NET)
- **Framework:** .NET 10, self-contained, single-file
- **Architecture:** x64 (Intel / AMD 64-bit)
- **Source:** `Form1.vb` + `anime finder.vbproj` at project root
- **For ARM64 Surface/Copilot+ PCs:** Use the `windows-arm64/` folder instead

## Rebuild from Source
Run from the project root (requires .NET 10 SDK):
```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "versions\windows"
```

## Features
- 🧱 **Brick layouts** — 8 patterns that rotate across levels
- 🎨 **Skins store** — 13 ball skins, 8 paddle skins, 10 brick palettes, 16 bonus packs
- 💰 **Coin economy** — earn coins by breaking bricks, spend them in the Store
- ⚡ **Power-ups** — grow ball, extra life, multi-ball, shrink, wide paddle, slow & fast
- 🎮 **Full gamepad support** — Xbox, PlayStation, Switch, and generic controllers
- 🎵 **Chiptune music** — 10 tracks cycling through 8-bit styles
- 🔊 **5 SFX styles** — Zelda, Mega Man, Tetris, Retro Arcade, and Classic
- 🏆 **High score table** — top-10 persistent leaderboard
- 📊 **Stats screen** — games played, bricks broken, total coins earned
- ♿ **Colorblind mode** — high-contrast palette with shape symbols
- 🌙 **Dark mode aware** — follows system theme on Windows 11
