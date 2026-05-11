# Publishing Guide — macOS Apple Silicon ARM64

**Target:** macOS 11+ (Apple Silicon M1/M2/M3/M4)  
**Package type:** Self-contained .NET 10 + Avalonia native binary  
**Source project:** `anime finder macos/` sub-project (Avalonia cross-platform)

---

## Step 1 — Rebuild the Native Binary

```powershell
# From project root (any OS with .NET 10 SDK)
dotnet publish "anime finder macos\anime finder macos.csproj" `
    -c Release `
    -r osx-arm64 `
    --self-contained true `
    -o "versions\macos-arm64"
```

---

## Step 2 — Copy Game Assets

```powershell
Copy-Item "anime finder macos\Assets" "versions\macos-arm64\Assets" -Recurse -Force
```

---

## Step 3 — Notarize (Optional, for Mac App Store / Gatekeeper)

```bash
# On macOS build machine (Apple Silicon)
codesign --deep --force --sign "Developer ID Application: Your Name" "anime finder macos"
xcrun notarytool submit "anime finder macos" --apple-id you@example.com \
    --team-id XXXXXXXXXX --password @keychain:notary-pass --wait
xcrun stapler staple "anime finder macos"
```

---

## Step 4 — Test

```bash
chmod +x RUN_MACOS_ARM64.sh && ./RUN_MACOS_ARM64.sh
```

### Test Checklist
- [ ] App launches natively on Apple Silicon (no Rosetta translation)
- [ ] Gatekeeper prompt handled (right-click → Open on first run)
- [ ] Music, sound effects, and store all work
- [ ] Power-ups, daily challenge, and endless mode work
- [ ] Controller input works
- [ ] High scores persist between sessions
- [ ] Verify ARM64 architecture: `file "anime finder macos"` should show `arm64`

---

## Step 5 — Distribute

### itch.io
1. Zip the `versions/macos-arm64/` folder
2. Upload to [itch.io](https://itch.io) → Edit Game → Uploads, mark as **macOS**

### GitHub Releases
1. Attach `BrickBlast-macos-arm64.zip` to the GitHub Release

---

## Key Files

| File | Purpose |
|------|---------|
| `anime finder macos` | Native macOS ARM64 executable |
| `libSkiaSharp.dylib` | Skia rendering |
| `libHarfBuzzSharp.dylib` | Text shaping |
| `libAvaloniaNative.dylib` | Avalonia native layer |
| `Assets/` | Game assets |
