# BrickBlast: Velocity Market — Windows x86 Assembly Launcher

## Purpose
A bare-metal Win32 assembly stub that launches the main `BrickBlast.exe`
(the self-contained .NET 10 WinForms build) from its sibling `windows-x64/` folder.

This gives a zero-dependency native entry point usable in kiosk environments,
batch scripts, or anywhere you need a small non-CLR `.exe` to bootstrap the game.

## Files
| File                    | Description                              |
|-------------------------|------------------------------------------|
| `BrickBlast.asm`        | MASM-syntax Win32 assembly source        |
| `Build-Assembly.bat`    | One-step build script (MASM + LINK)      |
| `BrickBlast-Launcher.exe` | Output — produced by the build script  |

## Build
1. Open **"x86 Native Tools Command Prompt for VS 2022"**
2. Navigate to this directory
3. Run: `Build-Assembly.bat`

## Run
```
BrickBlast-Launcher.exe
```
The launcher will start `windows-x64\BrickBlast.exe` in its own process.
If that path is not found, it falls back to `ShellExecute`, which respects UAC.

## ARM64 note
The `.asm` file is x86 (32-bit).  For ARM64 assembly, a separate ARM64 MASM
file using the ARM64 ABI (X0–X7 params, BL instruction, etc.) would be needed.
An ARM64 version using the Windows ARM64 calling convention is left as a
future exercise; the `windows-arm64\BrickBlast.exe` already runs natively on
ARM64 Windows without a stub launcher.
