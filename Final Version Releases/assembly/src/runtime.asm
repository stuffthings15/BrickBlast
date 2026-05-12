;
; runtime.asm — Minimal no-CRT runtime stubs
; Provides: _fltused, memset, memcpy, sqrtf, sin
; All use Microsoft x64 calling convention.
;
OPTION CASEMAP:NONE

.DATA
    PUBLIC _fltused
    _fltused  DD 9875h   ; magic value the linker expects for FP code

.CODE

; ── memset(void* dst, int val, size_t count) → dst ──────────────────────
PUBLIC memset
memset PROC
    ; rcx = dst, rdx = val (byte), r8 = count
    push    rdi
    mov     r9,  rcx        ; save original dst for return
    mov     rdi, rcx        ; destination
    movzx   eax, dl         ; byte value (zero-extended)
    mov     rcx, r8         ; count
    rep     stosb
    mov     rax, r9         ; return original dst
    pop     rdi
    ret
memset ENDP

; ── memcpy(void* dst, const void* src, size_t count) → dst ──────────────
PUBLIC memcpy
memcpy PROC
    ; rcx = dst, rdx = src, r8 = count
    push    rsi
    push    rdi
    mov     rdi, rcx        ; dst
    mov     rsi, rdx        ; src
    mov     rcx, r8         ; count
    rep     movsb
    pop     rdi
    pop     rsi
    ret
memcpy ENDP

; ── sqrtf(float x) → float ───────────────────────────────────────────────
PUBLIC sqrtf
sqrtf PROC
    ; xmm0 = x (float)
    sqrtss  xmm0, xmm0
    ret
sqrtf ENDP

; ── sin(double x) → double  (x87 FSIN) ───────────────────────────────────
PUBLIC sin
sin PROC
    sub     rsp, 8
    movsd   QWORD PTR [rsp], xmm0
    fld     QWORD PTR [rsp]
    fsin
    fstp    QWORD PTR [rsp]
    movsd   xmm0, QWORD PTR [rsp]
    add     rsp, 8
    ret
sin ENDP

END
