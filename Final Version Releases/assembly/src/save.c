/*
 * save.c — Binary save/load for player profile and store data
 * Replaces the JSON StoreSaveData used in Form1.vb.
 * Uses a simple flat binary format (versioned header).
 * No CRT. Win32 file APIs only.
 */

#include "BrickBlast.h"
#include "bbutil.h"

/* ── Save file format ────────────────────────────────────────────
 * Header (16 bytes):
 *   DWORD magic    = 0x42425353  ('BBSS' — BrickBlast Save State)
 *   DWORD version  = 2
 *   DWORD dataSize = sizeof(SavePayload)
 *   DWORD crc32    (simple CRC of the payload bytes)
 *
 * Payload (SavePayload struct — fixed layout):
 *   [player identity, economy, owned items bitmap, active skins, stats]
 * ───────────────────────────────────────────────────────────────── */

#define SAVE_MAGIC   0x42425353u
#define SAVE_VERSION 2u
#define SAVE_DIR     L"BrickBlast"

/* Owned-item bitmap: 64 bits covers up to 64 store items by index */
typedef struct {
    DWORD magic;
    DWORD version;
    DWORD dataSize;
    DWORD crc32;
} SaveHeader;

typedef struct {
    /* Player */
    char  playerName[32];

    /* Economy */
    int   coinBalance;
    int   totalCoinsEarned;

    /* Owned items: bit N set if storeItems[N] is owned */
    DWORD ownedBits[2];     /* 64 bits = up to 64 items */

    /* Active cosmetic indices */
    int   activeBallSkin;
    int   activeBrickPalette;
    int   activeBonusPack;
    int   activePaddleSkin;
    int   activeMusicStyle;
    int   activeSfxStyle;

    /* Persistent stats */
    int   highScore;
    int   totalBricksDestroyed;
    int   totalPlaytimeSec;
    int   bestCombo;
    int   levelsCompleted;

    /* Daily challenge */
    int   dailyBestScore;
    char  dailyLastDate[12];    /* "YYYY-MM-DD\0\0" */

    /* Endless mode */
    int   endlessBestScore;

    /* Settings */
    int   musicVolume;
    int   sfxVolume;
    int   colorblind;
    int   windowScale;

    DWORD _pad[4];
} SavePayload;

/* ── CRC-32 (standard polynomial) ──────────────────────────── */
static DWORD Crc32(const BYTE* data, int len) {
    DWORD crc = 0xFFFFFFFFu;
    for (int i = 0; i < len; i++) {
        BYTE b = data[i];
        for (int bit = 0; bit < 8; bit++) {
            DWORD mixed = (crc ^ b) & 1u;
            crc >>= 1;
            if (mixed) crc ^= 0xEDB88320u;
            b >>= 1;
        }
    }
    return ~crc;
}

/* ── Path helpers ────────────────────────────────────────────── */
static void GetSavePath(WCHAR* out) {
    WCHAR appData[MAX_PATH];
    /* SHGetFolderPath not available without Shell32 — use env var */
    DWORD n = GetEnvironmentVariableW(L"APPDATA", appData, MAX_PATH);
    if (!n) {
        GetTempPathW(MAX_PATH, appData);
    }

    /* Build path: %APPDATA%\BrickBlast\save.bin */
    int i = 0;
    while (appData[i]) i++;
    appData[i++] = L'\\';
    const WCHAR* sub = SAVE_DIR L"\\save.bin";
    for (int j = 0; sub[j]; j++) appData[i++] = sub[j];
    appData[i] = 0;
    for (int j = 0; j <= i; j++) out[j] = appData[j];
}

static void EnsureSaveDir(void) {
    WCHAR appData[MAX_PATH];
    DWORD n = GetEnvironmentVariableW(L"APPDATA", appData, MAX_PATH);
    if (!n) GetTempPathW(MAX_PATH, appData);

    int i = 0;
    while (appData[i]) i++;
    appData[i++] = L'\\';
    const WCHAR* sub = SAVE_DIR;
    for (int j = 0; sub[j]; j++) appData[i++] = sub[j];
    appData[i] = 0;
    CreateDirectoryW(appData, 0);  /* OK if already exists */
}

