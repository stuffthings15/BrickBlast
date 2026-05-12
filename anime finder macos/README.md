# anime finder macos — macOS Native Port
**BrickBlast: Velocity Market — Team Fast Talk**

Swift + AppKit port of BrickBlast for macOS, distributable via the Mac App Store.

## Platform Target
- macOS 12+ (Monterey and later)
- Apple Silicon (arm64) and Intel (x86_64) via Universal Binary

## Key Files
| File | Purpose |
|------|---------|
| `Assets/AppIcon.appiconset/` | Mac App Store icon set |
| `Sources/BrickBlastApp.swift` | SwiftUI App entry point |
| `Sources/GameView.swift` | Main game rendering (SpriteKit or AppKit) |

## How to Build (Mac only)
```sh
open anime\ finder\ macos.xcodeproj
# Select scheme: BrickBlast
# Product > Archive
```

## Store Submission
See `Final Version Releases/macos/PUBLISHING.md` for Mac App Store submission steps.

## Note
Building this target requires Xcode on macOS. It cannot be compiled on Windows.
