/*
 * draw_game.c — In-game GDI rendering
 * Ported from Form1.vb: Form1_Paint (playing sub-path), DrawCombo,
 * DrawParticles, DrawGetReady, DrawPauseMenu (HUD only), and all
 * ball/brick/power-up/paddle painting.
 * No CRT. Win32 only.
 */

#include "BrickBlast.h"
#include "bbutil.h"

/* ── Helpers ─────────────────────────────────────────────────── */
static void FillEllipseF(HDC hdc, float cx, float cy, float r) {
    int x = (int)(cx - r), y = (int)(cy - r);
    int w = (int)(r * 2.0f), h = (int)(r * 2.0f);
    if (w < 1) w = 1;
    if (h < 1) h = 1;
    Ellipse(hdc, x, y, x + w, y + h);
}

static void DrawRoundRect(HDC hdc, int x, int y, int w, int h, int rx) {
    RoundRect(hdc, x, y, x + w, y + h, rx, rx);
}

/* Draw a single text string centered at x, with optional drop shadow */
static void TextCentered(HDC hdc, const wchar_t* txt, HFONT font,
                          COLORREF clr, int cx, int y, int w) {
    HFONT old = (HFONT)SelectObject(hdc, font);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, clr);
    RECT rc = { cx - w/2, y, cx + w/2, y + 60 };
    DrawTextW(hdc, txt, -1, &rc, DT_CENTER | DT_SINGLELINE | DT_VCENTER);
    SelectObject(hdc, old);
}

