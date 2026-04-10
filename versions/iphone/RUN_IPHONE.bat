@echo off
title Brick Blast - iPhone Version
echo ============================================
echo   BRICK BLAST - iPhone (iOS) Version
echo   Team Fast Talk
echo ============================================
echo.
echo This folder has TWO install options:
echo.
echo   OPTION 1: NATIVE APP (Xcode)
echo     Run BUILD_IOS.bat for instructions, or
echo     copy xcode-project/ to a Mac and build in Xcode.
echo.
echo   OPTION 2: PWA (no Mac needed)
echo     Host index.html on an HTTPS server,
echo     open in Safari, tap Share, Add to Home Screen.
echo.
echo Opening index.html for preview...
cd /d "%~dp0"
start "" "index.html"
pause
