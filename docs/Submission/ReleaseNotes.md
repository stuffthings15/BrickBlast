# Release Notes — BrickBlast: Velocity Market

---

## v1.0.0 — May 13, 2026

### Initial Release

This is the first public release of **BrickBlast: Velocity Market**, built from the
BrickBlast starter project as the final deliverable for CS-120.

---

### What's New

**Core Gameplay**
- 8 playable levels with hand-designed brick layouts and progressive difficulty
- 5 brick types: Standard, Durable, Reward, Hazard, Moving
- Physics-based ball with anti-stall, speed clamping, and predictable deflection
- Mouse, keyboard, and XInput gamepad support

**Economy and Marketplace**
- Per-player coin economy — earn by destroying bricks and completing levels
- 52-item store: 18 ball skins, 17 brick palettes, 17 bonus packs
- Purchases and equipped cosmetics persist per player profile
- Dev mode available for demonstration purposes

**Cosmetic Theming**
- Each bonus pack re-themes every power-up drop in gameplay:
  shapes, colours, and illustrated icons
- 17 themes include Ninja, Space Odyssey, Dragon Fire, Halloween Horror,
  Sakura Spring, Golden Age, and more

**Persistence**
- Per-player save files under `%AppData%\BrickBlast\players\`
- Global leaderboard under `%AppData%\BrickBlast\highscores.json`
- Corrupted saves silently reset without crashing

**Networking**
- Async profile sync to configurable REST endpoint
- Graceful offline fallback — gameplay never blocked
- Sync status indicator: Offline / Syncing / Synced / Failed

**Audio**
- Procedurally synthesised SFX for every game event
- MIDI background music with loop support
- SFX and music volume controls in Settings
- Colorblind mode adds icon labels to all power-ups

---

### Known Issues

| ID | Description | Workaround |
|----|-------------|-----------|
| K-01 | Ball may reach near-horizontal angle on rare edge deflections | Anti-stall fires within 18 bounces |
| K-02 | Network sync requires manual `_syncEndpoint` constant configuration | Update constant in `Form1.vb` before build |
| K-03 | Gamepad trigger buttons not mapped | Use bumpers or keyboard/mouse |

---

### Installation

1. Download `BrickBlast.exe` from the release folder or itch.io page.
2. Run on Windows 10+ with .NET 10 runtime installed.
3. No installer required — portable executable.

---

### Credits

- Original starter: BrickBlast by stuffthings15
- Development: Curtis Loop, Alyssa Puentes, Andrea Albisser
- Audio: Win32 procedural waveform synthesis
- Documentation pairing: GitHub Copilot (reviewed and owned by team)
