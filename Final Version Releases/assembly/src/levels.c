/*
 * levels.c — Brick layout generation and star-field initialization
 * Ported from Form1.vb: SetupLevel, InitStarField, GetBrickPalette,
 * GenerateProceduralBrickPalette, 8 layout patterns.
 * No CRT. Win32 only.
 */

#include "BrickBlast.h"
#include "bbutil.h"

/* ── Brick palette tables ────────────────────────────────────── */
/*
 * Each palette: BRICK_ROWS entries, each a gradient pair (dark, light).
 * Packed as COLORREF = 0x00BBGGRR.
 */

#define C(r,g,b) ((COLORREF)((b)<<16|(g)<<8|(r)))

/* Palette 0 — Classic */
static const COLORREF _pal0[BRICK_ROWS][2] = {
    { C(220,50,50),  C(255,120,120) },
    { C(210,100,30), C(255,160, 80) },
    { C(200,180,20), C(255,230, 80) },
    { C(40,170,40),  C(100,220,100) },
    { C(30,130,210), C( 80,180,255) },
    { C(100,50,200), C(160,100,255) },
    { C(190,40,160), C(240,100,210) },
};
/* Palette 1 — Toxic */
static const COLORREF _pal1[BRICK_ROWS][2] = {
    { C( 20,180, 20), C( 80,240, 80) },
    { C( 10,150, 50), C( 60,220,100) },
    { C( 50,200, 10), C(100,255, 60) },
    { C( 30,210, 90), C( 90,255,140) },
    { C( 10,160,160), C( 60,220,210) },
    { C( 40,100, 20), C(100,160, 70) },
    { C( 80,220, 40), C(140,255, 90) },
};
/* Palette 2 — Sunset */
static const COLORREF _pal2[BRICK_ROWS][2] = {
    { C(220, 60, 20), C(255,120, 60) },
    { C(210, 80, 10), C(255,140, 50) },
    { C(200,120, 20), C(255,180, 70) },
    { C(180,150, 30), C(240,210, 80) },
    { C(150, 60, 10), C(210,110, 50) },
    { C(190, 30, 30), C(250, 90, 80) },
    { C(160, 40, 80), C(220,100,140) },
};
/* Palette 3 — Forest */
static const COLORREF _pal3[BRICK_ROWS][2] = {
    { C( 20,100, 20), C( 60,160, 60) },
    { C( 30,120, 40), C( 80,180, 90) },
    { C( 10, 80, 30), C( 50,140, 80) },
    { C( 50,140, 20), C(100,200, 70) },
    { C( 40,160, 60), C(100,220,110) },
    { C( 60,100, 20), C(120,160, 70) },
    { C( 20, 60, 10), C( 70,120, 50) },
};
/* Palettes 4-9: reuse slight variations for brevity */
static const COLORREF _pal4[BRICK_ROWS][2] = {   /* Ocean */
    { C( 20, 80,200), C( 70,140,255) },
    { C( 10,100,180), C( 60,160,240) },
    { C( 30,120,210), C( 80,180,255) },
    { C( 10, 60,160), C( 50,120,220) },
    { C( 40,140,200), C( 90,200,255) },
    { C( 20,100,150), C( 70,160,210) },
    { C( 10, 80,120), C( 60,140,180) },
};
static const COLORREF _pal5[BRICK_ROWS][2] = {   /* Galaxy */
    { C( 80, 20,200), C(140, 80,255) },
    { C( 60, 10,180), C(120, 60,240) },
    { C(100, 40,220), C(160,100,255) },
    { C( 50, 10,150), C(110, 60,210) },
    { C(120, 60,210), C(180,120,255) },
    { C( 40, 10,120), C(100, 60,180) },
    { C( 90, 30,190), C(150, 90,250) },
};
static const COLORREF _pal6[BRICK_ROWS][2] = {   /* Gold */
    { C(200,160, 20), C(255,220, 80) },
    { C(180,140, 10), C(240,200, 60) },
    { C(220,180, 30), C(255,240,100) },
    { C(190,150, 10), C(250,210, 60) },
    { C(160,120,  0), C(220,180, 50) },
    { C(200,170, 40), C(255,230,110) },
    { C(180,140, 20), C(240,200, 80) },
};
static const COLORREF _pal7[BRICK_ROWS][2] = {   /* Obsidian */
    { C( 50, 50, 60), C( 90, 90,100) },
    { C( 40, 40, 50), C( 80, 80, 90) },
    { C( 60, 55, 70), C(100, 95,110) },
    { C( 30, 30, 40), C( 70, 70, 80) },
    { C( 70, 65, 80), C(110,105,120) },
    { C( 50, 45, 60), C( 90, 85,100) },
    { C( 35, 35, 45), C( 75, 75, 85) },
};
static const COLORREF _pal8[BRICK_ROWS][2] = {   /* Sakura */
    { C(220,120,150), C(255,180,200) },
    { C(200,100,130), C(240,160,180) },
    { C(230,140,160), C(255,200,220) },
    { C(210,110,140), C(250,170,190) },
    { C(180, 80,110), C(230,140,160) },
    { C(220,130,155), C(255,190,210) },
    { C(200,110,140), C(245,170,195) },
};
static const COLORREF _pal9[BRICK_ROWS][2] = {   /* Aurora */
    { C( 20,180,180), C( 80,240,240) },
    { C( 10,150,200), C( 60,210,255) },
    { C( 40,200,150), C(100,255,210) },
    { C( 20,160,120), C( 80,220,180) },
    { C( 50,220,180), C(110,255,240) },
    { C( 10,140,160), C( 60,200,220) },
    { C( 30,190,140), C( 90,250,200) },
};

