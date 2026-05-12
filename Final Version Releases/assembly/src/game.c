/*
 * game.c — BrickBlast Game State Machine
 * Ported from Form1.vb (VB.NET original).
 * Pure Win32 / no CRT. Uses game.h shared types.
 */

#include "BrickBlast.h"
#include "bbutil.h"

/* ── Externs from ASM modules ─────────────────────────────── */
extern void   Input_PollGamepad(void);
extern void   Input_EndFrame(void);
extern int    Input_KeyDown(int vk);
extern int    Input_KeyPressed(int vk);
extern int    Input_KeyReleased(int vk);
extern int    Input_MouseX(void);
extern int    Input_MouseY(void);
extern int    Input_GamepadButton(int mask);
extern int    Input_GamepadLX(void);
extern int    Input_GamepadConnected(void);
extern void   Sound_PlaySFX(const BYTE* pWav, int len);
extern void   Sound_PlayMusic(const WCHAR* path);
extern void   Sound_StopMusic(void);
extern void   Sound_SetMusicVol(int vol);
extern int    Sound_IsPlaying(void);

/* Externs from other C modules */
extern void   Physics_Update(GameState* gs);
extern void   Levels_Setup(GameState* gs, int level);
extern void   Levels_InitStars(GameState* gs);
extern void   Save_Load(GameState* gs);
extern void   Save_Write(const GameState* gs);
extern void   Music_GenerateAll(GameState* gs);
extern BYTE*  Music_GetTrack(int style, int* outLen);
extern BYTE*  Music_GenerateSFX(int freqHz, int durationMs, int vol, int* outLen);
extern void   DrawGame_Frame(HDC hdc, const GameState* gs);
extern void   DrawScreens_Frame(HDC hdc, const GameState* gs);
extern void   Store_Init(GameState* gs);
extern void   Store_Load(GameState* gs);
extern void   Store_Save(const GameState* gs);
extern int    Render_GetW(void);
extern int    Render_GetH(void);
extern HDC    Render_GetDC(void);

/* ── File-scope global game state ────────────────────────────── */
static GameState _gs;

/* ── Virtual-key codes (matching Form1.vb usage) ─────────────── */
#define VK_LEFT     0x25
#define VK_RIGHT    0x27
#define VK_UP       0x26
#define VK_DOWN     0x28
#define VK_SPACE    0x20
#define VK_RETURN   0x0D
#define VK_ESCAPE   0x1B
#define VK_F1       0x70
#define VK_F2       0x71
#define VK_F3       0x72
#define VK_F5       0x74
#define VK_A        0x41
#define VK_D        0x44
#define VK_M        0x4D
#define VK_P        0x50
#define VK_Q        0x51
#define VK_R        0x52
#define VK_S        0x53

/* XInput button masks (match input.asm constants) */
#define GP_DPAD_LEFT    0x0004
#define GP_DPAD_RIGHT   0x0008
#define GP_DPAD_UP      0x0001
#define GP_DPAD_DOWN    0x0002
#define GP_A            0x1000
#define GP_B            0x2000
#define GP_START        0x0010
#define GP_BACK         0x0020

/* ── Helpers ─────────────────────────────────────────────────── */
static void PlaySFXNote(int freq, int ms) {
    int len = 0;
    BYTE* wav = Music_GenerateSFX(freq, ms, _gs.sfxVolume, &len);
    if (wav && len > 0)
        Sound_PlaySFX(wav, len);
}

static void StartMusicForStyle(void) {
    int len = 0;
    BYTE* track = Music_GetTrack(_gs.musicStyle, &len);
    if (track && len > 0) {
        /* write temp file and play via MCI — handled by music.c */
        extern void Music_PlayTrack(int style, int vol);
        Music_PlayTrack(_gs.musicStyle, _gs.musicVolume);
    }
}

static void ResetBallToCenter(GameState* gs) {
    Ball* b = &gs->balls[0];
    b->x = (float)(gs->canvasW / 2);
    b->y = (float)(gs->paddle.y - BALL_RADIUS - 2);
    b->dx = 3.5f;
    b->dy = -INITIAL_BALL_SPEED;
    b->radius = BALL_RADIUS;
    b->active = 1;
    gs->ballCount = 1;
}

