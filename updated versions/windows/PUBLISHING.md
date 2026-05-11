# Publishing Documentation — Windows x64 (WinForms)
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`BrickBlast.exe` — self-contained Windows x64 WinForms executable (no .NET install required).

## Contents
```
windows/
├── BrickBlast.exe          ← Launch this
└── Assets/
    ├── Audio/              ← SFX and music WAV files
    ├── Sprites/            ← Game sprites (PNG)
    ├── Tiles/              ← Tile assets (PNG)
    ├── UI/                 ← UI assets (PNG)
    ├── Animations/         ← Animation frames
    └── Characters/         ← Character sprites
```

## How to Run
Double-click `BrickBlast.exe` — no installer needed.

## How to Distribute

### Direct Download (GitHub Releases / itch.io)
```powershell
# From repo root — zip this folder
Compress-Archive -Path "updated versions\windows\*" -DestinationPath "BrickBlast-Windows-x64.zip"
```
Upload the zip to GitHub Releases or itch.io as the Windows channel.

### itch.io via Butler
```sh
butler push "updated versions/windows" teamfasttalk/brickblast:windows
```

## System Requirements
- Windows 10 / 11 (x64)
- No additional runtime required (self-contained .NET 10 runtime bundled)

## Version: v1.2.0
