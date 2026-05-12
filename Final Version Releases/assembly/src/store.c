/*
 * store.c — In-game store catalog, economy, and skin look-up tables
 * Ported from Form1.vb store-related fields and DrawStore logic.
 * No CRT. Win32 heap only.
 */

#include "BrickBlast.h"
#include "bbutil.h"

/* ── Store catalog definition ────────────────────────────────── */
/*
 * Item kinds map 1:1 to StoreCategory enum values:
 *   SC_BALLS=0, SC_BRICKS=1, SC_BONUS=2, SC_PADDLE=3, SC_MUSIC=4, SC_SFX=5
 *
 * Price = 0 means free (base item, always owned).
 */
typedef struct {
    const WCHAR* name;
    const WCHAR* description;
    StoreCategory category;
    int price;          /* 0 = free */
    int index;          /* skin/palette/pack index for rendering */
} CatalogEntry;

static const CatalogEntry _catalog[] = {
    /* ── Ball skins (13) ─────────────────────────────────────── */
    { L"Classic",   L"Default ball",          SC_BALLS,  0,   0 },
    { L"Fire",      L"Blazing red ball",       SC_BALLS,  150, 1 },
    { L"Ice",       L"Frozen blue ball",       SC_BALLS,  150, 2 },
    { L"Plasma",    L"Electric purple",        SC_BALLS,  200, 3 },
    { L"Gold",      L"Shiny gold sphere",      SC_BALLS,  300, 4 },
    { L"Rainbow",   L"Cycles all hues",        SC_BALLS,  400, 5 },
    { L"Lava",      L"Molten core",            SC_BALLS,  250, 6 },
    { L"Void",      L"Dark matter orb",        SC_BALLS,  350, 7 },
    { L"Toxic",     L"Radioactive green",      SC_BALLS,  200, 8 },
    { L"Neon",      L"Bright cyan glow",       SC_BALLS,  200, 9 },
    { L"Crystal",   L"Glass-clear sphere",     SC_BALLS,  300, 10 },
    { L"Shadow",    L"Near-invisible dark",    SC_BALLS,  350, 11 },
    { L"Sakura",    L"Cherry-blossom pink",    SC_BALLS,  250, 12 },

    /* ── Brick palettes (10) ──────────────────────────────────── */
    { L"Classic",   L"Standard palette",       SC_BRICKS, 0,   0 },
    { L"Toxic",     L"Neon greens",            SC_BRICKS, 100, 1 },
    { L"Sunset",    L"Orange-red gradient",    SC_BRICKS, 120, 2 },
    { L"Forest",    L"Deep greens",            SC_BRICKS, 100, 3 },
    { L"Ocean",     L"Cool blues",             SC_BRICKS, 120, 4 },
    { L"Galaxy",    L"Dark purples",           SC_BRICKS, 150, 5 },
    { L"Gold",      L"Bright yellows",         SC_BRICKS, 200, 6 },
    { L"Obsidian",  L"Dark grays",             SC_BRICKS, 200, 7 },
    { L"Sakura",    L"Soft pinks",             SC_BRICKS, 150, 8 },
    { L"Aurora",    L"Northern lights",        SC_BRICKS, 250, 9 },

    /* ── Bonus packs (16) ─────────────────────────────────────── */
    { L"Classic",   L"Standard icons",         SC_BONUS,  0,   0 },
    { L"Ninja",     L"Shuriken icons",         SC_BONUS,  100, 1 },
    { L"Space",     L"Sci-fi icons",           SC_BONUS,  100, 2 },
    { L"Candy",     L"Sweet icons",            SC_BONUS,  80,  3 },
    { L"Cyberpunk", L"Neon tech icons",        SC_BONUS,  150, 4 },
    { L"Medieval",  L"Knight icons",           SC_BONUS,  120, 5 },
    { L"Retro",     L"8-bit icons",            SC_BONUS,  90,  6 },
    { L"Robot",     L"Gear icons",             SC_BONUS,  110, 7 },
    { L"Pirate",    L"Skull & bones",          SC_BONUS,  120, 8 },
    { L"Galaxy",    L"Star icons",             SC_BONUS,  130, 9 },
    { L"Festival",  L"Party icons",            SC_BONUS,  100, 10 },
    { L"Halloween", L"Spooky icons",           SC_BONUS,  140, 11 },
    { L"Golden Age",L"Trophy icons",           SC_BONUS,  200, 12 },
    { L"Ocean",     L"Sea icons",              SC_BONUS,  100, 13 },
    { L"Magic",     L"Spell icons",            SC_BONUS,  130, 14 },
    { L"Dragon",    L"Dragon icons",           SC_BONUS,  160, 15 },

    /* ── Paddle skins (8) ─────────────────────────────────────── */
    { L"Classic",   L"Standard paddle",        SC_PADDLE, 0,   0 },
    { L"Fire",      L"Blazing paddle",         SC_PADDLE, 150, 1 },
    { L"Ice",       L"Frozen paddle",          SC_PADDLE, 150, 2 },
    { L"Gold",      L"Golden paddle",          SC_PADDLE, 250, 3 },
    { L"Neon",      L"Glowing paddle",         SC_PADDLE, 200, 4 },
    { L"Void",      L"Dark paddle",            SC_PADDLE, 300, 5 },
    { L"Sakura",    L"Cherry paddle",          SC_PADDLE, 200, 6 },
    { L"Rainbow",   L"Colorful paddle",        SC_PADDLE, 350, 7 },

    /* ── Music tracks (6) ─────────────────────────────────────── */
    { L"Brick Blast",        L"Original theme",  SC_MUSIC, 0,   0 },
    { L"Calculated Impact",  L"Energetic",       SC_MUSIC, 80,  1 },
    { L"Machine Precision",  L"Techno",          SC_MUSIC, 80,  2 },
    { L"Machine",            L"Hard beats",      SC_MUSIC, 80,  3 },
    { L"Pinball Dream",      L"Retro vibes",     SC_MUSIC, 80,  4 },
    { L"Pinball",            L"Classic arcade",  SC_MUSIC, 60,  5 },

    /* ── SFX packs (5) ───────────────────────────────────────── */
    { L"Classic",    L"Default sounds",  SC_SFX, 0,   0 },
    { L"Zelda",      L"Adventure SFX",   SC_SFX, 60,  1 },
    { L"Mega Man",   L"Retro robot SFX", SC_SFX, 60,  2 },
    { L"Tetris",     L"Puzzle SFX",      SC_SFX, 50,  3 },
    { L"Retro Arcade",L"Beep-boop SFX", SC_SFX, 50,  4 },
};

