' =============================================================================
' CURTIS LOOP: BRICK BLAST � WPF Version
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
Imports System.Windows.Media.Imaging
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
            If msg = MM_MCINOTIFY AndAlso wParam.ToInt32() = MCI_NOTIFY_SUCCESSFUL AndAlso _musicPlaying Then
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

        ' ?? Asset Pipeline Systems ??
        Private _assetMgr As AssetManager
        Private _tileMap As TileMap
        Private _enemyMgr As EnemyManager
        Private _inventory As InventorySystem
        Private _uiMgr As UIManager
        Private _menuBackground As BitmapSource
        Private _menuMascot As BitmapSource
        Private _gameBackground As BitmapSource
        Private _textGameOver As BitmapSource
        Private _textGetReady As BitmapSource
        Private _textYouWin As BitmapSource
        Private _textPaused As BitmapSource
        Private _hudHeart As BitmapSource
        Private _hudStar As BitmapSource
        Private _hudTrophy As BitmapSource
        Private ReadOnly _powerUpSpriteMap As New Dictionary(Of PowerUpType, String) From {
        {PowerUpType.RedExtraLife, "UI/powerup_life"},
        {PowerUpType.BlueBallGrow, "UI/powerup_grow"},
        {PowerUpType.GreenMultiBall, "UI/powerup_multi"},
        {PowerUpType.YellowBallShrink, "UI/powerup_shrink"},
        {PowerUpType.PurplePaddleMega, "UI/powerup_mega"},
        {PowerUpType.OrangeBallSlow, "UI/powerup_slow"},
        {PowerUpType.PinkBallFast, "UI/powerup_fast"}}
        Private ReadOnly _ballColorKeys As String() = {"Sprites/ball_blue", "Sprites/ball_green", "Sprites/ball_purple", "Sprites/ball_red"}
        ' Alternate brick set keys (OGA Full Kit BRICK/ folder)
        Private ReadOnly _brick2Colors As String() = {"Sprites/brick2_red", "Sprites/brick2_brown", "Sprites/brick2_blue", "Sprites/brick2_green", "Sprites/brick2_pink", "Sprites/brick2_purple", "Sprites/brick2_lightblue", "Sprites/brick2_metal"}
        ' Colored paddle keys that rotate by level
        Private ReadOnly _paddleColorKeys As String() = {"Sprites/paddle_hd_blue", "Sprites/paddle_hd_green", "Sprites/paddle_hd_purple", "Sprites/paddle_hd_red"}
        ' Sized paddle variants (OGA Full Kit)
        Private ReadOnly _paddleLargeKeys As String() = {"Sprites/paddle_blue_large", "Sprites/paddle_green_large", "Sprites/paddle_purple_large", "Sprites/paddle_red_large"}
        Private ReadOnly _paddleSmallKeys As String() = {"Sprites/paddle_blue_small", "Sprites/paddle_green_small", "Sprites/paddle_purple_small", "Sprites/paddle_red_small"}
        Private ReadOnly _paddleMedKeys As String() = {"Sprites/paddle_blue_med", "Sprites/paddle_green_med", "Sprites/paddle_purple_med", "Sprites/paddle_red_med"}
        ' Retro background for alternate levels
        Private _retroBackground As BitmapSource
        ' Pixel-art button backgrounds per settings row color
        Private ReadOnly _pixelBtnKeys As String() = {"UI/pixel_blue", "UI/pixel_green", "UI/pixel_red", "UI/pixel_yellow", "UI/pixel_brown", "UI/pixel_tan", "UI/pixel_grey"}
        Private ReadOnly _pixelBtnPressedKeys As String() = {"UI/pixel_blue_pressed", "UI/pixel_green_pressed", "UI/pixel_red_pressed", "UI/pixel_yellow_pressed", "UI/pixel_brown_pressed", "UI/pixel_tan_pressed", "UI/pixel_grey_pressed"}
#End Region

