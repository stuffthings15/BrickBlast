@echo off
title Brick Blast - Windows WPF
echo ============================================
echo   BRICK BLAST - Windows WPF Version
echo   WPF + VB.NET  -  Team Fast Talk
echo ============================================
echo.
cd /d "%~dp0"
if exist "BrickBlast.exe" (
    echo Launching BrickBlast.exe...
    start "" "BrickBlast.exe"
) else (
    echo BrickBlast.exe not found. Building from source...
    cd /d "%~dp0..\..\anime finder wpf"
    dotnet publish "anime finder wpf.vbproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "%~dp0" >nul 2>&1
    cd /d "%~dp0"
    if exist "BrickBlast.exe" (
        echo Build successful!
        start "" "BrickBlast.exe"
    ) else (
        echo ERROR: Build failed.
        echo Make sure .NET SDK 10 is installed.
        pause
    )
)
