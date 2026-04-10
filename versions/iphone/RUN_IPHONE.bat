@echo off
title Brick Blast - iPhone Version
echo ============================================
echo   BRICK BLAST - iPhone (iOS) Version
echo   Team Fast Talk
echo ============================================
echo.
echo INSTALL AS PWA (No Mac or Xcode required!)
echo.
echo Steps:
echo   1. Host web/index.html on GitHub Pages or any web server
echo   2. Open the URL on your iPhone in Safari
echo   3. Tap the Share button (box with arrow)
echo   4. Tap "Add to Home Screen"
echo   5. Tap "Add" - the game appears as an app icon
echo   6. Launch from home screen for full-screen play!
echo.
echo The game supports:
echo   - Full touch controls
echo   - Landscape orientation
echo   - Offline play (after first load)
echo   - MFi game controller support
echo.
echo Opening the HTML source for reference...
start "" "%~dp0..\..\web\index.html"
pause
