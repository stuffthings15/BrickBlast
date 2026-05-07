# Brick Blast — macOS Apple Silicon Version

This folder is fully self-contained. Zip it and share — works on any Apple Silicon Mac.

## How to Run

**Option A — Run directly**
```bash
chmod +x "anime finder macos"
./"anime finder macos"
```

**Option B — Double-click** the `anime finder macos` binary in Finder.

> **First run:** macOS may show a security prompt.
> Go to **System Settings → Privacy & Security → Open Anyway** to allow it.

## Contents
| File | Purpose |
|------|---------|
| `anime finder macos` | Native macOS executable (Apple Silicon arm64) |
| `anime finder macos.pdb` | Debug symbols (optional — can delete) |
| `libAvaloniaNative.dylib` | Avalonia UI native renderer |
| `libHarfBuzzSharp.dylib` | Text shaping library |
| `libSkiaSharp.dylib` | 2D graphics library |
| `Assets/` | Sprites, tiles, UI images used by the game |
| `README.md` | This file |

## Requirements
- macOS 12 Monterey or later
- Apple Silicon Mac (M1 / M2 / M3 / M4)
- No additional software needed — all runtimes are bundled

## Controls
| Input | Action |
|-------|--------|
| ← → / A D | Move paddle |
| SPACE | Start / Resume / Speed boost (2×) |
| P / ESC | Pause |
| F | Speed boost (2×) |
| H / O | Options menu |
| Mouse click | Speed up ball (during gameplay) |

## Platform
- **Type:** Native macOS app (Avalonia UI + VB.NET)
- **Architecture:** arm64 (Apple Silicon only)
- **Framework:** .NET 10 (self-contained)
- **For Intel Mac:** Use the `macos/` folder instead

## Rebuild from Source
```bash
cd "anime finder macos"
dotnet publish -r osx-arm64 -c Release --self-contained true
```
