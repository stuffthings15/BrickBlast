' =============================================================================
' TILE MAP — Brick Blast WPF
' Grid-based level background system using tile sprites.
' Supports procedural pattern generation per level.
'
' Usage:
'   Dim map As New TileMap(24, 17, 50, 51)
'   map.LoadTileSprites(assetManager)
'   map.LoadFromPattern(level)
'   map.Render(dc, 0, 0)
' =============================================================================
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Namespace BrickBlastWPF
Public Class TileMap

    Private _tiles(,) As Integer
    Private ReadOnly _width As Integer
    Private ReadOnly _height As Integer
    Private ReadOnly _tileWidth As Integer
    Private ReadOnly _tileHeight As Integer
    Private _tileSprites As BitmapSource()

    Public Sub New(width As Integer, height As Integer, tileWidth As Integer, tileHeight As Integer)
        _width = width
        _height = height
        _tileWidth = tileWidth
        _tileHeight = tileHeight
        ReDim _tiles(width - 1, height - 1)
    End Sub

    ''' <summary>
    ''' Load tile images from the AssetManager (keys: Tiles/tile_0 .. tile_7).
    ''' </summary>
    Public Sub LoadTileSprites(mgr As AssetManager)
        ' Real tile sprites: platform_0..5 then decor_brick_0..1; fallback to procedural tile_0..7
        Dim tileKeys = {"Tiles/platform_0", "Tiles/platform_1", "Tiles/platform_2",
                        "Tiles/platform_3", "Tiles/platform_4", "Tiles/platform_5",
                        "Tiles/decor_brick_0", "Tiles/decor_brick_1"}
        Dim sprites As New List(Of BitmapSource)
        For i = 0 To tileKeys.Length - 1
            Dim s = mgr.GetSprite(tileKeys(i))
            If s Is Nothing Then s = mgr.GetSprite($"Tiles/tile_{i}")
            sprites.Add(s)
        Next
        _tileSprites = sprites.ToArray()
    End Sub

    Public Sub SetTile(x As Integer, y As Integer, tileId As Integer)
        If x >= 0 AndAlso x < _width AndAlso y >= 0 AndAlso y < _height Then
            _tiles(x, y) = tileId
        End If
    End Sub

    Public Function GetTile(x As Integer, y As Integer) As Integer
        If x >= 0 AndAlso x < _width AndAlso y >= 0 AndAlso y < _height Then Return _tiles(x, y)
        Return 0
    End Function

    ''' <summary>
    ''' Procedurally fill the tile grid based on the current level number.
    ''' Each pattern creates a distinct visual atmosphere.
    ''' </summary>
    Public Sub LoadFromPattern(level As Integer)
        Dim pattern = (level - 1) Mod 6
        Dim rng As New Random(level * 31)
        For ty = 0 To _height - 1
            For tx = 0 To _width - 1
                Select Case pattern
                    Case 0 ' Deep space — mostly dark, sparse highlights
                        _tiles(tx, ty) = If(rng.Next(10) < 2, rng.Next(1, 4), 0)
                    Case 1 ' Nebula — warm gradient blending
                        _tiles(tx, ty) = If((tx + ty) Mod 3 = 0, rng.Next(3, 6), rng.Next(0, 3))
                    Case 2 ' Grid station — structural grid lines
                        _tiles(tx, ty) = If(tx Mod 4 = 0 OrElse ty Mod 4 = 0, 6, rng.Next(0, 2))
                    Case 3 ' Asteroid field — high variation
                        _tiles(tx, ty) = rng.Next(0, 8)
                    Case 4 ' Corridor — walls on sides
                        _tiles(tx, ty) = If(tx < 3 OrElse tx >= _width - 3, 5, rng.Next(0, 3))
                    Case Else ' Radial — distance-based pattern
                        Dim dist = Math.Sqrt((tx - _width / 2.0) ^ 2 + (ty - _height / 2.0) ^ 2)
                        _tiles(tx, ty) = CInt(dist) Mod 8
                End Select
            Next
        Next
    End Sub

    ''' <summary>
    ''' Render the entire tilemap at the given pixel offset.
    ''' Called before all game elements for background layer.
    ''' </summary>
    Public Sub Render(dc As DrawingContext, offsetX As Single, offsetY As Single)
        If _tileSprites Is Nothing OrElse _tileSprites.Length = 0 Then Return
        For ty = 0 To _height - 1
            For tx = 0 To _width - 1
                Dim tid = _tiles(tx, ty)
                If tid >= 0 AndAlso tid < _tileSprites.Length AndAlso _tileSprites(tid) IsNot Nothing Then
                    dc.DrawImage(_tileSprites(tid),
                        New Rect(offsetX + tx * _tileWidth, offsetY + ty * _tileHeight, _tileWidth, _tileHeight))
                End If
            Next
        Next
    End Sub

    Public ReadOnly Property MapWidth As Integer
        Get
            Return _width
        End Get
    End Property

    Public ReadOnly Property MapHeight As Integer
        Get
            Return _height
        End Get
    End Property

End Class
End Namespace
