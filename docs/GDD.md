# CURTIS LOOP: BRICK BLAST
## Game Development Document (GDD)

**Project:** Curtis Loop — Brick Blast  
**Team:** Team Fast Talk  
**Course:** CS-120  
**Platform:** Windows (Desktop)  
**Engine:** VB.NET Windows Forms (GDI+) + WPF (DrawingContext)  
**Version:** 1.1

---

## 1. Story & Gameplay

### Story (Brief)
A lone paddle stands at the edge of a neon void, defending the bottom of the screen against an endless cascade of colorful bricks. The player must destroy every brick to advance deeper into the void, where bricks become tougher and the ball moves faster.

### Story (Detailed)
In the far reaches of a digital cosmos — visible as a twinkling star field scrolling behind the play area — rows of crystalline bricks have formed barriers across the void. The player controls a glowing energy paddle that deflects a plasma ball upward to shatter the bricks. As the player advances through levels, the top rows of bricks become reinforced (requiring 2 hits), and the ball accelerates by 5% per level. Mysterious power-up orbs fall from destroyed bricks, granting abilities like multi-ball bursts, paddle enlargement, and speed modifications. The player's goal is to survive as long as possible, build the highest score, and earn a place on the leaderboard.

### Gameplay (Brief)
Move a paddle left and right to bounce a ball into a grid of bricks. Destroy all bricks to complete the level. Catch falling power-ups for bonuses. Lose all 10 lives and the game ends.

### Gameplay (Detailed)

**Core Loop:**
```
MENU → Press SPACE → PLAYING → Destroy all bricks → LEVEL COMPLETE
  → Press SPACE → Next Level (harder) → PLAYING → ...
  → Lose all lives → HIGH SCORE ENTRY → MENU
```

**Controls:**
| Input | Action |
|-------|--------|
| ← → / A D | Move paddle left/right |
| SPACE | Start game / Resume / Next level |
| P / ESC | Pause / Unpause |
| F | Toggle 2x speed boost |
| H / O | Open Options screen |

**Rules:**
- Player starts with 10 lives
- Ball bounces off walls (left, right, top) and the paddle
- Ball falling past the bottom of the screen costs 1 life
- After losing a life, a 3-second "GET READY" countdown plays before the ball launches
- All bricks destroyed = level complete; next level loads with 5% faster ball speed
- On level 2+, the top 2 rows of bricks require 2 hits to destroy
- Power-ups drop from destroyed bricks with a 54% chance
- Game over triggers high score entry screen

**Scoring:**
- Each brick awards `(BRICK_ROWS - row) × 10` base points (top rows = 70 pts, bottom = 10 pts)
- Combo multiplier: hitting bricks in rapid succession (within 90 frames / ~1.5 seconds) increases a combo counter
- Score per brick = `base_points × min(combo, 8)` — max 8x multiplier
- Combo resets when the timer expires or the ball is lost

---

## 2. Assets Needed

### Visual Assets
All visuals are rendered procedurally using GDI+ — no external image files.

| Asset | Implementation |
|-------|---------------|
| Paddle | Rounded rectangle with linear gradient (blue or yellow in colorblind mode), highlight strip, drop shadow ellipse |
| Ball | Circle with 5-layer glow rings, gradient fill, specular highlight. Tints yellow/orange during speed boost |
| Bricks | Rounded rectangles with row-based gradient colors (7 color pairs), white shine overlay. Hit counter displayed for multi-hit bricks |
| Power-ups | Colored circles with bobbing animation, text symbol labels (font size 12) |
| Particles | Colored circles that spawn on brick destruction, ball loss, paddle hit, and power-up collection. Gravity-affected, fade over 20-40 frames |
| Star field | 120 dots with random brightness, twinkle via sine wave, scrolling downward |
| HUD | Score (top-left), Level (top-center), Lives as hearts (top-right), speed boost indicator, ball size indicator, paddle timer |
| Menus | GraphicsPath-rendered title text with glow outlines and gradient fills |

### Characters (Abstract)
| Entity | Role | Description |
|--------|------|-------------|
| Paddle | Player avatar | Horizontal bar controlled by keyboard. Width changes with Mega Paddle power-up (3x for 8 seconds) |
| Ball | Projectile | Bouncing sphere. Radius changes with grow/shrink power-ups (4–20 px range). Speed range: 4–25 units |
| Bricks | Targets | 7 rows × 12 columns grid. Color-coded by row. 1 or 2 hits to destroy |
| Power-ups | Collectibles | 7 types, fall at 3.0 speed, collected by paddle contact |

### Environment
- **Background:** Black void with animated star field (120 stars, parallax scrolling)
- **Play area:** 1200 × 867 logical pixels, scaled to window size
- **Window sizes:** 900×650, 1200×867, 1500×1083, 1800×1300

### Audio Assets
All audio is generated procedurally at runtime — no external audio files.

