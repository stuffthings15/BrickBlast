; ============================================================
;  input.asm — Keyboard, Mouse, and XInput Gamepad
;  MASM x64  •  Microsoft calling convention
;
;  Exported:
;    Input_SetKey(vk, down)        — called by WndProc on WM_KEYDOWN/UP
;    Input_EndFrame()              — swap prev/curr key tables each tick
;    Input_KeyDown(vk) -> BOOL     — true while key held
;    Input_KeyPressed(vk) -> BOOL  — true on first frame of press
;    Input_KeyReleased(vk) -> BOOL — true on frame key released
;    Input_MouseX() -> int         — last mouse X in client coords
;    Input_MouseY() -> int         — last mouse Y in client coords
;    Input_SetMouse(x, y)          — set by WndProc on WM_MOUSEMOVE
;    Input_PollGamepad()           — reads XInput controller 0
;    Input_GamepadButton(btn) -> BOOL — queries a GP button bitmask
;    Input_GamepadLX() -> int      — left stick X (-32768..32767)
;    Input_GamepadLY() -> int      — left stick Y
; ============================================================

OPTION CASEMAP:NONE

; XInput (xinput1_4.dll dynamically loaded to avoid link-time dep)
EXTERN LoadLibraryW:PROC
EXTERN GetProcAddress:PROC
EXTERN FreeLibrary:PROC

; ── Constants ───────────────────────────────────────────────────────────────
XINPUT_GAMEPAD_DPAD_UP          EQU 0001h
XINPUT_GAMEPAD_DPAD_DOWN        EQU 0002h
XINPUT_GAMEPAD_DPAD_LEFT        EQU 0004h
XINPUT_GAMEPAD_DPAD_RIGHT       EQU 0008h
XINPUT_GAMEPAD_START            EQU 0010h
XINPUT_GAMEPAD_BACK             EQU 0020h
XINPUT_GAMEPAD_A                EQU 1000h
XINPUT_GAMEPAD_B                EQU 2000h
XINPUT_GAMEPAD_X                EQU 4000h
XINPUT_GAMEPAD_Y                EQU 8000h
XINPUT_GAMEPAD_LEFT_SHOULDER    EQU 0100h
XINPUT_GAMEPAD_RIGHT_SHOULDER   EQU 0200h
XINPUT_MAX_CONTROLLER EQU 4

; XINPUT_STATE layout (x64, packed):
;   DWORD dwPacketNumber   [0]
;   WORD  wButtons         [4]
;   BYTE  bLeftTrigger     [6]
;   BYTE  bRightTrigger    [7]
;   SHORT sThumbLX         [8]
;   SHORT sThumbLY         [10]
;   SHORT sThumbRX         [12]
;   SHORT sThumbRY         [14]
; Total = 16 bytes
XINPUT_STATE_SIZE EQU 16

; ── .data — XInput library name and proc name ───────────────────────────────
.data
szXinput        WORD 'x','i','n','p','u','t','1','_','4','.','d','l','l',0
szGetState      BYTE 'X','I','n','p','u','t','G','e','t','S','t','a','t','e',0

; ── .data? — input state tables ─────────────────────────────────────────────
.data?
; 256-entry key tables (current and previous frame)
_keyCurr        BYTE 256 dup(?)
_keyPrev        BYTE 256 dup(?)
_mouseX         DWORD ?
_mouseY         DWORD ?

; Gamepad
_gpButtons      WORD  ?
_gpLX           SWORD ?
_gpLY           SWORD ?
_gpRX           SWORD ?
_gpRY           SWORD ?
_gpConnected    DWORD ?

_hXInput        QWORD ?     ; HMODULE for xinput1_4
_pfnGetState    QWORD ?     ; function pointer to XInputGetState

_gpState        BYTE XINPUT_STATE_SIZE dup(?) ; XINPUT_STATE buffer

.data
_xinputInited   DWORD 0

.data?

.code

; ============================================================
;  Input_InitXInput — called once at startup (internal)
;  Dynamically loads xinput1_4.dll so the EXE still runs
;  on machines that do not have XInput (graceful no-gamepad).
; ============================================================
Input_InitXInput PROC PRIVATE
    push    rbx
    sub     rsp, 32         ; N=1 push → X%16=0. 32%16=0 ✓

    cmp     DWORD PTR [_xinputInited], 0
    jne     @already

    lea     rcx, szXinput
    call    LoadLibraryW
    mov     [_hXInput], rax
    test    rax, rax
    jz      @no_xinput

    mov     rcx, rax
    lea     rdx, szGetState
    call    GetProcAddress
    mov     [_pfnGetState], rax

@no_xinput:
    mov     DWORD PTR [_xinputInited], 1
@already:
    add     rsp, 32
    pop     rbx
    ret
Input_InitXInput ENDP

; ============================================================
;  Input_SetKey(rcx=vk [0-255], edx=down [0 or 1])
; ============================================================
Input_SetKey PROC
    ; Clamp vk to 0-255
    movzx   rax, cl
    and     dl, 1
    lea     r8, _keyCurr
    mov     BYTE PTR [r8+rax], dl
    ret
Input_SetKey ENDP

; ============================================================
;  Input_EndFrame — swap current into previous; keep current
;  Called once per game tick AFTER all game logic runs.
; ============================================================
Input_EndFrame PROC
    push    rsi
    push    rdi
    push    rcx
    sub     rsp, 32         ; N=3 pushes → X%16=0. 32%16=0 ✓

    ; Copy _keyCurr → _keyPrev  (256 bytes)
    lea     rsi, _keyCurr
    lea     rdi, _keyPrev
    mov     ecx, 64         ; 256 bytes / 4 = 64 QWORDs
