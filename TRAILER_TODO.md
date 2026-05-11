# Trailer Recording Checklist — BrickBlast: Velocity Market

> **Status:** ⏳ ONE REMAINING MANUAL STEP  
> Everything else in the project is complete (AC-01 through AC-32 ✅).  
> The trailer MP4 must be recorded by a human — it cannot be auto-generated.

---

## What You Need to Record

**File:** `docs/Trailer/BrickBlast_Trailer_v1.mp4`  
**Length:** 30–60 seconds  
**Resolution:** 1920×1080 landscape  
**Audio:** In-game sound on (do not mute)

---

## Fastest Method — Xbox Game Bar (Built Into Windows)

1. Launch the game:
   ```
   Final Version Releases\windows-x64\BrickBlast.exe
   ```

2. Press **Win + G** to open Game Bar

3. Click the **Record** button (or press **Win + Alt + R**)

4. Play through these scenes in order:
   | Time | What to do |
   |------|-----------|
   | 0–4 s | Sit on main menu — show title, version, team name |
   | 4–10 s | Enter player name, start a game |
   | 10–20 s | Play Level 1 → Level 3, show ball, bricks, score rising |
   | 20–26 s | Let ball hit each brick type (Standard, Durable, Reward, Hazard, Moving) |
   | 26–32 s | Open Marketplace — browse Balls, Bricks, Bonuses tabs, buy one item |
   | 32–38 s | Return to game — show equipped skin applied |
   | 38–44 s | Complete a level — show "LEVEL COMPLETE" overlay with coin reward |
   | 44–48 s | Open Settings — show sync status ("✓ Synced") |
   | 48–54 s | Open Credits screen |
   | 54–60 s | Return to main menu, hold for 3 s, then stop recording |

5. Press **Win + Alt + R** to stop

6. Clip saves to: `C:\Users\stuff\Videos\Captures\`

7. Rename the file to `BrickBlast_Trailer_v1.mp4`

8. Move it here:
   ```
   docs\Trailer\BrickBlast_Trailer_v1.mp4
   ```

---

## Alternative — OBS Studio (Free)

Download: https://obsproject.com

1. Add source: **Window Capture** → select `BrickBlast`
2. Output settings: MP4, 1920×1080, 30 fps, H.264
3. Start Recording → play the shot list above → Stop Recording
4. Move output to `docs\Trailer\BrickBlast_Trailer_v1.mp4`

---

## After Recording

Run these commands to commit and push the trailer:

```powershell
cd "C:\Users\stuff\Desktop\Classes\CS-120\PROJECTS\CS-120\Weeks\Week 12\anime finder"

# Track MP4 with Git LFS so GitHub accepts it
git lfs track "*.mp4"
git add .gitattributes
git add "docs/Trailer/BrickBlast_Trailer_v1.mp4"
git commit -m "feat: add gameplay trailer BrickBlast_Trailer_v1.mp4"
git push origin master
```

---

## Post-Upload Checklist

- [ ] `docs/Trailer/BrickBlast_Trailer_v1.mp4` exists in repo
- [ ] File is 30–60 seconds long
- [ ] Gameplay, marketplace, and level-complete are all visible
- [ ] Audio is audible (not silent)
- [ ] Git push succeeded (LFS upload complete)
- [ ] `docs/Submission/FinalSubmissionChecklist.md` AC-22 evidence updated to include MP4 path

---

## That's It — Project Is Otherwise 100% Done

| Area | Status |
|------|--------|
| Source code | ✅ |
| Playable Windows exe | ✅ |
| All 32 acceptance criteria | ✅ |
| Documentation (all types) | ✅ |
| PUBLISHING.md in every folder (33 files) | ✅ |
| Root PUBLISHING.md master overview | ✅ |
| Multi-platform release tree (20 targets) | ✅ |
| GitHub pushed | ✅ |
| **Trailer MP4** | ⏳ Record with Win+G (5 minutes) |
