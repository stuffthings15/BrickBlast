# Testing Log — BrickBlast: Velocity Market

## Log Format

| Field | Description |
|-------|-------------|
| Date | Test date |
| Tester | Team member |
| Build | Version tag |
| Feature | System under test |
| Steps | How to reproduce |
| Expected | Expected outcome |
| Actual | Observed outcome |
| Status | Pass / Fail / Blocked |
| Notes | Bug ref or fix |

---

## Entries

---

### TEST-001
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Application launch  
**Steps:** Run `BrickBlast.exe` from `bin\Release\`.  
**Expected:** Name-entry screen appears within 2 seconds.  
**Actual:** ✅ Name-entry screen appeared immediately.  
**Status:** Pass

---

### TEST-002
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Name entry — new player  
**Steps:** Type "TestUser", press Enter.  
**Expected:** Main menu loads; HUD shows "TestUser" and 0 coins.  
**Actual:** ✅ Pass.  
**Status:** Pass

---

### TEST-003
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Name entry — returning player  
**Steps:** Run game; enter "Curtis" (previously saved with 340 coins).  
**Expected:** Main menu loads with coin balance 340.  
**Actual:** ✅ Balance and owned items restored correctly.  
**Status:** Pass

---

### TEST-004
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Dev mode activation  
**Steps:** Enter name "luffyisking", proceed to store.  
**Expected:** Store shows "◆ DEV" label; purchase any item without deducting coins.  
**Actual:** ✅ Dev mode active; coins unchanged after purchases.  
**Status:** Pass

---

### TEST-005
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Main menu navigation  
**Steps:** Use arrow keys to cycle all 5 menu items; press Enter on each.  
**Expected:** Each screen opens and returns to main menu.  
**Actual:** ✅ All screens opened and returned correctly.  
**Status:** Pass

---

### TEST-006
**Date:** 2026-05-06  
**Tester:** Andrea Albisser  
**Build:** v0.9.0  
**Feature:** Ball launch  
**Steps:** Start Level 1; click to launch ball.  
**Expected:** Ball departs from paddle and bounces off walls.  
**Actual:** ✅ Ball launched and moved as expected.  
**Status:** Pass

---

### TEST-007
**Date:** 2026-05-06  
**Tester:** Andrea Albisser  
**Build:** v0.9.0  
**Feature:** Paddle mouse control  
**Steps:** Move mouse left and right during play.  
**Expected:** Paddle follows mouse, clamped to playfield bounds.  
**Actual:** ✅ Responsive, no over/undershoot observed.  
**Status:** Pass

---

### TEST-008
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Brick collision — Standard Brick  
**Steps:** Aim ball at standard bricks; observe destruction.  
**Expected:** Each brick destroyed in one hit; score increments.  
**Actual:** ✅ Bricks destroy on one hit; score updated.  
**Status:** Pass

---

### TEST-009
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Brick collision — Durable Brick  
**Steps:** Aim ball at durable (multi-health) bricks.  
**Expected:** Brick changes colour on damage; destroys after required hits.  
**Actual:** ✅ Visual degradation and correct multi-hit destruction.  
**Status:** Pass

---

### TEST-010
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Reward brick — coin award  
**Steps:** Destroy a reward brick (gold-coloured).  
**Expected:** Coin balance increments by brick's `CurrencyReward`.  
**Actual:** ✅ Coins awarded; HUD updated.  
**Status:** Pass

---

### TEST-011
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Hazard brick — speed penalty  
**Steps:** Hit a hazard brick (red outline).  
**Expected:** Ball speed increases slightly.  
**Actual:** ✅ Speed clamped to maximum after hazard hit.  
**Status:** Pass

---

### TEST-012
**Date:** 2026-05-06  
**Tester:** Andrea Albisser  
**Build:** v0.9.0  
**Feature:** Moving brick  
**Steps:** Observe bricks in level 6+ that shift horizontally.  
**Expected:** Bricks move within lane, bounce at walls.  
**Actual:** ✅ Movement correct; collision still registering.  
**Status:** Pass

---

### TEST-013
**Date:** 2026-05-06  
**Tester:** Andrea Albisser  
**Build:** v0.9.0  
**Feature:** Level complete condition  
**Steps:** Clear all required bricks in Level 1.  
**Expected:** "Level Complete" state fires; results screen shown.  
**Actual:** ✅ Transition fired correctly.  
**Status:** Pass

---

### TEST-014
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Game over condition  
**Steps:** Let ball fall below paddle three times.  
**Expected:** "Game Over" state fires; results screen shown with final score.  
**Actual:** ✅ Game over triggered; score and partial coins displayed.  
**Status:** Pass

---

### TEST-015
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Pause and resume  
**Steps:** Press Esc during gameplay; observe overlay; press Esc again.  
**Expected:** Game freezes; overlay shown; resumes correctly.  
**Actual:** ✅ Pause/resume works; ball continues from frozen position.  
**Status:** Pass

---

### TEST-016
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Marketplace — purchase item  
**Steps:** Navigate to Store; select Fire Ball (150 coins); purchase.  
**Expected:** Coin balance decrements by 150; "Owned" indicator appears.  
**Actual:** ✅ Purchase succeeded; balance updated.  
**Status:** Pass

---

### TEST-017
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Marketplace — insufficient funds  
**Steps:** Attempt to purchase Aurora Ball (700 coins) with 0 coins.  
**Expected:** Error feedback shown; coins unchanged.  
**Actual:** ✅ "Not enough coins" feedback displayed; balance intact.  
**Status:** Pass

---

### TEST-018
**Date:** 2026-05-06  
**Tester:** Andrea Albisser  
**Build:** v0.9.0  
**Feature:** Marketplace — equip and persist  
**Steps:** Purchase and equip "Dragon Fire Pack"; quit game; relaunch; enter same name.  
**Expected:** Dragon Fire Pack is still equipped; power-ups in gameplay use dragon theme.  
**Actual:** ✅ Bonus pack persisted and loaded correctly.  
**Status:** Pass

---

### TEST-019
**Date:** 2026-05-06  
**Tester:** Andrea Albisser  
**Build:** v0.9.0  
**Feature:** Save / load — coin persistence  
**Steps:** Earn 200 coins; quit; relaunch; enter same name.  
**Expected:** Coin balance restored to 200.  
**Actual:** ✅ Balance restored.  
**Status:** Pass

---

### TEST-020
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Offline gameplay  
**Steps:** Disable network adapter; launch game; complete a level.  
**Expected:** Game plays normally; HUD shows "Offline"; no crash.  
**Actual:** ✅ Offline mode fully functional; sync status shows "Offline".  
**Status:** Pass

---

### TEST-021
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** Network sync — online  
**Steps:** Re-enable network; press Manual Sync in Settings.  
**Expected:** Status cycles Syncing → Synced; timestamp updates.  
**Actual:** ✅ (Mock endpoint) sync completed; timestamp recorded.  
**Status:** Pass  
**Notes:** Live endpoint requires `_syncEndpoint` constant to be configured.

---

### TEST-022
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Bonus pack — gameplay theming  
**Steps:** Equip "Space Odyssey Pack"; start a level; observe power-ups.  
**Expected:** Power-up shapes are hexagons; colours match space pack; icons show rockets/planets.  
**Actual:** ✅ Theming applied correctly to all spawned power-ups.  
**Status:** Pass

---

### TEST-023
**Date:** 2026-05-06  
**Tester:** Andrea Albisser  
**Build:** v0.9.0  
**Feature:** Anti-stall logic  
**Steps:** Let ball bounce horizontally between walls ~20 times.  
**Expected:** Ball angle perturbed slightly; vertical movement restored.  
**Actual:** ✅ Anti-stall fired after threshold; angle corrected.  
**Status:** Pass

---

### TEST-024
**Date:** 2026-05-06  
**Tester:** Curtis Loop  
**Build:** v0.9.0  
**Feature:** High score board  
**Steps:** Achieve score > 0; finish game; open High Scores from main menu.  
**Expected:** Player name and score appear in leaderboard.  
**Actual:** ✅ Entry recorded and displayed.  
**Status:** Pass

---

### TEST-025
**Date:** 2026-05-06  
**Tester:** Alyssa Puentes  
**Build:** v0.9.0  
**Feature:** Settings — sound toggle  
**Steps:** Open Settings; toggle SFX off; destroy bricks.  
**Expected:** No sound effects play.  
**Actual:** ✅ SFX disabled; music unaffected.  
**Status:** Pass

---

### TEST-026
**Date:** 2026-05-07  
**Tester:** Curtis Loop  
**Build:** v1.0.0  
**Feature:** Game Over screen — action buttons  
**Steps:** Start a game; lose all lives; verify Game Over overlay appears with Retry, Store, and Menu buttons; click each in turn.  
**Expected:** Retry restarts the current level; Store opens the marketplace; Menu returns to main menu.  
**Actual:** ✅ All three buttons responded correctly via mouse click and keyboard navigation.  
**Status:** Pass

---

### TEST-027
**Date:** 2026-05-07  
**Tester:** Andrea Albisser  
**Build:** v1.0.0  
**Feature:** Credits screen  
**Steps:** From main menu press C; verify credits screen shows team, technology, starter source, and course; press Esc to return.  
**Expected:** Credits screen appears and Esc returns to main menu.  
**Actual:** ✅ Credits displayed correctly; Esc returned to main menu.  
**Status:** Pass

---

### TEST-028
**Date:** 2026-05-07  
**Tester:** Alyssa Puentes  
**Build:** v1.0.0  
**Feature:** Analytics logger  
**Steps:** Play through a level; purchase and equip a store item; let the game over trigger; check `%AppData%\BrickBlast\analytics.log`.  
**Expected:** Log contains entries for GameStarted, LevelStarted, LevelComplete, GameOver, ItemPurchased, ItemEquipped, ProfileSaved.  
**Actual:** ✅ All expected event entries present with timestamps and detail strings.  
**Status:** Pass

---

### TEST-029
**Date:** 2026-05-07  
**Tester:** Curtis Loop  
**Build:** v1.0.0  
**Feature:** Level pattern coverage  
**Steps:** Play through levels 1–8; confirm each level has a visually distinct brick layout; continue to level 9 and verify pattern cycles back to level 1 layout.  
**Expected:** Eight distinct patterns on levels 1–8; level 9 matches level 1 pattern with higher ball speed and brick health.  
**Actual:** ✅ All eight patterns confirmed distinct; level 9 cycled correctly.  
**Status:** Pass

---

### TEST-030
**Date:** 2026-05-07  
**Tester:** Andrea Albisser  
**Build:** v1.0.0  
**Feature:** Sync status HUD indicator  
**Steps:** Launch game with network unavailable; observe HUD sync label; re-enable network; press S in Settings to manually trigger sync; observe label change.  
**Expected:** HUD shows "Offline" without network; label transitions to "Syncing" then "Synced" or "Failed" after sync attempt.  
**Actual:** ✅ Offline label appeared correctly; manual sync updated label to "Synced" when endpoint reachable; "Failed" shown when unreachable.  
**Status:** Pass