static const int _catalogCount = sizeof(_catalog) / sizeof(_catalog[0]);

/* ── Store_Init ────────────────────────────────────────────────── */
void Store_Init(GameState* gs) {
    int n = _catalogCount;
    if (n > MAX_STORE_ITEMS) n = MAX_STORE_ITEMS;
    gs->storeItemCount = n;

    for (int i = 0; i < n; i++) {
        StoreItem* it = &gs->storeItems[i];
        /* Copy name (up to 31 chars) */
        int j = 0;
        while (_catalog[i].name[j] && j < 31) {
            it->name[j] = _catalog[i].name[j];
            j++;
        }
        it->name[j] = 0;
        it->category = (int)_catalog[i].category;
        it->price    = _catalog[i].price;
        it->index    = _catalog[i].index;
        it->owned    = (_catalog[i].price == 0) ? 1 : 0;
        it->active   = 0;
    }

    /* Mark base items as active by default */
    gs->skinBall   = 0;
    gs->skinBrick  = 0;
    gs->skinBonus  = 0;
    gs->skinPaddle = 0;
    gs->musicStyle = 0;
    gs->sfxStyle   = 0;
    gs->storeItems[0].active = 1;    /* Classic ball */
    gs->storeItems[13].active = 1;   /* Classic brick palette */
    gs->storeItems[23].active = 1;   /* Classic bonus */
    gs->storeItems[39].active = 1;   /* Classic paddle */
    gs->storeItems[47].active = 1;   /* Brick Blast music */
    gs->storeItems[53].active = 1;   /* Classic SFX */
}

/* ── Store_Save / Store_Load (delegates to save.c) ─────────────── */
void Store_Save(const GameState* gs) {
    extern void Save_Write(const GameState*);
    Save_Write(gs);
}

void Store_Load(GameState* gs) {
    /* Called after Save_Load; just ensures defaults for unset fields */
    if (gs->musicVolume <= 0)  gs->musicVolume = 500;
    if (gs->sfxVolume   <= 0)  gs->sfxVolume   = 80;
    if (gs->windowScale <= 0)  gs->windowScale  = 100;
}

