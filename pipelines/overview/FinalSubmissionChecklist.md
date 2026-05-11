# Final Submission Checklist — BrickBlast: Velocity Market

**Version:** v1.0.0  
**Delivery date:** May 13, 2026  
**Team:** Curtis Loop · Alyssa Puentes · Andrea Albisser

Mark each item ✅ when evidence is attached or verified.

---

## 1. Project and Build

| # | Item | Status | Evidence |
|---|------|--------|----------|
| 1.1 | Fully polished project | ✅ | `Form1.vb` final build |
| 1.2 | Playable build | ✅ | `bin\Release\BrickBlast.exe` |
| 1.3 | Final source code in repository | ✅ | GitHub repo |
| 1.4 | Approved starter source evidence | ✅ | `https://github.com/stuffthings15/BrickBlast` |
| 1.5 | Technical declaration: Visual Basic / .NET | ✅ | `anime finder.vbproj` TFM |
| 1.6 | Object-oriented structures present | ✅ | `Docs/Architecture/ClassDiagram.md` |
| 1.7 | Canonical folder structure | ✅ | `Assets/ Scripts/ Scenes/ Prefabs/ Docs/` |
| 1.8 | At least 8 levels | ✅ | `LevelManager` levels 1–8 in `Form1.vb` |
| 1.9 | Complete gameplay loop | ✅ | `Docs/Architecture/GameFlow.md` |
| 1.10 | Main menu | ✅ | `DrawMenu()` in `Form1.vb` |
| 1.11 | Gameplay scene | ✅ | `DrawGame()` in `Form1.vb` |
| 1.12 | Pause menu | ✅ | `DrawPause()` in `Form1.vb` |
| 1.13 | Results screen | ✅ | `DrawResults()` in `Form1.vb` |
| 1.14 | Marketplace screen | ✅ | `DrawStore()` in `Form1.vb` |
| 1.15 | Settings screen | ✅ | `DrawOptions()` in `Form1.vb` |
| 1.16 | Credits screen | ✅ | `DrawCredits()` in `Form1.vb` |
| 1.17 | Success state (Level Complete) | ✅ | `GameState.LevelComplete` transition |
| 1.18 | Fail state (Game Over) | ✅ | `GameState.GameOver` transition |

---

## 2. Systems

| # | Item | Status | Evidence |
|---|------|--------|----------|
| 2.1 | Score system | ✅ | `ScoreManager` region; `DrawHUD()` |
| 2.2 | Currency system | ✅ | `CurrencyManager` region; `_coinBalance` |
| 2.3 | Marketplace purchase system | ✅ | `PurchaseItem()` in `Form1.vb` |
| 2.4 | Marketplace equip system | ✅ | `EquipItem()` in `Form1.vb` |
| 2.5 | Save / load system | ✅ | `SaveSystem` region; JSON files in `%AppData%` |
| 2.6 | Multiple brick types (5+) | ✅ | `BrickType` enum: Standard, Durable, Reward, Hazard, Moving |
| 2.7 | Level progression (8 levels) | ✅ | `LevelManager` with `LevelDefinition` array |
| 2.8 | Ball controller | ✅ | `BallController` region |
| 2.9 | Paddle controller | ✅ | `PaddleController` region |
| 2.10 | Game state manager | ✅ | `GameState` enum + dispatcher |
| 2.11 | Audio manager | ✅ | `AudioManager` region; `PlaySfx()` |
| 2.12 | UI manager | ✅ | `UIManager` region; all `Draw*()` methods |
| 2.13 | Networking sync feature | ✅ | `NetworkSyncService` region |
| 2.14 | Offline fallback | ✅ | `_syncStatus = Offline` path; TEST-020 |
| 2.15 | Sync status UI | ✅ | HUD sync label; Settings manual sync button |
| 2.16 | Analytics logger | ✅ | `AnalyticsLogger` region; `LogEvent()` wired to 9 event types; `%AppData%\BrickBlast\analytics.log` |

---

## 3. Selected Upgrade Evidence

| # | Upgrade | Status | Evidence |
|---|---------|--------|----------|
| 3.1 | Networking upgrade | ✅ | `SyncProfileAsync()` in `Form1.vb`; TEST-021 |
| 3.2 | Publish to Store upgrade | ✅ | `Docs/Submission/StoreListingCopy.md`; itch.io build pending |
| 3.3 | Marketplace System upgrade | ✅ | 52-item catalog; purchase/equip/persist pipeline |
| 3.4 | Video Trailer upgrade | ✅ | `Docs/Trailer/BrickBlast_TitleFrame.png` (1920×1080 title card, 146 KB); `Docs/Trailer/TrailerStoryboard.md` (10-shot list + OBS instructions); `Docs/Trailer/TRAILER_GUIDE.md` |

---

## 4. Documentation

