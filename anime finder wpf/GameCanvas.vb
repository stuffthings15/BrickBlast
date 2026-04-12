' =============================================================================
' CURTIS LOOP: BRICK BLAST — WPF Version
' Team Fast Talk | CS-120 | .NET 10 | WPF DrawingContext Rendering
'
' Full port of the WinForms game to WPF. Inherits FrameworkElement and
' renders via OnRender(DrawingContext). Same gameplay, same audio system
' (winmm.dll MCI), same procedural asset generation.
' =============================================================================
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.Json
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Interop
Imports System.Windows.Media
Imports System.Windows.Threading

Namespace BrickBlastWPF
Public Class GameCanvas
    Inherits FrameworkElement

#Region "Win32 Sound API"
    <DllImport("winmm.dll", SetLastError:=True)>
    Private Shared Function PlaySound(pszSound As Byte(), hmod As IntPtr, fdwSound As UInteger) As Boolean
    End Function

    <DllImport("winmm.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function mciSendString(command As String, buffer As StringBuilder, bufferSize As Integer, hwndCallback As IntPtr) As Integer
    End Function

    Private Const SND_ASYNC As UInteger = &H1UI
    Private Const SND_MEMORY As UInteger = &H4UI
    Private Const MM_MCINOTIFY As Integer = &H3B9
    Private Const MCI_NOTIFY_SUCCESSFUL As Integer = 1
    Private _hwnd As IntPtr = IntPtr.Zero

    Private Function WndProc(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr
        If msg = MM_MCINOTIFY AndAlso wParam.ToInt32() = MCI_NOTIFY_SUCCESSFUL Then
            _musicPlaying = False
            If _usingHighScoreMusic Then
                ScheduleHighScoreMusicStart(10)
            Else
                _musicStyle = (_musicStyle + 1) Mod 10
                ScheduleMusicStart(10)
            End If
        End If
        Return IntPtr.Zero
    End Function
#End Region

#Region "Constants"
    Private Const PADDLE_WIDTH As Integer = 240
    Private Const PADDLE_HEIGHT As Integer = 14
    Private Const PADDLE_Y_OFFSET As Integer = 50
    Private Const PADDLE_SPEED As Integer = 26
    Private Const BALL_RADIUS As Integer = 8
    Private Const MIN_BALL_RADIUS As Integer = 4
    Private Const MAX_BALL_RADIUS As Integer = 20
    Private Const INITIAL_BALL_SPEED As Single = 8.25F
    Private Const BRICK_ROWS As Integer = 7
    Private Const BRICK_COLS As Integer = 12
    Private Const BRICK_PADDING As Integer = 4
    Private Const BRICK_TOP_OFFSET As Integer = 70
    Private Const MAX_LIVES As Integer = 10
    Private Const POWERUP_SIZE As Integer = 45
    Private Const POWERUP_SPEED As Single = 3.0F
    Private Const PARTICLE_COUNT As Integer = 8
    Private Const LOGICAL_WIDTH As Integer = 1200
    Private Const LOGICAL_HEIGHT As Integer = 867
#End Region

#Region "State and Structures"
    Private Enum GameState
        Menu
        Playing
        Paused
        LevelComplete
        Options
        HighScore
    End Enum

    Private Structure Ball
        Public X As Single, Y As Single, DX As Single, DY As Single, Speed As Single, Active As Boolean
    End Structure

    Private Structure Brick
        Public Rect As Rect, Color1 As Color, Color2 As Color, Alive As Boolean, HitsLeft As Integer, Points As Integer, Row As Integer
    End Structure

    Private Enum PowerUpType
        BlueBallGrow
        RedExtraLife
        GreenMultiBall
        YellowBallShrink
        PurplePaddleMega
        OrangeBallSlow
        PinkBallFast
    End Enum

    Private Structure PowerUp
        Public X As Single, Y As Single, PType As PowerUpType, Active As Boolean, Color1 As Color, Symbol As String
    End Structure

    Private Structure Particle
        Public X As Single, Y As Single, DX As Single, DY As Single, Life As Single, MaxLife As Single, ParticleColor As Color, Size As Single, Active As Boolean
    End Structure

    Private Structure ScoreRecord
        Public PlayerName As String
        Public PlayerScore As Integer
    End Structure
#End Region

#Region "Variables"
    Private _state As GameState = GameState.Menu
    Private _score As Integer = 0
    Private _highScore As Integer = 0
    Private _lives As Integer = MAX_LIVES
    Private _level As Integer = 1
    Private _combo As Integer = 0
    Private _comboTimer As Integer = 0
    Private _ballRadius As Integer = BALL_RADIUS
    Private _paddleX As Single
    Private _paddleWidth As Integer = PADDLE_WIDTH
    Private _paddleWidthTimer As Integer = 0
    Private _balls As New List(Of Ball)
    Private _bricks As New List(Of Brick)
    Private _powerUps As New List(Of PowerUp)
    Private _particles As New List(Of Particle)
    Private _leftPressed As Boolean = False
    Private _rightPressed As Boolean = False
    Private _rng As New Random()
    Private _frameCount As Integer = 0
    Private _screenShake As Integer = 0
    Private _sfxVolume As Integer = 80
    Private _musicVolume As Integer = 100
    Private _musicSpeed As Integer = 50
    Private _colorblindMode As Boolean = False
    Private _speedBoost As Boolean = False
    Private _settingsSelection As Integer = 0
    Private _previousState As GameState = GameState.Menu
    Private _musicTempFile As String = ""
    Private _musicPlaying As Boolean = False
    Private _musicFiles() As String = Nothing
    Private _lastSfxBuffer As Byte() = Nothing
    Private _musicStyle As Integer = 2
    Private _sfxStyle As Integer = 0
    Private _musicChangeTimer As DispatcherTimer = Nothing
    Private _pendingHighScore As Boolean = False
    Private _highScoreDelayFrames As Integer = 0
    Private _highScoreMusicFile As String = ""
    Private _usingHighScoreMusic As Boolean = False
    Private _highScores As New List(Of ScoreRecord)
    Private _nameInput As String = ""
    Private _highScoreSaved As Boolean = False
    Private ReadOnly _highScorePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BrickBlast", "highscores.json")
    Private _getReadyFrames As Integer = 0
    Private _gameTimer As DispatcherTimer
    Private _dpi As Double = 1.0
    Private _starFieldX() As Single, _starFieldY() As Single, _starFieldSpeed() As Single, _starFieldBright() As Integer
    Private _windowScale As Integer = 2
    Private _windowScaleNames() As String = {"Small (900x650)", "Medium (1200x867)", "Large (1500x1083)", "XL (1800x1300)"}
    Private _windowScaleSizes()() As Integer = {New Integer() {900, 650}, New Integer() {1200, 867}, New Integer() {1500, 1083}, New Integer() {1800, 1300}}

    Private Shared Function C3(r As Integer, g As Integer, b As Integer) As Color
        Return Color.FromRgb(CByte(r), CByte(g), CByte(b))
    End Function
    Private Shared Function C4(a As Integer, r As Integer, g As Integer, b As Integer) As Color
        Return Color.FromArgb(CByte(a), CByte(r), CByte(g), CByte(b))
    End Function
    Private Shared Function SB(c As Color) As SolidColorBrush
        Return New SolidColorBrush(c)
    End Function

    Private _rowColors As Color()() = {
        New Color() {C3(255, 60, 80), C3(255, 100, 120)},
        New Color() {C3(255, 140, 50), C3(255, 180, 90)},
        New Color() {C3(255, 220, 50), C3(255, 240, 120)},
        New Color() {C3(50, 220, 100), C3(100, 255, 150)},
        New Color() {C3(50, 180, 255), C3(100, 210, 255)},
        New Color() {C3(130, 80, 255), C3(170, 130, 255)},
        New Color() {C3(255, 80, 200), C3(255, 140, 230)}}
    Private _colorblindColors As Color()() = {
        New Color() {C3(0, 114, 178), C3(60, 150, 210)},
        New Color() {C3(230, 159, 0), C3(255, 195, 60)},
        New Color() {C3(240, 228, 66), C3(255, 245, 140)},
        New Color() {C3(0, 158, 115), C3(60, 200, 155)},
        New Color() {C3(213, 94, 0), C3(245, 140, 50)},
        New Color() {C3(86, 180, 233), C3(140, 210, 245)},
        New Color() {C3(204, 121, 167), C3(235, 170, 200)}}
    Private _colorblindSymbols() As String = {ChrW(&H25A0), ChrW(&H25B2), ChrW(&H25CF), ChrW(&H2666), ChrW(&H2605), ChrW(&H25C6), ChrW(&H2663)}
    Private _musicStyleNames() As String = {"Zelda Adventure", "Mega Man Energy", "Tetris Classic", "Pac-Man Playful", "Space Invaders", "Castlevania Dark", "Metroid Atmosphere", "Galaga Arcade", "Contra Action", "Double Dragon"}
    Private _sfxStyleNames() As String = {"Classic", "Zelda", "Mega Man", "Tetris", "Retro Arcade"}
    Private _sfxData()() As Integer = {
        New Integer() {300, 60, 500, 80, 600, 80, 1000, 120, 200, 400, 900, 300},
        New Integer() {660, 50, 880, 70, 1047, 70, 1319, 100, 330, 350, 1047, 350},
        New Integer() {784, 40, 1047, 50, 1319, 55, 1568, 90, 392, 300, 1568, 300},
        New Integer() {440, 55, 523, 70, 659, 75, 880, 110, 220, 400, 880, 350},
        New Integer() {250, 65, 400, 80, 500, 85, 800, 120, 150, 400, 700, 350}}
#End Region

#Region "Constructor"
    Public Sub New()
        Focusable = True
        AddHandler Loaded, AddressOf OnLoaded
        AddHandler Unloaded, AddressOf OnUnloaded
    End Sub

    Private Sub OnLoaded(sender As Object, e As RoutedEventArgs)
        _dpi = VisualTreeHelper.GetDpi(Me).PixelsPerDip
        Dim src = TryCast(PresentationSource.FromVisual(Me), HwndSource)
        If src IsNot Nothing Then
            _hwnd = src.Handle
            src.AddHook(New HwndSourceHook(AddressOf WndProc))
        End If
        InitStarField()
        _state = GameState.Menu
        LoadHighScores()
        PreGenerateAllMusic()
        _gameTimer = New DispatcherTimer()
        _gameTimer.Interval = TimeSpan.FromMilliseconds(16)
        AddHandler _gameTimer.Tick, AddressOf GameTimer_Tick
        _gameTimer.Start()
        StartMusic()
        Focus()
    End Sub

    Private Sub OnUnloaded(sender As Object, e As RoutedEventArgs)
        If _gameTimer IsNot Nothing Then _gameTimer.Stop()
        CleanupMusic()
    End Sub
#End Region

#Region "Game Loop"
    Private Sub GameTimer_Tick(sender As Object, e As EventArgs)
        _frameCount += 1
        UpdateStarField()
        If _highScoreDelayFrames > 0 Then
            _highScoreDelayFrames -= 1
            If _highScoreDelayFrames = 0 AndAlso _pendingHighScore Then
                _pendingHighScore = False
                _state = GameState.HighScore
                StartHighScoreMusic()
            End If
        End If
        If _state = GameState.Playing Then
            UpdatePaddle()
            UpdateBalls()
            UpdatePowerUps()
            UpdateParticles()
            UpdateTimers()
            CheckLevelComplete()
        End If
        If _screenShake > 0 Then _screenShake -= 1
        InvalidateVisual()
    End Sub
#End Region

#Region "Text Helpers"
    Private Function MkText(text As String, size As Double, color As Color, Optional bold As Boolean = False) As FormattedText
        Return New FormattedText(text, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            New Typeface(New FontFamily("Segoe UI"), FontStyles.Normal, If(bold, FontWeights.Bold, FontWeights.Normal), FontStretches.Normal),
            size, SB(color), _dpi)
    End Function
    Private Function MkMono(text As String, size As Double, color As Color) As FormattedText
        Return New FormattedText(text, Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Consolas"), size, SB(color), _dpi)
    End Function
    Private Sub DrawCT(dc As DrawingContext, text As String, size As Double, color As Color, y As Double, Optional bold As Boolean = False)
        Dim ft = MkText(text, size, color, bold)
        dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, y))
    End Sub
#End Region

#Region "Rendering"
    Protected Overrides Sub OnRender(dc As DrawingContext)
        dc.DrawRectangle(SB(C3(15, 15, 30)), Nothing, New Rect(0, 0, ActualWidth, ActualHeight))
        Dim sx = ActualWidth / LOGICAL_WIDTH, sy = ActualHeight / LOGICAL_HEIGHT
        dc.PushTransform(New ScaleTransform(sx, sy))
        Dim shook = _screenShake > 0
        If shook Then dc.PushTransform(New TranslateTransform(_rng.Next(-3, 4), _rng.Next(-3, 4)))
        DrawStarField(dc)
        Select Case _state
            Case GameState.Menu : DrawMenu(dc)
            Case GameState.Playing, GameState.Paused
                DrawGame(dc)
                If _state = GameState.Paused Then DrawOverlay(dc, "PAUSED", "Press SPACE to resume")
            Case GameState.LevelComplete
                DrawGame(dc)
                DrawOverlay(dc, $"LEVEL {_level} COMPLETE!", "Press SPACE for next level", True)
            Case GameState.Options
                If _previousState = GameState.Playing OrElse _previousState = GameState.Paused Then DrawGame(dc)
                DrawOptions(dc)
            Case GameState.HighScore : DrawHighScore(dc)
        End Select
        If shook Then dc.Pop()
        dc.Pop()
    End Sub

    Private Sub DrawStarField(dc As DrawingContext)
        For i = 0 To _starFieldX.Length - 1
            Dim bright = Math.Max(30, Math.Min(255, _starFieldBright(i) + CInt(Math.Sin(_frameCount * 0.05 + i) * 40)))
            dc.DrawEllipse(SB(Color.FromArgb(CByte(bright), CByte(bright), CByte(bright), CByte(Math.Min(255, bright + 30)))), Nothing, New Point(_starFieldX(i) + 1, _starFieldY(i) + 1), 1, 1)
        Next
    End Sub

    Private Sub DrawMenu(dc As DrawingContext)
        Dim ft1 = MkText("TEAM FAST TALK", 48, Colors.White, True)
        Dim geo1 = ft1.BuildGeometry(New Point((LOGICAL_WIDTH - ft1.Width) / 2, 140))
        For gl = 5 To 1 Step -1
            dc.DrawGeometry(Nothing, New Pen(SB(C4(30, 100, 180, 255)), gl * 2), geo1)
        Next
        dc.DrawGeometry(New LinearGradientBrush(C3(100, 200, 255), C3(255, 120, 200), 90.0), Nothing, geo1)
        Dim ft2 = MkText("BRICK BLAST", 58, Colors.White, True)
        Dim geo2 = ft2.BuildGeometry(New Point((LOGICAL_WIDTH - ft2.Width) / 2, 195))
        For gl = 5 To 1 Step -1
            dc.DrawGeometry(Nothing, New Pen(SB(C4(25, 255, 150, 50)), gl * 2), geo2)
        Next
        dc.DrawGeometry(New LinearGradientBrush(C3(255, 200, 80), C3(255, 100, 50), 90.0), Nothing, geo2)
        DrawCT(dc, "Press SPACE to Start", 18, Colors.White, 310)
        If _highScore > 0 Then DrawCT(dc, $"High Score: {_highScore}", 14, C3(255, 220, 100), 350)
        DrawCT(dc, ChrW(&H2699) & "  Press H or O for OPTIONS  " & ChrW(&H2699), 14, C3(100, 200, 255), 385, True)
        DrawCT(dc, "ARROW KEYS to move  |  F speed boost  |  P pause", 11, C3(150, 150, 170), 430)
        DrawCT(dc, $"Music: {_musicStyleNames(_musicStyle)}  |  SFX: {_sfxStyleNames(_sfxStyle)}", 11, C3(120, 140, 180), 460)
        DrawCT(dc, $"Window: {_windowScaleNames(_windowScale)}", 11, C3(120, 140, 180), 490)
        DrawCT(dc, "Destroy bricks " & ChrW(8226) & " Catch power-ups " & ChrW(8226) & " Build combos!", 11, C3(150, 150, 170), 520)
    End Sub

    Private Sub DrawGame(dc As DrawingContext)
        DrawHUD(dc) : DrawBricks(dc) : DrawPaddle(dc) : DrawBalls(dc) : DrawPowerUps(dc) : DrawParticles(dc) : DrawCombo(dc) : DrawGetReady(dc)
    End Sub

    Private Sub DrawHUD(dc As DrawingContext)
        dc.DrawText(MkText($"SCORE: {_score}", 13, Colors.White, True), New Point(15, 12))
        DrawCT(dc, $"LEVEL {_level}", 13, C3(180, 200, 255), 12, True)
        Dim lt = MkText($"LIVES: {New String(ChrW(&H2665), _lives)}", 13, C3(255, 100, 130), True)
        dc.DrawText(lt, New Point(LOGICAL_WIDTH - lt.Width - 15, 12))
        If _speedBoost Then DrawCT(dc, ChrW(&H26A1) & " 2x SPEED", 13, C3(255, 200, 50), 32, True)
        If _ballRadius <> BALL_RADIUS Then dc.DrawText(MkText($"Ball: {_ballRadius}px", 13, C3(150, 200, 255), True), New Point(15, 32))
        If _paddleWidthTimer > 0 Then
            Dim pt = MkText($"Paddle: {CInt(Math.Ceiling(_paddleWidthTimer / 60.0))}s", 13, C3(170, 80, 255), True)
            dc.DrawText(pt, New Point(LOGICAL_WIDTH - pt.Width - 15, 32))
        End If
        dc.DrawLine(New Pen(SB(C4(40, 100, 180, 255)), 1), New Point(0, 50), New Point(LOGICAL_WIDTH, 50))
    End Sub

    Private Sub DrawBricks(dc As DrawingContext)
        For Each bk In _bricks
            If Not bk.Alive Then Continue For
            dc.DrawRoundedRectangle(New LinearGradientBrush(bk.Color1, bk.Color2, 90.0), Nothing, bk.Rect, 4, 4)
            dc.DrawRectangle(SB(C4(50, 255, 255, 255)), Nothing, New Rect(bk.Rect.X + 2, bk.Rect.Y + 1, bk.Rect.Width - 4, bk.Rect.Height / 2.5))
            If _colorblindMode Then
                dc.DrawRoundedRectangle(Nothing, New Pen(SB(Colors.White), 2), bk.Rect, 4, 4)
                Dim sym = _colorblindSymbols(bk.Row Mod _colorblindSymbols.Length)
                If bk.HitsLeft > 1 Then sym = bk.HitsLeft.ToString() & sym
                Dim st = MkText(sym, 10, Colors.White, True)
                dc.DrawText(st, New Point(bk.Rect.X + (bk.Rect.Width - st.Width) / 2, bk.Rect.Y + (bk.Rect.Height - st.Height) / 2))
            ElseIf bk.HitsLeft > 1 Then
                Dim ht = MkText(bk.HitsLeft.ToString(), 8, C4(180, 0, 0, 0), True)
                dc.DrawText(ht, New Point(bk.Rect.X + (bk.Rect.Width - ht.Width) / 2, bk.Rect.Y + (bk.Rect.Height - ht.Height) / 2))
            End If
        Next
    End Sub

    Private Sub DrawPaddle(dc As DrawingContext)
        Dim py As Double = LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT
        Dim c1 = If(_colorblindMode, C3(240, 228, 66), C3(80, 180, 255))
        Dim c2 = If(_colorblindMode, C3(200, 190, 40), C3(40, 100, 200))
        dc.DrawEllipse(SB(Color.FromArgb(30, c1.R, c1.G, c1.B)), Nothing, New Point(_paddleX + _paddleWidth / 2.0, py + 12), _paddleWidth / 2.0 + 10, 10)
        Dim pr = New Rect(_paddleX, py, _paddleWidth, PADDLE_HEIGHT)
        dc.DrawRoundedRectangle(New LinearGradientBrush(c1, c2, 90.0), Nothing, pr, 7, 7)
        dc.DrawRectangle(SB(C4(80, 255, 255, 255)), Nothing, New Rect(_paddleX + 4, py + 1, _paddleWidth - 8, PADDLE_HEIGHT / 2.5))
        If _colorblindMode Then dc.DrawRoundedRectangle(Nothing, New Pen(SB(Colors.White), 2), pr, 7, 7)
    End Sub

    Private Sub DrawBalls(dc As DrawingContext)
        Dim br2 As Double = _ballRadius
        For Each b In _balls
            If Not b.Active Then Continue For
            For gs = 20 To 4 Step -4
                Dim al = CByte(CInt(20 * (4.0 / gs)))
                dc.DrawEllipse(SB(If(_speedBoost, Color.FromArgb(al, 255, 200, 50), Color.FromArgb(al, 200, 230, 255))), Nothing, New Point(b.X, b.Y), gs / 2.0, gs / 2.0)
            Next
            dc.DrawEllipse(New LinearGradientBrush(If(_speedBoost, C3(255, 255, 200), Colors.White), If(_speedBoost, C3(255, 140, 20), C3(160, 210, 255)), 45.0), Nothing, New Point(b.X, b.Y), br2, br2)
            dc.DrawEllipse(SB(C4(180, 255, 255, 255)), Nothing, New Point(b.X - br2 * 0.2, b.Y - br2 * 0.3), br2 * 0.3, br2 * 0.25)
        Next
    End Sub

    Private Sub DrawPowerUps(dc As DrawingContext)
        For Each pu In _powerUps
            If Not pu.Active Then Continue For
            Dim cy As Double = pu.Y + Math.Sin(_frameCount * 0.1) * 3
            dc.DrawEllipse(SB(Color.FromArgb(200, pu.Color1.R, pu.Color1.G, pu.Color1.B)), Nothing, New Point(pu.X, cy), POWERUP_SIZE / 2.0, POWERUP_SIZE / 2.0)
            Dim st = MkText(pu.Symbol, 18, Colors.White, True)
            dc.DrawText(st, New Point(pu.X - st.Width / 2, cy - st.Height / 2))
        Next
    End Sub

    Private Sub DrawParticles(dc As DrawingContext)
        For Each p In _particles
            If Not p.Active Then Continue For
            Dim al = CByte(Math.Max(0, Math.Min(255, CInt(255 * (p.Life / p.MaxLife)))))
            Dim sz As Double = p.Size * (p.Life / p.MaxLife)
            If sz < 0.5 Then Continue For
            dc.DrawEllipse(SB(Color.FromArgb(al, p.ParticleColor.R, p.ParticleColor.G, p.ParticleColor.B)), Nothing, New Point(p.X, p.Y), sz / 2, sz / 2)
        Next
    End Sub

    Private Sub DrawCombo(dc As DrawingContext)
        If _combo >= 2 AndAlso _comboTimer > 0 Then
            Dim ca = CByte(Math.Max(0, Math.Min(255, CInt(_comboTimer * 5))))
            Dim ft = MkText($"COMBO x{_combo}!", 20, Color.FromArgb(ca, 255, 240, 100), True)
            dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, LOGICAL_HEIGHT / 2.0 + 30))
        End If
    End Sub

    Private Sub DrawGetReady(dc As DrawingContext)
        If _getReadyFrames <= 0 Then Return
        Dim ct = If(_getReadyFrames > 120, "3", If(_getReadyFrames > 60, "2", "1"))
        Dim pulse = Math.Abs(Math.Sin(_frameCount * 0.15)) * 10 + 58
        Dim ft = MkText(ct, pulse, C4(230, 255, 240, 100), True)
        dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, LOGICAL_HEIGHT / 2.0 - ft.Height / 2 - 20))
        DrawCT(dc, "GET READY!", 14, C4(180, 200, 200, 220), LOGICAL_HEIGHT / 2.0 + 50)
    End Sub

    Private Sub DrawOptions(dc As DrawingContext)
        dc.DrawRectangle(SB(C4(215, 0, 0, 20)), Nothing, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
        Dim pw = 780.0, ph = 600.0, px = (LOGICAL_WIDTH - pw) / 2, py = (LOGICAL_HEIGHT - ph) / 2
        dc.DrawRoundedRectangle(SB(C4(245, 12, 12, 35)), New Pen(SB(C4(100, 80, 160, 255)), 2), New Rect(px, py, pw, ph), 14, 14)
        DrawCT(dc, "OPTIONS", 22, C3(100, 200, 255), py + 12, True)
        Dim y = py + 60, lx = px + 25, rx = px + pw / 2 + 10, barX = px + 260
        dc.DrawText(MkText("CONTROLS:", 12, C3(255, 200, 100), True), New Point(lx, y))
        dc.DrawText(MkText("POWER-UPS:", 12, C3(255, 200, 100), True), New Point(rx, y)) : y += 24
        dc.DrawText(MkText(ChrW(&H2190) & " " & ChrW(&H2192) & " / A D   Move Paddle", 10, C3(210, 210, 225)), New Point(lx + 8, y))
        dc.DrawText(MkText(ChrW(&H25CF) & " Blue   Ball grows", 10, C3(80, 150, 255)), New Point(rx + 8, y)) : y += 19
        dc.DrawText(MkText("SPACE       Start / Resume", 10, C3(210, 210, 225)), New Point(lx + 8, y))
        dc.DrawText(MkText(ChrW(&H25CF) & " Red    +1 life", 10, C3(255, 90, 90)), New Point(rx + 8, y)) : y += 19
        dc.DrawText(MkText("P / ESC     Pause", 10, C3(210, 210, 225)), New Point(lx + 8, y))
        dc.DrawText(MkText(ChrW(&H25CF) & " Green  Multi-ball", 10, C3(80, 220, 120)), New Point(rx + 8, y)) : y += 19
        dc.DrawText(MkText("F           Speed Boost (2x)", 10, C3(210, 210, 225)), New Point(lx + 8, y))
        dc.DrawText(MkText(ChrW(&H25CF) & " Yellow Ball shrinks", 10, C3(255, 220, 60)), New Point(rx + 8, y)) : y += 19
        dc.DrawText(MkText("H / O       Options", 10, C3(210, 210, 225)), New Point(lx + 8, y))
        dc.DrawText(MkText(ChrW(&H25CF) & " Purple 3x paddle", 10, C3(170, 80, 255)), New Point(rx + 8, y)) : y += 19
        dc.DrawText(MkText(ChrW(&H25CF) & " Orange Ball slows", 10, C3(255, 150, 60)), New Point(rx + 8, y)) : y += 19
        dc.DrawText(MkText(ChrW(&H25CF) & " Pink   Ball speeds", 10, C3(255, 120, 200)), New Point(rx + 8, y)) : y += 26
        dc.DrawText(MkText("SETTINGS:", 12, C3(255, 200, 100), True), New Point(lx, y)) : y += 28
        Dim items() = {"SFX Volume:", "Music Volume:", "Music Speed:", "Music Style:", "SFX Style:", "Colorblind Mode:", "Window Size:"}
        For idx = 0 To 6
            Dim sc = If(_settingsSelection = idx, C3(255, 220, 100), C3(195, 195, 215))
            Dim sel = If(_settingsSelection = idx, ChrW(&H25BA) & "  ", "    ")
            dc.DrawText(MkText(sel & items(idx), 10, sc), New Point(lx + 8, y))
            Select Case idx
                Case 0 : DrawVolBar(dc, barX, y + 2, 200, 16, _sfxVolume, sc) : dc.DrawText(MkText($"{_sfxVolume}%", 10, sc, True), New Point(barX + 210, y))
                Case 1 : Dim ev = GetEffectiveMusicVolume() : DrawVolBar(dc, barX, y + 2, 200, 16, ev, sc) : dc.DrawText(MkText($"{ev}%", 10, sc, True), New Point(barX + 210, y))
                Case 2 : DrawVolBar(dc, barX, y + 2, 200, 16, Math.Min(100, _musicSpeed), sc) : dc.DrawText(MkText($"{_musicSpeed}%", 10, sc, True), New Point(barX + 210, y))
                Case 3 : dc.DrawText(MkText(ChrW(&H25C4) & " " & _musicStyleNames(_musicStyle) & " " & ChrW(&H25BA), 10, sc, True), New Point(barX, y))
                Case 4 : dc.DrawText(MkText(ChrW(&H25C4) & " " & _sfxStyleNames(_sfxStyle) & " " & ChrW(&H25BA), 10, sc, True), New Point(barX, y))
                Case 5 : dc.DrawText(MkText($"[ {If(_colorblindMode, "ON", "OFF")} ]", 10, If(_colorblindMode, C3(100, 255, 150), C3(255, 100, 100)), True), New Point(barX, y))
                Case 6 : dc.DrawText(MkText(ChrW(&H25C4) & " " & _windowScaleNames(_windowScale) & " " & ChrW(&H25BA), 10, sc, True), New Point(barX, y))
            End Select
            y += 32
        Next
        DrawCT(dc, ChrW(&H2191) & ChrW(&H2193) & " Select   " & ChrW(&H2190) & ChrW(&H2192) & " Adjust   ENTER Toggle   O / ESC Close", 10, C3(130, 130, 155), py + ph - 30)
    End Sub

    Private Sub DrawVolBar(dc As DrawingContext, x As Double, y As Double, w As Double, h As Double, value As Integer, col As Color)
        dc.DrawRoundedRectangle(SB(C4(60, 255, 255, 255)), Nothing, New Rect(x, y, w, h), 4, 4)
        Dim fw = w * value / 100.0
        If fw > 2 Then dc.DrawRoundedRectangle(SB(Color.FromArgb(200, col.R, col.G, col.B)), Nothing, New Rect(x, y, fw, h), 4, 4)
    End Sub

    Private Sub DrawHighScore(dc As DrawingContext)
        DrawStarField(dc)
        dc.DrawRectangle(SB(C4(200, 0, 0, 20)), Nothing, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
        Dim pw = 520.0, ph = 540.0, px = (LOGICAL_WIDTH - pw) / 2, py = (LOGICAL_HEIGHT - ph) / 2
        dc.DrawRoundedRectangle(SB(C4(245, 12, 12, 35)), New Pen(SB(C4(100, 255, 80, 80)), 2), New Rect(px, py, pw, ph), 14, 14)
        DrawCT(dc, "GAME OVER", 30, C3(255, 80, 100), py + 15, True)
        DrawCT(dc, $"Final Score: {_score}", 18, C3(255, 220, 100), py + 65, True)
        DrawCT(dc, $"Level {_level}  |  Ball Size: {_ballRadius}px", 12, C3(180, 200, 255), py + 100)
        Dim y = py + 135
        If Not _highScoreSaved Then
            DrawCT(dc, "Enter Name: " & _nameInput & If(_frameCount Mod 60 < 30, "_", " "), 14, Colors.White, y)
            DrawCT(dc, "Press ENTER to save", 10, C3(140, 140, 160), y + 30)
        Else
            DrawCT(dc, "Score saved! Press SPACE to continue", 12, C3(100, 255, 150), y)
        End If
        y = py + 210
        DrawCT(dc, "HIGH SCORES", 14, C3(100, 200, 255), y, True) : y += 30
        If _highScores.Count = 0 Then
            Dim ft = MkMono("No scores yet!", 12, C3(140, 140, 160))
            dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, y))
        Else
            For i = 0 To Math.Min(9, _highScores.Count - 1)
                Dim rec = _highScores(i)
                Dim nm = If(rec.PlayerName.Length > 12, rec.PlayerName.Substring(0, 12), rec.PlayerName.PadRight(12))
                Dim ec = If(_highScoreSaved AndAlso rec.PlayerName = _nameInput AndAlso rec.PlayerScore = _score, C3(255, 220, 100), C3(195, 195, 215))
                Dim ft = MkMono($"{(i + 1).ToString().PadLeft(2)}. {nm} {rec.PlayerScore.ToString("N0").PadLeft(10)}", 12, ec)
                dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, y)) : y += 25
            Next
        End If
    End Sub

    Private Sub DrawOverlay(dc As DrawingContext, title As String, subtitle As String, Optional animated As Boolean = False)
        dc.DrawRectangle(SB(C4(180, 0, 0, 20)), Nothing, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
        Dim sz = If(animated, 40 + Math.Sin(_frameCount * 0.08) * 6, 40.0)
        Dim pulse = (Math.Sin(_frameCount * 0.05) + 1) / 2
        Dim tc = If(animated, Color.FromRgb(CByte(180 + pulse * 75), CByte(180 + pulse * 75), 255), Colors.White)
        DrawCT(dc, title, sz, tc, LOGICAL_HEIGHT / 2.0 - 60, True)
        DrawCT(dc, subtitle, 16, C3(200, 200, 220), LOGICAL_HEIGHT / 2.0 + 10)
    End Sub
#End Region

#Region "Input"
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        If _pendingHighScore Then Return
        If _state = GameState.HighScore Then
            If e.Key = Key.Back Then
                If _nameInput.Length > 0 Then _nameInput = _nameInput.Substring(0, _nameInput.Length - 1)
            ElseIf e.Key = Key.Enter Then
                If _nameInput.Length > 0 AndAlso Not _highScoreSaved Then AddHighScore(_nameInput, _score) : _highScoreSaved = True
            ElseIf e.Key = Key.Space AndAlso _highScoreSaved Then
                _usingHighScoreMusic = False : _pendingHighScore = False : StartMusic() : _state = GameState.Menu
            ElseIf Not _highScoreSaved Then
                Dim c = KeyToChar(e)
                If c IsNot Nothing AndAlso _nameInput.Length < 12 Then _nameInput &= c
            End If
            Return
        End If
        If _state = GameState.Options Then
            Select Case e.Key
                Case Key.Up : _settingsSelection = (_settingsSelection - 1 + 7) Mod 7
                Case Key.Down : _settingsSelection = (_settingsSelection + 1) Mod 7
                Case Key.Left : AdjustSettingLeft(_settingsSelection)
                Case Key.Right : AdjustSettingRight(_settingsSelection)
                Case Key.Enter, Key.Space : If _settingsSelection = 5 Then _colorblindMode = Not _colorblindMode
                Case Key.O, Key.H, Key.Escape : _state = _previousState
            End Select
            Return
        End If
        Select Case e.Key
            Case Key.Left, Key.A : _leftPressed = True
            Case Key.Right, Key.D : _rightPressed = True
            Case Key.Space
                If _state = GameState.Menu Then
                    StartNewGame()
                ElseIf _state = GameState.LevelComplete Then
                    NextLevel()
                ElseIf _state = GameState.Paused Then
                    _state = GameState.Playing
                End If
            Case Key.P, Key.Escape
                If _state = GameState.Playing Then
                    _state = GameState.Paused
                ElseIf _state = GameState.Paused Then
                    _state = GameState.Playing
                End If
            Case Key.F
                If _state = GameState.Playing Then _speedBoost = Not _speedBoost : PlaySFX(_sfxData(_sfxStyle)(10), 80)
            Case Key.H, Key.O
                If _state = GameState.Menu OrElse _state = GameState.Playing OrElse _state = GameState.Paused Then _previousState = _state : _state = GameState.Options
        End Select
    End Sub

    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)
        Select Case e.Key
            Case Key.Left, Key.A : _leftPressed = False
            Case Key.Right, Key.D : _rightPressed = False
        End Select
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseButtonEventArgs)
        MyBase.OnMouseDown(e)
        If _state = GameState.Playing Then
            AdjustBallSpeed(1.12F) : PlaySFX(_sfxData(_sfxStyle)(10), 60)
            SpawnParticles(LOGICAL_WIDTH / 2, LOGICAL_HEIGHT / 2, C3(255, 200, 50), 6) : Return
        End If
        If _state <> GameState.Options Then Return
        Dim mx = CSng(e.GetPosition(Me).X / (ActualWidth / LOGICAL_WIDTH))
        Dim my = CSng(e.GetPosition(Me).Y / (ActualHeight / LOGICAL_HEIGHT))
        Dim px = CSng((LOGICAL_WIDTH - 780) / 2), py = CSng((LOGICAL_HEIGHT - 600) / 2)
        Dim settingsY = py + 298, barX = px + 260
        For idx = 0 To 6
            Dim itemY = settingsY + idx * 32
            If my >= itemY AndAlso my < itemY + 30 Then
                _settingsSelection = idx
                If mx >= barX Then AdjustSettingRight(idx) Else AdjustSettingLeft(idx)
                Exit For
            End If
        Next
    End Sub

    Private Function KeyToChar(e As KeyEventArgs) As String
        If e.Key >= Key.A AndAlso e.Key <= Key.Z Then Return e.Key.ToString()
        If e.Key >= Key.D0 AndAlso e.Key <= Key.D9 Then Return (CInt(e.Key) - CInt(Key.D0)).ToString()
        If e.Key >= Key.NumPad0 AndAlso e.Key <= Key.NumPad9 Then Return (CInt(e.Key) - CInt(Key.NumPad0)).ToString()
        If e.Key = Key.OemMinus OrElse e.Key = Key.Subtract Then Return "-"
        If e.Key = Key.OemPeriod OrElse e.Key = Key.Decimal Then Return "."
        Return Nothing
    End Function
