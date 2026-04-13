' =============================================================================
' ENEMY AI SYSTEM — Brick Blast WPF
' Enemy behaviors: Patrol, Chase, Bounce, Descend, Orbit.
' EnemyManager handles spawning, updating, rendering, and collision.
'
' Integration:
'   _enemyMgr.SpawnWave(level, LOGICAL_WIDTH, LOGICAL_HEIGHT)
'   _enemyMgr.Update(paddleX, paddleY)
'   _enemyMgr.Render(dc, assetMgr, frameCount, colorblindMode)
'   Dim hit = _enemyMgr.CheckBallCollision(bx, by, radius)
' =============================================================================
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Media

Namespace BrickBlastWPF

Public Enum EnemyBehavior
    Patrol
    Chase
    Bounce
    Descend
    Orbit
End Enum

Public Class Enemy
    Inherits GameEntity

    Public Property Behavior As EnemyBehavior = EnemyBehavior.Patrol
    Public Property PatrolMinX As Single = 0
    Public Property PatrolMaxX As Single = 1200
    Public Property OrbitCenterX As Single = 600
    Public Property OrbitCenterY As Single = 200
    Public Property OrbitRadius As Single = 80
    Public Property OrbitAngle As Single = 0
    Public Property OrbitSpeed As Single = 0.03F
    Public Property TargetX As Single = 600
    Public Property TargetY As Single = 400

    Public Overrides Sub Update()
        If Not Active Then Return
        Select Case Behavior
            Case EnemyBehavior.Patrol
                X += VelX
                If X <= PatrolMinX OrElse X >= PatrolMaxX Then VelX = -VelX

            Case EnemyBehavior.Chase
                Dim dx = TargetX - X, dy = TargetY - Y
                Dim dist = CSng(Math.Sqrt(dx * dx + dy * dy))
                If dist > 2 Then
                    Dim spd = CSng(Math.Sqrt(VelX * VelX + VelY * VelY))
                    If spd < 0.5F Then spd = 1.0F
                    X += CSng(dx / dist * spd * 0.3)
                    Y += CSng(dy / dist * spd * 0.3)
                End If

            Case EnemyBehavior.Bounce
                X += VelX : Y += VelY
                If X <= PatrolMinX OrElse X >= PatrolMaxX Then VelX = -VelX
                If Y <= 60 OrElse Y >= 400 Then VelY = -VelY

            Case EnemyBehavior.Descend
                Y += Math.Abs(VelY)
                X += CSng(Math.Sin(Y * 0.05) * 2)

            Case EnemyBehavior.Orbit
                OrbitAngle += OrbitSpeed
                X = OrbitCenterX + CSng(Math.Cos(OrbitAngle) * OrbitRadius)
                Y = OrbitCenterY + CSng(Math.Sin(OrbitAngle) * OrbitRadius)
        End Select
    End Sub
End Class

