; ============================================================
;  main.asm — BrickBlast Win32 Entry Point
;  MASM x64  •  Microsoft calling convention
;  No CRT. WinMain → RegisterClassEx → CreateWindowEx → message loop.
; ============================================================

OPTION CASEMAP:NONE

; ── Externs: Win32 API ──────────────────────────────────────────────────────
EXTERN GetModuleHandleW:PROC
EXTERN LoadCursorW:PROC
EXTERN RegisterClassExW:PROC
EXTERN CreateWindowExW:PROC
EXTERN ShowWindow:PROC
EXTERN UpdateWindow:PROC
EXTERN GetMessageW:PROC
EXTERN TranslateMessage:PROC
EXTERN DispatchMessageW:PROC
EXTERN PostQuitMessage:PROC
EXTERN DefWindowProcW:PROC
EXTERN BeginPaint:PROC
EXTERN EndPaint:PROC
EXTERN GetClientRect:PROC
EXTERN SetTimer:PROC
EXTERN KillTimer:PROC
EXTERN DestroyWindow:PROC
EXTERN ReleaseDC:PROC
EXTERN GetDC:PROC
EXTERN CreateMutexW:PROC
EXTERN GetLastError:PROC
EXTERN CloseHandle:PROC
EXTERN MessageBoxW:PROC
EXTERN SetWindowTextW:PROC
EXTERN InvalidateRect:PROC
EXTERN GetSystemMetrics:PROC
EXTERN MoveWindow:PROC
EXTERN GetWindowRect:PROC
EXTERN SystemParametersInfoW:PROC
EXTERN ExitProcess:PROC

; ── Externs: C functions ────────────────────────────────────────────────────
EXTERN Game_Init:PROC
EXTERN Game_Tick:PROC
EXTERN Game_OnKey:PROC
EXTERN Game_OnChar:PROC
EXTERN Game_OnMouse:PROC
EXTERN Game_OnSize:PROC
EXTERN Game_OnPaint:PROC
EXTERN Game_Shutdown:PROC
EXTERN Input_SetKey:PROC
EXTERN Input_EndFrame:PROC
EXTERN Sound_Init:PROC
EXTERN Render_InitBackBuffer:PROC
EXTERN Render_BeginFrame:PROC
EXTERN Render_EndFrame:PROC
EXTERN Render_GetDC:PROC
EXTERN Render_CreateFonts:PROC
EXTERN Render_DestroyFonts:PROC

; ── Constants ───────────────────────────────────────────────────────────────
IDC_ARROW       EQU 32512
SW_SHOWDEFAULT  EQU 10
WS_OVERLAPPEDWINDOW EQU 0CF0000h
WS_VISIBLE      EQU 10000000h
CS_HREDRAW      EQU 0002h
CS_VREDRAW      EQU 0001h
CS_OWNDC        EQU 0020h
WM_DESTROY      EQU 0002h
WM_PAINT        EQU 000Fh
WM_TIMER        EQU 0113h
WM_KEYDOWN      EQU 0100h
WM_KEYUP        EQU 0101h
WM_CHAR         EQU 0102h
WM_MOUSEMOVE    EQU 0200h
WM_LBUTTONDOWN  EQU 0201h
WM_LBUTTONUP    EQU 0202h
WM_MOUSEWHEEL   EQU 020Ah
WM_SIZE         EQU 0005h
WM_GETMINMAXINFO EQU 0024h
WM_CLOSE        EQU 0010h
ERROR_ALREADY_EXISTS EQU 183
SM_CXSCREEN     EQU 0
SM_CYSCREEN     EQU 1
SPI_GETWORKAREA EQU 48
TIMER_ID_GAME   EQU 1
TIMER_MS        EQU 16
CANVAS_W        EQU 900
CANVAS_H        EQU 700
MB_OK           EQU 0
MB_ICONERROR    EQU 10h

; WNDCLASSEX size = 80 bytes on x64
WNDCLASSEX_SIZE EQU 80

; PAINTSTRUCT = 72 bytes
PAINTSTRUCT_SIZE EQU 72

; MSG = 48 bytes
MSG_SIZE        EQU 48

; MINMAXINFO offsets for ptMinTrackSize
MINMAX_ptMinTrackX EQU 24
MINMAX_ptMinTrackY EQU 28

