# Publishing Documentation — Updated Versions Root
**BrickBlast: Velocity Market — Team Fast Talk**

This folder contains the latest compiled and packaged artifacts for all platforms.
Each sub-folder is an independent release unit. See the folder-level PUBLISHING.md for platform-specific instructions.

## Platform Index

| Folder | Type | Artifact |
|--------|------|----------|
| `windows/` | Windows WinForms EXE (x64) | `BrickBlast.exe` + `Assets/` |
| `windows-arm64/` | Windows WinForms EXE (ARM64) | `BrickBlast.exe` + `Assets/` |
| `windows-wpf/` | Windows WPF EXE (x64) | `BrickBlast.exe` + `Assets/` |
| `windows-store/` | Microsoft Store MSIX bundles | `BrickBlast.msixbundle`, `BrickBlast-x64.msix`, `BrickBlast-arm64.msix` |
| `html/` | HTML5 browser version | `index.html` |
| `ipad/` | iPad PWA | `index.html` |
| `iphone/` | iPhone PWA | `index.html` |
| `android-phone/` | Android phone PWA | `index.html` |
| `android-tablet/` | Android tablet PWA | `index.html` |
| `linux/` | Linux x64 launcher | launcher script + game files |
| `macos/` | macOS launcher | launcher script + game files |
| `macos-arm64/` | macOS ARM64 launcher | launcher script + game files |

## Quick Distribute

### Windows Direct Download
1. Zip `windows/` folder → `BrickBlast-Windows.zip`
2. Upload to GitHub Releases, itch.io, or distribute directly

### Windows Store
See `windows-store/PUBLISHING.md` — upload `BrickBlast.msixbundle` to Partner Center.

### Web / PWA (HTML, iPad, iPhone, Android)
Host `index.html` on any static server (GitHub Pages, Netlify, itch.io HTML):
```sh
# GitHub Pages example
git subtree push --prefix "updated versions/html" origin gh-pages
```

## Version
v1.2.0 — Latest release with full asset pipeline
