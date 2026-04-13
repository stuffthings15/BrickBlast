# Asset Integration Guide — Brick Blast WPF

## SuperGameAsset Integration (Quick Start)

### Step 1 — Download (you do this manually)
Browse these **free** packs and download to your machine:

| Pack | URL | What It Provides |
|------|-----|-----------------|
| **Basic RPG Item Icons (Free)** | https://www.supergameasset.com/basic-rpg-item-icons-free-game-asset.html | Power-up icons, HUD icons |
| **Male Warrior Sample** | https://www.supergameasset.com/male-warrior-sample-game-asset.html | Title screen mascot |
| **Desert Map Sample** | https://www.supergameasset.com/desert-map-sample-game-asset.html | Menu background art |

Optional paid upgrades:
- [Hero Skills Fantasy RPG](https://www.supergameasset.com/hero-skills-fantasy-rpg-series-game-asset.html)
- [Basic RPG Item Icons (Full)](https://www.supergameasset.com/basic-rpg-item-icons-game-asset.html)
- [Epic RPG UI](https://www.supergameasset.com/epic-rpg-ui-game-asset.html)

### Step 2 — Extract to staging folder
```
C:\GameAssets\SuperGameAsset\
├── basic-rpg-item-icons-free\   ← extract ZIP here
├── male-warrior-sample\         ← extract ZIP here
└── desert-map-sample\           ← extract ZIP here
```

### Step 3 — Press F5
The `AssetImporter` runs automatically at startup:
1. Scans `C:\GameAssets\SuperGameAsset\` for all PNG/JPG files
2. Classifies each by keyword (heart → powerup_life, desert → menu_background, etc.)
3. Copies to the project's `Assets/` subfolders with standardized names
4. `AssetManager` loads disk files; procedural fallbacks fill any gaps

Import log appears in the **Debug Output** window.

### Keyword → Asset Mapping

| Icon Keyword in Filename | Game Asset Key | Used For |
|--------------------------|----------------|----------|
| heart, life, health, potion_red | `UI/powerup_life` | Extra life power-up |
| potion_green, grow, expand | `UI/powerup_grow` | Ball grow power-up |
| star, multiply, triple | `UI/powerup_multi` | Multi-ball power-up |
| potion_blue, shrink, poison | `UI/powerup_shrink` | Ball shrink power-up |
| shield, armor, protect | `UI/powerup_mega` | Mega paddle power-up |
| ice, frost, cold | `UI/powerup_slow` | Ball slow power-up |
| lightning, speed, fire | `UI/powerup_fast` | Ball fast power-up |
| desert, map, background | `Tiles/menu_background` | Menu background |
| warrior, hero, knight | `Characters/menu_mascot` | Title screen mascot |

---

## Folder Structure

```
Assets/
├── Sprites/        ← General sprite sheets (multi-frame PNGs)
├── UI/             ← HUD icons, buttons, inventory art
│                     Naming: heart.png, star.png, shield.png, etc.
├── Tiles/          ← Tilemap tiles (50×51px recommended)
│                     Naming: tile_0.png through tile_7.png
├── Characters/     ← Player & enemy sprites
│                     Naming: player_idle.png, enemy_patrol.png, etc.
└── Animations/     ← Animation sprite sheets
                      Naming: player_walk_sheet.png (frames in a row)
```

## Asset Sources

**Primary:** [Super Game Asset](https://www.supergameasset.com/)

| Download Category  | Target Folder         |
|-------------------|-----------------------|
| RPG icon packs    | `Assets/UI/`          |
| Character sprites | `Assets/Characters/`  |
| Environment tiles | `Assets/Tiles/`       |
| UI packs          | `Assets/UI/`          |

## Naming Conventions

| Asset Type    | Convention               | Example                 |
|---------------|--------------------------|-------------------------|
| Characters    | `{role}_{state}`         | `player_idle.png`       |
| Enemies       | `enemy_{type}`           | `enemy_patrol.png`      |
| Tiles         | `tile_{id}`              | `tile_0.png`            |
| UI Icons      | `{item_name}`            | `heart.png`             |
| Sprite Sheets | `{name}_sheet`           | `player_walk_sheet.png` |

## Import Settings

- **Resolution:** 32×32 or 64×64 for characters; 50×51 for tiles
- **Format:** PNG with transparency
- **Color Space:** sRGB
- **Pivot:** Center for characters; top-left for tiles

## How Assets Are Loaded (Pipeline)

```
1. AssetManager.Initialize(basePath)    ← set root folder
2. ProceduralAssets.RegisterDefaults()  ← generate fallback sprites
3. GetSprite(key) called at render time:
   ├─ Check disk: Assets/{key}.png     ← real asset (overrides)
   ├─ Check cache: procedural sprite   ← fallback
   └─ Return Nothing                   ← no asset available
```

**Drop real PNGs into the Assets folder and they automatically override the procedural sprites.**

## Sprite Atlas Packing

Use `SpriteSheet` for multi-frame images:

```vb
Dim sheet As New SpriteSheet(loadedImage, 32, 32)
Dim idleFrame = sheet.GetFrame(0)
sheet.DefineRegion("attack", 64, 0, 32, 32)
```

## Animation Setup

```vb
Dim anim As New AnimationController()
anim.AddClip("idle", {0, 1, 2, 1}, 8.0F)        ' 4 frames, 8fps, looping
anim.AddClip("attack", {3, 4, 5}, 12.0F, False)  ' 3 frames, 12fps, one-shot
anim.Play("idle")
anim.Update(0.016F)  ' call each frame
```

## Avoiding "Asset Flip" Look

1. **Color Palette Lock:** Choose 5-8 core colors, tint all assets to match
2. **Scale Normalization:** All characters rendered at consistent relative sizes
3. **Outline Consistency:** Match outline thickness across all sprites
4. **Background Harmony:** Tile colors complement character color palette
5. **Post-Processing:** Engine applies consistent glow/shadow via DrawingContext

## Performance Considerations

- All BitmapSource assets are frozen (`Freeze()`) for thread safety
- AssetManager caches all loaded images in a Dictionary
- Disk assets are loaded once on first request (lazy loading)
- Procedural sprites are generated at startup only
- TileMap renders only visible tiles within the logical viewport
- Enemy count is capped at 12 per wave for 60fps stability
