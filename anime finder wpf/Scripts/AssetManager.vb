' =============================================================================
' ASSET MANAGER — Brick Blast WPF
' Singleton that loads, caches, and serves BitmapSource assets.
' Pipeline: Disk (PNG/JPG) → Cache ← ProceduralAssets (fallback)
'
' Usage:
'   AssetManager.Instance.Initialize(basePath)
'   Dim sprite = AssetManager.Instance.GetSprite("Characters/player_idle")
' =============================================================================
Imports System.IO
Imports System.Windows.Media.Imaging

Namespace BrickBlastWPF
Public Class AssetManager

    Private Shared _instance As AssetManager
    Private ReadOnly _cache As New Dictionary(Of String, BitmapSource)
    Private _basePath As String = ""

    ' ── Singleton accessor ──
    Public Shared ReadOnly Property Instance As AssetManager
        Get
            If _instance Is Nothing Then _instance = New AssetManager()
            Return _instance
        End Get
    End Property

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Set the root folder for on-disk asset resolution.
    ''' Typically: AppDomain.CurrentDomain.BaseDirectory + "Assets"
    ''' </summary>
    Public Sub Initialize(basePath As String)
        _basePath = basePath
    End Sub

    ''' <summary>
    ''' Register a programmatically generated asset into the cache.
    ''' Called by ProceduralAssets.RegisterDefaults().
    ''' </summary>
    Public Sub RegisterGenerated(key As String, img As BitmapSource)
        _cache(key) = img
    End Sub

    ''' <summary>
    ''' Primary retrieval method. Checks cache first, then disk, returns Nothing if absent.
    ''' Disk assets override procedural assets when present.
    ''' </summary>
    Public Function GetSprite(key As String) As BitmapSource
        If String.IsNullOrEmpty(key) Then Return Nothing
        ' Check disk first (allows real assets to override procedural)
        Dim diskPath = TryResolvePath(key)
        If diskPath IsNot Nothing Then
            Dim diskKey = "disk:" & key
            If Not _cache.ContainsKey(diskKey) Then
                Try
                    Dim bmp As New BitmapImage()
                    bmp.BeginInit()
                    bmp.UriSource = New Uri(diskPath, UriKind.Absolute)
                    bmp.CacheOption = BitmapCacheOption.OnLoad
                    bmp.EndInit()
                    bmp.Freeze()
                    _cache(diskKey) = bmp
                Catch
                End Try
            End If
            If _cache.ContainsKey(diskKey) Then Return _cache(diskKey)
        End If
        ' Fall back to procedural cache
        If _cache.ContainsKey(key) Then Return _cache(key)
        Return Nothing
    End Function

    ''' <summary>
    ''' Returns True if the key resolves to either a cached or disk asset.
    ''' </summary>
    Public Function AssetExists(key As String) As Boolean
        Return _cache.ContainsKey(key) OrElse TryResolvePath(key) IsNot Nothing
    End Function

    ''' <summary>
    ''' Enumerate all cached keys (for debug / inventory listing).
    ''' </summary>
    Public Function GetAllKeys() As IEnumerable(Of String)
        Return _cache.Keys
    End Function

    ' ── Disk resolution: tries .png, .jpg, .bmp, .gif ──
    Private Function TryResolvePath(key As String) As String
        If String.IsNullOrEmpty(_basePath) Then Return Nothing
        Dim exts = {".png", ".jpg", ".bmp", ".gif"}
        Dim normalized = key.Replace("/"c, Path.DirectorySeparatorChar)
        For Each ext In exts
            Dim p = Path.Combine(_basePath, normalized & ext)
            If File.Exists(p) Then Return p
        Next
        Dim direct = Path.Combine(_basePath, normalized)
        If File.Exists(direct) Then Return direct
        Return Nothing
    End Function

End Class
End Namespace
