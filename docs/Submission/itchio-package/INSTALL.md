# BrickBlast: Velocity Market — itch.io Submission Package

## Contents

| File | Purpose |
|------|---------|
| `icon.png` | 256×256 game icon for itch.io page |
| `StoreListingCopy.md` | Short / medium / long description copy |
| `INSTALL.md` | Player installation guide |
| `BrickBlast_v1.0.0_windows.zip` | Self-contained build (generate from publish output — see below) |

---

## How to generate the build ZIP

```powershell
# From project root:
dotnet publish "anime finder.vbproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o bin\publish\windows
Compress-Archive -Path bin\publish\windows\* -DestinationPath "Docs\Submission\itchio-package\BrickBlast_v1.0.0_windows.zip" -Force
```

## itch.io Upload Steps

1. Log in at https://itch.io/dashboard
2. Create New Project → **BrickBlast: Velocity Market**
3. Kind: **Downloadable**
4. Classification: **Game**
5. Upload `BrickBlast_v1.0.0_windows.zip` → Platform: **Windows**
6. Paste short description from `StoreListingCopy.md`
7. Upload `icon.png` as Cover Image
8. Upload screenshots from `Docs/Screenshots/SS-*.png`
9. Upload trailer `Docs/Trailer/BrickBlast_Trailer_v1.mp4` (when recorded)
10. Set price: Free
11. Publish → copy public URL → paste into `FinalSubmissionChecklist.md` item 5.7

## Public URL placeholder

> **TODO:** Replace this line with the live itch.io URL after publishing  
> Example: `https://stuffthings15.itch.io/brickblast`