#End Region

#Region "Game Init"
    Private Sub StartNewGame()
        _score = 0 : _lives = MAX_LIVES : _level = 1 : _combo = 0 : _comboTimer = 0
        _pendingHighScore = False : _highScoreDelayFrames = 0 : _paddleWidth = PADDLE_WIDTH
        _paddleWidthTimer = 0 : _ballRadius = BALL_RADIUS : _speedBoost = False
        _nameInput = "" : _highScoreSaved = False : _getReadyFrames = 0
        SetupLevel() : _state = GameState.Playing : PlaySFX(_sfxData(_sfxStyle)(10), 100)
    End Sub

    Private Sub NextLevel()
        _level += 1 : _combo = 0 : _comboTimer = 0 : _pendingHighScore = False
        _highScoreDelayFrames = 0 : _paddleWidth = PADDLE_WIDTH : _paddleWidthTimer = 0 : _getReadyFrames = 0
        SetupLevel() : _state = GameState.Playing : PlaySFX(_sfxData(_sfxStyle)(10), 100)
    End Sub

    Private Sub SetupLevel()
        _paddleX = (LOGICAL_WIDTH - _paddleWidth) / 2.0F
        _balls.Clear() : _powerUps.Clear() : _particles.Clear()
        Dim b As Ball
        b.X = LOGICAL_WIDTH / 2.0F : b.Y = LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT - _ballRadius - 2
        b.Speed = CSng(INITIAL_BALL_SPEED * Math.Pow(1.05, _level - 1))
        Dim angle = _rng.Next(220, 320) * Math.PI / 180.0
        b.DX = CSng(Math.Cos(angle) * b.Speed) : b.DY = CSng(Math.Sin(angle) * b.Speed)
        If b.DY > 0 Then b.DY = -b.DY
        b.Active = True : _balls.Add(b)
        Dim margin = 20, brickCols = BRICK_COLS
        Dim brickRows = Math.Min(10, BRICK_ROWS + CInt(Math.Floor((_level - 1) / 3.0)))
        Dim brickW = CInt((LOGICAL_WIDTH - 2 * margin - (brickCols - 1) * BRICK_PADDING) / brickCols)
        Dim brickH = CInt(brickW * 22 / 65.0)
        If _colorblindMode Then brickH = Math.Max(brickH, 28)
        Dim palette = If(_colorblindMode, _colorblindColors, _rowColors)
        _bricks.Clear()
        Dim pattern = (_level - 1) Mod 8
        For row = 0 To brickRows - 1
            For col = 0 To brickCols - 1
                Dim skip = False
                Select Case pattern
                    Case 1 : skip = ((row + col) Mod 2 = 1)
                    Case 2 : skip = (Math.Abs(row - brickRows / 2.0) + Math.Abs(col - brickCols / 2.0) > (brickRows + brickCols) / 4.0 + 1)
                    Case 3 : skip = (row > 1 AndAlso row < brickRows - 2 AndAlso col > 1 AndAlso col < brickCols - 2)
                    Case 4 : skip = (row Mod 3 = 1)
                    Case 5 : skip = Not (Math.Abs(row - brickRows \ 2) <= 1 OrElse Math.Abs(col - brickCols \ 2) <= 1)
                    Case 6 : skip = (row > 0 AndAlso row < brickRows - 1 AndAlso col > 0 AndAlso col < brickCols - 1)
                    Case 7 : skip = (_rng.Next(100) < 30)
                End Select
                If skip Then Continue For
                Dim bk As Brick
                bk.Rect = New Rect(margin + col * (brickW + BRICK_PADDING), BRICK_TOP_OFFSET + row * (brickH + BRICK_PADDING), brickW, brickH)
                Dim ci = row Mod palette.Length
                bk.Color1 = palette(ci)(0) : bk.Color2 = palette(ci)(1) : bk.Row = row : bk.Points = (brickRows - row) * 10
                Dim hits = 1
                If _level >= 2 AndAlso row < 2 Then hits = 2
                If _level >= 4 AndAlso row < 4 Then hits = 2
                If _level >= 6 Then hits = Math.Max(hits, If(row < 2, 3, 2))
                If _level >= 9 Then hits = Math.Max(hits, If(row < 3, 3, 2))
                If _level >= 12 Then hits = Math.Max(hits, If(row < 2, 4, 3))
                bk.HitsLeft = hits : bk.Alive = True : _bricks.Add(bk)
            Next
        Next
    End Sub

    Private Sub InitStarField()
        Dim count = 120
        ReDim _starFieldX(count - 1) : ReDim _starFieldY(count - 1) : ReDim _starFieldSpeed(count - 1) : ReDim _starFieldBright(count - 1)
        For i = 0 To count - 1
            _starFieldX(i) = _rng.Next(0, LOGICAL_WIDTH) : _starFieldY(i) = _rng.Next(0, LOGICAL_HEIGHT)
            _starFieldSpeed(i) = 0.2F + CSng(_rng.NextDouble()) * 0.6F : _starFieldBright(i) = _rng.Next(60, 200)
        Next
    End Sub
