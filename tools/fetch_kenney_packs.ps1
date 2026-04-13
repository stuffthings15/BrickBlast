# =============================================================================
# fetch_kenney_packs.ps1 — Downloads all 6 Kenney CC0 asset packs
#
# Uses corrected URL extraction: full-URL regex instead of href-only pattern.
# Kenney.nl embeds ZIP links as full https:// URLs in page content.
# =============================================================================

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$Root        = Split-Path $PSScriptRoot -Parent
$DownloadDir = Join-Path $Root "ExternalAssets\downloads"
$UnpackDir   = Join-Path $Root "ExternalAssets\unpacked"
$LogFile     = Join-Path $Root "ExternalAssets\fetch_log.txt"
$UA          = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) BrickBlast-AssetFetcher/2.0"

New-Item -ItemType Directory -Path $DownloadDir -Force | Out-Null
New-Item -ItemType Directory -Path $UnpackDir -Force | Out-Null

function Write-Log($msg) {
    $ts = Get-Date -Format "HH:mm:ss"
    $line = "[$ts] $msg"
    Write-Host $line
    Add-Content -Path $LogFile -Value $line
}

$packs = @(
    @{ id="kenney-ui-pack";           url="https://kenney.nl/assets/ui-pack" },
    @{ id="kenney-pixel-ui-pack";     url="https://kenney.nl/assets/pixel-ui-pack" },
    @{ id="kenney-game-icons";        url="https://kenney.nl/assets/game-icons" },
    @{ id="kenney-input-prompts";     url="https://kenney.nl/assets/input-prompts" },
    @{ id="kenney-brick-pack";        url="https://kenney.nl/assets/brick-pack" },
    @{ id="kenney-platformer-bricks"; url="https://kenney.nl/assets/platformer-bricks" }
)

Write-Log "=== Kenney CC0 Pack Downloader ==="
$totalDownloaded = 0
$totalSkipped = 0
$totalFailed = 0

foreach ($pack in $packs) {
    $zipPath = Join-Path $DownloadDir "$($pack.id).zip"
    $unpackPath = Join-Path $UnpackDir $pack.id

    # Skip if already unpacked with files
    if ((Test-Path $unpackPath) -and (Get-ChildItem $unpackPath -Recurse -File).Count -gt 5) {
        Write-Log "[SKIP] $($pack.id) — already unpacked ($((Get-ChildItem $unpackPath -Recurse -File).Count) files)"
        $totalSkipped++
        continue
    }

    # Skip if ZIP already downloaded
    if (Test-Path $zipPath) {
        Write-Log "[CACHED] $($pack.id) — ZIP exists, unpacking"
    } else {
        # Fetch page and extract full ZIP URL
        Write-Log "[FETCH] $($pack.url)"
        try {
            $response = Invoke-WebRequest -Uri $pack.url -UseBasicParsing -UserAgent $UA -TimeoutSec 20
            $match = [regex]::Match($response.Content, '(https?://kenney\.nl/media/pages/assets/[^"''\s>]+\.zip)')
            if (-not $match.Success) {
                Write-Log "[FAIL] $($pack.id) — no ZIP URL found in page"
                $totalFailed++
                continue
            }
            $dlUrl = $match.Value
            Write-Log "[DOWNLOAD] $dlUrl"
            Start-Sleep -Seconds 1
            Invoke-WebRequest -Uri $dlUrl -OutFile $zipPath -UseBasicParsing -UserAgent $UA -TimeoutSec 120
            $sizeMB = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
            Write-Log "[OK] $($pack.id) — $sizeMB MB"
        } catch {
            Write-Log "[FAIL] $($pack.id) — $($_.Exception.Message)"
            $totalFailed++
            continue
        }
    }

    # Unpack
    if (Test-Path $zipPath) {
        New-Item -ItemType Directory -Path $unpackPath -Force | Out-Null
        try {
            Expand-Archive -Path $zipPath -DestinationPath $unpackPath -Force
            $count = (Get-ChildItem $unpackPath -Recurse -File).Count
            Write-Log "[UNPACKED] $($pack.id) — $count files"
            $totalDownloaded++
        } catch {
            Write-Log "[FAIL] Unpack $($pack.id) — $($_.Exception.Message)"
            $totalFailed++
        }
    }

    Start-Sleep -Seconds 1
}

Write-Log ""
Write-Log "=== Done: $totalDownloaded downloaded, $totalSkipped cached, $totalFailed failed ==="