; ── .data — read-only strings ────────────────────────────────────────────────
.data
szClass     WORD 'B','r','i','c','k','B','l','a','s','t','W','n','d',0
szTitle     WORD 'B','r','i','c','k','B','l','a','s','t',':',' ','V','e','l','o','c','i','t','y',' ','M','a','r','k','e','t',0
szMutex     WORD 'B','r','i','c','k','B','l','a','s','t','_','S','i','n','g','l','e','I','n','s','t','a','n','c','e',0
szAlready   WORD 'B','r','i','c','k','B','l','a','s','t',' ','i','s',' ','a','l','r','e','a','d','y',' ','r','u','n','n','i','n','g','.',0
szErrTitle  WORD 'B','r','i','c','k','B','l','a','s','t',0
szInitFail  WORD 'C','r','e','a','t','e','W','i','n','d','o','w','E','x',' ','f','a','i','l','e','d','.',0

; ── .data? — uninitialized ───────────────────────────────────────────────────
.data?
g_hwnd          QWORD ?
g_hinstance     QWORD ?
g_hMutex        QWORD ?
_wce            BYTE WNDCLASSEX_SIZE dup(?)   ; WNDCLASSEX
_ps             BYTE PAINTSTRUCT_SIZE dup(?)  ; PAINTSTRUCT
_msg            BYTE MSG_SIZE dup(?)          ; MSG
_rc             BYTE 16 dup(?)               ; RECT (4 x DWORD)

; ── .code ────────────────────────────────────────────────────────────────────
.code

; ============================================================
;  WinMainASM — exported entry point (LINK /ENTRY:WinMainASM)
;  rcx = hInstance, rdx = hPrevInstance, r8 = lpCmdLine, r9 = nShowCmd
; ============================================================
WinMainASM PROC
    push    rbx
    push    rsi
    push    rdi
    push    r12
    push    r13
    sub     rsp, 96         ; N=5 pushes → need X%16=0. 96%16=0 ✓; stack args at [rsp+32]..[rsp+88]

    ; Save hInstance
    mov     [g_hinstance], rcx

    ; ── Single-instance mutex ────────────────────────────────
    xor     rcx, rcx                    ; lpMutexAttributes = NULL
    mov     rdx, 1                      ; bInitialOwner = TRUE
    lea     r8, szMutex
    call    CreateMutexW
    mov     [g_hMutex], rax
    call    GetLastError
    cmp     eax, ERROR_ALREADY_EXISTS
    jne     @mutex_ok
    ; Already running — notify and exit
    xor     rcx, rcx
    lea     rdx, szAlready
    lea     r8, szErrTitle
    mov     r9, MB_OK or MB_ICONERROR
    call    MessageBoxW
    xor     rcx, rcx
    call    ExitProcess