#End Region

#Region "Update Logic"
    Private Sub UpdatePaddle()
        If _leftPressed Then _paddleX -= PADDLE_SPEED
        If _rightPressed Then _paddleX += PADDLE_SPEED
        _paddleX = Math.Max(0, Math.Min(LOGICAL_WIDTH - _paddleWidth, _paddleX))
    End Sub

    Private Sub UpdateBalls()
        If _getReadyFrames > 0 Then Return
        Dim sm As Single = If(_speedBoost, 2.0F, 1.0F)
        For i = 0 To _balls.Count - 1
            Dim b = _balls(i)
            If Not b.Active Then Continue For
            b.X += b.DX * sm : b.Y += b.DY * sm
            If b.X - _ballRadius <= 0 Then b.X = _ballRadius : b.DX = Math.Abs(b.DX) : PlayWallHit()
            If b.X + _ballRadius >= LOGICAL_WIDTH Then b.X = LOGICAL_WIDTH - _ballRadius : b.DX = -Math.Abs(b.DX) : PlayWallHit()
            If b.Y - _ballRadius <= 0 Then b.Y = _ballRadius : b.DY = Math.Abs(b.DY) : PlayWallHit()
            If b.Y + _ballRadius >= LOGICAL_HEIGHT Then
                b.Active = False : SpawnParticles(b.X, b.Y, Colors.White, 12) : _combo = 0 : _comboTimer = 0 : _balls(i) = b : Continue For
            End If
            Dim pr = New Rect(_paddleX, LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT, _paddleWidth, PADDLE_HEIGHT)
            If b.DY > 0 AndAlso BallHitsRect(b, pr) Then
                b.Y = CSng(pr.Top) - _ballRadius - 1
                Dim hp = Math.Max(0.05F, Math.Min(0.95F, (b.X - _paddleX) / _paddleWidth))
                Dim rad = (150 - hp * 120) * Math.PI / 180.0
                b.DX = CSng(-Math.Cos(rad) * b.Speed) : b.DY = CSng(-Math.Sin(rad) * b.Speed)
                If Math.Abs(b.DY) < 2.0F Then
                    b.DY = -2.0F : Dim ratio = b.Speed / CSng(Math.Sqrt(b.DX * b.DX + b.DY * b.DY)) : b.DX *= ratio : b.DY *= ratio
                End If
                PlayPaddleHit() : SpawnParticles(b.X, b.Y, C3(100, 200, 255), 4)
            End If
            For j = 0 To _bricks.Count - 1
                If Not _bricks(j).Alive Then Continue For
                Dim bk = _bricks(j)
                If BallHitsRect(b, bk.Rect) Then
                    bk.HitsLeft -= 1
                    If bk.HitsLeft <= 0 Then
                        bk.Alive = False : _combo += 1 : _comboTimer = 90 : _score += bk.Points * Math.Min(_combo, 8)
                        SpawnParticles(CSng(bk.Rect.X + bk.Rect.Width / 2), CSng(bk.Rect.Y + bk.Rect.Height / 2), bk.Color1, PARTICLE_COUNT)
                        If _combo >= 2 Then PlayComboSound() Else PlayBrickHit()
                        _screenShake = 3
                        If _rng.Next(100) < Math.Max(20, 54 - _level * 3) Then SpawnPowerUp(CSng(bk.Rect.X + bk.Rect.Width / 2), CSng(bk.Rect.Y + bk.Rect.Height / 2))
                    Else
                        bk.Color1 = C3(200, 200, 200) : bk.Color2 = C3(240, 240, 240) : PlaySFX(400, 30)
                    End If
                    _bricks(j) = bk
                    Dim ol = (b.X + _ballRadius) - CSng(bk.Rect.Left), or2 = CSng(bk.Rect.Right) - (b.X - _ballRadius)
                    Dim ot = (b.Y + _ballRadius) - CSng(bk.Rect.Top), ob2 = CSng(bk.Rect.Bottom) - (b.Y - _ballRadius)
                    If Math.Min(ol, or2) < Math.Min(ot, ob2) Then b.DX = -b.DX Else b.DY = -b.DY
                    Exit For
                End If
            Next
            _balls(i) = b
        Next
        If _balls.Where(Function(bl) bl.Active).Count() = 0 Then
            _lives -= 1 : PlayBallLost() : _screenShake = 10
            If _lives <= 0 Then
                If _score > _highScore Then _highScore = _score
                _nameInput = "" : _highScoreSaved = False : _pendingHighScore = True : _highScoreDelayFrames = 60 : _state = GameState.Paused
            Else
                ResetBall()
            End If
        Else
            _balls.RemoveAll(Function(bl) Not bl.Active)
        End If
    End Sub

    Private Sub UpdatePowerUps()
        For i = _powerUps.Count - 1 To 0 Step -1
            Dim pu = _powerUps(i)
            If Not pu.Active Then _powerUps.RemoveAt(i) : Continue For
            pu.Y += POWERUP_SPEED
            If pu.Y > LOGICAL_HEIGHT Then _powerUps.RemoveAt(i) : Continue For
            Dim pr = New Rect(_paddleX, LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT, _paddleWidth, PADDLE_HEIGHT)
            Dim ur = New Rect(pu.X - POWERUP_SIZE / 2, pu.Y - POWERUP_SIZE / 2, POWERUP_SIZE, POWERUP_SIZE)
            If pr.IntersectsWith(ur) Then
                ApplyPowerUp(pu.PType) : SpawnParticles(pu.X, pu.Y, pu.Color1, 10) : PlayPowerUpSound() : _powerUps.RemoveAt(i) : Continue For
            End If
            _powerUps(i) = pu
        Next
    End Sub

    Private Sub UpdateParticles()
        For i = _particles.Count - 1 To 0 Step -1
            Dim p = _particles(i)
            If Not p.Active Then _particles.RemoveAt(i) : Continue For
            p.X += p.DX : p.Y += p.DY : p.DY += 0.1F : p.Life -= 1
            If p.Life <= 0 Then _particles.RemoveAt(i) : Continue For
            _particles(i) = p
        Next
    End Sub

    Private Sub UpdateTimers()
        If _comboTimer > 0 Then _comboTimer -= 1 : If _comboTimer <= 0 Then _combo = 0
        If _paddleWidthTimer > 0 Then _paddleWidthTimer -= 1 : If _paddleWidthTimer <= 0 Then _paddleWidth = PADDLE_WIDTH
        If _getReadyFrames > 0 Then _getReadyFrames -= 1
    End Sub

    Private Sub UpdateStarField()
        For i = 0 To _starFieldY.Length - 1
            _starFieldY(i) += _starFieldSpeed(i)
            If _starFieldY(i) > LOGICAL_HEIGHT Then _starFieldY(i) = 0 : _starFieldX(i) = _rng.Next(0, LOGICAL_WIDTH)
        Next
    End Sub

    Private Sub CheckLevelComplete()
        If _bricks.All(Function(bk) Not bk.Alive) Then _state = GameState.LevelComplete : PlayLevelWin()
    End Sub
