# =============================================================================
# fetch_new_sources.ps1 — Downloads NEW breakout asset packs
#
# Sources auto-downloaded:
#   1. OpenGameArt – Breakout Game Assets (FULL KIT) — CC0
#   2. OpenGameArt – Breakout Graphics (Retro) — CC0
#   3. itch.io – Jamie Cross Breakout (CC0, min_price=0)
#
# Sources marked manual:
#   4. itch.io listing page — Cloudflare blocked
#   5. pkgames itch.io — requires browser session
# =============================================================================

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$Root = Split-Path $PSScriptRoot -Parent
$DL   = Join-Path $Root "ExternalAssets\downloads"
$UP   = Join-Path $Root "ExternalAssets\unpacked"
$Log  = Join-Path $Root "ExternalAssets\fetch_log_new.txt"
$UA   = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) BrickBlast/2.0"

New-Item -ItemType Directory -Path $DL -Force | Out-Null
New-Item -ItemType Directory -Path $UP -Force | Out-Null
"" | Set-Content $Log

function Log($m) { $t = Get-Date -Format "HH:mm:ss"; "[$t] $m" | Tee-Object -FilePath $Log -Append }

Log "=== New Asset Source Download ==="

# ─── 1. OpenGameArt – Breakout Game Assets (FULL KIT) ───
$zip1 = "$DL\oga-breakout-full.zip"
$up1  = "$UP\oga-breakout-full"
if ((Test-Path $up1) -and (Get-ChildItem $up1 -Recurse -File).Count -gt 50) {
    Log "[SKIP] oga-breakout-full already unpacked"
} else {
    Log "[DOWNLOAD] OGA Breakout Game Assets (FULL KIT)"
    Invoke-WebRequest -Uri "https://opengameart.org/sites/default/files/BREAKOUT.zip" -OutFile $zip1 -UseBasicParsing -UserAgent $UA -TimeoutSec 60
    Log "[OK] $([math]::Round((Get-Item $zip1).Length/1KB)) KB"
    Expand-Archive -Path $zip1 -DestinationPath $up1 -Force
    Log "[UNPACKED] $((Get-ChildItem $up1 -Recurse -File).Count) files"
}

Start-Sleep 2

# ─── 2. OpenGameArt – Breakout Graphics (Retro) ───
$up2 = "$UP\oga-breakout-retro"
if ((Test-Path $up2) -and (Get-ChildItem $up2 -File).Count -ge 2) {
    Log "[SKIP] oga-breakout-retro already present"
} else {
    Log "[DOWNLOAD] OGA Breakout Graphics (Retro)"
    New-Item -ItemType Directory -Path $up2 -Force | Out-Null
    Invoke-WebRequest -Uri "https://opengameart.org/sites/default/files/breakout_sprites.png" -OutFile "$up2\breakout_sprites.png" -UseBasicParsing -UserAgent $UA -TimeoutSec 30
    Invoke-WebRequest -Uri "https://opengameart.org/sites/default/files/breakout_bg.png" -OutFile "$up2\breakout_bg.png" -UseBasicParsing -UserAgent $UA -TimeoutSec 30
    Log "[OK] 2 files (sprite sheet + background)"
}

Start-Sleep 2

# ─── 3. itch.io – Jamie Cross (free, min_price=0) ───
$zip3 = "$DL\jamiecross-breakout.zip"
$up3  = "$UP\jamiecross-breakout"
if ((Test-Path $up3) -and (Get-ChildItem $up3 -Recurse -File).Count -gt 50) {
    Log "[SKIP] jamiecross-breakout already unpacked"
} else {
    Log "[DOWNLOAD] Jamie Cross Breakout (itch.io, free)"
    try {
        # Step 1: Get page + CSRF token
        $sess = Invoke-WebRequest -Uri "https://jamiecross.itch.io/breakout-brick-breaker-game-tile-set-free" -UseBasicParsing -UserAgent $UA -TimeoutSec 15 -SessionVariable ws
        $csrf = [regex]::Match($sess.Content, 'name="csrf_token"\s+value="([^"]+)"').Groups[1].Value
        # Step 2: Get download page URL
        $dlResp = Invoke-WebRequest -Uri "https://jamiecross.itch.io/breakout-brick-breaker-game-tile-set-free/download_url" -UseBasicParsing -UserAgent $UA -Method Post -WebSession $ws -Headers @{"X-Requested-With"="XMLHttpRequest"} -Body "csrf_token=$csrf" -ContentType "application/x-www-form-urlencoded" -TimeoutSec 15
        $dlUrl = ($dlResp.Content | ConvertFrom-Json).url
        # Step 3: Load download page + get CSRF + upload_id
        $dlPage = Invoke-WebRequest -Uri $dlUrl -UseBasicParsing -UserAgent $UA -WebSession $ws -TimeoutSec 15
        $csrf2 = [regex]::Match($dlPage.Content, 'name="csrf_token"\s+value="([^"]+)"').Groups[1].Value
        $uploadId = [regex]::Match($dlPage.Content, 'data-upload_id="(\d+)"').Groups[1].Value
        # Step 4: Get CDN URL
        $fileResp = Invoke-WebRequest -Uri "https://jamiecross.itch.io/breakout-brick-breaker-game-tile-set-free/file/$uploadId" -UseBasicParsing -UserAgent $UA -Method Post -WebSession $ws -Headers @{"X-Requested-With"="XMLHttpRequest"} -Body "csrf_token=$csrf2&uuid=&after_download_lightbox=true" -ContentType "application/x-www-form-urlencoded" -TimeoutSec 15
        $cdnUrl = [regex]::Match($fileResp.Content, '"url":"([^"]+)"').Groups[1].Value -replace '\\/', '/'
        # Step 5: Download from CDN
        Invoke-WebRequest -Uri $cdnUrl -OutFile $zip3 -UseBasicParsing -TimeoutSec 60
        Log "[OK] $([math]::Round((Get-Item $zip3).Length/1KB)) KB"
        Expand-Archive -Path $zip3 -DestinationPath $up3 -Force
        Log "[UNPACKED] $((Get-ChildItem $up3 -Recurse -File).Count) files"
    } catch {
        Log "[FAIL] Jamie Cross: $($_.Exception.Message)"
        Log "[MANUAL] Download from https://jamiecross.itch.io/breakout-brick-breaker-game-tile-set-free"
    }
}

# ─── Manual sources ───
Log ""
Log "[MANUAL] itch.io/game-assets/free/tag-breakout/tag-sprites — Cloudflare challenge blocks automation"
Log "[MANUAL] pkgames.itch.io/breakout-full-ctjs-project — requires browser session for download"
Log ""
Log "=== Done ==="
