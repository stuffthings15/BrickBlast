/*
 * draw_screens.c — All non-gameplay screen rendering
 * Covers: main menu, game-over, high-score, options, store, stats,
 *         credits, daily-challenge menu, name-entry, and level-win overlay.
 * Ported from Form1.vb DrawGameOverScreen, DrawHighScore, DrawStore,
 * DrawStats, DrawCredits, DrawOptions, DrawDailyChallengeMenu, DrawOverlay.
 * No CRT. Win32 only.
 */

#include "BrickBlast.h"
#include "bbutil.h"

/* ── Internal helpers ────────────────────────────────────────── */
static void ClearBg(HDC hdc, GameState* gs) {
    HBRUSH bg = CreateSolidBrush(RGB(8, 8, 24));
    RECT full = { 0, 0, gs->canvasW, gs->canvasH };
    FillRect(hdc, &full, bg);
    DeleteObject(bg);
}

static void DrawTitle(HDC hdc, GameState* gs,
                       const wchar_t* top, const wchar_t* sub) {
    SetBkMode(hdc, TRANSPARENT);

    /* Shadow */
    HFONT old = (HFONT)SelectObject(hdc, g_fontTitle);
    SetTextColor(hdc, RGB(0, 0, 0));
    RECT rs = { 2, 32, gs->canvasW + 2, 102 };
    DrawTextW(hdc, top, -1, &rs, DT_CENTER | DT_SINGLELINE);
    /* Main gradient sim: two passes */
    SetTextColor(hdc, RGB(80, 140, 255));
    RECT rm = { 0, 30, gs->canvasW, 100 };
    DrawTextW(hdc, top, -1, &rm, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    if (sub && sub[0]) {
        old = (HFONT)SelectObject(hdc, g_fontSmall);
        SetTextColor(hdc, RGB(160, 180, 220));
        RECT rs2 = { 0, 100, gs->canvasW, 130 };
        DrawTextW(hdc, sub, -1, &rs2, DT_CENTER | DT_SINGLELINE);
        SelectObject(hdc, old);
    }
}

static void DrawMenuItem(HDC hdc, int cx, int y, int w, int h,
                          const wchar_t* txt, BOOL selected, HFONT font) {
    COLORREF bgC  = selected ? RGB(60, 80, 160) : RGB(20, 20, 50);
    COLORREF txtC = selected ? RGB(255,255,255)  : RGB(180,200,240);
    HBRUSH br = CreateSolidBrush(bgC);
    HPEN  pen = CreatePen(PS_SOLID, 1,
                           selected ? RGB(100,140,255) : RGB(40,40,80));
    HBRUSH oldB = (HBRUSH)SelectObject(hdc, br);
    HPEN   oldP = (HPEN)SelectObject(hdc, pen);
    RoundRect(hdc, cx - w/2, y, cx + w/2, y + h, 10, 10);
    SelectObject(hdc, oldB); SelectObject(hdc, oldP);
    DeleteObject(br); DeleteObject(pen);

    HFONT old = (HFONT)SelectObject(hdc, font);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, txtC);
    RECT rc = { cx - w/2, y, cx + w/2, y + h };
    DrawTextW(hdc, txt, -1, &rc, DT_CENTER | DT_VCENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

static void HRule(HDC hdc, int x1, int x2, int y, COLORREF c) {
    HPEN pen = CreatePen(PS_SOLID, 1, c);
    HPEN old = (HPEN)SelectObject(hdc, pen);
    MoveToEx(hdc, x1, y, NULL);
    LineTo(hdc, x2, y);
    SelectObject(hdc, old);
    DeleteObject(pen);
}

/* ── Main Menu ───────────────────────────────────────────────── */
void DrawMainMenu(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);

    /* Draw a starfield effect — just use pre-built stars list */
    for (int i = 0; i < MAX_STARS; i++) {
        int br = gs->stars[i].brightness;
        SetPixel(hdc, (int)gs->stars[i].x, (int)gs->stars[i].y,
                 RGB(br, br, br));
    }

    DrawTitle(hdc, gs, L"BRICK BLAST", L"Team Fast Talk");

    int cx = gs->canvasW / 2;
    int mw = 280, mh = 48;
    int y0 = 170;
    int gap = 58;

    /* Menu entries */
    const wchar_t* items[] = {
        L"\u25B6  Play",
        L"\u2605  Daily Challenge",
        L"\u221E  Endless Mode",
        L"\u2665  Store",
        L"\u2699  Options",
        L"\u2139  Credits"
    };
    int n = 6;
    for (int i = 0; i < n; i++) {
        DrawMenuItem(hdc, cx, y0 + i * gap, mw, mh, items[i],
                     (gs->menuSelection == i), g_fontMed);
    }

    /* Version / hi-score footer */
    wchar_t buf[64];
    _snwprintf_s(buf, 64, _TRUNCATE, L"Best: %d", gs->hiScore);
    HFONT old = (HFONT)SelectObject(hdc, g_fontTiny);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(100, 120, 160));
    RECT rf = { 0, gs->canvasH - 24, gs->canvasW, gs->canvasH };
    DrawTextW(hdc, buf, -1, &rf, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Generic overlay (Level Win / Title overlay) ─────────────── */
void DrawOverlay(HDC hdc, GameState* gs,
                  const wchar_t* title, const wchar_t* sub) {
    /* Dark vignette */
    HBRUSH dim = CreateSolidBrush(RGB(0,0,10));
    for (int y = 0; y < gs->canvasH; y += 2) {
        RECT s = { 0, y, gs->canvasW, y + 1 };
        FillRect(hdc, &s, dim);
    }
    DeleteObject(dim);

    SetBkMode(hdc, TRANSPARENT);
    HFONT old = (HFONT)SelectObject(hdc, g_fontLarge);
    SetTextColor(hdc, RGB(255,220,60));
    RECT r1 = { 0, gs->canvasH/2 - 60, gs->canvasW, gs->canvasH/2 };
    DrawTextW(hdc, title, -1, &r1, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    old = (HFONT)SelectObject(hdc, g_fontSmall);
    SetTextColor(hdc, RGB(200,220,255));
    RECT r2 = { 0, gs->canvasH/2 + 10, gs->canvasW, gs->canvasH/2 + 45 };
    DrawTextW(hdc, sub, -1, &r2, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Game Over ───────────────────────────────────────────────── */
void DrawGameOver(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"GAME OVER", NULL);

    int cx = gs->canvasW / 2;
    wchar_t buf[64];

    _snwprintf_s(buf, 64, _TRUNCATE, L"Score: %d", gs->score);
    HFONT old = (HFONT)SelectObject(hdc, g_fontLarge);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(255,200,60));
    RECT r1 = { 0, 130, gs->canvasW, 180 };
    DrawTextW(hdc, buf, -1, &r1, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    if (gs->score >= gs->hiScore) {
        old = (HFONT)SelectObject(hdc, g_fontMed);
        SetTextColor(hdc, RGB(255, 80, 150));
        RECT rhs = { 0, 185, gs->canvasW, 220 };
        DrawTextW(hdc, L"\u2605 NEW HIGH SCORE! \u2605", -1, &rhs,
                  DT_CENTER | DT_SINGLELINE);
        SelectObject(hdc, old);
    }

    _snwprintf_s(buf, 64, _TRUNCATE, L"Best: %d   Level: %d", gs->hiScore, gs->level);
    old = (HFONT)SelectObject(hdc, g_fontSmall);
    SetTextColor(hdc, RGB(160, 180, 220));
    RECT r2 = { 0, 225, gs->canvasW, 255 };
    DrawTextW(hdc, buf, -1, &r2, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    HRule(hdc, cx - 120, cx + 120, 268, RGB(40,50,90));

    int mw = 260, mh = 46, y0 = 280;
    DrawMenuItem(hdc, cx, y0,       mw, mh, L"\u25B6  Play Again",  gs->menuSelection == 0, g_fontMed);
    DrawMenuItem(hdc, cx, y0 + 54,  mw, mh, L"\u2302  Main Menu",   gs->menuSelection == 1, g_fontMed);
}

/* ── High Score screen ───────────────────────────────────────── */
void DrawHighScore(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"\u2605 HIGH SCORE \u2605", NULL);

    wchar_t buf[64];
    _snwprintf_s(buf, 64, _TRUNCATE, L"%d", gs->hiScore);

    HFONT old = (HFONT)SelectObject(hdc, g_fontTitle);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(255, 220, 60));
    RECT r1 = { 0, 130, gs->canvasW, 210 };
    DrawTextW(hdc, buf, -1, &r1, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    old = (HFONT)SelectObject(hdc, g_fontSmall);
    SetTextColor(hdc, RGB(160, 180, 220));
    RECT r2 = { 0, 220, gs->canvasW, 250 };
    DrawTextW(hdc, L"Press any key or click to continue", -1, &r2,
              DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Options ─────────────────────────────────────────────────── */
static void DrawVolumeBar(HDC hdc, int x, int y, int w, int h,
                           int val, COLORREF barC) {
    /* Track */
    HBRUSH trk = CreateSolidBrush(RGB(30,30,60));
    RECT rc = { x, y, x+w, y+h };
    FillRect(hdc, &rc, trk);
    DeleteObject(trk);

    /* Fill */
    int fw = (int)((float)val / 100.0f * w);
    if (fw > 0) {
        HBRUSH br = CreateSolidBrush(barC);
        RECT rf2 = { x, y, x+fw, y+h };
        FillRect(hdc, &rf2, br);
        DeleteObject(br);
    }

    /* Border */
    HPEN pen = CreatePen(PS_SOLID, 1, RGB(60,80,120));
    HPEN old = (HPEN)SelectObject(hdc, pen);
    HBRUSH nb = (HBRUSH)GetStockObject(NULL_BRUSH);
    HBRUSH oldB = (HBRUSH)SelectObject(hdc, nb);
    Rectangle(hdc, x, y, x+w, y+h);
    SelectObject(hdc, old); SelectObject(hdc, oldB);
    DeleteObject(pen);
}

void DrawOptions(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"\u2699 OPTIONS", NULL);

    int cx = gs->canvasW / 2;
    int x0 = 80, bw = gs->canvasW - 160;
    int y  = 130;
    int lh = 50;

    SetBkMode(hdc, TRANSPARENT);

    struct { const wchar_t* label; int val; BOOL isToggle; const wchar_t* valTxt; } rows[] = {
        { L"Music Volume",  gs->volMusic,    FALSE, NULL },
        { L"SFX Volume",    gs->volSFX,      FALSE, NULL },
        { L"Music Style",   gs->musicStyle,  TRUE,  gs->musicStyleName },
        { L"SFX Style",     gs->sfxStyle,    TRUE,  gs->sfxStyleName   },
        { L"Colorblind",    gs->colorblind,  TRUE,  gs->colorblind ? L"ON" : L"OFF" },
        { L"Window Scale",  gs->windowScale, TRUE,  NULL },
    };

    for (int i = 0; i < 6; i++) {
        BOOL sel = (gs->optionsSelection == i);
        COLORREF lc = sel ? RGB(255,255,255) : RGB(160,180,220);

        HFONT old = (HFONT)SelectObject(hdc, g_fontSmall);
        SetTextColor(hdc, lc);
        RECT rl = { x0, y + 12, cx - 10, y + lh };
        DrawTextW(hdc, rows[i].label, -1, &rl, DT_RIGHT | DT_VCENTER | DT_SINGLELINE);
        SelectObject(hdc, old);

        if (!rows[i].isToggle) {
            DrawVolumeBar(hdc, cx + 10, y + 14, bw / 2, 20,
                           rows[i].val, RGB(80,140,255));
        } else {
            wchar_t tbuf[64] = L"";
            if (rows[i].valTxt) {
                _snwprintf_s(tbuf, 64, _TRUNCATE, L"< %s >", rows[i].valTxt);
            } else {
                _snwprintf_s(tbuf, 64, _TRUNCATE, L"< %d >", rows[i].val);
            }
            old = (HFONT)SelectObject(hdc, g_fontSmall);
            SetTextColor(hdc, RGB(255,200,60));
            RECT rv = { cx + 10, y + 10, cx + 10 + bw/2, y + lh };
            DrawTextW(hdc, tbuf, -1, &rv, DT_LEFT | DT_VCENTER | DT_SINGLELINE);
            SelectObject(hdc, old);
        }

        if (sel) HRule(hdc, x0, gs->canvasW - x0, y + lh - 1, RGB(60,80,200));

        y += lh;
    }

    /* Back hint */
    HFONT old = (HFONT)SelectObject(hdc, g_fontTiny);
    SetTextColor(hdc, RGB(80,100,140));
    RECT rb = { 0, gs->canvasH - 28, gs->canvasW, gs->canvasH };
    DrawTextW(hdc, L"ESC — Back   \u2190\u2192 or drag — Adjust", -1, &rb,
              DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Store ───────────────────────────────────────────────────── */
void DrawStore(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"\u2665 STORE", NULL);

    int cx = gs->canvasW / 2;

    /* Coin balance */
    wchar_t cbuf[32];
    _snwprintf_s(cbuf, 32, _TRUNCATE, L"[coin] %d", gs->coinBalance);
    HFONT old = (HFONT)SelectObject(hdc, g_fontSmall);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(255,210,60));
    RECT rc = { gs->canvasW - 120, 30, gs->canvasW - 10, 60 };
    DrawTextW(hdc, cbuf, -1, &rc, DT_RIGHT | DT_SINGLELINE);
    SelectObject(hdc, old);

    /* Category tabs */
    const wchar_t* tabs[] = { L"Balls", L"Bricks", L"Bonus", L"Paddles", L"Music", L"SFX" };
    int nTabs = 6, tw = gs->canvasW / nTabs;
    for (int t = 0; t < nTabs; t++) {
        BOOL active = (gs->storeCategory == t);
        HBRUSH tb = CreateSolidBrush(active ? RGB(50,70,150) : RGB(15,15,40));
        RECT tr = { t * tw, 95, (t+1) * tw, 125 };
        FillRect(hdc, &tr, tb);
        DeleteObject(tb);

        old = (HFONT)SelectObject(hdc, g_fontTiny);
        SetTextColor(hdc, active ? RGB(255,255,255) : RGB(120,140,180));
        DrawTextW(hdc, tabs[t], -1, &tr, DT_CENTER | DT_VCENTER | DT_SINGLELINE);
        SelectObject(hdc, old);
    }
    HRule(hdc, 0, gs->canvasW, 125, RGB(40,50,100));

    /* Items grid — 3 columns */
    int cols = 3;
    int cardW = (gs->canvasW - 40) / cols;
    int cardH = 100;
    int startY = 135;
    int visible = 6; /* max visible per scroll page */

    int shown = 0;
    for (int i = gs->storeScrollOffset;
         i < gs->storeItemCount && shown < visible; i++) {
        StoreItem* si = &gs->storeItems[i];
        if (si->category != gs->storeCategory) continue;

        int col = shown % cols;
        int row = shown / cols;
        int cx2 = 20 + col * cardW + cardW / 2;
        int y   = startY + row * (cardH + 8);

        BOOL owned    = si->owned;
        BOOL selected = (gs->storeSelectedIndex == i);

        COLORREF bgC = selected ? RGB(50,70,160) :
                       owned    ? RGB(20,50,20)  : RGB(20,20,50);
        HBRUSH br = CreateSolidBrush(bgC);
        HPEN  pen = CreatePen(PS_SOLID, 1,
            selected ? RGB(100,140,255) : RGB(40,50,80));
        HBRUSH ob = (HBRUSH)SelectObject(hdc, br);
        HPEN   op = (HPEN)SelectObject(hdc, pen);
        RoundRect(hdc, cx2 - cardW/2 + 4, y,
                  cx2 + cardW/2 - 4, y + cardH, 8, 8);
        SelectObject(hdc, ob); SelectObject(hdc, op);
        DeleteObject(br); DeleteObject(pen);

        /* Name */
        old = (HFONT)SelectObject(hdc, g_fontTiny);
        SetBkMode(hdc, TRANSPARENT);
        SetTextColor(hdc, RGB(220,230,255));
        RECT rn = { cx2 - cardW/2 + 6, y + 4, cx2 + cardW/2 - 6, y + 26 };
        DrawTextW(hdc, si->name, -1, &rn, DT_CENTER | DT_SINGLELINE);
        SelectObject(hdc, old);

        /* Price or OWNED/ACTIVE badge */
        old = (HFONT)SelectObject(hdc, g_fontTiny);
        if (si->active) {
            SetTextColor(hdc, RGB(100,255,100));
            RECT rb2 = { cx2 - cardW/2 + 6, y + cardH - 22, cx2 + cardW/2 - 6, y + cardH };
            DrawTextW(hdc, L"\u2713 Equipped", -1, &rb2, DT_CENTER | DT_SINGLELINE);
        } else if (owned) {
            SetTextColor(hdc, RGB(180,220,180));
            RECT rb2 = { cx2 - cardW/2 + 6, y + cardH - 22, cx2 + cardW/2 - 6, y + cardH };
            DrawTextW(hdc, L"Owned", -1, &rb2, DT_CENTER | DT_SINGLELINE);
        } else {
            wchar_t pb[24];
            _snwprintf_s(pb, 24, _TRUNCATE, L"[coin] %d", si->price);
            SetTextColor(hdc, RGB(255,210,60));
            RECT rb2 = { cx2 - cardW/2 + 6, y + cardH - 22, cx2 + cardW/2 - 6, y + cardH };
            DrawTextW(hdc, pb, -1, &rb2, DT_CENTER | DT_SINGLELINE);
        }
        SelectObject(hdc, old);

        shown++;
    }

    /* Scroll hint */
    old = (HFONT)SelectObject(hdc, g_fontTiny);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(80,100,140));
    RECT rh = { 0, gs->canvasH - 28, gs->canvasW, gs->canvasH };
    DrawTextW(hdc, L"Arrows — Navigate   Enter/Click — Buy/Equip   ESC — Back",
              -1, &rh, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Stats ───────────────────────────────────────────────────── */
void DrawStats(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"STATS", NULL);

    struct { const wchar_t* label; int val; BOOL isTime; } rows[] = {
        { L"Games Played",      gs->stats.gamesPlayed,     FALSE },
        { L"Bricks Destroyed",  gs->stats.bricksDestroyed, FALSE },
        { L"Best Combo",        gs->stats.bestCombo,       FALSE },
        { L"Coins Earned",      gs->stats.coinsEarned,     FALSE },
        { L"Levels Completed",  gs->stats.levelsCompleted, FALSE },
        { L"Playtime",          gs->stats.playtimeSec,     TRUE  },
        { L"Daily Best",        gs->stats.dailyBest,       FALSE },
        { L"Endless Best",      gs->stats.endlessBest,     FALSE },
    };

    int n = 8, y = 130;
    for (int i = 0; i < n; i++) {
        HFONT old = (HFONT)SelectObject(hdc, g_fontSmall);
        SetBkMode(hdc, TRANSPARENT);
        SetTextColor(hdc, RGB(140,160,200));
        RECT rl = { 60, y, gs->canvasW/2, y + 34 };
        DrawTextW(hdc, rows[i].label, -1, &rl, DT_RIGHT | DT_VCENTER | DT_SINGLELINE);

        wchar_t vbuf[32];
        if (rows[i].isTime) {
            int s = rows[i].val;
            int h = s/3600; s -= h*3600;
            int m = s/60;   s -= m*60;
            _snwprintf_s(vbuf, 32, _TRUNCATE, L"%dh %dm %ds", h, m, s);
        } else {
            _snwprintf_s(vbuf, 32, _TRUNCATE, L"%d", rows[i].val);
        }
        SetTextColor(hdc, RGB(255,220,80));
        RECT rv = { gs->canvasW/2 + 10, y, gs->canvasW - 60, y + 34 };
        DrawTextW(hdc, vbuf, -1, &rv, DT_LEFT | DT_VCENTER | DT_SINGLELINE);
        SelectObject(hdc, old);

        y += 38;
    }

    HFONT old = (HFONT)SelectObject(hdc, g_fontTiny);
    SetTextColor(hdc, RGB(80,100,140));
    RECT rb = { 0, gs->canvasH - 28, gs->canvasW, gs->canvasH };
    DrawTextW(hdc, L"ESC — Back", -1, &rb, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Credits ─────────────────────────────────────────────────── */
void DrawCredits(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"CREDITS", NULL);

    const wchar_t* lines[] = {
        L"Brick Blast",
        L"A CS-120 Final Project",
        L"",
        L"Game Design & Programming",
        L"Team Fast Talk",
        L"",
        L"Engine: Win32 / GDI / MASM x64",
        L"Audio: waveOut + MCI",
        L"",
        L"Thank you for playing!",
    };
    int n = 10, y = 118, lh = 38;
    for (int i = 0; i < n; i++) {
        if (lines[i][0] == L'\0') { y += 16; continue; }
        BOOL header = (i == 0 || i == 3 || i == 6 || i == 9);
        HFONT fnt = header ? g_fontMed : g_fontSmall;
        COLORREF clr = header ? RGB(255,220,80) : RGB(180,200,240);
        HFONT old = (HFONT)SelectObject(hdc, fnt);
        SetBkMode(hdc, TRANSPARENT);
        SetTextColor(hdc, clr);
        RECT rc = { 0, y, gs->canvasW, y + lh };
        DrawTextW(hdc, lines[i], -1, &rc, DT_CENTER | DT_SINGLELINE);
        SelectObject(hdc, old);
        y += lh;
    }

    HFONT old = (HFONT)SelectObject(hdc, g_fontTiny);
    SetTextColor(hdc, RGB(80,100,140));
    RECT rb = { 0, gs->canvasH - 28, gs->canvasW, gs->canvasH };
    DrawTextW(hdc, L"ESC — Back", -1, &rb, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Daily Challenge menu ────────────────────────────────────── */
void DrawDailyMenu(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"\u2605 DAILY CHALLENGE", NULL);

    int cx = gs->canvasW / 2;

    /* Date */
    SYSTEMTIME st; GetLocalTime(&st);
    wchar_t dbuf[64];
    _snwprintf_s(dbuf, 64, _TRUNCATE,
                 L"%04d-%02d-%02d", st.wYear, st.wMonth, st.wDay);

    HFONT old = (HFONT)SelectObject(hdc, g_fontMed);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(200, 220, 255));
    RECT rd = { 0, 118, gs->canvasW, 150 };
    DrawTextW(hdc, dbuf, -1, &rd, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    /* Best */
    wchar_t bb[48];
    if (gs->stats.dailyBest > 0)
        _snwprintf_s(bb, 48, _TRUNCATE, L"Today's Best: %d", gs->stats.dailyBest);
    else
        wcscpy_s(bb, 48, L"Not played today yet");
    old = (HFONT)SelectObject(hdc, g_fontSmall);
    SetTextColor(hdc, RGB(255,210,60));
    RECT rb2 = { 0, 155, gs->canvasW, 185 };
    DrawTextW(hdc, bb, -1, &rb2, DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    HRule(hdc, cx - 120, cx + 120, 195, RGB(40,50,90));

    DrawMenuItem(hdc, cx, 208, 260, 48, L"\u25B6  Play Today's Challenge",
                 gs->menuSelection == 0, g_fontMed);
    DrawMenuItem(hdc, cx, 264, 260, 48, L"\u2302  Main Menu",
                 gs->menuSelection == 1, g_fontMed);
}

/* ── Name Entry ──────────────────────────────────────────────── */
void DrawNameEntry(HDC hdc, GameState* gs) {
    ClearBg(hdc, gs);
    DrawTitle(hdc, gs, L"ENTER YOUR NAME", NULL);

    int cx = gs->canvasW / 2;
    int bw = 300, bh = 48;
    int bx = cx - bw/2, by = gs->canvasH/2 - bh/2;

    /* Input box */
    HBRUSH br = CreateSolidBrush(RGB(20,20,50));
    HPEN  pen = CreatePen(PS_SOLID, 2, RGB(80,120,255));
    HBRUSH ob = (HBRUSH)SelectObject(hdc, br);
    HPEN   op = (HPEN)SelectObject(hdc, pen);
    RoundRect(hdc, bx, by, bx+bw, by+bh, 8, 8);
    SelectObject(hdc, ob); SelectObject(hdc, op);
    DeleteObject(br); DeleteObject(pen);

    /* Name text */
    wchar_t display[32];
    _snwprintf_s(display, 32, _TRUNCATE, L"%s_", gs->nameInputW);
    HFONT old = (HFONT)SelectObject(hdc, g_fontMed);
    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(255,255,255));
    RECT rt = { bx+10, by, bx+bw-10, by+bh };
    DrawTextW(hdc, display, -1, &rt, DT_LEFT | DT_VCENTER | DT_SINGLELINE);
    SelectObject(hdc, old);

    old = (HFONT)SelectObject(hdc, g_fontTiny);
    SetTextColor(hdc, RGB(100,120,160));
    RECT rh = { 0, gs->canvasH - 28, gs->canvasW, gs->canvasH };
    DrawTextW(hdc, L"Type your name   Enter — Confirm", -1, &rh,
              DT_CENTER | DT_SINGLELINE);
    SelectObject(hdc, old);
}

/* ── Dispatch ────────────────────────────────────────────────── */
void DrawScreens(HDC hdc, GameState* gs) {
    switch (gs->screen) {
        case SCREEN_MENU:        DrawMainMenu(hdc, gs);   break;
        case SCREEN_GAMEOVER:    DrawGameOver(hdc, gs);   break;
        case SCREEN_HIGHSCORE:   DrawHighScore(hdc, gs);  break;
        case SCREEN_OPTIONS:     DrawOptions(hdc, gs);    break;
        case SCREEN_STORE:       DrawStore(hdc, gs);      break;
        case SCREEN_STATS:       DrawStats(hdc, gs);      break;
        case SCREEN_CREDITS:     DrawCredits(hdc, gs);    break;
        case SCREEN_DAILY_MENU:  DrawDailyMenu(hdc, gs);  break;
        case SCREEN_NAMEENTRY:   DrawNameEntry(hdc, gs);  break;
        case SCREEN_LEVELWIN:
            DrawOverlay(hdc, gs, L"LEVEL COMPLETE!", L"Get ready for the next wave...");
            break;
        default: break;
    }
}

/* Entry point called by game.c */
void DrawScreens_Frame(HDC hdc, const GameState* gs) {
    DrawScreens(hdc, (GameState*)gs);
}
