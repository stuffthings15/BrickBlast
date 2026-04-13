' =============================================================================
' BRICK BLAST macOS — GameCanvas (Avalonia UI Port)
'
' Avalonia DrawingContext API is nearly identical to WPF's.
' Key differences documented inline:
'   PushOpacity / PushTransform  → return IDisposable; use "Using" blocks
'   DrawRoundedRectangle         → use DrawRectangle with CornerRadius overload
'   FormattedText constructor    → Avalonia 11.x variant (see MkText below)
'   BitmapSource                 → Avalonia.Media.Imaging.Bitmap
'   DispatcherTimer              → Avalonia.Threading.DispatcherTimer
'   Sound                        → macOS "afplay" subprocess (no winmm.dll)
'   WndProc/HwndSource           → not needed on Avalonia
' =============================================================================
Imports System.Globalization
Imports System.IO
Imports System.Diagnostics
Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Input
Imports Avalonia.Media
Imports Avalonia.Media.Imaging
Imports Avalonia.Threading

Namespace BrickBlastMacOS

    Public Class GameCanvas
        Inherits Control

        ' ── Logical resolution (identical to WPF version) ────────────────────
        Private Const LOGICAL_WIDTH As Integer  = 1200
        Private Const LOGICAL_HEIGHT As Integer = 867
        Private Const PADDLE_WIDTH As Integer   = 240
        Private Const PADDLE_HEIGHT As Integer  = 14
        Private Const PADDLE_Y_OFFSET As Integer = 50
        Private Const PADDLE_SPEED As Integer   = 26
        Private Const BALL_RADIUS As Integer    = 8
        Private Const INITIAL_BALL_SPEED As Single = 8.25F
        Private Const BRICK_ROWS As Integer     = 7
        Private Const BRICK_COLS As Integer     = 12
        Private Const BRICK_TOP_OFFSET As Integer = 70
        Private Const MAX_LIVES As Integer      = 10

        ' ── Paths ─────────────────────────────────────────────────────────────
        Private ReadOnly _assetsPath As String =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets")

        ' ── Sprite cache ──────────────────────────────────────────────────────
        Private ReadOnly _cache As New Dictionary(Of String, Bitmap)()

        ' ── Game state ────────────────────────────────────────────────────────
        Private _score As Integer = 0
        Private _lives As Integer = MAX_LIVES
        Private _level As Integer = 1
        Private _combo As Integer = 0
        Private _frameCount As Integer = 0
        Private _leftDown As Boolean, _rightDown As Boolean
        Private ReadOnly _rng As New Random()
        Private _timer As DispatcherTimer

        ' ── Star field ────────────────────────────────────────────────────────
        Private _starX(79) As Single
        Private _starY(79) As Single
        Private _starB(79) As Integer    ' brightness

        ' ── Constructor ───────────────────────────────────────────────────────
        Public Sub New()
            Focusable = True
            AddHandler Loaded, AddressOf OnLoaded
        End Sub

        Private Sub OnLoaded(sender As Object, e As Avalonia.Interactivity.RoutedEventArgs)
            InitStarField()
            _timer = New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(16)}
            AddHandler _timer.Tick, AddressOf OnTick
            _timer.Start()
        End Sub

        ' ══════════════════════════════════════════════════════════════════════
        '  ASSET LOADING
        ' ══════════════════════════════════════════════════════════════════════
        Private Function GetSprite(key As String) As Bitmap
            If _cache.ContainsKey(key) Then Return _cache(key)
            Dim parts = key.Split("/"c)
            If parts.Length < 2 Then Return Nothing
            Dim file = Path.Combine(_assetsPath, parts(0), parts(1) & ".png")
            If Not File.Exists(file) Then Return Nothing
            Try
                Dim bmp = New Bitmap(file)
                _cache(key) = bmp
                Return bmp
            Catch
                Return Nothing
            End Try
        End Function

        ' ══════════════════════════════════════════════════════════════════════
        '  COLOR / BRUSH HELPERS  (same logic as WPF version)
        ' ══════════════════════════════════════════════════════════════════════
        Private Shared Function C3(r As Integer, g As Integer, b As Integer) As Color
            Return Color.FromRgb(CByte(r), CByte(g), CByte(b))
        End Function
        Private Shared Function C4(a As Integer, r As Integer, g As Integer, b As Integer) As Color
            Return Color.FromArgb(CByte(a), CByte(r), CByte(g), CByte(b))
        End Function
        Private Shared Function SB(c As Color) As IBrush
            Return New SolidColorBrush(c)
        End Function

        ' ── FormattedText helper (Avalonia 11.x API) ──────────────────────────
        ' WPF:     MkText(text, size, color, bold)  returns FormattedText
        ' Avalonia: same signature, different constructor
        Private Function MkText(text As String, size As Double, color As Color,
                                Optional bold As Boolean = False) As FormattedText
            Dim weight = If(bold, FontWeight.Bold, FontWeight.Normal)
            Dim tf = New Typeface(New FontFamily("Segoe UI, Inter, Arial"), FontStyle.Normal, weight)
            Return New FormattedText(text, CultureInfo.InvariantCulture,
                                     FlowDirection.LeftToRight, tf, size, SB(color))
        End Function

        ' ── Centered text helper ──────────────────────────────────────────────
        Private Sub DrawCT(dc As DrawingContext, text As String, size As Double,
                           color As Color, y As Double, Optional bold As Boolean = False)
            Dim ft = MkText(text, size, color, bold)
            dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, y))
        End Sub

        ' ══════════════════════════════════════════════════════════════════════
        '  GAME LOOP
        ' ══════════════════════════════════════════════════════════════════════
        Private Sub OnTick(sender As Object, e As EventArgs)
            _frameCount += 1
            UpdateStarField()
            InvalidateVisual()   ' Avalonia equivalent of WPF InvalidateVisual()
        End Sub

        ' ══════════════════════════════════════════════════════════════════════
        '  RENDER  (Avalonia override — "Render" instead of WPF "OnRender")
        ' ══════════════════════════════════════════════════════════════════════
        Public Overrides Sub Render(context As DrawingContext)
            ' Black background
            context.DrawRectangle(SB(C3(15, 15, 30)), Nothing, New Rect(0, 0, Bounds.Width, Bounds.Height))

            ' Scale logical → physical  (same ScaleTransform logic as WPF)
            Dim sx = Bounds.Width  / LOGICAL_WIDTH
            Dim sy = Bounds.Height / LOGICAL_HEIGHT
            '  Avalonia: PushTransform returns IDisposable → use Using
            Using context.PushTransform(Matrix.CreateScale(sx, sy))
                DrawStarField(context)
                DrawMenu(context)   ' <── Replace with full game-state switch for complete port
            End Using
        End Sub

        ' ══════════════════════════════════════════════════════════════════════
        '  STAR FIELD  (identical logic to WPF version)
        ' ══════════════════════════════════════════════════════════════════════
        Private Sub InitStarField()
            For i = 0 To 79
                _starX(i) = CSng(_rng.NextDouble() * LOGICAL_WIDTH)
                _starY(i) = CSng(_rng.NextDouble() * LOGICAL_HEIGHT)
                _starB(i) = _rng.Next(60, 220)
            Next
        End Sub

        Private Sub UpdateStarField()
            For i = 0 To 79
                _starY(i) += 0.15F
                If _starY(i) > LOGICAL_HEIGHT Then _starY(i) = 0
            Next
        End Sub

        Private Sub DrawStarField(dc As DrawingContext)
            For i = 0 To 79
                Dim b = CByte(Math.Max(30, Math.Min(255,
                    _starB(i) + CInt(Math.Sin(_frameCount * 0.05 + i) * 40))))
                dc.DrawEllipse(SB(Color.FromArgb(b, b, b, CByte(Math.Min(255, b + 30)))),
                    Nothing, New Point(_starX(i), _starY(i)), 1.5, 1.5)
            Next
        End Sub

        ' ══════════════════════════════════════════════════════════════════════
        '  MENU SCREEN  (abbreviated — expand with full DrawMenu from WPF port)
        '  Avalonia PushOpacity returns IDisposable → must use "Using" blocks
        ' ══════════════════════════════════════════════════════════════════════
        Private Sub DrawMenu(dc As DrawingContext)
            Dim bg = GetSprite("Tiles/menu_background")
            If bg IsNot Nothing Then
                Using dc.PushOpacity(0.25)    ' <── Avalonia: Using, not dc.Pop()
                    dc.DrawImage(bg, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
                End Using
            End If

            ' Team name
            Dim ft1 = MkText("TEAM FAST TALK", 48, Colors.White, True)
            Dim geo1 = ft1.BuildGeometry(New Point((LOGICAL_WIDTH - ft1.Width) / 2, 140))
            For gl = 5 To 1 Step -1
                dc.DrawGeometry(Nothing, New Pen(SB(C4(30, 100, 180, 255)), gl * 2), geo1)
            Next
            dc.DrawGeometry(New LinearGradientBrush() With {
                .StartPoint = New RelativePoint(0, 0, RelativeUnit.Relative),
                .EndPoint   = New RelativePoint(0, 1, RelativeUnit.Relative),
                .GradientStops = New GradientStops From {
                    New GradientStop(C3(100, 200, 255), 0),
                    New GradientStop(C3(255, 120, 200), 1)}
            }, Nothing, geo1)

            ' Game title
            Dim ft2 = MkText("BRICK BLAST", 58, Colors.White, True)
            Dim geo2 = ft2.BuildGeometry(New Point((LOGICAL_WIDTH - ft2.Width) / 2, 200))
            For gl = 5 To 1 Step -1
                dc.DrawGeometry(Nothing, New Pen(SB(C4(25, 255, 150, 50)), gl * 2), geo2)
            Next
            dc.DrawGeometry(New LinearGradientBrush() With {
                .StartPoint = New RelativePoint(0, 0, RelativeUnit.Relative),
                .EndPoint   = New RelativePoint(0, 1, RelativeUnit.Relative),
                .GradientStops = New GradientStops From {
                    New GradientStop(C3(255, 200, 80), 0),
                    New GradientStop(C3(255, 100, 50), 1)}
            }, Nothing, geo2)

            DrawCT(dc, "Press SPACE to Start", 18, Colors.White, 315)
            DrawCT(dc, "ARROW KEYS  /  A D  to move  |  F  Speed  |  P  Pause  |  Alt+Enter  Fullscreen", 11, C3(150, 150, 170), 432)

            Dim mascot = GetSprite("Characters/menu_mascot")
            If mascot IsNot Nothing Then
                Using dc.PushOpacity(0.7)
                    dc.DrawImage(mascot, New Rect(LOGICAL_WIDTH - 180, LOGICAL_HEIGHT - 280, 160, 240))
                End Using
            End If
        End Sub

        ' ══════════════════════════════════════════════════════════════════════
        '  SOUND  (macOS: afplay subprocess instead of winmm.dll)
        ' ══════════════════════════════════════════════════════════════════════
        ''' <summary>Play a WAV file asynchronously via macOS afplay command.</summary>
        Private Shared Sub PlayWav(wavPath As String)
            If Not File.Exists(wavPath) Then Return
            Try
                Process.Start(New ProcessStartInfo("afplay", $"""{wavPath}""") With {
                    .UseShellExecute  = False,
                    .CreateNoWindow   = True
                })
            Catch
                ' Silent fail — afplay not available or path invalid
            End Try
        End Sub

        ' ══════════════════════════════════════════════════════════════════════
        '  INPUT  (Avalonia KeyEventArgs — Key names match WPF)
        ' ══════════════════════════════════════════════════════════════════════
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            Select Case e.Key
                Case Key.Left,  Key.A : _leftDown  = True
                Case Key.Right, Key.D : _rightDown = True
                Case Key.Space        : ' TODO: launch ball / resume
                Case Key.P, Key.Escape : ' TODO: pause
            End Select
        End Sub

        Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
            MyBase.OnKeyUp(e)
            Select Case e.Key
                Case Key.Left,  Key.A : _leftDown  = False
                Case Key.Right, Key.D : _rightDown = False
            End Select
        End Sub

    End Class

End Namespace