static const COLORREF (*_palettes[10])[2] = {
    _pal0,_pal1,_pal2,_pal3,_pal4,_pal5,_pal6,_pal7,_pal8,_pal9
};

/* ── Layout patterns ──────────────────────────────────────────── */
/*
 * 8 patterns (0-7) cycling by level.
 * Each entry is BRICK_ROWS × BRICK_COLS; value = max HP (0 = empty).
 */
static void Layout_Full(GameState* gs, int maxHp) {
    for (int r = 0; r < BRICK_ROWS; r++)
        for (int c = 0; c < BRICK_COLS; c++) {
            gs->bricks[r][c].hp    = maxHp;
            gs->bricks[r][c].maxHp = maxHp;
        }
}

static void Layout_Checkerboard(GameState* gs, int maxHp) {
    for (int r = 0; r < BRICK_ROWS; r++)
        for (int c = 0; c < BRICK_COLS; c++) {
            int hp = ((r + c) % 2 == 0) ? maxHp : 0;
            gs->bricks[r][c].hp = gs->bricks[r][c].maxHp = hp;
        }
}

static void Layout_Diamond(GameState* gs, int maxHp) {
    int cr = BRICK_ROWS / 2, cc = BRICK_COLS / 2;
    int radius = 3;
    for (int r = 0; r < BRICK_ROWS; r++)
        for (int c = 0; c < BRICK_COLS; c++) {
            int d = (r - cr < 0 ? cr - r : r - cr) +
                    (c - cc < 0 ? cc - c : c - cc);
            int hp = (d <= radius) ? maxHp : 0;
            gs->bricks[r][c].hp = gs->bricks[r][c].maxHp = hp;
        }
}

static void Layout_Pyramid(GameState* gs, int maxHp) {
    for (int r = 0; r < BRICK_ROWS; r++) {
        int skip = r;
        int end  = BRICK_COLS - r;
        for (int c = 0; c < BRICK_COLS; c++) {
            int hp = (c >= skip && c < end) ? maxHp : 0;
            gs->bricks[r][c].hp = gs->bricks[r][c].maxHp = hp;
        }
    }
}

static void Layout_InvPyramid(GameState* gs, int maxHp) {
    for (int r = 0; r < BRICK_ROWS; r++) {
        int skip = BRICK_ROWS - 1 - r;
        int end  = BRICK_COLS - skip;
        for (int c = 0; c < BRICK_COLS; c++) {
            int hp = (c >= skip && c < end) ? maxHp : 0;
            gs->bricks[r][c].hp = gs->bricks[r][c].maxHp = hp;
        }
    }
}

static void Layout_Cross(GameState* gs, int maxHp) {
    int cr = BRICK_ROWS / 2, cc = BRICK_COLS / 2;
    for (int r = 0; r < BRICK_ROWS; r++)
        for (int c = 0; c < BRICK_COLS; c++) {
            int hp = (r == cr || c == cc) ? maxHp : 0;
            gs->bricks[r][c].hp = gs->bricks[r][c].maxHp = hp;
        }
}