@copy_loop:
    mov     rax, QWORD PTR [rsi]
    mov     QWORD PTR [rdi], rax
    add     rsi, 8
    add     rdi, 8
    dec     ecx
    jnz     @copy_loop

    add     rsp, 32
    pop     rcx
    pop     rdi
    pop     rsi
    ret
Input_EndFrame ENDP

; ============================================================
;  Input_KeyDown(rcx=vk) -> eax = 1 if currently held
; ============================================================
Input_KeyDown PROC
    movzx   rax, cl
    lea     r8, _keyCurr
    movzx   eax, BYTE PTR [r8+rax]
    ret
Input_KeyDown ENDP

; ============================================================
;  Input_KeyPressed(rcx=vk) -> eax = 1 if pressed this frame
;  (curr=1 AND prev=0)
; ============================================================
Input_KeyPressed PROC
    movzx   rax, cl
    lea     r8, _keyCurr
    lea     r9, _keyPrev
    movzx   ecx, BYTE PTR [r8+rax]
    movzx   edx, BYTE PTR [r9+rax]
    not     edx
    and     ecx, edx
    and     ecx, 1
    mov     eax, ecx
    ret
Input_KeyPressed ENDP

; ============================================================
;  Input_KeyReleased(rcx=vk) -> eax = 1 if released this frame
;  (curr=0 AND prev=1)
; ============================================================
Input_KeyReleased PROC
    movzx   rax, cl
    lea     r8, _keyCurr
    lea     r9, _keyPrev
    movzx   ecx, BYTE PTR [r8+rax]
    movzx   edx, BYTE PTR [r9+rax]
    not     ecx
    and     ecx, edx
    and     ecx, 1
    mov     eax, ecx
    ret
Input_KeyReleased ENDP

; ============================================================
;  Input_SetMouse(ecx=x, edx=y)
; ============================================================
Input_SetMouse PROC
    mov     DWORD PTR [_mouseX], ecx
    mov     DWORD PTR [_mouseY], edx
    ret
Input_SetMouse ENDP

; ============================================================
;  Input_MouseX() -> eax
; ============================================================
Input_MouseX PROC
    mov     eax, DWORD PTR [_mouseX]
    ret
Input_MouseX ENDP

; ============================================================
;  Input_MouseY() -> eax
; ============================================================
Input_MouseY PROC
    mov     eax, DWORD PTR [_mouseY]
    ret
Input_MouseY ENDP

; ============================================================
;  Input_PollGamepad()
;  Reads controller 0; stores buttons and stick values.
;  Gracefully no-ops if XInput not available.
; ============================================================
Input_PollGamepad PROC
    push    rbx
    sub     rsp, 48         ; shadow + alignment for XINPUT_STATE on stack

    ; Init XInput on first call
    call    Input_InitXInput

    ; If no function pointer, skip
    mov     rax, [_pfnGetState]
    test    rax, rax
    jz      @gp_done

    ; XInputGetState(dwUserIndex=0, pState=&_gpState)
    ; Call through function pointer: rax = pfnGetState
    xor     ecx, ecx            ; controller index 0
    lea     rdx, _gpState
    call    rax                 ; indirect call

    ; eax = ERROR_SUCCESS (0) if connected
    test    eax, eax
    jnz     @gp_disconnected

    mov     DWORD PTR [_gpConnected], 1

    ; Extract fields from XINPUT_STATE
    ; wButtons at offset 4 in the struct
    movzx   eax, WORD PTR [_gpState+4]
    mov     WORD PTR [_gpButtons], ax

    movsx   eax, SWORD PTR [_gpState+8]
    mov     WORD PTR [_gpLX], ax

    movsx   eax, SWORD PTR [_gpState+10]
    mov     WORD PTR [_gpLY], ax

    movsx   eax, SWORD PTR [_gpState+12]
    mov     WORD PTR [_gpRX], ax

    movsx   eax, SWORD PTR [_gpState+14]
    mov     WORD PTR [_gpRY], ax
    jmp     @gp_done

@gp_disconnected:
    mov     DWORD PTR [_gpConnected], 0
    xor     eax, eax
    mov     WORD PTR [_gpButtons], ax
    mov     WORD PTR [_gpLX], ax
    mov     WORD PTR [_gpLY], ax

@gp_done:
    add     rsp, 48
    pop     rbx
    ret
Input_PollGamepad ENDP

; ============================================================
;  Input_GamepadButton(ecx=buttonMask) -> eax = 1 if pressed
; ============================================================
Input_GamepadButton PROC
    movzx   eax, WORD PTR [_gpButtons]
    and     eax, ecx
    setnz   al
    movzx   eax, al
    ret
Input_GamepadButton ENDP

; ============================================================
;  Input_GamepadLX() -> eax (sign-extended SHORT)
; ============================================================
Input_GamepadLX PROC
    movsx   eax, WORD PTR [_gpLX]
    ret
Input_GamepadLX ENDP

; ============================================================
;  Input_GamepadLY() -> eax (sign-extended SHORT)
; ============================================================
Input_GamepadLY PROC
    movsx   eax, WORD PTR [_gpLY]
    ret
Input_GamepadLY ENDP

; ============================================================
;  Input_GamepadConnected() -> eax = 1 if pad detected
; ============================================================
Input_GamepadConnected PROC
    mov     eax, DWORD PTR [_gpConnected]
    ret
Input_GamepadConnected ENDP

END
