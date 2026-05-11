# Publishing Documentation — macOS ARM64 (Apple Silicon)
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
macOS ARM64 launcher + game files. Runs natively on M1 / M2 / M3 / M4 Macs.

## How to Run
```sh
chmod +x BrickBlast.sh
./BrickBlast.sh
```

## How to Distribute

### Direct Download
```powershell
Compress-Archive -Path "updated versions\macos-arm64\*" -DestinationPath "BrickBlast-macOS-ARM64.zip"
```

### itch.io via Butler (run on Mac)
```sh
butler push "updated versions/macos-arm64" teamfasttalk/brickblast:mac-arm64
```

### Mac App Store / Universal DMG
For a Universal binary `.app` targeting both Intel and Apple Silicon, build via Electron on macOS:
```
Final Version Releases/macos-arm64/PUBLISHING.md
```
Requires macOS + Xcode + Apple Developer Program ($99/yr) for signing and notarization.

## System Requirements
- macOS 12 Monterey or later (Apple Silicon — M1/M2/M3/M4)

## Version: v1.2.0