| Audio | Type | Implementation |
|-------|------|---------------|
| SFX | WAV (in-memory) | Square wave synthesis via `GenerateWav()`. 5 style packs: Classic, Zelda, Mega Man, Tetris, Retro Arcade. Each pack defines frequencies/durations for: wall hit, paddle hit, brick hit, power-up, ball lost, level win |
| Music | MIDI (temp file) | 10 styles generated via `GenerateMidiBytes()`: Zelda, Mega Man, Tetris, Pac-Man, Space Invaders, Castlevania, Metroid, Galaga, Contra, Double Dragon. Played via Windows MCI (mciSendString). Auto-advances to next style on song completion |
| High Score Music | MIDI | Metroid Atmosphere style, loops during score entry |
| Combo SFX | WAV | Rising frequency: 800 + combo×100 Hz, duration scales with combo |

---

## 3. Code / Scripting

### Architecture
Single-file architecture: `Form1.vb` (VB.NET Windows Forms)  
Organized into `#Region` blocks:

| Region | Purpose |
|--------|---------|
| Win32 Sound API | P/Invoke for PlaySound and mciSendString |
| Game Constants | All tuning values as `Const` |
| Game State Enum | Menu, Playing, Paused, LevelComplete, Options, HighScore |
| Data Structures | Ball, Brick, PowerUp, Particle, ScoreRecord (all `Structure`) |
| Game Variables | All mutable game state |
| Form Events | Load, KeyDown/Up, Paint, MouseDown |
| Game Loop | `GameTimer_Tick` — single 60fps timer |
| Game Init | StartNewGame, NextLevel, SetupLevel |
| Sound System | WAV generation, MIDI generation, playback control |
| Update Logic | Paddle, Balls, PowerUps, Particles, Timers, StarField |
| Helpers | Collision, ResetBall, SpawnPowerUp, ApplyPowerUp, HighScores |
| Drawing | All GDI+ rendering methods |

### Player Systems
- **Paddle Movement:** `UpdatePaddle()` — reads `_leftPressed`/`_rightPressed` booleans set by KeyDown/KeyUp. Moves paddle at 26 px/frame. Clamped to screen bounds.
- **Speed Boost:** F key toggles `_speedBoost`. When active, ball movement is multiplied by 2x. Ball tints yellow/orange as visual indicator.

### Game Logic Systems
- **Game Loop:** `GameTimer_Tick` fires every ~16ms (60fps). Updates star field, checks high score delay, then if Playing: updates paddle → balls → power-ups → particles → timers → checks level complete. Calls `Invalidate()` to trigger repaint.
- **Level Progression:** `CheckLevelComplete()` — if all bricks are dead, state changes to LevelComplete. NextLevel increments `_level`, resets paddle/combo, calls `SetupLevel()`.
- **Power-up System:** 7 types with distinct effects:

| Type | Color | Effect |
|------|-------|--------|
| BlueBallGrow | Blue | Ball radius +6 (max 20) |
| RedBallShrink | Red | +1 life (max 10) |
| GreenMultiBall | Green | Spawns 8 additional balls |
| YellowBallShrink | Yellow | Ball radius -6 (min 4) |
| PurplePaddleMega | Purple | 3x paddle width for 8 seconds |
| OrangeBallSlow | Orange | Ball speed ×0.85 |
| PinkBallFast | Pink | Ball speed ×1.15 |

### Collision Systems
- **Ball vs Walls:** Simple bounds checking. Ball reverses DX or DY on contact with left/right/top edges.
- **Ball vs Paddle:** `BallIntersectsRect()` uses circle-to-rectangle nearest-point distance. On hit, the ball's exit angle is calculated from where it hit the paddle: `angle = 150 - hitPosition × 120` (degrees). This gives directional control — hitting the paddle edge sends the ball at a steep angle.
- **Ball vs Brick:** Same `BallIntersectsRect()` check. On collision, determines whether to flip DX or DY by comparing overlap distances on each axis (`Math.Min(overlapLeft, overlapRight) < Math.Min(overlapTop, overlapBottom)`).
- **Power-up vs Paddle:** Standard rectangle intersection (`RectangleF.IntersectsWith`).

### Score System
- Base points per brick: `(7 - row) × 10` — top rows are worth more
- Combo multiplier: consecutive brick hits within 90 frames stack a combo counter
- Actual score: `basePoints × min(combo, 8)` — capped at 8x
- Combo text "COMBO x{n}!" displayed at screen center with glow effect
- High scores stored in `%AppData%\BrickBlast\highscores.json`, top 10, persisted across sessions

### Reset / Game Over System
1. Ball falls below screen → `b.Active = False`, particles spawn, combo resets
2. If no active balls remain → lose 1 life, screen shakes for 10 frames
3. If lives > 0 → `ResetBall()` places new ball above paddle, starts 180-frame (3 second) GET READY countdown
4. If lives = 0 → `_highScore` updated, state transitions to Paused for 60 frames, then to HighScore screen
5. High Score screen: type name (max 12 chars) → ENTER to save → SPACE to return to Menu

---

## 4. Animation

### Paddle Movement
- Immediate response: 26 pixels per frame horizontal movement
- Mega Paddle: width instantly snaps to 720px (3×240), timer counts down on HUD in purple text
- Drop shadow ellipse moves with paddle for grounding effect
- Highlight strip on top half creates 3D appearance

