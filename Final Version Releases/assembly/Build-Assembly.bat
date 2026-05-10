@echo off
:: Build-Assembly.bat
:: Requires Visual Studio 2022 (or Build Tools) with the "Desktop development with C++"
:: workload installed — specifically the MASM tools component.
::
:: Run from a "x86 Native Tools Command Prompt for VS 2022".

setlocal
set SRC=BrickBlast.asm
set OBJ=BrickBlast.obj
set EXE=BrickBlast-Launcher.exe

echo =========================================================
echo  BrickBlast Win32 Assembly Launcher — Build Script
echo =========================================================

:: Assemble with MASM
ml.exe /c /coff /Fo%OBJ% %SRC%
if errorlevel 1 (
    echo [ERROR] Assembly step failed. Make sure ml.exe is in PATH.
    echo         Open "x86 Native Tools Command Prompt for VS 2022" and retry.
    exit /b 1
)

:: Link
link.exe /SUBSYSTEM:WINDOWS /ENTRY:WinMain /OUT:%EXE% %OBJ% kernel32.lib shell32.lib
if errorlevel 1 (
    echo [ERROR] Link step failed.
    exit /b 1
)

echo.
echo [OK] Built: %EXE%
echo      Place alongside windows-x64\BrickBlast.exe and run.
endlocal
