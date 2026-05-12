; ============================================================
;  render.asm — GDI Double-Buffer Rendering
;  MASM x64  •  Microsoft calling convention
;
;  Exported:
;    Render_InitBackBuffer(hdc, w, h)   — (re)create off-screen bitmap
;    Render_BeginFrame(hdc)             — clear back buffer to black
;    Render_EndFrame(hdc, hwnd)         — BitBlt to window
;    Render_GetDC() -> HDC              — back buffer DC for C drawing code
;    Render_GetW()  -> int              — current back-buffer width
;    Render_GetH()  -> int              — current back-buffer height
; ============================================================

OPTION CASEMAP:NONE

EXTERN CreateCompatibleDC:PROC
EXTERN CreateCompatibleBitmap:PROC
EXTERN SelectObject:PROC
EXTERN DeleteObject:PROC
EXTERN DeleteDC:PROC
EXTERN BitBlt:PROC
EXTERN PatBlt:PROC
EXTERN GetStockObject:PROC
EXTERN CreateSolidBrush:PROC
EXTERN FillRect:PROC
EXTERN GetClientRect:PROC
EXTERN SetBkMode:PROC
EXTERN SetTextColor:PROC
EXTERN SetBkColor:PROC
EXTERN CreateFontW:PROC
EXTERN TextOutW:PROC
EXTERN MoveToEx:PROC
EXTERN LineTo:PROC
EXTERN Ellipse:PROC
EXTERN Rectangle:PROC
EXTERN CreatePen:PROC
EXTERN Polygon:PROC
EXTERN SelectClipRgn:PROC
EXTERN SetStretchBltMode:PROC
EXTERN StretchBlt:PROC
EXTERN GdiFlush:PROC

; ── Constants ───────────────────────────────────────────────────────────────
SRCCOPY         EQU 0CC0020h
BLACKNESS       EQU 0042h
PATCOPY         EQU 0F00021h
BLACK_BRUSH     EQU 4
NULL_PEN        EQU 8
NULL_BRUSH      EQU 5
TRANSPARENT_BK  EQU 1
OPAQUE_BK       EQU 2

; ── Data ────────────────────────────────────────────────────────────────────
.data?
_bbDC           QWORD ?     ; back-buffer DC
_bbBmp          QWORD ?     ; back-buffer bitmap
_bbOldBmp       QWORD ?     ; previous bitmap (for cleanup)
_bbBrush        QWORD ?     ; solid black background brush
_bbW            DWORD ?     ; current buffer width
_bbH            DWORD ?     ; current buffer height

.data
_bbInitialized  DWORD 0

.code

; ============================================================
;  Render_InitBackBuffer(rcx=hdc, edx=w, r8d=h)
;  Creates (or recreates) the off-screen compatible DC/bitmap.
; ============================================================
Render_InitBackBuffer PROC
    push    rbx
    push    rsi
    push    rdi
    sub     rsp, 32         ; N=3 pushes → X%16=0. 32%16=0 ✓

    mov     rbx, rcx        ; hdc
    mov     esi, edx        ; w
    mov     edi, r8d        ; h

    ; Clamp minimum size to 1×1
    test    esi, esi
    jg      @w_ok
    mov     esi, 1
@w_ok:
    test    edi, edi
    jg      @h_ok
    mov     edi, 1
@h_ok:

    ; If already initialized, destroy old resources
    cmp     DWORD PTR [_bbInitialized], 0
    je      @create_new

    ; SelectObject back the original bitmap
    mov     rcx, [_bbDC]
    mov     rdx, [_bbOldBmp]
    call    SelectObject

    ; Delete back-buffer bitmap and DC
    mov     rcx, [_bbBmp]
    call    DeleteObject
    mov     rcx, [_bbDC]
    call    DeleteDC

    ; Delete old brush
    mov     rcx, [_bbBrush]
    call    DeleteObject

