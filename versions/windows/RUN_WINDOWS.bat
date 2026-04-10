@echo off
title Brick Blast - Windows Desktop
echo ============================================
echo   BRICK BLAST - Windows Desktop Version
echo   Team Fast Talk
echo ============================================
echo.
echo Building and launching...
cd /d "%~dp0..\.."
dotnet build -c Release -o "versions\windows\build" >nul 2>&1
if exist "versions\windows\build\anime finder.exe" (
    start "" "versions\windows\build\anime finder.exe"
) else (
    echo Build failed. Trying pre-built version...
    if exist "publish\anime finder.exe" (
        start "" "publish\anime finder.exe"
    ) else (
        echo ERROR: No executable found. Open the .sln in Visual Studio and build.
        pause
    )
)