static void ResetPaddle(GameState* gs) {
    gs->paddle.x = (float)((gs->canvasW - PADDLE_WIDTH) / 2);
    gs->paddle.y = (float)(gs->canvasH - PADDLE_Y_OFFSET);
    gs->paddle.w = PADDLE_WIDTH;
    gs->paddle.h = PADDLE_HEIGHT;
}

static void AddScore(GameState* gs, int pts) {
    gs->score += pts;
    if (gs->score > gs->highScore)
        gs->highScore = gs->score;
}

static int AllBricksCleared(const GameState* gs) {
    for (int r = 0; r < BRICK_ROWS; r++)
        for (int c = 0; c < BRICK_COLS; c++)
            if (gs->bricks[r][c].hp > 0)
                return 0;
    return 1;
}

/* ── Public API ──────────────────────────────────────────────── */

void Game_Init(void) {
    /* Zero state */
    for (BYTE* p = (BYTE*)&_gs; p < (BYTE*)&_gs + sizeof(_gs); p++)
        *p = 0;

    _gs.screen   = SCREEN_MENU;
    _gs.canvasW  = CANVAS_W;
    _gs.canvasH  = CANVAS_H;
    _gs.lives    = MAX_LIVES;
    _gs.level    = 1;
    _gs.musicStyle = 0;
    _gs.musicVolume = 500;
    _gs.sfxVolume   = 80;
    _gs.speedBoost  = 0;
    _gs.colorblind  = 0;

    /* Init subsystems */
    Store_Init(&_gs);
    Save_Load(&_gs);
    Levels_InitStars(&_gs);
    /* Music_GenerateAll deferred to first tick to avoid startup crash */
    /* Music_GenerateAll(&_gs); */
    /* StartMusicForStyle(); */
}

