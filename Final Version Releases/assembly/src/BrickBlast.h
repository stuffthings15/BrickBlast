/*
 * BrickBlast.h — Shared types, constants, and extern declarations
 * Win32 + MASM x64 build — no CRT, no .NET
 *
 * Calling convention: Microsoft x64 (rcx, rdx, r8, r9, stack, 32-byte shadow)
 */

#pragma once

/* ── Win32 minimal includes ─────────────────────────────────────────────── */
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <mmsystem.h>   /* winmm: waveOut, mciSendString */
#include <xinput.h>     /* XInput gamepad */

/* ── Compiler / linker pragmas ──────────────────────────────────────────── */
#pragma comment(lib, "kernel32.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "gdi32.lib")
#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "xinput1_4.lib")

/* ── Build version ──────────────────────────────────────────────────────── */
#define BB_VERSION_MAJOR  1
#define BB_VERSION_MINOR  0
#define BB_VERSION_STR    "1.0"
#define BB_TITLE          L"BrickBlast: Velocity Market"
#define BB_CLASS          L"BrickBlastWnd"
#define BB_MUTEX          L"BrickBlast_SingleInstance"

/* ── Window / canvas ────────────────────────────────────────────────────── */
#define CANVAS_W          900
#define CANVAS_H          700
#define WINDOW_MIN_W      600
#define WINDOW_MIN_H      467

/* ── Game constants (mirror Form1.vb) ──────────────────────────────────── */
#define PADDLE_WIDTH      240
#define PADDLE_HEIGHT     14
#define PADDLE_Y_OFFSET   50
#define PADDLE_SPEED      26

#define BALL_RADIUS       8
#define MIN_BALL_RADIUS   4
#define MAX_BALL_RADIUS   20
#define INITIAL_BALL_SPEED  8.25f
#define MAX_BALLS         8

#define BRICK_ROWS        7
#define BRICK_COLS        12
#define BRICK_WIDTH       65
#define BRICK_HEIGHT      22
#define BRICK_PADDING     4
#define BRICK_TOP_OFFSET  70
#define BRICK_LEFT_OFFSET 27

#define MAX_LIVES         10
#define POWERUP_SIZE      45
#define POWERUP_SPEED     3.0f
#define MAX_POWERUPS      16
#define MAX_PARTICLES     256
#define MAX_STARS         80
#define MAX_STORE_ITEMS   128

#define COIN_PER_BRICK    3
#define COIN_LEVEL_BONUS  10

#define COMBO_TIMEOUT_MS  2500

#define TIMER_ID_GAME     1
#define TIMER_INTERVAL_MS 16    /* ~60 fps */

/* ── Enumerations ───────────────────────────────────────────────────────── */
typedef enum {
    SCR_NAMEENTRY = 0,
    SCREEN_NAMEENTRY = 0,
    SCR_MENU,
    SCREEN_MENU = SCR_MENU,
    SCR_PLAY,
    SCREEN_PLAYING = SCR_PLAY,
    SCR_PAUSE,     SCREEN_PAUSE     = SCR_PAUSE,
    SCR_GAMEOVER,  SCREEN_GAMEOVER  = SCR_GAMEOVER,
    SCR_HIGHSCORE, SCREEN_HIGHSCORE = SCR_HIGHSCORE,
    SCR_OPTIONS,   SCREEN_OPTIONS   = SCR_OPTIONS,
    SCR_STORE,     SCREEN_STORE     = SCR_STORE,
    SCR_STATS,     SCREEN_STATS     = SCR_STATS,
    SCR_CREDITS,   SCREEN_CREDITS   = SCR_CREDITS,
    SCR_DAILY,     SCREEN_DAILY_MENU = SCR_DAILY, SCREEN_DAILY = SCR_DAILY,
    SCR_ENDLESS,   SCREEN_ENDLESS   = SCR_ENDLESS,
    SCR_ROULETTE,  SCREEN_ROULETTE  = SCR_ROULETTE,
    SCR_GETREADY,  SCREEN_GETREADY  = SCR_GETREADY,
    SCR_LEVELWIN,  SCREEN_LEVELWIN  = SCR_LEVELWIN,
    SCR_COUNT
} GameScreen;

typedef enum {
    PU_NONE = 0,
    PU_GROW,      PU_GROW_BALL    = PU_GROW,
    PU_SHRINK,    PU_SHRINK_BALL  = PU_SHRINK,
    PU_LIFE,      PU_EXTRA_LIFE   = PU_LIFE,
    PU_MULTI,     PU_MULTIBALL    = PU_MULTI,
    PU_WIDE,      PU_WIDE_PADDLE  = PU_WIDE,
    PU_SLOW,      PU_SLOW_BALL    = PU_SLOW,
    PU_FAST,      PU_FAST_BALL    = PU_FAST,
    PU_FIREBALL,
    PU_COUNT
} PowerUpType;