/* ── Map GameState ↔ SavePayload ─────────────────────────────── */
static void GsToPayload(const GameState* gs, SavePayload* p) {
    /* Zero fill */
    BYTE* pb = (BYTE*)p;
    for (int i = 0; i < (int)sizeof(SavePayload); i++) pb[i] = 0;

    /* Player name (WCHAR → char ASCII truncate) */
    for (int i = 0; i < 31 && gs->playerName[i]; i++)
        p->playerName[i] = (char)gs->playerName[i];

    p->coinBalance        = gs->coinBalance;
    p->totalCoinsEarned   = gs->statsCoinsEarned;
    p->activeBallSkin     = gs->skinBall;
    p->activeBrickPalette = gs->skinBrick;
    p->activeBonusPack    = gs->skinBonus;
    p->activePaddleSkin   = gs->skinPaddle;
    p->activeMusicStyle   = gs->musicStyle;
    p->activeSfxStyle     = gs->sfxStyle;
    p->highScore          = gs->highScore;
    p->totalBricksDestroyed = gs->statsBricksDestroyed;
    p->totalPlaytimeSec   = gs->statsPlaytimeSec;
    p->bestCombo          = gs->statsBestCombo;
    p->levelsCompleted    = gs->statsLevelsCompleted;
    p->dailyBestScore     = gs->dailyBestScore;
    p->endlessBestScore   = gs->endlessBestScore;
    p->musicVolume        = gs->musicVolume;
    p->sfxVolume          = gs->sfxVolume;
    p->colorblind         = gs->colorblind;
    p->windowScale        = gs->windowScale;

    /* Owned items: bit per store item index */
    for (int i = 0; i < gs->storeItemCount && i < 64; i++) {
        if (gs->storeItems[i].owned) {
            int word = i / 32;
            int bit  = i % 32;
            p->ownedBits[word] |= (1u << bit);
        }
    }

    /* Daily date */
    for (int i = 0; i < 11 && gs->dailyLastDate[i]; i++)
        p->dailyLastDate[i] = gs->dailyLastDate[i];
}

static void PayloadToGs(const SavePayload* p, GameState* gs) {
    /* Player name char → WCHAR */
    for (int i = 0; i < 31 && p->playerName[i]; i++)
        gs->playerName[i] = (WCHAR)p->playerName[i];

    gs->coinBalance           = p->coinBalance;
    gs->statsCoinsEarned      = p->totalCoinsEarned;
    gs->skinBall              = p->activeBallSkin;
    gs->skinBrick             = p->activeBrickPalette;
    gs->skinBonus             = p->activeBonusPack;
    gs->skinPaddle            = p->activePaddleSkin;
    gs->musicStyle            = p->activeMusicStyle;
    gs->sfxStyle              = p->activeSfxStyle;
    gs->highScore             = p->highScore;
    gs->statsBricksDestroyed  = p->totalBricksDestroyed;
    gs->statsPlaytimeSec      = p->totalPlaytimeSec;
    gs->statsBestCombo        = p->bestCombo;
    gs->statsLevelsCompleted  = p->levelsCompleted;
    gs->dailyBestScore        = p->dailyBestScore;
    gs->endlessBestScore      = p->endlessBestScore;
    gs->musicVolume           = p->musicVolume ? p->musicVolume : 500;
    gs->sfxVolume             = p->sfxVolume   ? p->sfxVolume   : 80;
    gs->colorblind            = p->colorblind;
    gs->windowScale           = p->windowScale ? p->windowScale : 100;

    /* Restore owned items */
    for (int i = 0; i < gs->storeItemCount && i < 64; i++) {
        int word = i / 32, bit = i % 32;
        gs->storeItems[i].owned = (p->ownedBits[word] >> bit) & 1;
    }

    /* Daily date */
    for (int i = 0; i < 11; i++)
        gs->dailyLastDate[i] = p->dailyLastDate[i];
}

/* ── Public API ──────────────────────────────────────────────── */

void Save_Load(GameState* gs) {
    WCHAR path[MAX_PATH];
    GetSavePath(path);

    HANDLE h = CreateFileW(path, GENERIC_READ, FILE_SHARE_READ, 0,
                           OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
    if (h == INVALID_HANDLE_VALUE) return;  /* no save file yet — use defaults */

    SaveHeader hdr;
    DWORD read;
    ReadFile(h, &hdr, sizeof(hdr), &read, 0);
    if (read != sizeof(hdr) || hdr.magic != SAVE_MAGIC) {
        CloseHandle(h);
        return;
    }

    SavePayload payload;
    ReadFile(h, &payload, sizeof(payload), &read, 0);
    CloseHandle(h);

    if (read != sizeof(payload)) return;

    /* Verify CRC */
    DWORD crc = Crc32((const BYTE*)&payload, sizeof(payload));
    if (crc != hdr.crc32) return;  /* corrupt save — ignore */

    PayloadToGs(&payload, gs);
}

void Save_Write(const GameState* gs) {
    EnsureSaveDir();

    WCHAR path[MAX_PATH];
    GetSavePath(path);

    SavePayload payload;
    GsToPayload(gs, &payload);

    SaveHeader hdr;
    hdr.magic    = SAVE_MAGIC;
    hdr.version  = SAVE_VERSION;
    hdr.dataSize = sizeof(SavePayload);
    hdr.crc32    = Crc32((const BYTE*)&payload, sizeof(payload));

    HANDLE h = CreateFileW(path, GENERIC_WRITE, 0, 0,
                           CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
    if (h == INVALID_HANDLE_VALUE) return;

    DWORD written;
    WriteFile(h, &hdr,     sizeof(hdr),     &written, 0);
    WriteFile(h, &payload, sizeof(payload), &written, 0);
    CloseHandle(h);
}