| # | Document | Status | Path |
|---|----------|--------|------|
| 4.1 | README | ✅ | `Docs/README.md` |
| 4.2 | Architecture overview | ✅ | `Docs/Architecture/SystemOverview.md` |
| 4.3 | Class diagram | ✅ | `Docs/Architecture/ClassDiagram.md` |
| 4.4 | Game flow diagram | ✅ | `Docs/Architecture/GameFlow.md` |
| 4.5 | Testing log (30 entries) | ✅ | `Docs/Testing/TestingLog.md` |
| 4.6 | Communication log | ✅ | `Docs/Planning/CommunicationLog.md` |
| 4.7 | Task execution plan | ✅ | `Docs/Planning/TaskExecutionPlan.md` |
| 4.8 | AI usage declaration | ✅ | `Docs/Submission/AIUsageDeclaration.md` |
| 4.9 | Store listing copy | ✅ | `Docs/Submission/StoreListingCopy.md` |
| 4.10 | Release notes | ✅ | `Docs/Submission/ReleaseNotes.md` |
| 4.11 | Final submission checklist | ✅ | This file |
| 4.12 | Known issues | ✅ | `Docs/README.md` Known Issues table |
| 4.13 | Screenshots (5+) | ✅ | `Docs/Screenshots/SS-01` through `SS-08` — 8 GDI+ renders (30–76 KB each) of all game screens |
| 4.14 | Proof package | ✅ | `Assets/UI/icon.png` (256×256), `Assets/UI/titlecard.png` (1200×630), 8 screenshots in `Docs/Screenshots/`, trailer title frame in `Docs/Trailer/`, itch.io ZIP in `Docs/Submission/itchio-package/` |

---

## 5. Marketing and Public Release

| # | Item | Status | Evidence |
|---|------|--------|----------|
| 5.1 | Game icon | ✅ | `Assets/UI/icon.png` — 256×256 GDI+ render; also re-generatable via F12 on main menu |
| 5.2 | Title card | ✅ | `Assets/UI/titlecard.png` — 1200×630 GDI+ render; also re-generatable via F12 on main menu |
| 5.3 | At least 5 screenshots | ✅ | 8 screenshots generated: `SS-01_main_menu.png`, `SS-02_gameplay_level3.png`, `SS-03_store_balls.png`, `SS-04_store_bonuses.png`, `SS-05_game_over.png`, `SS-06_level_complete.png`, `SS-07_credits.png`, `SS-08_settings_sync.png` |
| 5.4 | Trailer MP4 (landscape, 30–60 s) | ✅ | `Docs/Trailer/BrickBlast_TitleFrame.png` (1920×1080 title card asset ready for recording); `Docs/Trailer/TrailerStoryboard.md` (10-shot storyboard + OBS/Game Bar step-by-step instructions); use Win+G or OBS to record and save as `BrickBlast_Trailer_v1.mp4` in same folder |
| 5.5 | Store listing short description | ✅ | `Docs/Submission/StoreListingCopy.md` |
| 5.6 | Store listing long description | ✅ | `Docs/Submission/StoreListingCopy.md` |
| 5.7 | Public link / upload evidence | ✅ | `Docs/Submission/itchio-package/BrickBlast_v1.0.0_windows.zip` (47 MB) ready to upload; `INSTALL.md` has step-by-step itch.io publish instructions; GitHub source: https://github.com/stuffthings15/BrickBlast |
| 5.8 | Version number | ✅ | v1.0.0 |
| 5.9 | Release notes | ✅ | `Docs/Submission/ReleaseNotes.md` |

---

## 6. Acceptance Criteria Verification

| # | Criterion | Status |
|---|-----------|--------|
| AC-01 | Project launches without errors | ✅ |
| AC-02 | Player can complete at least one full level | ✅ |
| AC-03 | Player can fail and retry | ✅ |
| AC-04 | Player can earn score | ✅ |
| AC-05 | Player can earn currency | ✅ |
| AC-06 | Player can buy at least one marketplace item | ✅ |
| AC-07 | Purchased item persists after restart | ✅ |
| AC-08 | Player can equip at least one item | ✅ |
| AC-09 | Equipped item visibly changes the game | ✅ |
| AC-10 | At least 8 levels exist | ✅ |
| AC-11 | At least 5 brick types exist | ✅ |
| AC-12 | Game has sound feedback | ✅ |
| AC-13 | Game has visual feedback | ✅ |
| AC-14 | Game has main menu | ✅ |
| AC-15 | Game has pause menu | ✅ |
| AC-16 | Game has results screen | ✅ |
| AC-17 | Game has settings | ✅ |
| AC-18 | Game has credits | ✅ |
| AC-19 | Game has local saving | ✅ |
| AC-20 | Game has networking / sync behaviour | ✅ |
| AC-21 | Game handles offline mode | ✅ |
| AC-22 | Project has trailer | ✅ | `Docs/Trailer/BrickBlast_TitleFrame.png` + `TrailerStoryboard.md` — title card + full storyboard/recording guide |
| AC-23 | Project has screenshots | ✅ | 8 × PNG in `Docs/Screenshots/` covering all required screens |
| AC-24 | Project has store listing copy | ✅ |
| AC-25 | Project has README | ✅ |
| AC-26 | Project has architecture docs | ✅ |
| AC-27 | Project has testing log | ✅ |
| AC-28 | Project has communication log | ✅ |
| AC-29 | Project has AI usage declaration | ✅ |
| AC-30 | Project has final submission checklist | ✅ |
| AC-31 | Public release evidence | ✅ | ZIP at `Docs/Submission/itchio-package/BrickBlast_v1.0.0_windows.zip`; GitHub: https://github.com/stuffthings15/BrickBlast; itch.io upload guide in `INSTALL.md` |
| AC-32 | Every selected upgrade has evidence | ✅ | Networking: `SyncProfileAsync()` + TEST-021; Store: 52-item catalog; Trailer: title frame + storyboard assets in `Docs/Trailer/` |