@create_new:
    ; CreateCompatibleDC(hdc)
    mov     rcx, rbx
    call    CreateCompatibleDC
    mov     [_bbDC], rax

    ; CreateCompatibleBitmap(hdc, w, h)
    mov     rcx, rbx
    mov     edx, esi
    mov     r8d, edi
    call    CreateCompatibleBitmap
    mov     [_bbBmp], rax

    ; SelectObject(bbDC, bbBmp) — save old
    mov     rcx, [_bbDC]
    mov     rdx, [_bbBmp]
    call    SelectObject
    mov     [_bbOldBmp], rax

    ; Create solid black brush for clear
    xor     rcx, rcx        ; RGB(0,0,0)
    call    CreateSolidBrush
    mov     [_bbBrush], rax

    ; Store dimensions
    mov     DWORD PTR [_bbW], esi
    mov     DWORD PTR [_bbH], edi
    mov     DWORD PTR [_bbInitialized], 1

    add     rsp, 32
    pop     rdi
    pop     rsi
    pop     rbx
    ret
Render_InitBackBuffer ENDP

; ============================================================
;  Render_BeginFrame(rcx=hdc_window)
;  If not initialized yet, auto-init from the window DC.
;  Clears back buffer to black.
; ============================================================
Render_BeginFrame PROC
    push    rbx
    push    rsi
    push    rdi
    sub     rsp, 48         ; N=3 pushes → X%16=0. 48%16=0 ✓; RECT at [rsp+32..+47]

    mov     rbx, rcx        ; window hdc

    ; Auto-init if needed
    cmp     DWORD PTR [_bbInitialized], 0
    jne     @already_init

    ; Use 900×700 default until WM_SIZE fires
    mov     rcx, rbx
    mov     edx, 900
    mov     r8d, 700
    call    Render_InitBackBuffer

@already_init:
    ; FillRect(bbDC, {0,0,w,h}, brush)
    ; Build RECT on stack at [rsp+32]
    mov     DWORD PTR [rsp+32], 0       ; left
    mov     DWORD PTR [rsp+36], 0       ; top
    mov     eax, DWORD PTR [_bbW]
    mov     DWORD PTR [rsp+40], eax     ; right
    mov     eax, DWORD PTR [_bbH]
    mov     DWORD PTR [rsp+44], eax     ; bottom

    mov     rcx, [_bbDC]
    lea     rdx, [rsp+32]
    mov     r8, [_bbBrush]
    call    FillRect

    ; SetBkMode(bbDC, TRANSPARENT)
    mov     rcx, [_bbDC]
    mov     edx, TRANSPARENT_BK
    call    SetBkMode

    add     rsp, 48
    pop     rdi
    pop     rsi
    pop     rbx
    ret
Render_BeginFrame ENDP

; ============================================================
;  Render_EndFrame(rcx=hdc_window, rdx=hwnd) [hwnd unused here]
;  Blits the back buffer to the window DC.
; ============================================================
Render_EndFrame PROC
    push    rbx
    sub     rsp, 80         ; shadow(32) + 5 stack args(40) + align(8)

    mov     rbx, rcx        ; window hdc

    ; BitBlt(hdcDest, x, y, w, h, hdcSrc, x1, y1, rop)  — 9 args
    ; rcx=hdcDest, edx=x, r8d=y, r9d=w
    ; [rsp+32]=h, [rsp+40]=hdcSrc, [rsp+48]=x1, [rsp+56]=y1, [rsp+64]=rop
    mov     rcx, rbx
    xor     edx, edx
    xor     r8d, r8d
    mov     r9d, DWORD PTR [_bbW]
    mov     eax, DWORD PTR [_bbH]
    mov     DWORD PTR [rsp+32], eax
    mov     rax, [_bbDC]
    mov     QWORD PTR [rsp+40], rax
    mov     DWORD PTR [rsp+48], 0
    mov     DWORD PTR [rsp+56], 0
    mov     DWORD PTR [rsp+64], SRCCOPY
    call    BitBlt

    call    GdiFlush

    add     rsp, 80
    pop     rbx
    ret
Render_EndFrame ENDP

; ============================================================
;  Render_GetDC() -> rax = back-buffer HDC
; ============================================================
Render_GetDC PROC
    mov     rax, [_bbDC]
    ret
