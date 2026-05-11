# Publishing Documentation — macOS (Intel x64)
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifact
macOS x64 launcher + game files. Runs on Intel Macs via the HTML5 game in the default browser.

## How to Run
```sh
chmod +x BrickBlast.sh
./BrickBlast.sh
```

## How to Distribute

### Direct Download
```powershell
Compress-Archive -Path "updated versions\macos\*" -DestinationPath "BrickBlast-macOS-Intel.zip"
```
Upload to GitHub Releases or itch.io.

### itch.io via Butler (run on Mac)
```sh
butler push "updated versions/macos" teamfasttalk/brickblast:mac
```

### Mac App Store / Notarized DMG
For a proper `.app` bundle and Mac App Store submission, use the Electron macOS build.
**Must be built on macOS** — see `Final Version Releases/macos/PUBLISHING.md`.

## System Requirements
- macOS 11 Big Sur or later (Intel x64)
- Default browser (Safari, Chrome, Firefox)

## Version: v1.2.0
