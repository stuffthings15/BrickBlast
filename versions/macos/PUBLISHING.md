# Publishing Guide — macOS Intel x64

**Target:** macOS 11+ (Intel x64)  
**Package type:** Self-contained .NET 10 + Avalonia native binary  
**Source project:** `anime finder macos/` sub-project (Avalonia cross-platform)

---

## Step 1 — Rebuild the Native Binary

```powershell
# From project root (any OS with .NET 10 SDK)
dotnet publish "anime finder macos\anime finder macos.csproj" `
    -c Release `
    -r osx-x64 `
    --self-contained true `
    -o "versions\macos"
```

---

## Step 2 — Copy Game Assets

```powershell
Copy-Item "anime finder macos\Assets" "versions\macos\assets" -Recurse -Force
```

---

## Step 3 — Notarize (Optional, for Mac App Store / Gatekeeper)

For distribution outside the Mac App Store:
```bash
# On macOS build machine
codesign --deep --force --sign "Developer ID Application: Your Name" "anime finder macos"
xcrun notarytool submit "anime finder macos" --apple-id you@example.com \
    --team-id XXXXXXXXXX --password @keychain:notary-pass --wait
xcrun stapler staple "anime finder macos"
```

---

## Step 4 — Test

```bash
chmod +x RUN_MACOS.sh && ./RUN_MACOS.sh
```

### Test Checklist
- [ ] App launches without browser or runtime dependency
- [ ] Gatekeeper prompt handled (right-click → Open on first run)
- [ ] Music, sound effects, and store all work
- [ ] Power-ups, daily challenge, and endless mode work
- [ ] Controller input works
- [ ] High scores persist between sessions

---

## Step 5 — Distribute

### itch.io
1. Zip the `versions/macos/` folder
2. Upload to [itch.io](https://itch.io) → Edit Game → Uploads, mark as **macOS**

### GitHub Releases
1. Attach `BrickBlast-macos-x64.zip` to the GitHub Release

---

## Key Files

| File | Purpose |
|------|---------|
| `anime finder macos` | Native macOS x64 executable |
| `libSkiaSharp.dylib` | Skia rendering |
| `libHarfBuzzSharp.dylib` | Text shaping |
| `libAvaloniaNative.dylib` | Avalonia native layer |
| `assets/` | Game assets |