static void Layout_Borders(GameState* gs, int maxHp) {
    for (int r = 0; r < BRICK_ROWS; r++)
        for (int c = 0; c < BRICK_COLS; c++) {
            int border = (r == 0 || r == BRICK_ROWS-1 ||
                          c == 0 || c == BRICK_COLS-1);
            int hp = border ? maxHp : 0;
            gs->bricks[r][c].hp = gs->bricks[r][c].maxHp = hp;
        }
}

static void Layout_Alternating(GameState* gs, int maxHp) {
    for (int r = 0; r < BRICK_ROWS; r++) {
        int offset = (r % 2);
        for (int c = 0; c < BRICK_COLS; c++) {
            int hp = ((c + offset) % 3 != 2) ? maxHp : 0;
            gs->bricks[r][c].hp = gs->bricks[r][c].maxHp = hp;
        }
    }
}

typedef void (*LayoutFn)(GameState*, int);
static const LayoutFn _layouts[] = {
    Layout_Full, Layout_Checkerboard, Layout_Diamond, Layout_Pyramid,
    Layout_InvPyramid, Layout_Cross, Layout_Borders, Layout_Alternating
};

/* ── Apply palette colors to bricks ─────────────────────────── */
static void ApplyPalette(GameState* gs) {
    int palIdx = gs->skinBrick;
    if (palIdx < 0 || palIdx >= 10) palIdx = 0;
    const COLORREF (*pal)[2] = _palettes[palIdx];

    for (int r = 0; r < BRICK_ROWS; r++) {
        COLORREF dark  = pal[r][0];
        COLORREF light = pal[r][1];
        for (int c = 0; c < BRICK_COLS; c++) {
            /* Alternate between dark and light by column */
            gs->bricks[r][c].color = (c % 2 == 0) ? dark : light;
        }
    }
}

/* ── Levels_Setup ──────────────────────────────────────────────── */
void Levels_Setup(GameState* gs, int level) {
    /* Choose layout (cycles 0-7) */
    int layoutIdx = (level - 1) % 8;

    /* HP scales with level: 1 HP for levels 1-3, 2 for 4-6, etc. */
    int maxHp = 1 + (level - 1) / 3;
    if (maxHp > 4) maxHp = 4;

    /* If daily or endless, use a seeded random layout */
    if (gs->dailyMode) {
        /* Use dailySeed to pick layout and palette */
        layoutIdx = gs->dailySeed % 8;
    }

    _layouts[layoutIdx](gs, maxHp);
    ApplyPalette(gs);

    /* Clear power-ups */
    for (int i = 0; i < MAX_POWERUPS; i++)
        gs->powerups[i].active = 0;

    /* Clear particles */
    for (int i = 0; i < MAX_PARTICLES; i++)
        gs->particles[i].life = 0;

    /* Reset balls */
    for (int i = 0; i < MAX_BALLS; i++)
        gs->balls[i].active = 0;
    gs->ballCount = 1;
    gs->balls[0].active = 1;
    gs->balls[0].radius = BALL_RADIUS;
    gs->widePaddleTimer = 0;
    gs->fireballTimer   = 0;
    gs->ballOnPaddle    = 1;
}

/* ── Levels_InitStars ──────────────────────────────────────────── */
void Levels_InitStars(GameState* gs) {
    /* Seed RNG from time */
    SYSTEMTIME st;
    GetSystemTime(&st);
    gs->rngState = (DWORD)(st.wMilliseconds + st.wSecond * 1000 +
                            st.wMinute * 60000);
    if (gs->rngState == 0) gs->rngState = 0xDEADBEEFu;

    for (int i = 0; i < MAX_STARS; i++) {
        gs->rngState = gs->rngState * 1664525u + 1013904223u;
        gs->stars[i].x = (float)((gs->rngState >> 8) % CANVAS_W);
        gs->rngState = gs->rngState * 1664525u + 1013904223u;
        gs->stars[i].y = (float)((gs->rngState >> 8) % CANVAS_H);
        gs->rngState = gs->rngState * 1664525u + 1013904223u;
        int sz = 1 + (int)((gs->rngState >> 12) % 2);
        gs->stars[i].size = sz;
        gs->rngState = gs->rngState * 1664525u + 1013904223u;
        /* brightness 60-220 */
        int br = 60 + (int)((gs->rngState >> 8) % 160);
        gs->stars[i].brightness = br;
        gs->rngState = gs->rngState * 1664525u + 1013904223u;
        /* slow downward drift */
        gs->stars[i].dy = 0.1f + 0.3f * (float)((gs->rngState >> 8) % 10) / 10.0f;
    }
}
