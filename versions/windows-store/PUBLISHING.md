# Publishing Guide — Windows Store (versions folder)

**Target:** Microsoft Store — Windows 10/11  
**This folder contains the MSIX package for Store submission.**

---

## Quick Reference

For full instructions, see `../../Final Version Releases/windows-store/PUBLISHING.md`.

---

## Files in This Folder

| File | Purpose |
|------|---------|
| `BrickBlast.msix` | Packaged MSIX installer |
| `AppxManifest.xml` | Store app manifest |
| `payload/` | Published app files inside the MSIX |

---

## Submit to Microsoft Store

1. Go to https://partner.microsoft.com/dashboard
2. Create or open the **Brick Blast** submission
3. Upload `BrickBlast.msix` under **Packages**
4. Fill listing from `../../docs/Submission/StoreListingCopy.md`
5. Submit — review takes 1–5 business days

---

## Rebuild MSIX

```powershell
cd "..\..\Final Version Releases\windows-store"
.\Build-MSIX.ps1
```
