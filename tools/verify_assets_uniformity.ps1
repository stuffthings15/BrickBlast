# Verifies that all target build asset folders match the canonical WPF Assets folder
$ErrorActionPreference = "Stop"

$Root = Split-Path $PSScriptRoot -Parent
$Canonical = Join-Path $Root "anime finder wpf\Assets"

$Targets = @(
    "versions\windows-wpf\Assets",
    "anime finder macos\Assets",
    "versions\windows\Assets",
    "versions\html\assets",
    "versions\android-phone\assets",
    "versions\android-tablet\assets",
    "versions\ipad\assets",
    "versions\iphone\assets",
    "versions\macos\assets",
    "web\assets",
    "mobile\www\assets"
)

function Get-AssetList($baseDir) {
    if (-not (Test-Path $baseDir)) { return @() }
    return Get-ChildItem $baseDir -Recurse -File -Filter "*.png" |
        ForEach-Object { $_.FullName.Replace($baseDir, "").TrimStart('\\').Replace('\\','/') } |
        Sort-Object
}

$canonList = Get-AssetList $Canonical
$canonCount = $canonList.Count
Write-Host "Canonical: anime finder wpf/Assets ($canonCount PNG files)"

$allOk = $true
foreach ($rel in $Targets) {
    $target = Join-Path $Root $rel
    $list = Get-AssetList $target
    $count = $list.Count

    $missing = $canonList | Where-Object { $_ -notin $list }
    $extra = $list | Where-Object { $_ -notin $canonList }

    if ($missing.Count -eq 0 -and $extra.Count -eq 0 -and $count -eq $canonCount) {
        Write-Host "[OK]   $rel ($count PNG)"
    } else {
        $allOk = $false
        Write-Host "[DIFF] $rel ($count PNG)"
        if ($missing.Count -gt 0) { Write-Host "  Missing: $($missing.Count)" }
        if ($extra.Count -gt 0)   { Write-Host "  Extra:   $($extra.Count)" }
    }
}

if ($allOk) {
    Write-Host "\nUniformity check PASSED."
    exit 0
}

Write-Host "\nUniformity check FAILED."
exit 1
