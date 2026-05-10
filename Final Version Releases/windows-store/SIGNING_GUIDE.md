# Windows Store — Signing & Submission Guide

## Prerequisites
- Windows SDK 10.0.22000.0 or later (for `makeappx.exe` and `signtool.exe`)
- A code-signing certificate (self-signed for testing; Microsoft-trusted for store submission)
- A [Partner Center](https://partner.microsoft.com/en-us/dashboard) developer account

---

## Step 1 — Build the MSIX

```powershell
.\Build-MSIX.ps1
```

This produces `BrickBlast.msix` in this folder.

## Step 2 — Sign locally (test only)

```powershell
# Create a self-signed cert (one-time)
New-SelfSignedCertificate `
    -Type Custom -Subject "CN=BrickBlast" `
    -KeyUsage DigitalSignature -FriendlyName "BrickBlast Dev" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3","2.5.29.19={text}")

# Export password-protected PFX
$cert = (Get-ChildItem Cert:\CurrentUser\My | Where-Object {$_.FriendlyName -eq "BrickBlast Dev"})
Export-PfxCertificate -Cert $cert -FilePath BrickBlast-dev.pfx -Password (ConvertTo-SecureString -String "test1234" -Force -AsPlainText)

# Sign the MSIX
signtool sign /fd SHA256 /a /f BrickBlast-dev.pfx /p test1234 BrickBlast.msix
```

## Step 3 — Install locally (sideload test)

```powershell
Add-AppxPackage -Path BrickBlast.msix
```

## Step 4 — Submit to Partner Center

1. Log in to [Partner Center](https://partner.microsoft.com/dashboard).
2. Create a new application → reserve the name **BrickBlast**.
3. Start a new submission → **Packages** → upload `BrickBlast.msix`.
4. Fill in description, screenshots, age rating, and pricing.
5. Click **Submit to the Store**.

> **Note**: For store submission the certificate must be issued by a trusted CA or you can use Microsoft's automatic signing during submission (recommended).

---

## Package contents

| Path | Purpose |
|------|---------|
| `Package.appxmanifest` | Store identity, capabilities, and entry point |
| `payload/` | Published WinForms application binaries |
| `Assets/` | Required logo/tile PNGs |
| `Build-MSIX.ps1` | Automates `makeappx pack` |
