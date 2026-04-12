# BRICK BLAST — Global Documentation

## Project Overview
A single-player brick-breaking arcade game with 7 platform builds.

### Implementations
| Platform | Technology | Source |
|----------|-----------|--------|
| Windows Desktop | VB.NET WinForms (.NET 10, GDI+) | `Form1.vb` |
| Windows Desktop | VB.NET WPF (.NET 10, DrawingContext) | `anime finder wpf/GameCanvas.vb` |
| Browser | HTML5 Canvas + JavaScript | `web/index.html` |
| Android | Capacitor APK + PWA | `mobile/` |
| iOS | PWA (Safari) | `versions/iphone/`, `versions/ipad/` |

### Core Features
- Paddle + ball physics, 7 power-up types, multi-ball, combo system
- Procedurally generated MIDI music (10 styles), retro SFX (5 styles)
- Colorblind mode, 4 window sizes, animated star field
- Persistent high scores in `%AppData%\BrickBlast\highscores.json`
- 8 level patterns with progressive difficulty

## Strategy
Goal: deliver a polished multi-platform arcade game from a single codebase.
Approach: WinForms core → WPF port → HTML5 → mobile wrappers.

## Pipeline Breakdown
| Stage          | Description                                      | Status    |
|----------------|--------------------------------------------------|-----------|
| overview       | Project summary and goals                        | ✅ Done   |
| mindset        | Design decisions and rationale                   | ✅ Done   |
| docs           | GDD, PROJECT_DOCS, STORY, TEAM_PRODUCTION        | ✅ Done   |
| storyboard     | Screen flow (Menu→Play→Pause→LevelComplete→HighScore→Options) | ✅ Done |
| assets         | All procedurally generated (no files)            | ✅ Done   |
| implementation | WinForms + WPF + HTML5 + Mobile                 | ✅ Done   |
| git            | GitHub repository pushed                         | ✅ Done   |
| exe            | Self-contained builds for all 7 platforms        | ✅ Done   |

## Notes
- WinForms and WPF versions use winmm.dll P/Invoke — Windows only
- HTML5/mobile versions use Web Audio API instead of MCI
- Music is generated at runtime to temp folder, cleaned up on close
- High scores persist to `%AppData%\BrickBlast\highscores.json`
