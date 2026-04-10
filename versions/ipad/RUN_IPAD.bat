@echo off
title Brick Blast - iPad Version
echo ============================================
echo   BRICK BLAST - iPad Version
echo   Team Fast Talk
echo ============================================
echo.
echo INSTALL AS PWA (No Mac or Xcode required!)
echo.
echo Steps:
echo   1. Host web/index.html on GitHub Pages or any web server
echo   2. Open the URL on your iPad in Safari
echo   3. Tap the Share button (box with arrow)
echo   4. Tap "Add to Home Screen"
echo   5. Tap "Add" - the game appears as an app icon
echo   6. Launch from home screen for full-screen play!
echo.
echo The iPad's larger screen gives the best touch experience.
echo Bluetooth keyboard and gamepad fully supported.
echo.
echo Opening the HTML source for reference...
start "" "%~dp0..\..\web\index.html"
pause