#End Region

#Region "Helpers"
    Private Function BallHitsRect(b As Ball, r As Rect) As Boolean
        Dim cx = Math.Max(r.Left, Math.Min(CDbl(b.X), r.Right)), cy = Math.Max(r.Top, Math.Min(CDbl(b.Y), r.Bottom))
        Dim dx = b.X - cx, dy = b.Y - cy
        Return (dx * dx + dy * dy) <= (_ballRadius * _ballRadius)
    End Function

    Private Sub ResetBall()
        _balls.Clear()
        Dim b As Ball
        b.X = _paddleX + _paddleWidth / 2.0F : b.Y = LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT - _ballRadius - 2
        b.Speed = CSng(INITIAL_BALL_SPEED * Math.Pow(1.05, _level - 1))
        Dim angle = _rng.Next(220, 320) * Math.PI / 180.0
        b.DX = CSng(Math.Cos(angle) * b.Speed) : b.DY = CSng(Math.Sin(angle) * b.Speed)
        If b.DY > 0 Then b.DY = -b.DY
        b.Active = True : _balls.Add(b) : _getReadyFrames = 180
    End Sub

    Private Sub SpawnPowerUp(x As Single, y As Single)
        Dim pu As PowerUp : pu.X = x : pu.Y = y : pu.Active = True
        Select Case _rng.Next(9)
            Case 0 : pu.PType = PowerUpType.BlueBallGrow : pu.Color1 = If(_colorblindMode, C3(0, 114, 178), C3(50, 120, 255)) : pu.Symbol = If(_colorblindMode, "BIG", "+")
            Case 1 : pu.PType = PowerUpType.RedExtraLife : pu.Color1 = If(_colorblindMode, C3(213, 94, 0), C3(255, 60, 60)) : pu.Symbol = If(_colorblindMode, "1UP", ChrW(&H2665))
            Case 2, 3, 4 : pu.PType = PowerUpType.GreenMultiBall : pu.Color1 = If(_colorblindMode, C3(240, 228, 66), C3(50, 220, 100)) : pu.Symbol = "x3"
            Case 5 : pu.PType = PowerUpType.YellowBallShrink : pu.Color1 = If(_colorblindMode, C3(86, 180, 233), C3(255, 220, 50)) : pu.Symbol = If(_colorblindMode, "SML", "-")
            Case 6 : pu.PType = PowerUpType.PurplePaddleMega : pu.Color1 = If(_colorblindMode, C3(148, 0, 211), C3(170, 80, 255)) : pu.Symbol = If(_colorblindMode, "BIG", "x3")
            Case 7 : pu.PType = PowerUpType.OrangeBallSlow : pu.Color1 = If(_colorblindMode, C3(230, 159, 0), C3(255, 150, 60)) : pu.Symbol = If(_colorblindMode, "SLOW", "-")
            Case Else : pu.PType = PowerUpType.PinkBallFast : pu.Color1 = If(_colorblindMode, C3(204, 121, 167), C3(255, 120, 200)) : pu.Symbol = If(_colorblindMode, "FAST", "+")
        End Select
        _powerUps.Add(pu)
    End Sub

    Private Sub ApplyPowerUp(pType As PowerUpType)
        Select Case pType
            Case PowerUpType.BlueBallGrow : _ballRadius = Math.Min(MAX_BALL_RADIUS, _ballRadius + 6)
            Case PowerUpType.RedExtraLife : _lives = Math.Min(MAX_LIVES, _lives + 1)
            Case PowerUpType.GreenMultiBall
                Dim cur = _balls.Where(Function(b) b.Active).ToList()
                If cur.Count > 0 Then
                    Dim src = cur(0)
                    For k = 0 To 7
                        Dim nb As Ball : nb.X = src.X : nb.Y = src.Y : nb.Speed = src.Speed
                        Dim ang = _rng.Next(200, 340) * Math.PI / 180.0
                        nb.DX = CSng(Math.Cos(ang) * nb.Speed) : nb.DY = CSng(Math.Sin(ang) * nb.Speed)
                        If nb.DY > 0 Then nb.DY = -nb.DY
                        nb.Active = True : _balls.Add(nb)
                    Next
                End If
            Case PowerUpType.YellowBallShrink : _ballRadius = Math.Max(MIN_BALL_RADIUS, _ballRadius - 6)
            Case PowerUpType.PurplePaddleMega : _paddleWidth = CInt(PADDLE_WIDTH * 3) : _paddleWidthTimer = 480
            Case PowerUpType.OrangeBallSlow : AdjustBallSpeed(0.85F)
            Case PowerUpType.PinkBallFast : AdjustBallSpeed(1.15F)
        End Select
    End Sub

    Private Sub AdjustBallSpeed(m As Single)
        For i = 0 To _balls.Count - 1
            Dim b = _balls(i) : If Not b.Active Then Continue For
            Dim ns = Math.Max(4.0F, Math.Min(25.0F, b.Speed * m))
            If b.Speed > 0 Then Dim sc = ns / b.Speed : b.DX *= sc : b.DY *= sc
            b.Speed = ns : _balls(i) = b
        Next
    End Sub

    Private Sub SpawnParticles(x As Single, y As Single, c As Color, count As Integer)
        For i = 0 To count - 1
            Dim p As Particle : p.X = x : p.Y = y
            Dim ang = _rng.NextDouble() * Math.PI * 2, spd = 1.5 + _rng.NextDouble() * 3.0
            p.DX = CSng(Math.Cos(ang) * spd) : p.DY = CSng(Math.Sin(ang) * spd)
            p.MaxLife = 20 + _rng.Next(20) : p.Life = p.MaxLife : p.ParticleColor = c
            p.Size = 2 + CSng(_rng.NextDouble()) * 4 : p.Active = True : _particles.Add(p)
        Next
    End Sub

    Private Sub AdjustSettingLeft(idx As Integer)
        Select Case idx
            Case 0 : _sfxVolume = Math.Max(0, _sfxVolume - 5) : UpdateMusicVolume()
            Case 1 : _musicVolume = Math.Max(0, _musicVolume - 5) : UpdateMusicVolume()
            Case 2 : _musicSpeed = Math.Max(10, _musicSpeed - 5) : RegenerateCurrentMusicFile() : ChangeMusic()
            Case 3 : _musicStyle = (_musicStyle - 1 + 10) Mod 10 : ChangeMusic()
            Case 4 : _sfxStyle = (_sfxStyle - 1 + 5) Mod 5
            Case 5 : _colorblindMode = Not _colorblindMode
            Case 6 : ApplyWindowScale((_windowScale - 1 + _windowScaleSizes.Length) Mod _windowScaleSizes.Length)
        End Select
    End Sub

    Private Sub AdjustSettingRight(idx As Integer)
        Select Case idx
            Case 0 : _sfxVolume = Math.Min(100, _sfxVolume + 5) : UpdateMusicVolume()
            Case 1 : _musicVolume = Math.Min(100, _musicVolume + 5) : UpdateMusicVolume()
            Case 2 : _musicSpeed = Math.Min(200, _musicSpeed + 5) : RegenerateCurrentMusicFile() : ChangeMusic()
            Case 3 : _musicStyle = (_musicStyle + 1) Mod 10 : ChangeMusic()
            Case 4 : _sfxStyle = (_sfxStyle + 1) Mod 5
            Case 5 : _colorblindMode = Not _colorblindMode
            Case 6 : ApplyWindowScale((_windowScale + 1) Mod _windowScaleSizes.Length)
        End Select
    End Sub

    Private Sub ApplyWindowScale(ns As Integer)
        _windowScale = ns
        Dim win = Window.GetWindow(Me)
        If win Is Nothing Then Return
        win.Width = _windowScaleSizes(_windowScale)(0) + 16 : win.Height = _windowScaleSizes(_windowScale)(1) + 39
        Dim wa = SystemParameters.WorkArea
        win.Left = (wa.Width - win.Width) / 2 : win.Top = (wa.Height - win.Height) / 2
    End Sub

    Private Sub AddHighScore(name As String, score As Integer)
        Dim rec As New ScoreRecord() : rec.PlayerName = name : rec.PlayerScore = score
        _highScores.Add(rec) : _highScores = _highScores.OrderByDescending(Function(s) s.PlayerScore).Take(10).ToList()
        If _highScores.Count > 0 Then _highScore = _highScores(0).PlayerScore
        SaveHighScores()
    End Sub

    Private Sub LoadHighScores()
        Try
            If Not File.Exists(_highScorePath) Then Return
            Dim list = JsonSerializer.Deserialize(Of List(Of ScoreRecord))(File.ReadAllText(_highScorePath))
            If list IsNot Nothing Then _highScores = list : If _highScores.Count > 0 Then _highScore = _highScores(0).PlayerScore
        Catch : End Try
    End Sub

    Private Sub SaveHighScores()
        Try
            Dim d = Path.GetDirectoryName(_highScorePath)
            If Not String.IsNullOrEmpty(d) AndAlso Not Directory.Exists(d) Then Directory.CreateDirectory(d)
            File.WriteAllText(_highScorePath, JsonSerializer.Serialize(_highScores))
        Catch : End Try
    End Sub
