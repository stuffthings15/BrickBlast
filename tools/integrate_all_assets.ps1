# =============================================================================
# integrate_all_assets.ps1 — Maps ALL downloaded assets into the WPF project
#
# Handles:
#   - OpenGameArt breakout tiles (already mapped, verifies)
#   - Kenney UI Pack (Blue theme for buttons, arrows, sliders)
#   - Kenney Game Icons (gear, pause, play, star, trophy, music, etc.)
#   - Kenney Input Prompts (keyboard space, arrows, escape, P)
#   - Kenney Brick Pack (decorative background tiles)
#   - Kenney Pixel UI Pack (pixel-art panel/button alternatives)
#   - Kenney Platformer Bricks (decorative textures)
#
# Copies to: anime finder wpf\Assets\{Sprites,UI,Tiles,Characters}/
# AssetManager auto-loads disk files and overrides procedural fallbacks.
# =============================================================================

$ErrorActionPreference = "Continue"

$Root       = Split-Path $PSScriptRoot -Parent
$Unpacked   = Join-Path $Root "ExternalAssets\unpacked"
$AssetsDir  = Join-Path $Root "anime finder wpf\Assets"
$ReportFile = Join-Path $Root "ExternalAssets\integration_report.txt"

function Log($msg) {
    Write-Host $msg
    Add-Content $ReportFile $msg
}

"" | Set-Content $ReportFile
Log "=== Asset Integration Report ==="
Log "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
Log ""

$copied = 0
$skipped = 0

function Copy-Asset($src, $destKey) {
    if (-not (Test-Path $src)) {
        Log "  [MISS] $destKey — source not found: $(Split-Path $src -Leaf)"
        return
    }
    $parts = $destKey -split "/"
    $dir = Join-Path $AssetsDir $parts[0]
    $ext = [System.IO.Path]::GetExtension($src)
    $dest = Join-Path $dir "$($parts[1])$ext"
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    if (Test-Path $dest) {
        $script:skipped++
        return
    }
    Copy-Item $src $dest -Force
    $script:copied++
    Log "  [OK] $destKey <- $(Split-Path $src -Leaf)"
}

# ═══════════════════════════════════════════════════════════════
#  1. OpenGameArt Breakout Tiles (verify existing)
# ═══════════════════════════════════════════════════════════════
Log "--- OpenGameArt Breakout (verify) ---"
$oga = Join-Path $Unpacked "opengameart-breakout\Breakout_Tile_Set_Free\Breakout Tile Set Free\PNG"

# Bricks: even numbers 02-14 are the full-color brick variants
Copy-Asset "$oga\02-Breakout-Tiles.png" "Sprites/brick_0"
Copy-Asset "$oga\04-Breakout-Tiles.png" "Sprites/brick_1"
Copy-Asset "$oga\06-Breakout-Tiles.png" "Sprites/brick_2"
Copy-Asset "$oga\08-Breakout-Tiles.png" "Sprites/brick_3"
Copy-Asset "$oga\10-Breakout-Tiles.png" "Sprites/brick_4"
Copy-Asset "$oga\12-Breakout-Tiles.png" "Sprites/brick_5"
Copy-Asset "$oga\14-Breakout-Tiles.png" "Sprites/brick_6"

# Additional brick variants (odd = cracked/damaged versions)
Copy-Asset "$oga\01-Breakout-Tiles.png" "Sprites/brick_0_damaged"
Copy-Asset "$oga\03-Breakout-Tiles.png" "Sprites/brick_1_damaged"
Copy-Asset "$oga\05-Breakout-Tiles.png" "Sprites/brick_2_damaged"
Copy-Asset "$oga\07-Breakout-Tiles.png" "Sprites/brick_3_damaged"
Copy-Asset "$oga\09-Breakout-Tiles.png" "Sprites/brick_4_damaged"
Copy-Asset "$oga\11-Breakout-Tiles.png" "Sprites/brick_5_damaged"
Copy-Asset "$oga\13-Breakout-Tiles.png" "Sprites/brick_6_damaged"

# More brick styles: 16-20
Copy-Asset "$oga\16-Breakout-Tiles.png" "Sprites/brick_grey"
Copy-Asset "$oga\18-Breakout-Tiles.png" "Sprites/brick_gold"
Copy-Asset "$oga\20-Breakout-Tiles.png" "Sprites/brick_dark"