@mutex_ok:
    ; ── Register window class ────────────────────────────────
    lea     rbx, _wce
    ; cbSize
    mov     DWORD PTR [rbx], WNDCLASSEX_SIZE
    ; style
    mov     DWORD PTR [rbx+4], CS_HREDRAW or CS_VREDRAW or CS_OWNDC
    ; lpfnWndProc
    lea     rax, WndProc
    mov     QWORD PTR [rbx+8], rax
    ; cbClsExtra, cbWndExtra = 0 (already zeroed by BSS? no, use .data? so zero init)
    mov     DWORD PTR [rbx+16], 0
    mov     DWORD PTR [rbx+20], 0
    ; hInstance
    mov     rax, [g_hinstance]
    mov     QWORD PTR [rbx+24], rax
    ; hIcon = NULL
    mov     QWORD PTR [rbx+32], 0
    ; hCursor = LoadCursor(NULL, IDC_ARROW)
    xor     rcx, rcx
    mov     rdx, IDC_ARROW
    call    LoadCursorW
    mov     QWORD PTR [rbx+40], rax
    ; hbrBackground = (HBRUSH)(COLOR_WINDOW+1) = 6
    mov     QWORD PTR [rbx+48], 6
    ; lpszMenuName = NULL
    mov     QWORD PTR [rbx+56], 0
    ; lpszClassName
    lea     rax, szClass
    mov     QWORD PTR [rbx+64], rax
    ; hIconSm = NULL
    mov     QWORD PTR [rbx+72], 0

    lea     rcx, _wce
    call    RegisterClassExW

    ; ── Compute centered window position ────────────────────
    mov     ecx, SM_CXSCREEN
    call    GetSystemMetrics
    mov     r12d, eax               ; screen width

    mov     ecx, SM_CYSCREEN
    call    GetSystemMetrics
    mov     r13d, eax               ; screen height

    ; center: x = (screenW - CANVAS_W) / 2, y = (screenH - CANVAS_H) / 2
    mov     eax, r12d
    sub     eax, CANVAS_W
    sar     eax, 1
    mov     esi, eax                ; winX

    mov     eax, r13d
    sub     eax, CANVAS_H
    sar     eax, 1
    mov     edi, eax                ; winY

    ; ── CreateWindowEx ───────────────────────────────────────
    ; CreateWindowExW(exStyle, class, title, style, x, y, w, h, parent, menu, hInst, lParam)
    xor     rcx, rcx                ; dwExStyle = 0
    lea     rdx, szClass            ; lpClassName
    lea     r8, szTitle             ; lpWindowName
    mov     r9d, WS_OVERLAPPEDWINDOW ; no WS_VISIBLE — ShowWindow called after Game_Init
    ; args 5-12 on stack at 8-byte-aligned slots
    mov     DWORD PTR [rsp+32], esi     ; x       (arg 5)
    mov     DWORD PTR [rsp+40], edi     ; y       (arg 6)
    mov     DWORD PTR [rsp+48], CANVAS_W ; nWidth  (arg 7)
    mov     DWORD PTR [rsp+56], CANVAS_H ; nHeight (arg 8)
    mov     QWORD PTR [rsp+64], 0       ; hWndParent (arg 9)
    mov     QWORD PTR [rsp+72], 0       ; hMenu   (arg 10)
    mov     rax, [g_hinstance]
    mov     QWORD PTR [rsp+80], rax     ; hInstance (arg 11)
    mov     QWORD PTR [rsp+88], 0       ; lpParam (arg 12)
    call    CreateWindowExW
    mov     [g_hwnd], rax
    test    rax, rax
    jz      @exit_fail

    ; ── Sound + Game init ────────────────────────────────────
    call    Sound_Init
    call    Game_Init
    call    Render_CreateFonts

    ; ── Init back buffer before first paint ──────────────────
    ; Use CANVAS_W / CANVAS_H — window isn't shown yet so
    ; GetClientRect would return a zero rect.
    mov     rcx, [g_hwnd]
    call    GetDC
    mov     rbx, rax                    ; save hdc
    mov     rcx, rbx                    ; hdc   (arg 1)
    mov     edx, CANVAS_W               ; w     (arg 2)
    mov     r8d, CANVAS_H               ; h     (arg 3)
    call    Render_InitBackBuffer
    mov     rcx, [g_hwnd]
    mov     rdx, rbx
    call    ReleaseDC

    ; ── ShowWindow + UpdateWindow ────────────────────────────
    mov     rcx, [g_hwnd]
    mov     edx, SW_SHOWDEFAULT
    call    ShowWindow

    mov     rcx, [g_hwnd]
    call    UpdateWindow

    ; ── SetTimer (game loop) ─────────────────────────────────
    mov     rcx, [g_hwnd]
    mov     edx, TIMER_ID_GAME
    mov     r8d, TIMER_MS
    xor     r9, r9
    call    SetTimer

    ; ── Message loop ─────────────────────────────────────────
@msg_loop:
    lea     rcx, _msg
    xor     rdx, rdx
    xor     r8d, r8d
    xor     r9d, r9d
    call    GetMessageW
    test    eax, eax
    jle     @msg_done

    lea     rcx, _msg
    call    TranslateMessage

    lea     rcx, _msg
    call    DispatchMessageW
    jmp     @msg_loop

@msg_done:
    ; wParam of WM_QUIT is exit code
    movsxd  rax, DWORD PTR [_msg+8]
    jmp     @exit

@exit_fail:
    xor     rcx, rcx
    lea     rdx, szInitFail
    lea     r8,  szErrTitle
    mov     r9d, MB_OK or MB_ICONERROR
    call    MessageBoxW
    xor     rax, rax