#End Region

#Region "Sound System"
    Private Function GenerateWav(frequency As Integer, durationMs As Integer, volume As Integer) As Byte()
        Const SR As Integer = 22050
        Dim n = CInt(SR * durationMs / 1000.0) : If n < 1 Then n = 1
        Dim fs = 44 + n : Dim w(fs - 1) As Byte
        Encoding.ASCII.GetBytes("RIFF").CopyTo(w, 0) : BitConverter.GetBytes(fs - 8).CopyTo(w, 4)
        Encoding.ASCII.GetBytes("WAVE").CopyTo(w, 8) : Encoding.ASCII.GetBytes("fmt ").CopyTo(w, 12)
        BitConverter.GetBytes(16).CopyTo(w, 16) : BitConverter.GetBytes(CShort(1)).CopyTo(w, 20)
        BitConverter.GetBytes(CShort(1)).CopyTo(w, 22) : BitConverter.GetBytes(SR).CopyTo(w, 24)
        BitConverter.GetBytes(SR).CopyTo(w, 28) : BitConverter.GetBytes(CShort(1)).CopyTo(w, 32)
        BitConverter.GetBytes(CShort(8)).CopyTo(w, 34) : Encoding.ASCII.GetBytes("data").CopyTo(w, 36) : BitConverter.GetBytes(n).CopyTo(w, 40)
        Dim amp = 127.0 * volume / 100.0
        For s = 0 To n - 1
            If frequency <= 0 Then w(44 + s) = 128 : Continue For
            Dim p = SR / CDbl(frequency)
            Dim sv As Double = If((s Mod CInt(Math.Max(1, p))) < CInt(Math.Max(1, p / 2)), amp, -amp)
            Dim env = 1.0, a2 = CInt(n * 0.05), d2 = CInt(n * 0.15)
            If a2 > 0 AndAlso s < a2 Then env = s / CDbl(a2)
            If d2 > 0 AndAlso s > n - d2 Then env = (n - s) / CDbl(d2)
            w(44 + s) = CByte(Math.Max(0, Math.Min(255, CInt(128 + sv * env))))
        Next
        Return w
    End Function

    Private Sub PlaySFX(frequency As Integer, durationMs As Integer)
        Try
            If _sfxVolume <= 0 Then Return
            _lastSfxBuffer = GenerateWav(frequency, durationMs, _sfxVolume)
            PlaySound(_lastSfxBuffer, IntPtr.Zero, SND_ASYNC Or SND_MEMORY)
        Catch : End Try
    End Sub

    Private Sub PlayWallHit()
        PlaySFX(_sfxData(_sfxStyle)(0), _sfxData(_sfxStyle)(1))
    End Sub
    Private Sub PlayPaddleHit()
        PlaySFX(_sfxData(_sfxStyle)(2), _sfxData(_sfxStyle)(3))
    End Sub
    Private Sub PlayBrickHit()
        PlaySFX(_sfxData(_sfxStyle)(4) + _combo * 40, _sfxData(_sfxStyle)(5))
    End Sub
    Private Sub PlayPowerUpSound()
        PlaySFX(_sfxData(_sfxStyle)(6), _sfxData(_sfxStyle)(7))
    End Sub
    Private Sub PlayBallLost()
        PlaySFX(_sfxData(_sfxStyle)(8), _sfxData(_sfxStyle)(9))
    End Sub
    Private Sub PlayLevelWin()
        PlaySFX(_sfxData(_sfxStyle)(10), _sfxData(_sfxStyle)(11))
    End Sub
    Private Sub PlayComboSound()
        PlaySFX(800 + Math.Min(_combo, 10) * 100, 100 + Math.Min(_combo, 6) * 15)
    End Sub

    Private Sub GetMusicData(style As Integer, ByRef freqs() As Integer, ByRef durs() As Integer)
        Select Case style
            Case 0 : freqs = {330,392,494,659,587,494,392,0,330,392,494,659,784,659,494,0,440,523,659,880,784,659,523,0,440,523,659,784,659,523,440,0} : durs = {175,175,175,250,175,175,350,100,175,175,175,250,175,175,350,100,175,175,175,250,175,175,350,100,175,175,175,250,175,175,350,200}
            Case 1 : freqs = {659,659,587,523,587,659,784,659,880,784,659,587,523,587,659,523,659,659,587,523,587,659,784,659,880,784,659,587,523,587,659,0} : durs = {130,130,130,130,130,130,200,130,130,130,130,130,130,130,200,200,130,130,130,130,130,130,200,130,130,130,130,130,130,130,200,150}
            Case 2 : freqs = {659,494,523,587,659,587,523,494,440,440,523,659,587,523,494,523,587,659,523,440,440,0,587,698,880,784,698,659,523,659,587,523} : durs = {200,100,100,200,100,100,200,100,200,100,100,200,100,100,200,100,200,200,200,200,200,200,200,100,200,100,100,200,100,200,100,100}
            Case 3 : freqs = {523,1047,784,659,1047,784,659,0,523,494,440,494,523,659,784,0,523,1047,784,659,1047,784,659,0,523,494,440,494,523,659,784,0} : durs = {160,160,160,160,160,160,300,100,160,160,160,160,160,160,300,100,160,160,160,160,160,160,300,100,160,160,160,160,160,160,300,150}
            Case 4 : freqs = {165,165,165,0,147,147,147,0,131,131,131,0,147,165,196,262,196,165,131,0,165,165,165,0,147,147,147,0,131,165,196,0} : durs = {250,250,250,150,250,250,250,150,250,250,250,150,200,200,200,400,200,200,400,200,250,250,250,150,250,250,250,150,250,250,400,150}
            Case 5 : freqs = {440,523,659,880,831,659,523,440,494,587,698,988,880,698,587,494,440,523,659,880,831,659,523,440,392,440,494,523,494,440,392,0} : durs = {180,180,180,250,180,180,180,250,180,180,180,250,180,180,180,250,180,180,180,250,180,180,180,250,180,180,180,250,180,180,350,150}
            Case 6 : freqs = {165,0,175,0,196,0,220,0,196,175,165,0,147,0,131,0,165,0,175,0,196,0,220,0,247,220,196,0,175,165,147,0} : durs = {300,150,300,150,300,150,300,150,200,200,400,200,300,150,400,200,300,150,300,150,300,150,300,150,200,200,400,200,200,200,400,150}
            Case 7 : freqs = {523,659,784,1047,784,659,523,0,587,698,880,1175,880,698,587,0,523,659,784,1047,784,659,523,0,587,698,880,784,659,523,440,0} : durs = {140,140,140,200,140,140,280,100,140,140,140,200,140,140,280,100,140,140,140,200,140,140,280,100,140,140,140,200,140,140,280,150}
            Case 8 : freqs = {330,330,349,392,392,349,330,294,262,262,294,330,330,294,294,0,330,330,349,392,392,349,330,294,262,262,294,330,294,262,262,0} : durs = {170,170,170,250,170,170,170,250,170,170,170,250,250,170,350,100,170,170,170,250,170,170,170,250,170,170,170,250,250,170,350,150}
            Case Else : freqs = {220,262,330,440,392,330,262,220,247,294,349,494,440,349,294,247,220,262,330,440,392,330,262,220,196,220,247,262,247,220,196,0} : durs = {190,190,190,250,190,190,190,250,190,190,190,250,190,190,190,250,190,190,190,250,190,190,190,250,190,190,190,250,190,190,350,150}
        End Select
    End Sub

    Private Function GenerateMidiBytes() As Byte()
        Dim midi As New List(Of Byte)
        Dim instruments() = {73, 80, 10, 81, 38, 19, 88, 80, 29, 27}
        Dim bpms() = {50, 60, 54, 56, 36, 44, 29, 58, 56, 48}
        Dim inst = instruments(Math.Min(_musicStyle, 9))
        Dim bpm = Math.Max(1, CInt(bpms(Math.Min(_musicStyle, 9)) * _musicSpeed / 100.0))
        Dim usPerQN = CInt(60000000.0 / bpm), tpq = 480
        Dim freqs() As Integer = Nothing, durs() As Integer = Nothing
        GetMusicData(_musicStyle, freqs, durs)
        Dim n = freqs.Length, f2(2 * n - 1) As Integer, d2(2 * n - 1) As Integer
        Array.Copy(freqs, 0, f2, 0, n) : Array.Copy(durs, 0, d2, 0, n)
        Array.Copy(freqs, 0, f2, n, n) : Array.Copy(durs, 0, d2, n, n)
        freqs = f2 : durs = d2
        Dim trk As New List(Of Byte)
        MidiVL(trk, 0) : trk.Add(&HFF) : trk.Add(&H51) : trk.Add(3)
        trk.Add(CByte((usPerQN >> 16) And &HFF)) : trk.Add(CByte((usPerQN >> 8) And &HFF)) : trk.Add(CByte(usPerQN And &HFF))
        MidiVL(trk, 0) : trk.Add(&HB0) : trk.Add(7) : trk.Add(CByte(Math.Min(127, CInt(_musicVolume * 1.27))))
        MidiVL(trk, 0) : trk.Add(&HC0) : trk.Add(CByte(inst))
        Dim pd = 0
        For i = 0 To freqs.Length - 1
            Dim ticks = CInt(durs(i) * tpq * bpm / 60000.0) : If ticks < 1 Then ticks = 1
            If freqs(i) <= 0 Then pd += ticks : Continue For
            Dim nt = CInt(Math.Max(0, Math.Min(127, Math.Round(69.0 + 12.0 * Math.Log(freqs(i) / 440.0) / Math.Log(2.0)))))
            MidiVL(trk, pd) : trk.Add(&H90) : trk.Add(CByte(nt)) : trk.Add(100)
            Dim onT = CInt(ticks * 0.9) : If onT < 1 Then onT = 1
            MidiVL(trk, onT) : trk.Add(&H80) : trk.Add(CByte(nt)) : trk.Add(0)
            pd = ticks - onT
        Next
        MidiVL(trk, pd) : trk.Add(&HFF) : trk.Add(&H2F) : trk.Add(0)
        midi.AddRange(Encoding.ASCII.GetBytes("MThd"))
        BE32(midi, 6) : BE16(midi, 0) : BE16(midi, 1) : BE16(midi, tpq)
        midi.AddRange(Encoding.ASCII.GetBytes("MTrk"))
        BE32(midi, trk.Count) : midi.AddRange(trk) : Return midi.ToArray()
    End Function

    Private Sub MidiVL(d As List(Of Byte), v As Integer)
        If v < 0 Then v = 0
        If v < &H80 Then
            d.Add(CByte(v))
        ElseIf v < &H4000 Then
            d.Add(CByte((v >> 7) Or &H80))
            d.Add(CByte(v And &H7F))
        ElseIf v < &H200000 Then
            d.Add(CByte((v >> 14) Or &H80))
            d.Add(CByte(((v >> 7) And &H7F) Or &H80))
            d.Add(CByte(v And &H7F))
        Else
            d.Add(CByte((v >> 21) Or &H80))
            d.Add(CByte(((v >> 14) And &H7F) Or &H80))
            d.Add(CByte(((v >> 7) And &H7F) Or &H80))
            d.Add(CByte(v And &H7F))
        End If
    End Sub
    Private Sub BE32(d As List(Of Byte), v As Integer)
        d.Add(CByte((v >> 24) And &HFF))
        d.Add(CByte((v >> 16) And &HFF))
        d.Add(CByte((v >> 8) And &HFF))
        d.Add(CByte(v And &HFF))
    End Sub
    Private Sub BE16(d As List(Of Byte), v As Integer)
        d.Add(CByte((v >> 8) And &HFF))
        d.Add(CByte(v And &HFF))
    End Sub

    Private Sub PreGenerateAllMusic()
        Try
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_wpf_music")
            If Not Directory.Exists(tmpDir) Then Directory.CreateDirectory(tmpDir)
            ReDim _musicFiles(9)
            For i = 0 To 9
                _musicFiles(i) = Path.Combine(tmpDir, $"style_{i}.mid")
                Dim old = _musicStyle : _musicStyle = i : File.WriteAllBytes(_musicFiles(i), GenerateMidiBytes()) : _musicStyle = old
            Next
            _musicTempFile = _musicFiles(_musicStyle)
        Catch : End Try
    End Sub

    Private Sub RegenerateCurrentMusicFile()
        Try
            If _musicFiles Is Nothing OrElse _musicStyle < 0 OrElse _musicStyle >= _musicFiles.Length Then Return
            Dim p = _musicFiles(_musicStyle) : If String.IsNullOrEmpty(p) Then Return
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            File.WriteAllBytes(p, GenerateMidiBytes()) : _musicTempFile = p
        Catch : End Try
    End Sub

    Private Sub StartMusic()
        Try
            If _musicFiles Is Nothing Then Return
            _musicTempFile = _musicFiles(_musicStyle)
            If String.IsNullOrEmpty(_musicTempFile) OrElse Not File.Exists(_musicTempFile) Then Return
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("open """ & _musicTempFile & """ alias bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("setaudio bgmusic volume to " & CInt(GetEffectiveMusicVolume() * 10).ToString(), Nothing, 0, IntPtr.Zero)
            mciSendString("play bgmusic notify", Nothing, 0, _hwnd) : _musicPlaying = True
        Catch : End Try
    End Sub

    Private Sub StartMusicDirect()
        Try
            If _musicFiles Is Nothing Then Return
            _musicTempFile = _musicFiles(_musicStyle)
            If String.IsNullOrEmpty(_musicTempFile) OrElse Not File.Exists(_musicTempFile) Then Return
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("open """ & _musicTempFile & """ alias bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("setaudio bgmusic volume to " & CInt(GetEffectiveMusicVolume() * 10).ToString(), Nothing, 0, IntPtr.Zero)
            mciSendString("play bgmusic notify", Nothing, 0, _hwnd) : _musicPlaying = True
        Catch : End Try
    End Sub

    Private Sub ScheduleMusicStart(delayMs As Integer)
        If _musicChangeTimer IsNot Nothing Then _musicChangeTimer.Stop()
        _musicChangeTimer = New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(Math.Max(10, delayMs))}
        AddHandler _musicChangeTimer.Tick, Sub(s, ev)
                                                 _musicChangeTimer.Stop()
                                                 _musicChangeTimer = Nothing
                                                 StartMusicDirect()
                                             End Sub
        _musicChangeTimer.Start()
    End Sub

    Private Sub ScheduleHighScoreMusicStart(delayMs As Integer)
        If _musicChangeTimer IsNot Nothing Then _musicChangeTimer.Stop()
        _musicChangeTimer = New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(Math.Max(10, delayMs))}
        AddHandler _musicChangeTimer.Tick, Sub(s, ev)
                                                 _musicChangeTimer.Stop()
                                                 _musicChangeTimer = Nothing
                                                 StartHighScoreMusic()
                                             End Sub
        _musicChangeTimer.Start()
    End Sub

    Private Sub StartHighScoreMusic()
        Try
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_wpf_music")
            If Not Directory.Exists(tmpDir) Then Directory.CreateDirectory(tmpDir)
            _highScoreMusicFile = Path.Combine(tmpDir, "highscore.mid")
            Dim old = _musicStyle : _musicStyle = 6 : File.WriteAllBytes(_highScoreMusicFile, GenerateMidiBytes()) : _musicStyle = old
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("open """ & _highScoreMusicFile & """ alias bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("setaudio bgmusic volume to " & CInt(GetEffectiveMusicVolume() * 10).ToString(), Nothing, 0, IntPtr.Zero)
            mciSendString("play bgmusic notify", Nothing, 0, _hwnd) : _musicPlaying = True : _usingHighScoreMusic = True
        Catch : End Try
    End Sub

    Private Sub UpdateMusicVolume()
        Try : mciSendString("setaudio bgmusic volume to " & CInt(GetEffectiveMusicVolume() * 10).ToString(), Nothing, 0, IntPtr.Zero) : Catch : End Try
    End Sub

    Private Function GetEffectiveMusicVolume() As Integer
        Return Math.Max(0, Math.Min(_musicVolume, 100))
    End Function

    Private Sub ChangeMusic()
        ScheduleMusicStart(60)
    End Sub

    Private Sub CleanupMusic()
        Try
            mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero) : mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero) : _musicPlaying = False
            If _musicFiles IsNot Nothing Then
                For Each f In _musicFiles
                    If Not String.IsNullOrEmpty(f) AndAlso File.Exists(f) Then Try : File.Delete(f) : Catch : End Try
                Next
            End If
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_wpf_music")
            If Directory.Exists(tmpDir) Then Try : Directory.Delete(tmpDir, True) : Catch : End Try
        Catch : End Try
    End Sub
#End Region

End Class
End Namespace
