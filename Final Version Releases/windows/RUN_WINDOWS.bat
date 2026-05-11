@echo off
title Brick Blast - Windows Desktop
echo ============================================
echo   BRICK BLAST - Windows Desktop Version
echo   Team Fast Talk
echo ============================================
echo.
cd /d "%~dp0"
if exist "BrickBlast.exe" (
    echo Launching BrickBlast.exe...
    start "" "BrickBlast.exe"
) else (
    echo BrickBlast.exe not found. Building from source...
    cd /d "%~dp0..\.."
    dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "versions\windows" >nul 2>&1
    cd /d "%~dp0"
    if exist "anime finder.exe" (
        rename "anime finder.exe" "BrickBlast.exe" >nul 2>&1
        start "" "BrickBlast.exe"
    ) else (
        echo ERROR: Build failed. Install .NET SDK and try again.
        pause
    )
)