@exit:
    ; Cleanup mutex
    mov     rcx, [g_hMutex]
    test    rcx, rcx
    jz      @skip_mutex
    call    CloseHandle
@skip_mutex:
    add     rsp, 96
    pop     r13
    pop     r12
    pop     rdi
    pop     rsi
    pop     rbx
    ret
WinMainASM ENDP

; ============================================================
;  WndProc — window procedure
;  rcx = hwnd, rdx = uMsg, r8 = wParam, r9 = lParam
; ============================================================
WndProc PROC
    push    rbx
    push    rsi
    push    rdi
    push    r12
    push    r13
    push    r14
    push    r15
    sub     rsp, 64         ; N=7 pushes → need X%16=0. 64%16=0 ✓

    mov     rbx, rcx        ; save hwnd
    mov     esi, edx        ; save uMsg
    mov     r12, r8         ; save wParam
    mov     r13, r9         ; save lParam

    ; Dispatch on uMsg
    cmp     esi, WM_PAINT
    je      @wm_paint
    cmp     esi, WM_TIMER
    je      @wm_timer
    cmp     esi, WM_KEYDOWN
    je      @wm_keydown
    cmp     esi, WM_KEYUP
    je      @wm_keyup
    cmp     esi, WM_CHAR
    je      @wm_char
    cmp     esi, WM_MOUSEMOVE
    je      @wm_mouse
    cmp     esi, WM_LBUTTONDOWN
    je      @wm_lbdown
    cmp     esi, WM_LBUTTONUP
    je      @wm_lbup
    cmp     esi, WM_MOUSEWHEEL
    je      @wm_wheel
    cmp     esi, WM_SIZE
    je      @wm_size
    cmp     esi, WM_GETMINMAXINFO
    je      @wm_minmax
    cmp     esi, WM_CLOSE
    je      @wm_close
    cmp     esi, WM_DESTROY
    je      @wm_destroy
    jmp     @default

; ── WM_PAINT ─────────────────────────────────────────────────
@wm_paint:
    mov     rcx, rbx
    lea     rdx, _ps
    call    BeginPaint
    mov     rdi, rax        ; save window hdc

    mov     rcx, rdi        ; window hdc — Render_BeginFrame clears back buffer
    call    Render_BeginFrame

    call    Render_GetDC    ; rax = back-buffer DC
    mov     rcx, rax        ; pass back-buffer DC to game draw code
    call    Game_OnPaint

    mov     rcx, rdi        ; window hdc
    mov     rdx, rbx        ; hwnd
    call    Render_EndFrame

    mov     rcx, rbx
    lea     rdx, _ps
    call    EndPaint
    xor     rax, rax
    jmp     @done

; ── WM_TIMER ─────────────────────────────────────────────────
@wm_timer:
    cmp     r12d, TIMER_ID_GAME
    jne     @default
    call    Game_Tick
    ; Request repaint (NULL = whole client area, FALSE = no erase)
    mov     rcx, rbx
    xor     rdx, rdx
    xor     r8d, r8d
    call    InvalidateRect
    xor     rax, rax
    jmp     @done

; ── WM_KEYDOWN ───────────────────────────────────────────────
@wm_keydown:
    mov     rcx, r12        ; vk
    mov     edx, 1          ; down = TRUE
    call    Input_SetKey
    mov     rcx, r12
    mov     edx, 1
    call    Game_OnKey
    xor     rax, rax
    jmp     @done

; ── WM_KEYUP ─────────────────────────────────────────────────
@wm_keyup:
    mov     rcx, r12
    xor     rdx, rdx        ; down = FALSE
    call    Input_SetKey
    mov     rcx, r12
    xor     rdx, rdx
    call    Game_OnKey
    xor     rax, rax
    jmp     @done

; ── WM_CHAR ──────────────────────────────────────────────────
@wm_char:
    mov     rcx, r12        ; character code
    call    Game_OnChar
    xor     rax, rax
    jmp     @done

