' =============================================================================
' ASSET IMPORTER — Brick Blast WPF
' Scans C:\GameAssets\SuperGameAsset\ for manually downloaded assets,
' classifies them, and copies to the project's Assets/ subfolders.
'
' Supported packs from supergameasset.com:
'   FREE:  Basic RPG Item Icons Free, Male Warrior Sample, Desert Map Sample
'   PAID:  Hero Skills Fantasy RPG, Basic RPG Item Icons, Epic RPG UI
'
' Pipeline: User downloads → C:\GameAssets\SuperGameAsset\ → RunImport() →
'           Assets/{UI,Characters,Tiles}/ → AssetManager auto-loads
'
' LEGAL: Only imports files the user has already downloaded to their machine.
' =============================================================================
Imports System.IO
Imports System.Windows.Media

Namespace BrickBlastWPF

Public Class ImportReport
    Public SourceFound As Boolean = False
    Public Log As New List(Of String)
    Public Imported As New List(Of String)
    Public Skipped As New List(Of String)
    Public Errors As New List(Of String)
    Public ReadOnly Property Summary As String
        Get
            Return $"Import: {Imported.Count} new, {Skipped.Count} existing, {Errors.Count} errors"
        End Get
    End Property
End Class

Public NotInheritable Class AssetImporter

    Public Const SOURCE_ROOT As String = "C:\GameAssets\SuperGameAsset"

    ' ── Keyword → standard asset key mapping ──
    ' Icon keywords are matched against lowercase filenames.
    ' First match wins; order matters (most specific first).
    Private Shared ReadOnly _iconKeywords As String()() = {
        New String() {"potion_red", "UI/powerup_life"},
        New String() {"health_potion", "UI/powerup_life"},
        New String() {"heart", "UI/powerup_life"},
        New String() {"life", "UI/powerup_life"},
        New String() {"heal", "UI/powerup_life"},
        New String() {"potion_green", "UI/powerup_grow"},
        New String() {"grow", "UI/powerup_grow"},
        New String() {"expand", "UI/powerup_grow"},
        New String() {"enlarge", "UI/powerup_grow"},
        New String() {"multiply", "UI/powerup_multi"},
        New String() {"multi", "UI/powerup_multi"},
        New String() {"split", "UI/powerup_multi"},
        New String() {"triple", "UI/powerup_multi"},
        New String() {"star", "UI/powerup_multi"},
        New String() {"potion_blue", "UI/powerup_shrink"},
        New String() {"shrink", "UI/powerup_shrink"},
        New String() {"small", "UI/powerup_shrink"},
        New String() {"poison", "UI/powerup_shrink"},
        New String() {"shield", "UI/powerup_mega"},
        New String() {"armor", "UI/powerup_mega"},
        New String() {"protect", "UI/powerup_mega"},
        New String() {"ice", "UI/powerup_slow"},
        New String() {"frost", "UI/powerup_slow"},
        New String() {"slow", "UI/powerup_slow"},
        New String() {"cold", "UI/powerup_slow"},
        New String() {"lightning", "UI/powerup_fast"},
        New String() {"speed", "UI/powerup_fast"},
        New String() {"haste", "UI/powerup_fast"},
        New String() {"fire", "UI/powerup_fast"},
        New String() {"arrow", "UI/powerup_fast"},
        New String() {"gem", "UI/gem"},
        New String() {"diamond", "UI/gem"},
        New String() {"crystal", "UI/gem"},
        New String() {"coin", "UI/star"},
        New String() {"gold", "UI/star"},
        New String() {"sword", "UI/sword"},
        New String() {"blade", "UI/sword"},
        New String() {"key", "UI/key"},
        New String() {"scroll", "UI/scroll"},
        New String() {"book", "UI/scroll"},
        New String() {"potion", "UI/potion"},
        New String() {"ring", "UI/shield"}}

    ' ── Background / map keywords ──
    Private Shared ReadOnly _bgKeywords As String() =
        {"map", "desert", "background", "bg", "landscape", "scene", "terrain", "world"}

    ' ── Character / mascot keywords ──
    Private Shared ReadOnly _charKeywords As String() =
        {"warrior", "character", "hero", "player", "knight", "fighter", "male", "idle", "walk", "attack"}

    ''' <summary>
    ''' Scan C:\GameAssets\SuperGameAsset\, classify images, copy to project Assets/.
    ''' Safe to call every startup — skips already-imported files.
    ''' </summary>
    Public Shared Function RunImport(targetAssetsPath As String) As ImportReport
        Dim report As New ImportReport()

        If Not Directory.Exists(SOURCE_ROOT) Then
            report.SourceFound = False
            report.Log.Add($"[AssetImporter] Source not found: {SOURCE_ROOT}")
            report.Log.Add("[AssetImporter] To use SuperGameAsset art:")
            report.Log.Add("  1. Download free packs from supergameasset.com")
            report.Log.Add("  2. Extract to C:\GameAssets\SuperGameAsset\")
            report.Log.Add("  3. Restart the game — import runs automatically")
            Return report
        End If

        report.SourceFound = True

        ' Find all image files recursively
        Dim exts = {".png", ".jpg", ".jpeg", ".bmp"}
        Dim allFiles = Directory.GetFiles(SOURCE_ROOT, "*.*", SearchOption.AllDirectories).
            Where(Function(f) exts.Contains(Path.GetExtension(f).ToLower())).ToArray()

        report.Log.Add($"[AssetImporter] Found {allFiles.Length} image(s) in {SOURCE_ROOT}")

        ' Track which standard keys have already been assigned (first match wins)
        Dim assigned As New HashSet(Of String)
        ' Track unmapped files for fallback icon assignment
        Dim unmappedIcons As New List(Of String)

        For Each filePath In allFiles
            Dim fileName = Path.GetFileNameWithoutExtension(filePath).ToLower()
            Dim fileExt = Path.GetExtension(filePath).ToLower()
            Dim parentDir = Path.GetFileName(Path.GetDirectoryName(filePath)).ToLower()
            Dim combined = parentDir & "/" & fileName

            Dim targetSub As String = Nothing
            Dim targetName As String = Nothing

            ' ── Classify: background / map ──
            If MatchesAny(combined, _bgKeywords) Then
                targetSub = "Tiles"
                targetName = "menu_background"
                If assigned.Contains(targetSub & "/" & targetName) Then
                    ' Additional backgrounds get numbered
                    targetName = "background_" & Sanitize(fileName)
                End If

            ' ── Classify: character / mascot ──
            ElseIf MatchesAny(combined, _charKeywords) Then
                targetSub = "Characters"
                If Not assigned.Contains("Characters/menu_mascot") Then
                    targetName = "menu_mascot"
                Else
                    targetName = "char_" & Sanitize(fileName)
                End If

            ' ── Classify: icon (keyword match) ──
            Else
                targetSub = "UI"
                Dim matched = False
                For Each kw In _iconKeywords
                    If combined.Contains(kw(0)) Then
                        Dim stdKey = kw(1).Replace("UI/", "")
                        If Not assigned.Contains("UI/" & stdKey) Then
                            targetName = stdKey
                            matched = True
                            Exit For
                        End If
                    End If
                Next
                If Not matched Then
                    ' Queue for later — may fill remaining power-up slots
                    unmappedIcons.Add(filePath)
                    Continue For
                End If
            End If

            ' Copy to target
            Dim key = targetSub & "/" & targetName
            If CopyAsset(filePath, targetAssetsPath, targetSub, targetName & fileExt, report) Then
                assigned.Add(key)
            End If
        Next

        ' ── Fill missing power-up icons from unmapped files ──
        Dim requiredPowerUps = {"UI/powerup_life", "UI/powerup_grow", "UI/powerup_multi",
                                "UI/powerup_shrink", "UI/powerup_mega", "UI/powerup_slow", "UI/powerup_fast"}
        Dim unmappedIdx = 0
        For Each needed In requiredPowerUps
            If assigned.Contains(needed) Then Continue For
            If unmappedIdx >= unmappedIcons.Count Then Exit For
            Dim fp = unmappedIcons(unmappedIdx)
            unmappedIdx += 1
            Dim ext = Path.GetExtension(fp).ToLower()
            Dim nm = needed.Replace("UI/", "")
            If CopyAsset(fp, targetAssetsPath, "UI", nm & ext, report) Then
                assigned.Add(needed)
                report.Log.Add($"  [Fallback] Assigned unmapped icon → {needed}")
            End If
        Next

        ' ── Log missing assets ──
        For Each needed In requiredPowerUps
            If Not assigned.Contains(needed) Then
                report.Log.Add($"  [Missing] {needed} — procedural fallback will be used")
            End If
        Next
        If Not assigned.Contains("Tiles/menu_background") Then
            report.Log.Add("  [Missing] Tiles/menu_background — no background art found")
        End If
        If Not assigned.Contains("Characters/menu_mascot") Then
            report.Log.Add("  [Missing] Characters/menu_mascot — no mascot art found")
        End If

        report.Log.Add($"[AssetImporter] {report.Summary}")
        Return report
    End Function

    ' ── Helpers ──
    Private Shared Function CopyAsset(srcPath As String, assetsRoot As String,
                                       subFolder As String, destFileName As String,
                                       report As ImportReport) As Boolean
        Try
            Dim destDir = Path.Combine(assetsRoot, subFolder)
            If Not Directory.Exists(destDir) Then Directory.CreateDirectory(destDir)
            Dim destPath = Path.Combine(destDir, destFileName)
            If File.Exists(destPath) Then
                report.Skipped.Add($"{subFolder}/{destFileName}")
                Return True ' Already exists = counts as assigned
            End If
            File.Copy(srcPath, destPath, False)
            report.Imported.Add($"{subFolder}/{destFileName}")
            report.Log.Add($"  Imported: {Path.GetFileName(srcPath)} → Assets/{subFolder}/{destFileName}")
            Return True
        Catch ex As Exception
            report.Errors.Add($"Copy failed {Path.GetFileName(srcPath)}: {ex.Message}")
            Return False
        End Try
    End Function

    Private Shared Function MatchesAny(text As String, keywords As String()) As Boolean
        For Each kw In keywords
            If text.Contains(kw) Then Return True
        Next
        Return False
    End Function

    Private Shared Function Sanitize(name As String) As String
        Dim result = ""
        For Each c In name
            If Char.IsLetterOrDigit(c) OrElse c = "_"c OrElse c = "-"c Then result &= c
        Next
        If result.Length = 0 Then result = "asset"
        Return result
    End Function

End Class
End Namespace
