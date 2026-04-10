# Brick Blast — Android Tablet Version

This folder is fully self-contained. Zip it and share.

## Contents
- `BrickBlast-Android.apk` — Native Android app (same APK as phone, auto-scales)
- `index.html` — PWA version (host on web server, install via Chrome)
- `manifest.json` — PWA manifest
- `RUN_ANDROID_TABLET.bat` — Info launcher
- `README.md` — This file

## Option 1: Install APK (Easiest)
1. Transfer `BrickBlast-Android.apk` to your Android tablet
2. Open the file to install
3. Enable **"Unknown Sources"** in Settings if prompted
4. Launch from app drawer — game auto-scales to tablet screen

## Option 2: PWA Install (No APK)
1. Host `index.html` + `manifest.json` on any HTTPS web server
2. Open the URL on your tablet in **Chrome**
3. Tap **⋮ menu → "Add to Home Screen"**

## Requirements
- Android 5.0+ (API 21)
- Landscape orientation recommended
