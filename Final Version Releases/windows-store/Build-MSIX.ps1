# Build-MSIX.ps1 — Run this on a Windows PC with the Windows SDK installed.
# Requires: makeappx.exe and signtool.exe from the Windows 10/11 SDK
# Place this script in:  Final Version Releases\windows-store\
# Run from PowerShell (Administrator):  .\Build-MSIX.ps1

$ErrorActionPreference = "Stop"
$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$outMsix    = Join-Path $scriptDir "BrickBlast.msix"

# Locate makeappx (SDK 10)
$sdkBin = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\10.0.22621.0\x64"
if (-not (Test-Path $sdkBin)) {
    $sdkBin = (Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Directory |
               Sort-Object Name -Descending | Select-Object -First 1).FullName + "\x64"
}
$makeAppx = Join-Path $sdkBin "makeappx.exe"
if (-not (Test-Path $makeAppx)) { throw "makeappx.exe not found. Install the Windows 10/11 SDK." }

Write-Host "Packing MSIX..." -ForegroundColor Cyan
& $makeAppx pack /d $scriptDir /p $outMsix /nv /o
Write-Host "MSIX created: $outMsix" -ForegroundColor Green

Write-Host ""
Write-Host "Next steps for Windows Store submission:" -ForegroundColor Yellow
Write-Host "  1. Create a Partner Center account at https://partner.microsoft.com"
Write-Host "  2. Reserve app name 'BrickBlast: Velocity Market'"
Write-Host "  3. Sign the MSIX with a trusted cert (or use Store signing)"
Write-Host "  4. Upload the .msix in Partner Center > Packages"
