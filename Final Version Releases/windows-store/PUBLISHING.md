# Publishing Guide — Windows Store (MSIX)

**Target:** Microsoft Store — Windows 10/11  
**Artifact:** `BrickBlast.msix` + `AppxManifest.xml`  
**Status:** ✅ STAGED (MSIX built; Store submission requires Partner Center account)

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast.msix` | Packaged MSIX installer |
| `AppxManifest.xml` | Store manifest (Bundle ID, version, capabilities) |
| `payload/` | Published app files bundled into the MSIX |
| `assets/` | Store tile and splash screen images |
| `Build-MSIX.ps1` | Script to rebuild the MSIX |
| `SIGNING_GUIDE.md` | Code signing instructions |

---

## Step 1 — Rebuild MSIX (if needed)

```powershell
.\Build-MSIX.ps1
```

Requires Windows SDK `makeappx.exe` in PATH. The script publishes the app and packs it.

---

## Step 2 — Sign the MSIX

Microsoft Store submission handles signing automatically when you upload through Partner Center. For sideloading or testing:

```powershell
# Create a self-signed cert (test only)
New-SelfSignedCertificate -Type Custom -Subject "CN=TeamFastTalk" `
  -KeyUsage DigitalSignature -FriendlyName "BrickBlast" `
  -CertStoreLocation "Cert:\CurrentUser\My"

# Sign
signtool sign /fd SHA256 /a BrickBlast.msix
```

See `SIGNING_GUIDE.md` for full details.

---

## Step 3 — Submit to Microsoft Store

1. Go to https://partner.microsoft.com/dashboard
2. Sign in with your Microsoft account
3. **Create a new app** → reserve name **"Brick Blast"**
4. Go to **Submissions** → **New submission**
5. Fill in:
   - **Age rating:** Everyone (IARC)
   - **Category:** Games → Arcade
   - **Pricing:** Free (or set price)
   - **Markets:** All markets (or select)
6. Upload `BrickBlast.msix` under **Packages**
7. Upload screenshots from `docs/Screenshots/`
8. Paste store description from `docs/Submission/StoreListingCopy.md`
9. Upload `Assets/UI/icon.png` as the app icon
10. Submit for certification (review takes 1–5 business days)

---

## Store Listing Quick Reference

| Field | Value |
|-------|-------|
| App name | Brick Blast |
| Bundle ID | `com.teamfasttalk.brickblast` |
| Version | 1.0.0 |
| Category | Games → Arcade |
| Age rating | Everyone |
| Short description | See `docs/Submission/StoreListingCopy.md` |
| Publisher | Team Fast Talk |

---

## Testing Before Submit

- [ ] Install `BrickBlast.msix` via double-click
- [ ] App appears in Start menu as "Brick Blast"
- [ ] Full gameplay loop works from installed version
- [ ] Uninstall cleanly from Settings → Apps
- [ ] No UAC prompts during gameplay

---

## Requirements

- Windows 10 version 1903 or later
- Microsoft Partner Center account (free to create, $19 one-time registration fee for Store)
- Windows SDK (`makeappx.exe`, `signtool.exe`) for rebuilding