#Region "Constructor"
        Public Sub New()
            Focusable = True
            InitStarField()
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
            _state = GameState.Menu
            LoadHighScores()
            PreGenerateAllMusic()
            _gameTimer = New DispatcherTimer()
            _gameTimer.Interval = TimeSpan.FromMilliseconds(16)
            AddHandler _gameTimer.Tick, AddressOf GameTimer_Tick
            _gameTimer.Start()
            StartMusic()
            InitializeAssetSystems()
            Focus()
        End Sub

        Private Sub OnUnloaded()
            If _gameTimer IsNot Nothing Then _gameTimer.Stop()
            CleanupMusic()
        End Sub

        Private Sub InitializeAssetSystems()
            _assetMgr = AssetManager.Instance
            Dim assetsPath = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets")
            _assetMgr.Initialize(assetsPath)

            ' Log asset path for debugging
            Dim dbg = Sub(m As String) System.Diagnostics.Debug.WriteLine("[ASSETS] " & m)
            dbg($"Base path: {assetsPath}")
            dbg($"Sprites dir exists: {IO.Directory.Exists(IO.Path.Combine(assetsPath, "Sprites"))}")
            dbg($"UI dir exists: {IO.Directory.Exists(IO.Path.Combine(assetsPath, "UI"))}")
            dbg($"Tiles dir exists: {IO.Directory.Exists(IO.Path.Combine(assetsPath, "Tiles"))}")
            If IO.Directory.Exists(IO.Path.Combine(assetsPath, "Sprites")) Then
                Dim spriteFiles = IO.Directory.GetFiles(IO.Path.Combine(assetsPath, "Sprites"), "*.png")
                dbg($"Sprites on disk: {spriteFiles.Length} files")
                For Each f In spriteFiles.Take(5) : dbg($"  {IO.Path.GetFileName(f)}") : Next
            End If

            ' Import any SuperGameAsset downloads (safe no-op if folder absent)
            Dim report = AssetImporter.RunImport(assetsPath)
            For Each line In report.Log
                System.Diagnostics.Debug.WriteLine(line)
            Next

            ' Register procedural fallbacks AFTER import so disk files take priority
            ProceduralAssets.RegisterDefaults(_assetMgr)

            ' Verify sprite loading
            Dim testBrick = _assetMgr.GetSprite("Sprites/brick_0")
            Dim testBall = _assetMgr.GetSprite("Sprites/ball")
            Dim testPaddle = _assetMgr.GetSprite("Sprites/paddle")
            Dim testHeart = _assetMgr.GetSprite("UI/heart")
            Dim testGameBg = _assetMgr.GetSprite("Tiles/game_background")
            dbg($"brick_0: {If(testBrick IsNot Nothing, $"{testBrick.PixelWidth}x{testBrick.PixelHeight}", "NULL")}")
            dbg($"ball: {If(testBall IsNot Nothing, $"{testBall.PixelWidth}x{testBall.PixelHeight}", "NULL")}")
            dbg($"paddle: {If(testPaddle IsNot Nothing, $"{testPaddle.PixelWidth}x{testPaddle.PixelHeight}", "NULL")}")
            dbg($"heart: {If(testHeart IsNot Nothing, $"{testHeart.PixelWidth}x{testHeart.PixelHeight}", "NULL")}")
            dbg($"game_background: {If(testGameBg IsNot Nothing, $"{testGameBg.PixelWidth}x{testGameBg.PixelHeight}", "NULL")}")

            _tileMap = New TileMap(24, 17, 50, 51)
            _tileMap.LoadTileSprites(_assetMgr)
            _tileMap.LoadFromPattern(_level)
            _enemyMgr = New EnemyManager()
            _inventory = New InventorySystem(8)
            _uiMgr = New UIManager()
            _uiMgr.AddHealthBar("enemy_progress", 15, 35, 120, 8, Color.FromRgb(255, 100, 80))

            ' Cache menu assets (may be real or procedural)
            _menuBackground = _assetMgr.GetSprite("Tiles/menu_background")
            _menuMascot = _assetMgr.GetSprite("Characters/menu_mascot")

            ' Cache gameplay assets from downloaded packs
            _gameBackground = _assetMgr.GetSprite("Tiles/game_background")
            _textGameOver = _assetMgr.GetSprite("UI/text_gameover")
            _textGetReady = _assetMgr.GetSprite("UI/text_getready")
            _textYouWin = _assetMgr.GetSprite("UI/text_youwin")
            _textPaused = _assetMgr.GetSprite("UI/text_resume")
            _hudHeart = _assetMgr.GetSprite("UI/heart")
            _hudStar = _assetMgr.GetSprite("UI/star")
            _hudTrophy = _assetMgr.GetSprite("UI/trophy")
            _retroBackground = _assetMgr.GetSprite("Tiles/retro_background")
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
                If _enemyMgr IsNot Nothing Then _enemyMgr.Update(_paddleX + _paddleWidth / 2.0F, CSng(LOGICAL_HEIGHT - PADDLE_Y_OFFSET))
                CheckEnemyBallCollisions()
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
            ' Background art from SuperGameAsset (darkened overlay)
            If _menuBackground IsNot Nothing Then
                dc.PushOpacity(0.25)
                dc.DrawImage(_menuBackground, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
                dc.Pop()
            End If
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
            ' Faint retro spritesheet decoration left-edge
            Dim retroSheet = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("Sprites/retro_spritesheet"), Nothing)
            If retroSheet IsNot Nothing Then
                dc.PushOpacity(0.06) : dc.DrawImage(retroSheet, New Rect(0, 80, 200, 200)) : dc.Pop()
            End If
            Dim playerIcnM = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/player_icon"), Nothing)
            Dim multiIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/multiplayer_icon"), Nothing)
            If playerIcnM IsNot Nothing Then dc.DrawImage(playerIcnM, New Rect(20, 20, 26, 26))
            If multiIcn IsNot Nothing Then dc.DrawImage(multiIcn, New Rect(LOGICAL_WIDTH - 46, 20, 26, 26))
            Dim textMenu = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/text_menu"), Nothing)
            If textMenu IsNot Nothing Then
                Dim tmw = Math.Min(100.0, CDbl(textMenu.Width) * 0.35), tmh = tmw * textMenu.Height / textMenu.Width
                dc.DrawImage(textMenu, New Rect((LOGICAL_WIDTH - tmw) / 2, 114, tmw, tmh))
            End If
            ' Start button row: green bg + button_start icon + text_start sprite + space icon
            Dim btnGreen = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_green"), Nothing)
            Dim btnStartIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_start"), Nothing)
            Dim kSpaceIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_space_icon"), Nothing)
            If btnGreen IsNot Nothing Then
                dc.PushOpacity(0.45) : dc.DrawImage(btnGreen, New Rect((LOGICAL_WIDTH - 260) / 2, 298, 260, 36)) : dc.Pop()
            End If
            If btnStartIcn IsNot Nothing Then dc.DrawImage(btnStartIcn, New Rect(LOGICAL_WIDTH / 2.0 - 140, 303, 22, 22))
            Dim textStart = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/text_start"), Nothing)
            If textStart IsNot Nothing Then
                Dim sw = Math.Min(220.0, textStart.Width * 0.5), sh = sw * textStart.Height / textStart.Width
                dc.DrawImage(textStart, New Rect((LOGICAL_WIDTH - sw) / 2, 305, sw, sh))
            Else
                DrawCT(dc, "Press SPACE to Start", 18, Colors.White, 310)
            End If
            If kSpaceIcn IsNot Nothing Then dc.DrawImage(kSpaceIcn, New Rect(LOGICAL_WIDTH / 2.0 + 118, 307, 22, 14))
            Dim pixSpace = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_space"), Nothing)
            Dim pixSpaceInly = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_space_inlay"), Nothing)
            If pixSpace IsNot Nothing Then
                dc.PushOpacity(0.12) : dc.DrawImage(pixSpace, New Rect((LOGICAL_WIDTH - 80) / 2, 338, 80, 12)) : dc.Pop()
            End If
            If pixSpaceInly IsNot Nothing Then
                dc.PushOpacity(0.08) : dc.DrawImage(pixSpaceInly, New Rect((LOGICAL_WIDTH - 80) / 2, 338, 80, 12)) : dc.Pop()
            End If
            If _highScore > 0 Then
                If _hudTrophy IsNot Nothing Then dc.DrawImage(_hudTrophy, New Rect(LOGICAL_WIDTH / 2.0 - 130, 344, 20, 20))
                DrawCT(dc, $"High Score: {_highScore}", 14, C3(255, 220, 100), 350)
                Dim unlockIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/unlocked"), Nothing)
                If unlockIcn IsNot Nothing Then dc.DrawImage(unlockIcn, New Rect(LOGICAL_WIDTH / 2.0 + 112, 344, 16, 16))
            End If
            ' Options row: yellow bg + gear icons + O key
            Dim btnYellow = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_yellow"), Nothing)
            Dim gearIcon = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/gear"), Nothing)
            Dim kO = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_o"), Nothing)
            If btnYellow IsNot Nothing Then
                dc.PushOpacity(0.4) : dc.DrawImage(btnYellow, New Rect((LOGICAL_WIDTH - 300) / 2, 378, 300, 32)) : dc.Pop()
            End If
            If gearIcon IsNot Nothing Then
                dc.DrawImage(gearIcon, New Rect(LOGICAL_WIDTH / 2.0 - 145, 383, 18, 18))
                dc.DrawImage(gearIcon, New Rect(LOGICAL_WIDTH / 2.0 + 127, 383, 18, 18))
            End If
            If kO IsNot Nothing Then dc.DrawImage(kO, New Rect(LOGICAL_WIDTH / 2.0 - 122, 383, 18, 18))
            DrawCT(dc, "  Press H or O for OPTIONS  ", 14, C3(100, 200, 255), 385, True)
            ' Controls row with individual key sprites
            Dim kArrows = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrows"), Nothing)
            Dim kArrL = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrow_left"), Nothing)
            Dim kArrR = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrow_right"), Nothing)
            Dim kF = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_f"), Nothing)
            Dim kP = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_p"), Nothing)
            Dim kEsc = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_escape"), Nothing)
            Dim kSzM = 19.0 : Dim rowYM = 428.0
            If kArrows IsNot Nothing Then dc.DrawImage(kArrows, New Rect(55, rowYM, kSzM, kSzM))
            If kArrL IsNot Nothing Then dc.DrawImage(kArrL, New Rect(78, rowYM + 4, kSzM * 0.65, kSzM * 0.65))
            If kArrR IsNot Nothing Then dc.DrawImage(kArrR, New Rect(94, rowYM + 4, kSzM * 0.65, kSzM * 0.65))
            dc.DrawText(MkText("Move", 10, C3(150, 150, 170)), New Point(116, rowYM + 5))
            If kF IsNot Nothing Then dc.DrawImage(kF, New Rect(210, rowYM, kSzM, kSzM))
            dc.DrawText(MkText("Speed", 10, C3(150, 150, 170)), New Point(232, rowYM + 5))
            If kP IsNot Nothing Then dc.DrawImage(kP, New Rect(310, rowYM, kSzM, kSzM))
            If kEsc IsNot Nothing Then dc.DrawImage(kEsc, New Rect(332, rowYM, kSzM + 4, kSzM))
            dc.DrawText(MkText("Pause", 10, C3(150, 150, 170)), New Point(360, rowYM + 5))
            ' Status/info lines
            Dim infoIcnM = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/info"), Nothing)
            Dim movieIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/movie"), Nothing)
            Dim lockedIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/locked"), Nothing)
            Dim homeIcnM = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/home"), Nothing)
            Dim btnRound = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_round"), Nothing)
            If infoIcnM IsNot Nothing Then dc.DrawImage(infoIcnM, New Rect(18, 457, 14, 14))
            DrawCT(dc, $"Music: {_musicStyleNames(_musicStyle)}  |  SFX: {_sfxStyleNames(_sfxStyle)}", 11, C3(120, 140, 180), 460)
            If movieIcn IsNot Nothing Then dc.DrawImage(movieIcn, New Rect(18, 487, 14, 14))
            DrawCT(dc, $"Window: {_windowScaleNames(_windowScale)}", 11, C3(120, 140, 180), 490)
            Dim btnBlue = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_blue"), Nothing)
            If btnBlue IsNot Nothing Then
                dc.PushOpacity(0.3) : dc.DrawImage(btnBlue, New Rect((LOGICAL_WIDTH - 380) / 2, 516, 380, 24)) : dc.Pop()
            End If
            If lockedIcn IsNot Nothing Then dc.DrawImage(lockedIcn, New Rect(18, 517, 14, 14))
            DrawCT(dc, "Destroy bricks " & ChrW(8226) & " Catch power-ups " & ChrW(8226) & " Build combos!", 11, C3(150, 150, 170), 520)
            If homeIcnM IsNot Nothing Then dc.DrawImage(homeIcnM, New Rect(16, LOGICAL_HEIGHT - 46, 20, 20))
            If btnRound IsNot Nothing Then
                dc.PushOpacity(0.2) : dc.DrawImage(btnRound, New Rect(LOGICAL_WIDTH - 46, LOGICAL_HEIGHT - 46, 30, 30)) : dc.Pop()
            End If
            ' Mascot from SuperGameAsset (bottom-right, subtle)
            If _menuMascot IsNot Nothing Then
                dc.PushOpacity(0.7)
                dc.DrawImage(_menuMascot, New Rect(LOGICAL_WIDTH - 180, LOGICAL_HEIGHT - 280, 160, 240))
                dc.Pop()
            End If
        End Sub

        Private Sub DrawGame(dc As DrawingContext)
            ' Retro background every 3rd level, game_background otherwise
            Dim bgSprite = If(_level Mod 3 = 0 AndAlso _retroBackground IsNot Nothing, _retroBackground, _gameBackground)
            If bgSprite IsNot Nothing Then
                dc.PushOpacity(0.35)
                dc.DrawImage(bgSprite, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
                dc.Pop()
            End If
            If _tileMap IsNot Nothing Then _tileMap.Render(dc, 0, 0)
            DrawHUD(dc) : DrawBricks(dc) : DrawPaddle(dc) : DrawBalls(dc) : DrawPowerUps(dc) : DrawParticles(dc) : DrawCombo(dc) : DrawGetReady(dc)
            If _enemyMgr IsNot Nothing Then _enemyMgr.Render(dc, _assetMgr, _frameCount, _colorblindMode)
            If _inventory IsNot Nothing AndAlso _inventory.Count > 0 Then _inventory.Render(dc, _assetMgr, CSng((LOGICAL_WIDTH - 8 * 42) / 2.0), CSng(LOGICAL_HEIGHT - 18), 36, _dpi)
            If _uiMgr IsNot Nothing Then _uiMgr.Render(dc, _assetMgr)
        End Sub

        Private Sub DrawHUD(dc As DrawingContext)
            Dim playerIcnHUD = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/player_icon"), Nothing)
            If playerIcnHUD IsNot Nothing Then dc.DrawImage(playerIcnHUD, New Rect(12, 9, 16, 16))
            dc.DrawText(MkText($"SCORE: {_score}", 13, Colors.White, True), New Point(If(playerIcnHUD IsNot Nothing, 32, 15), 12))
            DrawCT(dc, $"LEVEL {_level}", 13, C3(180, 200, 255), 12, True)
            Dim pauseIcnHUD = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pause"), Nothing)
            If pauseIcnHUD IsNot Nothing Then dc.DrawImage(pauseIcnHUD, New Rect(LOGICAL_WIDTH / 2.0 + 70, 9, 14, 14))
            Dim bonusLife = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/bonus_extra_life"), Nothing)
            If _hudHeart IsNot Nothing Then
                Dim hx As Double = LOGICAL_WIDTH - 15
                If _lives >= MAX_LIVES AndAlso bonusLife IsNot Nothing Then
                    hx -= 20 : dc.DrawImage(bonusLife, New Rect(hx, 6, 18, 18))
                End If
                For i = 0 To _lives - 1
                    hx -= 18 : dc.DrawImage(_hudHeart, New Rect(hx, 8, 16, 16))
                Next
            Else
                Dim lt = MkText($"LIVES: {New String(ChrW(&H2665), _lives)}", 13, C3(255, 100, 130), True)
                dc.DrawText(lt, New Point(LOGICAL_WIDTH - lt.Width - 15, 12))
            End If
            If _speedBoost Then
                Dim powerIcnHUD = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/power_icon"), Nothing)
                Dim targetIcnHUD = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/target"), Nothing)
                If powerIcnHUD IsNot Nothing Then dc.DrawImage(powerIcnHUD, New Rect(12, 30, 14, 14))
                If targetIcnHUD IsNot Nothing Then dc.DrawImage(targetIcnHUD, New Rect(28, 30, 14, 14))
                dc.DrawText(MkText("2x SPEED", 11, C3(255, 200, 50), True), New Point(If(targetIcnHUD IsNot Nothing, 46, 15), 32))
            End If
            If _ballRadius <> BALL_RADIUS Then
                Dim sizeIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite(If(_ballRadius > BALL_RADIUS, "UI/plus", "UI/minus")), Nothing)
                Dim bx As Double = If(_speedBoost, 115, 12)
                If sizeIcn IsNot Nothing Then dc.DrawImage(sizeIcn, New Rect(bx, 30, 14, 14))
                dc.DrawText(MkText($"Ball: {_ballRadius}px", 11, C3(150, 200, 255), True), New Point(bx + If(sizeIcn IsNot Nothing, 16, 0), 32))
            End If
            If _paddleWidthTimer > 0 Then
                Dim pt = MkText($"Paddle: {CInt(Math.Ceiling(_paddleWidthTimer / 60.0))}s", 13, C3(170, 80, 255), True)
                dc.DrawText(pt, New Point(LOGICAL_WIDTH - pt.Width - 15, 32))
            End If
            dc.DrawLine(New Pen(SB(C4(40, 100, 180, 255)), 1), New Point(0, 50), New Point(LOGICAL_WIDTH, 50))
        End Sub

        Private Sub DrawBricks(dc As DrawingContext)
            For Each bk In _bricks
                If Not bk.Alive Then Continue For
                Dim brickIdx = bk.Row Mod 7
                Dim brickSprite As BitmapSource = Nothing
                If _assetMgr IsNot Nothing Then
                    ' High-hit bricks use special sprites (gold for 4+, dark for 3, grey for 2)
                    If bk.HitsLeft >= 4 Then
                        brickSprite = _assetMgr.GetSprite("Sprites/brick_gold")
                    ElseIf bk.HitsLeft = 3 Then
                        brickSprite = _assetMgr.GetSprite("Sprites/brick_dark")
                    ElseIf bk.HitsLeft = 2 Then
                        brickSprite = _assetMgr.GetSprite("Sprites/brick_grey")
                    End If
                    ' Levels 5+ use the alternate brick2 set from OGA Full Kit
                    If brickSprite Is Nothing AndAlso _level >= 5 Then
                        Dim altIdx = brickIdx Mod _brick2Colors.Length
                        brickSprite = _assetMgr.GetSprite(_brick2Colors(altIdx))
                    End If
                    ' Standard brick: try damaged variant first, then undamaged
                    If brickSprite Is Nothing Then
                        Dim brickKey = If(bk.HitsLeft <= 1, $"Sprites/brick_{brickIdx}_damaged", $"Sprites/brick_{brickIdx}")
                        brickSprite = _assetMgr.GetSprite(brickKey)
                        If brickSprite Is Nothing AndAlso bk.HitsLeft <= 1 Then
                            brickSprite = _assetMgr.GetSprite($"Sprites/brick_{brickIdx}")
                        End If
                    End If
                End If
                If brickSprite IsNot Nothing Then
                    dc.DrawImage(brickSprite, bk.Rect)
                    ' Hit-count overlay for multi-hit bricks
                    If bk.HitsLeft > 1 Then
                        dc.DrawRectangle(SB(C4(CInt(60 * (bk.HitsLeft - 1)), 0, 0, 0)), Nothing, bk.Rect)
                        Dim ht = MkText(bk.HitsLeft.ToString(), 9, Colors.White, True)
                        dc.DrawText(ht, New Point(bk.Rect.X + (bk.Rect.Width - ht.Width) / 2, bk.Rect.Y + (bk.Rect.Height - ht.Height) / 2))
                    End If
                Else
                    ' Procedural fallback: gradient rectangle
                    dc.DrawRoundedRectangle(New LinearGradientBrush(bk.Color1, bk.Color2, 90.0), Nothing, bk.Rect, 4, 4)
                    dc.DrawRectangle(SB(C4(50, 255, 255, 255)), Nothing, New Rect(bk.Rect.X + 2, bk.Rect.Y + 1, bk.Rect.Width - 4, bk.Rect.Height / 2.5))
                    If bk.HitsLeft > 1 Then
                        Dim ht = MkText(bk.HitsLeft.ToString(), 8, C4(180, 0, 0, 0), True)
                        dc.DrawText(ht, New Point(bk.Rect.X + (bk.Rect.Width - ht.Width) / 2, bk.Rect.Y + (bk.Rect.Height - ht.Height) / 2))
                    End If
                End If
                If _colorblindMode Then
                    dc.DrawRoundedRectangle(Nothing, New Pen(SB(Colors.White), 2), bk.Rect, 4, 4)
                    Dim sym = _colorblindSymbols(bk.Row Mod _colorblindSymbols.Length)
                    If bk.HitsLeft > 1 Then sym = bk.HitsLeft.ToString() & sym
                    Dim st = MkText(sym, 10, Colors.White, True)
                    dc.DrawText(st, New Point(bk.Rect.X + (bk.Rect.Width - st.Width) / 2, bk.Rect.Y + (bk.Rect.Height - st.Height) / 2))
                End If
            Next
        End Sub

        Private Sub DrawPaddle(dc As DrawingContext)
            Dim py As Double = LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT
            Dim pr = New Rect(_paddleX, py, _paddleWidth, PADDLE_HEIGHT)
            ' Select paddle sprite: colored by level, sized by power-up state
            Dim colorIdx = (_level - 1) Mod _paddleColorKeys.Length
            Dim paddleSprite As BitmapSource = Nothing
            If _assetMgr IsNot Nothing Then
                If _paddleWidth > PADDLE_WIDTH * 1.3 Then
                    paddleSprite = _assetMgr.GetSprite(_paddleLargeKeys(colorIdx))
                    If paddleSprite Is Nothing Then paddleSprite = _assetMgr.GetSprite("Sprites/paddle_wide")
                ElseIf _paddleWidth < PADDLE_WIDTH * 0.8 Then
                    paddleSprite = _assetMgr.GetSprite(_paddleSmallKeys(colorIdx))
                    If paddleSprite Is Nothing Then paddleSprite = _assetMgr.GetSprite("Sprites/paddle_short")
                Else
                    ' HD colored ? medium colored ? alt (even levels) ? default
                    paddleSprite = _assetMgr.GetSprite(_paddleColorKeys(colorIdx))
                    If paddleSprite Is Nothing Then paddleSprite = _assetMgr.GetSprite(_paddleMedKeys(colorIdx))
                    If paddleSprite Is Nothing AndAlso _level Mod 2 = 0 Then paddleSprite = _assetMgr.GetSprite("Sprites/paddle_alt")
                    If paddleSprite Is Nothing Then paddleSprite = _assetMgr.GetSprite("Sprites/paddle")
                End If
            End If
            If paddleSprite IsNot Nothing Then
                dc.DrawEllipse(SB(Color.FromArgb(25, 80, 180, 255)), Nothing, New Point(_paddleX + _paddleWidth / 2.0, py + 12), _paddleWidth / 2.0 + 10, 10)
                dc.DrawImage(paddleSprite, pr)
            Else
                ' Procedural fallback
                Dim c1 = If(_colorblindMode, C3(240, 228, 66), C3(80, 180, 255))
                Dim c2 = If(_colorblindMode, C3(200, 190, 40), C3(40, 100, 200))
                dc.DrawEllipse(SB(Color.FromArgb(30, c1.R, c1.G, c1.B)), Nothing, New Point(_paddleX + _paddleWidth / 2.0, py + 12), _paddleWidth / 2.0 + 10, 10)
                dc.DrawRoundedRectangle(New LinearGradientBrush(c1, c2, 90.0), Nothing, pr, 7, 7)
                dc.DrawRectangle(SB(C4(80, 255, 255, 255)), Nothing, New Rect(_paddleX + 4, py + 1, _paddleWidth - 8, PADDLE_HEIGHT / 2.5))
            End If
            If _colorblindMode Then dc.DrawRoundedRectangle(Nothing, New Pen(SB(Colors.White), 2), pr, 7, 7)
        End Sub

        Private Sub DrawBalls(dc As DrawingContext)
            Dim br2 As Double = _ballRadius
            ' Select ball variant: fire combo, speed boost, size changes, level color
            Dim ballKey = "Sprites/ball"
            If _combo >= 5 Then
                ballKey = "Sprites/ball_fire"
            ElseIf _speedBoost Then
                ballKey = "Sprites/ball_red"
            ElseIf _ballRadius > BALL_RADIUS Then
                ballKey = "Sprites/ball_blue"
            ElseIf _ballRadius < BALL_RADIUS Then
                ballKey = "Sprites/ball_purple"
            ElseIf _level Mod 2 = 0 Then
                ballKey = "Sprites/ball_alt"
            ElseIf _balls.Count > 1 Then
                ballKey = "Sprites/ball_green"
            End If
            Dim ballSprite = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite(ballKey), Nothing)
            If ballSprite Is Nothing Then ballSprite = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("Sprites/ball"), Nothing)
            For Each b In _balls
                If Not b.Active Then Continue For
                If ballSprite IsNot Nothing Then
                    ' Glow behind sprite ball
                    For gs = 20 To 8 Step -4
                        Dim al = CByte(CInt(15 * (4.0 / gs)))
                        dc.DrawEllipse(SB(If(_speedBoost, Color.FromArgb(al, 255, 200, 50), Color.FromArgb(al, 200, 230, 255))), Nothing, New Point(b.X, b.Y), gs / 2.0, gs / 2.0)
                    Next
                    dc.DrawImage(ballSprite, New Rect(b.X - br2, b.Y - br2, br2 * 2, br2 * 2))
                Else
                    ' Procedural fallback
                    For gs = 20 To 4 Step -4
                        Dim al = CByte(CInt(20 * (4.0 / gs)))
                        dc.DrawEllipse(SB(If(_speedBoost, Color.FromArgb(al, 255, 200, 50), Color.FromArgb(al, 200, 230, 255))), Nothing, New Point(b.X, b.Y), gs / 2.0, gs / 2.0)
                    Next
                    dc.DrawEllipse(New LinearGradientBrush(If(_speedBoost, C3(255, 255, 200), Colors.White), If(_speedBoost, C3(255, 140, 20), C3(160, 210, 255)), 45.0), Nothing, New Point(b.X, b.Y), br2, br2)
                    dc.DrawEllipse(SB(C4(180, 255, 255, 255)), Nothing, New Point(b.X - br2 * 0.2, b.Y - br2 * 0.3), br2 * 0.3, br2 * 0.25)
                End If
            Next
        End Sub

        Private Sub DrawPowerUps(dc As DrawingContext)
            For Each pu In _powerUps
                If Not pu.Active Then Continue For
                Dim cy As Double = pu.Y + Math.Sin(_frameCount * 0.1) * 3

                ' Try sprite-based rendering (SuperGameAsset or procedural icon)
                Dim spriteKey As String = Nothing
                If _powerUpSpriteMap.TryGetValue(pu.PType, spriteKey) AndAlso _assetMgr IsNot Nothing Then
                    Dim sprite = _assetMgr.GetSprite(spriteKey)
                    If sprite IsNot Nothing Then
                        ' Glow halo behind sprite
                        dc.DrawEllipse(SB(Color.FromArgb(50, pu.Color1.R, pu.Color1.G, pu.Color1.B)),
                        Nothing, New Point(pu.X, cy), POWERUP_SIZE / 2.0 + 5, POWERUP_SIZE / 2.0 + 5)
                        ' Sprite icon
                        dc.DrawImage(sprite, New Rect(
                        pu.X - POWERUP_SIZE / 2.0, cy - POWERUP_SIZE / 2.0,
                        POWERUP_SIZE, POWERUP_SIZE))
                        Continue For
                    End If
                End If

                ' Fallback: original colored circle + text symbol
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
                Dim cx = (LOGICAL_WIDTH - ft.Width) / 2
                dc.DrawText(ft, New Point(cx, LOGICAL_HEIGHT / 2.0 + 30))
                ' Left: bullet (>=10), star (>=5), gem (small combos)
                Dim leftKey = If(_combo >= 10, "UI/bonus_bullet", If(_combo >= 5, "UI/star", "UI/gem"))
                Dim leftSprite = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite(leftKey), Nothing)
                If leftSprite IsNot Nothing Then
                    dc.PushOpacity(ca / 255.0)
                    dc.DrawImage(leftSprite, New Rect(cx - 30, LOGICAL_HEIGHT / 2.0 + 28, 24, 24))
                    dc.Pop()
                End If
                Dim bonusKey = If(_combo >= 8, "UI/bonus_100", If(_combo >= 4, "UI/bonus_50", "UI/bonus_25"))
                Dim bonusSprite = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite(bonusKey), Nothing)
                If bonusSprite IsNot Nothing Then
                    dc.PushOpacity(ca / 255.0)
                    dc.DrawImage(bonusSprite, New Rect(LOGICAL_WIDTH / 2.0 + ft.Width / 2 + 8, LOGICAL_HEIGHT / 2.0 + 28, 24, 24))
                    dc.Pop()
                End If
            End If
        End Sub

        Private Sub DrawGetReady(dc As DrawingContext)
            If _getReadyFrames <= 0 Then Return
            Dim ct = If(_getReadyFrames > 120, "3", If(_getReadyFrames > 60, "2", "1"))
            Dim pulse = Math.Abs(Math.Sin(_frameCount * 0.15)) * 10 + 58
            Dim ft = MkText(ct, pulse, C4(230, 255, 240, 100), True)
            Dim cx = (LOGICAL_WIDTH - ft.Width) / 2
            dc.DrawText(ft, New Point(cx, LOGICAL_HEIGHT / 2.0 - ft.Height / 2 - 20))
            Dim warnIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/warning"), Nothing)
            If warnIcn IsNot Nothing Then
                dc.DrawImage(warnIcn, New Rect(cx - 32, LOGICAL_HEIGHT / 2.0 - 22, 24, 24))
                dc.DrawImage(warnIcn, New Rect(cx + ft.Width + 8, LOGICAL_HEIGHT / 2.0 - 22, 24, 24))
            End If
            ' Use imported text sprite if available
            If _textGetReady IsNot Nothing Then
                Dim tw = Math.Min(280.0, _textGetReady.Width), th = tw * _textGetReady.Height / _textGetReady.Width
                dc.DrawImage(_textGetReady, New Rect((LOGICAL_WIDTH - tw) / 2, LOGICAL_HEIGHT / 2.0 + 40, tw, th))
            Else
                DrawCT(dc, "GET READY!", 14, C4(180, 200, 200, 220), LOGICAL_HEIGHT / 2.0 + 50)
            End If
        End Sub

        Private Sub DrawOptions(dc As DrawingContext)
            dc.DrawRectangle(SB(C4(215, 0, 0, 20)), Nothing, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
            Dim pw = 780.0, ph = 600.0, px = (LOGICAL_WIDTH - pw) / 2, py = (LOGICAL_HEIGHT - ph) / 2
            Dim panelBgOpt = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/panel_button_rectangle_depth_line"), Nothing)
            If panelBgOpt IsNot Nothing Then
                dc.PushOpacity(0.12) : dc.DrawImage(panelBgOpt, New Rect(px, py, pw, ph)) : dc.Pop()
            End If
            dc.DrawRoundedRectangle(SB(C4(245, 12, 12, 35)), New Pen(SB(C4(100, 80, 160, 255)), 2), New Rect(px, py, pw, ph), 14, 14)
            Dim wrenchIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/wrench"), Nothing)
            If wrenchIcn IsNot Nothing Then dc.DrawImage(wrenchIcn, New Rect(px + pw - 36, py + 10, 20, 20))
            Dim optText = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/text_options"), Nothing)
            If optText IsNot Nothing Then
                Dim tw = Math.Min(200.0, optText.Width * 0.5), th = tw * optText.Height / optText.Width
                dc.DrawImage(optText, New Rect((LOGICAL_WIDTH - tw) / 2, py + 5, tw, th))
            Else
                DrawCT(dc, "OPTIONS", 22, C3(100, 200, 255), py + 12, True)
            End If
            Dim y = py + 60, lx = px + 25, rx = px + pw / 2 + 10, barX = px + 260
            Dim btnBlueFlt = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_blue_flat"), Nothing)
            If btnBlueFlt IsNot Nothing Then
                dc.PushOpacity(0.3)
                dc.DrawImage(btnBlueFlt, New Rect(lx - 4, y - 4, pw / 2 - 14, 22))
                dc.DrawImage(btnBlueFlt, New Rect(rx - 4, y - 4, pw / 2 - 14, 22))
                dc.Pop()
            End If
            dc.DrawText(MkText("CONTROLS:", 12, C3(255, 200, 100), True), New Point(lx, y))
            Dim infoIcn2 = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/information"), Nothing)
            If infoIcn2 IsNot Nothing Then dc.DrawImage(infoIcn2, New Rect(rx - 2, y + 2, 12, 12))
            dc.DrawText(MkText("POWER-UPS:", 12, C3(255, 200, 100), True), New Point(rx + 12, y)) : y += 24
            ' Left column: controls with keyboard prompt icons
            Dim kSz = 16.0, kOff = 20.0
            Dim kArr = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrows"), Nothing)
            Dim kSpc = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_space"), Nothing)
            Dim kPk = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_p"), Nothing)
            Dim kFk = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_f"), Nothing)
            Dim kHk = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_h"), Nothing)
            ' Right column: power-up icons from downloaded assets
            Dim puLife = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/powerup_life"), Nothing)
            Dim puGrow = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/powerup_grow"), Nothing)
            Dim puMulti = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/powerup_multi"), Nothing)
            Dim puShrk = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/powerup_shrink"), Nothing)
            Dim puMega = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/powerup_mega"), Nothing)
            Dim puSlow = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/powerup_slow"), Nothing)
            Dim puFast = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/powerup_fast"), Nothing)
            ' Row 1: Move
            If kArr IsNot Nothing Then dc.DrawImage(kArr, New Rect(lx + 8, y, kSz, kSz))
            dc.DrawText(MkText("/ A D   Move Paddle", 10, C3(210, 210, 225)), New Point(lx + 8 + kOff, y))
            If puGrow IsNot Nothing Then dc.DrawImage(puGrow, New Rect(rx + 4, y - 2, kSz, kSz))
            dc.DrawText(MkText("Blue   Ball grows", 10, C3(80, 150, 255)), New Point(rx + 8 + kOff, y)) : y += 19
            ' Row 2: Start
            If kSpc IsNot Nothing Then dc.DrawImage(kSpc, New Rect(lx + 8, y, kSz + 8, kSz))
            dc.DrawText(MkText("Start / Resume", 10, C3(210, 210, 225)), New Point(lx + 8 + kOff + 8, y))
            If puLife IsNot Nothing Then dc.DrawImage(puLife, New Rect(rx + 4, y - 2, kSz, kSz))
            dc.DrawText(MkText("Red    +1 life", 10, C3(255, 90, 90)), New Point(rx + 8 + kOff, y)) : y += 19
            ' Row 3: Pause
            If kPk IsNot Nothing Then dc.DrawImage(kPk, New Rect(lx + 8, y, kSz, kSz))
            dc.DrawText(MkText("/ ESC     Pause", 10, C3(210, 210, 225)), New Point(lx + 8 + kOff, y))
            If puMulti IsNot Nothing Then dc.DrawImage(puMulti, New Rect(rx + 4, y - 2, kSz, kSz))
            dc.DrawText(MkText("Green  Multi-ball", 10, C3(80, 220, 120)), New Point(rx + 8 + kOff, y)) : y += 19
            ' Row 4: Speed
            If kFk IsNot Nothing Then dc.DrawImage(kFk, New Rect(lx + 8, y, kSz, kSz))
            dc.DrawText(MkText("Speed Boost (2x)", 10, C3(210, 210, 225)), New Point(lx + 8 + kOff, y))
            If puShrk IsNot Nothing Then dc.DrawImage(puShrk, New Rect(rx + 4, y - 2, kSz, kSz))
            dc.DrawText(MkText("Yellow Ball shrinks", 10, C3(255, 220, 60)), New Point(rx + 8 + kOff, y)) : y += 19
            ' Row 5: Options
            If kHk IsNot Nothing Then dc.DrawImage(kHk, New Rect(lx + 8, y, kSz, kSz))
            dc.DrawText(MkText("/ O       Options", 10, C3(210, 210, 225)), New Point(lx + 8 + kOff, y))
            If puMega IsNot Nothing Then dc.DrawImage(puMega, New Rect(rx + 4, y - 2, kSz, kSz))
            dc.DrawText(MkText("Purple 3x paddle", 10, C3(170, 80, 255)), New Point(rx + 8 + kOff, y)) : y += 19
            ' Row 6-7: remaining power-ups
            If puSlow IsNot Nothing Then dc.DrawImage(puSlow, New Rect(rx + 4, y - 2, kSz, kSz))
            dc.DrawText(MkText("Orange Ball slows", 10, C3(255, 150, 60)), New Point(rx + 8 + kOff, y)) : y += 19
            If puFast IsNot Nothing Then dc.DrawImage(puFast, New Rect(rx + 4, y - 2, kSz, kSz))
            dc.DrawText(MkText("Pink   Ball speeds", 10, C3(255, 120, 200)), New Point(rx + 8 + kOff, y)) : y += 26
            Dim btnBlueGloss = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_blue_gloss"), Nothing)
            If btnBlueGloss IsNot Nothing Then
                dc.PushOpacity(0.25) : dc.DrawImage(btnBlueGloss, New Rect(lx - 4, y - 4, pw - 20, 22)) : dc.Pop()
            End If
            dc.DrawText(MkText("SETTINGS:", 12, C3(255, 200, 100), True), New Point(lx, y)) : y += 28
            Dim icnSoundOn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/sound_on"), Nothing)
            Dim icnSoundOff = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/sound_off"), Nothing)
            Dim icnMusicOn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/music_on"), Nothing)
            Dim icnMusicOff = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/music_off"), Nothing)
            Dim icnNavL = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/nav_left"), Nothing)
            Dim icnNavR = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/nav_right"), Nothing)
            Dim icnContrast = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/contrast"), Nothing)
            Dim icnZoom = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/zoom"), Nothing)
            Dim icnStop = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/stop"), Nothing)
            Dim chkOff = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/checkbox"), Nothing)
            Dim chkOn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/checkbox_checked"), Nothing)
            Dim icnCheck = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/icon_check"), Nothing)
            Dim icnX = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/icon_x"), Nothing)
            Dim uiArrLSet = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/ui_arrow_left"), Nothing)
            Dim uiArrRSet = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/ui_arrow_right"), Nothing)
            Dim items() = {"SFX Volume:", "Music Volume:", "Music Speed:", "Music Style:", "SFX Style:", "Colorblind Mode:", "Window Size:"}
            Dim settingIcons = {icnSoundOn, icnMusicOn, icnMusicOn, icnMusicOn, icnSoundOn, icnContrast, icnZoom}
            For idx = 0 To 6
                Dim sc = If(_settingsSelection = idx, C3(255, 220, 100), C3(195, 195, 215))
                Dim sel = If(_settingsSelection = idx, ChrW(&H25BA) & "  ", "    ")
                ' Pixel button row background
                Dim pixKey = If(_settingsSelection = idx, _pixelBtnPressedKeys(idx), _pixelBtnKeys(idx))
                Dim pixBtn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite(pixKey), Nothing)
                ' Inlay texture overlay for alternate rows
                Dim inlayKey = If(idx Mod 2 = 0, "UI/pixel_tan_inlay", "UI/pixel_brown_inlay")
                Dim inlayBtn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite(inlayKey), Nothing)
                If pixBtn IsNot Nothing Then
                    dc.PushOpacity(0.22) : dc.DrawImage(pixBtn, New Rect(lx - 4, y - 1, pw / 2 - 14, 22)) : dc.Pop()
                End If
                If inlayBtn IsNot Nothing Then
                    dc.PushOpacity(0.07) : dc.DrawImage(inlayBtn, New Rect(lx - 4, y - 1, pw / 2 - 14, 22)) : dc.Pop()
                End If
                Dim sIcon = settingIcons(idx)
                If sIcon IsNot Nothing Then
                    dc.DrawImage(sIcon, New Rect(lx + 8, y, 14, 14))
                    dc.DrawText(MkText(sel & items(idx), 10, sc), New Point(lx + 24, y))
                Else
                    dc.DrawText(MkText(sel & items(idx), 10, sc), New Point(lx + 8, y))
                End If
                Select Case idx
                    Case 0
                        DrawVolBar(dc, barX, y + 2, 200, 16, _sfxVolume, sc)
                        dc.DrawText(MkText($"{_sfxVolume}%", 10, sc, True), New Point(barX + 210, y))
                        If _sfxVolume = 0 Then
                            Dim muteSpr = If(icnStop IsNot Nothing, icnStop, icnSoundOff)
                            If muteSpr IsNot Nothing Then dc.DrawImage(muteSpr, New Rect(barX + 240, y, 14, 14))
                        End If
                    Case 1
                        Dim ev = GetEffectiveMusicVolume() : DrawVolBar(dc, barX, y + 2, 200, 16, ev, sc)
                        dc.DrawText(MkText($"{ev}%", 10, sc, True), New Point(barX + 210, y))
                        If ev = 0 AndAlso icnMusicOff IsNot Nothing Then dc.DrawImage(icnMusicOff, New Rect(barX + 240, y, 14, 14))
                    Case 2 : DrawVolBar(dc, barX, y + 2, 200, 16, Math.Min(100, _musicSpeed), sc) : dc.DrawText(MkText($"{_musicSpeed}%", 10, sc, True), New Point(barX + 210, y))
                    Case 3
                        If uiArrLSet IsNot Nothing Then dc.DrawImage(uiArrLSet, New Rect(barX - 2, y + 1, 12, 12))
                        If icnNavL IsNot Nothing Then dc.DrawImage(icnNavL, New Rect(barX + 12, y + 1, 12, 12))
                        dc.DrawText(MkText("  " & _musicStyleNames(_musicStyle) & "  ", 10, sc, True), New Point(barX + 26, y))
                        If icnNavR IsNot Nothing Then dc.DrawImage(icnNavR, New Rect(barX + 170, y + 1, 12, 12))
                        If uiArrRSet IsNot Nothing Then dc.DrawImage(uiArrRSet, New Rect(barX + 184, y + 1, 12, 12))
                    Case 4
                        If icnNavL IsNot Nothing Then dc.DrawImage(icnNavL, New Rect(barX - 2, y + 1, 12, 12))
                        dc.DrawText(MkText("  " & _sfxStyleNames(_sfxStyle) & "  ", 10, sc, True), New Point(barX + 12, y))
                        If icnNavR IsNot Nothing Then dc.DrawImage(icnNavR, New Rect(barX + 120, y + 1, 12, 12))
                    Case 5
                        Dim chkSprite = If(_colorblindMode, chkOn, chkOff)
                        If chkSprite IsNot Nothing Then dc.DrawImage(chkSprite, New Rect(barX, y, 18, 18))
                        Dim stateIcn = If(_colorblindMode, icnCheck, icnX)
                        If stateIcn IsNot Nothing Then dc.DrawImage(stateIcn, New Rect(barX + 22, y + 1, 14, 14))
                        dc.DrawText(MkText($" {If(_colorblindMode, "ON", "OFF")}", 10, If(_colorblindMode, C3(100, 255, 150), C3(255, 100, 100)), True), New Point(barX + 38, y))
                    Case 6
                        If icnNavL IsNot Nothing Then dc.DrawImage(icnNavL, New Rect(barX - 2, y + 1, 12, 12))
                        dc.DrawText(MkText("  " & _windowScaleNames(_windowScale) & "  ", 10, sc, True), New Point(barX + 12, y))
                        If icnNavR IsNot Nothing Then dc.DrawImage(icnNavR, New Rect(barX + 220, y + 1, 12, 12))
                End Select
                y += 32
            Next
            ' Bottom hint with actual key sprites + ui arrows + return icon
            Dim kArrU2 = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrow_up"), Nothing)
            Dim kArrD2 = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrow_down"), Nothing)
            Dim kArrLO = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrow_left"), Nothing)
            Dim kArrRO = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_arrow_right"), Nothing)
            Dim uiArrU2 = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/ui_arrow_up"), Nothing)
            Dim uiArrD2 = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/ui_arrow_down"), Nothing)
            Dim returnIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/return"), Nothing)
            Dim kEscOpt = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_escape"), Nothing)
            Dim kOOpt = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/key_o"), Nothing)
            Dim pixWhtOpt = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_white"), Nothing)
            Dim pixWhtPres = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_white_pressed"), Nothing)
            If pixWhtOpt IsNot Nothing Then
                dc.PushOpacity(0.06) : dc.DrawImage(pixWhtOpt, New Rect(px, py + ph - 40, pw, 40)) : dc.Pop()
            End If
            If pixWhtPres IsNot Nothing Then
                dc.PushOpacity(0.04) : dc.DrawImage(pixWhtPres, New Rect(px, py + ph - 40, pw, 40)) : dc.Pop()
            End If
            Dim hxB = px + 16.0 : Dim hyB = py + ph - 34 : Dim ks2 = 12.0
            For Each hi In {kArrU2, kArrD2, uiArrU2, uiArrD2}
                If hi IsNot Nothing Then dc.DrawImage(hi, New Rect(hxB, hyB, ks2, ks2)) : hxB += ks2 + 2
            Next
            dc.DrawText(MkText(" Select  ", 9, C3(130, 130, 155)), New Point(hxB, hyB)) : hxB += 44
            For Each hi In {kArrLO, kArrRO, uiArrLSet, uiArrRSet}
                If hi IsNot Nothing Then dc.DrawImage(hi, New Rect(hxB, hyB, ks2, ks2)) : hxB += ks2 + 2
            Next
            dc.DrawText(MkText(" Adjust  ", 9, C3(130, 130, 155)), New Point(hxB, hyB)) : hxB += 44
            If returnIcn IsNot Nothing Then dc.DrawImage(returnIcn, New Rect(hxB, hyB, ks2 + 2, ks2)) : hxB += ks2 + 4
            dc.DrawText(MkText(" Toggle  ", 9, C3(130, 130, 155)), New Point(hxB, hyB)) : hxB += 44
            If kEscOpt IsNot Nothing Then dc.DrawImage(kEscOpt, New Rect(hxB, hyB, ks2 + 4, ks2)) : hxB += ks2 + 6
            If kOOpt IsNot Nothing Then dc.DrawImage(kOOpt, New Rect(hxB, hyB, ks2, ks2)) : hxB += ks2 + 2
            dc.DrawText(MkText(" Close", 9, C3(130, 130, 155)), New Point(hxB, hyB))
        End Sub

        Private Sub DrawVolBar(dc As DrawingContext, x As Double, y As Double, w As Double, h As Double, value As Integer, col As Color)
            Dim sliderTrack = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/slider_track"), Nothing)
            Dim sliderHandle = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/slider_handle"), Nothing)
            If sliderTrack IsNot Nothing Then
                dc.DrawImage(sliderTrack, New Rect(x, y + h / 4, w, h / 2))
            Else
                dc.DrawRoundedRectangle(SB(C4(60, 255, 255, 255)), Nothing, New Rect(x, y, w, h), 4, 4)
            End If
            Dim fw = w * value / 100.0
            If fw > 2 Then
                dc.DrawRoundedRectangle(SB(Color.FromArgb(120, col.R, col.G, col.B)), Nothing, New Rect(x, y + h / 4, fw, h / 2), 2, 2)
            End If
            If sliderHandle IsNot Nothing Then
                dc.DrawImage(sliderHandle, New Rect(Math.Max(x, x + fw - h), y, h, h))
            End If
        End Sub

        Private Sub DrawHighScore(dc As DrawingContext)
            DrawStarField(dc)
            dc.DrawRectangle(SB(C4(200, 0, 0, 20)), Nothing, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
            Dim pw = 520.0, ph = 540.0, px = (LOGICAL_WIDTH - pw) / 2, py = (LOGICAL_HEIGHT - ph) / 2
            Dim panelRound = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/panel_button_round_line"), Nothing)
            Dim panelRoundD = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/panel_button_round_depth_line"), Nothing)
            Dim panelSq = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/panel_button_square_depth_line"), Nothing)
            Dim pixSheet = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_UIpackSheet_transparent"), Nothing)
            Dim pixMagenta = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_UIpackSheet_magenta"), Nothing)
            If panelRoundD IsNot Nothing Then
                dc.PushOpacity(0.07) : dc.DrawImage(panelRoundD, New Rect(px - 4, py - 4, pw + 8, ph + 8)) : dc.Pop()
            End If
            If panelRound IsNot Nothing Then
                dc.PushOpacity(0.12) : dc.DrawImage(panelRound, New Rect(px, py, pw, ph)) : dc.Pop()
            End If
            If pixSheet IsNot Nothing Then
                dc.PushOpacity(0.04) : dc.DrawImage(pixSheet, New Rect(px, py, pw, ph)) : dc.Pop()
            End If
            If pixMagenta IsNot Nothing Then
                dc.PushOpacity(0.05) : dc.DrawImage(pixMagenta, New Rect(px, py + ph - 10, pw, 10)) : dc.Pop()
            End If
            Dim btnRed = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_red"), Nothing)
            If btnRed IsNot Nothing Then
                dc.PushOpacity(0.35) : dc.DrawImage(btnRed, New Rect(px, py, pw, 44)) : dc.Pop()
            End If
            dc.DrawRoundedRectangle(SB(C4(245, 12, 12, 35)), New Pen(SB(C4(100, 255, 80, 80)), 2), New Rect(px, py, pw, ph), 14, 14)
            Dim lbIcon = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/leaderboard"), Nothing)
            Dim playerIcnHS = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/player_icon"), Nothing)
            If lbIcon IsNot Nothing Then dc.DrawImage(lbIcon, New Rect(px + 15, py + 12, 22, 22))
            If playerIcnHS IsNot Nothing Then dc.DrawImage(playerIcnHS, New Rect(px + 40, py + 12, 22, 22))
            ' Use imported GAME OVER text sprite if available
            If _textGameOver IsNot Nothing Then
                Dim tw = Math.Min(300.0, _textGameOver.Width * 0.6), th = tw * _textGameOver.Height / _textGameOver.Width
                dc.DrawImage(_textGameOver, New Rect((LOGICAL_WIDTH - tw) / 2, py + 10, tw, th))
            Else
                DrawCT(dc, "GAME OVER", 30, C3(255, 80, 100), py + 15, True)
            End If
            DrawCT(dc, $"Final Score: {_score}", 18, C3(255, 220, 100), py + 65, True)
            DrawCT(dc, $"Level {_level}  |  Ball Size: {_ballRadius}px", 12, C3(180, 200, 255), py + 100)
            Dim y = py + 135
            Dim saveIcn = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/save"), Nothing)
            Dim homeIcnHS = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/home"), Nothing)
            If Not _highScoreSaved Then
                DrawCT(dc, "Enter Name: " & _nameInput & If(_frameCount Mod 60 < 30, "_", " "), 14, Colors.White, y)
                If saveIcn IsNot Nothing Then dc.DrawImage(saveIcn, New Rect(px + pw - 36, y + 2, 18, 18))
                DrawCT(dc, "Press ENTER to save", 10, C3(140, 140, 160), y + 30)
            Else
                Dim btnGG = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/button_green_gloss"), Nothing)
                If btnGG IsNot Nothing Then
                    dc.PushOpacity(0.4) : dc.DrawImage(btnGG, New Rect((LOGICAL_WIDTH - 320) / 2, y - 2, 320, 26)) : dc.Pop()
                End If
                DrawCT(dc, "Score saved! Press SPACE to continue", 12, C3(100, 255, 150), y)
                If homeIcnHS IsNot Nothing Then dc.DrawImage(homeIcnHS, New Rect(LOGICAL_WIDTH / 2.0 + 140, y + 2, 16, 16))
            End If
            y = py + 210
            DrawCT(dc, "HIGH SCORES", 14, C3(100, 200, 255), y, True) : y += 30
            Dim medalGold = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/medal_gold"), Nothing)
            Dim medalSilver = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/medal_silver"), Nothing)
            Dim pixelList = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_list"), Nothing)
            Dim pixPreview = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_Preview"), Nothing)
            Dim pixGrey = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_grey_inlay"), Nothing)
            Dim pixWhite = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pixel_white_inlay"), Nothing)
            ' pixel_Preview subtle decoration in corner
            If pixPreview IsNot Nothing Then
                dc.PushOpacity(0.06) : dc.DrawImage(pixPreview, New Rect(px + pw - 60, py + 200, 50, 50)) : dc.Pop()
            End If
            If _highScores.Count = 0 Then
                Dim ft = MkMono("No scores yet!", 12, C3(140, 140, 160))
                dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, y))
            Else
                For i = 0 To Math.Min(9, _highScores.Count - 1)
                    Dim rec = _highScores(i)
                    Dim nm = If(rec.PlayerName.Length > 12, rec.PlayerName.Substring(0, 12), rec.PlayerName.PadRight(12))
                    Dim ec = If(_highScoreSaved AndAlso rec.PlayerName = _nameInput AndAlso rec.PlayerScore = _score, C3(255, 220, 100), C3(195, 195, 215))
                    ' Alternating row backgrounds from pixel UI pack
                    Dim isSaved = _highScoreSaved AndAlso rec.PlayerName = _nameInput AndAlso rec.PlayerScore = _score
                    Dim rowBg = If(isSaved, pixWhite, If(i Mod 2 = 0, pixelList, pixGrey))
                    If rowBg IsNot Nothing Then
                        dc.PushOpacity(If(isSaved, 0.18, 0.08)) : dc.DrawImage(rowBg, New Rect(px + 8, y - 1, pw - 16, 20)) : dc.Pop()
                    End If
                    If panelSq IsNot Nothing AndAlso i = 0 Then
                        dc.PushOpacity(0.06) : dc.DrawImage(panelSq, New Rect(px + 8, y - 1, pw - 16, 20)) : dc.Pop()
                    End If
                    ' Medal icons for top 3 (gold for 1st, silver for 2nd-3rd, star fallback)
                    If i = 0 AndAlso medalGold IsNot Nothing Then
                        dc.DrawImage(medalGold, New Rect(LOGICAL_WIDTH / 2.0 - 220, y + 1, 14, 14))
                    ElseIf i >= 1 AndAlso i <= 2 AndAlso medalSilver IsNot Nothing Then
                        dc.DrawImage(medalSilver, New Rect(LOGICAL_WIDTH / 2.0 - 220, y + 1, 14, 14))
                    ElseIf i < 3 AndAlso _hudStar IsNot Nothing Then
                        dc.DrawImage(_hudStar, New Rect(LOGICAL_WIDTH / 2.0 - 220, y + 1, 14, 14))
                    End If
                    Dim ft = MkMono($"{(i + 1).ToString().PadLeft(2)}. {nm} {rec.PlayerScore.ToString("N0").PadLeft(10)}", 12, ec)
                    dc.DrawText(ft, New Point((LOGICAL_WIDTH - ft.Width) / 2, y)) : y += 25
                Next
            End If
        End Sub

        Private Sub DrawOverlay(dc As DrawingContext, title As String, subtitle As String, Optional animated As Boolean = False)
            dc.DrawRectangle(SB(C4(180, 0, 0, 20)), Nothing, New Rect(0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT))
            Dim panelBgOvl = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/panel_button_rectangle_line"), Nothing)
            If panelBgOvl IsNot Nothing Then
                dc.PushOpacity(0.18) : dc.DrawImage(panelBgOvl, New Rect((LOGICAL_WIDTH - 420) / 2, LOGICAL_HEIGHT / 2.0 - 90, 420, 140)) : dc.Pop()
            End If
            If title = "PAUSED" Then
                Dim pauseIcnOvl = If(_assetMgr IsNot Nothing, _assetMgr.GetSprite("UI/pause"), Nothing)
                If pauseIcnOvl IsNot Nothing Then
                    dc.DrawImage(pauseIcnOvl, New Rect((LOGICAL_WIDTH - 40) / 2, LOGICAL_HEIGHT / 2.0 - 80, 40, 40))
                End If
            End If
            ' Use imported text sprite if available for known titles
            Dim overlaySprite As BitmapSource = Nothing
            If title.Contains("COMPLETE") AndAlso _textYouWin IsNot Nothing Then
                overlaySprite = _textYouWin
            ElseIf title = "PAUSED" AndAlso _textPaused IsNot Nothing Then
                overlaySprite = _textPaused
            End If
            If overlaySprite IsNot Nothing Then
                Dim tw = Math.Min(320.0, overlaySprite.Width * 0.6), th = tw * overlaySprite.Height / overlaySprite.Width
                dc.DrawImage(overlaySprite, New Rect((LOGICAL_WIDTH - tw) / 2, LOGICAL_HEIGHT / 2.0 - 70, tw, th))
            Else
                Dim sz = If(animated, 40 + Math.Sin(_frameCount * 0.08) * 6, 40.0)
                Dim pulse = (Math.Sin(_frameCount * 0.05) + 1) / 2
                Dim tc = If(animated, Color.FromRgb(CByte(180 + pulse * 75), CByte(180 + pulse * 75), 255), Colors.White)
                DrawCT(dc, title, sz, tc, LOGICAL_HEIGHT / 2.0 - 60, True)
            End If
            DrawCT(dc, subtitle, 16, C3(200, 200, 220), LOGICAL_HEIGHT / 2.0 + 10)
        End Sub
