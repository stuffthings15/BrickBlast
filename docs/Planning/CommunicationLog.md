# Communication Log — BrickBlast: Velocity Market

## Team

| Name | Role |
|------|------|
| Curtis Loop | Team Lead |
| Alyssa Puentes | Co-Lead |
| Andrea Albisser | Co-Lead |

## Cadence

The team reviews progress weekly. Blockers, completed work, and next actions are recorded below.  
All team members guarantee delivery by **May 13, 2026** at time of presentation.

---

## Log Entries

---

### Entry 001
**Date:** 2026-04-22  
**Participants:** Curtis Loop, Alyssa Puentes, Andrea Albisser  
**Topics:** Project kick-off. Reviewed Game Development Covenant requirements. Confirmed BrickBlast starter.  
**Decisions:**
- Use approved BrickBlast starter from `https://github.com/stuffthings15/BrickBlast`
- Language: Visual Basic / .NET 10 WinForms
- Store economy to use per-player JSON profiles
- Three-category store: Balls, Bricks, Bonuses

**Blockers:** None  
**Next Actions:** Curtis to scaffold folder structure and implement core loop. Alyssa to begin paddle/ball controllers. Andrea to begin brick system.  
**Evidence:** Repository created at `https://github.com/stuffthings15/BrickBlast`

---

### Entry 002
**Date:** 2026-04-29  
**Participants:** Curtis Loop, Alyssa Puentes  
**Topics:** Phase 1 wrap-up. Core gameplay loop functional. Name-entry screen complete.  
**Decisions:**
- Score and coin earn to be separate values
- Anti-stall logic to trigger after 18 horizontal bounces

**Blockers:** Moving bricks causing collision tunnelling at high speed — Alyssa assigned.  
**Next Actions:** Curtis to begin marketplace. Alyssa to fix tunnelling. Andrea to build levels 3–5.  
**Evidence:** `Form1.vb` commit — Phase 1 milestone

---

### Entry 003
**Date:** 2026-05-01  
**Participants:** Curtis Loop, Andrea Albisser  
**Topics:** Store expansion. Decided to triple catalog variety (18 balls, 17 bricks, 17 bonus packs). Dev mode design reviewed.  
**Decisions:**
- Dev mode activated by entering name `luffyisking`
- Dev mode shows `◆ DEV` label in HUD and store; bypasses coin deduction
- Bonus packs theme all power-up visuals: color, shape, and icon

**Blockers:** None  
**Next Actions:** Curtis to implement bonus-pack theming pipeline. Andrea to add levels 6–8.  
**Evidence:** `InitStoreItems()` catalog expanded; `DrawBonusBody` and icon painters added

---

### Entry 004
**Date:** 2026-05-05  
**Participants:** Curtis Loop, Alyssa Puentes, Andrea Albisser  
**Topics:** Phase 2 complete review. Networking sync designed. Final deliverable list reviewed against master prompt.  
**Decisions:**
- Networking: fire-and-forget `HttpClient` POST; game never blocked
- Sync status shown in HUD corner (3-char label)
- Documentation package to be complete by May 10

**Blockers:** `DrawHorrorIcon` compile error (invalid `FillTriangle` call) — Curtis assigned.  
**Next Actions:** Curtis to fix compile error, create all doc files. Alyssa to create app icon and 5 screenshots. Andrea to record gameplay footage for trailer.  
**Evidence:** Build attempt log; `FillTriangle` issue tracked

---

### Entry 005
**Date:** 2026-05-06  
**Participants:** Curtis Loop  
**Topics:** Compile fix applied (`FillPolygon` for horror icon triangles). Full build successful. All doc files scaffolded.  
**Decisions:**
- Testing log to have minimum 25 entries
- Submission checklist to cross-reference every covenant requirement

**Blockers:** None  
**Next Actions:** Complete trailer capture and itch.io upload before May 13.  
**Evidence:** Build log — `Build successful`; `Docs/` folder populated

---

### Entry 006
**Date:** 2026-05-07  
**Participants:** Curtis Loop, Alyssa Puentes, Andrea Albisser  
**Topics:** Final feature integration pass. GameOver screen upgraded with Retry / Store / Menu buttons (mouse + keyboard). Credits screen verified. AnalyticsLogger wired to 9 event types. Level coverage confirmed at 8 distinct patterns. Testing log expanded to 30 entries (TEST-026 through TEST-030). Submission checklist updated with AnalyticsLogger row (item 2.16). Communication log finalized.  
**Decisions:**
- Keyboard shortcuts for GameOver: R=Retry, S=Store, Esc/M=Menu
- Keyboard hint line rendered inside GameOver overlay
- Testing log count target raised to 30
- All core code features are now considered locked; remaining work is marketing artifacts

**Blockers:** Trailer capture, screenshots, itch.io public upload still pending before May 13 presentation deadline.  
**Next Actions:**
1. Record 30–60 s landscape trailer (OBS or Windows Game Bar) and export MP4 to `Docs/Trailer/`.
2. Capture 5+ gameplay screenshots to `Docs/Screenshots/`.
3. Export app icon (`Assets/UI/icon.ico`) and title card (`Assets/UI/titlecard.png`) using in-game procedural art.
4. Upload build ZIP to itch.io; paste public URL into `FinalSubmissionChecklist.md` items 3.2 and 5.7.
5. Final git commit and push to `https://github.com/stuffthings15/BrickBlast`.

**Evidence:** Build log — `Build successful`; `Docs/Testing/TestingLog.md` (30 entries); `Docs/Submission/FinalSubmissionChecklist.md` (item 2.16 added)
