# BrickBlast: Velocity Market — Trailer Storyboard

**Target length:** 30–60 seconds  
**Resolution:** 1920×1080 (or 1200×867 windowed, letterboxed)  
**Output file:** `BrickBlast_Trailer_v1.mp4`  
**Audio:** In-game Retro Chiptune music (looped), fade out last 3 s  

---

## Shot List

| # | Duration | Scene | Caption / Action |
|---|----------|-------|-----------------|
| 1 | 0:00–0:04 | Title frame (`BrickBlast_TitleFrame.png` or game title screen) | Hold on "BRICK BLAST — Velocity Market" title with starfield + brick row decoration |
| 2 | 0:04–0:10 | Name-entry screen | Type "CurtisG" to show player profile; press Enter |
| 3 | 0:10–0:20 | Gameplay — Level 1 → Level 3 | Ball in motion, breaking bricks; HUD shows score climbing; power-up drops and is caught |
| 4 | 0:20–0:26 | Brick variety showcase | Slow pan across 5+ brick types: Standard (red), Reinforced (blue), Explosive (amber) detonating, Ghost (purple), Armored (silver) |
| 5 | 0:26–0:32 | Velocity Market (Store) | Open the store; browse Balls tab (skins), Bricks tab, Bonuses tab; purchase one item |
| 6 | 0:32–0:38 | Equipped skin visible in gameplay | Return to game; ball/brick skin applied; score counter ticking up |
| 7 | 0:38–0:44 | Level Complete overlay | "LEVEL COMPLETE" overlay with score tally and coin reward; "+1,240 coins" |
| 8 | 0:44–0:48 | Settings — Sync status | Open Options screen; show "✓ Synced — Last: 2 min ago" green label |
| 9 | 0:48–0:54 | Credits screen | Quick cut to Credits; scroll team name and technology stack |
| 10 | 0:54–1:00 | Title card outro | Return to title; show GitHub link; slow fade to black |

---

## Recording Instructions (OBS / Xbox Game Bar)

### Option A — Xbox Game Bar (Win + G)
1. Launch `bin\Release\net10.0-windows10.0.22000.0\BrickBlast.exe`
2. Press **Win + G** → start recording
3. Play through shots 1–10 above
4. Stop recording → file saved under `%UserProfile%\Videos\Captures\`
5. Rename to `BrickBlast_Trailer_v1.mp4` and move to `Docs\Trailer\`

### Option B — OBS Studio
1. Add "Window Capture" source → select `BrickBlast`
2. Set output to MP4, 1920×1080, 30 fps
3. Start Recording → play shots 1–10 → Stop Recording
4. Move output file to `Docs\Trailer\BrickBlast_Trailer_v1.mp4`

---

## Checklist (post-recording)

- [ ] File exists at `Docs/Trailer/BrickBlast_Trailer_v1.mp4`
- [ ] Length 30–60 seconds
- [ ] Gameplay, store, and level-complete are all visible
- [ ] Audio present (no silent recording)
- [ ] Update `Docs/Submission/FinalSubmissionChecklist.md` items 3.4, 5.4 with evidence path
