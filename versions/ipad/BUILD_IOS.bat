@echo off
title Brick Blast - iPad Native Build
echo ============================================
echo   BRICK BLAST - iPad Native Build
echo   Team Fast Talk
echo ============================================
echo.
echo This folder contains a complete Xcode project
echo for building a NATIVE iPad app (.ipa).
echo.
echo REQUIREMENTS:
echo   - A Mac computer with Xcode 15+ installed
echo   - Apple Developer account (free for testing)
echo   - CocoaPods (installed via: sudo gem install cocoapods)
echo.
echo HOW TO BUILD:
echo   1. Copy the "xcode-project" folder to a Mac
echo   2. Open Terminal in xcode-project/App/
echo   3. Run: pod install
echo   4. Open App.xcworkspace in Xcode
echo   5. Select your iPad as target device
echo   6. Click the Play button to build and install
echo.
echo OR run BUILD_IOS.sh on a Mac for automated build.
echo.
echo QUICK TEST (on this PC):
echo   Opening index.html in browser for preview...
cd /d "%~dp0"
start "" "index.html"
pause
