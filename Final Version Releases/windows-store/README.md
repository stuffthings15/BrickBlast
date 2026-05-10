# BrickBlast — Windows Store (MSIX) Release

Packaged MSIX bundle for the Microsoft Store and enterprise sideloading.

## Folder Structure
| Path | Purpose |
|------|---------|
| `payload/` | Published self-contained x64 binaries (the app payload) |
| `Assets/` | Store tile logos and splash screens (required by the Store) |
| `Package.appxmanifest` | MSIX package identity, capabilities, and visual assets |
| `Build-MSIX.ps1` | PowerShell script to assemble and sign the MSIX |
| `SIGNING_GUIDE.md` | Step-by-step code signing and Store submission guide |

## Quick Build (Local Test)

```powershell
# 1. Create a self-signed test certificate
New-SelfSignedCertificate -Type Custom -Subject "CN=BrickBlast" `
    -KeyUsage DigitalSignature -FriendlyName "BrickBlast Dev" `
    -CertStoreLocation "Cert:\CurrentUser\My"

# 2. Run the packaging script
.\Build-MSIX.ps1

# 3. Install for local testing
Add-AppxPackage .\BrickBlast.msix
```

See `SIGNING_GUIDE.md` for production signing with an EV certificate and Partner Center submission.

## Store Submission Checklist
- [ ] Package identity name matches Partner Center reservation
- [ ] Publisher CN matches your Dev Center publisher display name
- [ ] All `Assets/` images meet Microsoft's required sizes (see manifest)
- [ ] Age rating questionnaire completed in Partner Center
- [ ] Privacy policy URL set in the Store listing
- [ ] Build passes `signtool verify /pa /v BrickBlast.msix`

## System Requirements (end user)
| Requirement | Value |
|-------------|-------|
| OS | Windows 10 version 1903+ / Windows 11 |
| Architecture | x64 (ARM64 variant ships separately) |
| .NET | Bundled inside MSIX — no separate install |

## Reference
- [MSIX packaging overview (Microsoft Learn)](https://learn.microsoft.com/windows/msix/)
- [Partner Center submission guide](https://learn.microsoft.com/windows/apps/publish/)
