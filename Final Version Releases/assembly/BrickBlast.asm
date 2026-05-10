; BrickBlast: Velocity Market — Win32 Assembly Launcher
; Assembler:  MASM (ml.exe) or NASM with MS ABI
; Linker:     LINK.exe (Visual Studio / Windows SDK)
;
; Purpose: A minimal native Win32 stub that launches BrickBlast.exe from its
;          own directory.  This gives a zero-dependency native x86 entry point
;          for the Windows release — useful for system-level integration, kiosks,
;          or embedded environments where a full CLR bootstrap is undesired.
;
; Build (from a Visual Studio Developer Command Prompt, 32-bit tools):
;   ml.exe /c /coff BrickBlast.asm
;   link.exe /SUBSYSTEM:WINDOWS /ENTRY:WinMain BrickBlast.obj kernel32.lib shell32.lib
;
; Build (NASM + LINK, 32-bit):
;   nasm -f win32 BrickBlast.asm -o BrickBlast.obj
;   link.exe /SUBSYSTEM:WINDOWS /ENTRY:_WinMain@16 BrickBlast.obj kernel32.lib shell32.lib
;
; The resulting BrickBlast-Launcher.exe should sit next to windows-x64\BrickBlast.exe
; (or add the windows-x64 directory to PATH).

.386
.model flat, stdcall
option casemap:none

include windows.inc
include kernel32.inc
include shell32.inc
includelib kernel32.lib
includelib shell32.lib

.data
    ; Relative path to the CLR-built executable.
    ; Place this launcher one level above the windows-x64 folder, or adjust as needed.
    exePath     db "windows-x64\BrickBlast.exe", 0
    workDir     db "windows-x64", 0
    szOpen      db "open", 0
    szError     db "Cannot find BrickBlast.exe.", 0
    szTitle     db "BrickBlast Launcher", 0
    szCaption   db "BrickBlast: Velocity Market", 0

.data?
    si      STARTUPINFO <>
    pi      PROCESS_INFORMATION <>
    hProc   HANDLE ?

.code
WinMain proc hInst:HINSTANCE, hPrev:HINSTANCE, lpCmd:LPSTR, nShow:int

    ; ------------------------------------------------------------------
    ; Try CreateProcess first (native launch, inherits console if any).
    ; ------------------------------------------------------------------
    lea     eax, si
    invoke  RtlZeroMemory, eax, sizeof STARTUPINFO
    mov     si.cb, sizeof STARTUPINFO

    invoke  CreateProcess, \
                addr exePath, \      ; lpApplicationName
                NULL,         \      ; lpCommandLine
                NULL, NULL,   \      ; security attrs
                FALSE,        \      ; inherit handles
                NORMAL_PRIORITY_CLASS or CREATE_NEW_CONSOLE, \
                NULL,         \      ; environment
                addr workDir, \      ; current directory
                addr si,      \      ; STARTUPINFO
                addr pi              ; PROCESS_INFORMATION

    test    eax, eax
    jnz     @launched

    ; ------------------------------------------------------------------
    ; Fallback: ShellExecute (handles UAC prompts automatically).
    ; ------------------------------------------------------------------
    invoke  ShellExecute, NULL, addr szOpen, addr exePath, NULL, addr workDir, SW_SHOWNORMAL
    cmp     eax, 32
    jg      @ok

    ; ------------------------------------------------------------------
    ; Neither method worked — show an error box.
    ; ------------------------------------------------------------------
    invoke  MessageBox, NULL, addr szError, addr szTitle, MB_OK or MB_ICONERROR
    jmp     @done

@launched:
    invoke  CloseHandle, pi.hThread
    ; Optionally wait for the child:
    ; invoke WaitForSingleObject, pi.hProcess, INFINITE
    invoke  CloseHandle, pi.hProcess
    jmp     @ok

@ok:
    xor     eax, eax
    jmp     @done

@done:
    ret
WinMain endp

end WinMain