Render_GetDC ENDP

; ============================================================
;  Render_GetW() -> eax = back-buffer width
; ============================================================
Render_GetW PROC
    mov     eax, DWORD PTR [_bbW]
    ret
Render_GetW ENDP

; ============================================================
;  Render_GetH() -> eax = back-buffer height
; ============================================================
Render_GetH PROC
    mov     eax, DWORD PTR [_bbH]
    ret
Render_GetH ENDP

; ============================================================
;  Font handles — exported so C draw modules can use them
; ============================================================
.data?
PUBLIC g_fontTitle
PUBLIC g_fontLarge
PUBLIC g_fontMed
PUBLIC g_fontSmall
PUBLIC g_fontTiny
g_fontTitle     QWORD ?
g_fontLarge     QWORD ?
g_fontMed       QWORD ?
g_fontSmall     QWORD ?
g_fontTiny      QWORD ?

; Wide string for font face name
.data
_szFontFace     DW 'S','e','g','o','e',' ','U','I',0

; ============================================================
;  Render_CreateFonts()
;  Call once after the window is shown.  Creates all five
;  HFONT handles stored in the globals above.
;
;  CreateFontW(nHeight, nWidth, nEsc, nOrient, nWeight,
;              bItalic, bUnderline, bStrikeOut,
;              nCharSet, nOutPrec, nClipPrec,
;              nQuality, nPitchAndFamily, lpszFace)
;  — 14 parameters, 10 on stack beyond the 4 register args.
; ============================================================
EXTERN CreateFontW:PROC
EXTERN DeleteObject:PROC   ; already declared above; harmless re-extern

; Helper macro to create one font (height in ecx, bold flag in edx)
; Result in rax.  Clobbers rax,rcx,rdx,r8,r9.
; Stack frame for CreateFontW (14 args):
;   rcx  = height
;   edx  = width (0)
;   r8d  = escapement (0)
;   r9d  = orientation (0)
;   [rsp+32]  = weight
;   [rsp+40]  = italic=0
;   [rsp+48]  = underline=0
;   [rsp+56]  = strikeout=0
;   [rsp+64]  = charset=0 (ANSI)
;   [rsp+72]  = outPrec=0
;   [rsp+80]  = clipPrec=0
;   [rsp+88]  = quality=5 (CLEARTYPE)
;   [rsp+96]  = pitchAndFamily=0
;   [rsp+104] = lpFaceName
;
;  Caller must have allocated >=112 bytes of stack above shadow.

FW_NORMAL   EQU 400
FW_BOLD     EQU 700
CLEARTYPE_QUALITY EQU 5

