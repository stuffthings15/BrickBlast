# Publishing Documentation — Windows WPF (x64)
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`BrickBlast.exe` — self-contained WPF version of BrickBlast for Windows x64.
Uses `DrawingContext` rendering, `DispatcherTimer`, and WPF gradient/geometry APIs.

## Contents
```
windows-wpf/
├── BrickBlast.exe          ← WPF build
└── Assets/                 ← Full asset tree (shared with WinForms version)
```

## How to Run
Double-click `BrickBlast.exe`.

## How to Distribute

### Direct Download
```powershell
Compress-Archive -Path "updated versions\windows-wpf\*" -DestinationPath "BrickBlast-Windows-WPF.zip"
```

### itch.io via Butler
```sh
butler push "updated versions/windows-wpf" teamfasttalk/brickblast:windows-wpf
```

## System Requirements
- Windows 10 / 11 (x64)
- No additional runtime required (self-contained)

## Notes
- WPF build is the secondary Windows variant; the WinForms build (`windows/`) is the primary.
- Source: `anime finder wpf/GameCanvas.vb`

## Version: v1.2.0
