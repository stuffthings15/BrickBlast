# Brick Blast — Windows Desktop Version

## How to Run
**Double-click `RUN_WINDOWS.bat`** or run `BrickBlast.exe` directly.

This folder is fully self-contained. Zip it and share — no install required.

## Contents
- `BrickBlast.exe` — Self-contained executable (no .NET install needed)
- `RUN_WINDOWS.bat` — Shortcut to launch the game
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
- **Type:** Native Windows Forms (VB.NET)
- **Source:** `Form1.vb` in project root

## Build EXE Yourself
If `BrickBlast.exe` is not present, build it from the project root:
```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o versions\windows
```
Then rename `anime finder.exe` to `BrickBlast.exe`.

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
