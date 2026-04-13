$ErrorActionPreference = "Stop"

$root = Split-Path $PSScriptRoot -Parent
$mainWpfProj = Join-Path $root "anime finder wpf\anime finder wpf.vbproj"
$winWpfOut = Join-Path $root "versions\windows-wpf"

Write-Host "=== Push Main App -> Version Folders ==="
Write-Host "Root: $root"

# 1) Ensure canonical assets are integrated, then mirrored to all build folders
Write-Host "[1/3] Integrating + mirroring assets..."
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root "tools\integrate_all_assets.ps1")

Write-Host "[2/3] Verifying asset uniformity..."
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $root "tools\verify_assets_uniformity.ps1")

# 3) Publish Windows WPF build from MAIN project into versions/windows-wpf
Write-Host "[3/3] Publishing MAIN WPF app to versions/windows-wpf..."
if (-not (Test-Path $mainWpfProj)) {
    throw "Main WPF project not found: $mainWpfProj"
}

dotnet publish $mainWpfProj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o $winWpfOut

if (Test-Path (Join-Path $winWpfOut "BrickBlast.exe")) {
    Write-Host "SUCCESS: versions/windows-wpf/BrickBlast.exe updated from MAIN project."
} else {
    throw "Publish did not produce BrickBlast.exe"
}

Write-Host "Done. All version asset folders are synced and windows-wpf binary is refreshed."
