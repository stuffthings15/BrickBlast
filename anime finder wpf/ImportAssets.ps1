# ============================================================================
# SuperGameAsset Import Helper — Brick Blast WPF
#
# USAGE:
#   1. Download free asset packs from supergameasset.com
#   2. Extract them into C:\GameAssets\SuperGameAsset\
#   3. Run this script (optional — the game auto-imports at startup)
#
# WHAT IT DOES:
#   Previews what the game's AssetImporter will find and copy.
#   Does NOT modify files — just scans and reports.
# ============================================================================

$SourceRoot = "C:\GameAssets\SuperGameAsset"
$TargetRoot = Join-Path $PSScriptRoot "Assets"

Write-Host "`n=== SuperGameAsset Import Preview ===" -ForegroundColor Cyan
Write-Host "Source: $SourceRoot"
Write-Host "Target: $TargetRoot`n"

if (-not (Test-Path $SourceRoot)) {
    Write-Host "[NOT FOUND] $SourceRoot" -ForegroundColor Red
    Write-Host ""
    Write-Host "To set up:" -ForegroundColor Yellow
    Write-Host "  1. Create folder: C:\GameAssets\SuperGameAsset\"
    Write-Host "  2. Download these FREE packs from supergameasset.com:"
    Write-Host "     - Basic RPG Item Icons (Free)"
    Write-Host "       https://www.supergameasset.com/basic-rpg-item-icons-free-game-asset.html"
    Write-Host "     - Male Warrior Sample (Free)"
    Write-Host "       https://www.supergameasset.com/male-warrior-sample-game-asset.html"
    Write-Host "     - Desert Map Sample (Free)"
    Write-Host "       https://www.supergameasset.com/desert-map-sample-game-asset.html"
    Write-Host "  3. Extract ZIPs into subfolders of C:\GameAssets\SuperGameAsset\"
    Write-Host "  4. Run the game — import happens automatically at startup"
    Write-Host ""
    exit 0
}

$images = Get-ChildItem -Path $SourceRoot -Recurse -Include *.png,*.jpg,*.jpeg,*.bmp
Write-Host "Found $($images.Count) image file(s):`n" -ForegroundColor Green

$bgKeywords   = @("map","desert","background","bg","landscape","scene","terrain")
$charKeywords = @("warrior","character","hero","player","knight","fighter","male","idle","walk")

foreach ($img in $images) {
    $name   = $img.BaseName.ToLower()
    $parent = $img.Directory.Name.ToLower()
    $combo  = "$parent/$name"

    $category = "UI/Icon"
    foreach ($kw in $bgKeywords) {
        if ($combo -like "*$kw*") { $category = "Tiles/Background"; break }
    }
    if ($category -eq "UI/Icon") {
        foreach ($kw in $charKeywords) {
            if ($combo -like "*$kw*") { $category = "Characters/Mascot"; break }
        }
    }

    $color = switch -Wildcard ($category) {
        "Tiles*"      { "DarkYellow" }
        "Characters*" { "Magenta" }
        default       { "White" }
    }

    $relative = $img.FullName.Replace($SourceRoot, "").TrimStart("\")
    Write-Host "  [$category]" -ForegroundColor $color -NoNewline
    Write-Host " $relative"
}

Write-Host "`n=== Expected Asset Mapping ===" -ForegroundColor Cyan
Write-Host "  UI/powerup_life     <- heart, health, potion_red"
Write-Host "  UI/powerup_grow     <- potion_green, grow, expand"
Write-Host "  UI/powerup_multi    <- star, multiply, triple"
Write-Host "  UI/powerup_shrink   <- potion_blue, shrink, poison"
Write-Host "  UI/powerup_mega     <- shield, armor, protect"
Write-Host "  UI/powerup_slow     <- ice, frost, cold"
Write-Host "  UI/powerup_fast     <- lightning, speed, fire"
Write-Host "  Tiles/menu_background <- desert, map, landscape"
Write-Host "  Characters/menu_mascot <- warrior, hero, knight"
Write-Host ""
Write-Host "The game auto-imports at startup. Just press F5!" -ForegroundColor Green