void Game_Tick(void) {
    /* Poll gamepad each tick */
    Input_PollGamepad();

    switch (_gs.screen) {

    case SCREEN_MENU:
        /* Space / Enter / GP_A → start game */
        if (Input_KeyPressed(VK_SPACE) || Input_KeyPressed(VK_RETURN) ||
            Input_GamepadButton(GP_A)) {
            _gs.screen = SCREEN_GETREADY;
            _gs.level  = 1;
            _gs.score  = 0;
            _gs.lives  = MAX_LIVES;
            _gs.combo  = 0;
            ResetPaddle(&_gs);
            Levels_Setup(&_gs, _gs.level);
            ResetBallToCenter(&_gs);
            _gs.getReadyTimer = 120;
        }
        /* S → store */
        if (Input_KeyPressed(VK_S) || Input_GamepadButton(GP_B))
            _gs.screen = SCREEN_STORE;
        /* O → options */
        if (Input_KeyPressed(VK_F1))
            _gs.screen = SCREEN_OPTIONS;
        /* C → credits */
        if (Input_KeyPressed(VK_F2))
            _gs.screen = SCREEN_CREDITS;
        /* D → daily challenge */
        if (Input_KeyPressed(VK_D))
            _gs.screen = SCREEN_DAILY;
        /* E → endless */
        if (Input_KeyPressed(VK_F3)) {
            _gs.screen = SCREEN_GETREADY;
            _gs.endlessMode = 1;
            _gs.level = 1;
            _gs.score = 0;
            _gs.lives = MAX_LIVES;
            _gs.combo = 0;
            ResetPaddle(&_gs);
            Levels_Setup(&_gs, _gs.level);
            ResetBallToCenter(&_gs);
            _gs.getReadyTimer = 120;
        }
        break;

    case SCREEN_GETREADY:
        if (_gs.getReadyTimer > 0) {
            _gs.getReadyTimer--;
        } else {
            _gs.screen = SCREEN_PLAYING;
            _gs.ballOnPaddle = 0;
        }
        break;

    case SCREEN_PLAYING:
        /* Pause */
        if (Input_KeyPressed(VK_ESCAPE) || Input_KeyPressed(VK_P) ||
            Input_GamepadButton(GP_START)) {
            _gs.screen = SCREEN_PAUSE;
            break;
        }
        /* Speed boost toggle: F5 or GP_B */
        if (Input_KeyPressed(VK_F5) || Input_GamepadButton(GP_B))
            _gs.speedBoost ^= 1;

        /* Paddle movement */
        {
            float spd = (float)PADDLE_SPEED * (_gs.speedBoost ? 1.5f : 1.0f);
            if (Input_KeyDown(VK_LEFT) || Input_KeyDown(VK_A) ||
                Input_GamepadButton(GP_DPAD_LEFT)) {
                _gs.paddle.x -= spd;
            }
            if (Input_KeyDown(VK_RIGHT) || Input_KeyDown(VK_D) ||
                Input_GamepadButton(GP_DPAD_RIGHT)) {
                _gs.paddle.x += spd;
            }
            /* Gamepad left stick */
            if (Input_GamepadConnected()) {
                int lx = Input_GamepadLX();
                if (lx > 8000 || lx < -8000)
                    _gs.paddle.x += spd * (float)lx / 32767.0f;
            }
            /* Mouse tracking */
            {
                float mx = (float)Input_MouseX();
                float half = _gs.paddle.w / 2.0f;
                if (mx - half >= 0 && mx + half <= _gs.canvasW)
                    _gs.paddle.x = mx - half;
            }
            /* Clamp paddle */
            if (_gs.paddle.x < 0) _gs.paddle.x = 0;
            if (_gs.paddle.x + _gs.paddle.w > _gs.canvasW)
                _gs.paddle.x = (float)(_gs.canvasW - _gs.paddle.w);
        }

        /* Launch ball */
        if (_gs.ballOnPaddle) {
            if (Input_KeyPressed(VK_SPACE) || Input_KeyPressed(VK_RETURN) ||
                Input_GamepadButton(GP_A)) {
                _gs.ballOnPaddle = 0;
                _gs.balls[0].dy = -INITIAL_BALL_SPEED;
                _gs.balls[0].dx = 3.5f;
            } else {
                /* Ball follows paddle */
                _gs.balls[0].x = _gs.paddle.x + _gs.paddle.w / 2.0f;
                _gs.balls[0].y = _gs.paddle.y - _gs.balls[0].radius - 1.0f;
            }
        }

        /* Physics step (handles collisions, powerups, particle update) */
        {
            int steps = _gs.speedBoost ? 2 : 1;
            for (int i = 0; i < steps; i++)
                Physics_Update(&_gs);
        }

        /* Check win */
        if (AllBricksCleared(&_gs)) {
            AddScore(&_gs, COIN_LEVEL_BONUS * _gs.level);
            _gs.statsLevelsCompleted++;
            _gs.screen = SCREEN_LEVELWIN;
            _gs.levelWinTimer = 90;
            PlaySFXNote(880, 300);
        }

        /* Update combo timer */
        if (_gs.comboTimer > 0) {
            _gs.comboTimer--;
            if (_gs.comboTimer == 0) _gs.combo = 0;
        }
        break;

    case SCREEN_LEVELWIN:
        if (_gs.levelWinTimer > 0) {
            _gs.levelWinTimer--;
        } else {
            _gs.level++;
            _gs.screen = SCREEN_GETREADY;
            ResetPaddle(&_gs);
            Levels_Setup(&_gs, _gs.level);
            ResetBallToCenter(&_gs);
            _gs.getReadyTimer = 90;
            _gs.ballOnPaddle = 1;
        }
        break;

    case SCREEN_GAMEOVER:
        if (Input_KeyPressed(VK_SPACE) || Input_KeyPressed(VK_RETURN) ||
            Input_GamepadButton(GP_A)) {
            /* Save and go to high score */
            Save_Write(&_gs);
            _gs.screen = SCREEN_HIGHSCORE;
        }
        if (Input_KeyPressed(VK_ESCAPE) || Input_GamepadButton(GP_B)) {
            Save_Write(&_gs);
            _gs.screen = SCREEN_MENU;
        }
        break;

    case SCREEN_HIGHSCORE:
        if (Input_KeyPressed(VK_SPACE) || Input_KeyPressed(VK_RETURN) ||
            Input_KeyPressed(VK_ESCAPE) || Input_GamepadButton(GP_A)) {
            _gs.screen = SCREEN_MENU;
        }
        break;

    case SCREEN_PAUSE:
        if (Input_KeyPressed(VK_ESCAPE) || Input_KeyPressed(VK_P) ||
            Input_GamepadButton(GP_START)) {
            _gs.screen = SCREEN_PLAYING;
        }
        if (Input_KeyPressed(VK_Q)) {
            Save_Write(&_gs);
            _gs.screen = SCREEN_MENU;
        }
        break;

    case SCREEN_OPTIONS:
        if (Input_KeyPressed(VK_ESCAPE) || Input_GamepadButton(GP_B))
            _gs.screen = SCREEN_MENU;
        if (Input_KeyPressed(VK_LEFT) || Input_KeyPressed(VK_A) ||
            Input_GamepadButton(GP_DPAD_LEFT)) {
            if (_gs.optionsCursor == 0 && _gs.musicVolume > 0)
                _gs.musicVolume -= 50;
            if (_gs.optionsCursor == 1 && _gs.sfxVolume > 0)
                _gs.sfxVolume -= 10;
        }
        if (Input_KeyPressed(VK_RIGHT) || Input_KeyPressed(VK_D) ||
            Input_GamepadButton(GP_DPAD_RIGHT)) {
            if (_gs.optionsCursor == 0 && _gs.musicVolume < 1000)
                _gs.musicVolume += 50;
            if (_gs.optionsCursor == 1 && _gs.sfxVolume < 100)
                _gs.sfxVolume += 10;
        }
        if (Input_KeyPressed(VK_UP) || Input_GamepadButton(GP_DPAD_UP))
            if (_gs.optionsCursor > 0) _gs.optionsCursor--;
        if (Input_KeyPressed(VK_DOWN) || Input_GamepadButton(GP_DPAD_DOWN))
            if (_gs.optionsCursor < 4) _gs.optionsCursor++;
        /* Apply music volume live */
        Sound_SetMusicVol(_gs.musicVolume);
        break;

    case SCREEN_STORE:
        if (Input_KeyPressed(VK_ESCAPE) || Input_GamepadButton(GP_B)) {
            Store_Save(&_gs);
            _gs.screen = SCREEN_MENU;
        }
        /* Store navigation handled in draw_screens.c hit-test callbacks */
        break;

    case SCREEN_CREDITS:
        if (Input_KeyPressed(VK_ESCAPE) || Input_KeyPressed(VK_SPACE) ||
            Input_GamepadButton(GP_B)) {
            _gs.screen = SCREEN_MENU;
        }
        _gs.creditsScroll++;
        break;

    case SCREEN_STATS:
        if (Input_KeyPressed(VK_ESCAPE) || Input_GamepadButton(GP_B))
            _gs.screen = SCREEN_MENU;
        break;

    case SCREEN_DAILY:
        if (Input_KeyPressed(VK_ESCAPE) || Input_GamepadButton(GP_B))
            _gs.screen = SCREEN_MENU;
        if (Input_KeyPressed(VK_RETURN) || Input_GamepadButton(GP_A)) {
            _gs.screen = SCREEN_GETREADY;
            _gs.dailyMode = 1;
            _gs.level = 1;
            _gs.score = 0;
            _gs.lives = MAX_LIVES;
            ResetPaddle(&_gs);
            Levels_Setup(&_gs, _gs.level);
            ResetBallToCenter(&_gs);
            _gs.getReadyTimer = 90;
            _gs.ballOnPaddle = 1;
        }
        break;

    default:
        break;
    }

    /* Tick playtime counter (once per frame ≈ 60 fps) */
    if (_gs.screen == SCREEN_PLAYING) {
        _gs.playFrames++;
        if (_gs.playFrames >= 60) {
            _gs.playFrames = 0;
            _gs.statsPlaytimeSec++;
        }
    }

    /* End-of-frame: swap key tables */
    Input_EndFrame();
}