static void TextShadow(HDC hdc, const wchar_t* txt, HFONT font,
                       COLORREF clr, int x, int y) {
    HFONT old = (HFONT)SelectObject(hdc, font);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(0,0,0));
    RECT rcS = { x+2, y+2, x+1000, y+100 };
    DrawTextW(hdc, txt, -1, &rcS, DT_LEFT | DT_SINGLELINE);
    SetTextColor(hdc, clr);
    RECT rc  = { x, y, x+1000, y+100 };
    DrawTextW(hdc, txt, -1, &rc, DT_LEFT | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Star field ─────────────────────────────────────────────── */
static void DrawStars(HDC hdc, GameState* gs) {
    for (int i = 0; i < MAX_STARS; i++) {
        int br = gs->stars[i].brightness;
        COLORREF c = RGB(br, br, br + 20 > 255 ? 255 : br + 20);
        HPEN pen = CreatePen(PS_SOLID, gs->stars[i].size, c);
        HPEN old = (HPEN)SelectObject(hdc, pen);
        int sx = (int)gs->stars[i].x;
        int sy = (int)gs->stars[i].y;
        SetPixel(hdc, sx, sy, c);
        if (gs->stars[i].size > 1) SetPixel(hdc, sx+1, sy, c);
        SelectObject(hdc, old);
        DeleteObject(pen);
        /* drift */
        gs->stars[i].y += gs->stars[i].dy;
        if (gs->stars[i].y >= (float)gs->canvasH)
            gs->stars[i].y = 0.0f;
    }
}

/* ── HUD (score, lives, level, combo) ───────────────────────── */
static void DrawHUD(HDC hdc, GameState* gs, HFONT fontSmall, HFONT fontMed) {
    SetBkMode(hdc, TRANSPARENT);

    /* Score */
    wchar_t buf[64];
    _snwprintf_s(buf, 64, _TRUNCATE, L"SCORE: %d", gs->score);
    TextShadow(hdc, buf, fontMed, RGB(255,255,255), 10, 5);

    /* Level */
    _snwprintf_s(buf, 64, _TRUNCATE, L"LVL %d", gs->level);
    {
        SIZE sz; GetTextExtentPoint32W(hdc, buf, (int)wcslen(buf), &sz);
        TextShadow(hdc, buf, fontSmall, RGB(200,220,255),
                   gs->canvasW / 2 - sz.cx / 2, 8);
    }

    /* Lives (hearts) */
    HFONT old = (HFONT)SelectObject(hdc, fontSmall);
    SetTextColor(hdc, RGB(255,80,80));
    RECT rcH = { gs->canvasW - 10 - gs->lives * 18, 8,
                  gs->canvasW, 40 };
    for (int i = 0; i < gs->lives; i++) {
        RECT r2 = { gs->canvasW - 10 - (i+1) * 18, 8,
                     gs->canvasW - 10 - i * 18, 30 };
        DrawTextW(hdc, L"\u2665", 1, &r2, DT_CENTER | DT_VCENTER | DT_SINGLELINE);
    }
    SelectObject(hdc, old);

    /* Speed-boost indicator */
    if (gs->speedBoost) {
        TextShadow(hdc, L"2x", fontMed, RGB(255,220,0),
                   gs->canvasW - 45, gs->canvasH - 36);
    }
}

/* ── Paddle ─────────────────────────────────────────────────── */
static void DrawPaddle(HDC hdc, GameState* gs) {
    int pw = gs->widePaddleTimer > 0 ? PADDLE_WIDTH * 3 / 2 : PADDLE_WIDTH;
    int px = (int)(gs->paddle.x - pw / 2);
    int py = (int)gs->paddle.y;

    /* Choose gradient colors by skin */
    COLORREF topC, botC;
    switch (gs->skinPaddle) {
        case 1:  topC = RGB(255, 80, 40);  botC = RGB(180, 30, 10); break; /* Fire */
        case 2:  topC = RGB(100,180,255);  botC = RGB( 40,100,200); break; /* Ice */
        case 3:  topC = RGB(255,210, 60);  botC = RGB(180,140,  0); break; /* Gold */
        case 4:  topC = RGB(  0,255,180);  botC = RGB(  0,160,120); break; /* Neon */
        case 5:  topC = RGB( 60, 60, 80);  botC = RGB( 20, 20, 40); break; /* Void */
        case 6:  topC = RGB(255,180,200);  botC = RGB(200,100,140); break; /* Sakura */
        case 7:  topC = RGB(140,  0,255);  botC = RGB(255,  0,140); break; /* Rainbow */
        default: topC = RGB(100,100,255);  botC = RGB( 60, 60,180); break; /* Classic */
    }

    /* Simple gradient: draw thin horizontal strips */
    for (int row = 0; row < PADDLE_HEIGHT; row++) {
        float t = (float)row / (float)(PADDLE_HEIGHT - 1);
        int r = (int)(GetRValue(topC) + t * (GetRValue(botC) - GetRValue(topC)));
        int g = (int)(GetGValue(topC) + t * (GetGValue(botC) - GetGValue(topC)));
        int b = (int)(GetBValue(topC) + t * (GetBValue(botC) - GetBValue(topC)));
        HPEN pen = CreatePen(PS_SOLID, 1, RGB(r,g,b));
        HPEN old = (HPEN)SelectObject(hdc, pen);
        MoveToEx(hdc, px,      py + row, NULL);
        LineTo  (hdc, px + pw, py + row);
        SelectObject(hdc, old);
        DeleteObject(pen);
    }

    /* Rounded edge highlight */
    HPEN hpen = CreatePen(PS_SOLID, 1, RGB(200,200,255));
    HPEN bold = (HPEN)SelectObject(hdc, hpen);
    HBRUSH hbr = (HBRUSH)GetStockObject(NULL_BRUSH);
    HBRUSH old2 = (HBRUSH)SelectObject(hdc, hbr);
    RoundRect(hdc, px, py, px + pw, py + PADDLE_HEIGHT, 6, 6);
    SelectObject(hdc, bold);
    SelectObject(hdc, old2);
    DeleteObject(hpen);
}

/* ── Ball ───────────────────────────────────────────────────── */
static void DrawBall(HDC hdc, GameState* gs, Ball* b) {
    if (!b->active) return;
    int cx = (int)b->x;
    int cy = (int)b->y;
    int r  = (int)b->radius;

    COLORREF clr;
    /* Fireball overrides skin */
    if (gs->fireballTimer > 0) {
        clr = RGB(255, 100, 0);
    } else {
        switch (gs->skinBall) {
            case 1:  clr = RGB(255, 80,  0); break; /* Fire */
            case 2:  clr = RGB(150,220,255); break; /* Ice */
            case 3:  clr = RGB(180,120,255); break; /* Plasma */
            case 4:  clr = RGB(255,210, 50); break; /* Gold */
            case 5:  clr = RGB(  0,255,180); break; /* Rainbow */
            case 6:  clr = RGB(220, 60, 20); break; /* Lava */
            case 7:  clr = RGB( 40, 10, 70); break; /* Void */
            case 8:  clr = RGB( 60,220, 60); break; /* Toxic */
            case 9:  clr = RGB(  0,255,255); break; /* Neon */
            case 10: clr = RGB(200,240,255); break; /* Crystal */
            case 11: clr = RGB( 80, 80,100); break; /* Shadow */
            case 12: clr = RGB(255,180,200); break; /* Sakura */
            default: clr = RGB(255,255,255); break; /* Classic */
        }
    }

    HPEN pen = CreatePen(PS_SOLID, 1, clr);
    HBRUSH br = CreateSolidBrush(clr);
    HPEN oldP = (HPEN)SelectObject(hdc, pen);
    HBRUSH oldB = (HBRUSH)SelectObject(hdc, br);
    Ellipse(hdc, cx-r, cy-r, cx+r, cy+r);
    SelectObject(hdc, oldP); SelectObject(hdc, oldB);
    DeleteObject(pen); DeleteObject(br);

    /* Specular highlight */
    HBRUSH spec = CreateSolidBrush(RGB(255,255,255));
    HBRUSH oldS = (HBRUSH)SelectObject(hdc, spec);
    HPEN nopen = (HPEN)GetStockObject(NULL_PEN);
    HPEN oldNP = (HPEN)SelectObject(hdc, nopen);
    int hr = r / 3; if (hr < 1) hr = 1;
    Ellipse(hdc, cx - r/3, cy - r/3, cx - r/3 + hr, cy - r/3 + hr);
    SelectObject(hdc, oldS); SelectObject(hdc, oldNP);
    DeleteObject(spec);
}

/* ── Bricks ──────────────────────────────────────────────────── */
static void DrawBricks(HDC hdc, GameState* gs) {
    for (int r = 0; r < BRICK_ROWS; r++) {
        for (int c = 0; c < BRICK_COLS; c++) {
            Brick* bk = &gs->bricks[r][c];
            if (bk->hp <= 0) continue;

            int x = BRICK_LEFT_OFFSET + c * (BRICK_WIDTH  + BRICK_PADDING);
            int y = BRICK_TOP_OFFSET  + r * (BRICK_HEIGHT + BRICK_PADDING);

            /* Darken by damage: lerp toward black */
            float dmg = 1.0f - (float)bk->hp / (float)bk->maxHp;
            COLORREF base = bk->color;
            int br = (int)(GetRValue(base) * (1.0f - dmg * 0.5f));
            int bg = (int)(GetGValue(base) * (1.0f - dmg * 0.5f));
            int bb = (int)(GetBValue(base) * (1.0f - dmg * 0.5f));

            HBRUSH brsh = CreateSolidBrush(RGB(br, bg, bb));
            HPEN   pen  = CreatePen(PS_SOLID, 1, RGB(0,0,0));
            HBRUSH oldB = (HBRUSH)SelectObject(hdc, brsh);
            HPEN   oldP = (HPEN)SelectObject(hdc, pen);
            RoundRect(hdc, x, y, x + BRICK_WIDTH, y + BRICK_HEIGHT, 4, 4);
            SelectObject(hdc, oldB); SelectObject(hdc, oldP);
            DeleteObject(brsh); DeleteObject(pen);

            /* Highlight strip */
            HPEN hpen = CreatePen(PS_SOLID, 1, RGB(
                min(br + 60, 255), min(bg + 60, 255), min(bb + 60, 255)));
            HPEN oldH = (HPEN)SelectObject(hdc, hpen);
            HBRUSH nb = (HBRUSH)GetStockObject(NULL_BRUSH);
            HBRUSH oldNB = (HBRUSH)SelectObject(hdc, nb);
            MoveToEx(hdc, x + 3, y + 2, NULL);
            LineTo  (hdc, x + BRICK_WIDTH - 3, y + 2);
            SelectObject(hdc, oldH); SelectObject(hdc, oldNB);
            DeleteObject(hpen);

            /* HP pip dots for multi-hit bricks */
            if (bk->maxHp > 1) {
                HBRUSH pip = CreateSolidBrush(RGB(255,255,255));
                HBRUSH oldPip = (HBRUSH)SelectObject(hdc, pip);
                HPEN npPen = (HPEN)GetStockObject(NULL_PEN);
                HPEN oldNPP = (HPEN)SelectObject(hdc, npPen);
                for (int p = 0; p < bk->hp; p++) {
                    int px = x + 4 + p * 6;
                    int py = y + BRICK_HEIGHT - 5;
                    Ellipse(hdc, px, py, px + 4, py + 4);
                }
                SelectObject(hdc, oldPip); SelectObject(hdc, oldNPP);
                DeleteObject(pip);
            }
        }
    }
}

/* ── Power-ups ───────────────────────────────────────────────── */
static COLORREF PowerUpColor(PowerUpType t) {
    switch (t) {
        case PU_MULTIBALL:    return RGB( 80,180,255);
        case PU_WIDE_PADDLE:  return RGB(100,255,100);
        case PU_SLOW_BALL:    return RGB(  0,220,220);
        case PU_FAST_BALL:    return RGB(255, 80, 80);
        case PU_EXTRA_LIFE:   return RGB(255, 80,160);
        case PU_FIREBALL:     return RGB(255,140,  0);
        case PU_SHRINK_BALL:  return RGB(200,100,255);
        case PU_GROW_BALL:    return RGB(255,200,  0);
        default:              return RGB(200,200,200);
    }
}

static const wchar_t* PowerUpLabel(PowerUpType t) {
    switch (t) {
        case PU_MULTIBALL:   return L"M";
        case PU_WIDE_PADDLE: return L"W";
        case PU_SLOW_BALL:   return L"S";
        case PU_FAST_BALL:   return L"F";
        case PU_EXTRA_LIFE:  return L"\u2665";
        case PU_FIREBALL:    return L"Fire";
        case PU_SHRINK_BALL: return L"-";
        case PU_GROW_BALL:   return L"+";
        default:             return L"?";
    }
}

static void DrawPowerUps(HDC hdc, GameState* gs, HFONT fontSmall) {
    for (int i = 0; i < MAX_POWERUPS; i++) {
        PowerUp* pu = &gs->powerups[i];
        if (!pu->active) continue;

        int x = (int)(pu->x - POWERUP_SIZE / 2);
        int y = (int)(pu->y - POWERUP_SIZE / 2);

        COLORREF clr = PowerUpColor(pu->type);
        HBRUSH br = CreateSolidBrush(clr);
        HPEN  pen = CreatePen(PS_SOLID, 2, RGB(255,255,255));
        HBRUSH oldB = (HBRUSH)SelectObject(hdc, br);
        HPEN   oldP = (HPEN)SelectObject(hdc, pen);
        RoundRect(hdc, x, y, x + POWERUP_SIZE, y + POWERUP_SIZE, 8, 8);
        SelectObject(hdc, oldB); SelectObject(hdc, oldP);
        DeleteObject(br); DeleteObject(pen);

        /* Label */
        HFONT old = (HFONT)SelectObject(hdc, fontSmall);
        SetBkMode(hdc, TRANSPARENT);
        SetTextColor(hdc, RGB(255,255,255));
        RECT rc = { x, y, x + POWERUP_SIZE, y + POWERUP_SIZE };
        DrawTextW(hdc, PowerUpLabel(pu->type), -1, &rc,
                  DT_CENTER | DT_VCENTER | DT_SINGLELINE);
        SelectObject(hdc, old);
    }
}

/* ── Particles ───────────────────────────────────────────────── */
static void DrawParticles(HDC hdc, GameState* gs) {
    for (int i = 0; i < MAX_PARTICLES; i++) {
        Particle* p = &gs->particles[i];
        if (p->life <= 0) continue;

        float alpha = (float)p->life / (float)p->maxLife;
        int   a = (int)(alpha * 255);
        int   r = GetRValue(p->color);
        int   g = GetGValue(p->color);
        int   b = GetBValue(p->color);
        /* Fade to black */
        COLORREF faded = RGB((int)(r * alpha), (int)(g * alpha), (int)(b * alpha));

        HPEN pen = CreatePen(PS_SOLID, p->size, faded);
        HPEN old = (HPEN)SelectObject(hdc, pen);
        int px = (int)p->x, py = (int)p->y;
        SetPixel(hdc, px, py, faded);
        if (p->size > 1) {
            SetPixel(hdc, px+1, py,   faded);
            SetPixel(hdc, px,   py+1, faded);
        }
        SelectObject(hdc, old);
        DeleteObject(pen);
    }
}

/* ── Combo text ──────────────────────────────────────────────── */
static void DrawCombo(HDC hdc, GameState* gs, HFONT fontLarge) {
    if (gs->combo < 2) return;

    /* Animate opacity based on comboTimer */
    float alpha = (float)gs->comboTimer / 60.0f;
    if (alpha > 1.0f) alpha = 1.0f;
    int a = (int)(alpha * 255);

    wchar_t buf[32];
    _snwprintf_s(buf, 32, _TRUNCATE, L"%dx COMBO!", gs->combo);

    HFONT old = (HFONT)SelectObject(hdc, fontLarge);
    SetBkMode(hdc, TRANSPARENT);
    /* Shadow */
    SetTextColor(hdc, RGB(0,0,0));
    RECT rcS = { 2, gs->canvasH / 2 - 38, gs->canvasW + 2, gs->canvasH / 2 + 2 };
    DrawTextW(hdc, buf, -1, &rcS, DT_CENTER | DT_SINGLELINE);
    /* Main */
    SetTextColor(hdc, RGB(255, 220, 0));
    RECT rc = { 0, gs->canvasH / 2 - 40, gs->canvasW, gs->canvasH / 2 };
    DrawTextW(hdc, buf, -1, &rc, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── "Get Ready" overlay ─────────────────────────────────────── */
static void DrawGetReady(HDC hdc, GameState* gs,
                          HFONT fontLarge, HFONT fontSmall) {
    /* Semi-transparent dark bar */
    HBRUSH dim = CreateSolidBrush(RGB(0, 0, 20));
    HBRUSH old = (HBRUSH)SelectObject(hdc, dim);
    RECT bar = { 0, gs->canvasH/2 - 50, gs->canvasW, gs->canvasH/2 + 50 };
    FillRect(hdc, &bar, dim);
    SelectObject(hdc, old);
    DeleteObject(dim);

    HFONT fo = (HFONT)SelectObject(hdc, fontLarge);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(255,255,255));
    RECT r1 = { 0, gs->canvasH/2 - 40, gs->canvasW, gs->canvasH/2 + 10 };
    DrawTextW(hdc, L"GET READY!", -1, &r1, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, fo);

    fo = (HFONT)SelectObject(hdc, fontSmall);
    SetTextColor(hdc, RGB(180,180,255));
    RECT r2 = { 0, gs->canvasH/2 + 15, gs->canvasW, gs->canvasH/2 + 45 };
    DrawTextW(hdc, L"Press SPACE or click to launch", -1, &r2,
              DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, fo);
}

/* ── Pause overlay ───────────────────────────────────────────── */
static void DrawPauseOverlay(HDC hdc, GameState* gs,
                              HFONT fontLarge, HFONT fontSmall) {
    HBRUSH dim = CreateSolidBrush(RGB(0,0,0));
    RECT full = { 0,0, gs->canvasW, gs->canvasH };
    /* Dim without true alpha — paint a dark semi-fill via PatBlt */
    SetBkMode(hdc, OPAQUE);
    SetBkColor(hdc, RGB(0,0,20));
    HBRUSH old = (HBRUSH)SelectObject(hdc, dim);
    /* draw stripes for semi-transparent feel */
    for (int y = 0; y < gs->canvasH; y += 2) {
        RECT stripe = { 0, y, gs->canvasW, y+1 };
        FillRect(hdc, &stripe, dim);
    }
    SelectObject(hdc, old);
    DeleteObject(dim);

    SetBkMode(hdc, TRANSPARENT);
    HFONT fo = (HFONT)SelectObject(hdc, fontLarge);
    SetTextColor(hdc, RGB(255,255,255));
    RECT r1 = { 0, gs->canvasH/2 - 60, gs->canvasW, gs->canvasH/2 };
    DrawTextW(hdc, L"PAUSED", -1, &r1, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, fo);

    fo = (HFONT)SelectObject(hdc, fontSmall);
    SetTextColor(hdc, RGB(180,180,255));
    RECT r2 = { 0, gs->canvasH/2 + 10, gs->canvasW, gs->canvasH/2 + 40 };
    DrawTextW(hdc, L"P / ESC — Resume   |   Q — Quit", -1, &r2,
              DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, fo);
}

/* ── Public entry point ──────────────────────────────────────── */
void DrawGame(HDC hdc, GameState* gs,
              HFONT fontLarge, HFONT fontMed, HFONT fontSmall) {
    DrawStars(hdc, gs);
    DrawBricks(hdc, gs);
    DrawPaddle(hdc, gs);

    for (int i = 0; i < MAX_BALLS; i++)
        DrawBall(hdc, gs, &gs->balls[i]);

    DrawPowerUps(hdc, gs, fontSmall);
    DrawParticles(hdc, gs);
    DrawHUD(hdc, gs, fontSmall, fontMed);
    DrawCombo(hdc, gs, fontLarge);

    if (gs->screen == SCREEN_GETREADY)
        DrawGetReady(hdc, gs, fontLarge, fontSmall);

    if (gs->screen == SCREEN_PAUSE)
        DrawPauseOverlay(hdc, gs, fontLarge, fontSmall);
}

/* Entry point called by game.c */
void DrawGame_Frame(HDC hdc, const GameState* gs) {
    DrawGame(hdc, (GameState*)gs, g_fontLarge, g_fontMed, g_fontSmall);
}
