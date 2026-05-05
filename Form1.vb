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
        NameEntry
        Menu
        Playing
        Paused
        LevelComplete
        GameOver
        Options
        HighScore
        Store
        Credits
    End Enum

    ' ── Store item categories ──────────────────────────────────────────────────
    Private Enum StoreCategory
        Balls
        Bricks
        Bonuses
    End Enum

    Private Structure StoreItem
        Public Id As String
        Public Name As String
        Public Description As String
        Public Price As Integer
        Public Category As StoreCategory
        Public IsBase As Boolean      ' Base items are free and pre-owned
    End Structure
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
    Private _proceduralSeed As Integer = 0   ' set fresh each new game; drives brick palette + power-up shapes
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

    ' ── In-game economy ───────────────────────────────────────────────────────
    ' Coins are earned while playing (separate from display score).
    ' All coin earnings use _coinsEarnedThisSession; balance is persisted.
    Private _coinBalance As Integer = 0
    Private _coinsEarnedThisSession As Integer = 0

    ' Store navigation
    Private _storeCategory As StoreCategory = StoreCategory.Balls
    Private _storeSelectedIndex As Integer = 0
    Private _storeScrollOffset As Integer = 0   ' first visible card index

    ' Active cosmetic selections
    Private _activeBallSkin As String = "base"
    Private _activeBrickPalette As String = "base"
    Private _activeBonusPack As String = "base"

    ' Coin earn rates (per brick broken, scaled by combo)
    Private Const COIN_PER_BRICK As Integer = 1
    Private Const COIN_LEVEL_BONUS As Integer = 10

    ' Store catalog — base items are free; others cost coins
    Private ReadOnly _storeItems As New List(Of StoreItem)

    ' Persistence path for store save data — set per-player in SetPlayerProfile()
    Private _storeSavePath As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BrickBlast", "store.json")

    ' Per-player profile
    Private ReadOnly _playersDir As String = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BrickBlast", "players")
    Private _playerName As String = ""
    Private _nameEntryInput As String = ""

    ' JSON-serializable save record for the economy
    Private Class StoreSaveData
        Public Property PlayerName As String
        Public Property CoinBalance As Integer
        Public Property OwnedItems As List(Of String)
        Public Property ActiveBallSkin As String
        Public Property ActiveBrickPalette As String
        Public Property ActiveBonusPack As String
        Public Sub New()
            OwnedItems = New List(Of String)
        End Sub
    End Class

    Private _ownedItems As New HashSet(Of String)
    Private _devMode As Boolean = False

    ' ── Networking ──────────────────────────────────────────────────────────
    Private Const SyncEndpointUrl As String = "https://your-sync-endpoint.example.com/api/profile"
    Private _syncStatus As String = "Offline"   ' Offline | Syncing | Synced | Failed
    Private _lastSyncUtc As DateTime = DateTime.MinValue
    Private _httpClient As New System.Net.Http.HttpClient() With {.Timeout = TimeSpan.FromSeconds(5)}


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
        _state = GameState.NameEntry
        LoadHighScores()
        InitStoreItems()   ' seeds base-owned items; real profile loads after name entry
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
        If _state = GameState.NameEntry Then
            Select Case e.KeyCode
                Case Keys.Back
                    If _nameEntryInput.Length > 0 Then _nameEntryInput = _nameEntryInput.Substring(0, _nameEntryInput.Length - 1)
                Case Keys.Enter
                    Dim entered = _nameEntryInput.Trim()
                    If entered.Length > 0 Then
                        SetPlayerProfile(entered)
                        _state = GameState.Menu
                    End If
                Case Else
                    Dim c = KeyToChar(e)
                    If c IsNot Nothing AndAlso _nameEntryInput.Length < 16 Then _nameEntryInput &= c
            End Select
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

        If _state = GameState.Credits Then
            If e.KeyCode = Keys.Escape OrElse e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.C Then
                _state = GameState.Menu
            End If
            Return
        End If

        If _state = GameState.Store Then
            ' Compute rowsVisible dynamically so scroll clamp matches DrawStore
            Const KBD_PAD_H As Integer = 36, KBD_PAD_V As Integer = 28
            Const KBD_HDR_H As Integer = 72, KBD_TAB_H As Integer = 52
            Const KBD_HDR_TAB_GAP As Integer = 8, KBD_TAB_GRID_GAP As Integer = 20
            Const KBD_FTR_H As Integer = 50, KBD_FTR_PAD As Integer = 20
            Const KBD_SB_W As Integer = 14, KBD_SB_GAP As Integer = 12
            Const KBD_ROW_GAP As Integer = 14, KBD_CARD_H As Integer = 132
            Dim KPH = CInt(LOGICAL_HEIGHT * 0.86)
            Dim KPW = CInt(LOGICAL_WIDTH * 0.88)
            Dim kpy = CSng((LOGICAL_HEIGHT - KPH) / 2)
            Dim kGridTop = kpy + KBD_PAD_V + KBD_HDR_H + KBD_HDR_TAB_GAP + KBD_TAB_H + KBD_TAB_GRID_GAP
            Dim kFtrTop = kpy + KPH - KBD_FTR_PAD - KBD_FTR_H
            Dim kGridH = Math.Max(1, kFtrTop - 8 - kGridTop)
            Dim kInnerW = KPW - KBD_PAD_H * 2
            Dim kGridW = CInt(kInnerW - KBD_SB_W - KBD_SB_GAP)
            Dim kCols = If(kGridW >= 640, 2, 1)
            Dim CARDS_VIS = Math.Max(1, CInt(Math.Floor((kGridH + KBD_ROW_GAP) / (KBD_CARD_H + KBD_ROW_GAP))))
            Select Case e.KeyCode
                Case Keys.Left, Keys.A
                    _storeCategory = CType((_storeCategory - 1 + 3) Mod 3, StoreCategory)
                    _storeSelectedIndex = 0
                    _storeScrollOffset = 0
                Case Keys.Right, Keys.D
                    _storeCategory = CType((_storeCategory + 1) Mod 3, StoreCategory)
                    _storeSelectedIndex = 0
                    _storeScrollOffset = 0
                Case Keys.Up
                    Dim catItemsUp = _storeItems.Where(Function(it) it.Category = _storeCategory).ToList()
                    If catItemsUp.Count > 0 Then
                        _storeSelectedIndex = Math.Max(0, _storeSelectedIndex - kCols)
                        Dim rowUp = _storeSelectedIndex \ kCols
                        If rowUp < _storeScrollOffset Then _storeScrollOffset = rowUp
                        If rowUp >= _storeScrollOffset + CARDS_VIS Then _storeScrollOffset = rowUp - CARDS_VIS + 1
                    End If
                Case Keys.Down
                    Dim catItemsDn = _storeItems.Where(Function(it) it.Category = _storeCategory).ToList()
                    If catItemsDn.Count > 0 Then
                        _storeSelectedIndex = Math.Min(catItemsDn.Count - 1, _storeSelectedIndex + kCols)
                        Dim rowDn = _storeSelectedIndex \ kCols
                        If rowDn < _storeScrollOffset Then _storeScrollOffset = rowDn
                        If rowDn >= _storeScrollOffset + CARDS_VIS Then _storeScrollOffset = rowDn - CARDS_VIS + 1
                    End If
                Case Keys.Enter, Keys.Space
                    Dim catItemsEnt = _storeItems.Where(Function(it) it.Category = _storeCategory).ToList()
                    If _storeSelectedIndex >= 0 AndAlso _storeSelectedIndex < catItemsEnt.Count Then
                        Dim sel = catItemsEnt(_storeSelectedIndex)
                        If IsOwned(sel.Category, sel.Id) Then
                            EquipItem(sel)
                        Else
                            PurchaseItem(sel)
                        End If
                    End If
                Case Keys.Escape, Keys.H, Keys.O
                    _state = GameState.Menu
            End Select
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
                Case Keys.S
                    SyncProfileAsync()
                Case Keys.O, Keys.H, Keys.Escape
                    _state = _previousState
            End Select
            Return
        End If

        If _state = GameState.GameOver Then
            Select Case e.KeyCode
                Case Keys.R, Keys.Enter, Keys.Space
                    _highScoreDelayFrames = 0 : _pendingHighScore = False
                    StartNewGame()
                Case Keys.S
                    _highScoreDelayFrames = 0 : _pendingHighScore = False
                    _storeCategory = StoreCategory.Balls : _storeSelectedIndex = 0
                    _state = GameState.Store
                Case Keys.Escape, Keys.M
                    _highScoreDelayFrames = 0 : _pendingHighScore = False
                    _state = GameState.Menu
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
            Case Keys.S
                If _state = GameState.Menu Then
                    _storeCategory = StoreCategory.Balls
                    _storeSelectedIndex = 0
                    _state = GameState.Store
                End If
            Case Keys.C
                If _state = GameState.Menu Then
                    _state = GameState.Credits
                End If
            Case Keys.F12
                If _state = GameState.Menu Then
                    ExportMarketingAssets()
                End If
            Case Keys.Escape
                If _state = GameState.Credits Then
                    _state = GameState.Menu
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

    Private Sub Form1_MouseWheel(sender As Object, e As MouseEventArgs) Handles MyBase.MouseWheel
        If _state = GameState.Store Then
            ' Mirror DrawStore responsive geometry
            Const PAD_H_W As Integer = 36, TAB_H_W As Integer = 52
            Const HDR_H_W As Integer = 72, HDR_TAB_GAP_W As Integer = 8
            Const TAB_GRID_GAP_W As Integer = 20, FTR_H_W As Integer = 50
            Const FTR_PAD_W As Integer = 20, SB_W_W As Integer = 14
            Const SB_GAP_W As Integer = 12, COL_GAP_W As Integer = 16
            Const ROW_GAP_W As Integer = 14, CARD_H_W As Integer = 132
            Const PAD_V_W As Integer = 28
            Dim PH_W = CInt(LOGICAL_HEIGHT * 0.86)
            Dim py2_W = CSng((LOGICAL_HEIGHT - PH_W) / 2)
            Dim PW_W = CInt(LOGICAL_WIDTH * 0.88)
            Dim innerW_W = PW_W - PAD_H_W * 2
            Dim gridTop_W = py2_W + PAD_V_W + HDR_H_W + HDR_TAB_GAP_W + TAB_H_W + TAB_GRID_GAP_W
            Dim ftrTop_W = py2_W + PH_W - FTR_PAD_W - FTR_H_W
            Dim gridH_W = Math.Max(1, ftrTop_W - 8 - gridTop_W)
            Dim sbGutter_W = SB_W_W + SB_GAP_W
            Dim gridW_W = CInt(innerW_W - sbGutter_W)
            Dim cols_W = If(gridW_W >= 640, 2, 1)
            Dim rowsVisible_W = Math.Max(1, CInt(Math.Floor((gridH_W + ROW_GAP_W) / (CARD_H_W + ROW_GAP_W))))
            Dim catItemsW = _storeItems.Where(Function(it) it.Category = _storeCategory).ToList()
            Dim totalRowsW = CInt(Math.Ceiling(catItemsW.Count / CDbl(cols_W)))
            Dim maxScrollW = Math.Max(0, totalRowsW - rowsVisible_W)
            If e.Delta < 0 Then
                _storeScrollOffset = Math.Min(_storeScrollOffset + 1, maxScrollW)
            Else
                _storeScrollOffset = Math.Max(_storeScrollOffset - 1, 0)
            End If
        End If
    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown
        Dim mx = CSng(e.X) * LOGICAL_WIDTH / ClientSize.Width
        Dim my = CSng(e.Y) * LOGICAL_HEIGHT / ClientSize.Height
        ' Touch/click to start, resume, or advance level
        If _state = GameState.NameEntry Then
            ' Clicking a returning-player name fills the input box
            Dim existing2 = GetExistingPlayerNames()
            If existing2.Count > 0 Then
                Dim ph2 = 420
                Dim py2 = CSng((LOGICAL_HEIGHT - ph2) / 2)
                Dim ry = py2 + 278.0F
                For Each nm In existing2.Take(5)
                    If my >= ry AndAlso my < ry + 22 Then
                        _nameEntryInput = nm
                        Return
                    End If
                    ry += 24
                Next
            End If
            Return
        End If
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
        If _state = GameState.GameOver Then
            Dim goPw = 480, goPh = 360
            Dim goPx = CSng((LOGICAL_WIDTH - goPw) / 2), goPy = CSng((LOGICAL_HEIGHT - goPh) / 2)
            Dim goBtnW = 120, goBtnH = 36, goGap = 18
            Dim goTotalW = goBtnW * 3 + goGap * 2
            Dim goBx = goPx + (goPw - goTotalW) / 2
            Dim goBy = goPy + 30 + 50 + 36 + 36 + 46
            ' Retry
            If mx >= goBx AndAlso mx < goBx + goBtnW AndAlso my >= goBy AndAlso my < goBy + goBtnH Then
                _highScoreDelayFrames = 0 : _pendingHighScore = False
                StartNewGame()
                Return
            End If
            ' Store
            Dim goSx = goBx + goBtnW + goGap
            If mx >= goSx AndAlso mx < goSx + goBtnW AndAlso my >= goBy AndAlso my < goBy + goBtnH Then
                _highScoreDelayFrames = 0 : _pendingHighScore = False
                _storeCategory = StoreCategory.Balls : _storeSelectedIndex = 0
                _state = GameState.Store
                Return
            End If
            ' Menu
            Dim goMx = goBx + (goBtnW + goGap) * 2
            If mx >= goMx AndAlso mx < goMx + goBtnW AndAlso my >= goBy AndAlso my < goBy + goBtnH Then
                _highScoreDelayFrames = 0 : _pendingHighScore = False
                _state = GameState.Menu
                Return
            End If
            Return
        End If
        If _state = GameState.Store Then
            ' ── Geometry must mirror DrawStore layout constants ──────────────────
            Const PAD_H_HT As Integer = 36, PAD_V_HT As Integer = 28
            Const HDR_H_HT As Integer = 72, TAB_H_HT As Integer = 52
            Const HDR_TAB_GAP_HT As Integer = 8, TAB_GRID_GAP_HT As Integer = 20
            Const FTR_H_HT As Integer = 50, FTR_PAD_HT As Integer = 20
            Const SB_W_HT As Integer = 14, SB_GAP_HT As Integer = 12
            Const COL_GAP_HT As Integer = 16, ROW_GAP_HT As Integer = 14
            Const CARD_H_HT As Integer = 132, BTN_W_HT As Integer = 152, BTN_H_HT As Integer = 42

            Dim PW_HT = CInt(LOGICAL_WIDTH * 0.88)
            Dim PH_HT = CInt(LOGICAL_HEIGHT * 0.86)
            Dim px2 = CSng((LOGICAL_WIDTH - PW_HT) / 2)
            Dim py2 = CSng((LOGICAL_HEIGHT - PH_HT) / 2)
            Dim innerX_HT = px2 + PAD_H_HT
            Dim innerW_HT = PW_HT - PAD_H_HT * 2

            Dim hdrBottom_HT = py2 + PAD_V_HT + HDR_H_HT
            Dim tabY_HT = hdrBottom_HT + HDR_TAB_GAP_HT
            Dim tabBottom_HT = tabY_HT + TAB_H_HT
            Dim ftrTop_HT = py2 + PH_HT - FTR_PAD_HT - FTR_H_HT
            Dim gridTop_HT = tabBottom_HT + TAB_GRID_GAP_HT
            Dim gridH_HT = Math.Max(1, ftrTop_HT - 8 - gridTop_HT)

            Dim sbGutter_HT = SB_W_HT + SB_GAP_HT
            Dim gridW_HT = CInt(innerW_HT - sbGutter_HT)
            Dim cols_HT = If(gridW_HT >= 640, 2, 1)
            Dim cardW_HT = (gridW_HT - (cols_HT - 1) * COL_GAP_HT) \ cols_HT
            Dim rowsVisible_HT = Math.Max(1, CInt(Math.Floor((gridH_HT + ROW_GAP_HT) / (CARD_H_HT + ROW_GAP_HT))))

            ' Tab row
            Const TAB_COUNT_HT As Integer = 3
            Dim totalTabW_HT = CInt(innerW_HT * 0.88)
            Dim tabW_HT = (totalTabW_HT - (TAB_COUNT_HT - 1) * 8) \ TAB_COUNT_HT
            Dim tabsLeft_HT = CSng(innerX_HT + (innerW_HT - totalTabW_HT) / 2)
            If my >= tabY_HT AndAlso my < tabY_HT + TAB_H_HT Then
                For ti = 0 To TAB_COUNT_HT - 1
                    Dim tx = tabsLeft_HT + ti * (tabW_HT + 8)
                    If mx >= tx AndAlso mx < tx + tabW_HT Then
                        _storeCategory = CType(ti, StoreCategory)
                        _storeSelectedIndex = 0
                        _storeScrollOffset = 0
                    End If
                Next
                Return
            End If

            ' ← MENU button in footer
            Const MENU_BTN_W_HT As Integer = 110
            Const MENU_BTN_H_HT As Integer = 32
            Dim kFtrTop2 = py2 + PH_HT - FTR_PAD_HT - FTR_H_HT
            Dim menuBtnX2 = CSng(innerX_HT)
            Dim menuBtnY2 = CSng(kFtrTop2 + (FTR_H_HT - MENU_BTN_H_HT) / 2)
            If mx >= menuBtnX2 AndAlso mx < menuBtnX2 + MENU_BTN_W_HT AndAlso
               my >= menuBtnY2 AndAlso my < menuBtnY2 + MENU_BTN_H_HT Then
                _state = GameState.Menu
                Return
            End If

            ' Card grid
            Dim catItems2 = _storeItems.Where(Function(it) it.Category = _storeCategory).ToList()
            Dim totalRows2 = CInt(Math.Ceiling(catItems2.Count / CDbl(cols_HT)))
            Dim maxScroll2 = Math.Max(0, totalRows2 - rowsVisible_HT)
            _storeScrollOffset = Math.Max(0, Math.Min(_storeScrollOffset, maxScroll2))

            For rowOff = 0 To rowsVisible_HT - 1
                Dim rowIdx2 = _storeScrollOffset + rowOff
                For col2 = 0 To cols_HT - 1
                    Dim itemIdx2 = rowIdx2 * cols_HT + col2
                    If itemIdx2 >= catItems2.Count Then Continue For
                    Dim cx2 = CSng(innerX_HT + col2 * (cardW_HT + COL_GAP_HT))
                    Dim cy2 = CSng(gridTop_HT + rowOff * (CARD_H_HT + ROW_GAP_HT))
                    If mx >= cx2 AndAlso mx < cx2 + cardW_HT AndAlso
                       my >= cy2 AndAlso my < cy2 + CARD_H_HT Then
                        _storeSelectedIndex = itemIdx2
                        Dim sel2 = catItems2(itemIdx2)
                        If IsOwned(sel2.Category, sel2.Id) Then
                            EquipItem(sel2)
                        Else
                            PurchaseItem(sel2)
                        End If
                        Return
                    End If
                Next
            Next
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
            Case GameState.NameEntry
                DrawNameEntry(g)
            Case GameState.Menu
                DrawMenu(g)
            Case GameState.Playing, GameState.Paused
                DrawGame(g)
                If _state = GameState.Paused Then DrawOverlay(g, "PAUSED", "Press SPACE to resume")
            Case GameState.LevelComplete
                DrawGame(g)
                DrawOverlay(g, $"LEVEL {_level} COMPLETE!", "Press SPACE for next level", True)
            Case GameState.GameOver
                DrawGame(g)
                DrawGameOverScreen(g)
            Case GameState.Options
                If _previousState = GameState.Playing OrElse _previousState = GameState.Paused Then DrawGame(g)
                DrawOptions(g)
            Case GameState.HighScore
                DrawHighScore(g)
            Case GameState.Store
                DrawStore(g)
            Case GameState.Credits
                DrawCredits(g)
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
        _proceduralSeed = Environment.TickCount  ' fresh seed → new procedural palette every game
        SetupLevel()
        _state = GameState.Playing
        PlaySFX(_sfxData(_sfxStyle)(10), 100)
        LogEvent("GameStarted", $"player={_playerName} level={_level}")
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
        LogEvent("LevelStarted", $"player={_playerName} level={_level}")
    End Sub

    ' Generates a 7-row palette
    ' Store cosmetic palettes are separate and intentionally override this.
    Private Function GenerateProceduralBrickPalette() As Color()()
        Dim rnd As New Random(_proceduralSeed)
        Dim pal(6)() As Color
        For r = 0 To 6
            Dim h1 = rnd.Next(0, 360)
            Dim h2 = (h1 + rnd.Next(20, 60)) Mod 360
            pal(r) = New Color() {ColorFromHSV(h1, 0.65 + rnd.NextDouble() * 0.2, 0.9 + rnd.NextDouble() * 0.1),
                                   ColorFromHSV(h2, 0.5 + rnd.NextDouble() * 0.2, 1.0)}
        Next
        Return pal
    End Function

    Private Function GetBrickPalette() As Color()()
        Select Case _activeBrickPalette
            Case "neon"
                Return {
                    New Color() {Color.FromArgb(255, 0, 220), Color.FromArgb(255, 80, 240)},
                    New Color() {Color.FromArgb(0, 220, 255), Color.FromArgb(80, 240, 255)},
                    New Color() {Color.FromArgb(0, 255, 80), Color.FromArgb(80, 255, 160)},
                    New Color() {Color.FromArgb(255, 255, 0), Color.FromArgb(255, 255, 100)},
                    New Color() {Color.FromArgb(255, 80, 0), Color.FromArgb(255, 160, 80)},
                    New Color() {Color.FromArgb(80, 0, 255), Color.FromArgb(160, 80, 255)},
                    New Color() {Color.FromArgb(0, 255, 255), Color.FromArgb(80, 255, 255)}}
            Case "pastel"
                Return {
                    New Color() {Color.FromArgb(255, 182, 193), Color.FromArgb(255, 210, 218)},
                    New Color() {Color.FromArgb(255, 218, 185), Color.FromArgb(255, 235, 210)},
                    New Color() {Color.FromArgb(255, 255, 180), Color.FromArgb(255, 255, 210)},
                    New Color() {Color.FromArgb(180, 255, 200), Color.FromArgb(210, 255, 220)},
                    New Color() {Color.FromArgb(180, 220, 255), Color.FromArgb(210, 235, 255)},
                    New Color() {Color.FromArgb(210, 190, 255), Color.FromArgb(230, 215, 255)},
                    New Color() {Color.FromArgb(255, 190, 240), Color.FromArgb(255, 215, 248)}}
            Case "metal"
                Return {
                    New Color() {Color.FromArgb(180, 180, 195), Color.FromArgb(220, 220, 235)},
                    New Color() {Color.FromArgb(160, 170, 185), Color.FromArgb(200, 210, 225)},
                    New Color() {Color.FromArgb(140, 155, 170), Color.FromArgb(180, 195, 210)},
                    New Color() {Color.FromArgb(120, 135, 155), Color.FromArgb(160, 175, 195)},
                    New Color() {Color.FromArgb(100, 115, 135), Color.FromArgb(140, 155, 175)},
                    New Color() {Color.FromArgb(80,  100, 120), Color.FromArgb(120, 140, 160)},
                    New Color() {Color.FromArgb(60,  80,  100), Color.FromArgb(100, 120, 140)}}
            Case "candy"
                Return {
                    New Color() {Color.FromArgb(255, 100, 150), Color.FromArgb(255, 160, 190)},
                    New Color() {Color.FromArgb(255, 160, 80), Color.FromArgb(255, 200, 140)},
                    New Color() {Color.FromArgb(255, 240, 80), Color.FromArgb(255, 250, 160)},
                    New Color() {Color.FromArgb(80, 240, 160), Color.FromArgb(160, 255, 200)},
                    New Color() {Color.FromArgb(80, 180, 255), Color.FromArgb(160, 220, 255)},
                    New Color() {Color.FromArgb(200, 100, 255), Color.FromArgb(230, 160, 255)},
                    New Color() {Color.FromArgb(255, 100, 220), Color.FromArgb(255, 160, 240)}}
            Case "space"
                Return {
                    New Color() {Color.FromArgb(20, 20, 80), Color.FromArgb(50, 50, 140)},
                    New Color() {Color.FromArgb(10, 40, 80), Color.FromArgb(30, 80, 160)},
                    New Color() {Color.FromArgb(20, 60, 60), Color.FromArgb(40, 120, 120)},
                    New Color() {Color.FromArgb(40, 20, 80), Color.FromArgb(80, 40, 160)},
                    New Color() {Color.FromArgb(60, 10, 60), Color.FromArgb(120, 20, 120)},
                    New Color() {Color.FromArgb(80, 30, 20), Color.FromArgb(160, 60, 40)},
                    New Color() {Color.FromArgb(10, 10, 50), Color.FromArgb(30, 30, 100)}}
            Case "lava"
                Return {
                    New Color() {Color.FromArgb(255, 60, 0), Color.FromArgb(255, 120, 40)},
                    New Color() {Color.FromArgb(220, 40, 0), Color.FromArgb(255, 90, 20)},
                    New Color() {Color.FromArgb(200, 80, 0), Color.FromArgb(240, 130, 40)},
                    New Color() {Color.FromArgb(180, 30, 0), Color.FromArgb(220, 70, 10)},
                    New Color() {Color.FromArgb(160, 20, 0), Color.FromArgb(200, 50, 0)},
                    New Color() {Color.FromArgb(120, 10, 0), Color.FromArgb(180, 40, 0)},
                    New Color() {Color.FromArgb(255, 100, 20), Color.FromArgb(255, 160, 60)}}
            Case "ice"
                Return {
                    New Color() {Color.FromArgb(180, 230, 255), Color.FromArgb(220, 245, 255)},
                    New Color() {Color.FromArgb(140, 210, 255), Color.FromArgb(190, 235, 255)},
                    New Color() {Color.FromArgb(100, 190, 240), Color.FromArgb(160, 220, 250)},
                    New Color() {Color.FromArgb(80, 170, 225), Color.FromArgb(140, 205, 245)},
                    New Color() {Color.FromArgb(60, 150, 210), Color.FromArgb(120, 190, 235)},
                    New Color() {Color.FromArgb(40, 130, 200), Color.FromArgb(100, 175, 225)},
                    New Color() {Color.FromArgb(200, 240, 255), Color.FromArgb(230, 250, 255)}}
            Case "toxic"
                Return {
                    New Color() {Color.FromArgb(80, 255, 0), Color.FromArgb(160, 255, 60)},
                    New Color() {Color.FromArgb(60, 220, 0), Color.FromArgb(130, 240, 40)},
                    New Color() {Color.FromArgb(100, 200, 20), Color.FromArgb(170, 230, 80)},
                    New Color() {Color.FromArgb(40, 180, 0), Color.FromArgb(100, 210, 40)},
                    New Color() {Color.FromArgb(20, 160, 0), Color.FromArgb(80, 190, 20)},
                    New Color() {Color.FromArgb(0, 140, 20), Color.FromArgb(60, 170, 60)},
                    New Color() {Color.FromArgb(120, 255, 40), Color.FromArgb(200, 255, 100)}}
            Case "sunset"
                Return {
                    New Color() {Color.FromArgb(255, 80, 20), Color.FromArgb(255, 140, 60)},
                    New Color() {Color.FromArgb(255, 120, 40), Color.FromArgb(255, 180, 90)},
                    New Color() {Color.FromArgb(255, 160, 60), Color.FromArgb(255, 210, 120)},
                    New Color() {Color.FromArgb(255, 200, 80), Color.FromArgb(255, 230, 150)},
                    New Color() {Color.FromArgb(240, 100, 80), Color.FromArgb(255, 160, 120)},
                    New Color() {Color.FromArgb(220, 60, 100), Color.FromArgb(255, 120, 150)},
                    New Color() {Color.FromArgb(180, 40, 120), Color.FromArgb(230, 100, 170)}}
            Case "forest"
                Return {
                    New Color() {Color.FromArgb(20, 100, 20), Color.FromArgb(60, 150, 40)},
                    New Color() {Color.FromArgb(40, 120, 30), Color.FromArgb(80, 170, 60)},
                    New Color() {Color.FromArgb(60, 140, 40), Color.FromArgb(100, 190, 70)},
                    New Color() {Color.FromArgb(80, 110, 20), Color.FromArgb(130, 160, 50)},
                    New Color() {Color.FromArgb(100, 80, 10), Color.FromArgb(160, 130, 40)},
                    New Color() {Color.FromArgb(60, 160, 60), Color.FromArgb(110, 210, 90)},
                    New Color() {Color.FromArgb(30, 80, 10), Color.FromArgb(70, 130, 30)}}
            Case "ocean"
                Return {
                    New Color() {Color.FromArgb(0, 120, 180), Color.FromArgb(40, 170, 220)},
                    New Color() {Color.FromArgb(0, 100, 160), Color.FromArgb(20, 150, 200)},
                    New Color() {Color.FromArgb(0, 140, 160), Color.FromArgb(40, 190, 200)},
                    New Color() {Color.FromArgb(20, 80, 140), Color.FromArgb(60, 130, 190)},
                    New Color() {Color.FromArgb(0, 160, 180), Color.FromArgb(40, 210, 220)},
                    New Color() {Color.FromArgb(10, 60, 120), Color.FromArgb(40, 110, 170)},
                    New Color() {Color.FromArgb(0, 180, 200), Color.FromArgb(60, 220, 235)}}
            Case "galaxy"
                Return {
                    New Color() {Color.FromArgb(120, 0, 180), Color.FromArgb(180, 60, 230)},
                    New Color() {Color.FromArgb(80, 0, 160), Color.FromArgb(140, 40, 210)},
                    New Color() {Color.FromArgb(160, 20, 200), Color.FromArgb(210, 80, 240)},
                    New Color() {Color.FromArgb(60, 0, 140), Color.FromArgb(120, 20, 200)},
                    New Color() {Color.FromArgb(200, 60, 220), Color.FromArgb(240, 120, 255)},
                    New Color() {Color.FromArgb(100, 20, 160), Color.FromArgb(160, 60, 220)},
                    New Color() {Color.FromArgb(40, 0, 120), Color.FromArgb(100, 0, 180)}}
            Case "gold"
                Return {
                    New Color() {Color.FromArgb(255, 200, 0), Color.FromArgb(255, 235, 80)},
                    New Color() {Color.FromArgb(230, 170, 0), Color.FromArgb(255, 210, 60)},
                    New Color() {Color.FromArgb(200, 140, 0), Color.FromArgb(240, 185, 40)},
                    New Color() {Color.FromArgb(255, 215, 40), Color.FromArgb(255, 245, 120)},
                    New Color() {Color.FromArgb(180, 120, 0), Color.FromArgb(220, 165, 20)},
                    New Color() {Color.FromArgb(160, 100, 0), Color.FromArgb(200, 145, 20)},
                    New Color() {Color.FromArgb(255, 230, 80), Color.FromArgb(255, 250, 160)}}
            Case "obsidian"
                Return {
                    New Color() {Color.FromArgb(30, 25, 40), Color.FromArgb(60, 50, 75)},
                    New Color() {Color.FromArgb(20, 15, 30), Color.FromArgb(50, 40, 65)},
                    New Color() {Color.FromArgb(40, 30, 55), Color.FromArgb(70, 60, 90)},
                    New Color() {Color.FromArgb(15, 10, 25), Color.FromArgb(45, 35, 60)},
                    New Color() {Color.FromArgb(50, 40, 65), Color.FromArgb(80, 70, 100)},
                    New Color() {Color.FromArgb(10, 8, 18), Color.FromArgb(35, 28, 50)},
                    New Color() {Color.FromArgb(60, 50, 80), Color.FromArgb(90, 80, 115)}}
            Case "sakura"
                Return {
                    New Color() {Color.FromArgb(255, 182, 200), Color.FromArgb(255, 215, 228)},
                    New Color() {Color.FromArgb(255, 160, 185), Color.FromArgb(255, 200, 218)},
                    New Color() {Color.FromArgb(240, 140, 170), Color.FromArgb(255, 185, 210)},
                    New Color() {Color.FromArgb(255, 200, 215), Color.FromArgb(255, 225, 235)},
                    New Color() {Color.FromArgb(220, 120, 155), Color.FromArgb(250, 170, 195)},
                    New Color() {Color.FromArgb(200, 100, 140), Color.FromArgb(235, 155, 185)},
                    New Color() {Color.FromArgb(255, 210, 225), Color.FromArgb(255, 235, 245)}}
            Case "aurora"
                Return {
                    New Color() {Color.FromArgb(0, 200, 160), Color.FromArgb(60, 240, 200)},
                    New Color() {Color.FromArgb(0, 160, 200), Color.FromArgb(40, 210, 240)},
                    New Color() {Color.FromArgb(80, 0, 200), Color.FromArgb(140, 60, 240)},
                    New Color() {Color.FromArgb(0, 220, 120), Color.FromArgb(60, 255, 170)},
                    New Color() {Color.FromArgb(120, 0, 200), Color.FromArgb(180, 60, 240)},
                    New Color() {Color.FromArgb(0, 180, 240), Color.FromArgb(60, 220, 255)},
                    New Color() {Color.FromArgb(40, 240, 180), Color.FromArgb(100, 255, 220)}}
            Case Else ' base / classic — procedurally generated from the current game seed
                Return GenerateProceduralBrickPalette()
        End Select
    End Function

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

        Dim basePalette = GetBrickPalette()
        Dim palette = If(_colorblindMode, _colorblindColors, basePalette)
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
                        ' Earn coins: 1 per brick + 1 extra per active combo level
                        Dim coinsEarned = COIN_PER_BRICK + Math.Max(0, Math.Min(_combo, 8) - 1)
                        _coinBalance += coinsEarned
                        _coinsEarnedThisSession += coinsEarned
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
                SaveStore()
                LogEvent("GameOver", $"player={_playerName} score={_score} level={_level}")
                _nameInput = ""
                _highScoreSaved = False
                _pendingHighScore = True
                _highScoreDelayFrames = 90
                _state = GameState.GameOver
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
            _coinBalance += COIN_LEVEL_BONUS
            _coinsEarnedThisSession += COIN_LEVEL_BONUS
            LogEvent("LevelComplete", $"player={_playerName} level={_level} score={_score}")
            SaveStore()
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
                pu.Symbol = If(_colorblindMode, "BIG", "+")
            Case 1
                pu.PType = PowerUpType.RedBallShrink
                pu.Symbol = If(_colorblindMode, "1UP", ChrW(&H2665))
            Case 2, 3, 4
                pu.PType = PowerUpType.GreenMultiBall
                pu.Symbol = "x3"
            Case 5
                pu.PType = PowerUpType.YellowBallShrink
                pu.Symbol = If(_colorblindMode, "SML", "-")
            Case 6
                pu.PType = PowerUpType.PurplePaddleMega
                pu.Symbol = If(_colorblindMode, "PAD", "x3")
            Case 7
                pu.PType = PowerUpType.OrangeBallSlow
                pu.Symbol = If(_colorblindMode, "SLW", "-")
            Case Else
                pu.PType = PowerUpType.PinkBallFast
                pu.Symbol = If(_colorblindMode, "FST", "+")
        End Select
        ' Colorblind mode overrides pack color for accessibility
        If _colorblindMode Then
            Select Case pu.PType
                Case PowerUpType.BlueBallGrow    : pu.Color1 = Color.FromArgb(0, 114, 178)
                Case PowerUpType.RedBallShrink   : pu.Color1 = Color.FromArgb(213, 94, 0)
                Case PowerUpType.GreenMultiBall  : pu.Color1 = Color.FromArgb(240, 228, 66)
                Case PowerUpType.YellowBallShrink: pu.Color1 = Color.FromArgb(86, 180, 233)
                Case PowerUpType.PurplePaddleMega: pu.Color1 = Color.FromArgb(148, 0, 211)
                Case PowerUpType.OrangeBallSlow  : pu.Color1 = Color.FromArgb(230, 159, 0)
                Case Else                        : pu.Color1 = Color.FromArgb(204, 121, 167)
            End Select
        Else
            pu.Color1 = GetBonusPackColor(pu.PType)
        End If
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

    ' ── Per-player profile helpers ────────────────────────────────────────────

    Private Sub SetPlayerProfile(name As String)
        _playerName = name.Trim()
        ' Dev mode: password grants unlimited store coins
        _devMode = String.Equals(_playerName, "luffyisking", StringComparison.OrdinalIgnoreCase)
        Dim safeName = String.Join("_", _playerName.Split(Path.GetInvalidFileNameChars()))
        _storeSavePath = Path.Combine(_playersDir, safeName & ".json")
        ' Reset economy state before loading so values don't bleed between profiles
        _coinBalance = 0
        _coinsEarnedThisSession = 0
        _activeBallSkin = "base"
        _activeBrickPalette = "base"
        _activeBonusPack = "base"
        _ownedItems.Clear()
        InitStoreItems()   ' re-seeds base items as owned
        LoadStore()
        If _devMode Then _coinBalance = 999999
    End Sub

    Private Function GetExistingPlayerNames() As List(Of String)
        Dim names As New List(Of String)
        Try
            If Not Directory.Exists(_playersDir) Then Return names
            For Each f In Directory.GetFiles(_playersDir, "*.json")
                Try
                    Dim json = File.ReadAllText(f)
                    Dim data = JsonSerializer.Deserialize(Of StoreSaveData)(json)
                    Dim nm As String
                    If data IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(data.PlayerName) Then
                        nm = data.PlayerName
                    Else
                        nm = Path.GetFileNameWithoutExtension(f)
                    End If
                    ' Never expose the dev-mode password as a selectable name
                    If Not String.Equals(nm, "luffyisking", StringComparison.OrdinalIgnoreCase) Then
                        names.Add(nm)
                    End If
                Catch
                    Dim fallback = Path.GetFileNameWithoutExtension(f)
                    If Not String.Equals(fallback, "luffyisking", StringComparison.OrdinalIgnoreCase) Then
                        names.Add(fallback)
                    End If
                End Try
            Next
        Catch
        End Try
        Return names
    End Function

    ' ── Store / Economy persistence ───────────────────────────────────────────

    Private Sub InitStoreItems()
        _storeItems.Clear()

        ' ── Balls ──────────────────────────────────────────────────────────────
        _storeItems.Add(New StoreItem With {
            .Id = "base", .Name = "Classic Ball", .Description = "The original white ball.",
            .Price = 0, .Category = StoreCategory.Balls, .IsBase = True})
        _storeItems.Add(New StoreItem With {
            .Id = "fire", .Name = "Fire Ball", .Description = "Blazing orange trail.",
            .Price = 150, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "ice", .Name = "Ice Ball", .Description = "Frosty cyan glow.",
            .Price = 150, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "plasma", .Name = "Plasma Ball", .Description = "Electric violet pulse.",
            .Price = 250, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "gold", .Name = "Gold Ball", .Description = "Shining gold sphere.",
            .Price = 400, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "rainbow", .Name = "Rainbow Ball", .Description = "Cycles through all colors.",
            .Price = 600, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "lava", .Name = "Lava Ball", .Description = "Scorching red-orange magma.",
            .Price = 200, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "void", .Name = "Void Ball", .Description = "Dark matter core.",
            .Price = 300, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "toxic", .Name = "Toxic Ball", .Description = "Radioactive green slime.",
            .Price = 200, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "neon", .Name = "Neon Ball", .Description = "Blinding electric cyan.",
            .Price = 250, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "crystal", .Name = "Crystal Ball", .Description = "Clear prismatic glass.",
            .Price = 350, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "shadow", .Name = "Shadow Ball", .Description = "Deep purple darkness.",
            .Price = 300, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "sakura", .Name = "Sakura Ball", .Description = "Soft cherry-blossom pink.",
            .Price = 250, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "copper", .Name = "Copper Ball", .Description = "Warm oxidised metal.",
            .Price = 350, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "ocean", .Name = "Ocean Ball", .Description = "Deep sea teal shimmer.",
            .Price = 250, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "star", .Name = "Star Ball", .Description = "Stellar white-gold burst.",
            .Price = 500, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "obsidian", .Name = "Obsidian Ball", .Description = "Volcanic black glass.",
            .Price = 450, .Category = StoreCategory.Balls, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "aurora", .Name = "Aurora Ball", .Description = "Northern-lights gradient.",
            .Price = 700, .Category = StoreCategory.Balls, .IsBase = False})

        ' ── Brick Palettes ─────────────────────────────────────────────────────
        _storeItems.Add(New StoreItem With {
            .Id = "base", .Name = "Classic Bricks", .Description = "Default rainbow palette.",
            .Price = 0, .Category = StoreCategory.Bricks, .IsBase = True})
        _storeItems.Add(New StoreItem With {
            .Id = "neon", .Name = "Neon Bricks", .Description = "Bright neon grid.",
            .Price = 200, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "pastel", .Name = "Pastel Bricks", .Description = "Soft pastel tones.",
            .Price = 200, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "metal", .Name = "Metal Bricks", .Description = "Sleek steel panels.",
            .Price = 350, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "candy", .Name = "Candy Bricks", .Description = "Sweet candy-coated blocks.",
            .Price = 350, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "space", .Name = "Space Bricks", .Description = "Deep-space dark theme.",
            .Price = 500, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "lava", .Name = "Lava Bricks", .Description = "Molten red-orange heat.",
            .Price = 250, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "ice", .Name = "Ice Bricks", .Description = "Frozen arctic blue tones.",
            .Price = 250, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "toxic", .Name = "Toxic Bricks", .Description = "Radioactive green glow.",
            .Price = 300, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "sunset", .Name = "Sunset Bricks", .Description = "Warm orange-pink horizon.",
            .Price = 300, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "forest", .Name = "Forest Bricks", .Description = "Deep earthy greens.",
            .Price = 300, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "ocean", .Name = "Ocean Bricks", .Description = "Blue-teal sea shades.",
            .Price = 300, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "galaxy", .Name = "Galaxy Bricks", .Description = "Violet-pink nebula palette.",
            .Price = 450, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "gold", .Name = "Gold Bricks", .Description = "Shining amber treasure.",
            .Price = 450, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "obsidian", .Name = "Obsidian Bricks", .Description = "Black volcanic stone.",
            .Price = 500, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "sakura", .Name = "Sakura Bricks", .Description = "Japanese cherry-blossom.",
            .Price = 350, .Category = StoreCategory.Bricks, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "aurora", .Name = "Aurora Bricks", .Description = "Northern-lights shimmer.",
            .Price = 600, .Category = StoreCategory.Bricks, .IsBase = False})

        ' ── Bonus Packs ────────────────────────────────────────────────────────
        ' Each pack re-themes every power-up drop: colors, shapes, and icon symbols.
        _storeItems.Add(New StoreItem With {
            .Id = "base", .Name = "Classic Bonuses", .Description = "Original rainbow power-ups.",
            .Price = 0, .Category = StoreCategory.Bonuses, .IsBase = True})
        _storeItems.Add(New StoreItem With {
            .Id = "ninja", .Name = "Ninja Pack", .Description = "Shuriken, smoke bombs & katana flashes.",
            .Price = 200, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "space", .Name = "Space Odyssey Pack", .Description = "Planets, rockets & black holes.",
            .Price = 300, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "candy", .Name = "Candy Land Pack", .Description = "Lollipops, gummies & jellybean bursts.",
            .Price = 200, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "cyber", .Name = "Cyberpunk Pack", .Description = "Circuit nodes, data streams & neon glitch.",
            .Price = 400, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "medieval", .Name = "Medieval Pack", .Description = "Swords, shields, potions & crowns.",
            .Price = 350, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "ocean", .Name = "Ocean Deep Pack", .Description = "Bubbles, fish, anchors & waves.",
            .Price = 250, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "retro", .Name = "Retro Arcade Pack", .Description = "8-bit stars, coins & power pills.",
            .Price = 300, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "magic", .Name = "Wizard Magic Pack", .Description = "Wands, stars, runes & spell orbs.",
            .Price = 450, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "dragon", .Name = "Dragon Fire Pack", .Description = "Claws, flames, scales & dragon eggs.",
            .Price = 500, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "sakura", .Name = "Sakura Spring Pack", .Description = "Petals, lanterns, fans & blossom rings.",
            .Price = 350, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "robot", .Name = "Robot Wars Pack", .Description = "Gears, bolts, laser eyes & CPU chips.",
            .Price = 400, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "pirate", .Name = "Pirate Seas Pack", .Description = "Skulls, treasure chests, cannons & hooks.",
            .Price = 300, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "galaxy", .Name = "Galaxy Core Pack", .Description = "Supernovas, comets, wormholes & nebulae.",
            .Price = 600, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "festival", .Name = "Festival Pack", .Description = "Fireworks, confetti, lanterns & party stars.",
            .Price = 250, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "horror", .Name = "Halloween Horror Pack", .Description = "Skulls, bats, pumpkins & ghost trails.",
            .Price = 350, .Category = StoreCategory.Bonuses, .IsBase = False})
        _storeItems.Add(New StoreItem With {
            .Id = "golden", .Name = "Golden Age Pack", .Description = "Coins, laurels, crowns & golden rings.",
            .Price = 700, .Category = StoreCategory.Bonuses, .IsBase = False})

        ' Base items are always owned
        For Each item In _storeItems
            If item.IsBase Then _ownedItems.Add(item.Category.ToString() & "_" & item.Id)
        Next
    End Sub

    Private Sub LoadStore()
        Try
            If Not File.Exists(_storeSavePath) Then Return
            Dim json = File.ReadAllText(_storeSavePath)
            Dim data = JsonSerializer.Deserialize(Of StoreSaveData)(json)
            If data Is Nothing Then Return
            _coinBalance = data.CoinBalance
            If data.OwnedItems IsNot Nothing Then
                For Each key In data.OwnedItems
                    _ownedItems.Add(key)
                Next
            End If
            If Not String.IsNullOrWhiteSpace(data.ActiveBallSkin) Then _activeBallSkin = data.ActiveBallSkin
            If Not String.IsNullOrWhiteSpace(data.ActiveBrickPalette) Then _activeBrickPalette = data.ActiveBrickPalette
            If Not String.IsNullOrWhiteSpace(data.ActiveBonusPack) Then _activeBonusPack = data.ActiveBonusPack
        Catch
        End Try
    End Sub

    Private Sub SaveStore()
        Try
            Dim dir = Path.GetDirectoryName(_storeSavePath)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If
            Dim data As New StoreSaveData With {
                .PlayerName = _playerName,
                .CoinBalance = _coinBalance,
                .OwnedItems = _ownedItems.ToList(),
                .ActiveBallSkin = _activeBallSkin,
                .ActiveBrickPalette = _activeBrickPalette,
                .ActiveBonusPack = _activeBonusPack
            }
            File.WriteAllText(_storeSavePath, JsonSerializer.Serialize(data))
            LogEvent("ProfileSaved", $"player={_playerName} coins={_coinBalance}")
            SyncProfileAsync()
        Catch
        End Try
    End Sub

    Private Function IsOwned(category As StoreCategory, id As String) As Boolean
        Return _ownedItems.Contains(category.ToString() & "_" & id)
    End Function

    Private Sub PurchaseItem(item As StoreItem)
        Dim key = item.Category.ToString() & "_" & item.Id
        If IsOwned(item.Category, item.Id) Then Return
        If Not _devMode AndAlso _coinBalance < item.Price Then Return
        If Not _devMode Then _coinBalance -= item.Price
        _ownedItems.Add(key)
        LogEvent("ItemPurchased", $"player={_playerName} item={key} cost={item.Price}")
        SaveStore()
    End Sub

    Private Sub EquipItem(item As StoreItem)
        If Not IsOwned(item.Category, item.Id) Then Return
        Select Case item.Category
            Case StoreCategory.Balls
                _activeBallSkin = item.Id
            Case StoreCategory.Bricks
                _activeBrickPalette = item.Id
            Case StoreCategory.Bonuses
                _activeBonusPack = item.Id
        End Select
        LogEvent("ItemEquipped", $"player={_playerName} item={item.Category}_{item.Id}")
        SaveStore()
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

    Private Sub DrawNameEntry(g As Graphics)
        Dim pw = 560, ph = 420
        Dim px = CSng((LOGICAL_WIDTH - pw) / 2), py = CSng((LOGICAL_HEIGHT - ph) / 2)
        Using br As New SolidBrush(Color.FromArgb(245, 12, 12, 35))
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.FillPath(br, rr)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(120, 100, 180, 255), 2)
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.DrawPath(pen, rr)
            End Using
        End Using

        DrawCenteredText(g, "BRICK BLAST", _fnt30b, Color.FromArgb(255, 200, 80), py + 20)
        DrawCenteredText(g, "Enter your player name", _fnt16r, Color.FromArgb(180, 200, 255), py + 90)

        ' Input box
        Dim boxW = 340, boxH = 48
        Dim bx = CSng((LOGICAL_WIDTH - boxW) / 2), bby = py + 145
        Using br As New SolidBrush(Color.FromArgb(60, 255, 255, 255))
            Using rr = RoundedRect(New RectangleF(bx, bby, boxW, boxH), 8)
                g.FillPath(br, rr)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(180, 100, 180, 255), 2)
            Using rr = RoundedRect(New RectangleF(bx, bby, boxW, boxH), 8)
                g.DrawPath(pen, rr)
            End Using
        End Using
        Dim cursor = If(_frameCount Mod 60 < 30, "|", " ")
        Dim inputDisplay = If(_nameEntryInput.Length > 0, _nameEntryInput, "")
        Dim fullText = inputDisplay & cursor
        Dim textSz = g.MeasureString(fullText, _fnt18b)
        Dim textX = CSng(bx + (boxW - textSz.Width) / 2)
        Dim textY = CSng(bby + (boxH - textSz.Height) / 2)
        Using tbr As New SolidBrush(Color.White)
            g.DrawString(fullText, _fnt18b, tbr, textX, textY)
        End Using

        DrawCenteredText(g, "Press ENTER to continue", _fnt12r, Color.FromArgb(140, 160, 200), py + 215)

        ' Returning player list
        Dim existing = GetExistingPlayerNames()
        If existing.Count > 0 Then
            DrawCenteredText(g, "— Returning players —", _fnt11r, Color.FromArgb(120, 140, 180), py + 255)
            Dim ry = py + 278.0F
            For Each nm In existing.Take(5)
                Dim highlight = String.Equals(nm, _nameEntryInput, StringComparison.OrdinalIgnoreCase)
                Dim nc = If(highlight, Color.FromArgb(255, 220, 80), Color.FromArgb(180, 190, 210))
                DrawCenteredText(g, nm, _fnt12b, nc, ry)
                ry += 24
            Next
        End If

        DrawCenteredText(g, "New name = new profile   |   Same name = restore saves", _fnt10r, Color.FromArgb(100, 120, 150), py + ph - 28)
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
        DrawCenteredText(g, ChrW(&H25C6) & $"  Press S for STORE  ({_coinBalance} coins)  " & ChrW(&H25C6), _fnt14b, Color.FromArgb(255, 220, 60), 390)
        DrawCenteredText(g, ChrW(&H2699) & "  Press H or O for OPTIONS  |  C for CREDITS  " & ChrW(&H2699), _fnt14b, Color.FromArgb(100, 200, 255), 420)
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
        DrawCenteredText(g, "BrickBlast: Velocity Market  |  v1.0.0", _fnt10r, Color.FromArgb(80, 90, 110), 570)
        DrawCenteredText(g, "[F12] Export marketing assets  |  [C] Credits  |  [S] Store  |  [H] Settings", _fnt10r, Color.FromArgb(55, 70, 90), 588)
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

        ' ── Row 1 (y=12): SCORE left | LEVEL centre | LIVES right ──────────
        Dim starSpr = TryGetSprite("ui/star")
        If starSpr IsNot Nothing Then
            g.DrawImage(starSpr, 15, 11, 20, 20)
        End If
        DrawTextShadow(g, $"SCORE: {_score}", f, Color.White, CSng(If(starSpr IsNot Nothing, 38, 15)), 12)
        DrawCenteredText(g, $"LEVEL {_level}", f, Color.FromArgb(180, 200, 255), 12)
        Dim hSz As Single = 22, hPad As Single = 2
        Dim hX = CSng(LOGICAL_WIDTH - 15 - (hSz + hPad) * _lives)
        For h = 0 To _lives - 1
            DrawHeartShape(g, hX + h * (hSz + hPad), 10.0F, hSz, hSz, Color.FromArgb(255, 80, 100))
        Next

        ' ── Row 2 (y=34): status left | COINS centre | paddle timer right ──
        ' Left status: speed-boost takes priority over ball-size
        If _speedBoost Then
            DrawTextShadow(g, ChrW(&H26A1) & " 2x SPEED", f, Color.FromArgb(255, 255, 80), 15, 34)
        ElseIf _ballRadius <> BALL_RADIUS Then
            DrawTextShadow(g, $"Ball: {_ballRadius}px", f, Color.FromArgb(150, 200, 255), 15, 34)
        End If

        ' Centre: coin balance
        Dim coinText = If(_devMode, $"{ChrW(&H25C6)} DEV", $"{ChrW(&H25C6)} {_coinBalance}")
        Dim coinSz = g.MeasureString(coinText, f)
        Dim coinColor = If(_devMode, Color.FromArgb(100, 255, 100), Color.FromArgb(255, 220, 60))
        DrawTextShadow(g, coinText, f, coinColor, (LOGICAL_WIDTH - coinSz.Width) / 2, 34)

        ' Right: paddle-width timer
        If _paddleWidthTimer > 0 Then
            Dim sec = CInt(Math.Ceiling(_paddleWidthTimer / 60.0))
            Dim pt = $"Paddle: {sec}s"
            Dim psz = g.MeasureString(pt, f)
            DrawTextShadow(g, pt, f, Color.FromArgb(170, 80, 255), LOGICAL_WIDTH - psz.Width - 15, 34)
        End If

        ' ── Separator ───────────────────────────────────────────────────────
        Using pen As New Pen(Color.FromArgb(40, 100, 180, 255), 1)
            g.DrawLine(pen, 0, 50, LOGICAL_WIDTH, 50)
        End Using

        ' ── Sync status — bottom-right corner ───────────────────────────────
        Dim syncLabel = GetSyncLabel()
        Dim syncF = _fnt11r
        Dim syncSz = g.MeasureString(syncLabel, syncF)
        Dim syncColor = If(_syncStatus = "Synced", Color.FromArgb(80, 220, 80),
                        If(_syncStatus = "Syncing", Color.FromArgb(255, 220, 60),
                        If(_syncStatus = "Failed", Color.FromArgb(255, 80, 80),
                        Color.FromArgb(140, 140, 140))))
        DrawTextShadow(g, syncLabel, syncF, syncColor,
                       CSng(LOGICAL_WIDTH - syncSz.Width - 8),
                       CSng(LOGICAL_HEIGHT - syncSz.Height - 6))
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
            ' Determine skin colors first so they can be used in the glow loop below
            Dim ballTop As Color, ballBot As Color, glowBase As Color
            Select Case _activeBallSkin
                Case "fire"
                    ballTop = Color.FromArgb(255, 220, 80) : ballBot = Color.FromArgb(220, 60, 0) : glowBase = Color.FromArgb(255, 140, 20)
                Case "ice"
                    ballTop = Color.FromArgb(200, 240, 255) : ballBot = Color.FromArgb(40, 160, 220) : glowBase = Color.FromArgb(100, 220, 255)
                Case "plasma"
                    ballTop = Color.FromArgb(220, 160, 255) : ballBot = Color.FromArgb(100, 0, 200) : glowBase = Color.FromArgb(180, 80, 255)
                Case "gold"
                    ballTop = Color.FromArgb(255, 245, 120) : ballBot = Color.FromArgb(200, 140, 0) : glowBase = Color.FromArgb(255, 200, 0)
                Case "rainbow"
                    Dim hue = (_frameCount * 3) Mod 360
                    ballTop = ColorFromHSV(hue, 0.9, 1.0) : ballBot = ColorFromHSV((hue + 120) Mod 360, 1.0, 0.8) : glowBase = ballTop
                Case "lava"
                    ballTop = Color.FromArgb(255, 80, 20) : ballBot = Color.FromArgb(180, 20, 0) : glowBase = Color.FromArgb(255, 60, 0)
                Case "void"
                    ballTop = Color.FromArgb(60, 0, 80) : ballBot = Color.FromArgb(10, 0, 20) : glowBase = Color.FromArgb(120, 0, 180)
                Case "toxic"
                    ballTop = Color.FromArgb(160, 255, 60) : ballBot = Color.FromArgb(60, 160, 0) : glowBase = Color.FromArgb(120, 255, 40)
                Case "neon"
                    ballTop = Color.FromArgb(0, 255, 240) : ballBot = Color.FromArgb(0, 160, 200) : glowBase = Color.FromArgb(0, 240, 255)
                Case "crystal"
                    ballTop = Color.FromArgb(220, 240, 255) : ballBot = Color.FromArgb(160, 200, 240) : glowBase = Color.FromArgb(200, 230, 255)
                Case "shadow"
                    ballTop = Color.FromArgb(120, 60, 180) : ballBot = Color.FromArgb(40, 0, 80) : glowBase = Color.FromArgb(100, 40, 160)
                Case "sakura"
                    ballTop = Color.FromArgb(255, 200, 220) : ballBot = Color.FromArgb(220, 100, 140) : glowBase = Color.FromArgb(255, 160, 190)
                Case "copper"
                    ballTop = Color.FromArgb(220, 130, 80) : ballBot = Color.FromArgb(140, 70, 30) : glowBase = Color.FromArgb(200, 110, 60)
                Case "ocean"
                    ballTop = Color.FromArgb(60, 200, 200) : ballBot = Color.FromArgb(0, 100, 140) : glowBase = Color.FromArgb(40, 180, 200)
                Case "star"
                    Dim sh = (_frameCount * 2) Mod 360
                    ballTop = ColorFromHSV(sh, 0.3, 1.0) : ballBot = Color.FromArgb(200, 180, 60) : glowBase = Color.FromArgb(255, 240, 120)
                Case "obsidian"
                    ballTop = Color.FromArgb(60, 50, 70) : ballBot = Color.FromArgb(10, 8, 14) : glowBase = Color.FromArgb(90, 70, 110)
                Case "aurora"
                    Dim ah = (_frameCount * 2 + 60) Mod 360
                    ballTop = ColorFromHSV(ah, 0.8, 0.9) : ballBot = ColorFromHSV((ah + 100) Mod 360, 1.0, 0.7) : glowBase = ballTop
                Case Else ' base
                    ballTop = If(_speedBoost, Color.FromArgb(255, 255, 200), Color.White)
                    ballBot = If(_speedBoost, Color.FromArgb(255, 140, 20), Color.FromArgb(160, 210, 255))
                    glowBase = If(_speedBoost, Color.FromArgb(255, 200, 50), Color.FromArgb(200, 230, 255))
            End Select
            For gs = 20 To 4 Step -4
                Dim al = CInt(20 * (4.0 / gs))
                Dim glowC = Color.FromArgb(al, glowBase)
                Using br As New SolidBrush(glowC)
                    g.FillEllipse(br, CSng(b.X - gs / 2), CSng(b.Y - gs / 2), CSng(gs), CSng(gs))
                End Using
            Next
            Using br As New LinearGradientBrush(New RectangleF(b.X - br2, b.Y - br2, br2 * 2, br2 * 2), ballTop, ballBot, LinearGradientMode.ForwardDiagonal)
                g.FillEllipse(br, b.X - br2, b.Y - br2, br2 * 2, br2 * 2)
            End Using
            Using br As New SolidBrush(Color.FromArgb(180, 255, 255, 255))
                g.FillEllipse(br, b.X - br2 * 0.4F, b.Y - br2 * 0.5F, br2 * 0.6F, br2 * 0.5F)
            End Using
        Next
    End Sub

    Private Sub DrawPowerUps(g As Graphics)
        For Each pu In _powerUps
            If Not pu.Active Then Continue For
            Dim bob = CSng(Math.Sin(_frameCount * 0.1) * 3)
            Dim cy = pu.Y + bob
            Dim sz = CSng(POWERUP_SIZE)
            Dim cx = pu.X

            ' Outer glow
            Using br As New SolidBrush(Color.FromArgb(45, pu.Color1))
                g.FillEllipse(br, cx - sz * 0.7F, cy - sz * 0.7F, sz * 1.4F, sz * 1.4F)
            End Using

            ' Themed body shape (pack-aware: circle, diamond, hex, shield, star, or rounded-square)
            Dim darkC = Color.FromArgb(160, Math.Max(0, pu.Color1.R - 60), Math.Max(0, pu.Color1.G - 60), Math.Max(0, pu.Color1.B - 60))
            DrawBonusBody(g, cx, cy, sz, pu.Color1, darkC)

            ' Shine
            Using br As New SolidBrush(Color.FromArgb(90, 255, 255, 255))
                g.FillEllipse(br, cx - sz * 0.3F, cy - sz * 0.38F, sz * 0.32F, sz * 0.22F)
            End Using

            ' Procedural icon drawn inside the circle
            DrawPowerUpIcon(g, pu.PType, cx, cy, sz * 0.36F, pu.Color1)
        Next
    End Sub

    ' =======================================================================
    ' BONUS PACK THEMING HELPERS
    ' Each bonus pack defines: body color per slot, icon glyph, and body shape.
    ' Body shapes: 0=circle, 1=diamond, 2=hexagon, 3=shield, 4=star, 5=rounded-square
    ' =======================================================================

    ' Returns the themed fill color for a given power-up slot under the active bonus pack.
    Private Function GetBonusPackColor(pType As PowerUpType) As Color
        Select Case _activeBonusPack
            Case "ninja"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(30, 30, 30)     ' ink black — shuriken grow
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(200, 20, 20)    ' blood red — life scroll
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(80, 180, 60)    ' forest green — clone smoke
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(200, 200, 0)    ' golden shuriken
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(50, 50, 100)    ' midnight — ninjato
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(180, 90, 0)     ' smoke bomb orange
                    Case Else                        : Return Color.FromArgb(220, 50, 50)    ' crimson — speed dash
                End Select

            Case "space"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(30, 80, 200)    ' rocket thruster blue
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(200, 50, 200)   ' alien life nebula
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(0, 200, 180)    ' teal warp clones
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(255, 220, 40)   ' sun shrink
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(100, 0, 200)    ' gravity field
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(180, 100, 0)    ' asteroid belt
                    Case Else                        : Return Color.FromArgb(0, 180, 255)    ' hyperdrive cyan
                End Select

            Case "candy"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(50, 150, 255)   ' blueberry gum
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(255, 60, 120)   ' strawberry heart
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(100, 220, 80)   ' lime gummy x3
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(255, 230, 50)   ' lemon drop
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(180, 80, 220)   ' grape jawbreaker
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(255, 140, 30)   ' orange creamsicle
                    Case Else                        : Return Color.FromArgb(255, 120, 180)  ' pink cotton candy
                End Select

            Case "cyber"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(0, 200, 255)    ' data stream cyan
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(255, 0, 80)     ' glitch red
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(0, 255, 120)    ' matrix green
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(220, 255, 0)    ' neon yellow
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(160, 0, 255)    ' circuit violet
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(255, 100, 0)    ' firewall orange
                    Case Else                        : Return Color.FromArgb(0, 255, 200)    ' teal uplink
                End Select

            Case "medieval"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(60, 80, 180)    ' royal cobalt
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(180, 20, 20)    ' crimson potion
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(20, 140, 60)    ' emerald clone
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(200, 180, 30)   ' golden shrink rune
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(80, 20, 120)    ' enchanted shield
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(160, 100, 20)   ' bronze slow rune
                    Case Else                        : Return Color.FromArgb(200, 160, 60)   ' gilded speed crown
                End Select

            Case "ocean"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(0, 120, 200)    ' deep sea
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(200, 60, 80)    ' coral heart
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(0, 180, 140)    ' kelp forest
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(240, 210, 60)   ' sand dollar
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(80, 0, 160)     ' ink octopus
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(200, 120, 0)    ' starfish slow
                    Case Else                        : Return Color.FromArgb(0, 200, 220)    ' surf turquoise
                End Select

            Case "retro"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(0, 80, 200)     ' 8-bit blue
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(200, 0, 0)      ' pixel heart red
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(0, 160, 0)      ' game boy green
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(255, 200, 0)    ' gold coin
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(100, 0, 160)    ' power pill purple
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(200, 80, 0)     ' fire flower
                    Case Else                        : Return Color.FromArgb(255, 60, 100)   ' extra life red
                End Select

            Case "magic"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(40, 60, 200)    ' arcane blue
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(160, 0, 180)    ' transmute violet
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(0, 180, 80)     ' summoning green
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(220, 180, 0)    ' alchemy gold
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(100, 0, 220)    ' enchantment purple
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(180, 80, 0)     ' time slow amber
                    Case Else                        : Return Color.FromArgb(255, 100, 200)  ' haste pink
                End Select

            Case "dragon"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(0, 80, 160)     ' ice dragon
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(200, 30, 0)     ' flame scale
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(20, 160, 40)    ' forest wyvern
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(200, 160, 0)    ' gold hoard
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(80, 0, 140)     ' void dragon
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(180, 60, 0)     ' lava breath
                    Case Else                        : Return Color.FromArgb(220, 80, 200)   ' chaos wing
                End Select

            Case "sakura"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(120, 160, 220)  ' clear sky
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(220, 80, 120)   ' blossom life
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(100, 180, 120)  ' bamboo
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(240, 210, 150)  ' rice paper
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(160, 100, 200)  ' wisteria
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(200, 130, 60)   ' maple amber
                    Case Else                        : Return Color.FromArgb(255, 160, 190)  ' petal pink
                End Select

            Case "robot"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(40, 120, 200)   ' servo blue
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(200, 40, 40)    ' laser eye red
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(40, 200, 100)   ' replication green
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(220, 200, 40)   ' cpu gold
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(80, 40, 180)    ' power core
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(200, 100, 20)   ' gear oil
                    Case Else                        : Return Color.FromArgb(0, 220, 220)    ' turbo teal
                End Select

            Case "pirate"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(20, 80, 160)    ' ocean deep
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(180, 20, 20)    ' skull red
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(0, 140, 80)     ' sea foam
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(200, 160, 0)    ' doubloon gold
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(60, 0, 100)     ' jolly roger purple
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(160, 80, 0)     ' rum barrel
                    Case Else                        : Return Color.FromArgb(120, 60, 0)     ' ship timber
                End Select

            Case "galaxy"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(0, 60, 200)     ' blue supergiant
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(200, 0, 100)    ' pulsar magenta
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(0, 200, 160)    ' nebula teal
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(255, 220, 60)   ' solar flare
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(100, 0, 200)    ' wormhole violet
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(180, 80, 0)     ' comet trail
                    Case Else                        : Return Color.FromArgb(0, 220, 255)    ' hyperspace
                End Select

            Case "festival"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(40, 120, 220)   ' sky firework
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(220, 40, 80)    ' confetti red
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(40, 200, 80)    ' party green
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(255, 220, 40)   ' golden sparkler
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(160, 60, 220)   ' violet burst
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(220, 120, 20)   ' lantern orange
                    Case Else                        : Return Color.FromArgb(255, 80, 160)   ' ribbon pink
                End Select

            Case "horror"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(0, 40, 80)      ' midnight
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(160, 0, 0)      ' blood
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(60, 140, 0)     ' slime green
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(180, 160, 0)    ' ghost yellow
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(60, 0, 100)     ' crypt purple
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(160, 60, 0)     ' pumpkin
                    Case Else                        : Return Color.FromArgb(100, 100, 100)  ' ash grey
                End Select

            Case "golden"
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(200, 160, 0)    ' amber
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(220, 120, 0)    ' bronze
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(180, 180, 0)    ' gold-green
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(255, 230, 60)   ' bright gold
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(180, 140, 0)    ' crown gold
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(200, 100, 0)    ' rose gold
                    Case Else                        : Return Color.FromArgb(240, 200, 80)   ' gilded
                End Select

            Case Else ' base — original colors
                Select Case pType
                    Case PowerUpType.BlueBallGrow    : Return Color.FromArgb(50, 120, 255)
                    Case PowerUpType.RedBallShrink   : Return Color.FromArgb(255, 60, 60)
                    Case PowerUpType.GreenMultiBall  : Return Color.FromArgb(50, 220, 100)
                    Case PowerUpType.YellowBallShrink: Return Color.FromArgb(255, 220, 50)
                    Case PowerUpType.PurplePaddleMega: Return Color.FromArgb(170, 80, 255)
                    Case PowerUpType.OrangeBallSlow  : Return Color.FromArgb(255, 150, 60)
                    Case Else                        : Return Color.FromArgb(255, 120, 200)
                End Select
        End Select
    End Function

    ' Returns the container body shape index for the active bonus pack.
    ' 0=circle  1=diamond  2=hexagon  3=shield  4=star  5=rounded-square
    Private Function GetBonusPackBodyShape() As Integer
        Select Case _activeBonusPack
            Case "ninja"    : Return 1   ' diamond — shuriken
            Case "space"    : Return 2   ' hexagon — hull plate
            Case "candy"    : Return 5   ' rounded-square — candy box
            Case "cyber"    : Return 5   ' rounded-square — circuit chip
            Case "medieval" : Return 3   ' shield
            Case "ocean"    : Return 0   ' circle — bubble
            Case "retro"    : Return 5   ' rounded-square — pixel block
            Case "magic"    : Return 4   ' star — spell orb
            Case "dragon"   : Return 1   ' diamond — scale facet
            Case "sakura"   : Return 0   ' circle — petal round
            Case "robot"    : Return 5   ' rounded-square — chassis panel
            Case "pirate"   : Return 3   ' shield — cannon ball
            Case "galaxy"   : Return 2   ' hexagon — space station
            Case "festival" : Return 4   ' star — firework burst
            Case "horror"   : Return 1   ' diamond — coffin lid
            Case "golden"   : Return 4   ' star — laurel star
            Case Else       : Return 0   ' base — circle
        End Select
    End Function

    ' Draws the bonus-pack container body (replaces the plain ellipse in DrawPowerUps).
    Private Sub DrawBonusBody(g As Graphics, cx As Single, cy As Single, sz As Single, fillColor As Color, darkColor As Color)
        Dim shape = GetBonusPackBodyShape()
        Dim hs = sz / 2
        Using br As New LinearGradientBrush(
                New RectangleF(cx - hs, cy - hs, sz, sz),
                Color.FromArgb(230, fillColor),
                Color.FromArgb(160, darkColor),
                LinearGradientMode.ForwardDiagonal)

            Select Case shape
                Case 1 ' diamond
                    Dim dpts() As PointF = {
                        New PointF(cx, cy - hs),
                        New PointF(cx + hs, cy),
                        New PointF(cx, cy + hs),
                        New PointF(cx - hs, cy)
                    }
                    g.FillPolygon(br, dpts)

                Case 2 ' hexagon
                    Dim hpts(5) As PointF
                    For i = 0 To 5
                        Dim ang = (i * 60 - 30) * Math.PI / 180.0
                        hpts(i) = New PointF(cx + CSng(Math.Cos(ang) * hs), cy + CSng(Math.Sin(ang) * hs))
                    Next
                    g.FillPolygon(br, hpts)

                Case 3 ' shield
                    Dim spts() As PointF = {
                        New PointF(cx - hs, cy - hs),
                        New PointF(cx + hs, cy - hs),
                        New PointF(cx + hs, cy + hs * 0.3F),
                        New PointF(cx, cy + hs),
                        New PointF(cx - hs, cy + hs * 0.3F)
                    }
                    g.FillPolygon(br, spts)

                Case 4 ' star (5-point)
                    Dim stpts(9) As PointF
                    For i = 0 To 9
                        Dim ang = (i * 36 - 90) * Math.PI / 180.0
                        Dim rad = If(i Mod 2 = 0, hs, hs * 0.42F)
                        stpts(i) = New PointF(cx + CSng(Math.Cos(ang) * rad), cy + CSng(Math.Sin(ang) * rad))
                    Next
                    g.FillPolygon(br, stpts)

                Case 5 ' rounded-square
                    Using rr = RoundedRect(New RectangleF(cx - hs, cy - hs, sz, sz), CInt(hs * 0.28F))
                        g.FillPath(br, rr)
                    End Using

                Case Else ' circle
                    g.FillEllipse(br, cx - hs, cy - hs, sz, sz)
            End Select
        End Using
    End Sub

    ' Draws a simple GDI+ icon for each power-up type — no external assets required.
    ' Routes to pack-specific themed artwork when a bonus pack is active.
    Private Sub DrawPowerUpIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, baseColor As Color)
        Using brW As New SolidBrush(Color.FromArgb(230, 255, 255, 255))
            Select Case _activeBonusPack
                Case "ninja"
                    DrawNinjaIcon(g, pType, cx, cy, r, brW)
                Case "space"
                    DrawSpaceIcon(g, pType, cx, cy, r, brW)
                Case "candy"
                    DrawCandyIcon(g, pType, cx, cy, r, brW)
                Case "cyber"
                    DrawCyberIcon(g, pType, cx, cy, r, brW)
                Case "medieval"
                    DrawMedievalIcon(g, pType, cx, cy, r, brW)
                Case "ocean"
                    DrawOceanIcon(g, pType, cx, cy, r, brW)
                Case "retro"
                    DrawRetroIcon(g, pType, cx, cy, r, brW)
                Case "magic"
                    DrawMagicIcon(g, pType, cx, cy, r, brW)
                Case "dragon"
                    DrawDragonIcon(g, pType, cx, cy, r, brW)
                Case "sakura"
                    DrawSakuraIcon(g, pType, cx, cy, r, brW)
                Case "robot"
                    DrawRobotIcon(g, pType, cx, cy, r, brW)
                Case "pirate"
                    DrawPirateIcon(g, pType, cx, cy, r, brW)
                Case "galaxy"
                    DrawGalaxyIcon(g, pType, cx, cy, r, brW)
                Case "festival"
                    DrawFestivalIcon(g, pType, cx, cy, r, brW)
                Case "horror"
                    DrawHorrorIcon(g, pType, cx, cy, r, brW)
                Case "golden"
                    DrawGoldenIcon(g, pType, cx, cy, r, brW)
                Case Else
                    DrawBaseIcon(g, pType, cx, cy, r, brW)
            End Select
        End Using

        If _colorblindMode Then
            Dim lbl = GetPowerUpCBLabel(pType)
            Dim ts = g.MeasureString(lbl, _fnt8b)
            Using brB As New SolidBrush(Color.FromArgb(220, 20, 20, 20))
                g.DrawString(lbl, _fnt8b, brB, cx - ts.Width / 2 + 1, cy + r * 0.55F + 1)
            End Using
            Using brLbl As New SolidBrush(Color.White)
                g.DrawString(lbl, _fnt8b, brLbl, cx - ts.Width / 2, cy + r * 0.55F)
            End Using
        End If
    End Sub

    Private Sub DrawBaseIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow
                Dim t = r * 0.28F
                g.FillRectangle(brW, cx - t / 2, cy - r, t, r * 2)
                g.FillRectangle(brW, cx - r, cy - t / 2, r * 2, t)
            Case PowerUpType.RedBallShrink
                DrawHeartShape(g, cx - r, cy - r * 0.9F, r * 2, r * 2, Color.FromArgb(230, 255, 120, 140))
            Case PowerUpType.GreenMultiBall
                Dim ts = g.MeasureString("x3", _fnt11b)
                g.DrawString("x3", _fnt11b, brW, cx - ts.Width / 2, cy - ts.Height / 2)
            Case PowerUpType.YellowBallShrink
                g.FillRectangle(brW, cx - r, cy - r * 0.14F, r * 2, r * 0.28F)
            Case PowerUpType.PurplePaddleMega
                Dim bw = r * 1.8F, bh = r * 0.35F
                Using rr = RoundedRect(New RectangleF(cx - bw / 2, cy - bh / 2, bw, bh), 3)
                    g.FillPath(brW, rr)
                End Using
            Case PowerUpType.OrangeBallSlow
                Dim pts() As PointF = {New PointF(cx, cy + r), New PointF(cx - r * 0.7F, cy - r * 0.3F), New PointF(cx + r * 0.7F, cy - r * 0.3F)}
                g.FillPolygon(brW, pts)
            Case PowerUpType.PinkBallFast
                Dim pts2() As PointF = {New PointF(cx, cy - r), New PointF(cx - r * 0.7F, cy + r * 0.3F), New PointF(cx + r * 0.7F, cy + r * 0.3F)}
                g.FillPolygon(brW, pts2)
        End Select
    End Sub

    ' ---- Pack-specific icon painters ----

    Private Sub DrawNinjaIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' shuriken (4-blade)
                For angle As Integer = 0 To 315 Step 90
                    Dim a = angle * Math.PI / 180.0
                    Dim pts() As PointF = {
                        New PointF(cx, cy),
                        New PointF(cx + CSng(Math.Cos(a) * r), cy + CSng(Math.Sin(a) * r)),
                        New PointF(cx + CSng(Math.Cos(a + Math.PI / 4) * r * 0.5F), cy + CSng(Math.Sin(a + Math.PI / 4) * r * 0.5F))
                    }
                    g.FillPolygon(brW, pts)
                Next
            Case PowerUpType.RedBallShrink ' scroll
                Using pen As New Pen(brW.Color, r * 0.22F)
                    g.DrawLine(pen, cx - r * 0.7F, cy, cx + r * 0.7F, cy)
                    g.DrawLine(pen, cx - r * 0.7F, cy - r * 0.35F, cx + r * 0.7F, cy - r * 0.35F)
                    g.DrawLine(pen, cx - r * 0.7F, cy + r * 0.35F, cx + r * 0.7F, cy + r * 0.35F)
                End Using
            Case PowerUpType.GreenMultiBall ' smoke bomb circles
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.7F
                    g.FillEllipse(brW, cx + ox - r * 0.28F, cy - r * 0.28F, r * 0.56F, r * 0.56F)
                Next
            Case PowerUpType.YellowBallShrink ' kunai blade
                Dim kpts() As PointF = {
                    New PointF(cx, cy - r),
                    New PointF(cx + r * 0.2F, cy + r * 0.4F),
                    New PointF(cx, cy + r * 0.2F),
                    New PointF(cx - r * 0.2F, cy + r * 0.4F)
                }
                g.FillPolygon(brW, kpts)
            Case PowerUpType.PurplePaddleMega ' ninjato (long rectangle)
                Dim bw = r * 0.22F, bh = r * 1.8F
                g.FillRectangle(brW, cx - bw / 2, cy - bh / 2, bw, bh)
            Case PowerUpType.OrangeBallSlow ' smoke cloud circles
                Using sbr As New SolidBrush(Color.FromArgb(160, 255, 255, 255))
                    g.FillEllipse(sbr, cx - r * 0.9F, cy - r * 0.4F, r * 1.0F, r * 0.8F)
                    g.FillEllipse(sbr, cx - r * 0.1F, cy - r * 0.6F, r * 1.0F, r * 0.9F)
                    g.FillEllipse(sbr, cx - r * 0.4F, cy + r * 0.0F, r * 0.7F, r * 0.6F)
                End Using
            Case PowerUpType.PinkBallFast ' dash lines
                Using pen As New Pen(brW.Color, r * 0.18F)
                    For i = -1 To 1
                        g.DrawLine(pen, cx + i * r * 0.4F - r * 0.4F, cy + i * r * 0.4F, cx + i * r * 0.4F + r * 0.4F, cy + i * r * 0.4F - r * 0.5F)
                    Next
                End Using
        End Select
    End Sub

    Private Sub DrawSpaceIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' rocket
                Dim rpts() As PointF = {New PointF(cx, cy - r), New PointF(cx + r * 0.4F, cy + r * 0.5F), New PointF(cx - r * 0.4F, cy + r * 0.5F)}
                g.FillPolygon(brW, rpts)
                Using pen As New Pen(Color.FromArgb(200, 255, 160, 0), r * 0.2F)
                    g.DrawLine(pen, cx, cy + r * 0.5F, cx, cy + r)
                End Using
            Case PowerUpType.RedBallShrink ' planet with ring
                g.FillEllipse(brW, cx - r * 0.55F, cy - r * 0.55F, r * 1.1F, r * 1.1F)
                Using pen As New Pen(Color.FromArgb(180, 255, 255, 255), r * 0.15F)
                    g.DrawEllipse(pen, cx - r * 0.85F, cy - r * 0.25F, r * 1.7F, r * 0.5F)
                End Using
            Case PowerUpType.GreenMultiBall ' three stars
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.7F
                    Dim stpts(7) As PointF
                    For k = 0 To 7
                        Dim ang2 = (k * 45 - 90) * Math.PI / 180.0
                        Dim rad = If(k Mod 2 = 0, r * 0.3F, r * 0.14F)
                        stpts(k) = New PointF(cx + ox + CSng(Math.Cos(ang2) * rad), cy + CSng(Math.Sin(ang2) * rad))
                    Next
                    g.FillPolygon(brW, stpts)
                Next
            Case PowerUpType.YellowBallShrink ' black hole (ring)
                Using pen As New Pen(brW.Color, r * 0.2F)
                    g.DrawEllipse(pen, cx - r * 0.65F, cy - r * 0.65F, r * 1.3F, r * 1.3F)
                End Using
            Case PowerUpType.PurplePaddleMega ' station bar
                g.FillRectangle(brW, cx - r, cy - r * 0.18F, r * 2, r * 0.36F)
                g.FillEllipse(brW, cx - r * 0.22F, cy - r * 0.55F, r * 0.44F, r * 1.1F)
            Case PowerUpType.OrangeBallSlow ' asteroid lumpy circle
                Using pen As New Pen(brW.Color, r * 0.2F)
                    Dim apts(7) As PointF
                    For i = 0 To 7
                        Dim ang3 = i * 45 * Math.PI / 180.0
                        Dim rad = r * (0.5F + CSng(_rng.NextDouble() * 0.25))
                        apts(i) = New PointF(cx + CSng(Math.Cos(ang3) * rad), cy + CSng(Math.Sin(ang3) * rad))
                    Next
                    g.DrawPolygon(pen, apts)
                End Using
            Case PowerUpType.PinkBallFast ' lightning bolt
                Dim lpts() As PointF = {
                    New PointF(cx + r * 0.2F, cy - r),
                    New PointF(cx - r * 0.1F, cy - r * 0.05F),
                    New PointF(cx + r * 0.3F, cy - r * 0.05F),
                    New PointF(cx - r * 0.2F, cy + r)
                }
                g.FillPolygon(brW, lpts)
        End Select
    End Sub

    Private Sub DrawCandyIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' gumball circle + plus
                g.FillEllipse(brW, cx - r * 0.7F, cy - r * 0.7F, r * 1.4F, r * 1.4F)
                Dim t = r * 0.2F
                Using brD As New SolidBrush(Color.FromArgb(200, 50, 150, 255))
                    g.FillRectangle(brD, cx - t / 2, cy - r * 0.55F, t, r * 1.1F)
                    g.FillRectangle(brD, cx - r * 0.55F, cy - t / 2, r * 1.1F, t)
                End Using
            Case PowerUpType.RedBallShrink ' strawberry heart
                DrawHeartShape(g, cx - r, cy - r * 0.9F, r * 2, r * 2, Color.FromArgb(240, 255, 60, 100))
            Case PowerUpType.GreenMultiBall ' three gummies
                Dim cc() As Color = {Color.FromArgb(200, 100, 255, 80), Color.FromArgb(200, 255, 80, 120), Color.FromArgb(200, 80, 200, 255)}
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.72F
                    Using gbr As New SolidBrush(cc(i))
                        g.FillEllipse(gbr, cx + ox - r * 0.3F, cy - r * 0.3F, r * 0.6F, r * 0.6F)
                    End Using
                Next
            Case PowerUpType.YellowBallShrink ' lollipop
                Using pen As New Pen(Color.FromArgb(200, 180, 100, 0), r * 0.18F)
                    g.DrawLine(pen, cx, cy - r * 0.3F, cx, cy + r)
                End Using
                g.FillEllipse(brW, cx - r * 0.5F, cy - r, r, r)
            Case PowerUpType.PurplePaddleMega ' candy bar rectangle
                Using rr = RoundedRect(New RectangleF(cx - r, cy - r * 0.32F, r * 2, r * 0.64F), 4)
                    g.FillPath(brW, rr)
                End Using
                Using pen As New Pen(Color.FromArgb(160, 200, 80, 200), r * 0.12F)
                    g.DrawLine(pen, cx - r * 0.35F, cy - r * 0.32F, cx - r * 0.35F, cy + r * 0.32F)
                    g.DrawLine(pen, cx + r * 0.35F, cy - r * 0.32F, cx + r * 0.35F, cy + r * 0.32F)
                End Using
            Case PowerUpType.OrangeBallSlow ' swirl — concentric arcs
                For k = 1 To 3
                    Using pen As New Pen(Color.FromArgb(200 - k * 40, 255, 255, 255), r * 0.15F)
                        g.DrawArc(pen, cx - k * r * 0.3F, cy - k * r * 0.3F, k * r * 0.6F, k * r * 0.6F, 0, 270)
                    End Using
                Next
            Case PowerUpType.PinkBallFast ' star sprinkle
                Dim stpts2(9) As PointF
                For i = 0 To 9
                    Dim ang4 = (i * 36 - 90) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r * 0.9F, r * 0.38F)
                    stpts2(i) = New PointF(cx + CSng(Math.Cos(ang4) * rad), cy + CSng(Math.Sin(ang4) * rad))
                Next
                g.FillPolygon(brW, stpts2)
        End Select
    End Sub

    Private Sub DrawCyberIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Dim t = r * 0.22F
        Select Case pType
            Case PowerUpType.BlueBallGrow ' circuit cross
                g.FillRectangle(brW, cx - t / 2, cy - r, t, r * 2)
                g.FillRectangle(brW, cx - r, cy - t / 2, r * 2, t)
                g.FillEllipse(brW, cx - t, cy - t, t * 2, t * 2)
            Case PowerUpType.RedBallShrink ' exclamation chip
                g.FillRectangle(brW, cx - t / 2, cy - r, t, r * 1.3F)
                g.FillEllipse(brW, cx - t * 0.7F, cy + r * 0.45F, t * 1.4F, t * 1.4F)
            Case PowerUpType.GreenMultiBall ' three data nodes
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.7F
                    g.FillRectangle(brW, cx + ox - t / 2, cy - r * 0.3F, t, r * 0.6F)
                    g.FillEllipse(brW, cx + ox - t * 0.8F, cy - r * 0.5F, t * 1.6F, t * 1.6F)
                Next
            Case PowerUpType.YellowBallShrink ' minus/delete line
                g.FillRectangle(brW, cx - r, cy - t / 2, r * 2, t)
            Case PowerUpType.PurplePaddleMega ' wide chip
                Using rr = RoundedRect(New RectangleF(cx - r, cy - r * 0.28F, r * 2, r * 0.56F), 3)
                    g.FillPath(brW, rr)
                End Using
                For i = -1 To 1
                    g.FillRectangle(brW, cx + i * r * 0.5F - t / 2, cy + r * 0.28F, t, r * 0.4F)
                Next
            Case PowerUpType.OrangeBallSlow ' wifi/signal arcs
                Using pen As New Pen(brW.Color, t)
                    For k = 1 To 3
                        g.DrawArc(pen, cx - k * r * 0.32F, cy - k * r * 0.32F, k * r * 0.64F, k * r * 0.64F, 200, 140)
                    Next
                End Using
            Case PowerUpType.PinkBallFast ' lightning bolt
                Dim lpts() As PointF = {
                    New PointF(cx + r * 0.2F, cy - r),
                    New PointF(cx - r * 0.15F, cy - r * 0.1F),
                    New PointF(cx + r * 0.3F, cy - r * 0.1F),
                    New PointF(cx - r * 0.25F, cy + r)
                }
                g.FillPolygon(brW, lpts)
        End Select
    End Sub

    Private Sub DrawMedievalIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' sword (vertical + crossguard)
                g.FillRectangle(brW, cx - r * 0.12F, cy - r, r * 0.24F, r * 1.8F)
                g.FillRectangle(brW, cx - r * 0.65F, cy + r * 0.1F, r * 1.3F, r * 0.22F)
            Case PowerUpType.RedBallShrink ' potion bottle
                Using rr = RoundedRect(New RectangleF(cx - r * 0.42F, cy - r * 0.1F, r * 0.84F, r), 5)
                    g.FillPath(brW, rr)
                End Using
                g.FillRectangle(brW, cx - r * 0.2F, cy - r * 0.6F, r * 0.4F, r * 0.55F)
            Case PowerUpType.GreenMultiBall ' three small shields
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.7F
                    Dim spts() As PointF = {
                        New PointF(cx + ox - r * 0.24F, cy - r * 0.4F),
                        New PointF(cx + ox + r * 0.24F, cy - r * 0.4F),
                        New PointF(cx + ox + r * 0.24F, cy + r * 0.1F),
                        New PointF(cx + ox, cy + r * 0.4F),
                        New PointF(cx + ox - r * 0.24F, cy + r * 0.1F)
                    }
                    g.FillPolygon(brW, spts)
                Next
            Case PowerUpType.YellowBallShrink ' rune dash
                g.FillRectangle(brW, cx - r, cy - r * 0.14F, r * 2, r * 0.28F)
                g.FillRectangle(brW, cx - r * 0.14F, cy - r, r * 0.28F, r * 2)
            Case PowerUpType.PurplePaddleMega ' long shield
                Dim spts2() As PointF = {
                    New PointF(cx - r, cy - r * 0.6F),
                    New PointF(cx + r, cy - r * 0.6F),
                    New PointF(cx + r, cy + r * 0.2F),
                    New PointF(cx, cy + r * 0.7F),
                    New PointF(cx - r, cy + r * 0.2F)
                }
                g.FillPolygon(brW, spts2)
            Case PowerUpType.OrangeBallSlow ' hourglass
                Dim hpts() As PointF = {
                    New PointF(cx - r * 0.65F, cy - r), New PointF(cx + r * 0.65F, cy - r),
                    New PointF(cx + r * 0.15F, cy), New PointF(cx + r * 0.65F, cy + r),
                    New PointF(cx - r * 0.65F, cy + r), New PointF(cx - r * 0.15F, cy)
                }
                g.FillPolygon(brW, hpts)
            Case PowerUpType.PinkBallFast ' crown
                Dim cpts() As PointF = {
                    New PointF(cx - r, cy + r * 0.4F),
                    New PointF(cx - r, cy - r * 0.4F),
                    New PointF(cx - r * 0.35F, cy + r * 0.1F),
                    New PointF(cx, cy - r),
                    New PointF(cx + r * 0.35F, cy + r * 0.1F),
                    New PointF(cx + r, cy - r * 0.4F),
                    New PointF(cx + r, cy + r * 0.4F)
                }
                g.FillPolygon(brW, cpts)
        End Select
    End Sub

    Private Sub DrawOceanIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' bubble with + 
                Using pen As New Pen(brW.Color, r * 0.18F)
                    g.DrawEllipse(pen, cx - r * 0.75F, cy - r * 0.75F, r * 1.5F, r * 1.5F)
                End Using
                g.FillRectangle(brW, cx - r * 0.12F, cy - r * 0.55F, r * 0.24F, r * 1.1F)
                g.FillRectangle(brW, cx - r * 0.55F, cy - r * 0.12F, r * 1.1F, r * 0.24F)
            Case PowerUpType.RedBallShrink ' coral heart
                DrawHeartShape(g, cx - r, cy - r * 0.9F, r * 2, r * 2, Color.FromArgb(220, 255, 120, 140))
            Case PowerUpType.GreenMultiBall ' three bubbles
                Dim sizes() As Single = {0.4F, 0.5F, 0.35F}
                Dim offsets() As Single = {-0.7F, 0.0F, 0.7F}
                For i = 0 To 2
                    Dim sr = r * sizes(i)
                    Using pen As New Pen(brW.Color, r * 0.14F)
                        g.DrawEllipse(pen, cx + offsets(i) * r - sr, cy - sr, sr * 2, sr * 2)
                    End Using
                Next
            Case PowerUpType.YellowBallShrink ' anchor
                Using pen As New Pen(brW.Color, r * 0.2F)
                    g.DrawLine(pen, cx, cy - r, cx, cy + r)
                    g.DrawLine(pen, cx - r * 0.65F, cy - r * 0.5F, cx + r * 0.65F, cy - r * 0.5F)
                    g.DrawArc(pen, cx - r * 0.65F, cy + r * 0.1F, r * 1.3F, r * 0.9F, 0, 180)
                End Using
            Case PowerUpType.PurplePaddleMega ' wave bar
                Dim wpts() As PointF = {
                    New PointF(cx - r, cy),
                    New PointF(cx - r * 0.5F, cy - r * 0.35F),
                    New PointF(cx, cy),
                    New PointF(cx + r * 0.5F, cy + r * 0.35F),
                    New PointF(cx + r, cy)
                }
                Using pen As New Pen(brW.Color, r * 0.25F)
                    g.DrawLines(pen, wpts)
                End Using
            Case PowerUpType.OrangeBallSlow ' starfish
                For i = 0 To 4
                    Dim ang5 = (i * 72 - 90) * Math.PI / 180.0
                    Using pen As New Pen(brW.Color, r * 0.2F)
                        g.DrawLine(pen, cx, cy, cx + CSng(Math.Cos(ang5) * r), cy + CSng(Math.Sin(ang5) * r))
                    End Using
                Next
            Case PowerUpType.PinkBallFast ' fish arrow
                Dim fpts() As PointF = {
                    New PointF(cx + r, cy),
                    New PointF(cx - r * 0.2F, cy - r * 0.6F),
                    New PointF(cx - r * 0.2F, cy + r * 0.6F)
                }
                g.FillPolygon(brW, fpts)
                Dim tpts() As PointF = {
                    New PointF(cx - r * 0.2F, cy - r * 0.5F),
                    New PointF(cx - r, cy - r * 0.7F),
                    New PointF(cx - r, cy + r * 0.7F),
                    New PointF(cx - r * 0.2F, cy + r * 0.5F)
                }
                g.FillPolygon(brW, tpts)
        End Select
    End Sub

    Private Sub DrawRetroIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Dim ps = r * 0.32F ' pixel size
        Select Case pType
            Case PowerUpType.BlueBallGrow ' pixelated +
                g.FillRectangle(brW, cx - ps / 2, cy - r, ps, r * 2)
                g.FillRectangle(brW, cx - r, cy - ps / 2, r * 2, ps)
            Case PowerUpType.RedBallShrink ' pixel heart (3x3 grid)
                Dim hmap(,) As Integer = {{0, 1, 0, 1, 0}, {1, 1, 1, 1, 1}, {1, 1, 1, 1, 1}, {0, 1, 1, 1, 0}, {0, 0, 1, 0, 0}}
                Dim scale = r * 0.38F
                For row = 0 To 4
                    For col = 0 To 4
                        If hmap(row, col) = 1 Then
                            g.FillRectangle(brW, cx - 2.5F * scale + col * scale, cy - 2.5F * scale + row * scale, scale - 1, scale - 1)
                        End If
                    Next
                Next
            Case PowerUpType.GreenMultiBall ' three pixel coins
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.7F
                    g.FillRectangle(brW, cx + ox - ps / 2, cy - ps, ps, ps * 2)
                    g.FillRectangle(brW, cx + ox - ps, cy - ps / 2, ps * 2, ps)
                Next
            Case PowerUpType.YellowBallShrink ' minus pixel
                g.FillRectangle(brW, cx - r, cy - ps / 2, r * 2, ps)
            Case PowerUpType.PurplePaddleMega ' pixel paddle block
                g.FillRectangle(brW, cx - r, cy - ps, r * 2, ps * 2)
            Case PowerUpType.OrangeBallSlow ' hourglass pixels
                For row = 0 To 4
                    Dim wid = Math.Abs(row - 2) * ps + ps / 2
                    g.FillRectangle(brW, cx - wid / 2, cy - r + row * r * 0.44F, wid, ps)
                Next
            Case PowerUpType.PinkBallFast ' pixel up arrow
                For row = 0 To 4
                    Dim wid = (row + 1) * ps
                    g.FillRectangle(brW, cx - wid / 2, cy + r * 0.6F - row * r * 0.44F, wid, ps)
                Next
        End Select
    End Sub

    Private Sub DrawMagicIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' wand with star tip
                Using pen As New Pen(brW.Color, r * 0.2F)
                    g.DrawLine(pen, cx - r * 0.6F, cy + r * 0.6F, cx + r * 0.5F, cy - r * 0.5F)
                End Using
                Dim stpts3(9) As PointF
                For i = 0 To 9
                    Dim ang6 = (i * 36 - 90) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r * 0.42F, r * 0.18F)
                    stpts3(i) = New PointF(cx + r * 0.5F + CSng(Math.Cos(ang6) * rad), cy - r * 0.5F + CSng(Math.Sin(ang6) * rad))
                Next
                g.FillPolygon(brW, stpts3)
            Case PowerUpType.RedBallShrink ' orb (bright circle)
                g.FillEllipse(brW, cx - r * 0.65F, cy - r * 0.65F, r * 1.3F, r * 1.3F)
                Using brGlow As New SolidBrush(Color.FromArgb(140, 255, 200, 255))
                    g.FillEllipse(brGlow, cx - r * 0.3F, cy - r * 0.55F, r * 0.4F, r * 0.35F)
                End Using
            Case PowerUpType.GreenMultiBall ' three sparkle stars
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.72F
                    Dim stpts4(7) As PointF
                    For k = 0 To 7
                        Dim ang7 = (k * 45 - 90) * Math.PI / 180.0
                        Dim rad = If(k Mod 2 = 0, r * 0.28F, r * 0.11F)
                        stpts4(k) = New PointF(cx + ox + CSng(Math.Cos(ang7) * rad), cy + CSng(Math.Sin(ang7) * rad))
                    Next
                    g.FillPolygon(brW, stpts4)
                Next
            Case PowerUpType.YellowBallShrink ' minus rune
                g.FillRectangle(brW, cx - r, cy - r * 0.14F, r * 2, r * 0.28F)
                Using pen As New Pen(brW.Color, r * 0.15F)
                    g.DrawEllipse(pen, cx - r * 0.5F, cy - r * 0.5F, r, r)
                End Using
            Case PowerUpType.PurplePaddleMega ' large spell scroll
                Using rr = RoundedRect(New RectangleF(cx - r, cy - r * 0.32F, r * 2, r * 0.64F), 5)
                    g.FillPath(brW, rr)
                End Using
                g.FillEllipse(brW, cx - r * 1.05F, cy - r * 0.4F, r * 0.4F, r * 0.8F)
                g.FillEllipse(brW, cx + r * 0.65F, cy - r * 0.4F, r * 0.4F, r * 0.8F)
            Case PowerUpType.OrangeBallSlow ' hourglass with sparkle
                Dim hpts2() As PointF = {
                    New PointF(cx - r * 0.6F, cy - r), New PointF(cx + r * 0.6F, cy - r),
                    New PointF(cx + r * 0.12F, cy), New PointF(cx + r * 0.6F, cy + r),
                    New PointF(cx - r * 0.6F, cy + r), New PointF(cx - r * 0.12F, cy)
                }
                g.FillPolygon(brW, hpts2)
            Case PowerUpType.PinkBallFast ' shooting star
                Dim stpts5(9) As PointF
                For i = 0 To 9
                    Dim ang8 = (i * 36 - 90) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r * 0.9F, r * 0.38F)
                    stpts5(i) = New PointF(cx + CSng(Math.Cos(ang8) * rad), cy + CSng(Math.Sin(ang8) * rad))
                Next
                g.FillPolygon(brW, stpts5)
                Using pen As New Pen(brW.Color, r * 0.14F)
                    g.DrawLine(pen, cx + r * 0.6F, cy + r * 0.4F, cx + r * 1.6F, cy + r * 1.0F)
                End Using
        End Select
    End Sub

    Private Sub DrawDragonIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' dragon egg (oval)
                g.FillEllipse(brW, cx - r * 0.6F, cy - r, r * 1.2F, r * 2)
                Using brS As New SolidBrush(Color.FromArgb(80, 0, 100, 200))
                    For row = 0 To 2
                        For col = 0 To 2
                            Dim ex = cx - r * 0.4F + col * r * 0.4F
                            Dim ey = cy - r * 0.5F + row * r * 0.5F
                            g.FillEllipse(brS, ex, ey, r * 0.3F, r * 0.25F)
                        Next
                    Next
                End Using
            Case PowerUpType.RedBallShrink ' flame (teardrop)
                Dim fpts2() As PointF = {
                    New PointF(cx, cy - r),
                    New PointF(cx + r * 0.55F, cy + r * 0.3F),
                    New PointF(cx, cy + r * 0.6F),
                    New PointF(cx - r * 0.55F, cy + r * 0.3F)
                }
                Using brFire As New SolidBrush(Color.FromArgb(220, 255, 140, 0))
                    g.FillPolygon(brFire, fpts2)
                End Using
                g.FillEllipse(brW, cx - r * 0.2F, cy - r * 0.4F, r * 0.4F, r * 0.4F)
            Case PowerUpType.GreenMultiBall ' claw marks (3 lines)
                Using pen As New Pen(brW.Color, r * 0.22F)
                    For i = -1 To 1
                        g.DrawLine(pen, cx + i * r * 0.45F, cy - r, cx + i * r * 0.45F + r * 0.3F, cy + r)
                    Next
                End Using
            Case PowerUpType.YellowBallShrink ' scale diamond
                Dim dpts2() As PointF = {
                    New PointF(cx, cy - r), New PointF(cx + r * 0.6F, cy),
                    New PointF(cx, cy + r), New PointF(cx - r * 0.6F, cy)
                }
                g.FillPolygon(brW, dpts2)
            Case PowerUpType.PurplePaddleMega ' wing spread
                Dim wl() As PointF = {New PointF(cx, cy), New PointF(cx - r * 0.6F, cy - r), New PointF(cx - r, cy), New PointF(cx - r * 0.5F, cy + r * 0.5F)}
                Dim wr() As PointF = {New PointF(cx, cy), New PointF(cx + r * 0.6F, cy - r), New PointF(cx + r, cy), New PointF(cx + r * 0.5F, cy + r * 0.5F)}
                g.FillPolygon(brW, wl)
                g.FillPolygon(brW, wr)
            Case PowerUpType.OrangeBallSlow ' lava bubble
                Using brLav As New SolidBrush(Color.FromArgb(200, 255, 80, 0))
                    g.FillEllipse(brLav, cx - r * 0.7F, cy - r * 0.7F, r * 1.4F, r * 1.4F)
                End Using
                g.FillEllipse(brW, cx - r * 0.25F, cy - r * 0.55F, r * 0.3F, r * 0.3F)
            Case PowerUpType.PinkBallFast ' dragon claw arrow
                Dim cpts2() As PointF = {
                    New PointF(cx, cy - r),
                    New PointF(cx + r * 0.4F, cy),
                    New PointF(cx + r * 0.2F, cy + r * 0.2F),
                    New PointF(cx, cy - r * 0.2F),
                    New PointF(cx - r * 0.2F, cy + r * 0.2F),
                    New PointF(cx - r * 0.4F, cy)
                }
                g.FillPolygon(brW, cpts2)
        End Select
    End Sub

    Private Sub DrawSakuraIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' 5-petal flower
                For i = 0 To 4
                    Dim ang9 = (i * 72 - 90) * Math.PI / 180.0
                    Dim px2 = cx + CSng(Math.Cos(ang9) * r * 0.55F)
                    Dim py2 = cy + CSng(Math.Sin(ang9) * r * 0.55F)
                    g.FillEllipse(brW, px2 - r * 0.35F, py2 - r * 0.35F, r * 0.7F, r * 0.7F)
                Next
                g.FillEllipse(brW, cx - r * 0.28F, cy - r * 0.28F, r * 0.56F, r * 0.56F)
            Case PowerUpType.RedBallShrink ' heart petal
                DrawHeartShape(g, cx - r, cy - r * 0.9F, r * 2, r * 2, Color.FromArgb(240, 255, 150, 180))
            Case PowerUpType.GreenMultiBall ' three petals
                For i = 0 To 2
                    Dim ang10 = (i * 120 - 90) * Math.PI / 180.0
                    Dim px3 = cx + CSng(Math.Cos(ang10) * r * 0.5F)
                    Dim py3 = cy + CSng(Math.Sin(ang10) * r * 0.5F)
                    g.FillEllipse(brW, px3 - r * 0.38F, py3 - r * 0.38F, r * 0.76F, r * 0.76F)
                Next
            Case PowerUpType.YellowBallShrink ' fan (wedge lines)
                Using pen As New Pen(brW.Color, r * 0.16F)
                    For i = -2 To 2
                        Dim ang11 = (i * 22 + 90) * Math.PI / 180.0
                        g.DrawLine(pen, cx, cy, cx + CSng(Math.Cos(ang11) * r), cy - CSng(Math.Sin(ang11) * r))
                    Next
                End Using
            Case PowerUpType.PurplePaddleMega ' branch (horizontal + petals)
                Using pen As New Pen(brW.Color, r * 0.18F)
                    g.DrawLine(pen, cx - r, cy, cx + r, cy)
                    g.DrawLine(pen, cx - r * 0.4F, cy, cx - r * 0.4F, cy - r * 0.6F)
                    g.DrawLine(pen, cx + r * 0.4F, cy, cx + r * 0.4F, cy - r * 0.6F)
                End Using
                g.FillEllipse(brW, cx - r * 0.7F, cy - r * 0.5F, r * 0.44F, r * 0.44F)
                g.FillEllipse(brW, cx + r * 0.24F, cy - r * 0.5F, r * 0.44F, r * 0.44F)
            Case PowerUpType.OrangeBallSlow ' leaf
                Dim lpts2() As PointF = {
                    New PointF(cx, cy - r),
                    New PointF(cx + r * 0.55F, cy),
                    New PointF(cx, cy + r),
                    New PointF(cx - r * 0.55F, cy)
                }
                g.FillPolygon(brW, lpts2)
                Using pen As New Pen(Color.FromArgb(120, 80, 60, 0), r * 0.12F)
                    g.DrawLine(pen, cx, cy - r, cx, cy + r)
                End Using
            Case PowerUpType.PinkBallFast ' spinning petal burst
                Dim stpts6(9) As PointF
                For i = 0 To 9
                    Dim ang12 = (i * 36 + _frameCount * 3) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r * 0.9F, r * 0.4F)
                    stpts6(i) = New PointF(cx + CSng(Math.Cos(ang12) * rad), cy + CSng(Math.Sin(ang12) * rad))
                Next
                g.FillPolygon(brW, stpts6)
        End Select
    End Sub

    Private Sub DrawRobotIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Dim t = r * 0.22F
        Select Case pType
            Case PowerUpType.BlueBallGrow ' gear (ring + teeth)
                Using pen As New Pen(brW.Color, t)
                    g.DrawEllipse(pen, cx - r * 0.6F, cy - r * 0.6F, r * 1.2F, r * 1.2F)
                End Using
                For i = 0 To 7
                    Dim ang13 = i * 45 * Math.PI / 180.0
                    g.FillRectangle(brW, cx + CSng(Math.Cos(ang13) * r * 0.6F) - t / 2, cy + CSng(Math.Sin(ang13) * r * 0.6F) - t / 2, t, t)
                Next
            Case PowerUpType.RedBallShrink ' eye (circle + dot)
                Using pen As New Pen(brW.Color, t * 0.8F)
                    g.DrawEllipse(pen, cx - r * 0.65F, cy - r * 0.35F, r * 1.3F, r * 0.7F)
                End Using
                g.FillEllipse(brW, cx - r * 0.2F, cy - r * 0.2F, r * 0.4F, r * 0.4F)
            Case PowerUpType.GreenMultiBall ' three cpu nodes
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.72F
                    Using rr = RoundedRect(New RectangleF(cx + ox - r * 0.28F, cy - r * 0.28F, r * 0.56F, r * 0.56F), 2)
                        g.FillPath(brW, rr)
                    End Using
                Next
                Using pen As New Pen(brW.Color, t * 0.7F)
                    g.DrawLine(pen, cx - r * 0.7F, cy, cx + r * 0.7F, cy)
                End Using
            Case PowerUpType.YellowBallShrink ' bolt
                g.FillRectangle(brW, cx - r, cy - t / 2, r * 2, t)
            Case PowerUpType.PurplePaddleMega ' chassis bar with bolts
                g.FillRectangle(brW, cx - r, cy - t, r * 2, t * 2)
                For i = -1 To 1
                    g.FillEllipse(brW, cx + i * r * 0.65F - t * 0.6F, cy - t * 1.4F, t * 1.2F, t * 1.2F)
                Next
            Case PowerUpType.OrangeBallSlow ' gear slow (ring only + teeth)
                Using pen As New Pen(brW.Color, t * 0.7F)
                    g.DrawEllipse(pen, cx - r * 0.55F, cy - r * 0.55F, r * 1.1F, r * 1.1F)
                End Using
                For i = 0 To 5
                    Dim ang14 = i * 60 * Math.PI / 180.0
                    Using pen As New Pen(brW.Color, t)
                        g.DrawLine(pen,
                            cx + CSng(Math.Cos(ang14) * r * 0.55F), cy + CSng(Math.Sin(ang14) * r * 0.55F),
                            cx + CSng(Math.Cos(ang14) * r * 0.9F), cy + CSng(Math.Sin(ang14) * r * 0.9F))
                    End Using
                Next
            Case PowerUpType.PinkBallFast ' lightning bolt
                Dim lpts2() As PointF = {
                    New PointF(cx + r * 0.18F, cy - r),
                    New PointF(cx - r * 0.12F, cy - r * 0.1F),
                    New PointF(cx + r * 0.28F, cy - r * 0.1F),
                    New PointF(cx - r * 0.22F, cy + r)
                }
                g.FillPolygon(brW, lpts2)
        End Select
    End Sub

    Private Sub DrawPirateIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' treasure chest
                Using rr = RoundedRect(New RectangleF(cx - r, cy - r * 0.4F, r * 2, r * 1.0F), 3)
                    g.FillPath(brW, rr)
                End Using
                Using brD As New SolidBrush(Color.FromArgb(150, 100, 60, 0))
                    g.FillRectangle(brD, cx - r, cy - r * 0.4F, r * 2, r * 0.2F)
                End Using
                Using brK As New SolidBrush(Color.FromArgb(200, 255, 200, 0))
                    g.FillEllipse(brK, cx - r * 0.18F, cy - r * 0.1F, r * 0.36F, r * 0.36F)
                End Using
            Case PowerUpType.RedBallShrink ' skull
                g.FillEllipse(brW, cx - r * 0.6F, cy - r, r * 1.2F, r * 1.1F)
                Using brD As New SolidBrush(Color.FromArgb(200, 40, 20, 40))
                    g.FillEllipse(brD, cx - r * 0.38F, cy - r * 0.6F, r * 0.38F, r * 0.38F)
                    g.FillEllipse(brD, cx + r * 0.0F, cy - r * 0.6F, r * 0.38F, r * 0.38F)
                    g.FillRectangle(brD, cx - r * 0.15F, cy, r * 0.3F, r * 0.25F)
                End Using
                g.FillRectangle(brW, cx - r * 0.4F, cy + r * 0.12F, r * 0.2F, r * 0.25F)
                g.FillRectangle(brW, cx + r * 0.2F, cy + r * 0.12F, r * 0.2F, r * 0.25F)
            Case PowerUpType.GreenMultiBall ' three gold coins
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.7F
                    Using brG As New SolidBrush(Color.FromArgb(220, 255, 200, 0))
                        g.FillEllipse(brG, cx + ox - r * 0.32F, cy - r * 0.32F, r * 0.64F, r * 0.64F)
                    End Using
                    Using pen As New Pen(Color.FromArgb(180, 200, 140, 0), r * 0.1F)
                        g.DrawEllipse(pen, cx + ox - r * 0.32F, cy - r * 0.32F, r * 0.64F, r * 0.64F)
                    End Using
                Next
            Case PowerUpType.YellowBallShrink ' anchor (reuse ocean)
                Using pen As New Pen(brW.Color, r * 0.2F)
                    g.DrawLine(pen, cx, cy - r, cx, cy + r)
                    g.DrawLine(pen, cx - r * 0.65F, cy - r * 0.5F, cx + r * 0.65F, cy - r * 0.5F)
                    g.DrawArc(pen, cx - r * 0.65F, cy + r * 0.1F, r * 1.3F, r * 0.9F, 0, 180)
                End Using
            Case PowerUpType.PurplePaddleMega ' jolly roger flag bar
                g.FillRectangle(brW, cx - r, cy - r * 0.18F, r * 2, r * 0.36F)
                Using pen As New Pen(brW.Color, r * 0.16F)
                    g.DrawLine(pen, cx - r * 0.5F, cy - r * 0.6F, cx + r * 0.5F, cy + r * 0.6F)
                    g.DrawLine(pen, cx + r * 0.5F, cy - r * 0.6F, cx - r * 0.5F, cy + r * 0.6F)
                End Using
            Case PowerUpType.OrangeBallSlow ' cannon ball
                Using pen As New Pen(brW.Color, r * 0.2F)
                    g.DrawEllipse(pen, cx - r * 0.65F, cy - r * 0.65F, r * 1.3F, r * 1.3F)
                End Using
                g.FillEllipse(brW, cx - r * 0.18F, cy - r * 0.5F, r * 0.3F, r * 0.3F)
            Case PowerUpType.PinkBallFast ' hook
                Using pen As New Pen(brW.Color, r * 0.22F)
                    g.DrawArc(pen, cx - r * 0.6F, cy - r * 0.6F, r * 1.2F, r * 1.2F, 20, 300)
                End Using
                Dim hkpts() As PointF = {
                    New PointF(cx + r * 0.5F, cy + r * 0.5F),
                    New PointF(cx + r * 0.95F, cy + r * 0.15F),
                    New PointF(cx + r * 0.7F, cy + r * 0.5F)
                }
                g.FillPolygon(brW, hkpts)
        End Select
    End Sub

    Private Sub DrawGalaxyIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' supernova burst (multi-ray star)
                Dim stpts7(11) As PointF
                For i = 0 To 11
                    Dim ang15 = (i * 30 - 90) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r, r * 0.35F)
                    stpts7(i) = New PointF(cx + CSng(Math.Cos(ang15) * rad), cy + CSng(Math.Sin(ang15) * rad))
                Next
                g.FillPolygon(brW, stpts7)
            Case PowerUpType.RedBallShrink ' pulsar rings
                For k = 1 To 3
                    Using pen As New Pen(Color.FromArgb(220 - k * 50, 255, 255, 255), r * 0.15F)
                        g.DrawEllipse(pen, cx - k * r * 0.3F, cy - k * r * 0.3F, k * r * 0.6F, k * r * 0.6F)
                    End Using
                Next
            Case PowerUpType.GreenMultiBall ' three mini nebulae
                Dim nc() As Color = {Color.FromArgb(200, 0, 200, 160), Color.FromArgb(200, 100, 0, 200), Color.FromArgb(200, 0, 160, 200)}
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.72F
                    Using gbr As New SolidBrush(nc(i))
                        g.FillEllipse(gbr, cx + ox - r * 0.38F, cy - r * 0.38F, r * 0.76F, r * 0.76F)
                    End Using
                Next
            Case PowerUpType.YellowBallShrink ' solar flare minus
                g.FillRectangle(brW, cx - r, cy - r * 0.14F, r * 2, r * 0.28F)
                For i = -1 To 1 Step 2
                    g.FillEllipse(brW, cx + i * r * 0.9F - r * 0.2F, cy - r * 0.2F, r * 0.4F, r * 0.4F)
                Next
            Case PowerUpType.PurplePaddleMega ' wormhole oval
                Using pen As New Pen(brW.Color, r * 0.18F)
                    g.DrawEllipse(pen, cx - r, cy - r * 0.4F, r * 2, r * 0.8F)
                End Using
                For k = 1 To 2
                    Using pen As New Pen(Color.FromArgb(120, 255, 255, 255), r * 0.1F)
                        g.DrawEllipse(pen, cx - r * 0.6F * k, cy - r * 0.24F * k, r * 1.2F * k, r * 0.48F * k)
                    End Using
                Next
            Case PowerUpType.OrangeBallSlow ' comet with tail
                g.FillEllipse(brW, cx + r * 0.2F, cy - r * 0.4F, r * 0.6F, r * 0.6F)
                Dim tpts2() As PointF = {
                    New PointF(cx + r * 0.2F, cy - r * 0.2F),
                    New PointF(cx - r, cy + r * 0.8F),
                    New PointF(cx + r * 0.2F, cy + r * 0.2F)
                }
                g.FillPolygon(brW, tpts2)
            Case PowerUpType.PinkBallFast ' hyperdrive arrow
                Dim pts3() As PointF = {
                    New PointF(cx + r, cy),
                    New PointF(cx, cy - r * 0.6F),
                    New PointF(cx - r * 0.2F, cy - r * 0.2F),
                    New PointF(cx - r, cy - r * 0.2F),
                    New PointF(cx - r, cy + r * 0.2F),
                    New PointF(cx - r * 0.2F, cy + r * 0.2F),
                    New PointF(cx, cy + r * 0.6F)
                }
                g.FillPolygon(brW, pts3)
        End Select
    End Sub

    Private Sub DrawFestivalIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' firework burst
                For i = 0 To 7
                    Dim ang16 = i * 45 * Math.PI / 180.0
                    Using pen As New Pen(brW.Color, r * 0.18F)
                        g.DrawLine(pen, cx, cy, cx + CSng(Math.Cos(ang16) * r), cy + CSng(Math.Sin(ang16) * r))
                    End Using
                Next
                g.FillEllipse(brW, cx - r * 0.22F, cy - r * 0.22F, r * 0.44F, r * 0.44F)
            Case PowerUpType.RedBallShrink ' heart confetti
                DrawHeartShape(g, cx - r, cy - r * 0.9F, r * 2, r * 2, Color.FromArgb(240, 255, 80, 120))
            Case PowerUpType.GreenMultiBall ' three confetti squares
                Dim cc2() As Color = {Color.FromArgb(220, 100, 255, 80), Color.FromArgb(220, 255, 80, 120), Color.FromArgb(220, 80, 160, 255)}
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.72F
                    Using gbr As New SolidBrush(cc2(i))
                        Dim rot = (i - 1) * 25.0F
                        Dim state = g.Save()
                        g.TranslateTransform(cx + ox, cy)
                        g.RotateTransform(rot)
                        g.FillRectangle(gbr, -r * 0.28F, -r * 0.28F, r * 0.56F, r * 0.56F)
                        g.Restore(state)
                    End Using
                Next
            Case PowerUpType.YellowBallShrink ' star sparkle
                Dim stpts8(9) As PointF
                For i = 0 To 9
                    Dim ang17 = (i * 36 - 90) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r * 0.9F, r * 0.38F)
                    stpts8(i) = New PointF(cx + CSng(Math.Cos(ang17) * rad), cy + CSng(Math.Sin(ang17) * rad))
                Next
                g.FillPolygon(brW, stpts8)
            Case PowerUpType.PurplePaddleMega ' party banner
                Using pen As New Pen(brW.Color, r * 0.18F)
                    g.DrawLine(pen, cx - r, cy - r * 0.3F, cx + r, cy - r * 0.3F)
                End Using
                For i = 0 To 4
                    Dim tx = cx - r + i * r * 0.5F
                    Dim tpts3() As PointF = {New PointF(tx, cy - r * 0.3F), New PointF(tx + r * 0.25F, cy + r * 0.5F), New PointF(tx - r * 0.0F, cy + r * 0.5F)}
                    Using gbr As New SolidBrush(Color.FromArgb(200, 255, 200, 50 + i * 40))
                        g.FillPolygon(gbr, tpts3)
                    End Using
                Next
            Case PowerUpType.OrangeBallSlow ' lantern
                Using rr = RoundedRect(New RectangleF(cx - r * 0.5F, cy - r * 0.7F, r, r * 1.2F), 6)
                    g.FillPath(brW, rr)
                End Using
                Using brG As New SolidBrush(Color.FromArgb(180, 255, 200, 0))
                    g.FillEllipse(brG, cx - r * 0.28F, cy - r * 0.4F, r * 0.56F, r * 0.56F)
                End Using
                Using pen As New Pen(brW.Color, r * 0.12F)
                    g.DrawLine(pen, cx, cy - r * 0.7F, cx, cy - r)
                End Using
            Case PowerUpType.PinkBallFast ' ribbon star
                Dim stpts9(9) As PointF
                For i = 0 To 9
                    Dim ang18 = (i * 36 - 90 + _frameCount * 2) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r, r * 0.42F)
                    stpts9(i) = New PointF(cx + CSng(Math.Cos(ang18) * rad), cy + CSng(Math.Sin(ang18) * rad))
                Next
                g.FillPolygon(brW, stpts9)
        End Select
    End Sub

    Private Sub DrawHorrorIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' bat wings
                Dim wl2() As PointF = {New PointF(cx, cy), New PointF(cx - r * 0.4F, cy - r), New PointF(cx - r, cy - r * 0.5F), New PointF(cx - r, cy + r * 0.2F)}
                Dim wr2() As PointF = {New PointF(cx, cy), New PointF(cx + r * 0.4F, cy - r), New PointF(cx + r, cy - r * 0.5F), New PointF(cx + r, cy + r * 0.2F)}
                g.FillPolygon(brW, wl2) : g.FillPolygon(brW, wr2)
            Case PowerUpType.RedBallShrink ' blood drip
                g.FillEllipse(brW, cx - r * 0.5F, cy - r * 0.7F, r, r)
                Dim dpts3() As PointF = {New PointF(cx - r * 0.25F, cy - r * 0.2F), New PointF(cx + r * 0.25F, cy - r * 0.2F), New PointF(cx, cy + r)}
                g.FillPolygon(brW, dpts3)
            Case PowerUpType.GreenMultiBall ' three slime blobs
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.7F
                    Dim spts3() As PointF = {
                        New PointF(cx + ox, cy - r * 0.4F),
                        New PointF(cx + ox + r * 0.32F, cy),
                        New PointF(cx + ox + r * 0.22F, cy + r * 0.4F),
                        New PointF(cx + ox - r * 0.22F, cy + r * 0.4F),
                        New PointF(cx + ox - r * 0.32F, cy)
                    }
                    g.FillPolygon(brW, spts3)
                Next
            Case PowerUpType.YellowBallShrink ' ghost minus
                g.FillRectangle(brW, cx - r, cy - r * 0.14F, r * 2, r * 0.28F)
                Using pen As New Pen(brW.Color, r * 0.14F)
                    g.DrawLine(pen, cx - r * 0.5F, cy - r * 0.6F, cx - r * 0.5F, cy - r * 0.1F)
                    g.DrawLine(pen, cx + r * 0.5F, cy - r * 0.6F, cx + r * 0.5F, cy - r * 0.1F)
                End Using
            Case PowerUpType.PurplePaddleMega ' coffin lid
                Dim cpts3() As PointF = {
                    New PointF(cx - r * 0.6F, cy - r),
                    New PointF(cx + r * 0.6F, cy - r),
                    New PointF(cx + r, cy - r * 0.3F),
                    New PointF(cx + r, cy + r * 0.8F),
                    New PointF(cx - r, cy + r * 0.8F),
                    New PointF(cx - r, cy - r * 0.3F)
                }
                g.FillPolygon(brW, cpts3)
            Case PowerUpType.OrangeBallSlow ' pumpkin
                g.FillEllipse(brW, cx - r * 0.75F, cy - r * 0.7F, r * 1.5F, r * 1.4F)
                Using brD As New SolidBrush(Color.FromArgb(180, 40, 20, 0))
                    ' Left eye triangle
                    Dim lEye() As PointF = {
                        New PointF(cx - r * 0.4F, cy - r * 0.25F),
                        New PointF(cx - r * 0.4F + r * 0.28F, cy - r * 0.25F),
                        New PointF(cx - r * 0.4F + r * 0.14F, cy - r * 0.25F - r * 0.24F)
                    }
                    g.FillPolygon(brD, lEye)
                    ' Right eye triangle
                    Dim rEye() As PointF = {
                        New PointF(cx + r * 0.12F, cy - r * 0.25F),
                        New PointF(cx + r * 0.12F + r * 0.28F, cy - r * 0.25F),
                        New PointF(cx + r * 0.12F + r * 0.14F, cy - r * 0.25F - r * 0.24F)
                    }
                    g.FillPolygon(brD, rEye)
                    g.FillRectangle(brD, cx - r * 0.28F, cy + r * 0.05F, r * 0.56F, r * 0.25F)
                End Using
                Using pen As New Pen(brW.Color, r * 0.14F)
                    g.DrawLine(pen, cx, cy - r * 0.7F, cx + r * 0.2F, cy - r)
                End Using
            Case PowerUpType.PinkBallFast ' ghost
                g.FillEllipse(brW, cx - r * 0.6F, cy - r, r * 1.2F, r * 1.2F)
                Dim gpts() As PointF = {
                    New PointF(cx - r * 0.6F, cy + r * 0.2F),
                    New PointF(cx - r * 0.6F, cy + r),
                    New PointF(cx - r * 0.3F, cy + r * 0.7F),
                    New PointF(cx, cy + r),
                    New PointF(cx + r * 0.3F, cy + r * 0.7F),
                    New PointF(cx + r * 0.6F, cy + r),
                    New PointF(cx + r * 0.6F, cy + r * 0.2F)
                }
                g.FillPolygon(brW, gpts)
                Using brE As New SolidBrush(Color.FromArgb(200, 40, 20, 60))
                    g.FillEllipse(brE, cx - r * 0.38F, cy - r * 0.4F, r * 0.3F, r * 0.3F)
                    g.FillEllipse(brE, cx + r * 0.08F, cy - r * 0.4F, r * 0.3F, r * 0.3F)
                End Using
        End Select
    End Sub

    Private Sub DrawGoldenIcon(g As Graphics, pType As PowerUpType, cx As Single, cy As Single, r As Single, brW As SolidBrush)
        Select Case pType
            Case PowerUpType.BlueBallGrow ' laurel wreath circle
                Using pen As New Pen(brW.Color, r * 0.18F)
                    g.DrawArc(pen, cx - r * 0.8F, cy - r * 0.8F, r * 1.6F, r * 1.6F, 160, 220)
                    g.DrawArc(pen, cx - r * 0.8F, cy - r * 0.8F, r * 1.6F, r * 1.6F, -20, -220)
                End Using
                Dim stpts10(9) As PointF
                For i = 0 To 9
                    Dim ang19 = (i * 36 - 90) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r * 0.4F, r * 0.18F)
                    stpts10(i) = New PointF(cx + CSng(Math.Cos(ang19) * rad), cy + CSng(Math.Sin(ang19) * rad))
                Next
                g.FillPolygon(brW, stpts10)
            Case PowerUpType.RedBallShrink ' coin with plus
                Using pen As New Pen(brW.Color, r * 0.18F)
                    g.DrawEllipse(pen, cx - r * 0.7F, cy - r * 0.7F, r * 1.4F, r * 1.4F)
                End Using
                g.FillRectangle(brW, cx - r * 0.1F, cy - r * 0.5F, r * 0.2F, r)
                g.FillRectangle(brW, cx - r * 0.5F, cy - r * 0.1F, r, r * 0.2F)
            Case PowerUpType.GreenMultiBall ' three star coins
                For i = 0 To 2
                    Dim ox = (i - 1) * r * 0.72F
                    Using gbr As New SolidBrush(Color.FromArgb(220, 255, 220, 40))
                        g.FillEllipse(gbr, cx + ox - r * 0.3F, cy - r * 0.3F, r * 0.6F, r * 0.6F)
                    End Using
                Next
            Case PowerUpType.YellowBallShrink ' ring
                Using pen As New Pen(brW.Color, r * 0.22F)
                    g.DrawEllipse(pen, cx - r * 0.7F, cy - r * 0.7F, r * 1.4F, r * 1.4F)
                End Using
            Case PowerUpType.PurplePaddleMega ' wide crown
                Dim cpts4() As PointF = {
                    New PointF(cx - r, cy + r * 0.4F),
                    New PointF(cx - r, cy - r * 0.3F),
                    New PointF(cx - r * 0.5F, cy + r * 0.1F),
                    New PointF(cx - r * 0.2F, cy - r),
                    New PointF(cx, cy + r * 0.1F),
                    New PointF(cx + r * 0.2F, cy - r),
                    New PointF(cx + r * 0.5F, cy + r * 0.1F),
                    New PointF(cx + r, cy - r * 0.3F),
                    New PointF(cx + r, cy + r * 0.4F)
                }
                g.FillPolygon(brW, cpts4)
            Case PowerUpType.OrangeBallSlow ' hourglass gold
                Dim hpts3() As PointF = {
                    New PointF(cx - r * 0.65F, cy - r), New PointF(cx + r * 0.65F, cy - r),
                    New PointF(cx + r * 0.12F, cy), New PointF(cx + r * 0.65F, cy + r),
                    New PointF(cx - r * 0.65F, cy + r), New PointF(cx - r * 0.12F, cy)
                }
                g.FillPolygon(brW, hpts3)
            Case PowerUpType.PinkBallFast ' shooting star
                Dim stpts11(9) As PointF
                For i = 0 To 9
                    Dim ang20 = (i * 36 - 90) * Math.PI / 180.0
                    Dim rad = If(i Mod 2 = 0, r * 0.9F, r * 0.38F)
                    stpts11(i) = New PointF(cx + CSng(Math.Cos(ang20) * rad), cy + CSng(Math.Sin(ang20) * rad))
                Next
                g.FillPolygon(brW, stpts11)
                Using pen As New Pen(brW.Color, r * 0.14F)
                    g.DrawLine(pen, cx + r * 0.5F, cy + r * 0.5F, cx + r * 1.5F, cy + r)
                End Using
        End Select
    End Sub

    Private Function GetPowerUpCBLabel(pType As PowerUpType) As String
        Select Case pType
            Case PowerUpType.BlueBallGrow : Return "BIG"
            Case PowerUpType.RedBallShrink : Return "1UP"
            Case PowerUpType.GreenMultiBall : Return "x3"
            Case PowerUpType.YellowBallShrink : Return "SML"
            Case PowerUpType.PurplePaddleMega : Return "PAD"
            Case PowerUpType.OrangeBallSlow : Return "SLW"
            Case PowerUpType.PinkBallFast : Return "FST"
            Case Else : Return "?"
        End Select
    End Function

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
            ' Sync status row
            Dim syncLabel = "SYNC:  " & GetSyncLabel() & "   [ Press S to sync now ]"
            Dim syncC = If(_syncStatus = "Synced", Color.FromArgb(80, 220, 80),
                        If(_syncStatus = "Syncing", Color.FromArgb(255, 220, 60),
                        If(_syncStatus = "Failed", Color.FromArgb(255, 80, 80),
                        Color.FromArgb(140, 140, 140))))
            Using brSync As New SolidBrush(syncC)
                g.DrawString(syncLabel, fHint, brSync, px + 25, py + ph - 52)
            End Using
            DrawCenteredText(g, ChrW(&H2191) & ChrW(&H2193) & " Select   " & ChrW(&H2190) & ChrW(&H2192) & " Adjust   ENTER Toggle   O / ESC Close", fHint, Color.FromArgb(130, 130, 155), py + ph - 30)
        End Using
    End Sub

    ' ── Helper: returns the two gradient colours used to represent a ball skin ──
    Private Function GetBallPreviewColors(skinId As String) As Color()
        Select Case skinId
            Case "fire"   : Return {Color.FromArgb(255, 100, 20), Color.FromArgb(255, 200, 50)}
            Case "ice"    : Return {Color.FromArgb(100, 200, 255), Color.FromArgb(200, 240, 255)}
            Case "plasma" : Return {Color.FromArgb(180, 60, 255), Color.FromArgb(240, 160, 255)}
            Case "gold"   : Return {Color.FromArgb(255, 215, 0), Color.FromArgb(255, 250, 140)}
            Case "rainbow": Return {Color.FromArgb(255, 80, 80), Color.FromArgb(80, 160, 255)}
            Case "lava"   : Return {Color.FromArgb(220, 30, 0), Color.FromArgb(255, 140, 40)}
            Case "void"   : Return {Color.FromArgb(20, 0, 40), Color.FromArgb(80, 0, 120)}
            Case "toxic"  : Return {Color.FromArgb(60, 220, 0), Color.FromArgb(180, 255, 60)}
            Case "neon"   : Return {Color.FromArgb(0, 240, 255), Color.FromArgb(160, 255, 255)}
            Case "crystal": Return {Color.FromArgb(180, 240, 255), Color.FromArgb(255, 255, 255)}
            Case "shadow" : Return {Color.FromArgb(60, 0, 100), Color.FromArgb(140, 60, 200)}
            Case "sakura" : Return {Color.FromArgb(255, 160, 200), Color.FromArgb(255, 220, 240)}
            Case "copper" : Return {Color.FromArgb(180, 100, 40), Color.FromArgb(230, 170, 100)}
            Case "ocean"  : Return {Color.FromArgb(0, 130, 180), Color.FromArgb(60, 210, 230)}
            Case "star"   : Return {Color.FromArgb(255, 240, 100), Color.FromArgb(255, 255, 200)}
            Case "obsidian": Return {Color.FromArgb(30, 20, 40), Color.FromArgb(80, 60, 100)}
            Case "aurora" : Return {Color.FromArgb(0, 200, 160), Color.FromArgb(160, 80, 255)}
            Case Else     : Return {Color.FromArgb(220, 230, 255), Color.FromArgb(255, 255, 255)}
        End Select
    End Function

    ' ── Helper: returns a representative 4-colour brick-row sample for a palette ──
    Private Function GetBrickPaletteSample(paletteId As String) As Color()
        Dim pal As Color()() = Nothing
        Dim savedId = _activeBrickPalette
        _activeBrickPalette = paletteId
        pal = GetBrickPalette()
        _activeBrickPalette = savedId
        Return {pal(0)(0), pal(2)(0), pal(4)(0), pal(6)(0)}
    End Function

    ' ── Helper: returns the icon character and colour for a bonus-pack preview ──
    Private Function GetBonusPreview(bonusId As String) As (Symbol As String, Clr As Color)
        Select Case bonusId
            Case "base"     : Return (ChrW(&H2605) & ChrW(&H25C6), Color.FromArgb(255, 220, 60))  ' ★◆  Classic
            Case "ninja"    : Return (ChrW(&H2726) & ChrW(&H22C6), Color.FromArgb(80, 100, 140))  ' ✦⋆  Ninja
            Case "space"    : Return (ChrW(&H25CE) & ChrW(&H2605), Color.FromArgb(80, 140, 255))  ' ◎★  Space
            Case "candy"    : Return (ChrW(&H2665) & ChrW(&H25CF), Color.FromArgb(255, 110, 180)) ' ♥●  Candy
            Case "cyber"    : Return (ChrW(&H26A1) & ChrW(&H25A0), Color.FromArgb(0, 220, 255))   ' ⚡■  Cyber
            Case "medieval" : Return (ChrW(&H2720) & ChrW(&H25C6), Color.FromArgb(200, 160, 60))  ' ✠◆  Medieval
            Case "ocean"    : Return (ChrW(&H223C) & ChrW(&H25CB), Color.FromArgb(60, 180, 255))  ' ∼○  Ocean
            Case "retro"    : Return (ChrW(&H25A0) & ChrW(&H25AA), Color.FromArgb(120, 255, 120)) ' ■▪  Retro
            Case "magic"    : Return (ChrW(&H2727) & ChrW(&H2726), Color.FromArgb(200, 80, 255))  ' ✧✦  Magic
            Case "dragon"   : Return (ChrW(&H25C6) & ChrW(&H2605), Color.FromArgb(255, 80, 40))   ' ◆★  Dragon
            Case "sakura"   : Return (ChrW(&H25CF) & ChrW(&H2665), Color.FromArgb(255, 160, 210)) ' ●♥  Sakura
            Case "robot"    : Return (ChrW(&H2699) & ChrW(&H25A0), Color.FromArgb(160, 200, 220)) ' ⚙■  Robot
            Case "pirate"   : Return (ChrW(&H2620) & ChrW(&H25C6), Color.FromArgb(200, 200, 200)) ' ☠◆  Pirate
            Case "galaxy"   : Return (ChrW(&H273A) & ChrW(&H2605), Color.FromArgb(160, 80, 255))  ' ✺★  Galaxy
            Case "festival" : Return (ChrW(&H272A) & ChrW(&H2665), Color.FromArgb(255, 130, 60))  ' ✪♥  Festival
            Case "horror"   : Return (ChrW(&H2620) & ChrW(&H2726), Color.FromArgb(160, 60, 200))  ' ☠✦  Horror
            Case "golden"   : Return (ChrW(&H25C6) & ChrW(&H25C6), Color.FromArgb(255, 200, 40))  ' ◆◆  Golden
            Case Else       : Return (ChrW(&H2605), Color.FromArgb(255, 220, 80))
        End Select
    End Function

    ' ─────────────────────────────────────────────────────────────────────────────
    '  DrawStore  — scrollable card-based store with visual item previews
    ' ─────────────────────────────────────────────────────────────────────────────
    Private Sub DrawStore(g As Graphics)
        DrawStarField(g)
        Using br As New SolidBrush(Color.FromArgb(210, 0, 0, 20))
            g.FillRectangle(br, 0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT)
        End Using

        ' ── Layout constants ────────────────────────────────────────────────────
        ' Panel: 88% wide, 86% tall, centered
        Const PAD_H As Integer = 36        ' inner left/right padding
        Const PAD_V As Integer = 28        ' inner top/bottom padding
        Const HDR_H As Integer = 72        ' header height
        Const TAB_H As Integer = 52        ' tab row height
        Const HDR_TAB_GAP As Integer = 8   ' gap between header bottom and tab top
        Const TAB_GRID_GAP As Integer = 20 ' gap between tab bottom and first card row
        Const FTR_H As Integer = 50        ' footer height
        Const FTR_PAD As Integer = 20      ' extra bottom padding above panel border
        Const SB_W As Integer = 14         ' scrollbar width
        Const SB_GAP As Integer = 12       ' gap between grid and scrollbar
        Const COL_GAP As Integer = 16      ' column gap
        Const ROW_GAP As Integer = 14      ' row gap
        Const CARD_H As Integer = 132      ' card height
        Const ICON_SZ As Integer = 80      ' icon preview size
        Const ICON_PAD As Integer = 14     ' padding inside card before icon
        Const BTN_W As Integer = 152       ' button width
        Const BTN_H As Integer = 42        ' button height

        Dim PW = CInt(LOGICAL_WIDTH * 0.88)
        Dim PH = CInt(LOGICAL_HEIGHT * 0.86)
        Dim px = CSng((LOGICAL_WIDTH - PW) / 2)
        Dim py = CSng((LOGICAL_HEIGHT - PH) / 2)

        ' Panel background + border
        Using br As New SolidBrush(Color.FromArgb(248, 10, 10, 32))
            Using rr = RoundedRect(New RectangleF(px, py, PW, PH), 16)
                g.FillPath(br, rr)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(140, 255, 200, 50), 2)
            Using rr = RoundedRect(New RectangleF(px, py, PW, PH), 16)
                g.DrawPath(pen, rr)
            End Using
        End Using

        ' ── Slice layout ────────────────────────────────────────────────────────
        Dim innerX = px + PAD_H
        Dim innerW = PW - PAD_H * 2
        Dim innerTop = py + PAD_V

        ' HeaderRegion
        Dim hdrY = innerTop
        Dim hdrBottom = hdrY + HDR_H

        ' TabRegion
        Dim tabY = hdrBottom + HDR_TAB_GAP
        Dim tabBottom = tabY + TAB_H

        ' FooterRegion (sliced from bottom)
        Dim ftrBottom = py + PH - FTR_PAD
        Dim ftrTop = ftrBottom - FTR_H

        ' ContentRegion (between tabs and footer)
        Dim gridTop = tabBottom + TAB_GRID_GAP
        Dim gridBottom = ftrTop - 8         ' 8 px cushion above footer
        Dim gridH = gridBottom - gridTop
        If gridH < 1 Then gridH = 1

        ' ── Header ──────────────────────────────────────────────────────────────
        Dim titleTxt = If(_devMode,
            ChrW(&H25C6) & " STORE — DEV MODE (unlimited coins) " & ChrW(&H25C6),
            ChrW(&H25C6) & $" STORE  —  Balance: {_coinBalance} coins  " & ChrW(&H25C6))
        Dim titleClr = If(_devMode, Color.FromArgb(100, 255, 100), Color.FromArgb(255, 220, 60))
        ' Fit title font to available width
        Dim titleFontSz = 18.0F
        Do While titleFontSz > 10
            Dim testFont As New Font("Segoe UI", titleFontSz, FontStyle.Bold, GraphicsUnit.Pixel)
            Dim tw = g.MeasureString(titleTxt, testFont).Width
            testFont.Dispose()
            If tw <= innerW - 20 Then Exit Do
            titleFontSz -= 1
        Loop
        Using titleFont As New Font("Segoe UI", titleFontSz, FontStyle.Bold, GraphicsUnit.Pixel)
            Dim tsz = g.MeasureString(titleTxt, titleFont)
            Using tbr As New SolidBrush(titleClr)
                g.DrawString(titleTxt, titleFont, tbr,
                             CSng(innerX + (innerW - tsz.Width) / 2),
                             CSng(hdrY + (HDR_H - tsz.Height) / 2))
            End Using
        End Using

        ' ── Category tabs ───────────────────────────────────────────────────────
        Dim categories() As StoreCategory = {StoreCategory.Balls, StoreCategory.Bricks, StoreCategory.Bonuses}
        Dim catLabels() As String = {ChrW(&H25CF) & " BALLS", ChrW(&H25A0) & " BRICKS", ChrW(&H25C6) & " BONUSES"}
        Const TAB_COUNT As Integer = 3
        Dim totalTabW = CInt(innerW * 0.88)   ' tabs span 88% of inner width
        Dim tabW = (totalTabW - (TAB_COUNT - 1) * 8) \ TAB_COUNT
        Dim tabsLeft = CSng(innerX + (innerW - totalTabW) / 2)
        For ti = 0 To TAB_COUNT - 1
            Dim tx = tabsLeft + ti * (tabW + 8)
            Dim isActive = (_storeCategory = categories(ti))
            Dim tabFill As Color = If(isActive, Color.FromArgb(220, 255, 200, 50), Color.FromArgb(50, 120, 120, 180))
            Using tbr As New SolidBrush(tabFill)
                Using rr = RoundedRect(New RectangleF(tx, tabY, tabW, TAB_H), 8)
                    g.FillPath(tbr, rr)
                End Using
            End Using
            If isActive Then
                Using pen As New Pen(Color.FromArgb(180, 255, 220, 60), 2)
                    Using rr = RoundedRect(New RectangleF(tx, tabY, tabW, TAB_H), 8)
                        g.DrawPath(pen, rr)
                    End Using
                End Using
            End If
            Dim tc = If(isActive, Color.FromArgb(20, 20, 20), Color.FromArgb(200, 200, 230))
            Dim fnt = If(isActive, _fnt14b, _fnt13b)
            Dim tsz2 = g.MeasureString(catLabels(ti), fnt)
            Using tbr As New SolidBrush(tc)
                g.DrawString(catLabels(ti), fnt, tbr,
                             CSng(tx + (tabW - tsz2.Width) / 2),
                             CSng(tabY + (TAB_H - tsz2.Height) / 2))
            End Using
        Next

        ' ── Content area clip ───────────────────────────────────────────────────
        ' Reserve scrollbar gutter on the right
        Dim sbGutter = SB_W + SB_GAP
        Dim gridW = CInt(innerW - sbGutter)

        ' Compute column count (1 or 2)
        Dim cols = If(gridW >= 640, 2, 1)
        Dim cardW = (gridW - (cols - 1) * COL_GAP) \ cols

        ' Visible rows that fit inside gridH
        Dim rowsVisible = Math.Max(1, CInt(Math.Floor((gridH + ROW_GAP) / (CARD_H + ROW_GAP))))

        Dim catItems = _storeItems.Where(Function(it) it.Category = _storeCategory).ToList()
        Dim totalRows = CInt(Math.Ceiling(catItems.Count / CDbl(cols)))
        Dim maxScroll = Math.Max(0, totalRows - rowsVisible)
        _storeScrollOffset = Math.Max(0, Math.Min(_storeScrollOffset, maxScroll))

        Dim clipRect As New RectangleF(CSng(innerX) - 2, gridTop - 2, gridW + sbGutter + 4, gridH + 4)
        g.SetClip(clipRect)

        ' ── Card grid ───────────────────────────────────────────────────────────
        For rowOffset = 0 To rowsVisible - 1
            Dim rowIdx = _storeScrollOffset + rowOffset
            For col = 0 To cols - 1
                Dim itemIdx = rowIdx * cols + col
                If itemIdx >= catItems.Count Then Continue For

                Dim item = catItems(itemIdx)
                Dim owned = IsOwned(item.Category, item.Id)
                Dim equipped = (item.Category = StoreCategory.Balls AndAlso _activeBallSkin = item.Id) OrElse
                               (item.Category = StoreCategory.Bricks AndAlso _activeBrickPalette = item.Id) OrElse
                               (item.Category = StoreCategory.Bonuses AndAlso _activeBonusPack = item.Id)
                Dim isSelected = (_storeSelectedIndex = itemIdx)

                Dim cx = CSng(innerX + col * (cardW + COL_GAP))
                Dim cardY = CSng(gridTop + rowOffset * (CARD_H + ROW_GAP))

                ' Card background
                Dim cardBg As Color
                If equipped Then
                    cardBg = Color.FromArgb(55, 60, 255, 80)
                ElseIf owned Then
                    cardBg = Color.FromArgb(45, 60, 160, 255)
                ElseIf isSelected Then
                    cardBg = Color.FromArgb(50, 255, 220, 60)
                Else
                    cardBg = Color.FromArgb(30, 200, 200, 255)
                End If
                Using br As New SolidBrush(cardBg)
                    Using rr = RoundedRect(New RectangleF(cx, cardY, cardW, CARD_H), 10)
                        g.FillPath(br, rr)
                    End Using
                End Using
                If isSelected OrElse equipped Then
                    Dim borderClr = If(equipped, Color.FromArgb(200, 80, 255, 100), Color.FromArgb(200, 255, 220, 60))
                    Using pen As New Pen(borderClr, If(equipped, 2.5F, 2.0F))
                        Using rr = RoundedRect(New RectangleF(cx, cardY, cardW, CARD_H), 10)
                            g.DrawPath(pen, rr)
                        End Using
                    End Using
                End If

                ' ── Preview icon (left side) ───────────────────────────────────
                Dim pvX = cx + ICON_PAD
                Dim pvY = cardY + (CARD_H - ICON_SZ) / 2
                Dim pvRect As New RectangleF(pvX, pvY, ICON_SZ, ICON_SZ)
                Using br As New SolidBrush(Color.FromArgb(80, 0, 0, 20))
                    Using rr = RoundedRect(pvRect, 8)
                        g.FillPath(br, rr)
                    End Using
                End Using
                Using pen As New Pen(Color.FromArgb(60, 255, 255, 255), 1)
                    Using rr = RoundedRect(pvRect, 8)
                        g.DrawPath(pen, rr)
                    End Using
                End Using
                Dim pvCx = pvX + ICON_SZ / 2
                Dim pvCy = pvY + ICON_SZ / 2

                Select Case item.Category
                    Case StoreCategory.Balls
                        Dim cols2 = GetBallPreviewColors(item.Id)
                        Dim bRad = ICON_SZ * 0.38F
                        Using br As New SolidBrush(Color.FromArgb(40, cols2(0)))
                            g.FillEllipse(br, CSng(pvCx - bRad * 1.4F), CSng(pvCy - bRad * 1.4F), bRad * 2.8F, bRad * 2.8F)
                        End Using
                        Using grd As New Drawing2D.LinearGradientBrush(
                                New PointF(pvCx - bRad, pvCy - bRad),
                                New PointF(pvCx + bRad, pvCy + bRad),
                                cols2(0), cols2(1))
                            g.FillEllipse(grd, CSng(pvCx - bRad), CSng(pvCy - bRad), bRad * 2, bRad * 2)
                        End Using
                        Using br As New SolidBrush(Color.FromArgb(140, 255, 255, 255))
                            g.FillEllipse(br, CSng(pvCx - bRad * 0.55F), CSng(pvCy - bRad * 0.55F), bRad * 0.5F, bRad * 0.4F)
                        End Using

                    Case StoreCategory.Bricks
                        Dim sample = GetBrickPaletteSample(item.Id)
                        Dim bw2 = CSng(ICON_SZ - 10) / 2
                        For ri = 0 To 1
                            For ci2 = 0 To 1
                                Dim sampleIdx = ri * 2 + ci2
                                Dim bx2 = pvX + 5 + ci2 * (bw2 + 3)
                                Dim by2 = pvY + (ICON_SZ - bw2 * 2 - 3) / 2 + ri * (bw2 + 3)
                                Dim c1 = If(sampleIdx < sample.Length, sample(sampleIdx), sample(0))
                                Dim c2 = Color.FromArgb(Math.Min(255, c1.R + 40), Math.Min(255, c1.G + 40), Math.Min(255, c1.B + 40))
                                Using grd As New Drawing2D.LinearGradientBrush(
                                        New PointF(bx2, by2), New PointF(bx2, by2 + bw2), c1, c2)
                                    Using rr2 = RoundedRect(New RectangleF(bx2, by2, bw2, bw2), 3)
                                        g.FillPath(grd, rr2)
                                    End Using
                                End Using
                                Using br As New SolidBrush(Color.FromArgb(60, 255, 255, 255))
                                    g.FillRectangle(br, CSng(bx2 + 2), CSng(by2 + 2), bw2 - 4, bw2 * 0.3F)
                                End Using
                            Next
                        Next

                    Case StoreCategory.Bonuses
                        Dim bp = GetBonusPreview(item.Id)
                        Dim iconFont As New Font("Segoe UI", 26, FontStyle.Bold, GraphicsUnit.Pixel)
                        Dim iconSz = g.MeasureString(bp.Symbol, iconFont)
                        Using br As New SolidBrush(bp.Clr)
                            g.DrawString(bp.Symbol, iconFont, br,
                                         CSng(pvCx - iconSz.Width / 2),
                                         CSng(pvCy - iconSz.Height / 2))
                        End Using
                        iconFont.Dispose()
                End Select

                ' ── Text area ─────────────────────────────────────────────────
                ' Text starts after icon; ends before button area
                Dim txtX = CSng(pvX + ICON_SZ + 16)
                Dim btnAreaW = BTN_W + 12          ' reserved right zone for button
                Dim txtW = CSng(cardW - ICON_SZ - ICON_PAD - 16 - btnAreaW)
                Dim nameClr = If(equipped, Color.FromArgb(100, 255, 100),
                               If(owned, Color.White, Color.FromArgb(210, 210, 220)))
                Using br As New SolidBrush(nameClr)
                    Using sf As New StringFormat()
                        sf.Trimming = StringTrimming.EllipsisCharacter
                        sf.FormatFlags = StringFormatFlags.NoWrap
                        g.DrawString(item.Name, _fnt14b, br,
                                     New RectangleF(txtX, cardY + 8, txtW, 34), sf)
                    End Using
                End Using
                Using br As New SolidBrush(Color.FromArgb(155, 170, 195))
                    Using sf As New StringFormat()
                        sf.Trimming = StringTrimming.EllipsisWord
                        g.DrawString(item.Description, _fnt11r, br,
                                     New RectangleF(txtX, cardY + 46, txtW, 60), sf)
                    End Using
                End Using

                ' ── Buy/Equip button (vertically centered right side) ──────────
                Dim btnX = CSng(cx + cardW - BTN_W - 12)
                Dim btnY = CSng(cardY + (CARD_H - BTN_H) / 2)
                Dim badgeText As String
                Dim badgeClr As Color
                If equipped Then
                    badgeText = ChrW(&H2713) & " EQUIPPED" : badgeClr = Color.FromArgb(80, 220, 80)
                ElseIf owned Then
                    badgeText = ChrW(&H25B6) & " EQUIP" : badgeClr = Color.FromArgb(100, 200, 255)
                ElseIf _devMode OrElse _coinBalance >= item.Price Then
                    badgeText = $"BUY  {item.Price}" & ChrW(&H25C6) : badgeClr = Color.FromArgb(255, 220, 60)
                Else
                    badgeText = $"{item.Price}" & ChrW(&H25C6) & " LOCKED" : badgeClr = Color.FromArgb(180, 80, 80)
                End If
                Using br As New SolidBrush(Color.FromArgb(90, badgeClr))
                    Using rr = RoundedRect(New RectangleF(btnX, btnY, BTN_W, BTN_H), 6)
                        g.FillPath(br, rr)
                    End Using
                End Using
                Using pen As New Pen(Color.FromArgb(160, badgeClr), 1.5F)
                    Using rr = RoundedRect(New RectangleF(btnX, btnY, BTN_W, BTN_H), 6)
                        g.DrawPath(pen, rr)
                    End Using
                End Using
                Dim bsz = g.MeasureString(badgeText, _fnt12b)
                Using tbr As New SolidBrush(badgeClr)
                    g.DrawString(badgeText, _fnt12b, tbr,
                                 CSng(btnX + (BTN_W - bsz.Width) / 2),
                                 CSng(btnY + (BTN_H - bsz.Height) / 2))
                End Using
            Next
        Next

        g.ResetClip()

        ' ── Scrollbar ───────────────────────────────────────────────────────────
        If totalRows > rowsVisible Then
            Dim sbX = CSng(innerX + gridW + SB_GAP)
            Dim sbH = CSng(gridH)
            Dim sbY = CSng(gridTop)
            ' Track
            Using br As New SolidBrush(Color.FromArgb(40, 200, 200, 255))
                Using rr = RoundedRect(New RectangleF(sbX, sbY, SB_W, sbH), 4)
                    g.FillPath(br, rr)
                End Using
            End Using
            ' Thumb
            Dim thumbH = Math.Max(20, sbH * rowsVisible / totalRows)
            Dim thumbY = sbY + (sbH - thumbH) * _storeScrollOffset / Math.Max(1, maxScroll)
            Using br As New SolidBrush(Color.FromArgb(180, 255, 200, 60))
                Using rr = RoundedRect(New RectangleF(sbX, CSng(thumbY), SB_W, CSng(thumbH)), 4)
                    g.FillPath(br, rr)
                End Using
            End Using
            ' Arrows
            Using tbr As New SolidBrush(Color.FromArgb(180, 255, 220, 60))
                If _storeScrollOffset > 0 Then
                    g.DrawString(ChrW(&H25B2), _fnt12b, tbr, sbX, sbY - 18)
                End If
                If _storeScrollOffset < maxScroll Then
                    g.DrawString(ChrW(&H25BC), _fnt12b, tbr, sbX, CSng(sbY + sbH + 2))
                End If
            End Using
        End If

        ' ── Footer ──────────────────────────────────────────────────────────────
        ' "← MENU" button on the left of the footer
        Const MENU_BTN_W As Integer = 110
        Const MENU_BTN_H As Integer = 32
        Dim menuBtnX = CSng(innerX)
        Dim menuBtnY = CSng(ftrTop + (FTR_H - MENU_BTN_H) / 2)
        Using menuBg As New SolidBrush(Color.FromArgb(180, 70, 80, 160))
            Using rr = RoundedRect(New RectangleF(menuBtnX, menuBtnY, MENU_BTN_W, MENU_BTN_H), 8)
                g.FillPath(menuBg, rr)
            End Using
        End Using
        Using menuPen As New Pen(Color.FromArgb(200, 120, 140, 255), 1.5F)
            Using rr = RoundedRect(New RectangleF(menuBtnX, menuBtnY, MENU_BTN_W, MENU_BTN_H), 8)
                g.DrawPath(menuPen, rr)
            End Using
        End Using
        Using menuFnt As New Font("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Pixel)
            Dim lbl = ChrW(&H2190) & " MENU"
            Dim lsz = g.MeasureString(lbl, menuFnt)
            Using mbr As New SolidBrush(Color.FromArgb(220, 200, 220, 255))
                g.DrawString(lbl, menuFnt, mbr,
                             CSng(menuBtnX + (MENU_BTN_W - lsz.Width) / 2),
                             CSng(menuBtnY + (MENU_BTN_H - lsz.Height) / 2))
            End Using
        End Using

        Dim footerTxt = ChrW(&H2191) & ChrW(&H2193) & " Navigate   ENTER Buy/Equip   " &
                        ChrW(&H2190) & ChrW(&H2192) & " Category   Scroll wheel   ESC Close"
        Dim ftrFontSz = 11.0F
        Dim ftrAvailW = innerW - MENU_BTN_W - 16
        Do While ftrFontSz > 8
            Dim tf As New Font("Segoe UI", ftrFontSz, GraphicsUnit.Pixel)
            If g.MeasureString(footerTxt, tf).Width <= ftrAvailW Then tf.Dispose() : Exit Do
            tf.Dispose() : ftrFontSz -= 0.5F
        Loop
        Using ftrFont As New Font("Segoe UI", ftrFontSz, GraphicsUnit.Pixel)
            Dim fsz = g.MeasureString(footerTxt, ftrFont)
            Using fbr As New SolidBrush(Color.FromArgb(120, 130, 155))
                g.DrawString(footerTxt, ftrFont, fbr,
                             CSng(innerX + MENU_BTN_W + 16 + (ftrAvailW - fsz.Width) / 2),
                             CSng(ftrTop + (FTR_H - fsz.Height) / 2))
            End Using
        End Using
    End Sub


        Private Sub DrawGameOverScreen(g As Graphics)
        ' Semi-transparent dark overlay over frozen game
        Using br As New SolidBrush(Color.FromArgb(185, 0, 0, 10))
            g.FillRectangle(br, 0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT)
        End Using
        Dim pw = 480, ph = 360
        Dim px = CSng((LOGICAL_WIDTH - pw) / 2), py = CSng((LOGICAL_HEIGHT - ph) / 2)
        Using br As New SolidBrush(Color.FromArgb(245, 16, 8, 30))
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 16)
                g.FillPath(br, rr)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(160, 220, 60, 60), 2)
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 16)
                g.DrawPath(pen, rr)
            End Using
        End Using
        Dim cy = py + 30
        DrawCenteredText(g, "GAME OVER", _fnt30b, Color.FromArgb(255, 90, 90), cy) : cy += 50
        DrawCenteredText(g, $"Score: {_score}", _fnt18b, Color.FromArgb(255, 240, 100), cy) : cy += 36
        Dim coinsEarned = CInt(_score / 20)
        DrawCenteredText(g, $"Coins Earned: {coinsEarned}", _fnt16r, Color.FromArgb(80, 220, 180), cy) : cy += 36
        DrawCenteredText(g, $"Level Reached: {_level}", _fnt16r, Color.FromArgb(160, 180, 220), cy) : cy += 46
        ' Three action buttons
        Dim btnW = 120, btnH = 36, gap = 18
        Dim totalW = btnW * 3 + gap * 2
        Dim bx = px + (pw - totalW) / 2
        Dim by = CSng(cy)
        Dim labels() As String = {"Retry", "Store", "Menu"}
        Dim btnColors() As Color = {Color.FromArgb(220, 70, 70), Color.FromArgb(60, 160, 220), Color.FromArgb(80, 80, 110)}
        For i = 0 To 2
            Dim rx = bx + i * (btnW + gap)
            Using br2 As New SolidBrush(btnColors(i))
                Using rr2 = RoundedRect(New RectangleF(rx, by, btnW, btnH), 8)
                    g.FillPath(br2, rr2)
                End Using
            End Using
            Using sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                Using brT As New SolidBrush(Color.White)
                    g.DrawString(labels(i), _fnt14b, brT, New RectangleF(rx, by, btnW, btnH), sf)
                End Using
            End Using
        Next
        If _highScoreDelayFrames > 0 Then
            DrawCenteredText(g, $"Auto-advancing in {CInt(Math.Ceiling(_highScoreDelayFrames / 60.0))}s...", _fnt11r, Color.FromArgb(90, 90, 110), by + btnH + 12)
        End If
        DrawCenteredText(g, "[R] Retry   [S] Store   [Esc] Menu", _fnt10r, Color.FromArgb(70, 80, 100), by + btnH + 30)
    End Sub

    Private Sub DrawCredits(g As Graphics)
        DrawStarField(g)
        Using br As New SolidBrush(Color.FromArgb(210, 0, 0, 20))
            g.FillRectangle(br, 0, 0, LOGICAL_WIDTH, LOGICAL_HEIGHT)
        End Using
        Dim pw = 700, ph = 560
        Dim px = CSng((LOGICAL_WIDTH - pw) / 2), py = CSng((LOGICAL_HEIGHT - ph) / 2)
        Using br As New SolidBrush(Color.FromArgb(245, 10, 10, 35))
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.FillPath(br, rr)
            End Using
        End Using
        Using pen As New Pen(Color.FromArgb(100, 100, 180, 255), 2)
            Using rr = RoundedRect(New RectangleF(px, py, pw, ph), 14)
                g.DrawPath(pen, rr)
            End Using
        End Using
        Dim cy = py + 22
        DrawCenteredText(g, "CREDITS", _fnt22b, Color.FromArgb(100, 200, 255), cy) : cy += 46
        Dim lx = px + 40
        Using fh As New Font("Segoe UI", 13, FontStyle.Bold)
            Using fb As New Font("Segoe UI", 11, FontStyle.Regular)
                Using brH As New SolidBrush(Color.FromArgb(255, 240, 100))
                    Using brB As New SolidBrush(Color.FromArgb(200, 210, 230))
                        g.DrawString("PROJECT", fh, brH, lx, cy) : cy += 28
                        g.DrawString("  BrickBlast: Velocity Market  —  v1.0.0", fb, brB, lx, cy) : cy += 26
                        g.DrawString("  A 2D Arcade Brick Breaker with Marketplace & Online Sync", fb, brB, lx, cy) : cy += 36

                        g.DrawString("TEAM", fh, brH, lx, cy) : cy += 28
                        g.DrawString("  Curtis Loop          —  Team Lead", fb, brB, lx, cy) : cy += 24
                        g.DrawString("  Alyssa Puentes       —  Co-Lead", fb, brB, lx, cy) : cy += 24
                        g.DrawString("  Andrea Albisser      —  Co-Lead", fb, brB, lx, cy) : cy += 36

                        g.DrawString("TECHNOLOGY", fh, brH, lx, cy) : cy += 28
                        g.DrawString("  Language:   Visual Basic  /  .NET 10", fb, brB, lx, cy) : cy += 24
                        g.DrawString("  Framework:  Windows Forms  (WinForms)", fb, brB, lx, cy) : cy += 24
                        g.DrawString("  Rendering:  GDI+ procedural art", fb, brB, lx, cy) : cy += 24
                        g.DrawString("  Starter:    BrickBlast  (github.com/stuffthings15/BrickBlast)", fb, brB, lx, cy) : cy += 36

                        g.DrawString("COURSE", fh, brH, lx, cy) : cy += 28
                        g.DrawString("  CS-120  —  Game Development Final Project  —  Spring 2026", fb, brB, lx, cy) : cy += 36
                    End Using
                End Using
            End Using
        End Using
        Using fHint As New Font("Segoe UI", 10, FontStyle.Regular)
            DrawCenteredText(g, "ESC or ENTER  —  Back to Main Menu", fHint, Color.FromArgb(130, 130, 155), py + ph - 28)
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

    Private Function ColorFromHSV(hue As Double, saturation As Double, value As Double) As Color
        Dim hi = CInt(Math.Floor(hue / 60)) Mod 6
        Dim f = hue / 60 - Math.Floor(hue / 60)
        Dim v = CInt(value * 255)
        Dim p = CInt(value * (1 - saturation) * 255)
        Dim q = CInt(value * (1 - f * saturation) * 255)
        Dim t = CInt(value * (1 - (1 - f) * saturation) * 255)
        Select Case hi
            Case 0 : Return Color.FromArgb(v, t, p)
            Case 1 : Return Color.FromArgb(q, v, p)
            Case 2 : Return Color.FromArgb(p, v, t)
            Case 3 : Return Color.FromArgb(p, q, v)
            Case 4 : Return Color.FromArgb(t, p, v)
            Case Else : Return Color.FromArgb(v, p, q)
        End Select
    End Function

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

