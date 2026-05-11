# Publishing Guide — Linux x64

**Target:** Linux x86-64 desktop  
**Package type:** Self-contained .NET 10 + Avalonia native binary  
**Source project:** `anime finder macos/` sub-project (Avalonia cross-platform)

---

## Step 1 — Rebuild the Native Binary

```powershell
# From project root (Windows build machine with .NET 10 SDK)
dotnet publish "anime finder macos\anime finder macos.csproj" `
    -c Release `
    -r linux-x64 `
    --self-contained true `
    -o "versions\linux\bin"
```

---

## Step 2 — Copy Game Assets

```powershell
# Copy audio and graphics assets into the output folder
Copy-Item "anime finder macos\Assets" "versions\linux\assets" -Recurse -Force
```

---

## Step 3 — Test on Linux

Transfer the `versions/linux/` folder to a Linux machine, then:

```bash
chmod +x RUN_LINUX.sh
./RUN_LINUX.sh
```

### Test Checklist
- [ ] App launches without any browser or runtime dependency
- [ ] Music, sound effects, and store all work
- [ ] Power-ups, daily challenge, and endless mode work
- [ ] Controller input works
- [ ] High scores persist between sessions
- [ ] Desktop shortcut launches the app correctly

---

## Step 4 — Distribute

### itch.io
1. Zip the entire `versions/linux/` folder
2. Upload to [itch.io](https://itch.io) → Edit Game → Uploads
3. Mark as **Linux** platform

### GitHub Releases
1. Tag the release: `git tag v1.x.x && git push origin v1.x.x`
2. Attach `BrickBlast-linux-x64.zip` to the GitHub Release

---

## Key Files

| File | Purpose |
|------|---------|
| `bin/anime finder macos` | Native Linux x64 executable |
| `bin/libSkiaSharp.so` | Native rendering library |
| `bin/libHarfBuzzSharp.so` | Text shaping library |
| `assets/` | Game assets (audio, graphics) |
