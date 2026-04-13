# Asset Pipeline — Setup Guide

## Quick Start

```powershell
# Step 1: Download free assets
cd tools
.\fetch_assets.ps1

# Step 2: Manually download anything that failed (see MANUAL_DOWNLOADS.md)

# Step 3: Normalize and copy to project
.\normalize_assets.ps1

# Step 4: Press F5 in Visual Studio — real sprites auto-override procedural fallbacks
```

## How It Works

```
fetch_assets.ps1                    normalize_assets.ps1
      │                                     │
      ▼                                     ▼
[Public Websites]              [ExternalAssets/unpacked/]
  Kenney.nl (CC0)                      │
  OpenGameArt (CC0)            Scan all PNGs by keyword
  itch.io (manual)                     │
      │                        Match to game asset keys
      ▼                                │
[ExternalAssets/downloads/]    Copy to project Assets/
[ExternalAssets/unpacked/]             │
                                       ▼
                            [anime finder wpf/Assets/]
                              Sprites/brick_0.png
                              Sprites/paddle.png
                              Sprites/ball.png
                              UI/heart.png
                              UI/star.png
                              ...
                                       │
                                       ▼
                            AssetManager.GetSprite()
                            (disk files override procedural)
```

## Re-running

Both scripts are idempotent:
- `fetch_assets.ps1` — skips already-downloaded ZIPs
- `normalize_assets.ps1` — skips already-copied files (use `-Force` to overwrite)

## Files Generated

| File | Purpose |
|------|---------|
| `ExternalAssets/fetch_log.txt` | Detailed download log |
| `ExternalAssets/manual_downloads_needed.txt` | What needs manual download |
| `ExternalAssets/normalize_report.txt` | What was copied where |
| `ExternalAssets/ASSET_CREDITS.md` | License attribution for all used assets |
| `tools/asset_manifest.json` | Source catalog with priorities and mappings |

## Fallback Chain

Every game element has a procedural fallback. If an external asset is missing, the game renders procedural shapes. The priority chain per element:

1. **Real sprite on disk** (from normalize pipeline)
2. **SuperGameAsset import** (from `C:\GameAssets\SuperGameAsset\`)
3. **Procedural fallback** (generated at startup by `ProceduralAssets.vb`)