#Region "Networking"
    ' ── NetworkSyncService ──────────────────────────────────────────────────
    ' Posts a lightweight player-profile payload to the configured REST endpoint.
    ' Gameplay is never blocked; failure silently sets _syncStatus = "Failed".

    Private Async Sub SyncProfileAsync()
        If String.IsNullOrWhiteSpace(_playerName) Then Return
        _syncStatus = "Syncing"
        Try
            Dim payload As New Dictionary(Of String, Object) From {
                {"playerId", _playerName},
                {"bestScore", If(_highScores.Count > 0, _highScores(0).PlayerScore, 0)},
                {"currency", _coinBalance},
                {"equippedBall", _activeBallSkin},
                {"equippedBricks", _activeBrickPalette},
                {"equippedBonuses", _activeBonusPack},
                {"purchasedItems", _ownedItems.ToList()},
                {"lastUpdatedUtc", DateTime.UtcNow.ToString("o")}
            }
            Dim json = System.Text.Json.JsonSerializer.Serialize(payload)
            Dim content = New System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json")
            Dim response = Await _httpClient.PostAsync(SyncEndpointUrl, content)
            _syncStatus = If(response.IsSuccessStatusCode, "Synced", "Failed")
            If response.IsSuccessStatusCode Then
                _lastSyncUtc = DateTime.UtcNow
                LogEvent("SyncSucceeded", $"player={_playerName}")
            Else
                LogEvent("SyncFailed", $"player={_playerName} status={response.StatusCode}")
            End If
        Catch ex As Exception
            _syncStatus = "Failed"
            LogEvent("SyncFailed", $"player={_playerName} ex={ex.Message}")
        End Try
        Invalidate()
    End Sub

    Private Async Function CheckConnectivityAsync() As Task(Of Boolean)
        Try
            Dim ping = Await _httpClient.GetAsync(SyncEndpointUrl & "/ping")
            Return ping.IsSuccessStatusCode
        Catch
            Return False
        End Try
    End Function

    Private Function GetSyncLabel() As String
        Select Case _syncStatus
            Case "Syncing" : Return "⟳ Syncing"
            Case "Synced"
                If _lastSyncUtc <> DateTime.MinValue Then
                    Return "✓ Synced " & _lastSyncUtc.ToLocalTime().ToString("HH:mm")
                End If
                Return "✓ Synced"
            Case "Failed" : Return "✗ Sync Failed"
            Case Else : Return "● Offline"
        End Select
    End Function
