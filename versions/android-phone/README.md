# Brick Blast — Android Phone Version

This folder is fully self-contained. Zip it and share.

## Contents
- `BrickBlast-Android.apk` — Native Android app (install directly)
- `index.html` — PWA version (host on web server, install via Chrome)
- `manifest.json` — PWA manifest
- `RUN_ANDROID_PHONE.bat` — Info launcher
- `README.md` — This file

## Option 1: Install APK (Easiest)
1. Transfer `BrickBlast-Android.apk` to your Android phone
2. Open the file on your phone to install
3. Enable **"Unknown Sources"** in Settings if prompted
4. Launch from app drawer

## Option 2: PWA Install (No APK)
1. Host `index.html` + `manifest.json` on any HTTPS web server
2. Open the URL on your phone in **Chrome**
3. Tap **⋮ menu → "Add to Home Screen"**
4. Game installs as a full-screen app

## Requirements
- Android 5.0+ (API 21)
- No additional software needed
