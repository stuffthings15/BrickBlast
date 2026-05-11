# Publishing Documentation — Windows Store (MSIX)
**BrickBlast: Velocity Market — Team Fast Talk**

## Artifacts
| File | Purpose |
|------|---------|
| `BrickBlast.msixbundle` | Universal bundle for Store submission (contains x64 + ARM64) |
| `BrickBlast-x64.msix` | x64-only MSIX for sideloading or direct distribution |
| `BrickBlast-arm64.msix` | ARM64-only MSIX for sideloading |

## Publishing to Microsoft Store

### Prerequisites
- Microsoft Partner Center account: https://partner.microsoft.com/dashboard
- App registered in Partner Center with package identity matching `AppxManifest.xml`
- Valid code-signing certificate (EV cert required for Store)

### Steps
1. **Sign the bundle** (required before Store upload):
   ```powershell
   signtool sign /fd SHA256 /a /f YourCert.pfx /p YourPassword "BrickBlast.msixbundle"
   ```
2. **Upload to Partner Center**:
   - Go to Partner Center → Your App → Submissions → New Submission
   - Under "Packages", drag and drop `BrickBlast.msixbundle`
   - Complete store listing, screenshots, pricing
   - Submit for certification

### Sideloading (Development / Testing)
```powershell
# Enable Developer Mode first (Settings → For developers)
Add-AppxPackage -Path "BrickBlast-x64.msix"
```

### Re-packaging from Source
See `Final Version Releases\windows-store\Build-MSIX.ps1` and `SIGNING_GUIDE.md`.

## Version: v1.2.0
