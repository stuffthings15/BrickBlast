' =============================================================================
' GAME ENTITY — Brick Blast WPF
' Base class for all interactive game objects (player, enemies, items).
' Provides position, velocity, bounds, collision, and sprite rendering.
' =============================================================================
Imports System.Windows
Imports System.Windows.Media

Namespace BrickBlastWPF
Public Class GameEntity

    Public Property X As Single = 0
    Public Property Y As Single = 0
    Public Property VelX As Single = 0
    Public Property VelY As Single = 0
    Public Property Width As Single = 32
    Public Property Height As Single = 32
    Public Property Health As Integer = 1
    Public Property MaxHealth As Integer = 1
    Public Property Active As Boolean = True
    Public Property EntityType As String = "entity"
    Public Property SpriteKey As String = ""
    Public Property SpriteColor As Color = Colors.White
    Public Property Points As Integer = 50
    Public Property DropItem As String = "gem"

    ' ── Axis-aligned bounding box (centered on X,Y) ──
    Public Function GetBounds() As Rect
        Return New Rect(X - Width / 2, Y - Height / 2, Width, Height)
    End Function

    ' ── AABB intersection test ──
    Public Function Intersects(other As GameEntity) As Boolean
        Return GetBounds().IntersectsWith(other.GetBounds())
    End Function

    ' ── Circle vs AABB collision (for ball hits) ──
    Public Function CircleIntersects(bx As Single, by As Single, radius As Integer) As Boolean
        Dim bounds = GetBounds()
        Dim cx = Math.Max(bounds.Left, Math.Min(CDbl(bx), bounds.Right))
        Dim cy = Math.Max(bounds.Top, Math.Min(CDbl(by), bounds.Bottom))
        Dim dx = bx - cx, dy = by - cy
        Return (dx * dx + dy * dy) <= (radius * radius)
    End Function

    ' ── Per-frame update (override in subclasses) ──
    Public Overridable Sub Update()
        X += VelX
        Y += VelY
    End Sub

    ' ── Render with sprite or fallback color ──
    Public Overridable Sub Render(dc As DrawingContext, mgr As AssetManager)
        Dim sprite = mgr.GetSprite(SpriteKey)
        If sprite IsNot Nothing Then
            dc.DrawImage(sprite, GetBounds())
        Else
            dc.DrawEllipse(New SolidColorBrush(SpriteColor), Nothing,
                New Point(X, Y), Width / 2, Height / 2)
        End If
    End Sub

End Class
End Namespace
