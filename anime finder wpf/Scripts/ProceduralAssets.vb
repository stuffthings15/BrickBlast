' =============================================================================
' PROCEDURAL ASSET GENERATOR — Brick Blast WPF
' Generates placeholder sprites when external assets are not available.
' Replace with real assets from SuperGameAsset.com for production.
' Pipeline: ProceduralAssets → AssetManager cache → Renderer
' =============================================================================
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Namespace BrickBlastWPF
Public NotInheritable Class ProceduralAssets

    ''' <summary>
    ''' Registers all default procedural assets into the AssetManager.
    ''' Call once at startup so every GetSprite() call has a fallback.
    ''' </summary>
    Public Shared Sub RegisterDefaults(mgr As AssetManager)
        ' ── Character sprites ──
        mgr.RegisterGenerated("Characters/player_idle", GenerateCharacter(32, 32, Color.FromRgb(80, 180, 255), "player"))
        mgr.RegisterGenerated("Characters/player_move", GenerateCharacter(32, 32, Color.FromRgb(100, 200, 255), "player"))

        ' ── Enemy sprites (match EnemyManager spawn keys) ──
        mgr.RegisterGenerated("Characters/enemy_patrol", GenerateEnemy(28, 28, Color.FromRgb(255, 80, 80), "patrol"))
        mgr.RegisterGenerated("Characters/enemy_chase", GenerateEnemy(28, 28, Color.FromRgb(255, 160, 40), "chase"))
        mgr.RegisterGenerated("Characters/enemy_tank", GenerateEnemy(34, 34, Color.FromRgb(180, 60, 220), "tank"))
        mgr.RegisterGenerated("Characters/enemy_fast", GenerateEnemy(24, 24, Color.FromRgb(255, 220, 50), "fast"))
        mgr.RegisterGenerated("Characters/enemy_boss", GenerateEnemy(48, 48, Color.FromRgb(200, 30, 30), "boss"))

        ' ── Tile sprites (50x51 to match TileMap cell size) ──
        For i = 0 To 7
            mgr.RegisterGenerated($"Tiles/tile_{i}", GenerateTile(50, 51, i))
        Next

        ' ── UI icon sprites ──
        Dim icons = {"heart", "star", "shield", "sword", "potion", "gem", "scroll", "key"}
        Dim syms = {ChrW(&H2665), ChrW(&H2605), ChrW(&H25C6), ChrW(&H2694), ChrW(&H2617), ChrW(&H25C8), ChrW(&H2709), ChrW(&H26BF)}
        Dim cols = {Color.FromRgb(255, 80, 100), Color.FromRgb(255, 220, 50),
                    Color.FromRgb(80, 160, 255), Color.FromRgb(200, 200, 220),
                    Color.FromRgb(80, 255, 120), Color.FromRgb(180, 50, 255),
                    Color.FromRgb(255, 200, 150), Color.FromRgb(255, 180, 50)}
        For i = 0 To icons.Length - 1
            mgr.RegisterGenerated($"UI/{icons(i)}", GenerateIcon(32, syms(i), cols(i)))
        Next

        ' ── Power-up icons (fallbacks for SuperGameAsset imports) ──
        mgr.RegisterGenerated("UI/powerup_life", GenerateIcon(48, ChrW(&H2665), Color.FromRgb(255, 60, 60)))
        mgr.RegisterGenerated("UI/powerup_grow", GenerateIcon(48, "+", Color.FromRgb(50, 150, 255)))
        mgr.RegisterGenerated("UI/powerup_multi", GenerateIcon(48, "x3", Color.FromRgb(50, 220, 100)))
        mgr.RegisterGenerated("UI/powerup_shrink", GenerateIcon(48, ChrW(&H2013), Color.FromRgb(255, 220, 50)))
        mgr.RegisterGenerated("UI/powerup_mega", GenerateIcon(48, ChrW(&H25C6), Color.FromRgb(170, 80, 255)))
        mgr.RegisterGenerated("UI/powerup_slow", GenerateIcon(48, ChrW(&H25BC), Color.FromRgb(255, 150, 60)))
        mgr.RegisterGenerated("UI/powerup_fast", GenerateIcon(48, ChrW(&H25B2), Color.FromRgb(255, 120, 200)))

        ' ── Menu mascot placeholder ──
        mgr.RegisterGenerated("Characters/menu_mascot", GenerateCharacter(64, 96, Color.FromRgb(200, 170, 80), "mascot"))

        ' ── Gameplay sprite fallbacks (overridden by OpenGameArt / itch.io imports) ──
        Dim brickColors = {
            Color.FromRgb(255, 60, 80), Color.FromRgb(255, 140, 50),
            Color.FromRgb(255, 220, 50), Color.FromRgb(50, 220, 100),
            Color.FromRgb(50, 180, 255), Color.FromRgb(130, 80, 255),
            Color.FromRgb(255, 80, 200)}
        For i = 0 To 6
            mgr.RegisterGenerated($"Sprites/brick_{i}", GenerateBrickSprite(65, 22, brickColors(i)))
        Next
        mgr.RegisterGenerated("Sprites/paddle", GeneratePaddleSprite(120, 14, Color.FromRgb(80, 180, 255)))
        mgr.RegisterGenerated("Sprites/ball", GenerateBallSprite(16, Colors.White))
    End Sub

    ' ── Character sprite: body + head + eyes + feet ──
    Public Shared Function GenerateCharacter(w As Integer, h As Integer, primary As Color, type As String) As BitmapSource
        Dim dv As New DrawingVisual()
        Using dc = dv.RenderOpen()
            Dim lighter = Color.FromRgb(CByte(Math.Min(255, CInt(primary.R) + 40)),
                                        CByte(Math.Min(255, CInt(primary.G) + 40)),
                                        CByte(Math.Min(255, CInt(primary.B) + 40)))
            Dim darker = Color.FromRgb(CByte(Math.Max(0, CInt(primary.R) - 60)),
                                       CByte(Math.Max(0, CInt(primary.G) - 60)),
                                       CByte(Math.Max(0, CInt(primary.B) - 60)))
            dc.DrawRoundedRectangle(New SolidColorBrush(primary), Nothing,
                New Rect(w * 0.2, h * 0.3, w * 0.6, h * 0.5), 3, 3)
            dc.DrawEllipse(New SolidColorBrush(lighter), Nothing,
                New Point(w / 2.0, h * 0.2), w * 0.18, h * 0.15)
            dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.38, h * 0.18), 2, 2)
            dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.62, h * 0.18), 2, 2)
            dc.DrawRoundedRectangle(New SolidColorBrush(darker), Nothing,
                New Rect(w * 0.2, h * 0.78, w * 0.2, h * 0.18), 2, 2)
            dc.DrawRoundedRectangle(New SolidColorBrush(darker), Nothing,
                New Rect(w * 0.6, h * 0.78, w * 0.2, h * 0.18), 2, 2)
        End Using
        Return Capture(dv, w, h)
    End Function

    ' ── Enemy sprites: shape varies by behavior type ──
    Public Shared Function GenerateEnemy(w As Integer, h As Integer, primary As Color, type As String) As BitmapSource
        Dim dv As New DrawingVisual()
        Using dc = dv.RenderOpen()
            Select Case type
                Case "patrol"
                    dc.DrawEllipse(New SolidColorBrush(primary),
                        New Pen(Brushes.White, 1), New Point(w / 2.0, h / 2.0), w / 2.0 - 1, h / 2.0 - 1)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.35, h * 0.4), 3, 3)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.65, h * 0.4), 3, 3)
                    dc.DrawEllipse(Brushes.Black, Nothing, New Point(w * 0.35, h * 0.4), 1.5, 1.5)
                    dc.DrawEllipse(Brushes.Black, Nothing, New Point(w * 0.65, h * 0.4), 1.5, 1.5)

                Case "chase"
                    Dim geo As New StreamGeometry()
                    Using ctx = geo.Open()
                        ctx.BeginFigure(New Point(w / 2.0, 2), True, True)
                        ctx.LineTo(New Point(w - 2, h - 2), True, False)
                        ctx.LineTo(New Point(2, h - 2), True, False)
                    End Using
                    dc.DrawGeometry(New SolidColorBrush(primary), New Pen(Brushes.White, 1), geo)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.4, h * 0.55), 2, 2)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.6, h * 0.55), 2, 2)

                Case "tank"
                    dc.DrawRoundedRectangle(New SolidColorBrush(primary),
                        New Pen(Brushes.White, 1.5), New Rect(2, 2, w - 4, h - 4), 4, 4)
                    dc.DrawLine(New Pen(New SolidColorBrush(Color.FromArgb(100, 255, 255, 255)), 2),
                        New Point(4, h / 3.0), New Point(w - 4, h / 3.0))
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.35, h * 0.5), 3, 3)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.65, h * 0.5), 3, 3)

                Case "fast"
                    Dim geo As New StreamGeometry()
                    Using ctx = geo.Open()
                        ctx.BeginFigure(New Point(w / 2.0, 2), True, True)
                        ctx.LineTo(New Point(w - 2, h / 2.0), True, False)
                        ctx.LineTo(New Point(w * 0.7, h - 2), True, False)
                        ctx.LineTo(New Point(w * 0.3, h - 2), True, False)
                        ctx.LineTo(New Point(2, h / 2.0), True, False)
                    End Using
                    dc.DrawGeometry(New SolidColorBrush(primary), New Pen(Brushes.White, 1), geo)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.4, h * 0.45), 2, 2)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.6, h * 0.45), 2, 2)

                Case "boss"
                    Dim gold = Color.FromRgb(255, 200, 50)
                    dc.DrawEllipse(New SolidColorBrush(primary),
                        New Pen(New SolidColorBrush(gold), 2),
                        New Point(w / 2.0, h / 2.0), w / 2.0 - 2, h / 2.0 - 2)
                    dc.DrawRectangle(New SolidColorBrush(gold), Nothing,
                        New Rect(w * 0.25, 2, w * 0.5, h * 0.15))
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.35, h * 0.4), 4, 4)
                    dc.DrawEllipse(Brushes.White, Nothing, New Point(w * 0.65, h * 0.4), 4, 4)
                    dc.DrawEllipse(Brushes.Red, Nothing, New Point(w * 0.35, h * 0.4), 2, 2)
                    dc.DrawEllipse(Brushes.Red, Nothing, New Point(w * 0.65, h * 0.4), 2, 2)

                Case Else
                    dc.DrawEllipse(New SolidColorBrush(primary), Nothing,
                        New Point(w / 2.0, h / 2.0), w / 2.0, h / 2.0)
            End Select
        End Using
        Return Capture(dv, w, h)
    End Function

    ' ── Tile sprites: subtle space-themed backgrounds ──
    Public Shared Function GenerateTile(w As Integer, h As Integer, tileType As Integer) As BitmapSource
        Dim dv As New DrawingVisual()
        Using dc = dv.RenderOpen()
            Dim palette = {
                Color.FromRgb(25, 25, 50), Color.FromRgb(30, 20, 55),
                Color.FromRgb(20, 30, 45), Color.FromRgb(35, 25, 60),
                Color.FromRgb(15, 25, 40), Color.FromRgb(28, 22, 48),
                Color.FromRgb(22, 28, 52), Color.FromRgb(32, 18, 42)}
            dc.DrawRectangle(New SolidColorBrush(palette(tileType Mod palette.Length)), Nothing,
                New Rect(0, 0, w, h))
            Dim gridBrush = New SolidColorBrush(Color.FromArgb(15, 100, 150, 255))
            dc.DrawLine(New Pen(gridBrush, 0.5), New Point(0, 0), New Point(w, 0))
            dc.DrawLine(New Pen(gridBrush, 0.5), New Point(0, 0), New Point(0, h))
            If tileType Mod 3 = 0 Then
                dc.DrawEllipse(New SolidColorBrush(Color.FromArgb(30, 200, 220, 255)), Nothing,
                    New Point(w * 0.3, h * 0.4), 1, 1)
            End If
            If tileType Mod 4 = 1 Then
                dc.DrawEllipse(New SolidColorBrush(Color.FromArgb(8, 150, 100, 255)), Nothing,
                    New Point(w * 0.6, h * 0.7), w * 0.3, h * 0.2)
            End If
        End Using
        Return Capture(dv, w, h)
    End Function

    ' ── UI icon sprites: rounded rect + symbol ──
    Public Shared Function GenerateIcon(size As Integer, symbol As String, bgColor As Color) As BitmapSource
        Dim dv As New DrawingVisual()
        Using dc = dv.RenderOpen()
            dc.DrawRoundedRectangle(
                New SolidColorBrush(Color.FromArgb(200, bgColor.R, bgColor.G, bgColor.B)),
                New Pen(New SolidColorBrush(Color.FromArgb(150, 255, 255, 255)), 1),
                New Rect(1, 1, size - 2, size - 2), 4, 4)
            Dim ft As New FormattedText(symbol, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, New Typeface("Segoe UI Symbol"),
                size * 0.55, Brushes.White, 96)
            dc.DrawText(ft, New Point((size - ft.Width) / 2, (size - ft.Height) / 2))
        End Using
        Return Capture(dv, size, size)
    End Function

    ' ── Brick sprite: gradient rectangle with highlight ──
    Public Shared Function GenerateBrickSprite(w As Integer, h As Integer, primary As Color) As BitmapSource
        Dim dv As New DrawingVisual()
        Using dc = dv.RenderOpen()
            Dim lighter = Color.FromRgb(CByte(Math.Min(255, CInt(primary.R) + 40)),
                                        CByte(Math.Min(255, CInt(primary.G) + 40)),
                                        CByte(Math.Min(255, CInt(primary.B) + 40)))
            dc.DrawRoundedRectangle(New LinearGradientBrush(primary, lighter, 90.0),
                Nothing, New Rect(0, 0, w, h), 3, 3)
            dc.DrawRectangle(New SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
                Nothing, New Rect(2, 1, w - 4, h / 2.5))
        End Using
        Return Capture(dv, w, h)
    End Function

    ' ── Paddle sprite: rounded gradient bar ──
    Public Shared Function GeneratePaddleSprite(w As Integer, h As Integer, primary As Color) As BitmapSource
        Dim dv As New DrawingVisual()
        Using dc = dv.RenderOpen()
            Dim darker = Color.FromRgb(CByte(Math.Max(0, CInt(primary.R) - 60)),
                                       CByte(Math.Max(0, CInt(primary.G) - 60)),
                                       CByte(Math.Max(0, CInt(primary.B) - 60)))
            dc.DrawRoundedRectangle(New LinearGradientBrush(primary, darker, 90.0),
                Nothing, New Rect(0, 0, w, h), 6, 6)
            dc.DrawRectangle(New SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                Nothing, New Rect(3, 1, w - 6, h / 2.5))
        End Using
        Return Capture(dv, w, h)
    End Function

    ' ── Ball sprite: gradient circle with specular highlight ──
    Public Shared Function GenerateBallSprite(size As Integer, primary As Color) As BitmapSource
        Dim dv As New DrawingVisual()
        Using dc = dv.RenderOpen()
            Dim r = size / 2.0
            dc.DrawEllipse(New RadialGradientBrush(primary, Color.FromRgb(160, 210, 255)),
                Nothing, New Point(r, r), r, r)
            dc.DrawEllipse(New SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                Nothing, New Point(r * 0.7, r * 0.6), r * 0.3, r * 0.25)
        End Using
        Return Capture(dv, size, size)
    End Function

    ' ── RenderTargetBitmap capture helper ──
    Private Shared Function Capture(dv As DrawingVisual, w As Integer, h As Integer) As BitmapSource
        Dim rtb As New RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32)
        rtb.Render(dv)
        rtb.Freeze()
        Return rtb
    End Function

End Class
End Namespace