# Paddle variants
Copy-Asset "$oga\32-Breakout-Tiles.png" "Sprites/paddle"
Copy-Asset "$oga\34-Breakout-Tiles.png" "Sprites/paddle_alt"
Copy-Asset "$oga\56-Breakout-Tiles.png" "Sprites/paddle_wide"
Copy-Asset "$oga\57-Breakout-Tiles.png" "Sprites/paddle_short"

# Ball variants
Copy-Asset "$oga\58-Breakout-Tiles.png" "Sprites/ball"
Copy-Asset "$oga\59-Breakout-Tiles.png" "Sprites/ball_alt"
Copy-Asset "$oga\60-Breakout-Tiles.png" "Sprites/ball_fire"

# Power-up / item circles (128x128)
Copy-Asset "$oga\21-Breakout-Tiles.png" "UI/powerup_life"
Copy-Asset "$oga\22-Breakout-Tiles.png" "UI/powerup_grow"
Copy-Asset "$oga\23-Breakout-Tiles.png" "UI/powerup_multi"
Copy-Asset "$oga\24-Breakout-Tiles.png" "UI/powerup_shrink"
Copy-Asset "$oga\25-Breakout-Tiles.png" "UI/powerup_mega"
Copy-Asset "$oga\26-Breakout-Tiles.png" "UI/powerup_slow"
Copy-Asset "$oga\27-Breakout-Tiles.png" "UI/powerup_fast"
Copy-Asset "$oga\28-Breakout-Tiles.png" "UI/heart"
Copy-Asset "$oga\29-Breakout-Tiles.png" "UI/star"
Copy-Asset "$oga\30-Breakout-Tiles.png" "UI/gem"

# ═══════════════════════════════════════════════════════════════
#  1b. OGA Full Kit — Alternate brick set (BRICK/ folder)
# ═══════════════════════════════════════════════════════════════
$ogaFull = Join-Path $Unpacked "oga-breakout-full"
Copy-Asset "$ogaFull\BRICK\REDBRICK.png"          "Sprites/brick2_red"
Copy-Asset "$ogaFull\BRICK\BROWNBRICK.png"         "Sprites/brick2_brown"
Copy-Asset "$ogaFull\BRICK\BLUEEBRICK.png"         "Sprites/brick2_blue"
Copy-Asset "$ogaFull\BRICK\rGREENBRICK.png"        "Sprites/brick2_green"
Copy-Asset "$ogaFull\BRICK\PINKBRICK.png"          "Sprites/brick2_pink"
Copy-Asset "$ogaFull\BRICK\PURPLEBRICK.png"        "Sprites/brick2_purple"
Copy-Asset "$ogaFull\BRICK\LIGHTBLUEEBRICK.png"    "Sprites/brick2_lightblue"
Copy-Asset "$ogaFull\BRICK\metalBRICK.png"         "Sprites/brick2_metal"

# OGA Full Kit — Colored ball variants
Copy-Asset "$ogaFull\BALLS\BLUEBALL.png"    "Sprites/ball_blue"
Copy-Asset "$ogaFull\BALLS\GREENBALL.png"   "Sprites/ball_green"
Copy-Asset "$ogaFull\BALLS\PURPLEBALL.png"  "Sprites/ball_purple"
Copy-Asset "$ogaFull\BALLS\redball.png"     "Sprites/ball_red"

# OGA Full Kit — Colored paddle variants (all sizes)
Copy-Asset "$ogaFull\PADDLE\bluePADDLE.png"        "Sprites/paddle_blue_med"
Copy-Asset "$ogaFull\PADDLE\bluePADDLE large.png"  "Sprites/paddle_blue_large"
Copy-Asset "$ogaFull\PADDLE\bluePADDLEsmall.png"   "Sprites/paddle_blue_small"
Copy-Asset "$ogaFull\PADDLE\GREENPADDLE.png"       "Sprites/paddle_green_med"
Copy-Asset "$ogaFull\PADDLE\GREENPADDLElarge.png"  "Sprites/paddle_green_large"
Copy-Asset "$ogaFull\PADDLE\GREENPADDLEsmall.png"  "Sprites/paddle_green_small"
Copy-Asset "$ogaFull\PADDLE\PURPLEPADDLE.png"      "Sprites/paddle_purple_med"
Copy-Asset "$ogaFull\PADDLE\PURPLEPADDLELARGE.png"  "Sprites/paddle_purple_large"
Copy-Asset "$ogaFull\PADDLE\PURPLEPADDLESMALL.png"  "Sprites/paddle_purple_small"
Copy-Asset "$ogaFull\PADDLE\REDPADDLELARGE.png"    "Sprites/paddle_red_large"
Copy-Asset "$ogaFull\PADDLE\REDPADDLEM.png"        "Sprites/paddle_red_med"
Copy-Asset "$ogaFull\PADDLE\REDPADDLES.png"        "Sprites/paddle_red_small"

