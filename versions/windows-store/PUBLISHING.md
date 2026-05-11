# Publishing Guide — Windows Store (MSIX / Partner Center)

**Target:** Microsoft Store — Windows 10/11 (x64 + ARM64)  
**Source:** `Form1.vb` + `anime finder.vbproj` at project root  
**Package format:** MSIX bundle (`BrickBlast.msixbundle`)

---

## Step 1 — Rebuild Binaries (if code changed)

```powershell
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "versions\windows"
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -o "versions\windows-arm64"
```

Then re-package the MSIX from the project root using `BUILD_MSIX.bat` (or the packaging steps in `msix\`).

---

## Step 2 — Update Publisher Identity (CRITICAL before first submission)

1. Sign in to https://partner.microsoft.com/dashboard
2. Open your **Brick Blast** app → **Product identity** page
3. Copy the **Package/Identity/Publisher** value (e.g. `CN=XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`)
4. In `msix\Package.appxmanifest`, set `Publisher="<your value>"`
5. Rebuild the MSIX

---

## Step 3 — Test Sideload

1. Double-click `BrickBlast.pfx` → install to **Local Machine → Trusted Root Certification Authorities**
2. Double-click `BrickBlast-x64.msix` → click **Install**
3. Verify game launches, Store/music/power-ups all work, save persists
4. Repeat on ARM64 hardware if available

---

## Step 4 — Submit to Microsoft Partner Center

1. Go to https://partner.microsoft.com/dashboard
2. **Apps and games → New product → App** → Name: **Brick Blast** → Reserve
3. Start a new submission:

### Packages
- Upload `BrickBlast.msixbundle` (covers x64 + ARM64)
- Partner Center will sign with Microsoft cert — your `.pfx` is not used

### Store listing
| Field | Value |
|-------|-------|
| App name | Brick Blast |
| Short description | Classic brick-breaking arcade — smash bricks, grab power-ups, top the leaderboard! |
| Description | Brick Blast is a fast-paced arcade game. Destroy colorful rows of bricks with a bouncing ball, collect power-ups (multi-ball, fire ball, slow-mo, paddle grow/shrink, extra life), and compete for the top score. Features a full skins Store with 13 ball styles, 10 brick palettes, 8 paddle skins, 16 bonus packs, 10 chiptune music tracks, 5 SFX packs, daily challenge, endless mode, stats tracking, and full gamepad support. No ads. No internet required. |
| Category | Games > Action & adventure |
| Age rating | ESRB: Everyone |
| Keywords | brick breaker, arcade, breakout, retro, chiptune |
| Privacy policy | Not required (no personal data collected) |

### Screenshots
- At least 1 screenshot at 1366×768 or larger (PNG/JPEG)
- Recommended: 3–5 showing gameplay, Store screen, power-ups, leaderboard
- 1920×1080 is ideal

### Pricing
- **Free** (no in-app purchases)

### 5. Submit
- Complete all required checklist items → **"Submit to the Store"**
- Review: typically **1–3 business days**

---

## Key Files

| File | Purpose |
|------|---------|
| `BrickBlast.msixbundle` | Partner Center upload artifact |
| `BrickBlast-x64.msix` | Sideload test on x64 machines |
| `BrickBlast-arm64.msix` | Sideload test on ARM64 machines |
| `BrickBlast.pfx` | Self-signed cert for sideload testing only |

---

## Rebuild MSIX

```powershell
cd "..\..\Final Version Releases\windows-store"
.\Build-MSIX.ps1
```
