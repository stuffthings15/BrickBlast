Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Input
Imports Avalonia.Media

Namespace BrickBlastMacOS
    Public Class MainWindow
        Inherits Window

        Public Sub New()
            ' Build the window in pure code — no AXAML loader needed
            Title = "Brick Blast — macOS"
            Width = 1200
            Height = 867
            MinWidth = 640
            MinHeight = 480
            Background = New SolidColorBrush(Color.Parse("#0F0F1E"))
            WindowStartupLocation = WindowStartupLocation.CenterScreen

            Dim game As New GameCanvas()
            game.Focusable = True
            Content = game
        End Sub

        Protected Overrides Sub OnOpened(e As EventArgs)
            MyBase.OnOpened(e)
            If Content IsNot Nothing Then
                CType(Content, GameCanvas).Focus()
            End If
        End Sub

        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            If e.Key = Key.Enter AndAlso e.KeyModifiers.HasFlag(KeyModifiers.Alt) Then
                WindowState = If(WindowState = WindowState.FullScreen, WindowState.Normal, WindowState.FullScreen)
                e.Handled = True
            End If
        End Sub
    End Class
End Namespace
