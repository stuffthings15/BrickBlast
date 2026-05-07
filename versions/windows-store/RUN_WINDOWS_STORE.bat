@echo off
title Brick Blast - Windows Store Packages
echo ============================================
echo   BRICK BLAST - Windows Store Version
echo   Team Fast Talk
echo ============================================
echo.
echo This folder contains Microsoft Store submission packages.
echo.
echo   BrickBlast.msixbundle   - Upload to Partner Center (Store submission)
echo   BrickBlast-x64.msix     - Sideload on x64 Windows 10/11
echo   BrickBlast-arm64.msix   - Sideload on ARM64 Windows
echo   BrickBlast.pfx          - Dev certificate (sideload only)
echo.
echo INSTALL VIA SIDELOAD (no Store needed):
echo   1. Double-click BrickBlast.pfx - install certificate to "Local Machine - Trusted Root"
echo   2. Double-click BrickBlast-x64.msix (or arm64 for ARM devices)
echo   3. Click Install
echo.
echo STORE SUBMISSION:
echo   See README.md for full Partner Center submission guide.
echo.
cd /d "%~dp0"
start "" "."
pause
