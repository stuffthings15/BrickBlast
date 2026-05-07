# Brick Blast — Windows Store Version

## Requirements
- Windows 10 version 1903 or later (x64 or ARM64)
- No additional software needed — MSIX is self-contained

## Contents
| File | Purpose |
|------|---------|
| `BrickBlast.msixbundle` | **Upload to Partner Center** — covers x64 + ARM64 |
| `BrickBlast-x64.msix` | Standalone x64 (sideload / direct install) |
| `BrickBlast-arm64.msix` | Standalone ARM64 (sideload / direct install) |
| `BrickBlast.pfx` | Self-signed cert (dev/sideload only) |
| `RUN_WINDOWS_STORE.bat` | Launcher / install helper |
| `README.md` | This file |

## Install (Sideload — No Store Needed)
1. Double-click `BrickBlast.pfx` → install to **Local Machine → Trusted Root**
2. Double-click `BrickBlast-x64.msix` (or `arm64` on ARM devices)
3. Click **Install**

## Controls
| Input | Action |
|-------|--------|
| ← → / A D | Move paddle |
| SPACE | Start / Resume / Speed boost (2×) |
| P / ESC | Pause |
| F | Speed boost (2×) |
| H / O | Options menu |
| Mouse click | Speed up ball (during gameplay) |
| Gamepad | Full controller support |

---

## Microsoft Store Submission Guide

## Artifacts
| File | Purpose |
|------|---------|
| `BrickBlast.msixbundle` | **Upload this to Partner Center** — covers x64 + ARM64 |
| `BrickBlast-x64.msix` | Standalone x64 (sideload / direct install only) |
| `BrickBlast-arm64.msix` | Standalone ARM64 (sideload / direct install only) |
| `BrickBlast.pfx` | Self-signed cert (dev/sideload only — Partner Center replaces this) |

## Step-by-Step: Submit to Microsoft Store

### 1. Create a Microsoft Partner Center account
- Go to https://partner.microsoft.com/dashboard
- Sign in with your Microsoft account → click **"Open a developer account"**
- Individual account: one-time $19 USD registration fee

### 2. Create a new app reservation
- Dashboard → **"Apps and games"** → **"New product"** → **"App"**
- Enter name: **Brick Blast**
- Click **"Reserve product name"**

### 3. Update the Publisher identity in Package.appxmanifest
> ⚠️ CRITICAL: Before your final build, replace the placeholder Publisher CN
> with the exact value from Partner Center.
- In Partner Center → your app → **"Product identity"** page
- Copy **"Package/Identity/Publisher"** value (e.g. `CN=XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`)
- In `msix\Package.appxmanifest`, replace `CN=BrickBlast` with that value
- Rebuild: run `BUILD_MSIX.bat` from the repo root (see below)

### 4. Upload the package
- In Partner Center → **"Packages"** section of your submission
- Drag and drop `BrickBlast.msixbundle`
- Partner Center will sign it with Microsoft's certificate — your .pfx is NOT used

### 5. Fill in store listing
| Field | Value |
|-------|-------|
| App name | Brick Blast |
| Short description | Classic brick-breaking arcade action — smash bricks, grab power-ups, top the leaderboard! |
| Description | Brick Blast is a fast-paced arcade game inspired by classic brick-breakers. Destroy rows of colorful bricks with a bouncing ball, collect power-ups like multi-ball, fire ball, and paddle extender, and compete for the highest score. Features multiple music tracks, smooth 60fps gameplay, and full keyboard/gamepad support. |
| Category | Games > Action & adventure |
| Age rating | ESRB: Everyone (no violence, no adult content) |
| Keywords | brick, breaker, arcade, breakout, ball, paddle, classic, retro |
| Privacy policy | Not required if no personal data collected |

### 6. Screenshots required
- At least **1 screenshot** at 1366×768 or larger (PNG or JPEG)
- Recommended: 3-5 screenshots showing gameplay, power-ups, leaderboard
- Desktop screenshot size: 1920×1080 recommended

### 7. Pricing
- Set to **Free** on the Pricing and availability page

### 8. Submit
- Complete all required fields → click **"Submit to the Store"**
- Review typically takes 1-3 business days

## Rebuild Script
Run from the repo root to regenerate the MSIX after any code change:

```bat
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o versions\windows
dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -o versions\windows-arm64
```
Then re-run the MSIX packaging steps in this folder.
