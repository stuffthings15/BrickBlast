# CURTIS LOOP: BRICK BLAST
## Project Review

**Project:** Curtis Loop — Brick Blast  
**Team:** Team Fast Talk  
**Course:** CS-120  
**Review Date:** April 2026  
**Version Reviewed:** 1.2.0  
**Repository:** https://github.com/stuffthings15/BrickBlast

---

## 1. Project Summary

Curtis Loop: Brick Blast is a retro arcade brick-breaker game built for CS-120. The project began as a single WinForms/VB.NET file and expanded into a 7-platform release including WPF, HTML5, Android, and iOS. All visuals, music, and sound effects are generated procedurally at runtime in the WinForms and HTML versions — no external asset files are required. The WPF version integrates 179 CC0 sprites from OpenGameArt and Kenney.

---

## 2. What Was Built

| Platform | Technology | Status |
|----------|-----------|--------|
| Windows (WinForms) | VB.NET, .NET 10, GDI+ | ✅ Shipping |
| Windows (WPF) | VB.NET, .NET 10, DrawingContext | ✅ Shipping |
| Browser | HTML5 Canvas, Web Audio API | ✅ Shipping |
| Android Phone | Capacitor PWA → APK | ✅ Shipping |
| Android Tablet | Capacitor PWA → APK | ✅ Shipping |
| iPhone | Progressive Web App (HTTPS) | ✅ Shipping |
| iPad | Progressive Web App (HTTPS) | ✅ Shipping |