# OGA Full Kit — HD paddle variants (PADDLE2)
Copy-Asset "$ogaFull\PADDLE2\BLUEPADDLEpng.png"   "Sprites/paddle_hd_blue"
Copy-Asset "$ogaFull\PADDLE2\GREENPADDLEpng.png"   "Sprites/paddle_hd_green"
Copy-Asset "$ogaFull\PADDLE2\PURPLEADDLEpng.png"   "Sprites/paddle_hd_purple"
Copy-Asset "$ogaFull\PADDLE2\REDPADDLEpng.png"     "Sprites/paddle_hd_red"

# OGA Full Kit — Bonus point sprites
Copy-Asset "$ogaFull\BONUS\25POINTS.png"           "UI/bonus_25"
Copy-Asset "$ogaFull\BONUS\50 POINTS.png"          "UI/bonus_50"
Copy-Asset "$ogaFull\BONUS\100POINTS.png"          "UI/bonus_100"
Copy-Asset "$ogaFull\BONUS\bullet.png"             "UI/bonus_bullet"
Copy-Asset "$ogaFull\BONUS\rBONUS EXTRA LIFE.png"  "UI/bonus_extra_life"

# OGA Full Kit — Text overlays
Copy-Asset "$ogaFull\TEXT\GAME OVER.png"   "UI/text_gameover"
Copy-Asset "$ogaFull\TEXT\GET READY.png"   "UI/text_getready"
Copy-Asset "$ogaFull\TEXT\MENU.png"        "UI/text_menu"
Copy-Asset "$ogaFull\TEXT\OPTIONS.png"     "UI/text_options"
Copy-Asset "$ogaFull\TEXT\RESUME.png"      "UI/text_resume"
Copy-Asset "$ogaFull\TEXT\START.png"       "UI/text_start"
Copy-Asset "$ogaFull\TEXT\YOU WIN.png"     "UI/text_youwin"

# OGA Full Kit — Backgrounds
Copy-Asset "$ogaFull\BACKGROUND\background.png"    "Tiles/game_background"
Copy-Asset "$ogaFull\BACKGROUND\background0.png"   "Tiles/menu_background"

# OGA Retro set — Spritesheet + background
$ogaRetro = Join-Path $Unpacked "oga-breakout-retro"
Copy-Asset "$ogaRetro\breakout_sprites.png" "Sprites/retro_spritesheet"
Copy-Asset "$ogaRetro\breakout_bg.png"      "Tiles/retro_background"

# ═══════════════════════════════════════════════════════════════
#  2. Kenney Game Icons (gear, pause, play, star, music, etc.)
# ═══════════════════════════════════════════════════════════════
Log ""
Log "--- Kenney Game Icons ---"
$kgi = Join-Path $Unpacked "kenney-game-icons\PNG\White\2x"

Copy-Asset "$kgi\gear.png"           "UI/gear"
Copy-Asset "$kgi\pause.png"          "UI/pause"
Copy-Asset "$kgi\home.png"           "UI/home"
Copy-Asset "$kgi\power.png"          "UI/power_icon"
Copy-Asset "$kgi\star.png"           "UI/star_icon"
Copy-Asset "$kgi\trophy.png"         "UI/trophy"
Copy-Asset "$kgi\musicOn.png"        "UI/music_on"
Copy-Asset "$kgi\musicOff.png"       "UI/music_off"
Copy-Asset "$kgi\checkmark.png"      "UI/checkmark"
Copy-Asset "$kgi\cross.png"          "UI/cross"
Copy-Asset "$kgi\arrowUp.png"        "UI/arrow_up"
Copy-Asset "$kgi\arrowDown.png"      "UI/arrow_down"
Copy-Asset "$kgi\arrowLeft.png"      "UI/arrow_left"
Copy-Asset "$kgi\arrowRight.png"     "UI/arrow_right"
Copy-Asset "$kgi\buttonStart.png"    "UI/button_start"
Copy-Asset "$kgi\medal1.png"         "UI/medal_gold"
Copy-Asset "$kgi\medal2.png"         "UI/medal_silver"
Copy-Asset "$kgi\singleplayer.png"   "UI/player_icon"
Copy-Asset "$kgi\multiplayer.png"    "UI/multiplayer_icon"
Copy-Asset "$kgi\plus.png"           "UI/plus"
Copy-Asset "$kgi\minus.png"          "UI/minus"
Copy-Asset "$kgi\wrench.png"         "UI/wrench"
Copy-Asset "$kgi\information.png"    "UI/information"
Copy-Asset "$kgi\warning.png"        "UI/warning"
Copy-Asset "$kgi\target.png"         "UI/target"
Copy-Asset "$kgi\locked.png"         "UI/locked"
Copy-Asset "$kgi\unlocked.png"       "UI/unlocked"
Copy-Asset "$kgi\left.png"           "UI/nav_left"
Copy-Asset "$kgi\right.png"          "UI/nav_right"
Copy-Asset "$kgi\leaderboardsSimple.png" "UI/leaderboard"
Copy-Asset "$kgi\stop.png"           "UI/stop"