Render_CreateFonts PROC
    push    rbx
    push    rsi
    push    rdi
    sub     rsp, 112        ; N=3 pushes → X%16=0. 112%16=0 ✓; stack args at [rsp+32]..[rsp+104]

    lea     rsi, [_szFontFace]

    ; ── Title font: 54px bold ─────────────────────────────────
    mov     rcx, 54
    xor     edx, edx
    xor     r8d, r8d
    xor     r9d, r9d
    mov     DWORD PTR [rsp+32], FW_BOLD
    mov     DWORD PTR [rsp+40], 0
    mov     DWORD PTR [rsp+48], 0
    mov     DWORD PTR [rsp+56], 0
    mov     DWORD PTR [rsp+64], 0
    mov     DWORD PTR [rsp+72], 0
    mov     DWORD PTR [rsp+80], 0
    mov     DWORD PTR [rsp+88], CLEARTYPE_QUALITY
    mov     DWORD PTR [rsp+96], 0
    mov     QWORD PTR [rsp+104], rsi
    call    CreateFontW
    mov     [g_fontTitle], rax

    ; ── Large font: 36px bold ─────────────────────────────────
    mov     rcx, 36
    xor     edx, edx
    xor     r8d, r8d
    xor     r9d, r9d
    mov     DWORD PTR [rsp+32], FW_BOLD
    mov     DWORD PTR [rsp+40], 0
    mov     DWORD PTR [rsp+48], 0
    mov     DWORD PTR [rsp+56], 0
    mov     DWORD PTR [rsp+64], 0
    mov     DWORD PTR [rsp+72], 0
    mov     DWORD PTR [rsp+80], 0
    mov     DWORD PTR [rsp+88], CLEARTYPE_QUALITY
    mov     DWORD PTR [rsp+96], 0
    mov     QWORD PTR [rsp+104], rsi
    call    CreateFontW
    mov     [g_fontLarge], rax

    ; ── Med font: 22px bold ───────────────────────────────────
    mov     rcx, 22
    xor     edx, edx
    xor     r8d, r8d
    xor     r9d, r9d
    mov     DWORD PTR [rsp+32], FW_BOLD
    mov     DWORD PTR [rsp+40], 0
    mov     DWORD PTR [rsp+48], 0
    mov     DWORD PTR [rsp+56], 0
    mov     DWORD PTR [rsp+64], 0
    mov     DWORD PTR [rsp+72], 0
    mov     DWORD PTR [rsp+80], 0
    mov     DWORD PTR [rsp+88], CLEARTYPE_QUALITY
    mov     DWORD PTR [rsp+96], 0
    mov     QWORD PTR [rsp+104], rsi
    call    CreateFontW
    mov     [g_fontMed], rax

    ; ── Small font: 16px normal ───────────────────────────────
    mov     rcx, 16
    xor     edx, edx
    xor     r8d, r8d
    xor     r9d, r9d
    mov     DWORD PTR [rsp+32], FW_NORMAL
    mov     DWORD PTR [rsp+40], 0
    mov     DWORD PTR [rsp+48], 0
    mov     DWORD PTR [rsp+56], 0
    mov     DWORD PTR [rsp+64], 0
    mov     DWORD PTR [rsp+72], 0
    mov     DWORD PTR [rsp+80], 0
    mov     DWORD PTR [rsp+88], CLEARTYPE_QUALITY
    mov     DWORD PTR [rsp+96], 0
    mov     QWORD PTR [rsp+104], rsi
    call    CreateFontW
    mov     [g_fontSmall], rax

    ; ── Tiny font: 12px normal ────────────────────────────────
    mov     rcx, 12
    xor     edx, edx
    xor     r8d, r8d
    xor     r9d, r9d
    mov     DWORD PTR [rsp+32], FW_NORMAL
    mov     DWORD PTR [rsp+40], 0
    mov     DWORD PTR [rsp+48], 0
    mov     DWORD PTR [rsp+56], 0
    mov     DWORD PTR [rsp+64], 0
    mov     DWORD PTR [rsp+72], 0
    mov     DWORD PTR [rsp+80], 0
    mov     DWORD PTR [rsp+88], CLEARTYPE_QUALITY
    mov     DWORD PTR [rsp+96], 0
    mov     QWORD PTR [rsp+104], rsi
    call    CreateFontW
    mov     [g_fontTiny], rax

    add     rsp, 112
    pop     rdi
    pop     rsi
    pop     rbx
    ret
Render_CreateFonts ENDP

; ============================================================
;  Render_DestroyFonts()
;  Call at shutdown before DestroyWindow.
; ============================================================
Render_DestroyFonts PROC
    sub     rsp, 40
    mov     rcx, [g_fontTitle]
    test    rcx, rcx
    jz      @f1
    call    DeleteObject
@f1:
    mov     rcx, [g_fontLarge]
    test    rcx, rcx
    jz      @f2
    call    DeleteObject
@f2:
    mov     rcx, [g_fontMed]
    test    rcx, rcx
    jz      @f3
    call    DeleteObject
@f3:
    mov     rcx, [g_fontSmall]
    test    rcx, rcx
    jz      @f4
    call    DeleteObject
@f4:
    mov     rcx, [g_fontTiny]
    test    rcx, rcx
    jz      @f5
    call    DeleteObject
@f5:
    add     rsp, 40
    ret
Render_DestroyFonts ENDP

END
