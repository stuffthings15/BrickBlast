@echo off
title Brick Blast - Windows WPF
echo ============================================
echo   BRICK BLAST - Windows WPF Version
echo   WPF + VB.NET  -  Team Fast Talk
echo ============================================
echo.
cd /d "%~dp0"
echo Syncing assets from main project...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\..\tools\integrate_all_assets.ps1" >nul 2>&1

echo Building from MAIN project (anime finder wpf)...
cd /d "%~dp0..\..\anime finder wpf"
dotnet publish "anime finder wpf.vbproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "%~dp0"

cd /d "%~dp0"
if exist "BrickBlast.exe" (
    echo Launching updated BrickBlast.exe...
    start "" "BrickBlast.exe"
) else (
    echo ERROR: Build failed. BrickBlast.exe not found.
    echo Make sure .NET SDK 10 is installed and build errors are fixed.
    pause
)
