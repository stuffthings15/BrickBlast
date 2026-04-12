# Mindset — Design Decisions

## Why Windows Forms (not WPF) for the original?
- Already implemented and working
- GDI+ gives direct pixel control for a game loop
- Single-file architecture keeps things simple for CS-120

## Why also port to WPF?
- Demonstrates the same game logic on a different rendering stack
- WPF `DrawingContext` offers built-in rounded rectangles and gradient text geometry
- Shows portability of the architecture across UI frameworks
- `FrameworkElement.OnRender()` replaces `Form.Paint` + `Graphics`

## Why procedural MIDI music?
- No external audio files needed
- Fully self-contained .exe after publish
- 10 styles gives variety without assets

## Why 7 platform builds?
- Windows WinForms — native desktop, most feature-complete
- Windows WPF — alternative rendering, same gameplay
- HTML5 — runs in any browser on any OS
- Android phone/tablet — APK for native install, PWA as fallback
- iPhone/iPad — PWA via Safari (no App Store needed)

## Why persistent high scores?
- JSON file in `%AppData%\BrickBlast\` survives app restarts
- Top 10 scores with player names
- Shared between WinForms and WPF versions

## Attention-Driven Priorities
1. Gameplay feel (ball physics, power-ups)
2. Visual polish (particles, glow, star field)
3. Accessibility (colorblind mode)
4. Audio (music + SFX styles)
5. Multi-platform export (7 builds)
6. Documentation (GDD, team roles, release notes)