### Ball Motion
- Continuous movement at `Speed × direction` per frame (or ×2 during speed boost)
- 5-layer glow rings (sizes 20→4, opacity scaling) create trailing light effect
- Gradient fill: white→light blue (normal) or yellow→orange (speed boost)
- Specular highlight ellipse on upper-left quadrant

### Brick Destruction Effects
- On final hit: brick removed, 8 colored particles spawn at brick center
- Particles: random angle, speed 1.5–4.5, gravity 0.1/frame, life 20-40 frames
- Particle size and opacity fade linearly over lifetime
- Screen shakes for 3 frames on brick destroy
- Screen shakes for 10 frames on ball lost
- Multi-hit bricks flash to gray/white on first hit

### Other Animations
- **Star field:** 120 stars scroll downward at varying speeds (0.2–0.8), brightness twinkles via sine wave
- **GET READY countdown:** Pulsing font size (58–68pt via sine), gold text with shadow layers, "GET READY!" subtitle
- **Level Complete:** Title font size pulses 34–46pt, color pulses between white and gold
- **Power-ups:** Vertical bobbing via `sin(frameCount × 0.1) × 3`
- **Combo text:** Fade out over 90 frames, gold glow shadow offset ±2px
- **Menu title:** GraphicsPath text with concentric glow outlines (10→2px) and gradient fill

---

## 5. Development Schedule

### Week 1 — Foundation
- [x] Project setup (VB.NET Windows Forms)
- [x] Paddle movement and screen bounds
- [x] Ball spawning with random upward angle
- [x] Ball bouncing off walls
- [x] Basic brick grid rendering
- **Deliverable:** Ball bounces, paddle moves, bricks display

### Week 2 — Core Gameplay
- [x] Ball-paddle collision with angle reflection
- [x] Ball-brick collision with axis-based bounce
- [x] Brick destruction and scoring
- [x] Lives system (10 lives, ball reset)
- [x] Level complete detection
- **Deliverable:** Fully playable single level

### Week 3 — Power-ups & Progression
- [x] Power-up spawning (54% drop rate)
- [x] 7 power-up types implemented
- [x] Multi-level progression (speed scaling, 2-hit bricks)
- [x] Combo system with multiplier cap
- [x] Particle effects on destruction
- **Deliverable:** Multi-level game with power-ups and combos

### Week 4 — Audio & Polish
- [x] Procedural WAV SFX generation (5 style packs)
- [x] Procedural MIDI music generation (10 styles)
- [x] Music playback via MCI with auto-advance
- [x] Options screen (volume, speed, styles, colorblind, window size)
- [x] Star field background animation
- **Deliverable:** Full audio, settings screen, visual polish

### Week 5 — High Scores & Accessibility
- [x] High score entry screen with keyboard input
- [x] JSON persistence to AppData
- [x] Colorblind mode (distinct palette + Unicode symbols on bricks)
- [x] GET READY countdown after life lost
- [x] Speed boost ball tinting
- [x] Mega Paddle HUD timer
- **Deliverable:** Persistent leaderboard, accessibility features

### Week 6 — Export & Documentation
- [x] Self-contained .exe publish (win-x64, ~110 MB)
- [x] Submission zip created (~45 MB)
- [x] GDD written (this document)
- [x] Project documentation and folder structure
- [x] Git repository initialized and pushed to GitHub
- [x] WPF port completed
- **Deliverable:** Final multi-platform submission package

---

## 6. Notes / References

### Inspirations
| Game | Influence |
|------|-----------|
| Atari Breakout (1976) | Core paddle + ball + bricks mechanic |
| Arkanoid (1986) | Power-up system, multi-ball, paddle width changes |
| DX-Ball 2 (1999) | Combo scoring, visual effects, multiple power-up types |
| Tetris (1984) | Music style #3, overall retro arcade aesthetic |
| Mega Man series | Music style #2, SFX style #3 |

### Tools
| Tool | Usage |
|------|-------|
| Visual Studio Community 2026 | IDE, build, publish |
| VB.NET / .NET 10 | Language and runtime |
| Windows Forms (GDI+) | Rendering and input |
| winmm.dll (P/Invoke) | WAV playback and MIDI via MCI |
| System.Text.Json | High score serialization |
| GitHub CLI (gh) | Repository management |

### Technical Notes
- **Rendering:** All drawing is done in `Form1_Paint` using GDI+ with `SmoothingMode.AntiAlias`. A logical coordinate system (1200×867) is scaled to the actual window size via `ScaleTransform`.
- **Sound:** WAV files are generated in-memory as byte arrays (22050 Hz, 8-bit mono, square wave with ADSR envelope) and played via `PlaySound` with `SND_ASYNC | SND_MEMORY`. MIDI files are written to temp directory and played via `mciSendString`.
- **State Machine:** Game uses a simple `GameState` enum. All input handling branches on current state. The game timer runs continuously; only the `Playing` state updates gameplay objects.
- **No External Dependencies:** The entire game is a single `.vb` file with no NuGet packages, no external assets, and no image files. Everything is generated procedurally.

---

*Document generated for CS-120 Final Project — Team Fast Talk*