# ═══════════════════════════════════════════════════════════════
#  3. Kenney UI Pack — Blue theme (buttons, sliders, checkboxes)
# ═══════════════════════════════════════════════════════════════
Log ""
Log "--- Kenney UI Pack (Blue) ---"
$kui = Join-Path $Unpacked "kenney-ui-pack\PNG\Blue\Default"

Copy-Asset "$kui\button_rectangle_depth_flat.png"   "UI/button_blue"
Copy-Asset "$kui\button_rectangle_depth_gloss.png"  "UI/button_blue_gloss"
Copy-Asset "$kui\button_rectangle_flat.png"          "UI/button_blue_flat"
Copy-Asset "$kui\button_round_depth_flat.png"        "UI/button_round"
Copy-Asset "$kui\arrow_basic_n.png"                  "UI/ui_arrow_up"
Copy-Asset "$kui\arrow_basic_s.png"                  "UI/ui_arrow_down"
Copy-Asset "$kui\arrow_basic_e.png"                  "UI/ui_arrow_right"
Copy-Asset "$kui\arrow_basic_w.png"                  "UI/ui_arrow_left"
Copy-Asset "$kui\check_square_color.png"             "UI/checkbox"
Copy-Asset "$kui\check_square_color_checkmark.png"   "UI/checkbox_checked"
Copy-Asset "$kui\slide_horizontal_color.png"         "UI/slider_track"
Copy-Asset "$kui\slide_hangle.png"                   "UI/slider_handle"
Copy-Asset "$kui\icon_checkmark.png"                 "UI/icon_check"
Copy-Asset "$kui\icon_cross.png"                     "UI/icon_x"

# Green buttons for "Play / Start"
$kuig = Join-Path $Unpacked "kenney-ui-pack\PNG\Green\Default"
Copy-Asset "$kuig\button_rectangle_depth_flat.png"  "UI/button_green"
Copy-Asset "$kuig\button_rectangle_depth_gloss.png" "UI/button_green_gloss"

# Red buttons for "Quit / Cancel"
$kuir = Join-Path $Unpacked "kenney-ui-pack\PNG\Red\Default"
Copy-Asset "$kuir\button_rectangle_depth_flat.png"  "UI/button_red"

# Yellow for "Options"
$kuiy = Join-Path $Unpacked "kenney-ui-pack\PNG\Yellow\Default"
Copy-Asset "$kuiy\button_rectangle_depth_flat.png"  "UI/button_yellow"

# Grey panel / frame from Extra
$kuie = Join-Path $Unpacked "kenney-ui-pack\PNG\Extra\Default"
if (Test-Path $kuie) {
    $panels = Get-ChildItem $kuie -Filter "*.png" -File | Select-Object -First 5
    foreach ($p in $panels) {
        $key = "UI/panel_$($p.BaseName -replace '[^a-zA-Z0-9]','_')"
        Copy-Asset $p.FullName $key
    }
}

# ═══════════════════════════════════════════════════════════════
#  4. Kenney Input Prompts — Keyboard keys for HUD
# ═══════════════════════════════════════════════════════════════
Log ""
Log "--- Kenney Input Prompts (Keyboard) ---"
$kip = Join-Path $Unpacked "kenney-input-prompts\Keyboard & Mouse\Default"

