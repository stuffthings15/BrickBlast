# =============================================================================
# normalize_assets.ps1 — BrickBlast WPF Asset Normalizer
#
# Scans unpacked external assets, selects the best match for each game
# asset key, copies to the WPF project's Assets/ folder with standardized
# names. The AssetManager auto-loads them at runtime (disk overrides procedural).
#
# Run AFTER fetch_assets.ps1 (or after manual downloads are placed).
# Idempotent: existing files are skipped unless -Force is specified.
# =============================================================================

param([switch]$Force)

$ErrorActionPreference = "Continue"

$Root       = Split-Path $PSScriptRoot -Parent
$UnpackDir  = Join-Path $Root "ExternalAssets\unpacked"
$TargetRoot = Join-Path $Root "anime finder wpf\Assets"
$ReportFile = Join-Path $Root "ExternalAssets\normalize_report.txt"
$CreditsFile = Join-Path $Root "ExternalAssets\ASSET_CREDITS.md"

"" | Set-Content $ReportFile

function Write-Report($msg) {
    Write-Host $msg
    Add-Content $ReportFile $msg
}

# ─── Scan all PNGs from unpacked sources ───
function Get-AllUnpackedPngs {
    if (-not (Test-Path $UnpackDir)) { return @() }
    return Get-ChildItem $UnpackDir -Recurse -Include "*.png","*.jpg" -File |
        Select-Object FullName, Name, BaseName, @{N='Source';E={
            $rel = $_.FullName.Replace($UnpackDir, "").TrimStart("\")
            ($rel -split "\\")[0]
        }}, @{N='SizePx';E={
            try {
                Add-Type -AssemblyName System.Drawing
                $img = [System.Drawing.Image]::FromFile($_.FullName)
                $s = "$($img.Width)x$($img.Height)"
                $img.Dispose()
                $s
            } catch { "unknown" }
        }}
}

# ─── Keyword matching: find best file for a game asset key ───
function Find-BestMatch($allFiles, $keywords, $preferSources) {
    # Score each file
    $scored = @()
    foreach ($f in $allFiles) {
        $name = $f.BaseName.ToLower()
        $matchCount = 0
        foreach ($kw in $keywords) {
            if ($name -like "*$kw*") { $matchCount++ }
        }
        if ($matchCount -eq 0) { continue }
        # Source priority bonus
        $srcBonus = 0
        for ($i = 0; $i -lt $preferSources.Count; $i++) {
            if ($f.Source -eq $preferSources[$i]) {
                $srcBonus = ($preferSources.Count - $i) * 10
                break
            }
        }
        $scored += [PSCustomObject]@{
            File = $f
            Score = $matchCount + $srcBonus
        }
    }
    $best = $scored | Sort-Object Score -Descending | Select-Object -First 1
    if ($best) { return $best.File }
    return $null
}

# ─── Copy to project Assets/ ───
function Copy-AssetToProject($srcFile, $assetKey) {
    $parts = $assetKey -split "/"
    $subDir = $parts[0]
    $fileName = $parts[1]
    $ext = [System.IO.Path]::GetExtension($srcFile.FullName)
    if ([string]::IsNullOrEmpty($ext)) { $ext = ".png" }

    $destDir = Join-Path $TargetRoot $subDir
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    $destPath = Join-Path $destDir "$fileName$ext"

    if ((Test-Path $destPath) -and -not $Force) {
        Write-Report "  [SKIP] $assetKey (already exists)"
        return $false
    }
    Copy-Item $srcFile.FullName $destPath -Force
    Write-Report "  [COPY] $($srcFile.Name) -> Assets/$subDir/$fileName$ext (from $($srcFile.Source))"
    return $true
}

# ─── Find multiple matching bricks (sorted by color keyword) ───
function Find-BrickSet($allFiles, $preferSources) {
    $colorKeywords = @("red","orange","yellow","green","blue","purple","pink")
    $bricks = @()
    for ($i = 0; $i -lt $colorKeywords.Count; $i++) {
        $color = $colorKeywords[$i]
        $match = Find-BestMatch $allFiles @("brick","block","element","tile",$color) $preferSources
        if (-not $match) {
            # Fallback: any brick-like file
            $match = Find-BestMatch $allFiles @("brick","block","element") $preferSources
        }
        if ($match) { $bricks += [PSCustomObject]@{ Index = $i; File = $match } }
    }
    return $bricks
}

# ═══════════════════════════════════════════════════════════════
#  MAIN
# ═══════════════════════════════════════════════════════════════

Write-Report "=== BrickBlast Asset Normalization ==="
Write-Report "Source: $UnpackDir"
Write-Report "Target: $TargetRoot"
Write-Report ""

$allFiles = Get-AllUnpackedPngs
Write-Report "Found $($allFiles.Count) image file(s) across all unpacked sources."
Write-Report ""

if ($allFiles.Count -eq 0) {
    Write-Report "[WARN] No unpacked assets found. Run fetch_assets.ps1 first."
    Write-Report "       Or place manual downloads in ExternalAssets\unpacked\"
    exit 0
}

$credits = @("# Asset Credits — BrickBlast WPF`n")
$importCount = 0
$skipCount = 0

# ─── Gameplay sprites ───
Write-Report "--- Gameplay Sprites ---"
$gameplaySources = @("opengameart-breakout","itch-breakout")

# Bricks
$brickSet = Find-BrickSet $allFiles $gameplaySources
foreach ($b in $brickSet) {
    if (Copy-AssetToProject $b.File "Sprites/brick_$($b.Index)") {
        $importCount++
        $credits += "- Sprites/brick_$($b.Index): $($b.File.Name) from $($b.File.Source)"
    } else { $skipCount++ }
}

# Paddle
$paddle = Find-BestMatch $allFiles @("paddle","bar","platform") $gameplaySources
if ($paddle) {
    if (Copy-AssetToProject $paddle "Sprites/paddle") { $importCount++; $credits += "- Sprites/paddle: $($paddle.Name) from $($paddle.Source)" } else { $skipCount++ }
} else { Write-Report "  [MISS] No paddle sprite found" }

# Ball
$ball = Find-BestMatch $allFiles @("ball","sphere","orb") $gameplaySources
if ($ball) {
    if (Copy-AssetToProject $ball "Sprites/ball") { $importCount++; $credits += "- Sprites/ball: $($ball.Name) from $($ball.Source)" } else { $skipCount++ }
} else { Write-Report "  [MISS] No ball sprite found" }

# ─── UI assets ───
Write-Report ""
Write-Report "--- UI Assets ---"
$uiSources = @("kenney-game-icons","kenney-ui-pack","kenney-pixel-ui-pack","itch-breakout")

$uiMappings = @(
    @{ Key = "UI/heart";       Keywords = @("heart","life","lives") },
    @{ Key = "UI/star";        Keywords = @("star","score","gem") },
    @{ Key = "UI/shield";      Keywords = @("shield","armor","protect") },
    @{ Key = "UI/button_blue"; Keywords = @("button","blue","btn") },
    @{ Key = "UI/panel";       Keywords = @("panel","window","frame","glass") },
    @{ Key = "UI/gear";        Keywords = @("gear","settings","cog","wrench") },
    @{ Key = "UI/pause";       Keywords = @("pause","stop") },
    @{ Key = "UI/play";        Keywords = @("play","start","right","forward") },
    @{ Key = "UI/home";        Keywords = @("home","house","menu") },
    @{ Key = "UI/key_space";   Keywords = @("space","keyboard_space","key_space") },
    @{ Key = "UI/key_arrow";   Keywords = @("arrow","left","right","key_arrow") }
)

foreach ($m in $uiMappings) {
    $match = Find-BestMatch $allFiles $m.Keywords $uiSources
    if ($match) {
        if (Copy-AssetToProject $match $m.Key) { $importCount++; $credits += "- $($m.Key): $($match.Name) from $($match.Source)" } else { $skipCount++ }
    } else {
        Write-Report "  [MISS] $($m.Key) — no match found (procedural fallback will be used)"
    }
}

# ─── Background ───
Write-Report ""
Write-Report "--- Backgrounds ---"
$bgSources = @("kenney-brick-pack","kenney-platformer-bricks")
$bg = Find-BestMatch $allFiles @("background","bg","wall","scene") $bgSources
if ($bg) {
    if (Copy-AssetToProject $bg "Tiles/menu_background") { $importCount++; $credits += "- Tiles/menu_background: $($bg.Name) from $($bg.Source)" } else { $skipCount++ }
} else { Write-Report "  [MISS] No background art found" }

# ─── Credits file ───
$credits += "`n## Licenses`nAll assets listed above are CC0 (Creative Commons Zero) — free for any use.`n"
$credits += "## Sources`n"
$credits += "- OpenGameArt: https://opengameart.org/`n"
$credits += "- Kenney: https://kenney.nl/`n"
$credits += "- Ethereal Regions (itch.io): https://ethereal-regions.itch.io/`n"
$credits | Set-Content $CreditsFile

Write-Report ""
Write-Report "=== Normalization Complete ==="
Write-Report "Imported: $importCount | Skipped: $skipCount"
Write-Report "Credits: $CreditsFile"
Write-Host "`nDone. Real sprites in Assets/ will auto-override procedural fallbacks at runtime." -ForegroundColor Green
