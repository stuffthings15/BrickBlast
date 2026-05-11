# CURTIS LOOP: BRICK BLAST

> A retro arcade brick-breaking game built in VB.NET — Team Fast Talk, CS-120

[![Platform](https://img.shields.io/badge/Platforms-Windows%20%7C%20macOS%20%7C%20Linux%20%7C%20Web%20%7C%20Android%20%7C%20iOS-blue)]()
[![Language](https://img.shields.io/badge/Language-VB.NET%20%2B%20HTML5-purple)]()
[![Framework](https://img.shields.io/badge/Framework-.NET%2010%20%7C%20WinForms%20%7C%20WPF%20%7C%20Avalonia-green)]()

---

## Quick Start

| Platform | How to Run |
|----------|-----------|
| **Windows x64 (WinForms)** | `versions/windows/BrickBlast.exe` — double-click, no install |
| **Windows ARM64 (WinForms)** | `versions/windows-arm64/BrickBlast.exe` — double-click, no install |
| **Windows Store** | `versions/windows-store/BrickBlast.msixbundle` — double-click to sideload |
| **Windows (WPF)** | `versions/windows-wpf/BrickBlast.exe` — double-click, no install |
| **macOS Intel** | `versions/macos/osx-x64/anime finder macos` — run from Terminal |
| **macOS Apple Silicon** | `versions/macos/osx-arm64/anime finder macos` — run from Terminal |
| **Linux** | `versions/linux/RUN_LINUX.sh` — run from terminal |
| **Browser** | `versions/html/index.html` — open in any browser |
| **Android Phone** | Transfer `versions/android-phone/BrickBlast-Android.apk` → install |
| **Android Tablet** | Transfer `versions/android-tablet/BrickBlast-Android.apk` → install |
| **iPhone** | Host `versions/iphone/` on HTTPS → install via Safari PWA |
| **iPad** | Host `versions/ipad/` on HTTPS → install via Safari PWA |

No install required for Windows — the EXE is fully self-contained.

---

## Controls

| Key | Action |
|-----|--------|
| ← → / A D | Move paddle |
| SPACE | Start / Resume / Speed boost (2x) |
| P / ESC | Pause |
| F | Toggle 2x speed boost |
| H / O | Options menu |
| Mouse click | Speed up ball (during gameplay) |

---

## Features

- **7 Power-Up Types** — multi-ball, ball grow/shrink, paddle enlarge, speed up/down, extra life
- **6-Track MP3 Soundtrack** — Brick Blast, Calculated Impact, Machine Precision, Machine, Pinball Dream, Pinball — plays on all native platforms
- **5 SFX Packs** — Classic, Zelda, Mega Man, Tetris, Retro Arcade
- **Combo Scoring** — up to 8x multiplier for rapid brick hits
- **8 Level Patterns** — standard grid, checkerboard, diamond, fortress, stripes, cross, border, random
- **Colorblind Mode** — CBF-safe palette with Unicode symbols on bricks
- **Persistent High Scores** — saved to `%AppData%\BrickBlast\highscores.json`
- **160 Imported Assets (WPF)** — CC0 sprites from OpenGameArt + Kenney for bricks, paddle, balls, UI, backgrounds
- **Cross-Platform Native** — WinForms (x64 + ARM64), WPF, Avalonia (macOS + Linux), HTML5, Android, iOS

---

## Project Structure

```
BrickBlast/
├── Form1.vb                        ← WinForms game (main source)
├── Form1.Designer.vb               ← WinForms designer
├── anime finder.vbproj             ← WinForms project
├── anime finder wpf/               ← WPF port
│   ├── GameCanvas.vb               ← WPF game (DrawingContext rendering)
│   ├── Assets/                     ← 160 CC0 sprites (OpenGameArt, Kenney)
│   ├── Scripts/                    ← AssetManager, TileMap, EnemyAI, etc.
│   ├── MainWindow.xaml             ← WPF window
│   ├── MainWindow.xaml.vb          ← WPF code-behind
│   ├── Program.vb                  ← WPF entry point
│   └── anime finder wpf.vbproj    ← WPF project
├── anime finder macos/             ← Avalonia VB port (macOS + Linux)
│   ├── GameCanvas.vb               ← Avalonia game canvas
│   └── anime finder macos.vbproj  ← Avalonia project
├── Assets/Audio/                   ← 6 MP3 soundtrack tracks
├── versions/                       ← Self-contained platform builds
│   ├── windows/                    ← WinForms EXE (win-x64, self-contained)
│   ├── windows-arm64/              ← WinForms EXE (win-arm64, self-contained)
│   ├── windows-wpf/                ← WPF EXE (win-x64, self-contained)
│   ├── windows-store/              ← MSIX bundle (x64+ARM64) for Microsoft Store
│   ├── macos/osx-x64/              ← Avalonia native (macOS Intel)
│   ├── macos/osx-arm64/            ← Avalonia native (macOS Apple Silicon)
│   ├── linux/                      ← Avalonia native (Linux x64)
│   ├── html/                       ← Browser (HTML5 Canvas)
│   ├── android-phone/              ← Android APK + AAB + Play Store guide
│   ├── android-tablet/             ← Android APK + AAB
│   ├── iphone/                     ← iOS PWA + App Store guide
│   └── ipad/                       ← iPadOS PWA + App Store guide
├── docs/                           ← Documentation
│   ├── GDD.md                      ← Game Design Document
│   ├── PROJECT_DOCS.md             ← Architecture overview
│   └── TEAM_PRODUCTION.md          ← 20-person team structure
├── msix/                           ← Windows Store packaging workspace
├── mobile/                         ← Capacitor Android/iOS project
├── web/                            ← HTML5 source
├── index.html                      ← Root HTML redirect
└── RELEASE_NOTES.md                ← Version changelog
```

---

## Building From Source

### Windows x64 (WinForms)
```bash
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o versions/windows
```

### Windows ARM64 (WinForms)
```bash
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -o versions/windows-arm64
```

### Windows (WPF)
```bash
cd "anime finder wpf"
dotnet publish "anime finder wpf.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../versions/windows-wpf
```

### macOS (Avalonia)
```bash
cd "anime finder macos"
dotnet publish "anime finder macos.vbproj" -c Release -r osx-x64 --self-contained true -o ../versions/macos/osx-x64
dotnet publish "anime finder macos.vbproj" -c Release -r osx-arm64 --self-contained true -o ../versions/macos/osx-arm64
```

### Linux (Avalonia)
```bash
cd "anime finder macos"
dotnet publish "anime finder macos.vbproj" -c Release -r linux-x64 --self-contained true -o ../versions/linux/bin
```

### Requirements
- .NET 10 SDK
- Windows 10+ for WinForms/WPF; macOS 12+ or Ubuntu 20.04+ for Avalonia
- No additional runtime dependencies — all builds are self-contained

---

## Documentation

| Document | Description |
|----------|-------------|
| [Game Design Document](docs/GDD.md) | Full GDD — story, gameplay, assets, code, schedule |
| [Project Docs](docs/PROJECT_DOCS.md) | Architecture overview and pipeline status |
| [Team Production](docs/TEAM_PRODUCTION.md) | 20-person team roles and responsibilities |
| [Plan](requirements/PLAN.md) | Batched improvement plan with status |
| [Story Assets](pipelines/docs/story_assets/STORY.md) | Narrative and character descriptions |
| [Mindset](pipelines/mindset/MINDSET.md) | Design decisions and rationale |
| [Release Notes](RELEASE_NOTES.md) | Version history and changelog |
| [Versions README](versions/README.md) | Platform build index |

---

## Credits

**Team Fast Talk** — CS-120 Final Project  
Built with VB.NET, .NET 10, Windows Forms, WPF, Avalonia, HTML5 Canvas, Capacitor  
Soundtrack: 6 original MP3 tracks · 160 CC0 sprites (WPF) · all other visuals procedural
