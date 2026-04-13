# Game Development Document

**Game Name:** Curtis Loop: Brick Blast  
**Logo / Cover Image:** Title rendered at runtime — "TEAM FAST TALK" above "BRICK BLAST" with neon glow outlines and gradient fill (blue→pink)  
**Version:** 1.2.0  
**Team:** Team Fast Talk — CS-120

---

## 1. Story & Gameplay

### Story (Brief)
A lone energy paddle defends the edge of a neon void, deflecting a plasma ball to shatter crystalline bricks that have formed barriers across a digital cosmos. The player destroys every brick to push deeper into the void, where bricks get tougher and the ball accelerates.

### Story (Detailed)
In the far reaches of a digital cosmos — visible as a twinkling 120-star field scrolling behind the play area — rows of crystalline bricks have formed barriers across the void. The player controls a glowing energy paddle that deflects a plasma ball upward to shatter them. As the player advances through levels, the top rows become reinforced (requiring 2 hits), and the ball accelerates by 5% per level. Mysterious power-up orbs fall from destroyed bricks, granting abilities like multi-ball bursts, paddle enlargement, ball-size changes, and speed modifications. The player starts with 10 lives. Losing all lives triggers a high-score entry screen; the leaderboard persists to `%AppData%\BrickBlast\highscores.json`. The goal: survive as long as possible, build the highest score, and earn a place on the leaderboard.

**Characters (abstract):**

| Entity | Role | Description |
|--------|------|-------------|
| Paddle | Player avatar | Horizontal bar (240 px, or 720 px with Mega Paddle). Controlled by keyboard, XInput gamepad, or touch/mouse drag |
| Ball | Projectile | Bouncing sphere (radius 4–20 px). Speed 4–25 units. Tints yellow/orange during speed boost |
| Bricks | Targets | 7 rows × 12 cols. Color-coded by row. 1 or 2 hits to destroy. 8 level patterns |
| Power-ups | Collectibles | 7 types. 54% drop rate, fall at 3.0 speed, collected on paddle contact |

### Gameplay (Brief)
Move a paddle left/right to bounce a ball into a grid of bricks. Destroy all bricks to complete the level. Catch falling power-ups for bonuses. Build combos for up to 8× score multipliers. Lose all 10 lives and the game ends.

### Gameplay (Detailed)

**Core Loop:**
```
MENU → SPACE / A button / Tap → PLAYING
  → Destroy all bricks → LEVEL COMPLETE → SPACE → Next Level (5% faster)
  → Lose all lives → HIGH SCORE ENTRY → type name → ENTER → MENU
```

**Controls (all platforms):**

| Input | Keyboard | Gamepad (XInput) | Touch / Mouse |
|-------|----------|------------------|---------------|
| Move paddle | ← → / A D | D-pad L/R, Left stick | Drag on screen |
| Start / Resume | SPACE | A button | Tap |
| Pause | P / ESC | B button | — |
| Speed boost | F / SPACE | Y button | — |
| Options | H / O | Start button | — |

**Scoring:**
- Base = `(7 − row) × 10` per brick (top = 70 pts, bottom = 10 pts)
- Combo: consecutive hits within 90 frames → multiplier up to 8×
- Actual = `base × min(combo, 8)`

**Power-ups:**

| Type | Color | Effect |
|------|-------|--------|
| Ball Grow | Blue | Ball radius +6 (max 20) |
| Extra Life | Red | +1 life (max 10) |
| Multi-Ball | Green | Spawns 8 additional balls |
| Ball Shrink | Yellow | Ball radius −6 (min 4) |
| Mega Paddle | Purple | 3× paddle width for 8 sec |
| Ball Slow | Orange | Ball speed × 0.85 |
| Ball Fast | Pink | Ball speed × 1.15 |

---

## 2. Assets Needed

### Visual Assets

**2D Textures — WinForms (procedural, zero external files):**

| Asset | Implementation |
|-------|---------------|
| Paddle | Rounded rectangle, linear gradient, highlight strip, drop-shadow ellipse |
| Ball | Circle with 5-layer glow rings, gradient fill, specular highlight |
| Bricks | Rounded rectangles, row-based gradient colors (7 pairs), white shine overlay |
| Power-ups | Colored circles with bobbing animation, text symbol labels |
| Particles | Colored circles, gravity-affected, fade over 20–40 frames |
| Star field | 120 dots, random brightness, sine-wave twinkle, scrolling downward |

