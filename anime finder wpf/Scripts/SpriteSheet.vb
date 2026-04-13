' =============================================================================
' SPRITE SHEET — Brick Blast WPF
' Slices a single BitmapSource into uniform frames or named regions.
' Supports sprite atlas packing for characters, tiles, and UI.
'
' Usage:
'   Dim sheet As New SpriteSheet(image, 32, 32)
'   Dim frame = sheet.GetFrame(0)
'   sheet.DefineRegion("attack", 64, 0, 32, 32)
' =============================================================================
Imports System.Windows
Imports System.Windows.Media.Imaging

Namespace BrickBlastWPF
Public Class SpriteSheet

    Private ReadOnly _source As BitmapSource
    Private ReadOnly _frameWidth As Integer
    Private ReadOnly _frameHeight As Integer
    Private ReadOnly _columns As Integer
    Private ReadOnly _rows As Integer
    Private ReadOnly _frames As New List(Of BitmapSource)
    Private ReadOnly _namedRegions As New Dictionary(Of String, BitmapSource)

    ''' <summary>
    ''' Construct from a source image and uniform frame dimensions.
    ''' Automatically slices the entire sheet into a grid.
    ''' </summary>
    Public Sub New(source As BitmapSource, frameWidth As Integer, frameHeight As Integer)
        _source = source
        _frameWidth = frameWidth
        _frameHeight = frameHeight
        _columns = CInt(Math.Floor(source.PixelWidth / frameWidth))
        _rows = CInt(Math.Floor(source.PixelHeight / frameHeight))
        BuildFrames()
    End Sub

    Private Sub BuildFrames()
        For row = 0 To _rows - 1
            For col = 0 To _columns - 1
                Dim region = New Int32Rect(col * _frameWidth, row * _frameHeight, _frameWidth, _frameHeight)
                Dim cropped As New CroppedBitmap(_source, region)
                cropped.Freeze()
                _frames.Add(cropped)
            Next
        Next
    End Sub

    ' ── Frame access ──
    Public ReadOnly Property FrameCount As Integer
        Get
            Return _frames.Count
        End Get
    End Property

    Public Function GetFrame(index As Integer) As BitmapSource
        If index < 0 OrElse index >= _frames.Count Then Return Nothing
        Return _frames(index)
    End Function

    ' ── Named region access (for non-uniform sprite sheets) ──
    Public Sub DefineRegion(name As String, x As Integer, y As Integer, w As Integer, h As Integer)
        If x + w > _source.PixelWidth OrElse y + h > _source.PixelHeight Then Return
        Dim cropped As New CroppedBitmap(_source, New Int32Rect(x, y, w, h))
        cropped.Freeze()
        _namedRegions(name) = cropped
    End Sub

    Public Function GetRegion(name As String) As BitmapSource
        If _namedRegions.ContainsKey(name) Then Return _namedRegions(name)
        Return Nothing
    End Function

    ' ── Grid info ──
    Public ReadOnly Property Columns As Integer
        Get
            Return _columns
        End Get
    End Property

    Public ReadOnly Property Rows As Integer
        Get
            Return _rows
        End Get
    End Property

End Class
End Namespace
