# Brick Blast — macOS Desktop Version (Intel x64)

This folder is fully self-contained. Zip it and share — works on any Intel Mac.

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
| `anime finder macos` | Native macOS executable (Intel x64) |
| `anime finder macos.pdb` | Debug symbols (optional — can delete) |
| `libAvaloniaNative.dylib` | Avalonia UI native renderer |
| `libHarfBuzzSharp.dylib` | Text shaping library |
| `libSkiaSharp.dylib` | 2D graphics library |
| `RUN_MACOS.sh` | Launch script |
| `README.md` | This file |

## Requirements
- macOS 12 Monterey or later
- Intel (x64) Mac
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
- **Architecture:** x64 (Intel only)
- **Framework:** .NET 10 (self-contained)
- **For Apple Silicon:** Use the `macos-arm64/` folder instead

## Rebuild from Source
```bash
cd "anime finder macos"
dotnet publish -r osx-x64 -c Release --self-contained true
```

## Features
- 🧱 **Brick layouts** — 8 patterns that rotate across levels
- 🎨 **Skins store** — 13 ball skins, 8 paddle skins, 10 brick palettes, 16 bonus packs
- 💰 **Coin economy** — earn coins by breaking bricks, spend them in the Store
- ⚡ **Power-ups** — grow ball, extra life, multi-ball, shrink, wide paddle, slow & fast
- 🎵 **Chiptune music** — 10 tracks cycling through 8-bit styles
- 🔊 **5 SFX styles** — Zelda, Mega Man, Tetris, Retro Arcade, and Classic
- 🏆 **High score table** — top-10 persistent leaderboard
- 📊 **Stats screen** — games played, bricks broken, total coins earned
- ♿ **Colorblind mode** — high-contrast palette with shape symbols

## Link Assets (one-time setup)

The macOS project reads from the same `Assets/` folder as the Windows WPF version.
Symlink it so you don't duplicate the 179 PNG files:

```bash
# From the repo root:
ln -sfn "anime finder wpf/Assets" "anime finder macos/Assets"
```

## WPF → Avalonia API Quick Reference

| WPF                                          | Avalonia 11                                       |
|----------------------------------------------|---------------------------------------------------|
| `Inherits FrameworkElement`                  | `Inherits Control`                                |
| `Protected Overrides Sub OnRender(dc)`       | `Public Overrides Sub Render(context)`            |
| `dc.PushOpacity(x)` / `dc.Pop()`            | `Using dc.PushOpacity(x)` (IDisposable)           |
| `dc.PushTransform(t)` / `dc.Pop()`          | `Using dc.PushTransform(t)` (IDisposable)         |
| `dc.DrawRoundedRectangle(b, p, r, rx, ry)`  | `dc.DrawRectangle(b, p, New RoundedRect(r, rx))`  |
| `New LinearGradientBrush(c1, c2, angle)`    | `New LinearGradientBrush With { .StartPoint=..., .EndPoint=..., .GradientStops=... }` |
| `New FormattedText(text, cult, dir, tf, sz, brush)` | Same constructor in Avalonia 11          |
| `BitmapImage` / `BitmapSource`              | `Avalonia.Media.Imaging.Bitmap`                   |
| `DispatcherTimer`                           | `Avalonia.Threading.DispatcherTimer`              |
| `Key.Enter`, `Key.Space`, etc.             | Identical key names                               |
| `winmm.dll` PlaySound / MCI               | `Process.Start("afplay", wavPath)` on macOS       |
| `HwndSource` / `WndProc`                  | Not needed                                        |
| `InvalidateVisual()`                       | `InvalidateVisual()` — identical                  |
| `WindowStyle.None` + `WindowState.Maximized` | `WindowState.FullScreen`                        |

## Porting Checklist

- [x] Project structure, Avalonia bootstrap (`Program.vb`, `App.axaml`)
- [x] Main window with Alt+Enter fullscreen toggle (`MainWindow.axaml.vb`)
- [x] Star field and menu screen rendering (`GameCanvas.vb`)
- [x] Sprite loading from shared `Assets/` folder
- [x] Input handling (keyboard)
- [x] macOS audio via `afplay` subprocess
- [ ] Full game loop — copy `Draw*` / `Update*` from WPF `GameCanvas.vb`, apply table above
- [ ] MIDI music — generate temp WAV then play via `afplay`
- [ ] High score JSON persistence
- [ ] Mouse/trackpad input

## File Structure

```
anime finder macos/
├── anime finder macos.csproj   ← Avalonia project file
├── Program.vb                  ← Entry point
├── App.axaml / App.axaml.vb    ← Application lifecycle
├── MainWindow.axaml / .vb      ← Window + Alt+Enter fullscreen
├── GameCanvas.vb               ← Ported game canvas (Avalonia)
└── Assets/ → (symlink to ../anime finder wpf/Assets)
```
