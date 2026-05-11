# Brick Blast — Windows ARM64 (Copilot+ / Surface / Snapdragon)

**Team Fast Talk** — Native WinForms build from the canonical VB.NET source.

This folder is fully self-contained. Zip it and share — no install required.

## How to Run
**Double-click `RUN_WINDOWS_ARM64.bat`** or run `BrickBlast.exe` directly.

## Contents
| File | Purpose |
|------|---------|
| `BrickBlast.exe` | Native Windows ARM64 executable (.NET 10, win-arm64) |
| `BrickBlast.pdb` | Debug symbols (optional — safe to delete for distribution) |
| `RUN_WINDOWS_ARM64.bat` | Convenience launcher |
| `Assets/` | Audio, sprites, and UI images |
| `PUBLISHING.md` | Distribution guide |
| `README.md` | This file |

## Requirements
- Windows 11 on **ARM64** hardware (Surface Pro X, Surface Pro 11, Snapdragon Elite/X Plus laptops, Copilot+ PCs)
- Windows 10 ARM64 (version 1903+) also supported
- No additional software needed — .NET 10 runtime is bundled in the EXE
- **x64 PC?** Use the `windows/` folder instead

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
- **Architecture:** arm64 (native — runs without x64 emulation)
- **Source:** `Form1.vb` + `anime finder.vbproj` at project root

## Rebuild from Source
```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -o "versions\windows-arm64"
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
