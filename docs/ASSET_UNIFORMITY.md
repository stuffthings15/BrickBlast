# Asset Uniformity Verification

This project now uses a **single canonical asset source**:

- `anime finder wpf/Assets/`

All other platform builds are mirrored from that folder so visual assets stay identical across:

- Windows WPF
- Windows (legacy build folder)
- macOS (Avalonia)
- HTML
- Android phone/tablet
- iPad/iPhone
- Web and mobile web wrapper folders

## 1) Sync all assets to every build

Run:

```powershell
tools\integrate_all_assets.ps1
```

At the end of the script, it now runs **Mirror Assets to All Builds** and writes sync lines to:

- `ExternalAssets/integration_report.txt`

## 2) Verify uniformity

Run:

```powershell
tools\verify_assets_uniformity.ps1
```

Expected result:

- Every target shows `[OK]`
- Same PNG count as canonical folder
- Final line: `Uniformity check PASSED.`

## 3) Runtime check (Visual Studio F5)

Startup project should be:

- `anime finder wpf\anime finder wpf.vbproj`

Important:

- Treat `versions/windows-wpf/BrickBlast.exe` as a **published copy**, not the source of truth.
- Source of truth is always `anime finder wpf/`.
- To refresh the published Windows-WPF binary from main source, run:

```powershell
tools\push_main_to_versions.ps1
```

On launch:

- Menu should show textured buttons/icons, not plain procedural-only visuals
- Level backgrounds should include imported game/retro backgrounds
- Options screen should show icon-heavy controls and slider assets

If visuals still appear old:

1. `Build > Clean Solution`
2. `Build > Rebuild Solution`
3. Confirm output folder has assets:
   - `anime finder wpf\bin\Debug\net10.0-windows\Assets\...`
4. Re-run `tools\integrate_all_assets.ps1`

## 4) Cross-platform handoff process

When Windows WPF visuals are approved, propagate to all builds with:

1. `tools\integrate_all_assets.ps1`
2. `tools\verify_assets_uniformity.ps1`
3. Commit + push

This guarantees every operating-system build uses the same textures, sprites, UI, and tile sets.

## 5) One-command push from main app to versions

```powershell
tools\push_main_to_versions.ps1
```

This command will:

1. Integrate and mirror assets to all version folders
2. Verify uniformity
3. Publish updated `versions/windows-wpf/BrickBlast.exe` from the **main** WPF project
