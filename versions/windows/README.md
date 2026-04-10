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
| SPACE | Start / Resume |
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