typedef enum {
    CAT_BALLS = 0,
    SC_BALLS   = 0,
    CAT_BRICKS,
    SC_BRICKS  = CAT_BRICKS,
    CAT_BONUS,
    SC_BONUS   = CAT_BONUS,
    CAT_PADDLES,
    SC_PADDLE  = CAT_PADDLES,
    CAT_MUSIC,
    SC_MUSIC   = CAT_MUSIC,
    CAT_SFX,
    SC_SFX     = CAT_SFX,
    CAT_COUNT
} StoreCategory;

/* ── Structs ────────────────────────────────────────────────────────────── */
typedef struct {
    float x, y;
    float dx, dy;
    int   radius;
    BOOL  active;
} Ball;

typedef struct {
    float x, y;
    int   w;
    int   h;    /* height (used by physics/draw) */
} Paddle;

typedef struct {
    int   hp;       /* 0 = destroyed */
    int   maxHp;
    COLORREF color;     /* face color (palette-assigned) */
} Brick;

typedef struct {
    float     x, y;
    float     dy;       /* fall speed */
    PowerUpType type;
    BOOL      active;
} PowerUp;

typedef struct {
    float  x, y;
    float  dx, dy;
    int    life;    /* countdown ticks; 0 = dead */
    int    maxLife;
    int    size;    /* pixel radius */
    COLORREF color;
} Particle;

typedef struct {
    float x, y;
    float dy;           /* drift speed */
    int   brightness;   /* 0-255 */
    int   size;         /* pixel radius */
} Star;

/* ── Store item ─────────────────────────────────────────────────────────── */
#define ITEM_ID_LEN  32
#define ITEM_LABEL_LEN 48

typedef struct {
    char      id[ITEM_ID_LEN];
    wchar_t   name[ITEM_LABEL_LEN];  /* display name */
    char      label[ITEM_LABEL_LEN]; /* internal label */
    StoreCategory category;
    int       price;        /* 0 = free */
    int       index;        /* skin/palette/pack index */
    BOOL      owned;        /* purchased or default */
    BOOL      active;       /* currently equipped */
} StoreItem;

/* ── Save data (binary, written to %APPDATA%\BrickBlast\save.bin) ────────── */
#define PLAYER_NAME_LEN   32
#define OWNED_BITS        16   /* 128 items max in 16 bytes bitmask */

typedef struct {
    WORD  magic;            /* SAVE_MAGIC */
    BYTE  version;
    char  playerName[PLAYER_NAME_LEN];
    int   coinBalance;
    BYTE  ownedBits[OWNED_BITS];
    BYTE  activeBallSkin;
    BYTE  activeBrickPalette;
    BYTE  activeBonusPack;
    BYTE  activePaddleSkin;
    BYTE  activeMusicStyle;
    BYTE  activeSfxStyle;
    int   totalBricksDestroyed;
    int   totalPlaytimeSeconds;
    int   bestCombo;
    int   totalCoinsEarned;
    int   levelsCompleted;
    int   dailyBestScore;
    char  dailyLastDate[12];
    int   endlessBestScore;
    int   highScores[10];
    int   musicVolume;      /* 0–100 */
    int   sfxVolume;        /* 0–100 */
    BOOL  colorblindMode;
    int   windowScale;      /* 1–4 */
} SaveData;

