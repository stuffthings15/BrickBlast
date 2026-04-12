Imports System.Windows

Namespace BrickBlastWPF
Partial Public Class MainWindow
    Inherits Window

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Dim gc = TryCast(FindName("Game"), FrameworkElement)
        If gc IsNot Nothing Then gc.Focus()
    End Sub

    Private Sub Window_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        Dim gc = TryCast(FindName("Game"), FrameworkElement)
        If gc IsNot Nothing Then gc.InvalidateVisual()
    End Sub
End Class
End Namespace