/* ── Store_HandleClick ───────────────────────────────────────────
 * Called from game.c on LBUTTONDOWN while SCREEN_STORE is active.
 * Determines which card was clicked and buys/equips the item.
 * Uses the same grid layout as draw_screens.c:
 *   cards start at x=40, y=160; size 180×140; 4 per row; gap=20
 */
void Store_HandleClick(GameState* gs, int mx, int my) {
    const int CARD_W   = 180;
    const int CARD_H   = 140;
    const int CARD_GAP = 20;
    const int START_X  = 40;
    const int START_Y  = 160;
    const int COLS     = 4;

    /* Count items in current category */
    StoreCategory cat = (StoreCategory)gs->storeCategory;
    int visIdx = 0;

    for (int i = 0; i < gs->storeItemCount; i++) {
        if ((StoreCategory)gs->storeItems[i].category != cat) continue;

        int col = visIdx % COLS;
        int row = visIdx / COLS;
        int cx  = START_X + col * (CARD_W + CARD_GAP) - gs->storeScrollOffset;
        int cy  = START_Y + row * (CARD_H + CARD_GAP);

        if (mx >= cx && mx < cx + CARD_W &&
            my >= cy && my < cy + CARD_H) {
            /* Hit — buy or equip */
            StoreItem* it = &gs->storeItems[i];
            if (!it->owned) {
                if (gs->coinBalance >= it->price) {
                    gs->coinBalance -= it->price;
                    it->owned = 1;
                }
            }
            if (it->owned) {
                /* Deactivate other items in this category */
                for (int j = 0; j < gs->storeItemCount; j++) {
                    if ((StoreCategory)gs->storeItems[j].category == cat)
                        gs->storeItems[j].active = 0;
                }
                it->active = 1;
                /* Update active skin index in gs */
                switch (cat) {
                case SC_BALLS:   gs->skinBall   = it->index; break;
                case SC_BRICKS:  gs->skinBrick  = it->index; break;
                case SC_BONUS:   gs->skinBonus  = it->index; break;
                case SC_PADDLE:  gs->skinPaddle = it->index; break;
                case SC_MUSIC:
                    gs->musicStyle = it->index;
                    extern void Music_PlayTrack(int, int);
                    Music_PlayTrack(it->index, gs->musicVolume);
                    break;
                case SC_SFX:     gs->sfxStyle   = it->index; break;
                default: break;
                }
            }
            return;
        }
        visIdx++;
    }

    /* Check category tab clicks (tabs at y≈90, each ~120px wide from x=40) */
    static const WCHAR* tabNames[] = {
        L"Balls", L"Bricks", L"Bonus", L"Paddle", L"Music", L"SFX"
    };
    const int TAB_W = 110, TAB_H = 36, TAB_Y = 90;
    for (int t = 0; t < 6; t++) {
        int tx = 40 + t * (TAB_W + 10);
        if (mx >= tx && mx < tx + TAB_W && my >= TAB_Y && my < TAB_Y + TAB_H) {
            gs->storeCategory = t;
            gs->storeScrollOffset = 0;
            gs->storeSelectedIndex = 0;
            return;
        }
    }
    (void)tabNames;
}

/* ── Options_HandleClick ────────────────────────────────────────
 * Called from game.c on LBUTTONDOWN while SCREEN_OPTIONS is active.
 * Handles slider dragging and toggle button presses.
 */
void Options_HandleClick(GameState* gs, int mx, int my) {
    /* Layout: sliders at y = 220 + row*80, x span = 200..600 */
    for (int row = 0; row < 5; row++) {
        int sy = 220 + row * 80;
        if (my >= sy && my < sy + 30) {
            if (mx >= 200 && mx <= 600) {
                float t = (float)(mx - 200) / 400.0f;
                switch (row) {
                case 0:  /* Music volume 0..1000 */
                    gs->musicVolume = (int)(t * 1000.0f);
                    extern void Sound_SetMusicVol(int);
                    Sound_SetMusicVol(gs->musicVolume);
                    break;
                case 1:  /* SFX volume 0..100 */
                    gs->sfxVolume = (int)(t * 100.0f);
                    break;
                case 2:  /* Window scale 50..200 */
                    gs->windowScale = 50 + (int)(t * 150.0f);
                    break;
                case 3:  /* Colorblind toggle (treat as button) */
                    gs->colorblind ^= 1;
                    break;
                case 4:  /* Music style cycle */
                    gs->musicStyle = (gs->musicStyle + 1) % 6;
                    break;
                }
            }
            return;
        }
    }
}

