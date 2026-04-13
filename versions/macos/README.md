# Brick Blast — macOS Desktop Version

Cross-platform desktop port using **Avalonia UI** (WPF-compatible DrawingContext API).

## Requirements
- .NET 10 SDK — install via [dotnet.microsoft.com](https://dotnet.microsoft.com/download) or Homebrew:
  ```bash
  brew install --cask dotnet-sdk
  ```
- macOS 12 Monterey or later
- Apple Silicon (arm64) or Intel (x64)

## Build & Run

```bash
cd "anime finder macos"
dotnet restore
dotnet run
```

## Publish standalone app bundle

```bash
# Apple Silicon M1/M2/M3
dotnet publish -r osx-arm64 -c Release --self-contained true

# Intel Mac
dotnet publish -r osx-x64 -c Release --self-contained true
```

Output lands in `bin/Release/net10.0/osx-arm64/publish/`.

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