#End Region

#Region "Analytics"
    ' ── AnalyticsLogger ─────────────────────────────────────────────────────────
    ' Lightweight local event logger. Writes timestamped entries to a log file
    ' under %AppData%\BrickBlast\analytics.log for testing evidence and debugging.

    Private ReadOnly _analyticsPath As String = IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BrickBlast", "analytics.log")

    Private Sub LogEvent(eventName As String, Optional detail As String = "")
        Try
            Dim line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {eventName}"
            If Not String.IsNullOrEmpty(detail) Then line &= $"  |  {detail}"
            IO.File.AppendAllText(_analyticsPath, line & Environment.NewLine)
        Catch
            ' Never crash gameplay over a logging failure
        End Try
    End Sub
#End Region

#Region "MarketingExport"
    ' ── MarketingExport ──────────────────────────────────────────────────────────
    ' Press F12 from the Main Menu to export icon.png and titlecard.png to Assets\UI\.
    ' These are GDI+-rendered bitmaps used as submission marketing assets.

    Private Sub ExportMarketingAssets()
        Try
            Dim uiDir = IO.Path.Combine(Application.StartupPath, "..", "..", "..", "..", "..", "..", "Assets", "UI")
            If Not IO.Directory.Exists(uiDir) Then IO.Directory.CreateDirectory(uiDir)

            ' --- 256×256 icon -------------------------------------------------------
            Using bmp As New Bitmap(256, 256)
                Using g = Graphics.FromImage(bmp)
                    g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    g.Clear(Color.FromArgb(18, 8, 35))
                    ' Outer glow ring
                    Using br As New SolidBrush(Color.FromArgb(60, 80, 40, 160))
                        g.FillEllipse(br, 4, 4, 248, 248)
                    End Using
                    Using pen As New Pen(Color.FromArgb(180, 120, 60, 220), 5)
                        g.DrawEllipse(pen, 8, 8, 240, 240)
                    End Using
                    ' Ball
                    Using br2 As New Drawing2D.LinearGradientBrush(New Rectangle(80, 60, 96, 96), Color.FromArgb(255, 80, 200), Color.FromArgb(200, 30, 120), 135)
                        g.FillEllipse(br2, 80, 60, 96, 96)
                    End Using
                    ' Brick row
                    Dim bColors() As Color = {Color.FromArgb(220, 60, 60), Color.FromArgb(60, 180, 220), Color.FromArgb(240, 180, 40), Color.FromArgb(80, 200, 100)}
                    For col = 0 To 3
                        Using br3 As New SolidBrush(bColors(col))
                            g.FillRectangle(br3, 20 + col * 56, 175, 48, 22)
                        End Using
                    Next
                    ' Title text
                    Using fnt As New Font("Segoe UI", 14, FontStyle.Bold)
                        Using brT As New SolidBrush(Color.White)
                            Dim sf As New StringFormat With {.Alignment = StringAlignment.Center}
                            g.DrawString("BRICKBLAST", fnt, brT, New RectangleF(0, 212, 256, 30), sf)
                        End Using
                    End Using
                End Using
                bmp.Save(IO.Path.Combine(uiDir, "icon.png"), Imaging.ImageFormat.Png)
            End Using

            ' --- 1200×630 title card ------------------------------------------------
            Using bmp2 As New Bitmap(1200, 630)
                Using g = Graphics.FromImage(bmp2)
                    g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    ' Background gradient
                    Using bgBr As New Drawing2D.LinearGradientBrush(New Rectangle(0, 0, 1200, 630), Color.FromArgb(10, 5, 30), Color.FromArgb(30, 10, 60), 90)
                        g.FillRectangle(bgBr, 0, 0, 1200, 630)
                    End Using
                    ' Star dots
                    Dim rng As New Random(42)
                    Using starBr As New SolidBrush(Color.FromArgb(120, 255, 255, 255))
                        For i = 0 To 150
                            Dim sx = rng.Next(0, 1200), sy = rng.Next(0, 630), sr = rng.Next(1, 3)
                            g.FillEllipse(starBr, sx, sy, sr, sr)
                        Next
                    End Using
                    ' Glow behind title
                    Using glowBr As New SolidBrush(Color.FromArgb(40, 120, 60, 220))
                        g.FillEllipse(glowBr, 200, 80, 800, 300)
                    End Using
                    ' Title
                    Using fntT As New Font("Segoe UI", 72, FontStyle.Bold)
                        Using brT As New SolidBrush(Color.White)
                            Dim sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                            g.DrawString("BRICKBLAST", fntT, brT, New RectangleF(0, 80, 1200, 180), sf)
                        End Using
                    End Using
                    ' Subtitle
                    Using fntS As New Font("Segoe UI", 24, FontStyle.Regular)
                        Using brS As New SolidBrush(Color.FromArgb(200, 180, 230, 255))
                            Dim sf As New StringFormat With {.Alignment = StringAlignment.Center}
                            g.DrawString("Velocity Market", fntS, brS, New RectangleF(0, 260, 1200, 50), sf)
                        End Using
                    End Using
                    ' Feature tags
                    Dim tags() As String = {"Marketplace", "8 Levels", "Online Sync", "Persistent Profile", "52 Items"}
                    Dim tagColors() As Color = {Color.FromArgb(200, 60, 140, 220), Color.FromArgb(200, 80, 180, 80), Color.FromArgb(200, 220, 140, 40), Color.FromArgb(200, 180, 60, 180), Color.FromArgb(200, 60, 200, 200)}
                    Dim tagX = 60
                    Using fntTag As New Font("Segoe UI", 15, FontStyle.Bold)
                        For i = 0 To tags.Length - 1
                            Using tagBr As New SolidBrush(tagColors(i))
                                Dim tagRect = New RectangleF(tagX, 340, 200, 36)
                                Using rp = RoundedRect(tagRect, 8)
                                    g.FillPath(tagBr, rp)
                                End Using
                                Using brW As New SolidBrush(Color.White)
                                    Dim sf As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                                    g.DrawString(tags(i), fntTag, brW, tagRect, sf)
                                End Using
                            End Using
                            tagX += 220
                        Next
                    End Using
                    ' Bottom bar
                    Using bbBr As New SolidBrush(Color.FromArgb(80, 255, 255, 255))
                        g.FillRectangle(bbBr, 0, 580, 1200, 2)
                    End Using
                    Using fntB As New Font("Segoe UI", 13, FontStyle.Regular)
                        Using brB As New SolidBrush(Color.FromArgb(140, 180, 200, 220))
                            g.DrawString("Visual Basic  /  .NET 10  /  WinForms  |  v1.0.0  |  github.com/stuffthings15/BrickBlast", fntB, brB, 40, 595)
                        End Using
                    End Using
                End Using
                bmp2.Save(IO.Path.Combine(uiDir, "titlecard.png"), Imaging.ImageFormat.Png)
            End Using

            LogEvent("MarketingAssetsExported", "icon.png + titlecard.png")
            MessageBox.Show($"Exported to:{Environment.NewLine}{uiDir}", "Marketing Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
#End Region

End Class
