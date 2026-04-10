@echo off
title Brick Blast - Android Tablet Build
echo ============================================
echo   BRICK BLAST - Android Tablet Version
echo   Team Fast Talk
echo ============================================
echo.
echo Option 1: Install PWA (Easiest)
echo   - Open web/index.html on your Android tablet's browser
echo   - Tap the browser menu (3 dots) then "Add to Home Screen"
echo   - The game installs as a full-screen app
echo.
echo Option 2: Build Android APK (Requires Android Studio)
echo   Opening the Android project folder...
echo.
start "" "%~dp0..\..\mobile\android"
echo.
echo The same APK works on both phones and tablets.
echo The game auto-scales to any screen size.
echo.
echo To build the APK:
echo   1. Open the "mobile\android" folder in Android Studio
echo   2. Click Build ^> Build Bundle(s) / APK(s) ^> Build APK(s)
echo   3. Transfer the APK to your Android tablet and install
echo.
pause
