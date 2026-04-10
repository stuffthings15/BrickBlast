@echo off
title Brick Blast - Android Tablet
echo ============================================
echo   BRICK BLAST - Android Tablet Version
echo   Team Fast Talk
echo ============================================
echo.
echo This folder contains:
echo   - BrickBlast-Android.apk  (install on Android tablet)
echo   - index.html              (PWA alternative)
echo.
echo OPTION 1: Install APK
echo   Transfer BrickBlast-Android.apk to your Android tablet
echo   Open it to install (enable "Unknown Sources" in Settings)
echo.
echo OPTION 2: PWA Install (no APK needed)
echo   Host index.html on any web server
echo   Open in Chrome on your tablet
echo   Tap menu (3 dots) then "Add to Home Screen"
echo.
echo Opening folder...
cd /d "%~dp0"
start "" "."
pause
