# BRICK BLAST — Global Documentation

## Project Overview
A single-player brick-breaking arcade game built in VB.NET Windows Forms.
- Paddle + ball physics, 7 power-up types, multi-ball, combo system
- Procedurally generated MIDI music (10 styles), retro SFX (5 styles)
- Colorblind mode, 4 window sizes, animated star field
- High score entry screen with per-session leaderboard

## Strategy
Goal: polish and finalize an already-functional game.
Approach: fix/improve in batches — visuals → gameplay → sound → export.

## Pipeline Breakdown
| Stage          | Description                                      | Status  |
|----------------|--------------------------------------------------|---------|
| overview       | Project summary and goals                        | Done    |
| mindset        | Design decisions and rationale                   | Done    |
| docs           | This file + story assets                         | Done    |
| storyboard     | Screen flow diagrams                             | Pending |
| assets         | Fonts, icons, placeholder images                 | Pending |
| photos         | Screenshots of gameplay                          | Pending |
| video          | Demo recording                                   | Pending |
| implementation | All source code (Form1.vb)                       | Done    |
| git            | Version control and GitHub push                  | Pending |
| exe            | Published build ready to run without VS          | Pending |

## Notes
- Project uses winmm.dll P/Invoke — Windows only, no Android build possible
- Music is generated at runtime to temp folder, cleaned up on close
- High scores are in-memory only (reset on restart) — persistence not implemented yet
- Power-up symbols upgraded to font size 12 for readability
