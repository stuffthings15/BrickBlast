# System Overview — BrickBlast: Velocity Market

## Architecture Summary

BrickBlast: Velocity Market is a single-file Visual Basic / WinForms game organised into
logical `#Region` blocks that map to the system categories below.  
All systems execute on a single UI thread driven by a `System.Windows.Forms.Timer` at 60 fps.

---

## Core Systems

### GameManager (region: Game Loop / Form Events)
Central coordinator.  Owns the `GameState` enum value and dispatches every timer tick to the
correct subsystem.  Calls `StartNewGame()`, `PauseGame()`, `ResumeGame()`, `RestartLevel()`,
`AdvanceLevel()`, and `GameOver()`.  Exposes nothing directly—other systems read shared fields
(`_state`, `_lives`, `_level`, etc.).

### LevelManager (region: Level Data / Update Logic)
Holds 8 `LevelDefinition` entries.  `LoadLevel(n)` calls `GenerateProceduralBrickPalette()` for
the colour seed and then `BrickManager.SpawnLevel(layout)`.  Tracks `_bricksRemaining` and fires
the `LevelComplete` transition when the count hits zero.

### BrickManager (region: Bricks)
Creates, updates, and destroys `Brick` instances.  Each `Brick` stores `Health`, `BrickType`,
`ScoreValue`, and `CurrencyReward`.  On destruction it calls `ScoreManager.Add()` and
`CurrencyManager.Add()` and spawns a `PowerUp` when RNG qualifies.

### BallController (region: Ball)
Manages `_ballPos`, `_ballVel`, speed clamping, wall / paddle / brick collision, multi-ball
states, and the anti-stall timer.  On out-of-bounds, decrements lives via `GameManager`.

### PaddleController (region: Paddle)
Reads mouse X (and XInput left-stick X) every tick.  Clamps to playfield bounds.  Applies the
equipped paddle skin from `InventoryManager`.

### ScoreManager (region: Score)
Accumulates per-run score, combo multiplier, and lifetime score.  Writes best score to
`PlayerProfile` on level complete.

### CurrencyManager (region: Economy)
Tracks `_coinBalance`.  Called by `BrickManager` on reward bricks, `LevelManager` on completion
bonus, and `MarketplaceManager` on purchase (decrement).

### MarketplaceManager / Store (region: Store)
Loads `_storeItems` (18 balls + 17 brick palettes + 17 bonus packs).  Handles `PurchaseItem()`,
`EquipItem()`, `IsOwned()`, `DrawStore()`, and the three-tab keyboard/mouse navigation.

### InventoryManager (region: Inventory)
`_ownedItems` (HashSet of `"Category_id"` strings).  `_activeBallSkin`, `_activeBrickPalette`,
`_activeBonusPack`.  Applied at render time by `DrawBall()`, `GetBrickPalette()`, and
`DrawBonusBody()`.

### SaveSystem (region: Persistence)
`LoadStore()` / `SaveStore()` read/write `%AppData%\BrickBlast\players\<name>.json` using
`System.Text.Json`.  `LoadHighScores()` / `SaveHighScores()` manage the global leaderboard at
`%AppData%\BrickBlast\highscores.json`.  Corrupted files are silently ignored and overwritten.

### NetworkSyncService (region: Networking)
`SyncProfileAsync()` posts a JSON payload to the configured REST endpoint.  Sets `_syncStatus`
to `Online`, `Syncing`, `Synced`, or `Failed`.  Gameplay is never blocked — the call is
`Async / Await` fire-and-forget with a 5-second timeout.

### AudioManager (region: Sound)
Wraps `PlaySound` (WAV) and `mciSendString` (MIDI).  `PlaySfx(SfxType)` synthesises short
waveforms procedurally into a temp file and plays them.  `PlayMusic()` loops MIDI tracks.

### UIManager (region: Drawing / Screens)
`Form1_Paint` dispatches to `DrawNameEntry`, `DrawMenu`, `DrawGame`, `DrawOptions`,
`DrawHighScore`, `DrawStore`, and `DrawPause`.  The HUD (`DrawHUD`) shows score, coins, lives,
level, active power-up, and sync status.

### AnalyticsLogger (region: Logging)
Lightweight in-memory ring buffer (50 entries).  Logs `GameStarted`, `LevelStarted`,
`LevelComplete`, `GameOver`, `ItemPurchased`, `ItemEquipped`, `SyncAttempted`, `SyncResult`.
Viewable in the debug overlay (Ctrl+D in dev mode).

---

## System Communication

```
GameManager
  ├─ LevelManager  →  BrickManager  →  Brick[]
  ├─ BallController  ↔  BrickManager / PaddleController
  ├─ ScoreManager  ←  BrickManager
  ├─ CurrencyManager  ←  BrickManager / LevelManager
  ├─ MarketplaceManager  →  CurrencyManager / InventoryManager
  ├─ SaveSystem  ↔  PlayerProfile
  ├─ NetworkSyncService  →  REST endpoint
  ├─ AudioManager
  └─ UIManager  ←  (reads all public state)
```

---

## Data Persistence Flow

1. Player enters name → `SetPlayerProfile(name)` resolves the save path.
2. `LoadStore()` deserialises `StoreSaveData` → populates `_ownedItems`, `_coinBalance`,
   `_activeBallSkin`, `_activeBrickPalette`, `_activeBonusPack`.
3. On purchase / equip: `SaveStore()` serialises updated state immediately.
4. On game-over / level-complete: `SaveHighScores()` appends or updates the leaderboard.
5. `SyncProfileAsync()` posts the merged profile to the remote endpoint when online.

---

## State Machine

```
Boot → NameEntry → MainMenu → Playing ↔ Paused
                                ↓         ↓
                           LevelComplete  GameOver
                                ↓         ↓
                             Results ← ← ←
                                ↓
                           Marketplace ⇄ MainMenu
                           Settings    ⇄ MainMenu
                           Credits     ⇄ MainMenu
                           HighScore   ⇄ MainMenu
```
