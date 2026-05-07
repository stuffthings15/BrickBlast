@echo off
title Brick Blast - Windows ARM64
echo ============================================
echo   BRICK BLAST - Windows ARM64 Version
echo   Team Fast Talk
echo ============================================
echo.
cd /d "%~dp0"
if exist "BrickBlast.exe" (
    echo Launching BrickBlast.exe...
    start "" "BrickBlast.exe"
) else (
    echo ERROR: BrickBlast.exe not found in this folder.
    echo.
    echo To rebuild, run from the repo root:
    echo   dotnet publish "anime finder.vbproj" -c Release -r win-arm64 --self-contained true /p:PublishSingleFile=true -o versions\windows-arm64
    pause
)