void Game_OnKey(int vk, int down) {
    /* Key state is already updated by WndProc → Input_SetKey.
       Additional per-key reactions (text input for name entry) handled here. */
    (void)vk;
    (void)down;
}

void Game_OnChar(int ch) {
    /* Name entry screen character input */
    if (_gs.screen == SCREEN_NAMEENTRY) {
        if (ch == '\r' || ch == '\n') {
            /* Confirm name */
            _gs.nameConfirmed = 1;
            _gs.screen = SCREEN_MENU;
        } else if (ch == '\b') {
            /* Backspace */
            if (_gs.nameLen > 0) _gs.nameLen--;
        } else if (_gs.nameLen < 15 && ch >= 32 && ch < 127) {
            _gs.playerName[_gs.nameLen++] = (char)ch;
        }
    }
}

void Game_OnMouse(int x, int y, int ldown, int lup, int wheel) {
    extern void Input_SetMouse(int x, int y);
    Input_SetMouse(x, y);

    /* Store click-through: pass raw coords to draw_screens for hit testing */
    if (ldown && _gs.screen == SCREEN_STORE) {
        extern void Store_HandleClick(GameState* gs, int x, int y);
        Store_HandleClick(&_gs, x, y);
    }

    /* Options slider drag */
    if (ldown && _gs.screen == SCREEN_OPTIONS) {
        extern void Options_HandleClick(GameState* gs, int x, int y);
        Options_HandleClick(&_gs, x, y);
    }

    (void)lup;
    (void)wheel;
}

