# Publishing Documentation — Linux x64
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
Linux x64 launcher + game files for direct distribution.

## How to Run
```sh
chmod +x BrickBlast.sh
./BrickBlast.sh
```

## How to Distribute

### itch.io via Butler
```sh
butler push "updated versions/linux" teamfasttalk/brickblast:linux
```

### Direct zip download
```powershell
Compress-Archive -Path "updated versions\linux\*" -DestinationPath "BrickBlast-Linux-x64.zip"
```
Upload to GitHub Releases or itch.io.

### Snap / Flatpak / AppImage
For full store-quality Linux packaging, use the Electron build in:
```
Final Version Releases/linux-x64/
```
That folder contains the Electron AppImage and zip distributables.

## System Requirements
- Ubuntu 20.04+ / Debian 11+ / Fedora 35+ (x64)
- Any modern Linux with a web browser for the HTML5 game

## Version: v1.2.0