**2D Textures — WPF (160 CC0 sprites):**

| Pack | Source | License | Count |
|------|--------|---------|-------|
| Breakout Tile Set Free | OpenGameArt (Jamie Cross) | CC-BY 4.0 | 60 |
| OGA Breakout Full Kit | OpenGameArt | CC0 | 50 |
| OGA Retro Breakout | OpenGameArt | CC0 | ~10 |
| Kenney Game Icons | kenney.nl | CC0 | 30 |
| Kenney UI Pack | kenney.nl | CC0 | ~15 |
| Kenney Input Prompts | kenney.nl | CC0 | ~14 |
| **Total** | | | **179 PNGs** |

### Characters

- **Character #1 — Paddle:** Player avatar. Horizontal energy bar, blue gradient (or CB-yellow). Abilities: move left/right, Mega Paddle power-up (3× width for 8 sec).
- **Character #2 — Ball:** Plasma projectile. White-blue sphere with 5-layer glow (or yellow-orange during speed boost). Abilities: bounces off walls/paddle/bricks, grows/shrinks via power-ups.
- **Character #3 — Power-up Orbs:** 7 colored circles that drop from destroyed bricks. Each grants a distinct ability on paddle contact.

### Environmental Art

- **Example #1 — Star field:** 120 dots with parallax scrolling (speeds 0.2–0.8), sine-wave brightness twinkle. Solid dark background (RGB 8, 8, 20).
- **Example #2 — Brick grid:** 7 rows × 12 cols, row-colored with gradient pairs. 8 level patterns: grid, checkerboard, diamond, fortress, stripes, cross, border, random.
- **Example #3 — Play area:** 1200 × 867 logical pixels, scaled to window. Window sizes: 900×650, 1200×867, 1500×1083, 1800×1300.

### Audio Assets

All audio is generated procedurally at runtime — no external audio files.

**Ambient Sounds:**
- Star field background has no ambient audio; music fills the role.

**Player Sounds (5 SFX packs: Classic / Zelda / Mega Man / Tetris / Retro Arcade):**
- Movement: N/A (paddle is silent)
- Collision: wall hit, paddle hit, brick hit, power-up collect
- Injured / Death: ball lost sound, combo sound (rising frequency)
- Level win: victory fanfare

**Music (10 MIDI styles from MusicXML reference P1–P10):**

| Slot | Style | BPM | MIDI Instrument |
|------|-------|-----|-----------------|
| 0 | Mega Man Dr. Wily | 160 | Square wave (80) |
| 1 | Zelda Overworld | 120 | Flute (73) |
| 2 | Tetris Type A | 140 | Music box (10) |
| 3 | Mario Overworld | 100 | Square wave (80) |
| 4 | Castlevania | 150 | Organ (19) |
| 5 | Final Fantasy | 90 | Harp (46) |
| 6 | Sonic Green Hill | 140 | Square wave (80) |
| 7 | Metroid Brinstar | 130 | Synth pad (88) |
| 8 | Mega Man Elec | 145 | Square wave (80) |
| 9 | Zelda Dungeon | 100 | Flute (73) |

---

## 3. Code / Scripting

### Player Scripts
- **UpdatePaddle()** — Reads keyboard (`_leftPressed`/`_rightPressed`), gamepad (`_gamepadLeft`/`_gamepadRight` via XInput D-pad + left stick), and touch (`_touchActive`/`_touchX` via MouseMove). Clamps to screen bounds.
- **PollGamepad()** — Called every frame. P/Invokes `XInputGetState`. Edge-detects button presses for one-shot actions (A/B/Y/Start/LB/RB).
- **Speed Boost** — F, SPACE, or Y button toggles `_speedBoost`. Ball movement ×2.

### NPC Scripts
- No NPCs. Bricks are static targets. Power-ups have simple gravity-fall behavior.

### Environment / Ambient Scripts
- **Star field** — 120 stars with random parallax speed, sine-wave brightness twinkle via `UpdateStarField()`.
- **Level patterns** — 8 patterns generated in `SetupLevel()` using row/col math.
- **Music system** — `PreGenerateAllMusic()` pre-renders 10 MIDI files to temp directory. `StartMusic()` opens via MCI `mciSendString`. `WndProc` handles `MM_MCINOTIFY` to auto-advance styles. Timer cancellation prevents double-playback.

