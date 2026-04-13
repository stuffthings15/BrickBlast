# =============================================================================
# fetch_assets.ps1 — BrickBlast WPF Asset Downloader
#
# Downloads FREE, CC0-licensed game art from public sources.
# RULES:
#   - No login, no paywall bypass, no anti-bot circumvention
#   - If a source can't be downloaded cleanly, log it as manual
#   - Idempotent: skip files already downloaded
#   - Respectful: 2-second delay between requests
# =============================================================================

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

$Root        = Split-Path $PSScriptRoot -Parent
$DownloadDir = Join-Path $Root "ExternalAssets\downloads"
$UnpackDir   = Join-Path $Root "ExternalAssets\unpacked"
$LogFile     = Join-Path $Root "ExternalAssets\fetch_log.txt"
$ManualFile  = Join-Path $Root "ExternalAssets\manual_downloads_needed.txt"
$ManifestFile = Join-Path $PSScriptRoot "asset_manifest.json"

New-Item -ItemType Directory -Path $DownloadDir -Force | Out-Null
New-Item -ItemType Directory -Path $UnpackDir   -Force | Out-Null

$UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) BrickBlast-AssetFetcher/1.0"

# ─── Logging ───
function Write-Log($msg) {
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$ts] $msg"
    Write-Host $line
    Add-Content -Path $LogFile -Value $line
}

function Write-Manual($source, $reason) {
    $entry = "`n## $($source.name)`n- Page: $($source.source_page)`n- License: $($source.expected_license)`n- Reason: $reason`n- Action: Download manually, extract to $($source.destination_folder)`n"
    Add-Content -Path $ManualFile -Value $entry
    Write-Log "[MANUAL] $($source.id): $reason"
}

# ─── Kenney Download: scrape page for ZIP link ───
function Get-KenneyAsset($source) {
    $zipPath = Join-Path $DownloadDir "$($source.id).zip"
    if (Test-Path $zipPath) {
        Write-Log "[SKIP] $($source.id) already downloaded"
        return $zipPath
    }
    Write-Log "[FETCH] Scraping $($source.source_page) for download link..."
    try {
        $response = Invoke-WebRequest -Uri $source.source_page -UserAgent $UserAgent -UseBasicParsing -TimeoutSec 30
        $html = $response.Content
        # Kenney embeds full ZIP URLs in page content (not always in href attributes)
        $match = [regex]::Match($html, '(https?://kenney\.nl/media/pages/assets/[^"''\s>]+\.zip)')
        if (-not $match.Success) {
            # Fallback: try href-based pattern
            $match = [regex]::Match($html, 'href="(/media/pages/assets/[^"]+\.zip)"')
        }
        if ($match.Success) {
            $dlUrl = $match.Groups[1].Value
            if ($dlUrl.StartsWith("/")) { $dlUrl = "https://kenney.nl$dlUrl" }
            Write-Log "[DOWNLOAD] $dlUrl"
            Start-Sleep -Seconds 2
            Invoke-WebRequest -Uri $dlUrl -OutFile $zipPath -UserAgent $UserAgent -UseBasicParsing -TimeoutSec 120
            Write-Log "[OK] Saved $zipPath ($([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB)"
            return $zipPath
        } else {
            Write-Manual $source "Could not find ZIP download link on page"
            return $null
        }
    } catch {
        Write-Manual $source "HTTP error: $($_.Exception.Message)"
        return $null
    }
}

