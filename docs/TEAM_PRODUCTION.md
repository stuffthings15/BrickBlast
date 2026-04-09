# CURTIS LOOP: BRICK BLAST
## 20-Person Team Production Document

**Project:** Curtis Loop — Brick Blast  
**Team:** Team Fast Talk  
**Course:** CS-120  
**Status:** Step 1 — Team Structure (awaiting role-by-role walkthrough)

---

## TEAM STRUCTURE — 20 Roles

### Leadership (2)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 1 | **Game Director** | Owns the creative vision. Approves gameplay feel, visual style, audio direction. Resolves cross-department conflicts. Signs off on each milestone. | Vision document, milestone sign-offs, creative briefs | Miro, Google Docs, Playtesting sessions |
| 2 | **Producer** | Manages schedule, assigns tasks, tracks blockers. Runs weekly standups. Owns the PLAN.md and sprint boards. | Sprint plans, burndown charts, risk log, team velocity reports | Trello/Jira, GitHub Projects, Excel |

### Design (3)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 3 | **Lead Game Designer** | Defines core loop, scoring formula, combo system, power-up balance, difficulty curve. Writes the GDD. | GDD (docs/GDD.md), balance spreadsheets, gameplay flow diagrams | Google Sheets, draw.io, playtesting |
| 4 | **Level Designer** | Designs brick layouts per level. Defines which rows get 2-hit bricks, when new patterns appear, brick grid spacing. Tunes `BRICK_ROWS`, `BRICK_COLS`, `BRICK_TOP_OFFSET`. | Level spec sheets, brick grid templates, difficulty ramp chart | Excel grid templates, graph paper, VB.NET `SetupLevel()` |
| 5 | **UX Designer** | Designs menu flow, options screen layout, HUD placement, high score entry flow. Ensures controls feel responsive (paddle speed, input buffering). | Wireframes for all 6 game states, HUD mockups, input response specs | Figma, paper prototypes |

### Engineering (7)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 6 | **Lead Engineer** | Owns `Form1.vb` architecture. Defines region structure, data structures (`Ball`, `Brick`, `PowerUp`, `Particle`). Reviews all code changes. | Architecture doc, code review approvals, struct definitions | Visual Studio, Git |
| 7 | **Gameplay Programmer** | Implements `UpdatePaddle()`, `UpdateBalls()`, `UpdatePowerUps()`, `ApplyPowerUp()`. Owns paddle movement, speed boost toggle, multi-ball spawning. | Working paddle/ball/power-up systems, `GameTimer_Tick` loop | Visual Studio, GDI+ |
| 8 | **Physics Programmer** | Implements `BallIntersectsRect()`, ball-paddle angle reflection, ball-brick collision with axis-based bounce detection, wall bouncing. | Collision detection functions, bounce angle math, edge-case fixes | Visual Studio, math reference sheets |
| 9 | **UI Programmer** | Implements `DrawHUD()`, `DrawMenu()`, `DrawOptions()`, `DrawHighScore()`, `DrawOverlay()`. Owns all text rendering, volume bars, GraphicsPath title text. | All Draw* methods, menu navigation, settings UI with mouse/keyboard | Visual Studio, GDI+, Segoe UI font |
| 10 | **Audio Programmer** | Implements `GenerateWav()`, `GenerateMidiBytes()`, `PlaySFX()`, all MCI music playback. Owns WAV synthesis, MIDI generation, 10 music styles, 5 SFX packs. | Sound system (in-memory WAV, temp MIDI files), volume control | Visual Studio, winmm.dll P/Invoke, MIDI spec |
| 11 | **Systems Programmer** | Implements `GameState` enum, state transitions, `Form1_KeyDown` input routing, `StartNewGame()`, `NextLevel()`, `CheckLevelComplete()`, high score persistence. | State machine, save/load system (`highscores.json`), game flow | Visual Studio, System.Text.Json |
| 12 | **Build Engineer** | Configures `dotnet publish` for self-contained single-file exe. Manages `.gitignore`, GitHub repo, submission zip. Ensures build works without VS installed. | `pipelines/exe/anime finder.exe`, `BrickBlast_Submit.zip`, GitHub repo | dotnet CLI, gh CLI, PowerShell |

### Art (3)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 13 | **2D Visual Artist** | Defines color palettes — both standard `_rowColors` (7 gradient pairs) and colorblind `_colorblindColors` (CBF-safe). Picks particle colors, glow tints. | Color palette specs (RGB values), colorblind mode palette, visual style guide | Color picker tools, Coblis simulator |
| 14 | **UI Artist** | Designs the look of menus, options panel, high score panel. Defines rounded rectangle styles, gradient directions, panel opacity values, font sizes. | UI style specs (corner radii, alpha values, font weights), HUD layout | Figma/Photoshop, GDI+ reference |
| 15 | **Animator** | Defines particle behaviors (spawn count, speed range, gravity, lifetime). Designs screen shake parameters, combo text fade, GET READY countdown pulse, star field twinkle. | Animation spec sheet with frame-by-frame values for all effects | Spreadsheet, timing calculators |

### Audio (1)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 16 | **Sound Designer** | Composes the 10 music style note sequences (`GetMusicData`). Defines SFX frequency/duration pairs for 5 style packs (`_sfxData`). Tunes BPM per style, MIDI instruments. | 10 music compositions (freq/dur arrays), 5 SFX packs (freq/dur arrays), instrument map | Piano/keyboard for pitch reference, MIDI spec |