/* ── Master game state ─────────────────────────────────────────────────── */
typedef struct {
    /* Screen */
    GameScreen screen;
    GameScreen prevScreen;

    /* Gameplay */
    Ball      balls[MAX_BALLS];
    int       ballCount;
    Paddle    paddle;
    Brick     bricks[BRICK_ROWS][BRICK_COLS];
    PowerUp   powerups[MAX_POWERUPS];
    int       powerupCount;
    Particle  particles[MAX_PARTICLES];
    Star      stars[MAX_STARS];

    /* HUD */
    int       score;
    int       lives;
    int       level;
    int       combo;
    int       comboTimer;   /* countdown ms */
    int       coinsEarned;

    /* Flags */
    BOOL      paused;
    BOOL      speedBoost;
    BOOL      colorblind;
    BOOL      getReadyActive;
    int       getReadyTimer;

    /* Daily / Endless */
    BOOL      isDaily;
    BOOL      isEndless;
    BOOL      endlessMode;  /* alias for isEndless used by game.c */
    int       dailySeed;
    int       dailyScore;

    /* Settings */
    int       musicVolume;
    int       sfxVolume;
    int       windowScale;

    /* Store / economy */
    int       coinBalance;
    int       coinsThisSession;
    StoreCategory storeCategory;
    int       storeSelectedIndex;
    int       storeScrollOffset;
    BYTE      activeBallSkin;
    BYTE      activeBrickPalette;
    BYTE      activeBonusPack;
    BYTE      activePaddleSkin;
    BYTE      activeMusicStyle;
    BYTE      activeSfxStyle;

    /* Save */
    SaveData  save;
    char      savePath[MAX_PATH];

    /* Name entry */
    char      playerName[PLAYER_NAME_LEN];
    char      nameInput[PLAYER_NAME_LEN];
    int       nameInputLen;

    /* High scores */
    int       highScores[10];
    int       totalBricksDestroyed;
    int       totalPlaytimeSeconds;
    int       bestCombo;
    int       totalCoinsEarned;
    int       levelsCompleted;
    int       dailyBestScore;
    char      dailyLastDate[12];
    int       endlessBestScore;

    /* Options menu cursor */
    int       optionsCursor;

    /* Roulette */
    PowerUpType rouletteSelected;
    int       rouletteTimer;

    /* Timing */
    DWORD     lastTickMs;
    int       playtimeAccumMs;

    /* Render */
    HDC       backDC;
    HBITMAP   backBmp;
    int       canvasW;
    int       canvasH;

    /* Menu / UI navigation */
    int       menuSelection;   /* currently highlighted menu item */
    int       optionsSelection;
    int       hiScore;

    /* Level win / credits animation */
    int       levelWinTimer;
    int       creditsScroll;

    /* Playtime frame counter */
    int       playFrames;      /* frames counted during active play */

    /* Name entry state */
    BOOL      nameConfirmed;
    int       nameLen;

    /* Ball state helpers */
    BOOL      ballOnPaddle;    /* ball not yet launched */
    int       widePaddleTimer; /* ticks remaining for wide paddle */
    int       fireballTimer;   /* ticks remaining for fireball */

    /* Skin shortcuts (mirrors activeBall/Brick/Paddle skin bytes) */
    int       skinBall;
    int       skinBrick;
    int       skinPaddle;

    /* Volume aliases */
    int       volMusic;   /* same storage as musicVolume */
    int       volSFX;

    /* Mode flag */
    BOOL      dailyMode;  /* true when playing a daily challenge */

    /* RNG for starfield / particle scatter */
    DWORD     rngState;

    /* Store items embedded in state for draw code access */
    StoreItem storeItems[128];
    int       storeItemCount;

    /* Display names for current music/sfx style */
    const wchar_t* musicStyleName;
    const wchar_t* sfxStyleName;

    /* Active style indices */
    int       musicStyle;
    int       sfxStyle;

    /* Name entry (wide char for display) */
    wchar_t   nameInputW[PLAYER_NAME_LEN];

    /* Aggregate stats sub-struct */
    struct {
        int gamesPlayed;
        int bricksDestroyed;
        int bestCombo;
        int coinsEarned;
        int levelsCompleted;
        int playtimeSec;
        int dailyBest;
        int endlessBest;
    } stats;

    /* Flat aliases for save.c compatibility */
    int       statsCoinsEarned;       /* mirrors stats.coinsEarned */
    int       statsBricksDestroyed;   /* mirrors stats.bricksDestroyed */
    int       statsPlaytimeSec;       /* mirrors stats.playtimeSec */
    int       statsBestCombo;         /* mirrors stats.bestCombo */
    int       statsLevelsCompleted;   /* mirrors stats.levelsCompleted */
    int       highScore;              /* best single-game score */
    int       skinBonus;              /* active bonus pack index */

} GameState;

/* ── Globals declared in game.c, used everywhere ─────────────────────────── */
extern GameState g;
extern HWND      g_hwnd;
extern HINSTANCE g_hinstance;
extern StoreItem g_storeItems[];
extern int       g_storeItemCount;