Copy-Asset "$kip\keyboard_space.png"                  "UI/key_space"
Copy-Asset "$kip\keyboard_space_icon.png"             "UI/key_space_icon"
Copy-Asset "$kip\keyboard_arrow_left.png"             "UI/key_arrow_left"
Copy-Asset "$kip\keyboard_arrow_right.png"            "UI/key_arrow_right"
Copy-Asset "$kip\keyboard_arrow_up.png"               "UI/key_arrow_up"
Copy-Asset "$kip\keyboard_arrow_down.png"             "UI/key_arrow_down"
Copy-Asset "$kip\keyboard_arrows_all.png"             "UI/key_arrows"
Copy-Asset "$kip\keyboard_escape.png"                 "UI/key_escape"
Copy-Asset "$kip\keyboard_p.png"                      "UI/key_p"
Copy-Asset "$kip\keyboard_f.png"                      "UI/key_f"
Copy-Asset "$kip\keyboard_h.png"                      "UI/key_h"
Copy-Asset "$kip\keyboard_o.png"                      "UI/key_o"

# ═══════════════════════════════════════════════════════════════
#  5. Kenney Brick Pack — Decorative background tiles
# ═══════════════════════════════════════════════════════════════
Log ""
Log "--- Kenney Brick Pack (Backgrounds) ---"
$kbp = Join-Path $Unpacked "kenney-brick-pack"
$brickPngs = Get-ChildItem $kbp -Recurse -Filter "*.png" -File | Where-Object { $_.Length -gt 2000 } | Select-Object -First 4
$idx = 0
foreach ($bp in $brickPngs) {
    Copy-Asset $bp.FullName "Tiles/decor_brick_$idx"
    $idx++
}

# ═══════════════════════════════════════════════════════════════
#  6. Kenney Pixel UI Pack — Pixel-art alternatives
# ═══════════════════════════════════════════════════════════════
Log ""
Log "--- Kenney Pixel UI Pack ---"
$kpui = Join-Path $Unpacked "kenney-pixel-ui-pack"
$pixelPngs = Get-ChildItem $kpui -Recurse -Filter "*.png" -File
foreach ($pp in $pixelPngs) {
    $key = "UI/pixel_$($pp.BaseName -replace '[^a-zA-Z0-9]','_')"
    Copy-Asset $pp.FullName $key
}

# ═══════════════════════════════════════════════════════════════
#  7. Kenney Platformer Bricks — Decorative textures
# ═══════════════════════════════════════════════════════════════
Log ""
Log "--- Kenney Platformer Bricks ---"
$kplat = Join-Path $Unpacked "kenney-platformer-bricks"
$platPngs = Get-ChildItem $kplat -Recurse -Filter "*.png" -File | Where-Object { $_.Length -gt 1000 } | Select-Object -First 6
$idx = 0
foreach ($pp in $platPngs) {
    Copy-Asset $pp.FullName "Tiles/platform_$idx"
    $idx++
}

# ═══════════════════════════════════════════════════════════════
#  8. Mirror canonical Assets to all platform builds
# ═══════════════════════════════════════════════════════════════
Log ""
Log "--- Mirror Assets to All Builds ---"

function Mirror-AssetsToTarget($targetAssetsDir) {
    New-Item -ItemType Directory -Path $targetAssetsDir -Force | Out-Null

    # Make target uniform with canonical WPF assets
    if (Test-Path $targetAssetsDir) {
        Get-ChildItem $targetAssetsDir -Force | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    }
    Copy-Item (Join-Path $AssetsDir "*") $targetAssetsDir -Recurse -Force

    $count = (Get-ChildItem $targetAssetsDir -Recurse -Filter "*.png" -File).Count
    Log "  [SYNC] $($targetAssetsDir.Replace($Root + '\\', '')) <= $count PNGs"
}

$assetTargets = @(
    (Join-Path $Root "versions\windows-wpf\Assets"),
    (Join-Path $Root "anime finder macos\Assets"),
    (Join-Path $Root "versions\windows\Assets"),
    (Join-Path $Root "versions\html\assets"),
    (Join-Path $Root "versions\android-phone\assets"),
    (Join-Path $Root "versions\android-tablet\assets"),
    (Join-Path $Root "versions\ipad\assets"),
    (Join-Path $Root "versions\iphone\assets"),
    (Join-Path $Root "versions\macos\assets"),
    (Join-Path $Root "web\assets"),
    (Join-Path $Root "mobile\www\assets")
)

foreach ($t in $assetTargets) {
    Mirror-AssetsToTarget $t
}

# ═══════════════════════════════════════════════════════════════
Log ""
Log "=== Integration Complete ==="
Log "Copied: $copied new files"
Log "Skipped: $skipped existing files"
$total = (Get-ChildItem $AssetsDir -Recurse -Filter "*.png" -File).Count
Log "Total PNG assets in project: $total"
Write-Host "`nDone! $copied new + $skipped existing = $($copied+$skipped) assets mapped." -ForegroundColor Green