void Game_OnSize(int w, int h) {
    if (w < 1) w = 1;
    if (h < 1) h = 1;
    _gs.canvasW = w;
    _gs.canvasH = h;
    /* Reposition paddle and balls proportionally (simple clamp) */
    if (_gs.paddle.x + _gs.paddle.w > w)
        _gs.paddle.x = (float)(w - _gs.paddle.w);
    for (int i = 0; i < MAX_BALLS; i++) {
        if (!_gs.balls[i].active) continue;
        if (_gs.balls[i].x > w) _gs.balls[i].x = (float)w;
        if (_gs.balls[i].y > h) _gs.balls[i].y = (float)h;
    }
}

void Game_OnPaint(HDC hdc) {
    /* Route to appropriate draw module */
    switch (_gs.screen) {
    case SCREEN_PLAYING:
    case SCREEN_PAUSE:
    case SCREEN_GETREADY:
    case SCREEN_LEVELWIN:
    case SCREEN_GAMEOVER:
        DrawGame_Frame(hdc, &_gs);
        break;
    default:
        DrawScreens_Frame(hdc, &_gs);
        break;
    }
}

void Game_Shutdown(void) {
    Save_Write(&_gs);
    Store_Save(&_gs);
    Sound_StopMusic();
}

/* ── Score / combo integration (called by physics.c) ────────── */
void Game_OnBrickHit(int row, int col, int brickPts) {
    _gs.combo++;
    if (_gs.combo > _gs.statsBestCombo)
        _gs.statsBestCombo = _gs.combo;
    _gs.comboTimer = 180;   /* ~3 seconds at 60fps */

    int pts = brickPts * (_gs.combo > 1 ? _gs.combo : 1);
    AddScore(&_gs, pts);

    /* Coin earn */
    int coins = COIN_PER_BRICK * (_gs.combo > 1 ? _gs.combo : 1);
    _gs.coinBalance += coins;
    _gs.statsCoinsEarned += coins;
    _gs.statsBricksDestroyed++;

    PlaySFXNote(440 + _gs.combo * 40, 60);
}

void Game_OnBallLost(void) {
    _gs.lives--;
    _gs.combo = 0;
    _gs.comboTimer = 0;
    PlaySFXNote(180, 400);

    if (_gs.lives <= 0) {
        _gs.screen = SCREEN_GAMEOVER;
        if (_gs.score > _gs.highScore)
            _gs.highScore = _gs.score;
    } else {
        ResetBallToCenter(&_gs);
        _gs.ballOnPaddle = 1;
    }
}

/* Accessor for external modules */
GameState* Game_GetState(void) {
    return &_gs;
}

