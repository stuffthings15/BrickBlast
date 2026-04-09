# Mindset — Design Decisions

## Why Windows Forms (not WPF)?
- Already implemented and working
- GDI+ gives direct pixel control for a game loop
- No need to migrate — focus on polish

## Why procedural MIDI music?
- No external audio files needed
- Fully self-contained .exe after publish
- 10 styles gives variety without assets

## Why in-memory high scores?
- Simple for a school project
- Future improvement: save to JSON file

## Attention-Driven Priorities
1. Gameplay feel (ball physics, power-ups)
2. Visual polish (particles, glow, star field)
3. Accessibility (colorblind mode)
4. Audio (music + SFX styles)
5. Export (self-contained .exe)