#End Region

#Region "Input"
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            ' WPF sends Alt+Key as Key.System — check SystemKey for the real key
            If e.Key = Key.System AndAlso e.SystemKey = Key.Return Then
                Dim win = TryCast(Window.GetWindow(Me), MainWindow)
                If win IsNot Nothing Then win.ToggleFullscreen()
                e.Handled = True
                Return
            End If
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
                    ElseIf _state = GameState.Playing Then
                        _speedBoost = Not _speedBoost : PlaySFX(_sfxData(_sfxStyle)(10), 80)
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
            If _enemyMgr IsNot Nothing Then _enemyMgr.SpawnWave(_level, LOGICAL_WIDTH, LOGICAL_HEIGHT)
            If _tileMap IsNot Nothing Then _tileMap.LoadFromPattern(_level)
            If _inventory IsNot Nothing Then _inventory.Clear()
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

        Private Sub CheckEnemyBallCollisions()
            If _enemyMgr Is Nothing Then Return
            For Each b In _balls
                If Not b.Active Then Continue For
                Dim hit = _enemyMgr.CheckBallCollision(b.X, b.Y, _ballRadius)
                If hit IsNot Nothing Then
                    _score += hit.Points : _combo += 1 : _comboTimer = 90
                    SpawnParticles(hit.X, hit.Y, hit.SpriteColor, 12)
                    _screenShake = 5
                    If hit.Health <= 0 Then
                        PlayBrickHit()
                        If _inventory IsNot Nothing AndAlso _rng.Next(100) < 40 Then
                            _inventory.AddItem(hit.DropItem, $"UI/{hit.DropItem}", hit.SpriteColor, "Loot: " & hit.DropItem)
                        End If
                    Else
                        PlaySFX(400, 30)
                    End If
                End If
            Next
            If _uiMgr IsNot Nothing Then
                Dim total = _enemyMgr.Enemies.Count
                Dim alive = _enemyMgr.ActiveCount
                _uiMgr.SetValue("enemy_progress", CSng(total - alive), CSng(Math.Max(1, total)))
                _uiMgr.SetVisible("enemy_progress", total > 0)
            End If
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

        ' =========================================================================
        ' MUSIC DATA � 96-note compositions per style (3x previous length).
        ' Each style: A(16) ? B(16) ? A'(16) ? C(16) ? D(16) ? A''(16)
        ' Doubled to 192 notes by GenerateMidiBytes for ~40s loops.
        ' =========================================================================
        Private Sub GetMusicData(style As Integer, ByRef freqs() As Integer, ByRef durs() As Integer)
            Select Case style
                Case 0 ' ?? Zelda Adventure ?? G major, lyrical flute, open arpeggios
                    freqs = {
                    392, 494, 587, 784, 659, 587, 494, 0, 392, 440, 494, 587, 659, 587, 494, 392,
                    440, 523, 659, 880, 784, 659, 523, 0, 587, 659, 784, 880, 784, 659, 587, 523,
                    392, 494, 587, 784, 659, 587, 494, 440, 392, 440, 494, 587, 784, 0, 659, 587,
                    494, 587, 784, 988, 880, 784, 659, 587, 523, 659, 784, 880, 988, 880, 784, 0,
                    330, 392, 494, 659, 587, 494, 392, 0, 440, 494, 587, 659, 587, 494, 440, 392,
                    392, 494, 587, 659, 587, 494, 392, 0, 330, 392, 440, 494, 587, 494, 392, 0}
                    durs = {
                    200, 200, 200, 300, 200, 200, 350, 100, 200, 200, 200, 300, 200, 200, 200, 300,
                    200, 200, 200, 300, 200, 200, 350, 100, 200, 200, 200, 300, 200, 200, 200, 300,
                    200, 200, 200, 300, 200, 200, 200, 200, 200, 200, 200, 350, 300, 100, 200, 300,
                    200, 200, 200, 350, 200, 200, 200, 200, 200, 200, 200, 300, 300, 200, 350, 200,
                    200, 200, 200, 300, 200, 200, 350, 100, 200, 200, 200, 300, 200, 200, 200, 300,
                    200, 200, 200, 300, 200, 200, 400, 100, 200, 200, 200, 300, 300, 200, 400, 200}

                Case 1 ' ?? Mega Man Energy ?? E minor, driving square lead, fast runs
                    freqs = {
                    659, 659, 587, 523, 587, 659, 784, 659, 880, 784, 659, 587, 523, 587, 659, 523,
                    659, 784, 880, 988, 880, 784, 659, 523, 587, 659, 784, 880, 784, 659, 587, 494,
                    659, 659, 587, 523, 587, 659, 784, 880, 784, 659, 523, 440, 494, 523, 587, 659,
                    880, 988, 880, 784, 659, 784, 880, 988, 784, 659, 587, 523, 587, 659, 784, 0,
                    330, 392, 440, 494, 523, 494, 440, 392, 440, 494, 523, 587, 659, 587, 523, 494,
                    659, 784, 880, 988, 880, 784, 659, 587, 523, 587, 659, 784, 880, 784, 659, 0}
                    durs = {
                    130, 130, 130, 130, 130, 130, 200, 130, 130, 130, 130, 130, 130, 130, 200, 200,
                    130, 130, 130, 200, 130, 130, 130, 130, 130, 130, 130, 200, 130, 130, 130, 200,
                    130, 130, 130, 130, 130, 130, 200, 130, 130, 130, 130, 130, 130, 130, 130, 200,
                    130, 200, 130, 130, 130, 130, 130, 200, 130, 130, 130, 130, 130, 130, 200, 150,
                    150, 150, 150, 150, 150, 150, 150, 200, 150, 150, 150, 150, 150, 150, 150, 200,
                    130, 130, 130, 200, 130, 130, 130, 130, 130, 130, 130, 200, 200, 130, 200, 150}

                Case 2 ' ?? Tetris Classic ?? A minor, Korobeiniki folk melody, music box
                    freqs = {
                    659, 494, 523, 587, 659, 587, 523, 494, 440, 440, 523, 659, 587, 523, 494, 0,
                    523, 587, 659, 523, 440, 440, 523, 587, 659, 587, 523, 494, 440, 494, 523, 587,
                    659, 494, 523, 587, 659, 587, 523, 494, 440, 440, 523, 659, 587, 523, 494, 523,
                    587, 0, 698, 880, 784, 698, 659, 0, 523, 0, 659, 587, 523, 494, 440, 0,
                    587, 698, 880, 784, 698, 659, 523, 659, 587, 523, 494, 440, 494, 523, 587, 659,
                    659, 494, 523, 587, 659, 587, 523, 494, 440, 440, 523, 659, 587, 523, 494, 0}
                    durs = {
                    200, 100, 100, 200, 100, 100, 200, 100, 200, 100, 100, 200, 100, 100, 200, 100,
                    100, 200, 200, 100, 200, 100, 100, 200, 200, 100, 100, 200, 100, 100, 100, 200,
                    200, 100, 100, 200, 100, 100, 200, 100, 200, 100, 100, 200, 100, 100, 200, 100,
                    200, 200, 200, 100, 200, 100, 100, 200, 200, 100, 200, 100, 100, 200, 200, 200,
                    200, 100, 200, 100, 100, 200, 100, 200, 200, 100, 100, 200, 100, 100, 200, 200,
                    200, 100, 100, 200, 100, 100, 200, 100, 200, 100, 100, 200, 100, 100, 300, 200}

                Case 3 ' ?? Pac-Man Playful ?? C major, bouncy staccato, octave jumps
                    freqs = {
                    523, 1047, 784, 659, 1047, 784, 659, 0, 523, 494, 440, 494, 523, 659, 784, 0,
                    659, 784, 880, 784, 659, 523, 440, 523, 659, 784, 880, 1047, 880, 784, 659, 0,
                    523, 1047, 784, 659, 1047, 784, 659, 0, 523, 494, 440, 494, 523, 659, 784, 880,
                    440, 523, 659, 523, 440, 392, 349, 392, 440, 523, 659, 784, 659, 523, 440, 0,
                    784, 880, 1047, 880, 784, 659, 784, 880, 1047, 880, 784, 659, 523, 659, 784, 0,
                    523, 1047, 784, 659, 523, 494, 440, 494, 523, 659, 784, 880, 1047, 880, 784, 0}
                    durs = {
                    160, 160, 160, 160, 160, 160, 300, 100, 160, 160, 160, 160, 160, 160, 300, 100,
                    160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 300, 160, 160, 200, 150,
                    160, 160, 160, 160, 160, 160, 300, 100, 160, 160, 160, 160, 160, 160, 160, 200,
                    160, 160, 200, 160, 160, 160, 160, 160, 160, 160, 160, 200, 160, 160, 300, 100,
                    200, 160, 200, 160, 160, 160, 160, 200, 200, 160, 160, 160, 160, 160, 300, 100,
                    160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 160, 200, 200, 160, 300, 150}

                Case 4 ' ?? Space Invaders ?? E low, ominous march, cello, building tension
                    freqs = {
                    165, 165, 165, 0, 147, 147, 147, 0, 131, 131, 131, 0, 147, 165, 196, 262,
                    196, 165, 131, 0, 165, 165, 165, 0, 147, 147, 147, 0, 131, 165, 196, 0,
                    220, 220, 220, 0, 196, 196, 196, 0, 175, 175, 175, 0, 196, 220, 262, 294,
                    262, 262, 294, 0, 262, 220, 196, 0, 220, 247, 262, 294, 262, 220, 196, 165,
                    294, 262, 247, 220, 196, 175, 165, 0, 131, 147, 165, 196, 220, 196, 165, 0,
                    165, 165, 165, 0, 147, 147, 147, 0, 131, 131, 131, 0, 165, 196, 220, 0}
                    durs = {
                    250, 250, 250, 150, 250, 250, 250, 150, 250, 250, 250, 150, 200, 200, 200, 400,
                    250, 250, 400, 200, 250, 250, 250, 150, 250, 250, 250, 150, 250, 250, 400, 150,
                    250, 250, 250, 150, 250, 250, 250, 150, 250, 250, 250, 150, 200, 200, 200, 400,
                    300, 300, 300, 200, 300, 250, 250, 200, 200, 200, 200, 300, 250, 250, 250, 300,
                    200, 200, 200, 200, 250, 250, 400, 200, 250, 250, 250, 250, 300, 250, 400, 200,
                    250, 250, 250, 150, 250, 250, 250, 150, 250, 250, 250, 150, 250, 250, 400, 200}

                Case 5 ' ?? Castlevania Dark ?? A minor harmonic, gothic organ arpeggios
                    freqs = {
                    440, 523, 659, 880, 831, 659, 523, 440, 494, 587, 698, 988, 880, 698, 587, 494,
                    880, 831, 659, 523, 440, 415, 392, 440, 523, 587, 659, 784, 831, 784, 659, 523,
                    440, 523, 659, 880, 831, 659, 523, 440, 392, 440, 494, 523, 494, 440, 392, 0,
                    659, 587, 523, 494, 523, 587, 659, 784, 831, 784, 659, 587, 523, 587, 659, 880,
                    440, 0, 415, 0, 392, 0, 349, 0, 392, 415, 440, 523, 659, 880, 831, 659,
                    440, 523, 659, 880, 988, 880, 784, 659, 523, 587, 659, 784, 880, 831, 659, 0}
                    durs = {
                    180, 180, 180, 250, 180, 180, 180, 250, 180, 180, 180, 250, 180, 180, 180, 250,
                    180, 180, 180, 180, 200, 200, 200, 250, 180, 180, 180, 250, 200, 180, 180, 250,
                    180, 180, 180, 250, 180, 180, 180, 250, 180, 180, 180, 250, 200, 200, 350, 150,
                    180, 180, 180, 180, 180, 180, 180, 250, 200, 180, 180, 180, 180, 180, 180, 300,
                    300, 150, 300, 150, 300, 150, 300, 150, 180, 180, 180, 250, 200, 200, 200, 250,
                    180, 180, 180, 250, 200, 180, 180, 180, 180, 180, 180, 250, 300, 200, 350, 200}

                Case 6 ' ?? Metroid Atmosphere ?? atonal, sparse synth pad, wide intervals
                    freqs = {
                    165, 0, 196, 0, 220, 0, 0, 0, 247, 0, 262, 0, 0, 0, 165, 0,
                    196, 0, 175, 0, 165, 0, 0, 0, 247, 262, 0, 0, 220, 196, 165, 0,
                    165, 0, 330, 0, 262, 0, 0, 0, 196, 0, 370, 0, 294, 0, 165, 0,
                    220, 247, 262, 294, 262, 247, 220, 0, 165, 196, 220, 247, 262, 220, 196, 0,
                    330, 294, 262, 247, 220, 196, 175, 165, 196, 0, 220, 0, 262, 0, 294, 0,
                    165, 0, 0, 0, 196, 0, 0, 0, 220, 0, 165, 0, 0, 0, 0, 0}
                    durs = {
                    400, 200, 400, 200, 400, 200, 300, 200, 400, 200, 400, 200, 300, 200, 500, 200,
                    400, 200, 400, 200, 400, 200, 300, 200, 300, 300, 300, 200, 400, 400, 500, 200,
                    400, 200, 500, 200, 400, 200, 300, 200, 400, 200, 500, 200, 400, 200, 500, 200,
                    250, 250, 250, 300, 250, 250, 400, 200, 250, 250, 250, 300, 300, 250, 500, 200,
                    300, 300, 300, 300, 300, 300, 300, 400, 400, 200, 400, 200, 400, 200, 400, 200,
                    500, 200, 300, 200, 500, 200, 300, 200, 400, 200, 500, 200, 300, 200, 300, 400}

                Case 7 ' ?? Galaga Arcade ?? C major, fast ascending, bright square lead
                    freqs = {
                    523, 659, 784, 1047, 784, 659, 523, 0, 587, 698, 880, 1175, 880, 698, 587, 0,
                    523, 659, 784, 1047, 784, 659, 523, 0, 587, 698, 880, 784, 659, 523, 440, 0,
                    659, 784, 880, 1047, 880, 784, 659, 523, 587, 698, 880, 1175, 1047, 880, 784, 659,
                    1047, 880, 784, 659, 523, 659, 784, 880, 1047, 1175, 1047, 880, 784, 659, 523, 0,
                    440, 523, 659, 784, 659, 523, 440, 0, 349, 440, 523, 659, 784, 659, 523, 440,
                    523, 659, 784, 1047, 880, 784, 659, 587, 523, 587, 659, 784, 1047, 880, 784, 0}
                    durs = {
                    140, 140, 140, 200, 140, 140, 280, 100, 140, 140, 140, 200, 140, 140, 280, 100,
                    140, 140, 140, 200, 140, 140, 280, 100, 140, 140, 140, 200, 140, 140, 200, 150,
                    140, 140, 140, 200, 140, 140, 140, 140, 140, 140, 140, 200, 200, 140, 140, 200,
                    200, 140, 140, 140, 140, 140, 140, 200, 200, 200, 140, 140, 140, 140, 280, 150,
                    140, 140, 140, 200, 140, 140, 280, 100, 140, 140, 140, 200, 200, 140, 140, 200,
                    140, 140, 140, 200, 140, 140, 140, 140, 140, 140, 140, 200, 200, 140, 280, 150}

                Case 8 ' ?? Contra Action ?? E minor, military march, overdriven guitar
                    freqs = {
                    330, 330, 370, 392, 440, 392, 370, 330, 294, 294, 330, 370, 392, 370, 330, 294,
                    330, 392, 440, 494, 523, 494, 440, 392, 440, 494, 523, 587, 659, 587, 523, 494,
                    330, 330, 370, 392, 440, 494, 523, 587, 659, 587, 494, 440, 392, 370, 330, 0,
                    659, 659, 587, 523, 494, 523, 587, 659, 784, 659, 587, 523, 494, 523, 587, 0,
                    262, 294, 330, 370, 392, 370, 330, 294, 262, 294, 330, 392, 440, 392, 330, 294,
                    330, 392, 440, 494, 523, 587, 659, 784, 659, 587, 494, 440, 392, 330, 294, 0}
                    durs = {
                    170, 170, 170, 250, 170, 170, 170, 250, 170, 170, 170, 250, 170, 170, 170, 250,
                    170, 170, 170, 200, 170, 170, 170, 250, 170, 170, 170, 200, 200, 170, 170, 250,
                    170, 170, 170, 250, 170, 170, 170, 250, 170, 170, 170, 170, 200, 170, 350, 100,
                    200, 200, 170, 170, 170, 170, 170, 250, 200, 170, 170, 170, 170, 170, 350, 150,
                    170, 170, 170, 170, 200, 170, 170, 250, 170, 170, 170, 200, 200, 170, 170, 250,
                    170, 170, 170, 200, 200, 170, 200, 250, 170, 170, 170, 170, 200, 170, 350, 150}

                Case Else ' ?? Double Dragon ?? A minor pentatonic, bluesy, gritty guitar
                    freqs = {
                    220, 262, 330, 440, 392, 330, 262, 220, 247, 294, 349, 494, 440, 349, 294, 247,
                    330, 392, 440, 523, 440, 392, 330, 262, 294, 330, 392, 440, 523, 440, 392, 330,
                    220, 262, 330, 440, 392, 330, 262, 220, 294, 330, 392, 440, 392, 330, 294, 0,
                    440, 523, 587, 659, 587, 523, 440, 392, 330, 392, 440, 523, 587, 523, 440, 330,
                    220, 0, 262, 294, 330, 294, 262, 0, 220, 262, 330, 440, 392, 330, 262, 220,
                    330, 392, 440, 523, 587, 523, 440, 392, 330, 262, 294, 330, 440, 392, 330, 0}
                    durs = {
                    190, 190, 190, 250, 190, 190, 190, 250, 190, 190, 190, 250, 190, 190, 190, 250,
                    190, 190, 190, 250, 190, 190, 190, 250, 190, 190, 190, 250, 190, 190, 190, 250,
                    190, 190, 190, 250, 190, 190, 250, 200, 190, 190, 190, 250, 200, 190, 350, 150,
                    200, 190, 190, 250, 190, 190, 190, 200, 190, 190, 190, 250, 200, 190, 190, 250,
                    250, 150, 200, 200, 250, 200, 350, 150, 190, 190, 190, 250, 190, 190, 190, 250,
                    190, 190, 190, 250, 190, 190, 190, 200, 190, 190, 190, 250, 250, 200, 400, 200}
            End Select
        End Sub

        ' =========================================================================
        ' MIDI GENERATOR � Format 1 multi-track: tempo + melody + bass.
        ' Bass line derived algorithmically from melody (root extraction).
        ' Velocity accents on beats 1 and 3 for rhythmic feel.
        ' =========================================================================
        Private Function GenerateMidiBytes() As Byte()
            Dim midi As New List(Of Byte)
            Const REF_BPM As Integer = 120
            Dim melodyInsts() = {73, 80, 10, 81, 42, 19, 88, 80, 30, 31}
            Dim bassInsts() = {33, 38, 33, 38, 38, 43, 39, 38, 34, 34}
            Dim harmInsts() = {48, 80, 52, 71, 43, 19, 92, 80, 28, 28}
            Dim bpms() = {90, 150, 140, 130, 70, 100, 50, 160, 145, 95}
            Dim si = Math.Min(_musicStyle, 9)
            Dim inst = melodyInsts(si), bassInst = bassInsts(si), harmInst = harmInsts(si)
            Dim playBpm = Math.Max(1, CInt(bpms(si) * _musicSpeed / 100.0))
            Dim usPerQN = CInt(60000000.0 / playBpm), tpq = 480
            Dim freqs() As Integer = Nothing, durs() As Integer = Nothing
            GetMusicData(_musicStyle, freqs, durs)
            ' Double for longer loop (96 ? 192 notes)
            Dim n = freqs.Length
            Dim f2(2 * n - 1) As Integer, d2(2 * n - 1) As Integer
            Array.Copy(freqs, 0, f2, 0, n) : Array.Copy(durs, 0, d2, 0, n)
            Array.Copy(freqs, 0, f2, n, n) : Array.Copy(durs, 0, d2, n, n)
            freqs = f2 : durs = d2
            ' Derive bass and harmony from melody
            Dim bassF() As Integer = Nothing, bassD() As Integer = Nothing
            DeriveBassLine(freqs, durs, bassF, bassD)
            Dim harmF() As Integer = Nothing, harmD() As Integer = Nothing
            DeriveHarmonyTrack(freqs, durs, harmF, harmD)
            ' ?? Track 0: Tempo ??
            Dim trk0 As New List(Of Byte)
            MidiVL(trk0, 0) : trk0.Add(&HFF) : trk0.Add(&H51) : trk0.Add(3)
            trk0.Add(CByte((usPerQN >> 16) And &HFF))
            trk0.Add(CByte((usPerQN >> 8) And &HFF))
            trk0.Add(CByte(usPerQN And &HFF))
            MidiVL(trk0, 0) : trk0.Add(&HFF) : trk0.Add(&H2F) : trk0.Add(0)
            ' ?? Track 1: Melody (channel 0) ??
            Dim trk1 As New List(Of Byte)
            MidiVL(trk1, 0) : trk1.Add(&HB0) : trk1.Add(7)
            trk1.Add(CByte(Math.Min(127, CInt(_musicVolume * 1.27))))
            MidiVL(trk1, 0) : trk1.Add(&HC0) : trk1.Add(CByte(inst))
            WriteMidiNotes(trk1, &H90, &H80, freqs, durs, tpq, REF_BPM, 100)
            MidiVL(trk1, 0) : trk1.Add(&HFF) : trk1.Add(&H2F) : trk1.Add(0)
            ' ?? Track 2: Bass (channel 1) ??
            Dim trk2 As New List(Of Byte)
            MidiVL(trk2, 0) : trk2.Add(&HB1) : trk2.Add(7)
            trk2.Add(CByte(Math.Min(127, CInt(_musicVolume * 1.1))))
            MidiVL(trk2, 0) : trk2.Add(&HC1) : trk2.Add(CByte(bassInst))
            WriteMidiNotes(trk2, &H91, &H81, bassF, bassD, tpq, REF_BPM, 75)
            MidiVL(trk2, 0) : trk2.Add(&HFF) : trk2.Add(&H2F) : trk2.Add(0)
            ' Track 3: harmony/pad (channel 2)
            Dim trk3 As New List(Of Byte)
            MidiVL(trk3, 0) : trk3.Add(&HB2) : trk3.Add(7)
            trk3.Add(CByte(Math.Min(127, CInt(_musicVolume * 0.9))))
            MidiVL(trk3, 0) : trk3.Add(&HC2) : trk3.Add(CByte(harmInst))
            WriteMidiNotes(trk3, &H92, &H82, harmF, harmD, tpq, REF_BPM, 55)
            MidiVL(trk3, 0) : trk3.Add(&HFF) : trk3.Add(&H2F) : trk3.Add(0)
            ' ?? MIDI Header: Format 1, 3 tracks ??
            midi.AddRange(Encoding.ASCII.GetBytes("MThd"))
            BE32(midi, 6) : BE16(midi, 1) : BE16(midi, 4) : BE16(midi, tpq)
            midi.AddRange(Encoding.ASCII.GetBytes("MTrk"))
            BE32(midi, trk0.Count) : midi.AddRange(trk0)
            midi.AddRange(Encoding.ASCII.GetBytes("MTrk"))
            BE32(midi, trk1.Count) : midi.AddRange(trk1)
            midi.AddRange(Encoding.ASCII.GetBytes("MTrk"))
            BE32(midi, trk2.Count) : midi.AddRange(trk2)
            midi.AddRange(Encoding.ASCII.GetBytes("MTrk"))
            BE32(midi, trk3.Count) : midi.AddRange(trk3)
            Return midi.ToArray()
        End Function

        Private Sub WriteMidiNotes(trk As List(Of Byte), noteOnCmd As Byte, noteOffCmd As Byte,
                               freqs() As Integer, durs() As Integer,
                               tpq As Integer, bpm As Integer, baseVel As Integer)
            Dim pd = 0
            For i = 0 To freqs.Length - 1
                Dim ticks = CInt(durs(i) * tpq * bpm / 60000.0)
                If ticks < 1 Then ticks = 1
                If freqs(i) <= 0 Then pd += ticks : Continue For
                Dim nt = CInt(Math.Max(0, Math.Min(127, Math.Round(69.0 + 12.0 * Math.Log(freqs(i) / 440.0) / Math.Log(2.0)))))
                ' Accent beats 1 and 3 for rhythmic dynamics
                Dim vel = CByte(Math.Min(127, baseVel + If(i Mod 4 = 0, 18, If(i Mod 4 = 2, 8, 0))))
                MidiVL(trk, pd) : trk.Add(noteOnCmd) : trk.Add(CByte(nt)) : trk.Add(vel)
                Dim onT = CInt(ticks * 0.85) : If onT < 1 Then onT = 1
                MidiVL(trk, onT) : trk.Add(noteOffCmd) : trk.Add(CByte(nt)) : trk.Add(0)
                pd = ticks - onT
            Next
        End Sub

        Private Sub DeriveBassLine(melFreqs() As Integer, melDurs() As Integer,
                               ByRef bassFreqs() As Integer, ByRef bassDurs() As Integer)
            Dim bf As New List(Of Integer), bd As New List(Of Integer)
            For i = 0 To melFreqs.Length - 1 Step 4
                Dim root = 0, dur = 0
                For j = i To Math.Min(i + 3, melFreqs.Length - 1)
                    dur += melDurs(j)
                    If melFreqs(j) > 0 AndAlso (root = 0 OrElse melFreqs(j) < root) Then root = melFreqs(j)
                Next
                If root > 0 Then
                    ' Transpose to bass register (below 300 Hz)
                    While root > 300 : root = root \ 2 : End While
                    ' Root note for 65%, fifth for 25%, rest 10%
                    Dim fifth = CInt(root * 1.5)
                    While fifth > 400 : fifth = fifth \ 2 : End While
                    bf.Add(root) : bd.Add(CInt(dur * 0.65))
                    bf.Add(fifth) : bd.Add(CInt(dur * 0.25))
                    bf.Add(0) : bd.Add(CInt(dur * 0.1))
                Else
                    bf.Add(0) : bd.Add(dur)
                End If
            Next
            bassFreqs = bf.ToArray() : bassDurs = bd.ToArray()
        End Sub

        Private Sub DeriveHarmonyTrack(melFreqs() As Integer, melDurs() As Integer,
                                       ByRef harmFreqs() As Integer, ByRef harmDurs() As Integer)
            Dim hf As New List(Of Integer), hd As New List(Of Integer)
            For i = 0 To melFreqs.Length - 1 Step 8
                Dim root = 0, dur = 0
                For j = i To Math.Min(i + 7, melFreqs.Length - 1)
                    dur += melDurs(j)
                    If melFreqs(j) > 0 AndAlso (root = 0 OrElse melFreqs(j) < root) Then root = melFreqs(j)
                Next
                If root > 0 Then
                    While root > 520 : root = root \ 2 : End While
                    While root < 260 : root = root * 2 : End While
                    Dim fifth = CInt(root * 1.498)
                    hf.Add(root) : hd.Add(CInt(dur * 0.48))
                    hf.Add(fifth) : hd.Add(CInt(dur * 0.38))
                    hf.Add(0) : hd.Add(CInt(dur * 0.14))
                Else
                    hf.Add(0) : hd.Add(dur)
                End If
            Next
            harmFreqs = hf.ToArray() : harmDurs = hd.ToArray()
        End Sub

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
                Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_wpf_music_v3")
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
                System.Diagnostics.Debug.WriteLine($"[MUSIC-WPF] Start style={_musicStyle} vol={GetEffectiveMusicVolume()}")
                mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
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
                mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
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
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_wpf_music_v3")
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
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_wpf_music_v3")
            If Directory.Exists(tmpDir) Then Try : Directory.Delete(tmpDir, True) : Catch : End Try
        Catch : End Try
    End Sub
#End Region

End Class
End Namespace
