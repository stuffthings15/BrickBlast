; BrickBlast: Velocity Market — Win64 Assembly Launcher
; Assembler: ml64.exe  (MASM, ships with Visual Studio)
; No external include files required.
;
; Build (Developer Command Prompt — x64):
;   ml64 /c BrickBlast64.asm
;   link /SUBSYSTEM:WINDOWS /ENTRY:WinMain /NODEFAULTLIB BrickBlast64.obj ^
;        kernel32.lib shell32.lib user32.lib
;
; Launches windows-x64\BrickBlast.exe from the same directory as this launcher.

EXTERN GetModuleHandleA:PROC
EXTERN ShellExecuteA:PROC
EXTERN ExitProcess:PROC
EXTERN MessageBoxA:PROC

.data
    szExe   db "..\\windows-x64\\BrickBlast.exe", 0
    szOpen  db "open", 0
    szDir   db "..\\windows-x64", 0
    szErr   db "Cannot find BrickBlast.exe.", 0
    szTtl   db "BrickBlast Launcher", 0

.code

WinMain PROC
    ; --- Prologue (shadow space) ---
    sub     rsp, 40

    ; ShellExecuteA(NULL, "open", exePath, NULL, workDir, SW_SHOWNORMAL)
    ; SW_SHOWNORMAL = 1
    xor     rcx, rcx                ; hWnd = NULL
    lea     rdx, szOpen             ; lpOperation
    lea     r8,  szExe              ; lpFile
    xor     r9,  r9                 ; lpParameters = NULL
    ; 5th arg on stack: lpDirectory
    lea     rax, szDir
    mov     [rsp+32], rax
    ; 6th arg on stack: nShowCmd = SW_SHOWNORMAL (1)
    mov     qword ptr [rsp+40], 1
    call    ShellExecuteA

    ; ShellExecuteA returns >32 on success
    cmp     rax, 32
    jg      @success

    ; Show error
    xor     rcx, rcx
    lea     rdx, szErr
    lea     r8,  szTtl
    mov     r9,  10h                ; MB_ICONERROR
    call    MessageBoxA

@success:
    xor     rcx, rcx
    call    ExitProcess

    add     rsp, 40
    ret
WinMain ENDP

END
