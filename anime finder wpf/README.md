# anime finder wpf — WPF Port
**BrickBlast: Velocity Market — Team Fast Talk**

Full port of the WinForms game to Windows Presentation Foundation (WPF).

## Key Differences from WinForms Version
| Feature | WinForms (`Form1.vb`) | WPF (`GameCanvas.vb`) |
|---------|----------------------|----------------------|
| Rendering | `GDI+` via `Graphics` | `DrawingContext` |
| Timer | `System.Windows.Forms.Timer` | `DispatcherTimer` |
| Rounded rects | Manual `GraphicsPath` arcs | `dc.DrawRoundedRectangle()` |
| Gradient text | Manual brush fill | `FormattedText.BuildGeometry()` |
| MCI music | `WndProc` override | `HwndSource.AddHook` |

## How to Run
```
Open: anime finder.slnx in Visual Studio
Set startup project to: anime finder wpf
Press F5
```

## Build Output
`updated versions\windows-wpf\BrickBlast.exe` — self-contained WPF EXE.

## Source
`GameCanvas.vb` — single-file WPF game implementation.
