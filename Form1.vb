' =============================================================================
' ANIME FINDER — WinForms Version
' Team Fast Talk | CS-120 | .NET 10 | GDI+ Rendering
'
' Single-file arcade brick-breaker with procedural audio, 7 power-ups,
' 10 music styles, 5 SFX packs, combo scoring, and colorblind mode.
' All assets generated at runtime — zero external files.
' =============================================================================
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Text
Imports System.Text.Json

Public Class Form1

#Region "Win32 Sound API"
    <DllImport("winmm.dll", SetLastError:=True)>
    Private Shared Function PlaySound(pszSound As Byte(), hmod As IntPtr, fdwSound As UInteger) As Boolean
    End Function

    <DllImport("winmm.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function mciSendString(command As String, buffer As StringBuilder, bufferSize As Integer, hwndCallback As IntPtr) As Integer
    End Function

    ' XInput gamepad support (xinput1_4 on Win8+)
    <DllImport("xinput1_4.dll", EntryPoint:="XInputGetState", SetLastError:=True)>
    Private Shared Function XInputGetState(dwUserIndex As Integer, ByRef pState As XINPUT_STATE) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure XINPUT_GAMEPAD
        Public wButtons As UShort
        Public bLeftTrigger As Byte
        Public bRightTrigger As Byte
        Public sThumbLX As Short
        Public sThumbLY As Short
        Public sThumbRX As Short
        Public sThumbRY As Short
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure XINPUT_STATE
        Public dwPacketNumber As Integer
        Public Gamepad As XINPUT_GAMEPAD
    End Structure

    Private Const XINPUT_GAMEPAD_DPAD_UP As UShort = &H1US
    Private Const XINPUT_GAMEPAD_DPAD_DOWN As UShort = &H2US
    Private Const XINPUT_GAMEPAD_DPAD_LEFT As UShort = &H4US
    Private Const XINPUT_GAMEPAD_DPAD_RIGHT As UShort = &H8US
    Private Const XINPUT_GAMEPAD_START As UShort = &H10US
    Private Const XINPUT_GAMEPAD_BACK As UShort = &H20US
    Private Const XINPUT_GAMEPAD_A As UShort = &H1000US
    Private Const XINPUT_GAMEPAD_B As UShort = &H2000US
    Private Const XINPUT_GAMEPAD_X As UShort = &H4000US
    Private Const XINPUT_GAMEPAD_Y As UShort = &H8000US
    Private Const XINPUT_GAMEPAD_LEFT_SHOULDER As UShort = &H100US
    Private Const XINPUT_GAMEPAD_RIGHT_SHOULDER As UShort = &H200US

    Private Const SND_ASYNC As UInteger = &H1
    Private Const SND_MEMORY As UInteger = &H4
    Private Const MM_MCINOTIFY As Integer = &H3B9
    Private Const MCI_NOTIFY_SUCCESSFUL As Integer = 1
    Private Const MutexName As String = "AnimeFinder_CS120_SingleInstance"
    Private Shared _appMutex As System.Threading.Mutex

    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = MM_MCINOTIFY AndAlso m.WParam.ToInt32() = MCI_NOTIFY_SUCCESSFUL AndAlso _musicPlaying Then
            ' Reject notifications that arrive within 2s of a play command.
            ' Prevents stale queued MCI_NOTIFY_SUCCESSFUL (from a just-finished
            ' track) from firing again after we have already started a new track.
            If Environment.TickCount64 - _musicLastStartMs < 2000L Then
                MyBase.WndProc(m)
                Return
            End If
            _musicPlaying = False
            If _usingHighScoreMusic Then
                ScheduleHighScoreMusicStart(10)
            Else
                _musicStyle = (_musicStyle + 1) Mod 10
                ScheduleMusicStart(10)
            End If
        End If
        MyBase.WndProc(m)
    End Sub
#End Region

#Region "Game Constants"
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
    Private Const BRICK_WIDTH As Integer = 65
    Private Const BRICK_HEIGHT As Integer = 22
    Private Const BRICK_PADDING As Integer = 4
    Private Const BRICK_TOP_OFFSET As Integer = 70
    Private Const BRICK_LEFT_OFFSET As Integer = 27

    Private Const MAX_LIVES As Integer = 10
    Private Const POWERUP_SIZE As Integer = 45
    Private Const POWERUP_SPEED As Single = 3.0F
    Private Const PARTICLE_COUNT As Integer = 8

    Private Const LOGICAL_WIDTH As Integer = 1200
    Private Const LOGICAL_HEIGHT As Integer = 867
#End Region

#Region "Game State Enum"
    Private Enum GameState
        Menu
        Playing
        Paused
        LevelComplete
        Options
        HighScore
    End Enum
#End Region

#Region "Data Structures"
    Private Structure Ball
        Public X As Single
        Public Y As Single
        Public DX As Single
        Public DY As Single
        Public Speed As Single
        Public Active As Boolean
    End Structure

    Private Structure Brick
        Public Rect As RectangleF
        Public Color1 As Color
        Public Color2 As Color
        Public Alive As Boolean
        Public HitsLeft As Integer
        Public Points As Integer
        Public Row As Integer
    End Structure

    Private Enum PowerUpType
        BlueBallGrow
        RedBallShrink
        GreenMultiBall
        YellowBallShrink
        PurplePaddleMega
        OrangeBallSlow
        PinkBallFast
    End Enum

    Private Structure PowerUp
        Public X As Single
        Public Y As Single
        Public PType As PowerUpType
        Public Active As Boolean
        Public Color1 As Color
        Public Symbol As String
    End Structure

    Private Structure Particle
        Public X As Single
        Public Y As Single
        Public DX As Single
        Public DY As Single
        Public Life As Single
        Public MaxLife As Single
        Public ParticleColor As Color
        Public Size As Single
        Public Active As Boolean
    End Structure

    Private Structure ScoreRecord
        Public PlayerName As String
        Public PlayerScore As Integer
    End Structure
#End Region

#Region "Game Variables"
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
    Private _gamepadLeft As Boolean = False
    Private _gamepadRight As Boolean = False
    Private _prevGamepadButtons As UShort = 0
    Private _gamepadAvailable As Boolean = True
    Private _touchActive As Boolean = False
    Private _touchX As Single = -1

    Private _rng As New Random()
    Private _frameCount As Integer = 0
    Private _screenShake As Integer = 0
    Private _flashTimer As Integer = 0

    Private _rowColors As Color()() = {
        New Color() {Color.FromArgb(255, 60, 80), Color.FromArgb(255, 100, 120)},
        New Color() {Color.FromArgb(255, 140, 50), Color.FromArgb(255, 180, 90)},
        New Color() {Color.FromArgb(255, 220, 50), Color.FromArgb(255, 240, 120)},
        New Color() {Color.FromArgb(50, 220, 100), Color.FromArgb(100, 255, 150)},
        New Color() {Color.FromArgb(50, 180, 255), Color.FromArgb(100, 210, 255)},
        New Color() {Color.FromArgb(130, 80, 255), Color.FromArgb(170, 130, 255)},
        New Color() {Color.FromArgb(255, 80, 200), Color.FromArgb(255, 140, 230)}
    }

    Private _starFieldX() As Single
    Private _starFieldY() As Single
    Private _starFieldSpeed() As Single
    Private _starFieldBright() As Integer

    Private _sfxVolume As Integer = 80
    Private _musicVolume As Integer = 100
    Private _musicSpeed As Integer = 75
    Private _colorblindMode As Boolean = False
    Private _speedBoost As Boolean = False
    Private _settingsSelection As Integer = 0
    Private _previousState As GameState = GameState.Menu
    Private _musicTempFile As String = ""
    Private _musicPlaying As Boolean = False
    Private _musicLastStartMs As Long = 0L
    Private _musicFiles() As String = Nothing
    Private _lastSfxBuffer As Byte() = Nothing
    Private _musicStyle As Integer = 2
    Private _sfxStyle As Integer = 0
    Private _musicChangeTimer As Timer = Nothing
    Private _pendingHighScore As Boolean = False
    Private _highScoreDelayFrames As Integer = 0
    Private _highScoreMusicFile As String = ""
    Private _usingHighScoreMusic As Boolean = False

    Private _windowScale As Integer = 2
    Private _windowScaleNames() As String = {"Small (900x650)", "Medium (1200x867)", "Large (1500x1083)", "XL (1800x1300)"}
    Private _windowScaleSizes()() As Integer = {
        New Integer() {900, 650},
        New Integer() {1200, 867},
        New Integer() {1500, 1083},
        New Integer() {1800, 1300}}

    Private _highScores As New List(Of ScoreRecord)
    Private _nameInput As String = ""
    Private _highScoreSaved As Boolean = False
    Private ReadOnly _highScorePath As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BrickBlast", "highscores.json")
    Private _getReadyFrames As Integer = 0
    Private _sprites As New Dictionary(Of String, Bitmap)
    ' Cached fonts — created once in Form1_Load, disposed in Form1_FormClosing
    Private _fnt8b As Font, _fnt10b As Font, _fnt10r As Font
    Private _fnt11b As Font, _fnt11r As Font, _fnt12b As Font, _fnt12r As Font
    Private _fnt13b As Font, _fnt14b As Font, _fnt14r As Font
    Private _fnt16r As Font, _fnt18r As Font, _fnt18b As Font
    Private _fnt20b As Font, _fnt22b As Font, _fnt30b As Font, _fnt12c As Font
    Private _brShadow As SolidBrush

    Private _colorblindColors As Color()() = {
        New Color() {Color.FromArgb(0, 114, 178), Color.FromArgb(60, 150, 210)},
        New Color() {Color.FromArgb(230, 159, 0), Color.FromArgb(255, 195, 60)},
        New Color() {Color.FromArgb(240, 228, 66), Color.FromArgb(255, 245, 140)},
        New Color() {Color.FromArgb(0, 158, 115), Color.FromArgb(60, 200, 155)},
        New Color() {Color.FromArgb(213, 94, 0), Color.FromArgb(245, 140, 50)},
        New Color() {Color.FromArgb(86, 180, 233), Color.FromArgb(140, 210, 245)},
        New Color() {Color.FromArgb(204, 121, 167), Color.FromArgb(235, 170, 200)}
    }
    Private _colorblindSymbols() As String = {ChrW(&H25A0), ChrW(&H25B2), ChrW(&H25CF), ChrW(&H2666), ChrW(&H2605), ChrW(&H25C6), ChrW(&H2663)}

    Private _musicStyleNames() As String = {
        "Style 1", "Style 2", "Style 3", "Style 4",
        "Style 5", "Style 6", "Style 7", "Style 8",
        "Style 9", "Style 10",
        "Style 11", "Style 12", "Style 13", "Style 14", "Style 15"}

    Private _sfxStyleNames() As String = {"Classic", "Style B", "Style C", "Style D", "Retro Arcade"}

    Private _sfxData()() As Integer = {
        New Integer() {300, 60, 500, 80, 600, 80, 1000, 120, 200, 400, 900, 300},
        New Integer() {660, 50, 880, 70, 1047, 70, 1319, 100, 330, 350, 1047, 350},
        New Integer() {784, 40, 1047, 50, 1319, 55, 1568, 90, 392, 300, 1568, 300},
        New Integer() {440, 55, 523, 70, 659, 75, 880, 110, 220, 400, 880, 350},
        New Integer() {250, 65, 400, 80, 500, 85, 800, 120, 150, 400, 700, 350}}
#End Region

#Region "Form Events"
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim createdNew As Boolean
        _appMutex = New System.Threading.Mutex(True, MutexName, createdNew)
        If Not createdNew Then
            MessageBox.Show(
                "Anime Finder is already running." & vbNewLine &
                "Close the other window first, then press F5.",
                "Already Running",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            _appMutex.Dispose()
            _appMutex = Nothing
            Me.Close()
            Return
        End If
        Me.DoubleBuffered = True
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        Me.UpdateStyles()
        LoadSprites()
        GenerateProceduralSprites()
        _fnt8b = New Font("Segoe UI", 8, FontStyle.Bold)
        _fnt10b = New Font("Segoe UI", 10, FontStyle.Bold)
        _fnt10r = New Font("Segoe UI", 10, FontStyle.Regular)
        _fnt11b = New Font("Segoe UI", 11, FontStyle.Bold)
        _fnt11r = New Font("Segoe UI", 11, FontStyle.Regular)
        _fnt12b = New Font("Segoe UI", 12, FontStyle.Bold)
        _fnt12r = New Font("Segoe UI", 12, FontStyle.Regular)
        _fnt13b = New Font("Segoe UI", 13, FontStyle.Bold)
        _fnt14b = New Font("Segoe UI", 14, FontStyle.Bold)
        _fnt14r = New Font("Segoe UI", 14, FontStyle.Regular)
        _fnt16r = New Font("Segoe UI", 16, FontStyle.Regular)
        _fnt18r = New Font("Segoe UI", 18, FontStyle.Regular)
        _fnt18b = New Font("Segoe UI", 18, FontStyle.Bold)
        _fnt20b = New Font("Segoe UI", 20, FontStyle.Bold)
        _fnt22b = New Font("Segoe UI", 22, FontStyle.Bold)
        _fnt30b = New Font("Segoe UI", 30, FontStyle.Bold)
        _fnt12c = New Font("Consolas", 12, FontStyle.Regular)
        _brShadow = New SolidBrush(Color.FromArgb(180, 0, 0, 0))
        InitStarField()
        _state = GameState.Menu
        LoadHighScores()
        PreGenerateAllMusic()
        GameTimer.Enabled = True
        StartMusic()
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        InitStarField()
        Me.Invalidate()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        For Each fnt As Font In {_fnt8b, _fnt10b, _fnt10r, _fnt11b, _fnt11r, _fnt12b, _fnt12r,
                                  _fnt13b, _fnt14b, _fnt14r, _fnt16r, _fnt18r, _fnt18b,
                                  _fnt20b, _fnt22b, _fnt30b, _fnt12c}
            If fnt IsNot Nothing Then Try : fnt.Dispose() : Catch : End Try
        Next
        If _brShadow IsNot Nothing Then _brShadow.Dispose()
        DisposeSprites()
        CleanupMusic()
        If _appMutex IsNot Nothing Then
            Try : _appMutex.ReleaseMutex() : Catch : End Try
            _appMutex.Dispose()
            _appMutex = Nothing
        End If
    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        ' Alt+Enter toggles maximize; Alt+F4 passes through to system
        If e.Alt AndAlso e.KeyCode = Keys.Return Then
            Me.WindowState = If(Me.WindowState = FormWindowState.Maximized, FormWindowState.Normal, FormWindowState.Maximized)
            e.Handled = True
            Return
        End If
        If _pendingHighScore Then
            Return
        End If
        If _state = GameState.HighScore Then
            If e.KeyCode = Keys.Back Then
                If _nameInput.Length > 0 Then _nameInput = _nameInput.Substring(0, _nameInput.Length - 1)
            ElseIf e.KeyCode = Keys.Enter Then
                If _nameInput.Length > 0 AndAlso Not _highScoreSaved Then
                    AddHighScore(_nameInput, _score)
                    _highScoreSaved = True
                End If
            ElseIf e.KeyCode = Keys.Space AndAlso _highScoreSaved Then
                _usingHighScoreMusic = False
                _pendingHighScore = False
                StartMusic()
                _state = GameState.Menu
            ElseIf Not _highScoreSaved Then
                Dim c = KeyToChar(e)
                If c IsNot Nothing AndAlso _nameInput.Length < 12 Then _nameInput &= c
            End If
            Return
        End If

        If _state = GameState.Options Then
            Select Case e.KeyCode
                Case Keys.Up
                    _settingsSelection = (_settingsSelection - 1 + 7) Mod 7
                Case Keys.Down
                    _settingsSelection = (_settingsSelection + 1) Mod 7
                Case Keys.Left
                    If _settingsSelection = 0 Then _sfxVolume = Math.Max(0, _sfxVolume - 5) : UpdateMusicVolume()
                    If _settingsSelection = 1 Then _musicVolume = Math.Max(0, _musicVolume - 5) : UpdateMusicVolume()
                    If _settingsSelection = 2 Then
                        _musicSpeed = Math.Max(10, _musicSpeed - 5)
                        RegenerateCurrentMusicFile()
                        ChangeMusic()
                    End If
                    If _settingsSelection = 3 Then
                        _musicStyle = (_musicStyle - 1 + 10) Mod 10
                        ChangeMusic()
                    End If
                    If _settingsSelection = 4 Then _sfxStyle = (_sfxStyle - 1 + 5) Mod 5
                    If _settingsSelection = 6 Then
                        _windowScale = (_windowScale - 1 + _windowScaleSizes.Length) Mod _windowScaleSizes.Length
                        ApplyWindowScale()
                    End If
                Case Keys.Right
                    If _settingsSelection = 0 Then _sfxVolume = Math.Min(100, _sfxVolume + 5) : UpdateMusicVolume()
                    If _settingsSelection = 1 Then _musicVolume = Math.Min(100, _musicVolume + 5) : UpdateMusicVolume()
                    If _settingsSelection = 2 Then
                        _musicSpeed = Math.Min(200, _musicSpeed + 5)
                        RegenerateCurrentMusicFile()
                        ChangeMusic()
                    End If
                    If _settingsSelection = 3 Then
                        _musicStyle = (_musicStyle + 1) Mod 10
                        ChangeMusic()
                    End If
                    If _settingsSelection = 4 Then _sfxStyle = (_sfxStyle + 1) Mod 5
                    If _settingsSelection = 6 Then
                        _windowScale = (_windowScale + 1) Mod _windowScaleSizes.Length
                        ApplyWindowScale()
                    End If
                Case Keys.Enter, Keys.Space
                    If _settingsSelection = 5 Then _colorblindMode = Not _colorblindMode
                Case Keys.O, Keys.H, Keys.Escape
                    _state = _previousState
            End Select
            Return
        End If

        Select Case e.KeyCode
            Case Keys.Left, Keys.A
                _leftPressed = True
            Case Keys.Right, Keys.D
                _rightPressed = True
            Case Keys.Space
                If _state = GameState.Menu Then
                    StartNewGame()
                ElseIf _state = GameState.LevelComplete Then
                    NextLevel()
                ElseIf _state = GameState.Paused Then
                    _state = GameState.Playing
                ElseIf _state = GameState.Playing Then
                    _speedBoost = Not _speedBoost
                    PlaySFX(_sfxData(_sfxStyle)(10), 80)
                End If
            Case Keys.P, Keys.Escape
                If _state = GameState.Playing Then
                    _state = GameState.Paused
                ElseIf _state = GameState.Paused Then
                    _state = GameState.Playing
                End If
            Case Keys.F
                If _state = GameState.Playing Then
                    _speedBoost = Not _speedBoost
                    PlaySFX(_sfxData(_sfxStyle)(10), 80)
                End If
            Case Keys.H, Keys.O
                If _state = GameState.Menu OrElse _state = GameState.Playing OrElse _state = GameState.Paused Then
                    _previousState = _state
                    _state = GameState.Options
                End If
        End Select
    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp
        Select Case e.KeyCode
            Case Keys.Left, Keys.A
                _leftPressed = False
            Case Keys.Right, Keys.D
                _rightPressed = False
        End Select
    End Sub

    Private Sub Form1_MouseMove(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        If _state = GameState.Playing AndAlso e.Button <> MouseButtons.None Then
            _touchActive = True
            _touchX = CSng(e.X) * LOGICAL_WIDTH / ClientSize.Width
        End If
    End Sub

    Private Sub Form1_MouseUp(sender As Object, e As MouseEventArgs) Handles MyBase.MouseUp
        _touchActive = False
    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown
        Dim mx = CSng(e.X) * LOGICAL_WIDTH / ClientSize.Width
        Dim my = CSng(e.Y) * LOGICAL_HEIGHT / ClientSize.Height
        ' Touch/click to start, resume, or advance level
        If _state = GameState.Menu Then
            StartNewGame()
            Return
        End If
        If _state = GameState.Paused AndAlso Not _pendingHighScore Then
            _state = GameState.Playing
            Return
        End If
        If _state = GameState.LevelComplete Then
            NextLevel()
            Return
        End If
        If _state = GameState.Playing Then
            ' Touch sets paddle target + speed boost
            _touchActive = True
            _touchX = mx
            AdjustBallSpeed(1.12F)
            PlaySFX(_sfxData(_sfxStyle)(10), 60)
            SpawnParticles(LOGICAL_WIDTH / 2, LOGICAL_HEIGHT / 2, Color.FromArgb(255, 200, 50), 6)
            Return
        End If
        If _state <> GameState.Options Then Return
        Dim pw = 780, ph = 600
        Dim px = CSng((LOGICAL_WIDTH - pw) / 2)
        Dim py = CSng((LOGICAL_HEIGHT - ph) / 2)
        Dim settingsY = py + 373
        Dim barX = px + 260
        For idx = 0 To 6
            Dim itemY = settingsY + idx * 28
            If my >= itemY AndAlso my < itemY + 30 Then
                _settingsSelection = idx
                If mx >= barX Then
                    AdjustSettingRight(idx)
                Else
                    AdjustSettingLeft(idx)
                End If
                Exit For
            End If
        Next
    End Sub

    Private Sub AdjustSettingLeft(idx As Integer)
        Select Case idx
            Case 0 : _sfxVolume = Math.Max(0, _sfxVolume - 5) : UpdateMusicVolume()
            Case 1 : _musicVolume = Math.Max(0, _musicVolume - 5) : UpdateMusicVolume()
            Case 2
                _musicSpeed = Math.Max(10, _musicSpeed - 5)
                RegenerateCurrentMusicFile()
                ChangeMusic()
            Case 3 : _musicStyle = (_musicStyle - 1 + 10) Mod 10 : ChangeMusic()
            Case 4 : _sfxStyle = (_sfxStyle - 1 + 5) Mod 5
            Case 5 : _colorblindMode = Not _colorblindMode
            Case 6 : _windowScale = (_windowScale - 1 + _windowScaleSizes.Length) Mod _windowScaleSizes.Length : ApplyWindowScale()
        End Select
    End Sub

    Private Sub AdjustSettingRight(idx As Integer)
        Select Case idx
            Case 0 : _sfxVolume = Math.Min(100, _sfxVolume + 5) : UpdateMusicVolume()
            Case 1 : _musicVolume = Math.Min(100, _musicVolume + 5) : UpdateMusicVolume()
            Case 2
                _musicSpeed = Math.Min(200, _musicSpeed + 5)
                RegenerateCurrentMusicFile()
                ChangeMusic()
            Case 3 : _musicStyle = (_musicStyle + 1) Mod 10 : ChangeMusic()
            Case 4 : _sfxStyle = (_sfxStyle + 1) Mod 5
            Case 5 : _colorblindMode = Not _colorblindMode
            Case 6 : _windowScale = (_windowScale + 1) Mod _windowScaleSizes.Length : ApplyWindowScale()
        End Select
    End Sub

    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.InterpolationMode = InterpolationMode.NearestNeighbor

        Dim scaleX = CSng(ClientSize.Width) / LOGICAL_WIDTH
        Dim scaleY = CSng(ClientSize.Height) / LOGICAL_HEIGHT
        g.ScaleTransform(scaleX, scaleY)

        If _screenShake > 0 Then
            g.TranslateTransform(_rng.Next(-3, 4), _rng.Next(-3, 4))
        End If

        DrawStarField(g)

        Select Case _state
            Case GameState.Menu
                DrawMenu(g)
            Case GameState.Playing, GameState.Paused
                DrawGame(g)
                If _state = GameState.Paused Then DrawOverlay(g, "PAUSED", "Press SPACE to resume")
            Case GameState.LevelComplete
                DrawGame(g)
                DrawOverlay(g, $"LEVEL {_level} COMPLETE!", "Press SPACE for next level", True)
            Case GameState.Options
                If _previousState = GameState.Playing OrElse _previousState = GameState.Paused Then DrawGame(g)
                DrawOptions(g)
            Case GameState.HighScore
                DrawHighScore(g)
        End Select
    End Sub
#End Region

#Region "Game Loop"
    Private Sub GameTimer_Tick(sender As Object, e As EventArgs) Handles GameTimer.Tick
        _frameCount += 1
        UpdateStarField()
        PollGamepad()
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
        Me.Invalidate()
    End Sub
#End Region

#Region "Game Init"
    Private Sub StartNewGame()
        _score = 0
        _lives = MAX_LIVES
        _level = 1
        _combo = 0
        _comboTimer = 0
        _pendingHighScore = False
        _highScoreDelayFrames = 0
        _paddleWidth = PADDLE_WIDTH
        _paddleWidthTimer = 0
        _ballRadius = BALL_RADIUS
        _speedBoost = False
        _nameInput = ""
        _highScoreSaved = False
        _getReadyFrames = 0
        SetupLevel()
        _state = GameState.Playing
        PlaySFX(_sfxData(_sfxStyle)(10), 100)
    End Sub

    Private Sub NextLevel()
        _level += 1
        _combo = 0
        _comboTimer = 0
        _pendingHighScore = False
        _highScoreDelayFrames = 0
        _paddleWidth = PADDLE_WIDTH
        _paddleWidthTimer = 0
        _getReadyFrames = 0
        SetupLevel()
        _state = GameState.Playing
        PlaySFX(_sfxData(_sfxStyle)(10), 100)
    End Sub

    Private Sub SetupLevel()
        _paddleX = (LOGICAL_WIDTH - _paddleWidth) / 2.0F
        _balls.Clear()
        _powerUps.Clear()
        _particles.Clear()

        Dim b As Ball
        b.X = LOGICAL_WIDTH / 2.0F
        b.Y = LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT - _ballRadius - 2
        b.Speed = CSng(INITIAL_BALL_SPEED * Math.Pow(1.05, _level - 1))
        Dim angle = _rng.Next(220, 320) * Math.PI / 180.0
        b.DX = CSng(Math.Cos(angle) * b.Speed)
        b.DY = CSng(Math.Sin(angle) * b.Speed)
        If b.DY > 0 Then b.DY = -b.DY
        b.Active = True
        _balls.Add(b)

        Dim margin As Integer = 20
        Dim brickCols As Integer = BRICK_COLS
        Dim brickRows As Integer = Math.Min(10, BRICK_ROWS + CInt(Math.Floor((_level - 1) / 3.0)))
        Dim brickW As Integer = CInt((LOGICAL_WIDTH - 2 * margin - (brickCols - 1) * BRICK_PADDING) / brickCols)
        Dim brickH As Integer = CInt(brickW * BRICK_HEIGHT / CDbl(BRICK_WIDTH))
        Dim brickLeft As Integer = margin

        If _colorblindMode Then
            brickH = Math.Max(brickH, 28)
        End If

        Dim palette = If(_colorblindMode, _colorblindColors, _rowColors)
        _bricks.Clear()

        Dim pattern = (_level - 1) Mod 8

        For row = 0 To brickRows - 1
            For col = 0 To brickCols - 1
                Dim skip As Boolean = False
                Select Case pattern
                    Case 1 ' Checkerboard
                        skip = ((row + col) Mod 2 = 1)
                    Case 2 ' Diamond
                        Dim cr = Math.Abs(row - brickRows / 2.0)
                        Dim cc = Math.Abs(col - brickCols / 2.0)
                        skip = (cr + cc > (brickRows + brickCols) / 4.0 + 1)
                    Case 3 ' Fortress - hollow rectangle
                        skip = (row > 1 AndAlso row < brickRows - 2 AndAlso col > 1 AndAlso col < brickCols - 2)
                    Case 4 ' Horizontal stripes
                        skip = (row Mod 3 = 1)
                    Case 5 ' Cross pattern
                        Dim midR = brickRows \ 2
                        Dim midC = brickCols \ 2
                        skip = Not (Math.Abs(row - midR) <= 1 OrElse Math.Abs(col - midC) <= 1)
                    Case 6 ' Border only
                        skip = (row > 0 AndAlso row < brickRows - 1 AndAlso col > 0 AndAlso col < brickCols - 1)
                    Case 7 ' Random gaps
                        skip = (_rng.Next(100) < 30)
                End Select

                If skip Then Continue For

                Dim bk As Brick
                bk.Rect = New RectangleF(
                    brickLeft + col * (brickW + BRICK_PADDING),
                    BRICK_TOP_OFFSET + row * (brickH + BRICK_PADDING),
                    brickW, brickH)
                Dim ci = row Mod palette.Length
                bk.Color1 = palette(ci)(0)
                bk.Color2 = palette(ci)(1)
                bk.Row = row
                bk.Points = (brickRows - row) * 10

                ' Hit points scale with level
                Dim hits = 1
                If _level >= 2 AndAlso row < 2 Then hits = 2
                If _level >= 4 AndAlso row < 4 Then hits = 2
                If _level >= 6 Then hits = Math.Max(hits, If(row < 2, 3, 2))
                If _level >= 9 Then hits = Math.Max(hits, If(row < 3, 3, 2))
                If _level >= 12 Then hits = Math.Max(hits, If(row < 2, 4, 3))
                bk.HitsLeft = hits
                bk.Alive = True
                _bricks.Add(bk)
            Next
        Next
    End Sub

    Private Sub InitStarField()
        Dim count = 120
        ReDim _starFieldX(count - 1)
        ReDim _starFieldY(count - 1)
        ReDim _starFieldSpeed(count - 1)
        ReDim _starFieldBright(count - 1)
        For i = 0 To count - 1
            _starFieldX(i) = _rng.Next(0, LOGICAL_WIDTH)
            _starFieldY(i) = _rng.Next(0, LOGICAL_HEIGHT)
            _starFieldSpeed(i) = 0.2F + CSng(_rng.NextDouble()) * 0.6F
            _starFieldBright(i) = _rng.Next(60, 200)
        Next
    End Sub
#End Region

#Region "Sound System"
    Private Function GenerateWav(frequency As Integer, durationMs As Integer, volume As Integer) As Byte()
        Const SR As Integer = 22050
        Dim n = CInt(SR * durationMs / 1000.0)
        If n < 1 Then n = 1
        Dim fs = 44 + n
        Dim w(fs - 1) As Byte
        Encoding.ASCII.GetBytes("RIFF").CopyTo(w, 0)
        BitConverter.GetBytes(fs - 8).CopyTo(w, 4)
        Encoding.ASCII.GetBytes("WAVE").CopyTo(w, 8)
        Encoding.ASCII.GetBytes("fmt ").CopyTo(w, 12)
        BitConverter.GetBytes(16).CopyTo(w, 16)
        BitConverter.GetBytes(CShort(1)).CopyTo(w, 20)
        BitConverter.GetBytes(CShort(1)).CopyTo(w, 22)
        BitConverter.GetBytes(SR).CopyTo(w, 24)
        BitConverter.GetBytes(SR).CopyTo(w, 28)
        BitConverter.GetBytes(CShort(1)).CopyTo(w, 32)
        BitConverter.GetBytes(CShort(8)).CopyTo(w, 34)
        Encoding.ASCII.GetBytes("data").CopyTo(w, 36)
        BitConverter.GetBytes(n).CopyTo(w, 40)
        Dim amp = 127.0 * volume / 100.0
        For s = 0 To n - 1
            If frequency <= 0 Then w(44 + s) = 128 : Continue For
            Dim p = SR / CDbl(frequency)
            Dim sv As Double = If((s Mod CInt(Math.Max(1, p))) < CInt(Math.Max(1, p / 2)), amp, -amp)
            Dim env = 1.0
            Dim a2 = CInt(n * 0.05)
            Dim d2 = CInt(n * 0.15)
            If a2 > 0 AndAlso s < a2 Then env = s / CDbl(a2)
            If d2 > 0 AndAlso s > n - d2 Then env = (n - s) / CDbl(d2)
            w(44 + s) = CByte(Math.Max(0, Math.Min(255, CInt(128 + sv * env))))
        Next
        Return w
    End Function

    ' =========================================================================
    ' MUSIC DATA — 96-note compositions per style (6 sections of 16 notes).
    ' Doubled to 192 notes by GenerateMidiBytes for ~40s loops.
    ' All 15 styles sourced from TextFile1.txt (MusicXML P1–P15).
    ' Harmonic vocabulary: C major (262,330,392,523) / D minor (294,349,440,587)
    ' BPMs match XML tempo markings exactly.
    ' =========================================================================
    Private Sub GetMusicData(style As Integer, ByRef freqs() As Integer, ByRef durs() As Integer)
        Select Case style
            Case 0 ' P1 — C/D arpeggios, fast driving (160 BPM, Q=375)
                freqs = {
                262, 311, 392, 523, 466, 392, 311, 0, 294, 349, 440, 587, 523, 440, 349, 0,
                523, 622, 784, 622, 523, 466, 392, 0, 466, 523, 622, 784, 622, 523, 466, 392,
                262, 392, 523, 784, 622, 523, 392, 0, 311, 392, 466, 523, 622, 523, 466, 0,
                262, 330, 392, 523, 659, 784, 659, 523, 294, 349, 440, 587, 698, 880, 698, 587,
                523, 659, 784, 659, 523, 392, 262, 0, 587, 698, 880, 784, 659, 523, 349, 0,
                262, 330, 392, 523, 784, 659, 523, 392, 294, 349, 440, 587, 880, 784, 698, 587}
                durs = {
                188, 188, 188, 375, 375, 375, 188, 375, 188, 188, 188, 375, 375, 375, 188, 375,
                375, 375, 375, 188, 188, 188, 375, 375, 375, 375, 375, 188, 188, 188, 375, 375,
                188, 188, 375, 375, 375, 188, 188, 375, 188, 188, 375, 375, 375, 188, 188, 375,
                375, 188, 188, 188, 188, 188, 188, 375, 375, 188, 188, 188, 188, 188, 188, 375,
                188, 188, 375, 375, 188, 188, 188, 375, 188, 188, 375, 375, 188, 188, 188, 375,
                188, 188, 188, 375, 188, 188, 375, 375, 188, 188, 188, 375, 188, 188, 375, 750}

            Case 1 ' P2 — C/D heroic lyrical ascending (120 BPM, Q=500)
                freqs = {
                262, 392, 523, 659, 784, 659, 523, 392, 294, 440, 587, 698, 880, 784, 698, 587,
                523, 659, 784, 880, 784, 659, 523, 0, 587, 784, 880, 784, 698, 587, 440, 0,
                392, 523, 659, 784, 659, 523, 392, 262, 440, 587, 698, 880, 784, 698, 587, 440,
                784, 880, 1047, 880, 784, 659, 523, 392, 880, 1047, 880, 784, 698, 587, 523, 440,
                523, 659, 784, 880, 784, 659, 523, 392, 587, 698, 880, 784, 698, 587, 440, 349,
                392, 523, 659, 784, 659, 523, 392, 0, 440, 587, 698, 880, 698, 587, 440, 0}
                durs = {
                250, 250, 500, 500, 500, 250, 250, 500, 250, 250, 500, 500, 500, 250, 250, 500,
                500, 500, 500, 250, 250, 250, 500, 500, 500, 500, 500, 250, 250, 250, 500, 500,
                250, 250, 500, 500, 250, 250, 250, 500, 250, 250, 500, 500, 250, 250, 250, 500,
                500, 500, 500, 250, 250, 250, 250, 500, 500, 500, 500, 250, 250, 250, 250, 500,
                250, 250, 500, 500, 250, 250, 250, 500, 250, 250, 500, 500, 250, 250, 250, 500,
                250, 250, 500, 500, 250, 250, 500, 500, 250, 250, 500, 500, 250, 250, 500, 1000}

            Case 2 ' P3 — C/D descending folk runs (140 BPM, Q=430)
                freqs = {
                659, 523, 392, 262, 330, 392, 523, 659, 784, 659, 523, 392, 330, 262, 330, 0,
                784, 659, 523, 440, 523, 659, 784, 880, 880, 784, 659, 523, 440, 523, 659, 0,
                262, 330, 392, 523, 659, 784, 659, 523, 294, 349, 440, 587, 698, 880, 784, 698,
                523, 392, 262, 330, 392, 523, 659, 784, 587, 440, 349, 294, 440, 587, 698, 880,
                784, 659, 523, 392, 262, 330, 262, 392, 880, 784, 698, 587, 440, 349, 294, 440,
                659, 523, 392, 262, 330, 262, 330, 0, 784, 698, 587, 440, 349, 294, 0, 0}
                durs = {
                215, 215, 215, 430, 215, 215, 430, 215, 215, 215, 215, 430, 215, 215, 430, 215,
                430, 215, 215, 215, 215, 215, 430, 215, 430, 215, 215, 215, 215, 215, 430, 215,
                215, 215, 430, 430, 430, 430, 215, 430, 215, 215, 430, 430, 430, 430, 215, 430,
                430, 215, 215, 215, 215, 430, 430, 430, 430, 215, 215, 215, 215, 430, 430, 430,
                430, 215, 215, 215, 215, 215, 215, 430, 430, 215, 215, 215, 215, 215, 215, 430,
                430, 215, 215, 215, 215, 215, 430, 430, 430, 215, 215, 215, 215, 215, 430, 860}

            Case 3 ' P4 — C/D bouncy staccato with rests (100 BPM, Q=600)
                freqs = {
                659, 659, 0, 659, 0, 523, 659, 0, 784, 0, 523, 659, 392, 0, 392, 0,
                523, 0, 392, 0, 330, 392, 440, 0, 523, 0, 440, 523, 659, 784, 659, 0,
                784, 880, 0, 784, 784, 0, 659, 0, 523, 0, 440, 523, 587, 659, 784, 0,
                523, 0, 659, 0, 784, 880, 784, 659, 523, 0, 392, 523, 659, 784, 659, 0,
                880, 0, 784, 880, 0, 784, 659, 0, 523, 659, 784, 0, 659, 523, 392, 0,
                523, 659, 784, 880, 784, 659, 523, 0, 440, 587, 698, 880, 698, 587, 440, 0}
                durs = {
                300, 300, 200, 300, 200, 300, 600, 200, 600, 200, 300, 300, 600, 200, 600, 200,
                600, 200, 600, 200, 300, 300, 600, 200, 600, 200, 300, 300, 600, 600, 600, 200,
                600, 600, 200, 300, 600, 200, 600, 200, 600, 200, 300, 300, 300, 300, 600, 200,
                600, 200, 600, 200, 600, 600, 300, 600, 600, 200, 300, 300, 600, 600, 600, 200,
                600, 200, 300, 600, 200, 600, 600, 200, 300, 300, 600, 200, 600, 600, 600, 200,
                300, 300, 600, 600, 300, 300, 600, 200, 300, 300, 600, 600, 300, 300, 600, 1200}

            Case 4 ' P5 — C/D gothic ascending arpeggios (150 BPM, Q=400)
                freqs = {
                262, 330, 392, 523, 659, 523, 392, 262, 294, 349, 440, 587, 698, 587, 440, 294,
                392, 523, 659, 784, 784, 659, 523, 392, 440, 587, 698, 880, 880, 784, 698, 587,
                262, 392, 523, 659, 523, 392, 262, 0, 294, 440, 587, 698, 698, 587, 440, 0,
                523, 659, 784, 880, 1047, 880, 784, 659, 587, 698, 880, 1047, 880, 784, 698, 587,
                659, 523, 392, 262, 330, 262, 330, 392, 698, 587, 440, 294, 349, 294, 349, 440,
                262, 330, 392, 523, 392, 330, 262, 0, 294, 349, 440, 587, 440, 349, 0, 0}
                durs = {
                200, 200, 200, 400, 400, 200, 200, 400, 200, 200, 200, 400, 400, 200, 200, 400,
                200, 200, 400, 400, 200, 200, 200, 400, 200, 200, 400, 400, 200, 200, 200, 400,
                200, 200, 400, 400, 200, 200, 400, 400, 200, 200, 400, 400, 200, 200, 400, 400,
                400, 400, 400, 400, 200, 200, 200, 400, 400, 400, 400, 400, 200, 200, 200, 400,
                200, 200, 200, 200, 200, 200, 200, 400, 200, 200, 200, 200, 200, 200, 200, 400,
                200, 200, 200, 400, 200, 200, 400, 400, 200, 200, 200, 400, 200, 200, 400, 800}

            Case 5 ' P6 — C/D sweeping harp arpeggios (90 BPM, Q=667)
                freqs = {
                262, 330, 392, 523, 392, 330, 262, 0, 294, 349, 440, 587, 440, 349, 294, 0,
                523, 659, 784, 880, 784, 659, 523, 0, 587, 698, 880, 784, 698, 587, 440, 0,
                262, 330, 392, 523, 659, 523, 392, 330, 294, 349, 440, 587, 698, 587, 440, 349,
                523, 784, 1047, 784, 523, 392, 262, 0, 587, 880, 1047, 880, 698, 587, 440, 0,
                392, 523, 659, 784, 880, 784, 659, 523, 440, 587, 698, 880, 1047, 880, 784, 698,
                262, 330, 392, 523, 659, 784, 659, 523, 294, 349, 440, 587, 698, 880, 698, 0}
                durs = {
                334, 334, 667, 667, 334, 334, 667, 667, 334, 334, 667, 667, 334, 334, 667, 667,
                334, 334, 667, 667, 334, 334, 667, 667, 334, 334, 667, 667, 334, 334, 667, 667,
                334, 334, 334, 667, 334, 334, 334, 667, 334, 334, 334, 667, 334, 334, 334, 667,
                667, 667, 1334, 667, 334, 334, 667, 667, 667, 667, 1334, 667, 334, 334, 667, 667,
                334, 334, 334, 667, 667, 334, 334, 667, 334, 334, 334, 667, 667, 334, 334, 667,
                334, 334, 334, 667, 334, 334, 334, 667, 334, 334, 334, 667, 334, 334, 667, 1334}

            Case 6 ' P7 — C/D fast energetic pop (140 BPM, Q=430)
                freqs = {
                523, 659, 784, 880, 784, 659, 523, 0, 587, 698, 880, 784, 698, 587, 440, 0,
                784, 880, 1047, 880, 784, 659, 523, 392, 880, 1047, 880, 784, 698, 587, 440, 349,
                523, 659, 784, 523, 659, 784, 880, 784, 587, 698, 880, 698, 784, 880, 1047, 880,
                784, 659, 523, 392, 330, 392, 523, 659, 880, 784, 698, 587, 440, 440, 587, 698,
                523, 784, 1047, 784, 523, 0, 523, 784, 587, 880, 1047, 880, 698, 0, 587, 880,
                784, 659, 523, 392, 523, 392, 262, 0, 880, 784, 698, 587, 587, 440, 294, 0}
                durs = {
                215, 215, 430, 430, 215, 215, 430, 215, 215, 215, 430, 430, 215, 215, 430, 215,
                215, 215, 430, 215, 215, 215, 215, 430, 215, 215, 430, 215, 215, 215, 215, 430,
                215, 215, 430, 215, 215, 215, 430, 430, 215, 215, 430, 215, 215, 215, 430, 430,
                430, 215, 215, 215, 215, 215, 215, 430, 430, 215, 215, 215, 215, 215, 215, 430,
                215, 430, 430, 430, 215, 215, 215, 430, 215, 430, 430, 430, 215, 215, 215, 430,
                430, 215, 215, 215, 215, 215, 430, 430, 430, 215, 215, 215, 215, 215, 430, 860}

            Case 7 ' P8 — C/D sparse eerie with rests (130 BPM, Q=460)
                freqs = {
                262, 0, 0, 0, 523, 0, 0, 0, 294, 0, 0, 0, 587, 0, 0, 0,
                392, 0, 262, 0, 0, 0, 392, 0, 440, 0, 294, 0, 0, 0, 440, 0,
                262, 523, 0, 523, 262, 0, 0, 0, 294, 587, 0, 587, 294, 0, 0, 0,
                523, 659, 784, 659, 523, 392, 262, 0, 587, 698, 880, 698, 587, 440, 294, 0,
                262, 0, 523, 0, 659, 0, 523, 0, 294, 0, 587, 0, 698, 0, 587, 0,
                262, 0, 0, 0, 0, 0, 0, 0, 294, 0, 0, 0, 0, 0, 0, 0}
                durs = {
                460, 460, 460, 460, 460, 460, 460, 460, 460, 460, 460, 460, 460, 460, 460, 460,
                460, 460, 460, 920, 460, 460, 460, 460, 460, 460, 460, 920, 460, 460, 460, 460,
                460, 460, 460, 460, 920, 460, 460, 460, 460, 460, 460, 460, 920, 460, 460, 460,
                230, 230, 230, 230, 230, 230, 460, 460, 230, 230, 230, 230, 230, 230, 460, 460,
                460, 460, 460, 460, 460, 920, 460, 460, 460, 460, 460, 460, 460, 920, 460, 460,
                920, 920, 920, 460, 460, 460, 460, 920, 920, 920, 920, 460, 460, 460, 460, 1840}

            Case 8 ' P9 — C/D high-register angular (145 BPM, Q=415)
                freqs = {
                659, 784, 659, 523, 659, 784, 880, 784, 784, 880, 1047, 880, 784, 659, 523, 0,
                523, 659, 784, 880, 784, 659, 784, 880, 587, 698, 880, 1047, 880, 784, 880, 1047,
                659, 523, 392, 523, 659, 784, 659, 523, 784, 698, 587, 698, 784, 880, 784, 698,
                880, 784, 659, 523, 392, 523, 392, 523, 1047, 880, 784, 698, 587, 698, 587, 698,
                523, 659, 784, 880, 784, 659, 523, 392, 587, 698, 880, 1047, 880, 784, 698, 587,
                659, 784, 880, 784, 659, 523, 392, 0, 784, 880, 1047, 880, 784, 698, 587, 0}
                durs = {
                208, 208, 208, 208, 208, 208, 415, 208, 208, 208, 208, 208, 208, 208, 415, 208,
                208, 208, 415, 208, 208, 208, 415, 208, 208, 208, 415, 208, 208, 208, 415, 208,
                208, 208, 208, 208, 208, 415, 208, 415, 208, 208, 208, 208, 208, 415, 208, 415,
                415, 208, 208, 208, 208, 208, 208, 415, 415, 208, 208, 208, 208, 208, 208, 415,
                208, 208, 415, 415, 208, 208, 208, 415, 208, 208, 415, 415, 208, 208, 208, 415,
                208, 208, 415, 208, 208, 208, 415, 415, 208, 208, 415, 208, 208, 208, 415, 830}



            Case 9 ' P10 — C/D tense mysterious (100 BPM, Q=600)
                freqs = {
                294, 0, 349, 0, 440, 0, 587, 440, 349, 0, 294, 0, 262, 294, 349, 0,
                587, 523, 440, 0, 349, 440, 523, 0, 659, 587, 523, 440, 349, 294, 262, 0,
                392, 523, 659, 784, 659, 523, 392, 0, 440, 587, 698, 880, 784, 698, 587, 0,
                523, 659, 784, 659, 523, 440, 392, 349, 587, 698, 880, 784, 698, 587, 523, 440,
                294, 0, 349, 0, 440, 523, 587, 0, 440, 0, 349, 0, 294, 349, 440, 0,
                523, 440, 392, 349, 392, 440, 523, 0, 294, 262, 0, 0, 0, 0, 0, 0}
                durs = {
                600, 300, 600, 300, 600, 300, 600, 600, 600, 300, 600, 300, 600, 300, 600, 600,
                600, 300, 600, 300, 300, 300, 600, 300, 600, 300, 300, 300, 300, 300, 600, 300,
                300, 300, 600, 600, 300, 300, 600, 300, 300, 300, 600, 600, 300, 300, 600, 300,
                300, 300, 600, 300, 300, 300, 300, 300, 300, 300, 600, 300, 300, 300, 300, 300,
                600, 300, 600, 300, 300, 300, 600, 300, 600, 300, 600, 300, 300, 300, 600, 300,
                300, 300, 300, 300, 300, 300, 600, 300, 600, 600, 300, 300, 300, 300, 300, 1200}

            Case 10 ' P11 — C/D lighter folk alternate (120 BPM, Q=500)
                freqs = {
                523, 659, 784, 659, 523, 392, 523, 0, 587, 698, 880, 784, 698, 587, 698, 0,
                784, 880, 1047, 880, 784, 698, 784, 659, 880, 1047, 880, 784, 698, 659, 698, 784,
                523, 392, 330, 392, 523, 659, 784, 659, 587, 440, 349, 440, 587, 698, 880, 784,
                659, 523, 392, 262, 330, 392, 523, 659, 698, 587, 440, 349, 294, 349, 440, 587,
                523, 659, 784, 523, 784, 659, 523, 392, 587, 698, 880, 698, 880, 784, 698, 587,
                659, 523, 392, 330, 392, 523, 659, 784, 784, 698, 587, 523, 440, 523, 659, 0}
                durs = {
                500, 250, 500, 250, 250, 250, 500, 250, 500, 250, 500, 250, 250, 250, 500, 250,
                250, 250, 500, 250, 250, 250, 250, 500, 250, 250, 500, 250, 250, 250, 250, 500,
                250, 250, 250, 250, 250, 500, 250, 500, 250, 250, 250, 250, 250, 500, 250, 500,
                500, 250, 250, 250, 250, 250, 250, 500, 500, 250, 250, 250, 250, 250, 250, 500,
                250, 250, 500, 250, 500, 250, 250, 500, 250, 250, 500, 250, 500, 250, 250, 500,
                500, 250, 250, 250, 250, 250, 500, 500, 500, 250, 250, 250, 250, 250, 500, 1000}

            Case 11 ' P12 — C/D low mysterious bass (110 BPM, Q=545)
                freqs = {
                262, 294, 330, 262, 349, 330, 294, 262, 294, 330, 349, 294, 392, 349, 330, 294,
                262, 330, 392, 330, 262, 294, 330, 294, 349, 392, 440, 392, 349, 330, 349, 330,
                262, 294, 330, 349, 392, 330, 262, 0, 294, 330, 349, 392, 440, 349, 294, 0,
                262, 349, 440, 349, 262, 294, 330, 262, 294, 392, 494, 392, 294, 330, 349, 294,
                262, 330, 392, 523, 392, 330, 262, 0, 294, 349, 440, 587, 440, 349, 294, 0,
                262, 294, 330, 262, 294, 330, 349, 392, 349, 330, 294, 262, 294, 330, 262, 0}
                durs = {
                545, 273, 273, 545, 273, 273, 273, 545, 273, 273, 273, 273, 273, 273, 273, 545,
                273, 273, 545, 273, 273, 273, 545, 273, 273, 273, 545, 273, 273, 273, 545, 273,
                273, 273, 273, 273, 545, 273, 545, 273, 273, 273, 273, 273, 545, 273, 545, 273,
                273, 545, 545, 273, 273, 273, 545, 273, 273, 545, 545, 273, 273, 273, 545, 273,
                273, 273, 545, 545, 273, 273, 545, 273, 273, 273, 545, 545, 273, 273, 545, 273,
                273, 273, 273, 545, 273, 273, 273, 545, 273, 273, 273, 273, 273, 273, 545, 1090}

            Case 12 ' P13 — C/D darker alternate gothic (150 BPM, Q=400)
                freqs = {
                262, 0, 294, 0, 349, 392, 440, 0, 523, 0, 587, 0, 659, 784, 659, 0,
                523, 440, 392, 349, 294, 262, 294, 0, 587, 523, 440, 392, 349, 294, 262, 0,
                440, 523, 659, 784, 659, 523, 440, 392, 494, 587, 698, 880, 784, 698, 587, 523,
                392, 523, 659, 784, 880, 784, 659, 523, 440, 587, 698, 880, 1047, 880, 784, 698,
                262, 392, 523, 659, 784, 659, 523, 392, 294, 440, 587, 698, 880, 784, 698, 587,
                523, 440, 392, 349, 294, 262, 330, 392, 587, 523, 440, 392, 349, 294, 0, 0}
                durs = {
                400, 200, 400, 200, 200, 200, 400, 200, 400, 200, 400, 200, 200, 200, 400, 200,
                200, 200, 200, 200, 200, 400, 400, 200, 200, 200, 200, 200, 200, 200, 400, 200,
                200, 200, 400, 400, 200, 200, 200, 400, 200, 200, 400, 400, 200, 200, 200, 400,
                200, 200, 400, 400, 400, 200, 200, 400, 200, 200, 400, 400, 400, 200, 200, 400,
                200, 200, 400, 400, 200, 200, 200, 400, 200, 200, 400, 400, 200, 200, 200, 400,
                200, 200, 200, 200, 200, 200, 400, 400, 200, 200, 200, 200, 200, 200, 400, 800}

            Case 13 ' P14 — C/D classic arpeggio prelude (90 BPM, Q=334 eighth)
                freqs = {
                262, 392, 523, 262, 392, 523, 784, 523, 294, 440, 587, 294, 440, 587, 880, 587,
                262, 392, 523, 659, 523, 392, 262, 392, 294, 440, 587, 698, 587, 440, 294, 440,
                392, 523, 659, 784, 659, 523, 392, 523, 440, 587, 698, 880, 698, 587, 440, 587,
                523, 659, 784, 1047, 784, 659, 523, 659, 587, 698, 880, 1047, 880, 784, 698, 784,
                262, 392, 523, 784, 1047, 784, 523, 392, 294, 440, 587, 880, 1047, 880, 698, 587,
                262, 330, 392, 523, 659, 784, 880, 784, 294, 349, 440, 587, 698, 880, 1047, 0}
                durs = {
                334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334,
                334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334,
                334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334,
                334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334,
                334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334,
                334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 334, 1334}

            Case Else ' P15 — C/D fast alternate chase (140 BPM, Q=430)
                freqs = {
                784, 880, 1047, 880, 784, 659, 523, 392, 880, 1047, 880, 784, 698, 587, 440, 349,
                523, 659, 784, 523, 784, 659, 523, 392, 587, 698, 880, 698, 880, 784, 698, 587,
                392, 523, 659, 784, 880, 1047, 880, 784, 440, 587, 698, 880, 1047, 880, 784, 698,
                784, 659, 523, 392, 262, 330, 392, 523, 880, 784, 698, 587, 440, 349, 440, 587,
                523, 784, 523, 784, 659, 523, 659, 784, 587, 880, 698, 880, 784, 698, 784, 880,
                659, 523, 392, 262, 330, 392, 523, 659, 784, 698, 587, 440, 349, 294, 0, 0}
                durs = {
                215, 215, 430, 215, 215, 215, 215, 430, 215, 215, 430, 215, 215, 215, 215, 430,
                215, 215, 430, 215, 430, 215, 215, 430, 215, 215, 430, 215, 430, 215, 215, 430,
                215, 215, 215, 430, 430, 430, 215, 430, 215, 215, 215, 430, 430, 430, 215, 430,
                430, 215, 215, 215, 215, 215, 215, 430, 430, 215, 215, 215, 215, 215, 215, 430,
                215, 430, 215, 430, 215, 215, 215, 430, 215, 430, 215, 430, 215, 215, 215, 430,
                430, 215, 215, 215, 215, 215, 215, 430, 430, 215, 215, 215, 215, 215, 430, 860}
        End Select
    End Sub

    Private Function GenerateMidiBytes() As Byte()
        Const REF_BPM As Integer = 120
        Dim midi As New List(Of Byte)
        Dim melodyInsts() = {80, 73, 10, 80, 19, 46, 80, 88, 80, 73, 73, 80, 19, 46, 80}
        Dim bpms() = {160, 120, 140, 100, 150, 90, 140, 130, 145, 100, 120, 110, 150, 90, 140}
        Dim si = Math.Min(_musicStyle, 14)
        Dim inst = melodyInsts(si)
        Dim playBpm = Math.Max(1, CInt(bpms(si) * _musicSpeed / 100.0))
        Dim usPerQN = CInt(60000000.0 / playBpm), tpq = 480

        Dim freqs() As Integer = Nothing, durs() As Integer = Nothing
        GetMusicData(_musicStyle, freqs, durs)

        ' Double for longer loop (96 -> 192 notes)
        Dim n = freqs.Length
        Dim f2(2 * n - 1) As Integer, d2(2 * n - 1) As Integer
        Array.Copy(freqs, 0, f2, 0, n) : Array.Copy(durs, 0, d2, 0, n)
        Array.Copy(freqs, 0, f2, n, n) : Array.Copy(durs, 0, d2, n, n)
        freqs = f2 : durs = d2

        ' Track 0: tempo
        Dim trk0 As New List(Of Byte)
        MidiVL(trk0, 0) : trk0.Add(&HFF) : trk0.Add(&H51) : trk0.Add(3)
        trk0.Add(CByte((usPerQN >> 16) And &HFF))
        trk0.Add(CByte((usPerQN >> 8) And &HFF))
        trk0.Add(CByte(usPerQN And &HFF))
        MidiVL(trk0, 0) : trk0.Add(&HFF) : trk0.Add(&H2F) : trk0.Add(0)

        ' Track 1: melody (channel 0)
        Dim trk1 As New List(Of Byte)
        MidiVL(trk1, 0) : trk1.Add(&HB0) : trk1.Add(7)
        trk1.Add(CByte(Math.Min(127, CInt(_musicVolume * 1.27))))
        MidiVL(trk1, 0) : trk1.Add(&HC0) : trk1.Add(CByte(inst))
        WriteMidiNotes(trk1, &H90, &H80, freqs, durs, tpq, REF_BPM, 100)
        MidiVL(trk1, 0) : trk1.Add(&HFF) : trk1.Add(&H2F) : trk1.Add(0)

        ' Header: format 1, 2 tracks (tempo + melody)
        midi.AddRange(Encoding.ASCII.GetBytes("MThd"))
        BE32(midi, 6) : BE16(midi, 1) : BE16(midi, 2) : BE16(midi, tpq)
        midi.AddRange(Encoding.ASCII.GetBytes("MTrk"))
        BE32(midi, trk0.Count) : midi.AddRange(trk0)
        midi.AddRange(Encoding.ASCII.GetBytes("MTrk"))
        BE32(midi, trk1.Count) : midi.AddRange(trk1)
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
                While root > 300 : root = root \ 2 : End While
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

    Private Sub PlaySFX(frequency As Integer, durationMs As Integer)
        Try
            If _sfxVolume <= 0 Then Return
            Dim wav = GenerateWav(frequency, durationMs, _sfxVolume)
            _lastSfxBuffer = wav
            PlaySound(wav, IntPtr.Zero, SND_ASYNC Or SND_MEMORY)
        Catch
        End Try
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
        Dim baseFreq = 800 + Math.Min(_combo, 10) * 100
        Dim dur = 100 + Math.Min(_combo, 6) * 15
        PlaySFX(baseFreq, dur)
    End Sub

    Private Sub PreGenerateAllMusic()
        Try
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_music_v4")
            If Not Directory.Exists(tmpDir) Then Directory.CreateDirectory(tmpDir)
            ReDim _musicFiles(9)
            For i = 0 To 9
                _musicFiles(i) = Path.Combine(tmpDir, $"style_{i}.mid")
                Dim oldStyle = _musicStyle
                _musicStyle = i
                File.WriteAllBytes(_musicFiles(i), GenerateMidiBytes())
                _musicStyle = oldStyle
            Next
            _musicTempFile = _musicFiles(_musicStyle)
        Catch
        End Try
    End Sub

    Private Sub RegenerateCurrentMusicFile()
        Try
            If _musicFiles Is Nothing Then Return
            If _musicStyle < 0 OrElse _musicStyle >= _musicFiles.Length Then Return
            Dim path = _musicFiles(_musicStyle)
            If String.IsNullOrEmpty(path) Then Return
            mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            _musicPlaying = False
            File.WriteAllBytes(path, GenerateMidiBytes())
            _musicTempFile = path
        Catch
        End Try
    End Sub

    Private Sub StartMusic()
        Try
            ' Cancel any pending music timer to prevent two tracks playing
            If _musicChangeTimer IsNot Nothing Then
                _musicChangeTimer.Stop()
                _musicChangeTimer.Dispose()
                _musicChangeTimer = Nothing
            End If
            If _musicFiles Is Nothing Then Return
            _musicTempFile = _musicFiles(_musicStyle)
            If String.IsNullOrEmpty(_musicTempFile) OrElse Not File.Exists(_musicTempFile) Then Return
            System.Diagnostics.Debug.WriteLine($"[MUSIC-MAIN] Start style={_musicStyle} vol={GetEffectiveMusicVolume()} speed={_musicSpeed}")
            mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("open """ & _musicTempFile & """ alias bgmusic", Nothing, 0, IntPtr.Zero)
            Dim vol = CInt(GetEffectiveMusicVolume() * 10)
            mciSendString("setaudio bgmusic volume to " & vol.ToString(), Nothing, 0, IntPtr.Zero)
            mciSendString("play bgmusic notify", Nothing, 0, Me.Handle)
            _musicPlaying = True
            _musicLastStartMs = Environment.TickCount64
        Catch
        End Try
    End Sub

    Private Sub StartMusicDirect()
        Try
            If _musicChangeTimer IsNot Nothing Then
                _musicChangeTimer.Stop()
                _musicChangeTimer.Dispose()
                _musicChangeTimer = Nothing
            End If
            If _musicFiles Is Nothing Then Return
            _musicTempFile = _musicFiles(_musicStyle)
            If String.IsNullOrEmpty(_musicTempFile) OrElse Not File.Exists(_musicTempFile) Then Return
            mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("open """ & _musicTempFile & """ alias bgmusic", Nothing, 0, IntPtr.Zero)
            Dim vol = CInt(GetEffectiveMusicVolume() * 10)
            mciSendString("setaudio bgmusic volume to " & vol.ToString(), Nothing, 0, IntPtr.Zero)
            mciSendString("play bgmusic notify", Nothing, 0, Me.Handle)
            _musicPlaying = True
            _musicLastStartMs = Environment.TickCount64
        Catch
        End Try
    End Sub

    Private Sub ScheduleMusicStart(delayMs As Integer)
        If _musicChangeTimer IsNot Nothing Then
            _musicChangeTimer.Stop()
            _musicChangeTimer.Dispose()
            _musicChangeTimer = Nothing
        End If
        _musicChangeTimer = New Timer With {.Interval = Math.Max(10, delayMs)}
        AddHandler _musicChangeTimer.Tick, Sub(s, ev)
                                               _musicChangeTimer.Stop()
                                               _musicChangeTimer.Dispose()
                                               _musicChangeTimer = Nothing
                                               StartMusicDirect()
                                           End Sub
        _musicChangeTimer.Start()
    End Sub

    Private Sub ScheduleHighScoreMusicStart(delayMs As Integer)
        If _musicChangeTimer IsNot Nothing Then
            _musicChangeTimer.Stop()
            _musicChangeTimer.Dispose()
            _musicChangeTimer = Nothing
        End If
        _musicChangeTimer = New Timer With {.Interval = Math.Max(10, delayMs)}
        AddHandler _musicChangeTimer.Tick, Sub(s, ev)
                                               _musicChangeTimer.Stop()
                                               _musicChangeTimer.Dispose()
                                               _musicChangeTimer = Nothing
                                               StartHighScoreMusic()
                                           End Sub
        _musicChangeTimer.Start()
    End Sub

    Private Sub StartHighScoreMusic()
        Try
            If _musicChangeTimer IsNot Nothing Then
                _musicChangeTimer.Stop()
                _musicChangeTimer.Dispose()
                _musicChangeTimer = Nothing
            End If
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_music_v4")
            If Not Directory.Exists(tmpDir) Then Directory.CreateDirectory(tmpDir)
            _highScoreMusicFile = Path.Combine(tmpDir, "highscore.mid")
            File.WriteAllBytes(_highScoreMusicFile, GenerateHighScoreMidiBytes())
            mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("open """ & _highScoreMusicFile & """ alias bgmusic", Nothing, 0, IntPtr.Zero)
            Dim vol = CInt(GetEffectiveMusicVolume() * 10)
            mciSendString("setaudio bgmusic volume to " & vol.ToString(), Nothing, 0, IntPtr.Zero)
            mciSendString("play bgmusic notify", Nothing, 0, Me.Handle)
            _musicPlaying = True
            _musicLastStartMs = Environment.TickCount64
            _usingHighScoreMusic = True
        Catch
        End Try
    End Sub

    Private Function GenerateHighScoreMidiBytes()
        Dim oldStyle = _musicStyle
        _musicStyle = 6
        Dim bytes = GenerateMidiBytes()
        _musicStyle = oldStyle
        Return bytes
    End Function

    Private Sub StopMusic()
        Try
            mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            _musicPlaying = False
        Catch
        End Try
    End Sub

    Private Sub CleanupMusic()
        Try
            StopMusic()
            If _musicFiles IsNot Nothing Then
                For Each f In _musicFiles
                    If Not String.IsNullOrEmpty(f) AndAlso File.Exists(f) Then
                        Try : File.Delete(f) : Catch : End Try
                    End If
                Next
            End If
            Dim tmpDir = Path.Combine(Path.GetTempPath(), "cl_brickblast_music_v4")
            If Directory.Exists(tmpDir) Then
                Try : Directory.Delete(tmpDir, True) : Catch : End Try
            End If
        Catch
        End Try
    End Sub

    Private Sub UpdateMusicVolume()
        Try
            Dim vol = CInt(GetEffectiveMusicVolume() * 10)
            mciSendString("setaudio bgmusic volume to " & vol.ToString(), Nothing, 0, IntPtr.Zero)
        Catch
        End Try
    End Sub

    Private Function GetEffectiveMusicVolume() As Integer
        Return Math.Max(0, Math.Min(_musicVolume, 100))
    End Function

    Private Sub ChangeMusic()
        Try
            mciSendString("stop bgmusic", Nothing, 0, IntPtr.Zero)
            mciSendString("close bgmusic", Nothing, 0, IntPtr.Zero)
            _musicPlaying = False
            ScheduleMusicStart(60)
        Catch
        End Try
    End Sub
    Private Sub ApplyWindowScale()
        Dim newW = _windowScaleSizes(_windowScale)(0)
        Dim newH = _windowScaleSizes(_windowScale)(1)
        Me.ClientSize = New Size(newW, newH)
        Me.CenterToScreen()
        InitStarField()
    End Sub

    ' =============================================================
    ' GAMEPAD — poll XInput controller each frame
    ' =============================================================
    Private Sub PollGamepad()
        _gamepadLeft = False
        _gamepadRight = False
        If Not _gamepadAvailable Then Return
        Try
            Dim state As XINPUT_STATE
            Dim result = XInputGetState(0, state)
            If result <> 0 Then Return
            Dim btns = state.Gamepad.wButtons
            Dim pressed = CUShort(btns And Not _prevGamepadButtons)
            ' D-pad and left stick for paddle movement
            _gamepadLeft = (btns And XINPUT_GAMEPAD_DPAD_LEFT) <> 0
            _gamepadRight = (btns And XINPUT_GAMEPAD_DPAD_RIGHT) <> 0
            If state.Gamepad.sThumbLX < -8000 Then _gamepadLeft = True
            If state.Gamepad.sThumbLX > 8000 Then _gamepadRight = True
            ' A = Space (start / resume / speed toggle)
            If (pressed And XINPUT_GAMEPAD_A) <> 0 Then SendGamepadKey(Keys.Space)
            ' B = Pause
            If (pressed And XINPUT_GAMEPAD_B) <> 0 Then SendGamepadKey(Keys.P)
            ' Start = Options
            If (pressed And XINPUT_GAMEPAD_START) <> 0 Then SendGamepadKey(Keys.O)
            ' Y = Speed boost
            If (pressed And XINPUT_GAMEPAD_Y) <> 0 Then SendGamepadKey(Keys.F)
            ' In Options: D-pad up/down/left/right for menu navigation
            If _state = GameState.Options Then
                If (pressed And XINPUT_GAMEPAD_DPAD_UP) <> 0 Then SendGamepadKey(Keys.Up)
                If (pressed And XINPUT_GAMEPAD_DPAD_DOWN) <> 0 Then SendGamepadKey(Keys.Down)
                If (pressed And XINPUT_GAMEPAD_DPAD_LEFT) <> 0 Then SendGamepadKey(Keys.Left)
                If (pressed And XINPUT_GAMEPAD_DPAD_RIGHT) <> 0 Then SendGamepadKey(Keys.Right)
                If (pressed And XINPUT_GAMEPAD_A) <> 0 Then SendGamepadKey(Keys.Enter)
            End If
            ' In High Score: shoulders for backspace / enter
            If _state = GameState.HighScore Then
                If (pressed And XINPUT_GAMEPAD_LEFT_SHOULDER) <> 0 Then SendGamepadKey(Keys.Back)
                If (pressed And XINPUT_GAMEPAD_RIGHT_SHOULDER) <> 0 Then SendGamepadKey(Keys.Enter)
            End If
            _prevGamepadButtons = btns
        Catch
            _gamepadAvailable = False
        End Try
    End Sub

    Private Sub SendGamepadKey(key As Keys)
        Dim e As New KeyEventArgs(key)
        Form1_KeyDown(Me, e)
    End Sub
#End Region

#Region "Sprite System"
    Private Function FindAssetsDir() As String
        Dim base = AppDomain.CurrentDomain.BaseDirectory
        Dim d = Path.Combine(base, "Assets")
        If Directory.Exists(d) Then Return d
        Dim up = base
        For i = 1 To 8
            up = Path.GetDirectoryName(up)
            If String.IsNullOrEmpty(up) Then Exit For
            Dim wpf = Path.Combine(up, "anime finder wpf", "Assets")
            If Directory.Exists(wpf) Then Return wpf
        Next
        Return Nothing
    End Function

    Private Sub LoadSprites()
        Try
            Dim dir = FindAssetsDir()
            If String.IsNullOrEmpty(dir) Then Return
            For Each file In Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories)
                Try
                    Dim rel = file.Substring(dir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    Dim folder = Path.GetDirectoryName(rel)
                    Dim stem = Path.GetFileNameWithoutExtension(rel)
                    Dim key = If(String.IsNullOrEmpty(folder), stem, folder & "/" & stem).ToLower().Replace("\"c, "/"c)
                    If Not _sprites.ContainsKey(key) Then _sprites(key) = New Bitmap(file)
                Catch
                End Try
            Next
        Catch
        End Try
    End Sub

    Private Function TryGetSprite(key As String) As Bitmap
        Dim bmp As Bitmap = Nothing
        _sprites.TryGetValue(key.ToLower(), bmp)
        Return bmp
    End Function

    Private Sub DisposeSprites()
        For Each kvp In _sprites.Values
            Try : kvp.Dispose() : Catch : End Try
        Next
        _sprites.Clear()
    End Sub

    Private Sub GenerateProceduralSprites()
        If Not _sprites.ContainsKey("ui/heart") Then
            _sprites("ui/heart") = GenerateHeartBitmap(22, Color.FromArgb(255, 80, 100))
        End If
        If Not _sprites.ContainsKey("ui/powerup_life") Then
            _sprites("ui/powerup_life") = GeneratePowerUpLifeBitmap(POWERUP_SIZE)
        End If
    End Sub

    Private Function GenerateHeartBitmap(size As Integer, fillColor As Color) As Bitmap
        Dim bmp As New Bitmap(size, size, Imaging.PixelFormat.Format32bppPArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Color.Transparent)
            Dim s = CSng(size)
            Dim pad = s * 0.08F
            Dim w = s - pad * 2
            Dim h = s - pad * 2
            Dim x = pad, y = pad
            Dim c1 = Color.FromArgb(fillColor.A,
                Math.Min(255, CInt(fillColor.R) + 70),
                Math.Min(255, CInt(fillColor.G) + 20),
                Math.Min(255, CInt(fillColor.B) + 20))
            Using br As New LinearGradientBrush(New RectangleF(x, y, w, h + 1), c1, fillColor, LinearGradientMode.Vertical)
                ' Two overlapping circles form the top bumps
                g.FillEllipse(br, x, y + h * 0.04F, w * 0.54F, h * 0.54F)
                g.FillEllipse(br, x + w * 0.46F, y + h * 0.04F, w * 0.54F, h * 0.54F)
                ' Triangle closes the bottom into a point
                Dim pts() As PointF = {
                    New PointF(x, y + h * 0.3F),
                    New PointF(x + w, y + h * 0.3F),
                    New PointF(x + w * 0.5F, y + h)
                }
                g.FillPolygon(br, pts)
            End Using
            ' Specular highlight on upper-left lobe
            Using br As New SolidBrush(Color.FromArgb(110, 255, 255, 255))
                g.FillEllipse(br, x + w * 0.1F, y + h * 0.08F, w * 0.22F, h * 0.18F)
            End Using
        End Using
        Return bmp
    End Function

    Private Function GeneratePowerUpLifeBitmap(size As Integer) As Bitmap
        Dim bmp As New Bitmap(size, size, Imaging.PixelFormat.Format32bppPArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Color.Transparent)
            Dim s = CSng(size)
            ' Dark circular badge background
            Using br As New SolidBrush(Color.FromArgb(160, 60, 0, 20))
                g.FillEllipse(br, 0, 0, s, s)
            End Using
            Using pen As New Pen(Color.FromArgb(200, 255, 80, 100), 2)
                g.DrawEllipse(pen, 1, 1, s - 3, s - 3)
            End Using
            ' Heart centered inside the badge
            Dim hs = CInt(s * 0.7F)
            Dim ho = CSng((s - hs) / 2)
            Dim w = CSng(hs), h = CSng(hs)
            Dim x = ho, y = ho - s * 0.03F
            Using br As New LinearGradientBrush(New RectangleF(x, y, w, h + 1),
                Color.FromArgb(255, 255, 150, 160), Color.FromArgb(255, 220, 20, 50), LinearGradientMode.Vertical)
                g.FillEllipse(br, x, y + h * 0.04F, w * 0.54F, h * 0.54F)
                g.FillEllipse(br, x + w * 0.46F, y + h * 0.04F, w * 0.54F, h * 0.54F)
                Dim pts() As PointF = {
                    New PointF(x, y + h * 0.3F),
                    New PointF(x + w, y + h * 0.3F),
                    New PointF(x + w * 0.5F, y + h)
                }
                g.FillPolygon(br, pts)
            End Using
            Using br As New SolidBrush(Color.FromArgb(120, 255, 255, 255))
                g.FillEllipse(br, x + w * 0.1F, y + h * 0.08F, w * 0.22F, h * 0.18F)
            End Using
        End Using
        Return bmp
    End Function
#End Region

#Region "Update Logic"
    Private Sub UpdatePaddle()
        ' Keyboard + gamepad movement
        If _leftPressed OrElse _gamepadLeft Then _paddleX -= PADDLE_SPEED
        If _rightPressed OrElse _gamepadRight Then _paddleX += PADDLE_SPEED
        ' Touch/mouse drag — move paddle toward finger position
        If _touchActive AndAlso _touchX >= 0 Then
            Dim target = _touchX - _paddleWidth / 2.0F
            Dim diff = target - _paddleX
            If Math.Abs(diff) > PADDLE_SPEED Then
                _paddleX += CSng(Math.Sign(diff)) * PADDLE_SPEED
            Else
                _paddleX = target
            End If
        End If
        If _paddleX < 0 Then _paddleX = 0
        If _paddleX > LOGICAL_WIDTH - _paddleWidth Then _paddleX = LOGICAL_WIDTH - _paddleWidth
    End Sub

    Private Sub UpdateBalls()
        If _getReadyFrames > 0 Then Return
        Dim sm As Single = If(_speedBoost, 2.0F, 1.0F)
        For i = 0 To _balls.Count - 1
            Dim b = _balls(i)
            If Not b.Active Then Continue For
            b.X += b.DX * sm
            b.Y += b.DY * sm
            If b.X - _ballRadius <= 0 Then
                b.X = _ballRadius
                b.DX = Math.Abs(b.DX)
                PlayWallHit()
            End If
            If b.X + _ballRadius >= LOGICAL_WIDTH Then
                b.X = LOGICAL_WIDTH - _ballRadius
                b.DX = -Math.Abs(b.DX)
                PlayWallHit()
            End If
            If b.Y - _ballRadius <= 0 Then
                b.Y = _ballRadius
                b.DY = Math.Abs(b.DY)
                PlayWallHit()
            End If
            If b.Y + _ballRadius >= LOGICAL_HEIGHT Then
                b.Active = False
                SpawnParticles(b.X, b.Y, Color.White, 12)
                _combo = 0 : _comboTimer = 0
                _balls(i) = b
                Continue For
            End If
            Dim paddleRect As New RectangleF(_paddleX, LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT, _paddleWidth, PADDLE_HEIGHT)
            If b.DY > 0 AndAlso BallIntersectsRect(b, paddleRect) Then
                b.Y = paddleRect.Top - _ballRadius - 1
                Dim hitPos = Math.Max(0.05F, Math.Min(0.95F, (b.X - _paddleX) / _paddleWidth))
                Dim ang = 150 - hitPos * 120
                Dim rad = ang * Math.PI / 180.0
                b.DX = CSng(-Math.Cos(rad) * b.Speed)
                b.DY = CSng(-Math.Sin(rad) * b.Speed)
                If Math.Abs(b.DY) < 2.0F Then
                    b.DY = -2.0F
                    Dim ratio = b.Speed / CSng(Math.Sqrt(b.DX * b.DX + b.DY * b.DY))
                    b.DX *= ratio
                    b.DY *= ratio
                End If
                PlayPaddleHit()
                SpawnParticles(b.X, b.Y, Color.FromArgb(100, 200, 255), 4)
            End If
            For j = 0 To _bricks.Count - 1
                If Not _bricks(j).Alive Then Continue For
                Dim bk = _bricks(j)
                If BallIntersectsRect(b, bk.Rect) Then
                    bk.HitsLeft -= 1
                    If bk.HitsLeft <= 0 Then
                        bk.Alive = False
                        _combo += 1 : _comboTimer = 90
                        _score += bk.Points * Math.Min(_combo, 8)
                        SpawnParticles(bk.Rect.X + bk.Rect.Width / 2, bk.Rect.Y + bk.Rect.Height / 2, bk.Color1, PARTICLE_COUNT)
                        If _combo >= 2 Then PlayComboSound() Else PlayBrickHit()
                        _screenShake = 3
                        If _rng.Next(100) < Math.Max(20, 54 - _level * 3) Then
                            SpawnPowerUp(bk.Rect.X + bk.Rect.Width / 2, bk.Rect.Y + bk.Rect.Height / 2)
                        End If
                    Else
                        bk.Color1 = Color.FromArgb(200, 200, 200)
                        bk.Color2 = Color.FromArgb(240, 240, 240)
                        PlaySFX(400, 30)
                    End If
                    _bricks(j) = bk
                    Dim ol = (b.X + _ballRadius) - bk.Rect.Left
                    Dim or2 = bk.Rect.Right - (b.X - _ballRadius)
                    Dim ot = (b.Y + _ballRadius) - bk.Rect.Top
                    Dim ob2 = bk.Rect.Bottom - (b.Y - _ballRadius)
                    If Math.Min(ol, or2) < Math.Min(ot, ob2) Then b.DX = -b.DX Else b.DY = -b.DY
                    Exit For
                End If
            Next
            _balls(i) = b
        Next
        Dim totalActive = _balls.Where(Function(bl) bl.Active).Count()
        If totalActive = 0 Then
            _lives -= 1
            PlayBallLost()
            _screenShake = 10
            If _lives <= 0 Then
                If _score > _highScore Then _highScore = _score
                _nameInput = ""
                _highScoreSaved = False
                _pendingHighScore = True
                _highScoreDelayFrames = 60
                _state = GameState.Paused
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
            If Not pu.Active Then
                _powerUps.RemoveAt(i)
                Continue For
            End If
            pu.Y += POWERUP_SPEED
            If pu.Y > LOGICAL_HEIGHT Then
                _powerUps.RemoveAt(i)
                Continue For
            End If
            Dim pr As New RectangleF(_paddleX, LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT, _paddleWidth, PADDLE_HEIGHT)
            Dim ur As New RectangleF(pu.X - POWERUP_SIZE / 2, pu.Y - POWERUP_SIZE / 2, POWERUP_SIZE, POWERUP_SIZE)
            If pr.IntersectsWith(ur) Then
                ApplyPowerUp(pu.PType)
                SpawnParticles(pu.X, pu.Y, pu.Color1, 10)
                PlayPowerUpSound()
                _powerUps.RemoveAt(i)
                Continue For
            End If
            _powerUps(i) = pu
        Next
    End Sub

    Private Sub UpdateParticles()
        For i = _particles.Count - 1 To 0 Step -1
            Dim p = _particles(i)
            If Not p.Active Then
                _particles.RemoveAt(i)
                Continue For
            End If
            p.X += p.DX : p.Y += p.DY : p.DY += 0.1F : p.Life -= 1
            If p.Life <= 0 Then
                _particles.RemoveAt(i)
                Continue For
            End If
            _particles(i) = p
        Next
    End Sub

    Private Sub UpdateTimers()
        If _comboTimer > 0 Then
            _comboTimer -= 1
            If _comboTimer <= 0 Then _combo = 0
        End If
        If _paddleWidthTimer > 0 Then
            _paddleWidthTimer -= 1
            If _paddleWidthTimer <= 0 Then _paddleWidth = PADDLE_WIDTH
        End If
        If _getReadyFrames > 0 Then _getReadyFrames -= 1
    End Sub

    Private Sub UpdateStarField()
        For i = 0 To _starFieldY.Length - 1
            _starFieldY(i) += _starFieldSpeed(i)
            If _starFieldY(i) > LOGICAL_HEIGHT Then
                _starFieldY(i) = 0
                _starFieldX(i) = _rng.Next(0, LOGICAL_WIDTH)
            End If
        Next
    End Sub

    Private Sub CheckLevelComplete()
        If _bricks.All(Function(bk) Not bk.Alive) Then
            _state = GameState.LevelComplete
            PlayLevelWin()
        End If
    End Sub
#End Region

#Region "Helpers"
    Private Function BallIntersectsRect(b As Ball, r As RectangleF) As Boolean
        Dim cx = Math.Max(r.Left, Math.Min(b.X, r.Right))
        Dim cy = Math.Max(r.Top, Math.Min(b.Y, r.Bottom))
        Dim dx = b.X - cx : Dim dy = b.Y - cy
        Return (dx * dx + dy * dy) <= (_ballRadius * _ballRadius)
    End Function

    Private Sub ResetBall()
        _balls.Clear()
        Dim b As Ball
        b.X = _paddleX + _paddleWidth / 2.0F
        b.Y = LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT - _ballRadius - 2
        b.Speed = CSng(INITIAL_BALL_SPEED * Math.Pow(1.05, _level - 1))
        Dim angle = _rng.Next(220, 320) * Math.PI / 180.0
        b.DX = CSng(Math.Cos(angle) * b.Speed)
        b.DY = CSng(Math.Sin(angle) * b.Speed)
        If b.DY > 0 Then b.DY = -b.DY
        b.Active = True
        _balls.Add(b)
        _getReadyFrames = 180
    End Sub

    Private Sub SpawnPowerUp(x As Single, y As Single)
        Dim pu As PowerUp
        pu.X = x : pu.Y = y : pu.Active = True
        Dim t = _rng.Next(9)
        Select Case t
            Case 0
                pu.PType = PowerUpType.BlueBallGrow
                pu.Color1 = If(_colorblindMode, Color.FromArgb(0, 114, 178), Color.FromArgb(50, 120, 255))
                pu.Symbol = If(_colorblindMode, "BIG", "+")
            Case 1
                pu.PType = PowerUpType.RedBallShrink
                pu.Color1 = If(_colorblindMode, Color.FromArgb(213, 94, 0), Color.FromArgb(255, 60, 60))
                pu.Symbol = If(_colorblindMode, "1UP", ChrW(&H2665))
            Case 2, 3, 4
                pu.PType = PowerUpType.GreenMultiBall
                pu.Color1 = If(_colorblindMode, Color.FromArgb(240, 228, 66), Color.FromArgb(50, 220, 100))
                pu.Symbol = If(_colorblindMode, "x3", "x3")
            Case 5
                pu.PType = PowerUpType.YellowBallShrink
                pu.Color1 = If(_colorblindMode, Color.FromArgb(86, 180, 233), Color.FromArgb(255, 220, 50))
                pu.Symbol = If(_colorblindMode, "SML", "-")
            Case 6
                pu.PType = PowerUpType.PurplePaddleMega
                pu.Color1 = If(_colorblindMode, Color.FromArgb(148, 0, 211), Color.FromArgb(170, 80, 255))
                pu.Symbol = If(_colorblindMode, "BIG", "x3")
            Case 7
                pu.PType = PowerUpType.OrangeBallSlow
                pu.Color1 = If(_colorblindMode, Color.FromArgb(230, 159, 0), Color.FromArgb(255, 150, 60))
                pu.Symbol = If(_colorblindMode, "SLOW", "-")
            Case Else
                pu.PType = PowerUpType.PinkBallFast
                pu.Color1 = If(_colorblindMode, Color.FromArgb(204, 121, 167), Color.FromArgb(255, 120, 200))
                pu.Symbol = If(_colorblindMode, "FAST", "+")
        End Select
        _powerUps.Add(pu)
    End Sub

    Private Sub ApplyPowerUp(pType As PowerUpType)
        Select Case pType
            Case PowerUpType.BlueBallGrow
                _ballRadius = Math.Min(MAX_BALL_RADIUS, _ballRadius + 6)
            Case PowerUpType.RedBallShrink
                _lives = Math.Min(MAX_LIVES, _lives + 1)
            Case PowerUpType.GreenMultiBall
                Dim current = _balls.Where(Function(b) b.Active).ToList()
                If current.Count > 0 Then
                    Dim src = current(0)
                    For k = 0 To 7
                        Dim nb As Ball
                        nb.X = src.X : nb.Y = src.Y : nb.Speed = src.Speed
                        Dim angle = _rng.Next(200, 340) * Math.PI / 180.0
                        nb.DX = CSng(Math.Cos(angle) * nb.Speed)
                        nb.DY = CSng(Math.Sin(angle) * nb.Speed)
                        If nb.DY > 0 Then nb.DY = -nb.DY
                        nb.Active = True
                        _balls.Add(nb)
                    Next
                End If
            Case PowerUpType.YellowBallShrink
                _ballRadius = Math.Max(MIN_BALL_RADIUS, _ballRadius - 6)
            Case PowerUpType.PurplePaddleMega
                _paddleWidth = CInt(PADDLE_WIDTH * 3)
                _paddleWidthTimer = 480
            Case PowerUpType.OrangeBallSlow
                AdjustBallSpeed(0.85F)
            Case PowerUpType.PinkBallFast
                AdjustBallSpeed(1.15F)
        End Select
    End Sub

    Private Sub AdjustBallSpeed(multiplier As Single)
        For i = 0 To _balls.Count - 1
            Dim b = _balls(i)
            If Not b.Active Then Continue For
            Dim newSpeed = Math.Max(4.0F, Math.Min(25.0F, b.Speed * multiplier))
            If b.Speed > 0 Then
                Dim scale = newSpeed / b.Speed
                b.DX *= scale
                b.DY *= scale
            End If
            b.Speed = newSpeed
            _balls(i) = b
        Next
    End Sub

    Private Sub SpawnParticles(x As Single, y As Single, c As Color, count As Integer)
        For i = 0 To count - 1
            Dim p As Particle
            p.X = x : p.Y = y
            Dim angle = _rng.NextDouble() * Math.PI * 2
            Dim speed = 1.5 + _rng.NextDouble() * 3.0
            p.DX = CSng(Math.Cos(angle) * speed)
            p.DY = CSng(Math.Sin(angle) * speed)
            p.MaxLife = 20 + _rng.Next(20)
            p.Life = p.MaxLife
            p.ParticleColor = c
            p.Size = 2 + CSng(_rng.NextDouble()) * 4
            p.Active = True
            _particles.Add(p)
        Next
    End Sub

    Private Sub AddHighScore(name As String, score As Integer)
        Dim rec As New ScoreRecord()
        rec.PlayerName = name
        rec.PlayerScore = score
        _highScores.Add(rec)
        _highScores = _highScores.OrderByDescending(Function(s) s.PlayerScore).Take(10).ToList()
        If _highScores.Count > 0 Then _highScore = _highScores(0).PlayerScore
        SaveHighScores()
    End Sub

    Private Function KeyToChar(e As KeyEventArgs) As String
        If e.KeyCode >= Keys.A AndAlso e.KeyCode <= Keys.Z Then Return ChrW(e.KeyValue).ToString()
        If e.KeyCode >= Keys.D0 AndAlso e.KeyCode <= Keys.D9 Then Return (e.KeyValue - Keys.D0).ToString()
        If e.KeyCode >= Keys.NumPad0 AndAlso e.KeyCode <= Keys.NumPad9 Then Return (e.KeyValue - Keys.NumPad0).ToString()
        If e.KeyCode = Keys.OemMinus OrElse e.KeyCode = Keys.Subtract Then Return "-"
        If e.KeyCode = Keys.OemPeriod OrElse e.KeyCode = Keys.Decimal Then Return "."
        Return Nothing
    End Function

    Private Sub LoadHighScores()
        Try
            If Not File.Exists(_highScorePath) Then Return
            Dim json = File.ReadAllText(_highScorePath)
            Dim list = JsonSerializer.Deserialize(Of List(Of ScoreRecord))(json)
            If list IsNot Nothing Then
                _highScores = list
                If _highScores.Count > 0 Then _highScore = _highScores(0).PlayerScore
            End If
        Catch
        End Try
    End Sub

    Private Sub SaveHighScores()
        Try
            Dim dir = Path.GetDirectoryName(_highScorePath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If
            File.WriteAllText(_highScorePath, JsonSerializer.Serialize(_highScores))
        Catch
        End Try
    End Sub
#End Region

#Region "Drawing"
    Private Sub DrawStarField(g As Graphics)
        ' Solid dark background — black, not the yellow sprite textures
        Using br As New SolidBrush(Color.FromArgb(255, 8, 8, 20))
            g.FillRectangle(br, 0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT)
        End Using
        For i = 0 To _starFieldX.Length - 1
            Dim bright = _starFieldBright(i)
            Dim twinkle = CInt(Math.Sin(_frameCount * 0.05 + i) * 40)
            bright = Math.Max(30, Math.Min(255, bright + twinkle))
            Using br As New SolidBrush(Color.FromArgb(bright, bright, Math.Min(255, bright + 30)))
                g.FillEllipse(br, _starFieldX(i), _starFieldY(i), 2, 2)
            End Using
        Next
    End Sub

    Private Sub DrawMenu(g As Graphics)
        Dim titleY = 140
        Using path As New GraphicsPath()
            Using ff As New FontFamily("Segoe UI")
                path.AddString("TEAM FAST TALK", ff, CInt(FontStyle.Bold), 48, New Point(0, 0), StringFormat.GenericDefault)
            End Using
            Dim bounds = path.GetBounds()
            Dim mat As New Matrix()
            mat.Translate(CSng((LOGICAL_WIDTH - bounds.Width) / 2 - bounds.X), CSng(titleY - bounds.Y))
            path.Transform(mat)
            For gl = 10 To 2 Step -2
                Using pen As New Pen(Color.FromArgb(30, 100, 180, 255), gl)
                    g.DrawPath(pen, path)
                End Using
            Next
            Using br As New LinearGradientBrush(New Point(0, titleY), New Point(0, titleY + 55), Color.FromArgb(100, 200, 255), Color.FromArgb(255, 120, 200))
                g.FillPath(br, path)
            End Using
        End Using
        Using path2 As New GraphicsPath()
            Using ff As New FontFamily("Segoe UI")
                path2.AddString("BRICK BLAST", ff, CInt(FontStyle.Bold), 58, New Point(0, 0), StringFormat.GenericDefault)
            End Using
            Dim b2 = path2.GetBounds()
            Dim mat2 As New Matrix()
            mat2.Translate(CSng((LOGICAL_WIDTH - b2.Width) / 2 - b2.X), CSng(titleY + 55 - b2.Y))
            path2.Transform(mat2)
            For gl = 10 To 2 Step -2
                Using pen As New Pen(Color.FromArgb(25, 255, 150, 50), gl)
                    g.DrawPath(pen, path2)
                End Using
            Next
            Using br As New LinearGradientBrush(New Point(0, titleY + 55), New Point(0, titleY + 120), Color.FromArgb(255, 200, 80), Color.FromArgb(255, 100, 50))
                g.FillPath(br, path2)
            End Using
        End Using
        Dim startSpr = TryGetSprite("ui/text_start")
        Dim keySpc = TryGetSprite("ui/key_space")
        If startSpr IsNot Nothing Then
            g.DrawImage(startSpr, CInt((LOGICAL_WIDTH - 240) / 2), 304, 240, 40)
        Else
            DrawCenteredText(g, "Press SPACE to Start", _fnt18r, Color.White, 310)
        End If
        If keySpc IsNot Nothing Then g.DrawImage(keySpc, CInt(LOGICAL_WIDTH / 2 + 130), 308, 32, 32)
        ' Mini leaderboard on front page
        If _highScores.Count > 0 Then
            Dim topN = Math.Min(3, _highScores.Count)
            Dim panelH = 16 + topN * 20 + 8
            Using pbr As New SolidBrush(Color.FromArgb(160, 0, 0, 30))
                Using rr = RoundedRect(New RectangleF(LOGICAL_WIDTH / 2 - 200, 348, 400, panelH), 8)
                    g.FillPath(pbr, rr)
                End Using
            End Using
            DrawCenteredText(g, "BEST SCORES", _fnt10b, Color.FromArgb(255, 240, 100), 351)
            Dim sy = 368.0F
            For i = 0 To topN - 1
                Dim rec = _highScores(i)
                Dim nm = If(rec.PlayerName.Length > 10, rec.PlayerName.Substring(0, 10), rec.PlayerName.PadRight(10))
                DrawCenteredText(g, $"{i + 1}. {nm}  {rec.PlayerScore:N0}", _fnt10r, Color.FromArgb(200, 200, 225), sy)
                sy += 20
            Next
        ElseIf _highScore > 0 Then
            DrawCenteredText(g, $"High Score: {_highScore}", _fnt14r, Color.FromArgb(255, 255, 120), 355)
        End If
        DrawCenteredText(g, ChrW(&H2699) & "  Press H or O for OPTIONS  " & ChrW(&H2699), _fnt14b, Color.FromArgb(100, 200, 255), 420)
        Dim keyArr = TryGetSprite("ui/key_arrows")
        Dim keyF = TryGetSprite("ui/key_f")
        Dim keyP = TryGetSprite("ui/key_p")
        If keyArr IsNot Nothing Then g.DrawImage(keyArr, CInt(LOGICAL_WIDTH / 2 - 290), 454, 24, 24)
        If keyF IsNot Nothing Then g.DrawImage(keyF, CInt(LOGICAL_WIDTH / 2 - 30), 454, 24, 24)
        If keyP IsNot Nothing Then g.DrawImage(keyP, CInt(LOGICAL_WIDTH / 2 + 130), 454, 24, 24)
        DrawCenteredText(g, "ARROW KEYS to move  |  F speed boost  |  P pause", _fnt11r, Color.FromArgb(150, 150, 170), 458)
        DrawCenteredText(g, $"Music: {_musicStyleNames(_musicStyle)}  |  SFX: {_sfxStyleNames(_sfxStyle)}", _fnt11r, Color.FromArgb(120, 140, 180), 488)
        DrawCenteredText(g, $"Window: {_windowScaleNames(_windowScale)}", _fnt11r, Color.FromArgb(120, 140, 180), 518)
        DrawCenteredText(g, "Destroy bricks  " & ChrW(8226) & "  Catch power-ups  " & ChrW(8226) & "  Build combos!", _fnt11r, Color.FromArgb(150, 150, 170), 548)
    End Sub

    Private Sub DrawGame(g As Graphics)
        DrawHUD(g)
        DrawBricks(g)
        DrawPaddle(g)
        DrawBalls(g)
        DrawPowerUps(g)
        DrawParticles(g)
        DrawCombo(g)
        DrawGetReady(g)
    End Sub

    Private Sub DrawHUD(g As Graphics)
        Dim f = _fnt13b
            ' Score with optional star icon
            Dim starSpr = TryGetSprite("ui/star")
            If starSpr IsNot Nothing Then
                g.DrawImage(starSpr, 15, 11, 20, 20)
            End If
            DrawTextShadow(g, $"SCORE: {_score}", f, Color.White, CSng(If(starSpr IsNot Nothing, 38, 15)), 12)
            DrawCenteredText(g, $"LEVEL {_level}", f, Color.FromArgb(180, 200, 255), 12)
            ' Lives — drawn directly so transparency issues with DrawImage never occur
            Dim hSz As Single = 22, hPad As Single = 2
            Dim hX = CSng(LOGICAL_WIDTH - 15 - (hSz + hPad) * _lives)
            For h = 0 To _lives - 1
                DrawHeartShape(g, hX + h * (hSz + hPad), 10.0F, hSz, hSz, Color.FromArgb(255, 80, 100))
            Next
            If _speedBoost Then
                Dim bt = ChrW(&H26A1) & " 2x SPEED"
                Dim bsz = g.MeasureString(bt, f)
                DrawTextShadow(g, bt, f, Color.FromArgb(255, 255, 80), (LOGICAL_WIDTH - bsz.Width) / 2, 32)
            End If
            If _ballRadius <> BALL_RADIUS Then
                Dim rt = $"Ball: {_ballRadius}px"
                DrawTextShadow(g, rt, f, Color.FromArgb(150, 200, 255), 15, 32)
            End If
            If _paddleWidthTimer > 0 Then
                Dim sec = CInt(Math.Ceiling(_paddleWidthTimer / 60.0))
                Dim pt = $"Paddle: {sec}s"
                Dim psz = g.MeasureString(pt, f)
                DrawTextShadow(g, pt, f, Color.FromArgb(170, 80, 255), LOGICAL_WIDTH - psz.Width - 15, 32)
            End If
        Using pen As New Pen(Color.FromArgb(40, 100, 180, 255), 1)
            g.DrawLine(pen, 0, 50, LOGICAL_WIDTH, 50)
        End Using
    End Sub

    Private Sub DrawBricks(g As Graphics)
        For Each bk In _bricks
            If Not bk.Alive Then Continue For
            Dim r = bk.Rect
            Dim isDamaged = bk.Color1.R >= 195 AndAlso bk.Color1.G >= 195 AndAlso bk.Color1.B >= 195
            Dim sprKey = If(isDamaged, $"sprites/brick_{bk.Row Mod 7}_damaged", If(bk.HitsLeft >= 3, "sprites/brick_gold", $"sprites/brick_{bk.Row Mod 7}"))
            Dim brickSpr = TryGetSprite(sprKey)
            If brickSpr Is Nothing AndAlso bk.HitsLeft >= 3 Then brickSpr = TryGetSprite($"sprites/brick_{bk.Row Mod 7}")
            If brickSpr IsNot Nothing Then
                g.DrawImage(brickSpr, r)
            Else
                Using br As New LinearGradientBrush(r, bk.Color1, bk.Color2, LinearGradientMode.Vertical)
                    Using rr = RoundedRect(r, 4)
                        g.FillPath(br, rr)
                    End Using
                End Using
                Dim sr2 As New RectangleF(r.X + 2, r.Y + 1, r.Width - 4, r.Height / 2.5F)
                Using br As New SolidBrush(Color.FromArgb(50, 255, 255, 255))
                    g.FillRectangle(br, sr2)
                End Using
            End If
            If _colorblindMode Then
                Using rr = RoundedRect(r, 4)
                    Using pen As New Pen(Color.White, 2)
                        g.DrawPath(pen, rr)
                    End Using
                End Using
                Dim sym = _colorblindSymbols(bk.Row Mod _colorblindSymbols.Length)
                If bk.HitsLeft > 1 Then sym = bk.HitsLeft.ToString() & sym
                Dim ts = g.MeasureString(sym, _fnt10b)
                Using br As New SolidBrush(Color.White)
                    g.DrawString(sym, _fnt10b, br, r.X + (r.Width - ts.Width) / 2, r.Y + (r.Height - ts.Height) / 2)
                End Using
            ElseIf bk.HitsLeft > 1 Then
                Using br As New SolidBrush(Color.FromArgb(180, 0, 0, 0))
                    Dim ts = g.MeasureString(bk.HitsLeft.ToString(), _fnt8b)
                    g.DrawString(bk.HitsLeft.ToString(), _fnt8b, br, r.X + (r.Width - ts.Width) / 2, r.Y + (r.Height - ts.Height) / 2)
                End Using
            End If
        Next
    End Sub

    Private Sub DrawPaddle(g As Graphics)
        Dim py = LOGICAL_HEIGHT - PADDLE_Y_OFFSET - PADDLE_HEIGHT
        Dim pr As New RectangleF(_paddleX, py, _paddleWidth, PADDLE_HEIGHT)
        Dim paddleC1 = If(_colorblindMode, Color.FromArgb(240, 228, 66), Color.FromArgb(80, 180, 255))
        Dim paddleC2 = If(_colorblindMode, Color.FromArgb(200, 190, 40), Color.FromArgb(40, 100, 200))
        Using br As New SolidBrush(Color.FromArgb(30, paddleC1))
            g.FillEllipse(br, _paddleX - 10, py + 5, _paddleWidth + 20, 20)
        End Using
        Dim paddleSpr As Bitmap = Nothing
        If _paddleWidth > PADDLE_WIDTH Then
            paddleSpr = TryGetSprite("sprites/paddle_wide")
        Else
            paddleSpr = TryGetSprite("sprites/paddle_hd_blue")
            If paddleSpr Is Nothing Then paddleSpr = TryGetSprite("sprites/paddle")
        End If
        If paddleSpr IsNot Nothing Then
            g.DrawImage(paddleSpr, pr)
            If _colorblindMode Then
                Using rr = RoundedRect(pr, 7)
                    Using pen As New Pen(Color.White, 2)
                        g.DrawPath(pen, rr)
                    End Using
                End Using
            End If
        Else
            Using rr = RoundedRect(pr, 7)
                Using br As New LinearGradientBrush(pr, paddleC1, paddleC2, LinearGradientMode.Vertical)
                    g.FillPath(br, rr)
                End Using
                Dim hl As New RectangleF(_paddleX + 4, py + 1, _paddleWidth - 8, PADDLE_HEIGHT / 2.5F)
                Using br As New SolidBrush(Color.FromArgb(80, 255, 255, 255))
                    g.FillRectangle(br, hl)
                End Using
                If _colorblindMode Then
                    Using pen As New Pen(Color.White, 2)
                        g.DrawPath(pen, rr)
                    End Using
                End If
            End Using
        End If
    End Sub

    Private Sub DrawBalls(g As Graphics)
        Dim br2 = _ballRadius
        For Each b In _balls
            If Not b.Active Then Continue For
            For gs = 20 To 4 Step -4
                Dim al = CInt(20 * (4.0 / gs))
                Dim glowC = If(_speedBoost, Color.FromArgb(al, 255, 200, 50), Color.FromArgb(al, 200, 230, 255))
                Using br As New SolidBrush(glowC)
                    g.FillEllipse(br, CSng(b.X - gs / 2), CSng(b.Y - gs / 2), CSng(gs), CSng(gs))
                End Using
            Next
            Dim ballTop = If(_speedBoost, Color.FromArgb(255, 255, 200), Color.White)
            Dim ballBot = If(_speedBoost, Color.FromArgb(255, 140, 20), Color.FromArgb(160, 210, 255))
            Using br As New LinearGradientBrush(New RectangleF(b.X - br2, b.Y - br2, br2 * 2, br2 * 2), ballTop, ballBot, LinearGradientMode.ForwardDiagonal)
                g.FillEllipse(br, b.X - br2, b.Y - br2, br2 * 2, br2 * 2)
            End Using
            Using br As New SolidBrush(Color.FromArgb(180, 255, 255, 255))
                g.FillEllipse(br, b.X - br2 * 0.4F, b.Y - br2 * 0.5F, br2 * 0.6F, br2 * 0.5F)
            End Using
        Next
    End Sub

    Private Sub DrawPowerUps(g As Graphics)
        Dim puSprKeys() As String = {"ui/powerup_grow", "ui/powerup_life", "ui/powerup_multi",
                                      "ui/powerup_shrink", "ui/powerup_mega", "ui/powerup_slow", "ui/powerup_fast"}
        For Each pu In _powerUps
            If Not pu.Active Then Continue For
            Dim bob = CSng(Math.Sin(_frameCount * 0.1) * 3)
            Dim cy = pu.Y + bob
            Using br As New SolidBrush(Color.FromArgb(40, pu.Color1))
                g.FillEllipse(br, pu.X - 14, cy - 14, 28, 28)
            End Using
            Dim puIdx = CInt(pu.PType)
            Dim puSpr = If(puIdx >= 0 AndAlso puIdx < puSprKeys.Length, TryGetSprite(puSprKeys(puIdx)), Nothing)
            If puSpr IsNot Nothing Then
                Dim sz2 = CSng(POWERUP_SIZE)
                g.DrawImage(puSpr, pu.X - sz2 / 2, cy - sz2 / 2, sz2, sz2)
                Dim ts = g.MeasureString(pu.Symbol, _fnt11b)
                DrawTextShadow(g, pu.Symbol, _fnt11b, Color.White, pu.X - ts.Width / 2, cy - ts.Height / 2)
            Else
                Using br As New SolidBrush(Color.FromArgb(200, pu.Color1.R, pu.Color1.G, pu.Color1.B))
                    g.FillEllipse(br, CSng(pu.X - POWERUP_SIZE / 2), CSng(cy - POWERUP_SIZE / 2), CSng(POWERUP_SIZE), CSng(POWERUP_SIZE))
                End Using
                Dim ts = g.MeasureString(pu.Symbol, _fnt18b)
                DrawTextShadow(g, pu.Symbol, _fnt18b, Color.White, pu.X - ts.Width / 2, cy - ts.Height / 2)
            End If
        Next
    End Sub

    Private Sub DrawParticles(g As Graphics)
        For Each p In _particles
            If Not p.Active Then Continue For
            Dim al = CInt(255 * (p.Life / p.MaxLife))
            al = Math.Max(0, Math.Min(255, al))
            Dim sz = p.Size * (p.Life / p.MaxLife)
            If sz < 0.5F Then Continue For
            Using br As New SolidBrush(Color.FromArgb(al, p.ParticleColor))
                g.FillEllipse(br, p.X - sz / 2, p.Y - sz / 2, sz, sz)
            End Using
        Next
    End Sub

    Private Sub DrawCombo(g As Graphics)
        If _combo >= 2 AndAlso _comboTimer > 0 Then
            Dim ca = Math.Max(0, Math.Min(255, CInt(_comboTimer * 5)))
            Dim ga = Math.Max(0, Math.Min(255, CInt(ca * 0.3)))
            Dim text = $"COMBO x{_combo}!"
            Dim f = _fnt20b
            If f Is Nothing Then Return
            Dim sz = g.MeasureString(text, f)
            Dim cx = (LOGICAL_WIDTH - sz.Width) / 2
            Dim cy = LOGICAL_HEIGHT / 2.0F + 30
            ' Dark contrasting backdrop ensures combo is always readable
            Using br As New SolidBrush(Color.FromArgb(Math.Min(210, ca), 0, 0, 20))
                Using rr = RoundedRect(New RectangleF(cx - 18, cy - 8, sz.Width + 36, sz.Height + 16), 10)
                    g.FillPath(br, rr)
                End Using
            End Using
            Dim gemSpr = TryGetSprite("ui/gem")
            If gemSpr IsNot Nothing Then g.DrawImage(gemSpr, cx - 38, cy + 2, 28, 28)
            Using br As New SolidBrush(Color.FromArgb(Math.Min(255, ga * 2), 255, 160, 0))
                g.DrawString(text, f, br, cx - 2, cy - 2)
                g.DrawString(text, f, br, cx + 2, cy + 2)
            End Using
            Using br As New SolidBrush(Color.FromArgb(ca, 255, 255, 180))
                g.DrawString(text, f, br, cx, cy)
            End Using
        End If
    End Sub

    Private Sub DrawGetReady(g As Graphics)
        If _getReadyFrames <= 0 Then Return
        Dim countText As String
        If _getReadyFrames > 120 Then
            countText = "3"
        ElseIf _getReadyFrames > 60 Then
            countText = "2"
        Else
            countText = "1"
        End If
        Dim pulse = CSng(Math.Abs(Math.Sin(_frameCount * 0.15)) * 10 + 58)
        Using f As New Font("Segoe UI", pulse, FontStyle.Bold)
            Dim sz = g.MeasureString(countText, f)
            Dim cx = (LOGICAL_WIDTH - sz.Width) / 2
            Dim cy = LOGICAL_HEIGHT / 2.0F - sz.Height / 2 - 20
            Using br As New SolidBrush(Color.FromArgb(120, 255, 160, 0))
                g.DrawString(countText, f, br, cx - 3, cy - 3)
                g.DrawString(countText, f, br, cx + 3, cy + 3)
            End Using
            Using br As New SolidBrush(Color.FromArgb(255, 255, 255, 140))
                g.DrawString(countText, f, br, cx, cy)
            End Using
        End Using
        Dim grSpr = TryGetSprite("ui/text_getready")
        If grSpr IsNot Nothing Then
            g.DrawImage(grSpr, CSng((LOGICAL_WIDTH - 240) / 2), LOGICAL_HEIGHT / 2.0F + 50, 240, 40)
        Else
            Using fSub As New Font("Segoe UI", 14, FontStyle.Regular)
                DrawCenteredText(g, "GET READY!", fSub, Color.FromArgb(180, 200, 200, 220), LOGICAL_HEIGHT / 2.0F + 50)
            End Using
        End If
    End Sub

    Private Sub DrawOptions(g As Graphics)
        Using br As New SolidBrush(Color.FromArgb(215, 0, 0, 20))
            g.FillRectangle(br, 0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT)
        End Using
        Dim pw = 780, ph = 600
        Dim px = CSng((LOGICAL_WIDTH - pw) / 2), py = CSng((LOGICAL_HEIGHT - ph) / 2)
        Using br As New SolidBrush(Color.FromArgb(245, 12, 12, 35))
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.FillPath(br, rr)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(100, 80, 160, 255), 2)
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.DrawPath(pen, rr)
            End Using
        End Using
        Dim y = py + 12
        Dim gearSpr = TryGetSprite("ui/gear")
        If gearSpr IsNot Nothing Then
            Dim gs2 = 28
            g.DrawImage(gearSpr, CSng(LOGICAL_WIDTH / 2) - 118, y + 4, gs2, gs2)
        End If
        Dim optSpr = TryGetSprite("ui/text_options")
        If optSpr IsNot Nothing Then
            g.DrawImage(optSpr, CInt((LOGICAL_WIDTH - 180) / 2) + 12, CInt(y + 4), 180, 30)
        Else
            DrawCenteredText(g, "OPTIONS", _fnt22b, Color.FromArgb(100, 200, 255), y)
        End If
        y += 48
        Dim lx = px + 25
        Dim rx = CSng(px + pw / 2 + 10)
        Using fh As New Font("Segoe UI", 12, FontStyle.Bold)
            Using fb As New Font("Segoe UI", 10, FontStyle.Regular)
                Using brH As New SolidBrush(Color.FromArgb(255, 240, 100))
                    Using brB As New SolidBrush(Color.FromArgb(210, 210, 225))
                        g.DrawString("CONTROLS:", fh, brH, lx, y)
                        g.DrawString("POWER-UPS:", fh, brH, rx, y) : y += 24
                        g.DrawString(ChrW(&H2190) & " " & ChrW(&H2192) & " / A D   Move Paddle", fb, brB, lx + 8, y)
                        Using brPU As New SolidBrush(Color.FromArgb(80, 150, 255))
                            g.DrawString(ChrW(&H25CF) & " Blue      Ball grows larger", fb, brPU, rx + 8, y)
                        End Using
                        y += 19
                        g.DrawString("SPACE       Start / Resume", fb, brB, lx + 8, y)
                        Using brPU As New SolidBrush(Color.FromArgb(255, 90, 90))
                            g.DrawString(ChrW(&H25CF) & " Red       +1 life", fb, brPU, rx + 8, y)
                        End Using
                        y += 19
                        g.DrawString("P / ESC     Pause", fb, brB, lx + 8, y)
                        Using brPU As New SolidBrush(Color.FromArgb(80, 220, 120))
                            g.DrawString(ChrW(&H25CF) & " Green     Multi-ball", fb, brPU, rx + 8, y)
                        End Using
                        y += 19
                        g.DrawString("F           Speed Boost (2x)", fb, brB, lx + 8, y)
                        Using brPU As New SolidBrush(Color.FromArgb(255, 220, 60))
                            g.DrawString(ChrW(&H25CF) & " Yellow    Ball shrinks", fb, brPU, rx + 8, y)
                        End Using
                        y += 19
                        Using brPU As New SolidBrush(Color.FromArgb(170, 80, 255))
                            g.DrawString(ChrW(&H25CF) & " Purple    3x paddle (8s)", fb, brPU, rx + 8, y)
                        End Using
                        y += 19
                        Using brPU As New SolidBrush(Color.FromArgb(255, 150, 60))
                            g.DrawString(ChrW(&H25CF) & " Orange    Ball slows", fb, brPU, rx + 8, y)
                        End Using
                        y += 19
                        Using brPU As New SolidBrush(Color.FromArgb(255, 120, 200))
                            g.DrawString(ChrW(&H25CF) & " Pink      Ball speeds", fb, brPU, rx + 8, y)
                        End Using
                        y += 19
                        g.DrawString("H / O       Options", fb, brB, lx + 8, y)
                        y += 18
                        Using brNote As New SolidBrush(Color.FromArgb(160, 180, 200))
                            g.DrawString("Bonus ball colors: Blue=Grow  Red=+1 life  Green=Multi  Yellow=Shrink  Purple=3x paddle  Orange=Slow  Pink=Fast", fb, brNote, lx + 8, y)
                        End Using
                        y += 25
                        g.DrawString("RULES:", fh, brH, rx - (rx - lx), y) : y += 22
                        g.DrawString(ChrW(8226) & " Destroy all bricks to advance     " & ChrW(8226) & " Ball speeds up each level", fb, brB, lx + 8, y) : y += 19
                        g.DrawString(ChrW(8226) & " Don't lose the ball!               " & ChrW(8226) & " Build combos for bonus points", fb, brB, lx + 8, y) : y += 19
                        g.DrawString(ChrW(8226) & " Catch power-ups with paddle        " & ChrW(8226) & " Enter your name on high scores", fb, brB, lx + 8, y) : y += 25
                    End Using
                End Using
                g.DrawString("SETTINGS:", fh, New SolidBrush(Color.FromArgb(255, 240, 100)), lx, y) : y += 28
                Dim barX = px + 260, barW As Single = 200, barH As Single = 16
                Dim items = {
                    $"SFX Volume:",
                    $"Music Volume:",
                    $"Music Speed:",
                    $"Music Style:",
                    $"SFX Style:",
                    $"Colorblind Mode:",
                    $"Window Size:"}
                For idx = 0 To 6
                    Dim sc = If(_settingsSelection = idx, Color.FromArgb(255, 255, 120), Color.FromArgb(195, 195, 215))
                    Dim sel = If(_settingsSelection = idx, ChrW(&H25BA) & "  ", "    ")
                    Using brS As New SolidBrush(sc)
                        g.DrawString(sel & items(idx), fb, brS, lx + 8, y)
                    End Using
                    Using fv As New Font("Segoe UI", 10, FontStyle.Bold)
                        Select Case idx
                            Case 0
                                Dim sndSpr = TryGetSprite(If(_sfxVolume > 0, "ui/sound_on", "ui/sound_off"))
                                If sndSpr IsNot Nothing Then g.DrawImage(sndSpr, barX - 28, y, 22, 22)
                                DrawVolumeBar(g, barX, y + 2, barW, barH, _sfxVolume, sc)
                                Using brS As New SolidBrush(sc)
                                    g.DrawString($"{_sfxVolume}%", fv, brS, barX + barW + 10, y)
                                End Using
                            Case 1
                                Dim musSpr = TryGetSprite(If(_musicVolume > 0, "ui/music_on", "ui/music_off"))
                                If musSpr IsNot Nothing Then g.DrawImage(musSpr, barX - 28, y, 22, 22)
                                Dim ev = GetEffectiveMusicVolume()
                                DrawVolumeBar(g, barX, y + 2, barW, barH, ev, sc)
                                Using brS As New SolidBrush(sc)
                                    g.DrawString($"{ev}%", fv, brS, barX + barW + 10, y)
                                End Using
                            Case 2
                                Dim spd = Math.Max(0, Math.Min(200, _musicSpeed))
                                DrawVolumeBar(g, barX, y + 2, barW, barH, Math.Min(100, spd), sc)
                                Using brS As New SolidBrush(sc)
                                    g.DrawString($"{spd}%", fv, brS, barX + barW + 10, y)
                                End Using
                            Case 3
                                Using brS As New SolidBrush(sc)
                                    g.DrawString(ChrW(&H25C4) & " " & _musicStyleNames(_musicStyle) & " " & ChrW(&H25BA), fv, brS, barX, y)
                                End Using
                            Case 4
                                Using brS As New SolidBrush(sc)
                                    g.DrawString(ChrW(&H25C4) & " " & _sfxStyleNames(_sfxStyle) & " " & ChrW(&H25BA), fv, brS, barX, y)
                                End Using
                            Case 5
                                Dim cbText = If(_colorblindMode, "ON", "OFF")
                                Dim cbC = If(_colorblindMode, Color.FromArgb(100, 255, 150), Color.FromArgb(255, 100, 100))
                                Using brS As New SolidBrush(cbC)
                                    g.DrawString($"[ {cbText} ]", fv, brS, barX, y)
                                End Using
                            Case 6
                                Using brS As New SolidBrush(sc)
                                    g.DrawString(ChrW(&H25C4) & " " & _windowScaleNames(_windowScale) & " " & ChrW(&H25BA), fv, brS, barX, y)
                                End Using
                        End Select
                    End Using
                    y += 28
                Next
            End Using
        End Using
        Using fHint As New Font("Segoe UI", 10, FontStyle.Regular)
            DrawCenteredText(g, ChrW(&H2191) & ChrW(&H2193) & " Select   " & ChrW(&H2190) & ChrW(&H2192) & " Adjust   ENTER Toggle   O / ESC Close", fHint, Color.FromArgb(130, 130, 155), py + ph - 30)
        End Using
    End Sub

    Private Sub DrawHighScore(g As Graphics)
        DrawStarField(g)
        Using br As New SolidBrush(Color.FromArgb(200, 0, 0, 20))
            g.FillRectangle(br, 0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT)
        End Using
        Dim pw = 520, ph = 540
        Dim px = CSng((LOGICAL_WIDTH - pw) / 2), py = CSng((LOGICAL_HEIGHT - ph) / 2)
        Using br As New SolidBrush(Color.FromArgb(245, 12, 12, 35))
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.FillPath(br, rr)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(100, 255, 80, 80), 2)
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.DrawPath(pen, rr)
            End Using
        End Using
        Dim goSpr = TryGetSprite("ui/text_gameover")
        If goSpr IsNot Nothing Then
            g.DrawImage(goSpr, CInt((LOGICAL_WIDTH - 320) / 2), CInt(py + 12), 320, 64)
        Else
            Using ft As New Font("Segoe UI", 30, FontStyle.Bold)
                DrawCenteredText(g, "GAME OVER", ft, Color.FromArgb(255, 80, 100), py + 15)
            End Using
        End If
        Using fs As New Font("Segoe UI", 18, FontStyle.Bold)
            DrawCenteredText(g, $"Final Score: {_score}", fs, Color.FromArgb(255, 255, 120), py + 65)
        End Using
        Using fl As New Font("Segoe UI", 12, FontStyle.Regular)
            DrawCenteredText(g, $"Level {_level}  |  Ball Size: {_ballRadius}px", fl, Color.FromArgb(180, 200, 255), py + 100)
        End Using
        Dim y = py + 135
        If Not _highScoreSaved Then
            Dim cursor = If(_frameCount Mod 60 < 30, "_", " ")
            Using fl As New Font("Segoe UI", 14, FontStyle.Regular)
                DrawCenteredText(g, "Enter Name: " & _nameInput & cursor, fl, Color.White, y)
            End Using
            y += 30
            Using fh As New Font("Segoe UI", 10, FontStyle.Regular)
                DrawCenteredText(g, "Press ENTER to save", fh, Color.FromArgb(140, 140, 160), y)
            End Using
        Else
            Using fh As New Font("Segoe UI", 12, FontStyle.Regular)
                DrawCenteredText(g, "Score saved! Press SPACE to continue", fh, Color.FromArgb(100, 255, 150), y)
            End Using
        End If
        y = py + 210
        Dim trophySpr = TryGetSprite("ui/trophy")
        Dim lbSpr = TryGetSprite("ui/leaderboard")
        If trophySpr IsNot Nothing Then
            g.DrawImage(trophySpr, CSng(LOGICAL_WIDTH / 2 - 115), y + 2, 24, 24)
        End If
        If lbSpr IsNot Nothing Then
            g.DrawImage(lbSpr, CSng(LOGICAL_WIDTH / 2 + 92), y + 2, 24, 24)
        End If
        Using fHeader As New Font("Segoe UI", 14, FontStyle.Bold)
            DrawCenteredText(g, "HIGH SCORES", fHeader, Color.FromArgb(100, 200, 255), y)
        End Using
        y += 30
        Using fe As New Font("Consolas", 12, FontStyle.Regular)
            If _highScores.Count = 0 Then
                DrawCenteredText(g, "No scores yet!", fe, Color.FromArgb(140, 140, 160), y)
            Else
                For i = 0 To Math.Min(9, _highScores.Count - 1)
                    Dim rec = _highScores(i)
                    Dim rank = (i + 1).ToString().PadLeft(2)
                    Dim nm = If(rec.PlayerName.Length > 12, rec.PlayerName.Substring(0, 12), rec.PlayerName.PadRight(12))
                    Dim sc = rec.PlayerScore.ToString("N0").PadLeft(10)
                    Dim ec = If(_highScoreSaved AndAlso rec.PlayerName = _nameInput AndAlso rec.PlayerScore = _score,
                                Color.FromArgb(255, 255, 120), Color.FromArgb(195, 195, 215))
                    If i = 0 Then
                        Dim goldSpr = TryGetSprite("ui/medal_gold")
                        If goldSpr IsNot Nothing Then g.DrawImage(goldSpr, CSng(LOGICAL_WIDTH / 2 - 230), y + 2, 18, 18)
                    ElseIf i = 1 Then
                        Dim silverSpr = TryGetSprite("ui/medal_silver")
                        If silverSpr IsNot Nothing Then g.DrawImage(silverSpr, CSng(LOGICAL_WIDTH / 2 - 230), y + 2, 18, 18)
                    End If
                    Using br As New SolidBrush(ec)
                        Dim text = $"{rank}. {nm} {sc}"
                        Dim sz = g.MeasureString(text, fe)
                        g.DrawString(text, fe, br, (LOGICAL_WIDTH - sz.Width) / 2, y)
                    End Using
                    y += 25
                Next
            End If
        End Using
    End Sub

    Private Sub DrawOverlay(g As Graphics, title As String, subtitle As String, Optional animated As Boolean = False)
        Using br As New SolidBrush(Color.FromArgb(180, 0, 0, 20))
            g.FillRectangle(br, 0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT)
        End Using
        If Not animated Then
            Dim pauseSpr = TryGetSprite("ui/pause")
            If pauseSpr IsNot Nothing Then
                Dim ps = 48
                g.DrawImage(pauseSpr, CInt((LOGICAL_WIDTH - ps) / 2), CInt(LOGICAL_HEIGHT / 2 - 120), ps, ps)
            End If
        End If
        Dim titleSize As Single = If(animated, CSng(40 + Math.Sin(_frameCount * 0.08) * 6), 40.0F)
        Dim pulse = CSng((Math.Sin(_frameCount * 0.05) + 1) / 2)
        Dim titleColor As Color = If(animated,
            Color.FromArgb(255, CInt(180 + pulse * 75), CInt(180 + pulse * 75)),
            Color.White)
        If animated Then
            Dim winSpr = TryGetSprite("ui/text_youwin")
            If winSpr IsNot Nothing Then
                Dim wW = 280, wH = 58
                g.DrawImage(winSpr, CInt((LOGICAL_WIDTH - wW) / 2), CInt(LOGICAL_HEIGHT / 2 - 80), wW, wH)
            Else
                Using ft As New Font("Segoe UI", titleSize, FontStyle.Bold)
                    DrawCenteredText(g, title, ft, titleColor, LOGICAL_HEIGHT / 2 - 60)
                End Using
            End If
        Else
            Using ft As New Font("Segoe UI", titleSize, FontStyle.Bold)
                DrawCenteredText(g, title, ft, titleColor, LOGICAL_HEIGHT / 2 - 60)
            End Using
        End If
        Dim resumeSpr = If(subtitle.Contains("resume"), TryGetSprite("ui/text_resume"), Nothing)
        If resumeSpr IsNot Nothing Then
            g.DrawImage(resumeSpr, CInt((LOGICAL_WIDTH - 200) / 2), CInt(LOGICAL_HEIGHT / 2 + 8), 200, 36)
        Else
            DrawCenteredText(g, subtitle, _fnt16r, Color.FromArgb(200, 200, 220), LOGICAL_HEIGHT / 2 + 10)
        End If
    End Sub

    Private Sub DrawCenteredText(g As Graphics, text As String, font As Font, color As Color, y As Single)
        Dim sz = g.MeasureString(text, font)
        Dim x = (LOGICAL_WIDTH - sz.Width) / 2
        If _brShadow IsNot Nothing Then g.DrawString(text, font, _brShadow, x + 1, y + 1)
        Using br As New SolidBrush(color)
            g.DrawString(text, font, br, x, y)
        End Using
    End Sub

    Private Sub DrawTextShadow(g As Graphics, text As String, font As Font, color As Color, x As Single, y As Single)
        If _brShadow IsNot Nothing Then g.DrawString(text, font, _brShadow, x + 1, y + 1)
        Using br As New SolidBrush(color)
            g.DrawString(text, font, br, x, y)
        End Using
    End Sub

    Private Sub DrawVolumeBar(g As Graphics, x As Single, y As Single, w As Single, h As Single, value As Integer, barColor As Color)
        Dim trackSpr = TryGetSprite("ui/slider_track")
        Dim handleSpr = TryGetSprite("ui/slider_handle")
        If trackSpr IsNot Nothing Then
            g.DrawImage(trackSpr, x, y, w, h)
        Else
            Using br As New SolidBrush(Color.FromArgb(60, 255, 255, 255))
                Using rr = RoundedRect(New RectangleF(x, y, w, h), 4)
                    g.FillPath(br, rr)
                End Using
            End Using
        End If
        If trackSpr Is Nothing Then
            Dim fw = CSng(w * value / 100.0)
            If fw > 2 Then
                Using br As New SolidBrush(Color.FromArgb(200, barColor.R, barColor.G, barColor.B))
                    Using rr = RoundedRect(New RectangleF(x, y, fw, h), 4)
                        g.FillPath(br, rr)
                    End Using
                End Using
            End If
        End If
        If handleSpr IsNot Nothing Then
            Dim hx = x + CSng(w * Math.Max(0, Math.Min(100, value)) / 100.0) - h / 2
            g.DrawImage(handleSpr, hx, y - 2, h + 4, h + 4)
        End If
    End Sub

    Private Sub DrawHeartShape(g As Graphics, x As Single, y As Single, w As Single, h As Single, fillColor As Color)
        Dim pad = w * 0.08F
        Dim iw = w - pad * 2
        Dim ih = h - pad * 2
        Dim ix = x + pad, iy = y + pad
        Dim c1 = Color.FromArgb(fillColor.A,
            Math.Min(255, CInt(fillColor.R) + 70),
            Math.Min(255, CInt(fillColor.G) + 20),
            Math.Min(255, CInt(fillColor.B) + 20))
        Using br As New LinearGradientBrush(New RectangleF(ix, iy, iw, ih + 1), c1, fillColor, LinearGradientMode.Vertical)
            g.FillEllipse(br, ix, iy + ih * 0.04F, iw * 0.54F, ih * 0.54F)
            g.FillEllipse(br, ix + iw * 0.46F, iy + ih * 0.04F, iw * 0.54F, ih * 0.54F)
            Dim pts() As PointF = {
                New PointF(ix, iy + ih * 0.3F),
                New PointF(ix + iw, iy + ih * 0.3F),
                New PointF(ix + iw * 0.5F, iy + ih)
            }
            g.FillPolygon(br, pts)
        End Using
        Using br As New SolidBrush(Color.FromArgb(110, 255, 255, 255))
            g.FillEllipse(br, ix + iw * 0.1F, iy + ih * 0.08F, iw * 0.22F, ih * 0.18F)
        End Using
    End Sub

    Private Function RoundedRect(rect As RectangleF, radius As Integer) As GraphicsPath
        Dim path As New GraphicsPath()
        Dim d = radius * 2
        path.AddArc(rect.X, rect.Y, d, d, 180, 90)
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90)
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90)
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90)
        path.CloseFigure()
        Return path
    End Function
#End Region

End Class