### QA (2)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 17 | **QA Lead** | Writes test plans for all 6 game states. Tests edge cases: 0 lives, max combo, all bricks cleared, rapid key input, window resize mid-game. | Test plan document, bug report log, regression checklist | Excel, screen recorder |
| 18 | **QA Tester** | Executes test plans. Plays through levels 1-10+. Tests all 7 power-ups, all 10 music styles, all 5 SFX packs, colorblind mode, all 4 window sizes. | Bug reports with repro steps, playtest session notes, performance observations | Game exe, stopwatch, notepad |

### Data & Analytics (1)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 19 | **Game Balance Designer** | Tunes all numeric constants: `INITIAL_BALL_SPEED` (8.25), speed scaling (1.05^level), power-up drop rate (54%), combo window (90 frames), combo cap (8x), paddle speed (26), mega paddle duration (480 frames). | Balance spreadsheet with formulas, difficulty curve graph, playtest data analysis | Excel, calculator, playtesting |

### Documentation (1)

| # | Title | Responsibilities | Deliverables | Tools |
|---|-------|-----------------|--------------|-------|
| 20 | **Technical Writer** | Writes all project documentation: GDD, PROJECT_DOCS, STORY, MINDSET, PLAN, TEAM_PRODUCTION. Maintains README for GitHub. Ensures docs match actual code values. | docs/GDD.md, docs/PROJECT_DOCS.md, pipelines/docs/story_assets/STORY.md, pipelines/mindset/MINDSET.md, requirements/PLAN.md, this document | Markdown, VS Code, GitHub |

---

## DEPARTMENT DEPENDENCY MAP

```
                    ┌─────────────┐
                    │ Game Director│
                    │   (Role 1)  │
                    └──────┬──────┘
                           │ vision
                    ┌──────┴──────┐
                    │  Producer   │
                    │  (Role 2)   │
                    └──────┬──────┘
                           │ schedule
          ┌────────────────┼────────────────┐
          │                │                │
   ┌──────┴──────┐  ┌─────┴─────┐  ┌──────┴──────┐
   │   Design    │  │Engineering│  │   Art/Audio  │
   │ (3, 4, 5)  │  │(6-12)     │  │ (13-16)     │
   └──────┬──────┘  └─────┬─────┘  └──────┬──────┘
          │               │               │
          └───────┬───────┴───────┬───────┘
                  │               │
           ┌──────┴──────┐ ┌─────┴──────┐
           │  QA / Test  │ │   Data     │
           │  (17, 18)   │ │   (19)     │
           └─────────────┘ └────────────┘
                  │
           ┌──────┴──────┐
           │  Tech Writer│
           │    (20)     │
           └─────────────┘
```

---

## WORKFLOW

Each role will be walked through individually in this order:

| Phase | Roles | Focus |
|-------|-------|-------|
| **Phase 1: Vision** | 1 (Director), 2 (Producer) | Goals, schedule, milestones |
| **Phase 2: Design** | 3 (Game Designer), 4 (Level Designer), 5 (UX Designer) | Rules, layouts, flows |
| **Phase 3: Core Engineering** | 6 (Lead), 7 (Gameplay), 8 (Physics) | Architecture, movement, collisions |
| **Phase 4: Systems Engineering** | 9 (UI), 10 (Audio), 11 (Systems), 12 (Build) | Menus, sound, state, publish |
| **Phase 5: Art & Audio** | 13 (Visual), 14 (UI Art), 15 (Animator), 16 (Sound) | Colors, layouts, effects, music |
| **Phase 6: Quality** | 17 (QA Lead), 18 (Tester), 19 (Balance), 20 (Writer) | Testing, tuning, documentation |

For each role:
1. What they build for THIS game
2. Their concrete output
3. Three meaningful improvements
4. Improved output implemented

---

## CURRENT GAME SYSTEMS SUMMARY

For reference — these are the systems the team works with:

| System | Owner | Key Constants |
|--------|-------|---------------|
| Paddle | Gameplay (7) | Width=240, Height=14, Speed=26, Y_Offset=50 |
| Ball | Physics (8) | Radius=8 (4-20), Speed=8.25 (×1.05/level, range 4-25) |
| Bricks | Level Design (4) | 7×12 grid, Padding=4, Top=70, Points=(7-row)×10 |
| Power-ups | Gameplay (7) | Size=45, Speed=3.0, 7 types, 54% drop rate |
| Particles | Animator (15) | Count=8, Life=20-40, Gravity=0.1, Speed=1.5-4.5 |
| Combo | Balance (19) | Window=90 frames, Cap=8×, Sound=800+combo×100 Hz |
| Music | Audio (10/16) | 10 styles, MIDI gen, MCI playback, auto-advance |
| SFX | Audio (10/16) | 5 packs, WAV gen, 22050Hz 8-bit mono square wave |
| State Machine | Systems (11) | 6 states: Menu, Playing, Paused, LevelComplete, Options, HighScore |
| High Scores | Systems (11) | JSON to %AppData%\BrickBlast\, top 10, 12-char names |
| Rendering | UI (9) | 1200×867 logical, ScaleTransform, GDI+ AntiAlias |
| Build | Build (12) | win-x64, self-contained, single-file, ~110 MB |

---

## STEP 1 COMPLETE

**The full 20-person team is defined above.**

Reply **"Role 1"** (or just **"go"**) to begin the role-by-role deep dive starting with the **Game Director**.

Each role will include:
- What they build
- Their concrete output
- 3 improvements
- Improved output

I will stop after each role and wait for your confirmation.