/* ── ASM-exported functions (defined in .asm files) ─────────────────────── */
#ifdef __cplusplus
extern "C" {
#endif

/* render.asm */
void Render_InitBackBuffer(HDC hdc, int w, int h);
void Render_BeginFrame(HDC hdc);
void Render_EndFrame(HDC hdc, HWND hwnd);
void Render_FillRect(int x, int y, int w, int h, COLORREF color);
void Render_DrawRect(int x, int y, int w, int h, COLORREF color, int penW);
void Render_FillEllipse(int x, int y, int w, int h, COLORREF color);
void Render_DrawLine(int x1, int y1, int x2, int y2, COLORREF color, int penW);
void Render_DrawTextW(const wchar_t* text, int x, int y, int w, int h, COLORREF color, int fontSize, BOOL bold, UINT fmt);
void Render_CreateFonts(void);
void Render_DestroyFonts(void);

/* input.asm */
void  Input_Init(void);
void  Input_SetKey(int vk, BOOL down);
BOOL  Input_IsKeyDown(int vk);
BOOL  Input_IsKeyPressed(int vk);   /* true once per press */
void  Input_EndFrame(void);         /* clear pressed flags */
void  Input_SetMouse(int x, int y);
void  Input_PollGamepad(void);

/* sound.asm */
void  Sound_Init(void);
void  Sound_PlayPcm(const BYTE* data, DWORD bytes, int sampleRate);
void  Sound_StopMusic(void);
void  Sound_PlayMusic(const wchar_t* filePath);

#ifdef __cplusplus
}
#endif

/* ── C-function prototypes (defined in .c files) ─────────────────────────── */

/* game.c */
void Game_Init(void);
void Game_Tick(void);
void Game_OnKey(int vk, int down);
void Game_OnChar(int ch);
void Game_OnMouse(int x, int y, int ldown, int lup, int wheel);
void Game_OnSize(int w, int h);
void Game_OnPaint(HDC hdc);
void Game_Shutdown(void);

/* physics.c */
void Physics_StartNewGame(void);
void Physics_NextLevel(void);
void Physics_SetupLevel(void);
void Physics_Tick(int dtMs);

/* music.c */
BYTE* Music_GenerateWav(int freqHz, int durationMs, int volume, DWORD* outBytes);
void  Music_GenerateAll(GameState* gs);
void  Music_GenerateAndPlay(int styleIndex, int musicVolume);
void  Music_Stop(void);

/* save.c */
void Save_GetPath(char* outPath, int maxLen);
void Save_Load(GameState* gs);
void Save_Write(const GameState* gs);

/* store.c */
void  Store_Init(GameState* gs);
BOOL  Store_OwnsItem(int index);
BOOL  Store_BuyItem(int index);
void  Store_EquipItem(int index);
COLORREF* Store_GetBallColors(BYTE skinIndex, COLORREF* out, int maxColors);
COLORREF* Store_GetBrickPalette(BYTE paletteIndex, COLORREF* out2);
COLORREF  Store_GetPaddleColor(BYTE paddleIndex);

/* levels.c */
void Levels_SetupLevel(int level, BOOL isDaily, int seed);
void Levels_Setup(GameState* gs, int level);
void Levels_InitStars(GameState* gs);
int  Levels_DailySeed(void);

/* draw_game.c */
void Draw_Game(void);
void DrawGame(HDC hdc, GameState* gs,
              HFONT fontLarge, HFONT fontMed, HFONT fontSmall);

/* draw_screens.c */
void Draw_Menu(void);
void Draw_NameEntry(void);
void Draw_Pause(void);
void Draw_GameOver(void);
void Draw_HighScore(void);
void Draw_Options(void);
void Draw_Store(void);
void Draw_Stats(void);
void Draw_Credits(void);
void Draw_Daily(void);
void Draw_Roulette(void);
void Draw_GetReady(void);
void Draw_Overlay(const wchar_t* title, const wchar_t* subtitle);
void DrawScreens(HDC hdc, GameState* gs);
void DrawMainMenu(HDC hdc, GameState* gs);
void DrawGameOver(HDC hdc, GameState* gs);
void DrawHighScore(HDC hdc, GameState* gs);
void DrawOptions(HDC hdc, GameState* gs);
void DrawStore(HDC hdc, GameState* gs);
void DrawStats(HDC hdc, GameState* gs);
void DrawCredits(HDC hdc, GameState* gs);
void DrawDailyMenu(HDC hdc, GameState* gs);
void DrawNameEntry(HDC hdc, GameState* gs);
void DrawOverlay(HDC hdc, GameState* gs,
                 const wchar_t* title, const wchar_t* sub);

/* Font handles (defined in render.asm, used by draw modules) */
extern HFONT g_fontTitle;
extern HFONT g_fontLarge;
extern HFONT g_fontMed;
extern HFONT g_fontSmall;
extern HFONT g_fontTiny;
