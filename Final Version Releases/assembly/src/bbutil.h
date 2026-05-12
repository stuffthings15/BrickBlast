/*
 * bbutil.h — No-CRT string/math helpers for BrickBlast draw modules.
 * All functions map to Win32 or inline equivalents.
 * Include after BrickBlast.h.
 */

#pragma once

/* ── String helpers (Win32, no CRT) ─────────────────────────── */

/* Safe formatted wide string — wraps wsprintfW (no buffer-overflow
 * protection beyond caller discipline; keep buffers ≥64 wchars).
 * Usage matches _snwprintf_s(buf, count, _TRUNCATE, fmt, ...) */
#define _snwprintf_s(buf, count, trunc, ...) wsprintfW(buf, __VA_ARGS__)

/* Safe wide copy */
#define wcscpy_s(dst, count, src) lstrcpynW(dst, src, (count))

/* Wide length */
#define wcslen(s) ((int)lstrlenW(s))

/* ── Math helpers ────────────────────────────────────────────── */
#ifndef min
#define min(a,b) ((a)<(b)?(a):(b))
#endif
#ifndef max
#define max(a,b) ((a)>(b)?(a):(b))
#endif