**Core features delivered:**
- 7 rows × 12 col brick grid with 8 level patterns (grid, checkerboard, diamond, fortress, stripes, cross, border, random)
- 7 power-up types with distinct visual and gameplay effects
- Combo scoring system (up to 8× multiplier, 90-frame window)
- 10 procedural MIDI music styles auto-advancing on completion
- 5 procedural WAV SFX packs (22 050 Hz, 8-bit, square wave with ADSR)
- XInput gamepad support (D-pad, left stick, ABXY, Start, LB/RB)
- Touch / mouse paddle drag (WinForms maps touch to mouse on Windows)
- Colorblind mode (CBF-safe palette + Unicode symbols on bricks)
- Persistent high scores (top 10, JSON, `%AppData%\BrickBlast\`)
- 4 window sizes (900×650 → 1800×1300) with logical coordinate scaling
- Screen shake, particles, star field, GET READY countdown

---

## 3. What Went Well

### Single-file architecture stayed manageable
`Form1.vb` uses `#Region` blocks to separate concerns (constants, state, update, draw, audio, helpers). This kept navigation easy for a solo or small-team course project without needing a full engine framework.

### Procedural audio eliminated all asset dependencies
Generating WAV bytes in-memory via square-wave synthesis and writing MIDI temp files means the WinForms and HTML builds have zero external files. This made packaging, sharing, and running the game frictionless — no missing asset errors possible.

### Multi-platform reach exceeded initial scope
The initial goal was a Windows desktop game. By week 6 the project shipped on 7 platforms. The HTML5 version required rewriting the game loop in JavaScript but reused all design decisions. The Android/iOS PWA builds added Capacitor integration.

### Asset integration in WPF was clean
The WPF port introduced 179 CC0 sprites with an `AssetManager` that falls back to procedural drawing when a sprite is missing. This meant WPF and WinForms rendering paths could develop in parallel without blocking each other.

### MusicXML reference (TextFile1.txt)
Storing the music style definitions as MusicXML (P1–P10, 10 parts, 16 measures each) provided a readable, structured reference for BPM, key signature, and instrument assignment that could be cross-checked against the MIDI generation code.

---

## 4. What Could Be Improved

### Double-music bug (fixed v1.2.0)
`RegenerateCurrentMusicFile()` stopped and closed the MCI device but left `_musicPlaying = True`. The WndProc `MM_MCINOTIFY` handler saw `True` and scheduled a new track. `ChangeMusic()` also scheduled a track. Both timers fired simultaneously — two tracks played at once. Fixed by setting `_musicPlaying = False` in both methods before scheduling.

### WPF crash on startup (fixed v1.2.0)
`OnRender` is called by WPF's layout/Arrange pass before the `Loaded` event fires. `_starFieldX` was `Nothing` because `InitStarField()` only ran in `OnLoaded`. This caused a `NullReferenceException` on `_starFieldX.Length` at the first render call, crashing the exe silently. Fixed by moving `InitStarField()` into the `New()` constructor.

### MCI audio is Windows-only
The music system uses `mciSendString` via `winmm.dll` P/Invoke. This works only on Windows. The HTML5 version correctly uses Web Audio API. Any future macOS or Linux desktop port would need the audio system replaced (e.g., NAudio or a cross-platform MIDI library).

### GDD version mismatch
`docs/GDD.md` was labeled version 1.1 while the game shipped at 1.2.0. The GDD did not document the gamepad input system, the double-music fix, or the WPF startup fix. The new `docs/GDD_FORM.md` reflects the current state.

### Single-file VB.NET limits testability
The entire WinForms game lives in one file with no unit tests. All state is instance-level on `Form1`. Extracting game logic (collision, scoring, power-ups) into separate testable classes would make regression testing practical as complexity grows.

### Power-up naming inconsistency
`RedBallShrink` actually grants an extra life — the name was inherited from an earlier design iteration. This caused confusion in code review (PLAN.md Batch 2 notes the fix, but the enum name was not updated in `Form1.vb`).

---

## 5. Bug Log

| # | Bug | Severity | Status | Fix |
|---|-----|----------|--------|-----|
| 1 | Two music tracks playing simultaneously | High | ✅ Fixed v1.2.0 | `ChangeMusic()` and `RegenerateCurrentMusicFile()` now set `_musicPlaying = False` before scheduling restart |
| 2 | WPF exe crashes silently on launch (`NullReferenceException` in `DrawStarField`) | Critical | ✅ Fixed v1.2.0 | Moved `InitStarField()` from `OnLoaded` to constructor `New()` |
| 3 | WPF missing from solution file — VS ran WinForms instead of WPF | Medium | ✅ Fixed v1.2.0 | Added WPF project to `anime finder.slnx` as startup project |
| 4 | Options screen overlapping text when opened during gameplay | Low | ✅ Fixed | Options draw renders game behind panel when entered from Playing/Paused state |
| 5 | `RedBallShrink` power-up name misleads — actually grants extra life | Low | ⚠️ Open | Documented; enum rename deferred to avoid breaking serialization |
| 6 | Music double-plays when speed slider changed in Options | High | ✅ Fixed v1.2.0 | Same root cause as Bug #1 |
| 7 | Stale `MM_MCINOTIFY` fires within 2 s of new track start | Medium | ✅ Fixed | WndProc 2-second guard rejects notifications arriving < 2 s after last `play` command |

---

## 6. Code Metrics

| Metric | Value |
|--------|-------|
| Main source file | `Form1.vb` |
| Approximate lines (WinForms) | ~2 800 |
| Approximate lines (WPF `GameCanvas.vb`) | ~2 200 |
| External NuGet packages | 0 (WinForms) |
| External asset files | 0 (WinForms/HTML), 179 PNGs (WPF) |
| Game states | 6 (`Menu`, `Playing`, `Paused`, `LevelComplete`, `Options`, `HighScore`) |
| Music styles | 10 |
| SFX packs | 5 |
| Power-up types | 7 |
| Level patterns | 8 |
| Platform builds | 7 |

---

## 7. Architecture Overview

```
anime finder.vbproj  (WinForms — MAIN project)
└── Form1.vb
    ├── #Region Win32 Sound API     — PlaySound, mciSendString, XInputGetState
    ├── #Region Game Constants      — All Const tuning values
    ├── #Region Game State Enum     — Menu / Playing / Paused / LevelComplete / Options / HighScore
    ├── #Region Data Structures     — Ball, Brick, PowerUp, Particle, ScoreRecord (Structures)
    ├── #Region Game Variables      — All mutable state
    ├── #Region Form Events         — Load, Resize, FormClosing, KeyDown/Up, MouseDown/Move/Up, Paint
    ├── #Region Game Loop           — GameTimer_Tick (60 fps)
    ├── #Region Game Init           — StartNewGame, NextLevel, SetupLevel
    ├── #Region Sound System        — WAV gen, MIDI gen, MCI playback, ChangeMusic, WndProc
    ├── #Region Sprite Loading      — FindAssetsDir, LoadSprites, TryGetSprite
    ├── #Region Update Logic        — UpdatePaddle, UpdateBalls, UpdatePowerUps, UpdateParticles, UpdateTimers, UpdateStarField
    ├── #Region Helpers             — Collision, ResetBall, SpawnPowerUp, ApplyPowerUp, HighScores, KeyToChar
    └── #Region Drawing             — All GDI+ render methods

anime finder wpf\  (WPF sub-project)
└── GameCanvas.vb   — Same game logic, WPF DrawingContext rendering, AssetManager integration
└── Scripts\        — AssetManager, AssetImporter, ProceduralAssets, TileMap, EnemyAI, UIManager, etc.
└── Assets\         — 179 CC0 PNGs (Sprites/, Tiles/, UI/, Backgrounds/)

web\index.html      — HTML5 Canvas + Web Audio port (self-contained, ~2 800 lines JS)
versions\           — 7 pre-built platform distributions
```

---

## 8. Lessons Learned

| Lesson | Detail |
|--------|--------|
| Initialize before first render | In WPF, `OnRender` fires during layout before `Loaded`. Any state used in rendering must be initialized in the constructor, not in the `Loaded` handler. |
| MCI stop ≠ playback state cleared | Calling `mciSendString("stop bgmusic")` does not set `_musicPlaying = False`. WndProc will still receive and act on the `MM_MCINOTIFY` message unless the flag is explicitly cleared. |
| Procedural generation simplifies distribution | Zero-asset builds are easier to share, version, and run cross-platform than asset-dependent builds. |
| Single-file architecture has a complexity ceiling | ~2 800 lines in one file is approachable for a course project but would become hard to maintain past ~4 000. Splitting into partial classes by region would be the next step. |
| Platform multiplier compounds effort | Each new platform (7 total) requires re-testing all features. A shared CI pipeline with automated smoke tests would catch regressions before manual testing. |

---

## 9. Team Roles Summary

See `docs/TEAM_PRODUCTION.md` for the full 20-role breakdown. Key contributors by area:

| Area | Roles | Primary Deliverable |
|------|-------|-------------------|
| Leadership | Game Director, Producer | Vision, schedule, milestones |
| Design | Lead Designer, Level Designer, UX Designer | GDD, level patterns, menu flow |
| Engineering | Lead Engineer × 7 specialists | `Form1.vb`, WPF port, audio, physics, UI, build |
| Art | 2D Artist, UI Artist, Animator | Color palettes, visual style, animation specs |
| Audio | Sound Designer | 10 music styles, 5 SFX packs |
| QA | QA Lead, QA Tester | Test plans, bug reports |
| Data | Balance Designer | Numeric constants, difficulty curve |
| Docs | Technical Writer | GDD, README, all markdown docs |

---

## 10. Final Status

| Category | Grade |
|----------|-------|
| Core gameplay (paddle, ball, bricks, lives) | ✅ Complete |
| Power-up system (7 types) | ✅ Complete |
| Scoring & combo system | ✅ Complete |
| Audio (10 music + 5 SFX, procedural) | ✅ Complete |
| High score persistence | ✅ Complete |
| Accessibility (colorblind mode) | ✅ Complete |
| Controller input (XInput) | ✅ Complete |
| Touch/mouse input | ✅ Complete |
| Multi-platform builds (7) | ✅ Complete |
| Documentation | ✅ Complete |
| Known open bugs | 1 minor (power-up enum name) |

---

*Review prepared for CS-120 Final Project — Team Fast Talk — April 2026*
