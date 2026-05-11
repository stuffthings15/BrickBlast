@echo off
title Brick Blast - Windows WPF
echo ============================================
echo   BRICK BLAST - Windows WPF Version
echo   WPF + VB.NET  -  Team Fast Talk
echo ============================================
echo.
cd /d "%~dp0"
if exist "BrickBlast.exe" (
    start "" "BrickBlast.exe"
) else (
    echo ERROR: BrickBlast.exe not found in this folder.
    pause
)
