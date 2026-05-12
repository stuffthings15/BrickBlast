@echo off
:: ============================================================
::  BrickBlast Native Build
::  Run AFTER calling vcvars64.bat (or from x64 Native Tools prompt)
::  ml64.exe / cl.exe / link.exe must already be on PATH.
:: ============================================================
setlocal

set "SRC=%~dp0src"
set "OBJ=%~dp0obj"
set "EXE=%~dp0BrickBlast-native.exe"

:: Verify tools on PATH
where ml64.exe >nul 2>&1 || (echo [ERROR] ml64.exe not on PATH. Run vcvars64.bat first. & exit /b 1)
where cl.exe   >nul 2>&1 || (echo [ERROR] cl.exe not on PATH.   Run vcvars64.bat first. & exit /b 1)
where link.exe >nul 2>&1 || (echo [ERROR] link.exe not on PATH.  Run vcvars64.bat first. & exit /b 1)

if not exist "%OBJ%" mkdir "%OBJ%"

echo.
echo ============================================================
echo  BrickBlast Win32 MASM+C Native Build
echo ============================================================
echo.

:: ?? ASM ??????????????????????????????????????????????????????
echo [ASM] main.asm
ml64.exe /nologo /c /Fo"%OBJ%\main.obj" "%SRC%\main.asm" || goto :err

echo [ASM] render.asm
ml64.exe /nologo /c /Fo"%OBJ%\render.obj" "%SRC%\render.asm" || goto :err

echo [ASM] input.asm
ml64.exe /nologo /c /Fo"%OBJ%\input.obj" "%SRC%\input.asm" || goto :err

echo [ASM] sound.asm
ml64.exe /nologo /c /Fo"%OBJ%\sound.obj" "%SRC%\sound.asm" || goto :err

echo [ASM] runtime.asm
ml64.exe /nologo /c /Fo"%OBJ%\runtime.obj" "%SRC%\runtime.asm" || goto :err

:: ?? C ????????????????????????????????????????????????????????
echo [C]   game.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\game.obj" "%SRC%\game.c" || goto :err

echo [C]   physics.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\physics.obj" "%SRC%\physics.c" || goto :err

echo [C]   music.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\music.obj" "%SRC%\music.c" || goto :err

echo [C]   save.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\save.obj" "%SRC%\save.c" || goto :err

echo [C]   store.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\store.obj" "%SRC%\store.c" || goto :err

echo [C]   levels.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\levels.obj" "%SRC%\levels.c" || goto :err

echo [C]   draw_game.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\draw_game.obj" "%SRC%\draw_game.c" || goto :err

echo [C]   draw_screens.c
cl.exe /nologo /O2 /GS- /GR- /EHa- /W3 /c /I"%SRC%" /Fo"%OBJ%\draw_screens.obj" "%SRC%\draw_screens.c" || goto :err

:: ?? LINK ?????????????????????????????????????????????????????
echo [LINK] %EXE%
link.exe /nologo /SUBSYSTEM:WINDOWS /ENTRY:WinMainASM /NODEFAULTLIB ^
    /OUT:"%EXE%" ^
    "%OBJ%\main.obj" ^
    "%OBJ%\render.obj" ^
    "%OBJ%\input.obj" ^
    "%OBJ%\sound.obj" ^
    "%OBJ%\runtime.obj" ^
    "%OBJ%\game.obj" ^
    "%OBJ%\physics.obj" ^
    "%OBJ%\music.obj" ^
    "%OBJ%\save.obj" ^
    "%OBJ%\store.obj" ^
    "%OBJ%\levels.obj" ^
    "%OBJ%\draw_game.obj" ^
    "%OBJ%\draw_screens.obj" ^
    kernel32.lib user32.lib gdi32.lib winmm.lib shell32.lib xinput.lib || goto :err

echo.
echo ============================================================
echo  [OK] %EXE%
for %%F in ("%EXE%") do echo  Size: %%~zF bytes
echo ============================================================
echo.
goto :eof

:err
echo.
echo [FAILED] ? see errors above.
exit /b 1

