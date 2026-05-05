# Task Execution Plan — BrickBlast: Velocity Market

**Project:** BrickBlast: Velocity Market  
**Team:** Curtis Loop, Alyssa Puentes, Andrea Albisser  
**Delivery:** May 13, 2026

---

## Phase 1 — Core Loop, Controls, and Playable Baseline
**Target:** Week 1 (Apr 22 – Apr 28)

| # | Task | Owner | Status |
|---|------|-------|--------|
| 1.1 | Clone / import BrickBlast starter, verify it runs | Curtis | ✅ Done |
| 1.2 | Create canonical folder structure | Curtis | ✅ Done |
| 1.3 | Refactor logic into region-based OO structure | Curtis | ✅ Done |
| 1.4 | Implement `GameState` enum and dispatcher | Curtis | ✅ Done |
| 1.5 | Implement `PaddleController` (mouse / XInput) | Alyssa | ✅ Done |
| 1.6 | Implement `BallController` (launch, bounce, reset) | Alyssa | ✅ Done |
| 1.7 | Implement `BrickManager` (spawn, collision, destroy) | Andrea | ✅ Done |
| 1.8 | Implement `ScoreManager` | Andrea | ✅ Done |
| 1.9 | Implement level-complete and game-over conditions | Curtis | ✅ Done |
| 1.10 | Implement main menu screen | Curtis | ✅ Done |
| 1.11 | Implement pause / resume | Alyssa | ✅ Done |
| 1.12 | Implement basic results screen | Alyssa | ✅ Done |
| 1.13 | Add first two levels | Andrea | ✅ Done |
| 1.14 | Document starter proof (URL, screenshot) | Curtis | ✅ Done |

---

## Phase 2 — Mechanics, Content, and Upgrade Foundations
**Target:** Week 2 (Apr 29 – May 5)

| # | Task | Owner | Status |
|---|------|-------|--------|
| 2.1 | Add 5 brick types (Standard, Durable, Reward, Hazard, Moving) | Andrea | ✅ Done |
| 2.2 | Add levels 3–8 with increasing difficulty | Andrea | ✅ Done |
| 2.3 | Implement `CurrencyManager` (earn coins, save) | Curtis | ✅ Done |
| 2.4 | Implement `MarketplaceManager` + store screens | Curtis | ✅ Done |
| 2.5 | Add 18 ball skins to catalog | Curtis | ✅ Done |
| 2.6 | Add 17 brick palettes to catalog | Curtis | ✅ Done |
| 2.7 | Add 17 bonus packs to catalog | Curtis | ✅ Done |
| 2.8 | Implement purchase / equip / persist logic | Curtis | ✅ Done |
| 2.9 | Implement `InventoryManager` (owned / equipped state) | Alyssa | ✅ Done |
| 2.10 | Implement `SaveSystem` (per-player JSON profiles) | Alyssa | ✅ Done |
| 2.11 | Add name-entry startup screen | Alyssa | ✅ Done |
| 2.12 | Add dev mode (password: luffyisking) | Curtis | ✅ Done |
| 2.13 | Add audio feedback (SFX + MIDI music) | Andrea | ✅ Done |
| 2.14 | Add visual effects (trails, particle bursts, screen shake) | Andrea | ✅ Done |
| 2.15 | Implement `NetworkSyncService` skeleton | Curtis | ✅ Done |
| 2.16 | Show Online/Offline/Syncing/Synced status in HUD | Curtis | ✅ Done |
| 2.17 | Add settings screen (SFX, music, colorblind mode) | Alyssa | ✅ Done |

---

## Phase 3 — Balancing, Saves, Polish, and Publication Prep
**Target:** Week 3 (May 6 – May 10)

| # | Task | Owner | Status |
|---|------|-------|--------|
| 3.1 | Balance brick health, speed curve per level | Andrea | ✅ Done |
| 3.2 | Balance currency rewards vs. store prices | Curtis | ✅ Done |
| 3.3 | Fix anti-stall logic (angle perturbation threshold) | Alyssa | ✅ Done |
| 3.4 | Fix collision edge cases (corner hits, fast-ball tunnelling) | Alyssa | ✅ Done |
| 3.5 | Test save / load across all item categories | Andrea | ✅ Done |
| 3.6 | Test offline network fallback | Curtis | ✅ Done |
| 3.7 | Create app icon and title card | Alyssa | ⏳ In progress |
| 3.8 | Create store listing copy | Curtis | ✅ Done |
| 3.9 | Create release notes | Curtis | ✅ Done |
| 3.10 | Prepare public itch.io build | Curtis | ⏳ Pending |
| 3.11 | Write architecture docs | Curtis | ✅ Done |
| 3.12 | Write testing log (25 entries) | All | ✅ Done |
| 3.13 | Write communication log | All | ✅ Done |

---

## Phase 4 — Final Export, Documentation, and Demo Assets
**Target:** Final (May 11 – May 13)

| # | Task | Owner | Status |
|---|------|-------|--------|
| 4.1 | Record gameplay footage (60 fps, windowed 1200×867) | Curtis | ⏳ Pending |
| 4.2 | Edit landscape trailer (30–60 s, 1920×1080 MP4) | Alyssa | ⏳ Pending |
| 4.3 | Export trailer to `Docs/Trailer/BrickBlast_Trailer_v1.mp4` | Alyssa | ⏳ Pending |
| 4.4 | Capture 5+ screenshots to `Docs/Screenshots/` | Andrea | ⏳ Pending |
| 4.5 | Finalise README | Curtis | ✅ Done |
| 4.6 | Finalise all architecture diagrams | Curtis | ✅ Done |
| 4.7 | Finalise AI usage declaration | All | ✅ Done |
| 4.8 | Finalise final submission checklist | Curtis | ✅ Done |
| 4.9 | Upload build to cloud / itch.io | Curtis | ⏳ Pending |
| 4.10 | Verify all submission links | All | ⏳ Pending |
| 4.11 | Prepare demo script walkthrough | Curtis | ✅ Done |
| 4.12 | Final proof package captured | All | ⏳ Pending |
