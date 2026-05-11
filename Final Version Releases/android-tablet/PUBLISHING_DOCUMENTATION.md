# Publishing Documentation — Brick Blast

**Team Fast Talk**  
**Project:** Brick Blast (BrickBlast: Velocity Market)  
**Canonical source:** `anime finder.vbproj` (WinForms, .NET 10, runs with F5)

---

## Platform Matrix

| Platform | Folder | Technology | Status |
|----------|--------|------------|--------|
| Windows x64 | `versions/windows/` | Native WinForms .NET 10 | ✅ Binary present |
| Windows ARM64 | `versions/windows-arm64/` | Native WinForms .NET 10 | ✅ Binary present |
| Windows Store | `versions/windows-store/` | MSIX bundle (x64 + ARM64) | ✅ Bundle present |
| Windows WPF | `versions/windows-wpf/` | WPF sub-project .NET 10 | ✅ Binary present |
| macOS x64 | `versions/macos/` | Avalonia .NET 10 native | ✅ Binary present |
| macOS ARM64 | `versions/macos-arm64/` | Avalonia .NET 10 native | ✅ Binary present |
| Linux x64 | `versions/linux/` | Avalonia .NET 10 native | ✅ Binary present |
| Android Phone | `versions/android-phone/` | Capacitor 6 + canonical HTML | ✅ APK/AAB present |
| Android Tablet | `versions/android-tablet/` | Capacitor 6 + canonical HTML | ✅ APK/AAB present |
| iPad | `versions/ipad/` | Capacitor 6 + iOS native | 🔒 Build on macOS |
| iPhone | `versions/iphone/` | Capacitor 6 + iOS native | 🔒 Build on macOS |
| HTML / PWA | `versions/html/` | Single-file HTML5/Canvas | ✅ index.html present |

> **Rule:** HTML is ONLY the delivery format for `versions/html/`. Every other release must be a native binary or a proper native-container package (Capacitor-wrapped native app for mobile).

---

## Canonical Source of Truth

The **F5 startup project** (`anime finder.vbproj`) is the canonical gameplay source.  
It contains:
- Full store economy (`StoreSaveData`, `StoreItem`, `InitStoreItems`)
- 10 chiptune music tracks (MP3 under `Assets/Audio/`)
- 5 SFX packs
- 13 ball skins, 10 brick palettes, 16 bonus packs, 8 paddle skins
- Power-up system (grow/shrink, extra life, multi-ball, wide paddle, slow/fast)
- Daily Challenge (seeded to today's date)
- Endless Mode
- Combo system
- Persistent save/load

All platform ports must trace their feature set to this source.

---

## Quick Rebuild Reference

### Windows x64 (Canonical)
```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -o "versions\windows"
```

### Windows ARM64 (Canonical)
```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true -o "versions\windows-arm64"
```

### Windows Store (MSIX)
See `versions/windows-store/PUBLISHING.md` — requires MSIX Packaging Tool or Visual Studio packaging.

### Windows WPF (Sub-project)
```powershell
dotnet publish "anime finder wpf\anime finder wpf.csproj" -c Release -r win-x64 --self-contained true -o "versions\windows-wpf"
```

### macOS x64 (Avalonia sub-project)
```powershell
dotnet publish "anime finder macos\anime finder macos.csproj" -c Release -r osx-x64 --self-contained true -o "versions\macos"
```

### macOS ARM64 (Avalonia sub-project)
```powershell
dotnet publish "anime finder macos\anime finder macos.csproj" -c Release -r osx-arm64 --self-contained true -o "versions\macos-arm64"
```

### Linux x64 (Avalonia sub-project)
```powershell
dotnet publish "anime finder macos\anime finder macos.csproj" -c Release -r linux-x64 --self-contained true -o "versions\linux\bin"
```

### Android Phone / Tablet (Capacitor)
```powershell
# 1. Sync canonical HTML source
Copy-Item "web\index.html" "mobile\www\index.html" -Force
Copy-Item "web\manifest.json" "mobile\www\manifest.json" -Force
```
```bash
cd mobile && npm install && npx cap sync android
cd android && ./gradlew bundleRelease assembleRelease
```

### iPad / iPhone (Capacitor — macOS only)
```bash
cp web/index.html mobile/www/index.html
cd mobile && npm install && npx cap sync ios
cd ios/App && pod install
# Then archive in Xcode or via xcodebuild
```

### HTML / PWA
```powershell
Copy-Item "web\index.html" "versions\html\index.html" -Force
Copy-Item "web\manifest.json" "versions\html\manifest.json" -Force
```

---

## Distribution Channels

| Channel | Platforms | Notes |
|---------|-----------|-------|
| [itch.io](https://itch.io) | Windows, Linux, macOS, HTML | Upload zips per platform |
| [GitHub Releases](https://github.com/stuffthings15/BrickBlast) | All | Attach zips + APK to release tag |
| [Google Play Console](https://play.google.com/console) | Android | Upload `BrickBlast-release.aab` |
| [Apple App Store Connect](https://appstoreconnect.apple.com) | iPad, iPhone | Upload IPA via Xcode Organizer |
| [Microsoft Partner Center](https://partner.microsoft.com) | Windows Store | Upload `BrickBlast.msixbundle` |

---

## Pre-Release Checklist (All Platforms)

- [ ] Store, music, SFX all work
- [ ] All power-ups function correctly
- [ ] Daily Challenge generates a unique level per day
- [ ] Endless Mode runs without crashing
- [ ] Combo system awards multiplied score
- [ ] Save data persists between sessions
- [ ] Purchased skins visibly affect gameplay
- [ ] Colorblind mode works
- [ ] Speed boost (2×) toggle works
- [ ] Credits and Stats screens accessible
- [ ] Gamepad/controller input works
- [ ] No HTML files in non-HTML platform releases

---

## Submission Checklist Reference

See `Docs/Submission/FinalSubmissionChecklist.md` for the complete academic submission checklist
including all evidence columns and definition-of-done criteria.

---

## Known Limitations

| Item | Detail |
|------|--------|
| iPad/iPhone build | Requires macOS 13+ with Xcode 15+ — cannot build from Windows |
| WPF build | Separate sub-project; may lag behind canonical feature set |
| Android APK signing | Requires `mobile/brickblast-release.keystore` — keep the keystore private |
| macOS Gatekeeper | Binary is unsigned; right-click → Open on first run |
| Linux ARM64 | Not currently targeted — x64 only |
