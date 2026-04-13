Imports System.Windows
Imports System.Windows.Input

Namespace BrickBlastWPF
Partial Public Class MainWindow
    Inherits Window

    Private _prevLeft As Double, _prevTop As Double
    Private _prevWidth As Double, _prevHeight As Double

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Try
            Dim assetsPath = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets")
            Dim pngCount = If(IO.Directory.Exists(assetsPath), IO.Directory.GetFiles(assetsPath, "*.png", IO.SearchOption.AllDirectories).Length, 0)
            Title = $"Anime Finder [ASSETS: {pngCount}]"
        Catch
        End Try
        Dim gc = TryCast(FindName("Game"), FrameworkElement)
        If gc IsNot Nothing Then gc.Focus()
    End Sub

    Private Sub Window_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        Dim gc = TryCast(FindName("Game"), FrameworkElement)
        If gc IsNot Nothing Then gc.InvalidateVisual()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        If e.Key = Key.Enter AndAlso (Keyboard.Modifiers And ModifierKeys.Alt) = ModifierKeys.Alt Then
            ToggleFullscreen()
            e.Handled = True
        End If
    End Sub

    Public Sub ToggleFullscreen()
        If WindowStyle = WindowStyle.None Then
            WindowStyle = WindowStyle.SingleBorderWindow
            WindowState = WindowState.Normal
            ResizeMode  = ResizeMode.CanResize
            If _prevWidth > 0 Then
                Left = _prevLeft : Top = _prevTop
                Width = _prevWidth : Height = _prevHeight
            End If
        Else
            _prevLeft = Left : _prevTop = Top
            _prevWidth = Width : _prevHeight = Height
            WindowStyle = WindowStyle.None
            WindowState = WindowState.Maximized
            ResizeMode  = ResizeMode.NoResize
        End If
        Dim gc = TryCast(FindName("Game"), FrameworkElement)
        If gc IsNot Nothing Then gc.InvalidateVisual()
    End Sub
End Class
End Namespace