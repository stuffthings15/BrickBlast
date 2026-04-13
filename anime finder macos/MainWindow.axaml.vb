Imports Avalonia.Controls
Imports Avalonia.Input

Namespace BrickBlastMacOS
    Partial Public Class MainWindow
        Inherits Window

        Protected Overrides Sub OnOpened(e As EventArgs)
            MyBase.OnOpened(e)
            FindControl(Of GameCanvas)("Game")?.Focus()
        End Sub

        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            ' Alt+Enter toggles fullscreen (same shortcut as Windows version)
            If e.Key = Key.Enter AndAlso e.KeyModifiers.HasFlag(KeyModifiers.Alt) Then
                WindowState = If(WindowState = WindowState.FullScreen, WindowState.Normal, WindowState.FullScreen)
                e.Handled = True
            End If
        End Sub
    End Class
End Namespace
