# BRICK BLAST — Release Notes

## v1.1.0 — WPF Port + Multi-Platform Release (Current)

### New
- **WPF version** — full port of the game to Windows Presentation Foundation
  - `DrawingContext` rendering instead of GDI+
  - `DispatcherTimer` instead of `System.Windows.Forms.Timer`
  - Rounded rectangles via `dc.DrawRoundedRectangle()` (no manual GraphicsPath arcs)
  - Gradient text via `FormattedText.BuildGeometry()`
  - MCI music hooks via `HwndSource.AddHook` instead of `WndProc` override
  - Source: `anime finder wpf/GameCanvas.vb`
- **7 platform builds** in `versions/` — all fully self-contained
  - Windows WinForms EXE (win-x64, self-contained)
  - Windows WPF EXE (win-x64, self-contained)
  - HTML5 browser (any modern browser)
  - Android Phone APK + PWA
  - Android Tablet APK + PWA
  - iPhone PWA (Safari)
  - iPad PWA (Safari)
- **Root README.md** — project overview with quick start table
- **Release notes** — this file

### Updated
- `versions/README.md` — added WPF row, labeled WinForms/WPF distinction
- `docs/GDD.md` — schedule Week 6 updated (Git repo marked complete)
- `docs/PROJECT_DOCS.md` — updated pipeline status, corrected high score persistence note
- `requirements/PLAN.md` — all batches marked complete
- `pipelines/mindset/MINDSET.md` — added WPF rationale
- `.gitignore` — added WPF build artifacts

---

## v1.0.0 — Initial Release

### Features
- Single-file VB.NET WinForms brick-breaking arcade game
- Paddle + ball physics with directional control
- 7 power-up types (multi-ball, grow, shrink, slow, fast, mega paddle, extra life)
- 8 level patterns (grid, checkerboard, diamond, fortress, stripes, cross, border, random)
- Combo scoring system (up to 8x multiplier)
- 10 procedural MIDI music styles with auto-advance
- 5 SFX style packs (square wave synthesis)
- Colorblind mode (CBF-safe palette + Unicode symbols)
- 4 window sizes (900×650 to 1800×1300)
- Persistent high scores (`%AppData%\BrickBlast\highscores.json`)
- GET READY countdown (3-2-1) after life lost
- Speed boost mode (F key, 2x ball speed, yellow tint)
- Screen shake on ball loss and brick destruction
- Star field background with parallax and twinkle
- Options screen with mouse + keyboard navigation
- Self-contained Windows EXE (no .NET install required)
- HTML5 browser version
- Android APK + PWA versions
- iOS PWA versions (iPhone + iPad)

### Technical
- Zero external dependencies — all assets procedurally generated
- Single-file architecture (`Form1.vb`)
- Organized into `#Region` blocks
- 22050 Hz 8-bit mono square wave WAV synthesis
- MIDI generation with variable-length delta encoding
- MCI playback via winmm.dll P/Invoke
- Circle-to-rectangle collision detection
- Logical coordinate system (1200×867) scaled to window

---

*Team Fast Talk — CS-120*
