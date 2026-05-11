# Publishing Documentation — Windows ARM64 (WinForms)
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
`BrickBlast.exe` — self-contained Windows ARM64 WinForms executable.
Runs natively on Surface Pro X, Snapdragon X Elite, and all Windows ARM devices.

## Contents
```
windows-arm64/
├── BrickBlast.exe          ← ARM64 native binary
└── Assets/                 ← Full asset tree
```

## How to Run
Double-click `BrickBlast.exe` on any Windows ARM64 device.

## How to Distribute

### Direct Download
```powershell
Compress-Archive -Path "updated versions\windows-arm64\*" -DestinationPath "BrickBlast-Windows-ARM64.zip"
```

### itch.io via Butler
```sh
butler push "updated versions/windows-arm64" teamfasttalk/brickblast:windows-arm64
```

### GitHub Releases
Attach `BrickBlast-Windows-ARM64.zip` to the GitHub release alongside the x64 build.

## System Requirements
- Windows 11 on ARM (Snapdragon X / Surface Pro X / any ARM64 device)
- No additional runtime required

## Version: v1.2.0