---

## 4. Animation

### Environment Animations
- Star field: 120 dots scroll downward, brightness pulses via `sin(frame × 0.05 + i) × 40`
- Screen shake: 3 frames on brick destroy, 10 frames on ball lost (random ±4 px offset)

### Character Animations
- **Player (Paddle):** Instant 26 px/frame movement. Mega Paddle snaps to 3× width. Drop-shadow tracks position.
- **Ball:** Continuous motion at `Speed × direction` (×2 during boost). 5-layer glow rings. Gradient white→blue (normal) or yellow→orange (boost).
- **Brick destruction:** 8 colored particles spawn at brick center. Random angle, speed 1.5–4.5, gravity 0.1/frame, life 20–40 frames.
- **Power-ups:** Vertical bobbing via `sin(frame × 0.1) × 3`.
- **GET READY:** Pulsing font 58–68 pt via sine, gold text.
- **Combo text:** "COMBO x{n}!" fades over 90 frames with glow.

---

## 5. Development Schedule

| Object / Feature | Time Scale | Milestones |
|-----------------|------------|------------|
| Core engine (paddle, ball, bricks, collision) | Week 1–2 | M1: Ball bounces / M2: Paddle collision / M3: Brick destruction |
| Power-ups & progression (7 types, multi-level, combo) | Week 3 | M1: Power-up spawning / M2: All 7 types / M3: Combo scoring |
| Audio system (WAV SFX, MIDI music, MCI playback) | Week 4 | M1: SFX generation / M2: 10 MIDI styles / M3: Options screen |
| High scores & accessibility (JSON, colorblind, HUD) | Week 5 | M1: Score entry / M2: JSON persistence / M3: Colorblind mode |
| Multi-platform export (WinForms, WPF, HTML5, mobile PWA) | Week 6 | M1: Win EXE / M2: WPF + 160 sprites / M3: 7 platform builds |
| Input systems (XInput gamepad, touch/mouse, music fixes) | Week 7 | M1: Gamepad polling / M2: Touch paddle drag / M3: Double-play fix |

---

## 6. Notes / References

### Inspirations

| Game | Influence |
|------|-----------|
| Atari Breakout (1976) | Core paddle + ball + bricks mechanic |
| Arkanoid (1986) | Power-up system, multi-ball, paddle width changes |
| DX-Ball 2 (1999) | Combo scoring, visual effects, multiple power-up types |
| Mega Man 2 (1988) | Music styles 0 + 8, SFX pack 2 |
| The Legend of Zelda (1986) | Music styles 1 + 9, SFX pack 1 |
| Tetris (1984) | Music style 2, SFX pack 3, retro aesthetic |
| Super Mario Bros (1985) | Music style 3 |
| Castlevania (1986) | Music style 4, gothic organ arpeggios |
| Final Fantasy (1987) | Music style 5, sweeping harp prelude |
| Sonic the Hedgehog (1991) | Music style 6, fast pop energy |
| Metroid (1986) | Music style 7, sparse eerie atmosphere |

### Tools

| Tool | Usage |
|------|-------|
| Visual Studio Community 2026 | IDE, build, publish |
| VB.NET / .NET 10 | Language and runtime |
| Windows Forms (GDI+) | Rendering (main project) |
| WPF (DrawingContext) | Rendering (WPF sub-project) |
| HTML5 Canvas + Web Audio | Browser versions |
| winmm.dll (P/Invoke) | WAV playback and MIDI via MCI |
| xinput1_4.dll (P/Invoke) | XInput gamepad support |
| System.Text.Json | High-score serialization |
| MusicXML 3.1 | Reference for music style definitions (TextFile1.txt) |

### References
- Music reference file: `TextFile1.txt` (MusicXML 3.1, 15 parts, P1–P10 used)
- Existing GDD: `docs/GDD.md`
- Team production: `docs/TEAM_PRODUCTION.md`
- Repository: https://github.com/stuffthings15/BrickBlast

---

*Document prepared for CS-120 Final Project — Team Fast Talk*
