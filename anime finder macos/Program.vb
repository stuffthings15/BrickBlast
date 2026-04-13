' =============================================================================
' BRICK BLAST macOS — Entry Point
' Build : dotnet build -r osx-arm64   (Apple Silicon M1/M2/M3)
'         dotnet build -r osx-x64     (Intel Mac)
' Run   : dotnet run
' =============================================================================
Imports Avalonia

Namespace BrickBlastMacOS
    Friend Module Program
        <STAThread>
        Sub Main(args As String())
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args)
        End Sub

        Function BuildAvaloniaApp() As AppBuilder
            Return AppBuilder.Configure(Of App)() _
                .UsePlatformDetect() _
                .WithInterFont() _
                .LogToTrace()
        End Function
    End Module
End Namespace