' ─────────────────────────────────────────────────────────────────────────────
' ENEMY MANAGER — Spawn waves, update AI, render, and detect ball collisions.
' ─────────────────────────────────────────────────────────────────────────────
Public Class EnemyManager

    Private ReadOnly _enemies As New List(Of Enemy)
    Private ReadOnly _rng As New Random()

    Public ReadOnly Property Enemies As List(Of Enemy)
        Get
            Return _enemies
        End Get
    End Property

    Public ReadOnly Property ActiveCount As Integer
        Get
            Return _enemies.Where(Function(e) e.Active).Count()
        End Get
    End Property

    ''' <summary>
    ''' Spawn a level-appropriate wave of enemies.
    ''' Enemies start appearing from level 2; count and types scale up.
    ''' </summary>
    Public Sub SpawnWave(level As Integer, logicalWidth As Integer, logicalHeight As Integer)
        _enemies.Clear()
        If level < 2 Then Return

        Dim count = Math.Min(12, 1 + CInt(Math.Floor((level - 2) / 2.0)))
        Dim behaviors = {EnemyBehavior.Patrol, EnemyBehavior.Bounce, EnemyBehavior.Chase,
                         EnemyBehavior.Orbit, EnemyBehavior.Descend}
        Dim spriteKeys = {"Characters/enemy_patrol", "Characters/enemy_chase",
                          "Characters/enemy_tank", "Characters/enemy_fast",
                          "Characters/enemy_boss"}
        Dim spriteColors = {Color.FromRgb(255, 80, 80), Color.FromRgb(255, 160, 40),
                            Color.FromRgb(180, 60, 220), Color.FromRgb(255, 220, 50),
                            Color.FromRgb(200, 30, 30)}
        Dim dropItems = {"heart", "star", "gem", "potion", "key"}

        For i = 0 To count - 1
            Dim e As New Enemy()
            Dim bIdx = i Mod behaviors.Length
            ' Gate advanced behaviors behind level progression
            If level < 4 Then bIdx = Math.Min(bIdx, 1)
            If level < 6 Then bIdx = Math.Min(bIdx, 2)

            e.Behavior = behaviors(bIdx)
            e.EntityType = behaviors(bIdx).ToString()
            e.SpriteKey = spriteKeys(bIdx)
            e.SpriteColor = spriteColors(bIdx)
            e.DropItem = dropItems(_rng.Next(dropItems.Length))
            e.Width = If(bIdx = 4, 42, If(bIdx = 2, 30, 24))
            e.Height = e.Width
            e.Health = If(bIdx = 2, 3, If(bIdx = 4, 5, 1))
            e.MaxHealth = e.Health
            e.Points = (bIdx + 1) * 50
            e.Active = True

            ' Position in upper 35% of screen (within brick area)
            e.X = 60 + _rng.Next(logicalWidth - 120)
            e.Y = 80 + _rng.Next(CInt(logicalHeight * 0.35))
            e.VelX = CSng(1.0 + _rng.NextDouble() * 2.0) * If(_rng.Next(2) = 0, 1, -1)
            e.VelY = CSng(0.5 + _rng.NextDouble() * 1.5) * If(_rng.Next(2) = 0, 1, -1)

            e.PatrolMinX = 30 : e.PatrolMaxX = logicalWidth - 30
            e.OrbitCenterX = e.X : e.OrbitCenterY = e.Y
            e.OrbitRadius = 40 + _rng.Next(60)
            e.OrbitAngle = CSng(_rng.NextDouble() * Math.PI * 2)
            e.OrbitSpeed = 0.015F + CSng(_rng.NextDouble() * 0.03)

            _enemies.Add(e)
        Next
    End Sub

    ''' <summary>
    ''' Update all active enemies. Chase enemies track toward paddle position.
    ''' </summary>
    Public Sub Update(paddleX As Single, paddleY As Single)
        For Each e In _enemies
            If Not e.Active Then Continue For
            e.TargetX = paddleX
            e.TargetY = paddleY * 0.4F
            e.Update()
        Next
    End Sub

    ''' <summary>
    ''' Render all enemies with sprite, float animation, health bars, and labels.
    ''' </summary>
    Public Sub Render(dc As DrawingContext, mgr As AssetManager, frameCount As Integer, colorblindMode As Boolean)
        For Each e In _enemies
            If Not e.Active Then Continue For
            Dim floatY = CSng(Math.Sin(frameCount * 0.08 + e.X * 0.1) * 3)
            Dim sprite = mgr.GetSprite(e.SpriteKey)
            Dim bounds = New Rect(e.X - e.Width / 2, e.Y - e.Height / 2 + floatY, e.Width, e.Height)

            If sprite IsNot Nothing Then
                dc.DrawImage(sprite, bounds)
            Else
                dc.DrawEllipse(New SolidColorBrush(e.SpriteColor), Nothing,
                    New Point(e.X, e.Y + floatY), e.Width / 2, e.Height / 2)
            End If

            ' Health bar for multi-hit enemies
            If e.Health < e.MaxHealth AndAlso e.Health > 0 Then
                Dim hbW = CDbl(e.Width), hbH = 3.0
                Dim hbX = e.X - hbW / 2, hbY = e.Y - e.Height / 2 + floatY - 6
                dc.DrawRectangle(New SolidColorBrush(Color.FromArgb(120, 0, 0, 0)), Nothing,
                    New Rect(hbX, hbY, hbW, hbH))
                Dim fill = hbW * e.Health / e.MaxHealth
                dc.DrawRectangle(New SolidColorBrush(Color.FromRgb(80, 255, 80)), Nothing,
                    New Rect(hbX, hbY, fill, hbH))
            End If

            ' Colorblind behavior label
            If colorblindMode Then
                Dim label = New FormattedText(
                    e.EntityType.Substring(0, Math.Min(3, e.EntityType.Length)).ToUpper(),
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    New Typeface("Segoe UI"), 8, Brushes.White, 96)
                dc.DrawText(label, New Point(e.X - label.Width / 2, e.Y + e.Height / 2 + floatY + 2))
            End If
        Next
    End Sub

    ''' <summary>
    ''' Test if a ball at (bx, by) with given radius hits any active enemy.
    ''' Damages the enemy and returns it (or Nothing).
    ''' </summary>
    Public Function CheckBallCollision(bx As Single, by As Single, radius As Integer) As Enemy
        For i = _enemies.Count - 1 To 0 Step -1
            Dim e = _enemies(i)
            If Not e.Active Then Continue For
            If e.CircleIntersects(bx, by, radius) Then
                e.Health -= 1
                If e.Health <= 0 Then e.Active = False
                Return e
            End If
        Next
        Return Nothing
    End Function

    Public Sub Clear()
        _enemies.Clear()
    End Sub

End Class
End Namespace