; ── WM_MOUSEMOVE / LBUTTONDOWN / LBUTTONUP ───────────────────
@wm_mouse:
    ; lParam: low word = x, high word = y (r13 = lParam)
    mov     rax, r13
    movsx   ecx, ax                 ; x = LOWORD(lParam)
    sar     rax, 16
    movsx   edx, ax                 ; y = HIWORD(lParam)
    ; rcx=x rdx=y, r8=ldown, r9=lup, stack=wheelDelta
    mov     r8d, 0
    mov     r9d, 0
    mov     DWORD PTR [rsp+32], 0   ; wheelDelta
    call    Game_OnMouse
    xor     rax, rax
    jmp     @done

@wm_lbdown:
    mov     rax, r13
    movsx   ecx, ax
    sar     rax, 16
    movsx   edx, ax
    mov     r8d, 1                  ; ldown
    mov     r9d, 0
    mov     DWORD PTR [rsp+32], 0
    call    Game_OnMouse
    xor     rax, rax
    jmp     @done

@wm_lbup:
    mov     rax, r13
    movsx   ecx, ax
    sar     rax, 16
    movsx   edx, ax
    mov     r8d, 0
    mov     r9d, 1                  ; lup
    mov     DWORD PTR [rsp+32], 0
    call    Game_OnMouse
    xor     rax, rax
    jmp     @done

@wm_wheel:
    ; wParam high word = delta (120 per notch)
    mov     rax, r12
    sar     rax, 16
    movsx   eax, ax                 ; signed delta
    ; mouse position from lParam
    mov     rdx, r13
    movsx   ecx, dx
    sar     rdx, 16
    movsx   edx, dx
    ; Game_OnMouse(x, y, 0, 0, delta)
    ; Re-order: rcx=x rdx=y r8=ldown r9=lup stack=delta
    push    rax                     ; save delta
    mov     r14d, ecx
    mov     r15d, edx
    pop     rax
    mov     ecx, r14d
    mov     edx, r15d
    mov     r8d, 0
    mov     r9d, 0
    mov     DWORD PTR [rsp+32], eax
    call    Game_OnMouse
    xor     rax, rax
    jmp     @done

; ── WM_SIZE ──────────────────────────────────────────────────
@wm_size:
    mov     rax, r13
    movsx   ecx, ax                 ; w = LOWORD
    sar     rax, 16
    movsx   edx, ax                 ; h = HIWORD
    ; Reinit back buffer
    call    Game_OnSize
    ; Also reinit back buffer for new size
    mov     rcx, rbx
    call    GetDC
    mov     rdi, rax
    ; get new client rect
    lea     rdx, _rc
    mov     rcx, rbx
    call    GetClientRect
    mov     edx, DWORD PTR [_rc+8]  ; right = width  (arg 2)
    mov     r8d, DWORD PTR [_rc+12] ; bottom = height (arg 3)
    ; Render_InitBackBuffer(hdc, w, h)
    mov     rcx, rdi
    call    Render_InitBackBuffer
    mov     rcx, rbx
    mov     rdx, rdi
    call    ReleaseDC
    xor     rax, rax
    jmp     @done

; ── WM_GETMINMAXINFO — enforce minimum window size ───────────
@wm_minmax:
    ; r13 = lParam = pointer to MINMAXINFO
    ; ptMinTrackSize is at offset 24
    mov     DWORD PTR [r13+24], 600     ; min width
    mov     DWORD PTR [r13+28], 467     ; min height
    xor     rax, rax
    jmp     @done

; ── WM_CLOSE ─────────────────────────────────────────────────
@wm_close:
    call    Game_Shutdown
    mov     rcx, rbx
    mov     edx, TIMER_ID_GAME
    call    KillTimer
    mov     rcx, rbx
    call    DestroyWindow
    xor     rax, rax
    jmp     @done

; ── WM_DESTROY ───────────────────────────────────────────────
@wm_destroy:
    call    Render_DestroyFonts
    xor     ecx, ecx
    call    PostQuitMessage
    xor     rax, rax
    jmp     @done

; ── Default ──────────────────────────────────────────────────
@default:
    mov     rcx, rbx
    mov     edx, esi
    mov     r8, r12
    mov     r9, r13
    call    DefWindowProcW
    jmp     @done

@done:
    add     rsp, 64
    pop     r15
    pop     r14
    pop     r13
    pop     r12
    pop     rdi
    pop     rsi
    pop     rbx
    ret
WndProc ENDP

END
