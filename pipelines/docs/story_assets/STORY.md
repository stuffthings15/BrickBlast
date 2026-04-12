# Story Assets

## Game Story
CURTIS LOOP: BRICK BLAST — A retro arcade experience across 7 platforms.
The player controls a paddle, breaking bricks across infinite levels.
Each level gets faster. Power-ups help or hinder. Survive as long as possible.

## Characters
- **Player Paddle** — The blue bar at the bottom. Moves left/right. Turns yellow in colorblind mode.
- **The Ball** — Bounces off everything. Radius changes with power-ups (4–20 px). Tints gold during speed boost.
- **Bricks** — 7 rows × 12 columns, colored by row. Top rows take 2+ hits on higher levels. 8 layout patterns.
- **Power-Ups** — 7 types: colored orbs with symbols that fall from destroyed bricks.

## World
- Neon void with 120-star animated parallax starfield
- Logical play area: 1200 × 867 pixels, scaled to window
- Dark background (#0F0F1E) with glowing UI elements

## Design Notes
- Colorblind mode uses distinct Unicode shapes + CBF-safe palette
- Music cycles through 10 retro game styles automatically
- Screen shake on ball loss (10 frames) and brick destroy (3 frames)
- Combo multiplier rewards fast consecutive brick hits (up to 8x)
- GET READY countdown (3-2-1) with pulsing font after each life lost
- High scores persist across sessions in `%AppData%\BrickBlast\highscores.json`

## Platforms
| Version | Rendering | Audio |
|---------|-----------|-------|
| WinForms | GDI+ (`System.Drawing.Graphics`) | winmm.dll MCI |
| WPF | `System.Windows.Media.DrawingContext` | winmm.dll MCI |
| HTML5 | Canvas 2D | Web Audio API |
| Android/iOS | WebView (Capacitor) | Web Audio API |
