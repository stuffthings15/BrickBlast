Imports Avalonia
Imports Avalonia.Controls.ApplicationLifetimes
Imports Avalonia.Markup.Xaml

Namespace BrickBlastMacOS
    Public Class App
        Inherits Application

        Public Overrides Sub Initialize()
            AvaloniaXamlLoader.Load(Me)
        End Sub

        Public Overrides Sub OnFrameworkInitializationCompleted()
            If TypeOf ApplicationLifetime Is IClassicDesktopStyleApplicationLifetime Then
                Dim desktop = CType(ApplicationLifetime, IClassicDesktopStyleApplicationLifetime)
                desktop.MainWindow = New MainWindow()
            End If
            MyBase.OnFrameworkInitializationCompleted()
        End Sub
    End Class
End Namespace
