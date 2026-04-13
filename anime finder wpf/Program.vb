Imports System.Windows
Imports System.IO
Imports System.Diagnostics

Namespace BrickBlastWPF
Module Program
    Private Const MinExpectedPngCount As Integer = 186

    Private Const MutexName As String = "AnimeFinder_CS120_SingleInstance"

    <STAThread>
    Sub Main()
        Dim createdNew As Boolean
        Dim appMutex As New System.Threading.Mutex(True, MutexName, createdNew)
        If Not createdNew Then
            MessageBox.Show(
                "Brick Blast is already running." & Environment.NewLine &
                "Close the other window first, then press F5.",
                "Already Running",
                MessageBoxButton.OK,
                MessageBoxImage.Warning)
            appMutex.Dispose()
            Return
        End If
        Try
            EnsureAssetsReady()
            Dim app As New Application()
            app.Run(New MainWindow())
        Finally
            Try : appMutex.ReleaseMutex() : Catch : End Try
            appMutex.Dispose()
        End Try
    End Sub

    Private Sub EnsureAssetsReady()
        Try
            Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
            Dim assetsDir = Path.Combine(baseDir, "Assets")

            Dim repoRoot = FindRepoRoot(baseDir)
            If String.IsNullOrEmpty(repoRoot) Then Return

            ' Permanent/offline behavior:
            ' Do NOT download at runtime.
            ' Always mirror canonical project assets into output so any asset edits show up on next F5.
            Dim canonicalAssets = Path.Combine(repoRoot, "anime finder wpf", "Assets")
            If CountPngs(canonicalAssets) < MinExpectedPngCount Then Return

            If Directory.Exists(assetsDir) Then
                Try
                    Directory.Delete(assetsDir, recursive:=True)
                Catch
                End Try
            End If
            CopyDirectory(canonicalAssets, assetsDir)

            Dim finalCount = CountPngs(assetsDir)
            Debug.WriteLine($"[ASSET_SYNC] runtime Assets mirrored. Count={finalCount}")
        Catch
            ' Silent fail: game can still run with procedural fallbacks
        End Try
    End Sub

    Private Function CountPngs(assetsDir As String) As Integer
        If Not Directory.Exists(assetsDir) Then Return 0
        Return Directory.GetFiles(assetsDir, "*.png", SearchOption.AllDirectories).Length
    End Function

    Private Function FindRepoRoot(startDir As String) As String
        Dim dir = New DirectoryInfo(startDir)
        While dir IsNot Nothing
            If File.Exists(Path.Combine(dir.FullName, "anime finder.slnx")) Then
                Return dir.FullName
            End If
            dir = dir.Parent
        End While
        Return Nothing
    End Function

    Private Sub CopyDirectory(sourceDir As String, destDir As String)
        Directory.CreateDirectory(destDir)

        For Each file In Directory.GetFiles(sourceDir)
            Dim destFile = Path.Combine(destDir, Path.GetFileName(file))
            System.IO.File.Copy(file, destFile, overwrite:=True)
        Next

        For Each subDir In Directory.GetDirectories(sourceDir)
            Dim childDest = Path.Combine(destDir, Path.GetFileName(subDir))
            CopyDirectory(subDir, childDest)
        Next
    End Sub
End Module
End Namespace
