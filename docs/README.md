# BrickBlast: Velocity Market

> A fast 2D brick-breaker arcade game with progression, customization,  
> a points-based marketplace, lightweight online sync, and a video trailer.

---

## Team

| Name | Role |
|------|------|
| Curtis Loop | Team Lead |
| Alyssa Puentes | Co-Lead |
| Andrea Albisser | Co-Lead |

---

## Tech Stack

| Item | Value |
|------|-------|
| Language | Visual Basic (.NET) |
| Framework | .NET 10 / WinForms |
| Renderer | GDI+ (System.Drawing) |
| Persistence | JSON (`System.Text.Json`) |
| Audio | Win32 `PlaySound` + `mciSendString` |
| Input | Keyboard, Mouse/Touch, XInput (gamepad) |
| Networking | REST sync via `System.Net.Http.HttpClient` |
| Source | [BrickBlast starter](https://github.com/stuffthings15/BrickBlast) |

---

## Installation

1. Clone the repository:
   ```
   git clone https://github.com/stuffthings15/BrickBlast.git
   ```
2. Open `anime finder.sln` in Visual Studio 2022+.
3. Set **anime finder** as the startup project.
4. Press **F5** to build and run.

No external dependencies beyond .NET 10 SDK are required.

---

## Build Instructions

```
dotnet build "anime finder.vbproj" -c Release
```

Output lands in `bin\Release\net10.0-windows10.0.22000.0\`.

---

## Gameplay Instructions

1. Enter your player name on the startup screen — your profile loads automatically.
2. Select **Play** from the main menu to begin.
3. Use mouse, touch, or gamepad to move the paddle.
4. Click / tap anywhere to launch the ball.
5. Destroy all required bricks to complete a level.
6. Miss the ball and you lose a life; lose all lives → Game Over.
7. Earn **coins** by destroying bricks and completing levels.
8. Spend coins in the **Store** on ball skins, brick palettes, and bonus packs.

---

## Controls

| Action | Keyboard | Mouse/Touch | Gamepad |
|--------|----------|------------|---------|
| Move paddle | ← → | Drag | Left stick / D-pad |
| Launch ball | Space | Click | A / Cross |
| Pause | Esc | Pause button | Start |
| Navigate menus | ↑ ↓ ← → | Click | D-pad |
| Confirm | Enter | Click | A |

---

## Feature List

- **8+ playable levels** with increasing brick variety and difficulty
- **5 brick types**: Standard, Durable, Reward, Hazard, Moving
- **Complete gameplay loop**: Menu → Play → Pause → Results → Marketplace → Repeat
- **In-game economy**: earn coins, spend in store, persistent per-player profile
- **Marketplace**: 18 ball skins, 17 brick palettes, 17 bonus packs
- **Dev mode**: enter name `luffyisking` for unlimited coins
- **Procedural generation**: base assets seeded per-run; store assets applied on top
- **Local persistence**: JSON save files per player under `%AppData%\BrickBlast\players\`
- **Network sync**: uploads player profile; shows Online / Offline / Synced status
- **Settings**: sound volume, colorblind mode, dev mode indicator
- **Credits** screen with team and tools attribution

---

## Marketplace

The store is accessible from the main menu. There are three categories:

| Category | Items | Example |
|----------|-------|---------|
| Balls | 18 skins | Fire Ball (150 coins), Aurora Ball (700 coins) |
| Bricks | 17 palettes | Space Bricks (500 coins), Sakura Bricks (350 coins) |
| Bonuses | 17 packs | Dragon Fire Pack (500 coins), Golden Age Pack (700 coins) |

Every purchased item is saved to the player's profile and restored on next login.

---

## Networking

`NetworkSyncService` in `Form1.vb` posts a JSON payload to the configured sync endpoint:

```json
{
  "playerId": "curtis",
  "bestScore": 12500,
  "bestLevel": 8,
  "currency": 740,
  "purchasedItems": ["balls_fire", "bricks_space"],
  "equippedBall": "fire",
  "equippedBricks": "space",
  "equippedBonuses": "dragon",
  "lastUpdatedUtc": "2026-05-06T20:30:00Z"
}
```

The game remains fully playable offline; sync failures are silently logged.

---

## Persistence

Save files:
- `%AppData%\BrickBlast\highscores.json` — global leaderboard
- `%AppData%\BrickBlast\players\<name>.json` — per-player store profile

---

## Known Issues

| ID | Description | Status |
|----|-------------|--------|
| K-01 | Ball may reach near-horizontal angle on edge deflection | Workaround in anti-stall logic |
| K-02 | Network sync requires manual endpoint configuration | See `NetworkSyncService` constant |
| K-03 | Gamepad triggers not mapped (bumpers only) | Planned |

---

## Credits

- Starter project: BrickBlast — Curtis Loop / stuffthings15
- GDI+ rendering, audio, and input: .NET BCL
- Sound assets: procedurally generated via Win32 waveform
- Documentation & AI pairing: GitHub Copilot (reviewed and owned by team)

---

## Submission Links

See [`Docs/Submission/FinalSubmissionChecklist.md`](Submission/FinalSubmissionChecklist.md) for all deliverable evidence.

**Version:** v1.0.0  
**Delivery date:** May 13, 2026
