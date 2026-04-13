' =============================================================================
' UI MANAGER — Brick Blast WPF
' Manages HUD overlay elements: health bars, progress bars, icon indicators.
' Renders on top of the game layer using asset sprites.
'
' Usage:
'   _uiMgr.AddHealthBar("enemy_progress", 15, 35, 120, 8, Colors.Red)
'   _uiMgr.SetValue("enemy_progress", killed, total)
'   _uiMgr.Render(dc, mgr)
' =============================================================================
Imports System.Windows
Imports System.Windows.Media

Namespace BrickBlastWPF

Public Structure UIBarElement
    Public Name As String
    Public X As Single
    Public Y As Single
    Public Width As Single
    Public Height As Single
    Public FillColor As Color
    Public Value As Single
    Public MaxValue As Single
    Public Visible As Boolean
End Structure

Public Class UIManager

    Private ReadOnly _bars As New Dictionary(Of String, UIBarElement)

    ''' <summary>
    ''' Register a named progress/health bar element.
    ''' </summary>
    Public Sub AddHealthBar(name As String, x As Single, y As Single, w As Single, h As Single, fillColor As Color)
        Dim el As UIBarElement
        el.Name = name : el.X = x : el.Y = y
        el.Width = w : el.Height = h : el.FillColor = fillColor
        el.Visible = True : el.Value = 0 : el.MaxValue = 1
        _bars(name) = el
    End Sub

    Public Sub SetValue(name As String, value As Single, maxValue As Single)
        If Not _bars.ContainsKey(name) Then Return
        Dim el = _bars(name)
        el.Value = value : el.MaxValue = maxValue
        _bars(name) = el
    End Sub

    Public Sub SetVisible(name As String, visible As Boolean)
        If Not _bars.ContainsKey(name) Then Return
        Dim el = _bars(name)
        el.Visible = visible
        _bars(name) = el
    End Sub

    ''' <summary>
    ''' Render all visible UI elements.
    ''' </summary>
    Public Sub Render(dc As DrawingContext, mgr As AssetManager)
        For Each kv In _bars
            Dim el = kv.Value
            If Not el.Visible Then Continue For

            ' Background track
            dc.DrawRoundedRectangle(
                New SolidColorBrush(Color.FromArgb(80, 0, 0, 0)), Nothing,
                New Rect(el.X, el.Y, el.Width, el.Height), 3, 3)

            ' Fill bar
            If el.MaxValue > 0 Then
                Dim fill = el.Width * Math.Max(0, Math.Min(1, el.Value / el.MaxValue))
                If fill > 1 Then
                    dc.DrawRoundedRectangle(
                        New SolidColorBrush(Color.FromArgb(200, el.FillColor.R, el.FillColor.G, el.FillColor.B)),
                        Nothing, New Rect(el.X, el.Y, fill, el.Height), 3, 3)
                End If
            End If

            ' Top highlight
            dc.DrawRectangle(
                New SolidColorBrush(Color.FromArgb(30, 255, 255, 255)), Nothing,
                New Rect(el.X + 1, el.Y + 1, el.Width - 2, el.Height / 2.5))
        Next
    End Sub

End Class
End Namespace