# ─── OpenGameArt Download: scrape page for file attachments ───
function Get-OpenGameArtAsset($source) {
    $destDir = Join-Path $DownloadDir $source.id
    $marker  = Join-Path $destDir ".downloaded"
    if (Test-Path $marker) {
        Write-Log "[SKIP] $($source.id) already downloaded"
        return $destDir
    }
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    Write-Log "[FETCH] Scraping $($source.source_page) for file links..."
    try {
        $response = Invoke-WebRequest -Uri $source.source_page -UserAgent $UserAgent -UseBasicParsing -TimeoutSec 30
        $html = $response.Content
        # Match file attachment links: /sites/default/files/{filename}
        $matches = [regex]::Matches($html, 'href="(/sites/default/files/[^"]+\.(zip|png|jpg))"')
        if ($matches.Count -eq 0) {
            # Try alternate pattern
            $matches = [regex]::Matches($html, 'href="(https?://opengameart\.org/sites/default/files/[^"]+\.(zip|png|jpg))"')
        }
        if ($matches.Count -eq 0) {
            Write-Manual $source "No downloadable file attachments found on page"
            return $null
        }
        $downloaded = 0
        foreach ($m in $matches) {
            $fileUrl = $m.Groups[1].Value
            if ($fileUrl.StartsWith("/")) { $fileUrl = "https://opengameart.org$fileUrl" }
            $fileName = [System.IO.Path]::GetFileName($fileUrl)
            $fileDest = Join-Path $destDir $fileName
            if (-not (Test-Path $fileDest)) {
                Write-Log "[DOWNLOAD] $fileUrl"
                Start-Sleep -Seconds 2
                try {
                    Invoke-WebRequest -Uri $fileUrl -OutFile $fileDest -UserAgent $UserAgent -UseBasicParsing -TimeoutSec 120
                    $downloaded++
                } catch {
                    Write-Log "[WARN] Failed to download $fileName : $($_.Exception.Message)"
                }
            }
        }
        if ($downloaded -gt 0 -or (Get-ChildItem $destDir -File).Count -gt 0) {
            New-Item -Path $marker -ItemType File -Force | Out-Null
            Write-Log "[OK] $($source.id): $downloaded file(s) downloaded"
            return $destDir
        } else {
            Write-Manual $source "All file downloads failed"
            return $null
        }
    } catch {
        Write-Manual $source "HTTP error: $($_.Exception.Message)"
        return $null
    }
}

# ─── Unpack ZIPs ───
function Expand-Asset($zipPath, $destDir) {
    if (Test-Path $destDir) {
        $existing = Get-ChildItem $destDir -Recurse -File
        if ($existing.Count -gt 0) {
            Write-Log "[SKIP] Already unpacked to $destDir ($($existing.Count) files)"
            return
        }
    }
    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    Write-Log "[UNPACK] $zipPath -> $destDir"
    try {
        Expand-Archive -Path $zipPath -DestinationPath $destDir -Force
        $count = (Get-ChildItem $destDir -Recurse -File).Count
        Write-Log "[OK] Unpacked $count file(s)"
    } catch {
        Write-Log "[ERROR] Unpack failed: $($_.Exception.Message)"
    }
}

# ─── Unpack nested ZIPs in OGA downloads ───
function Expand-NestedZips($dir) {
    $zips = Get-ChildItem $dir -Filter "*.zip" -File
    foreach ($z in $zips) {
        $subDir = Join-Path $dir $z.BaseName
        if (-not (Test-Path $subDir)) {
            Write-Log "[UNPACK-NESTED] $($z.Name)"
            Expand-Archive -Path $z.FullName -DestinationPath $subDir -Force
        }
    }
}

# ═══════════════════════════════════════════════════════════════
#  MAIN
# ═══════════════════════════════════════════════════════════════

"" | Set-Content $LogFile
"# Manual Downloads Needed`n" | Set-Content $ManualFile
Add-Content $ManualFile "Assets that could not be auto-downloaded. Download manually and extract to the listed folder.`n"

Write-Log "=== BrickBlast Asset Fetch Started ==="
Write-Log "Download dir: $DownloadDir"
Write-Log "Unpack dir:   $UnpackDir"

$manifest = Get-Content $ManifestFile -Raw | ConvertFrom-Json

foreach ($src in $manifest.sources) {
    Write-Log ""
    Write-Log "--- Processing: $($src.name) (priority $($src.priority)) ---"

    $result = $null

    switch ($src.downloader_type) {
        "kenney-page-scrape" {
            $result = Get-KenneyAsset $src
            if ($result -and $result.EndsWith(".zip")) {
                $unpackDest = Join-Path $UnpackDir $src.id
                Expand-Asset $result $unpackDest
            }
        }
        "opengameart-page-scrape" {
            $result = Get-OpenGameArtAsset $src
            if ($result) {
                # Copy/move to unpack dir
                $unpackDest = Join-Path $UnpackDir $src.id
                if (-not (Test-Path $unpackDest)) {
                    Copy-Item $result $unpackDest -Recurse -Force
                }
                Expand-NestedZips $unpackDest
            }
        }
        "manual" {
            Write-Manual $src "Source requires interactive download (itch.io name-your-price gate)"
        }
        default {
            Write-Manual $source "Unknown downloader type: $($src.downloader_type)"
        }
    }
}

Write-Log ""
Write-Log "=== Fetch Complete ==="
Write-Log "Check $ManualFile for assets requiring manual download."
Write-Host "`nDone. See ExternalAssets\fetch_log.txt for details." -ForegroundColor Green
