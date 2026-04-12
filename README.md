# CURTIS LOOP: BRICK BLAST

> A retro arcade brick-breaking game built in VB.NET — Team Fast Talk, CS-120

[![Platform](https://img.shields.io/badge/Platforms-Windows%20%7C%20Web%20%7C%20Android%20%7C%20iOS-blue)]()
[![Language](https://img.shields.io/badge/Language-VB.NET%20%2B%20HTML5-purple)]()
[![Framework](https://img.shields.io/badge/Framework-.NET%2010%20%7C%20WinForms%20%7C%20WPF-green)]()

---

## Quick Start

| Platform | How to Run |
|----------|-----------|
| **Windows (WinForms)** | `versions/windows/RUN_WINDOWS.bat` → double-click `BrickBlast.exe` |
| **Windows (WPF)** | `versions/windows-wpf/RUN_WINDOWS_WPF.bat` → double-click `BrickBlast.exe` |
| **Browser** | `versions/html/RUN_HTML.bat` → open `index.html` in any browser |
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
| SPACE | Start / Resume / Next level |
| P / ESC | Pause |
| F | Toggle 2x speed boost |
| H / O | Options menu |
| Mouse click | Speed up ball (during gameplay) |

---

## Features

- **7 Power-Up Types** — multi-ball, ball grow/shrink, paddle enlarge, speed up/down, extra life
- **10 Procedural Music Styles** — Zelda, Mega Man, Tetris, Pac-Man, Space Invaders, Castlevania, Metroid, Galaga, Contra, Double Dragon
- **5 SFX Packs** — Classic, Zelda, Mega Man, Tetris, Retro Arcade
- **Combo Scoring** — up to 8x multiplier for rapid brick hits
- **8 Level Patterns** — standard grid, checkerboard, diamond, fortress, stripes, cross, border, random
- **Colorblind Mode** — CBF-safe palette with Unicode symbols on bricks
- **Persistent High Scores** — saved to `%AppData%\BrickBlast\highscores.json`
- **Zero External Assets** — all visuals, music, and SFX generated procedurally at runtime

---

## Project Structure

```
BrickBlast/
├── Form1.vb                        ← WinForms game (main source)
├── Form1.Designer.vb               ← WinForms designer
├── anime finder.vbproj             ← WinForms project
├── anime finder wpf/               ← WPF port
│   ├── GameCanvas.vb               ← WPF game (DrawingContext rendering)
│   ├── MainWindow.xaml             ← WPF window
│   ├── MainWindow.xaml.vb          ← WPF code-behind
│   ├── Program.vb                  ← WPF entry point
│   └── anime finder wpf.vbproj    ← WPF project
├── versions/                       ← Self-contained platform builds
│   ├── windows/                    ← WinForms EXE (win-x64)
│   ├── windows-wpf/                ← WPF EXE (win-x64)
│   ├── html/                       ← Browser (HTML5 Canvas)
│   ├── android-phone/              ← Android APK + PWA
│   ├── android-tablet/             ← Android APK + PWA
│   ├── iphone/                     ← iOS PWA
│   └── ipad/                       ← iPadOS PWA
├── docs/                           ← Documentation
│   ├── GDD.md                      ← Game Design Document
│   ├── PROJECT_DOCS.md             ← Global project docs
│   └── TEAM_PRODUCTION.md          ← 20-person team structure
├── requirements/PLAN.md            ← Improvement plan
├── pipelines/                      ← Build pipelines + mindset
├── web/                            ← HTML5 source
├── index.html                      ← Root HTML redirect
└── RELEASE_NOTES.md                ← This release changelog
```

---

## Building From Source

### Windows (WinForms)
```bash
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o versions/windows
```

### Windows (WPF)
```bash
cd "anime finder wpf"
dotnet publish "anime finder wpf.vbproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o ../versions/windows-wpf
```

### Requirements
- .NET 10 SDK (for building from source)
- Windows 10+ 64-bit (for running)
- No additional dependencies

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
Built with VB.NET, .NET 10, Windows Forms, WPF, HTML5 Canvas, Capacitor  
All code, music, SFX, and visuals generated procedurally — zero external assets
