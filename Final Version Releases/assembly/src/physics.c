/*
 * physics.c — Ball, Paddle, Brick, and Power-Up Collision
 * Ported from Form1.vb GameTimer_Tick physics section.
 * No CRT. Win32 only.
 */

#include "BrickBlast.h"
#include "bbutil.h"
#include <math.h>   /* sqrtf, fabsf — pulled in via /nodefaultlib workaround below */

/* math.h intrinsics are fine with MSVC even under /nodefaultlib
   as long as we only use inline forms.  sqrtf → __sqrtf, fabsf → __fabsf */
#pragma intrinsic(sqrtf, fabsf)

/* Callbacks into game.c */
extern void Game_OnBrickHit(int row, int col, int pts);
extern void Game_OnBallLost(void);
extern void Sound_PlaySFX(const BYTE* wav, int len);
extern BYTE* Music_GenerateSFX(int freq, int ms, int vol, int* outLen);

static GameState* _gs;  /* set on first call */

/* ── Internal helpers ───────────────────────────────────────── */

static void PlayTone(int freq, int ms) {
    if (!_gs) return;
    int len = 0;
    BYTE* wav = Music_GenerateSFX(freq, ms, _gs->sfxVolume, &len);
    if (wav && len > 0) Sound_PlaySFX(wav, len);
}

static float Clamp(float v, float lo, float hi) {
    if (v < lo) return lo;
    if (v > hi) return hi;
    return v;
}

/* Brick world-space rect */
static void BrickRect(int r, int c, float* x, float* y, float* w, float* h) {
    *x = (float)(BRICK_LEFT_OFFSET + c * (BRICK_WIDTH  + BRICK_PADDING));
    *y = (float)(BRICK_TOP_OFFSET  + r * (BRICK_HEIGHT + BRICK_PADDING));
    *w = (float)BRICK_WIDTH;
    *h = (float)BRICK_HEIGHT;
}

/* Circle–AABB collision; returns 1 and sets nx,ny = surface normal */
static int CircleAABB(float cx, float cy, float cr,
                       float rx, float ry, float rw, float rh,
                       float* nx, float* ny)
{
    float nearX = Clamp(cx, rx, rx + rw);
    float nearY = Clamp(cy, ry, ry + rh);
    float dx = cx - nearX;
    float dy = cy - nearY;
    float dist2 = dx * dx + dy * dy;
    if (dist2 >= cr * cr) return 0;

    float dist = sqrtf(dist2);
    if (dist < 0.0001f) {
        *nx = 0.0f; *ny = -1.0f;
    } else {
        *nx = dx / dist;
        *ny = dy / dist;
    }
    return 1;
}

/* Reflect velocity over a normal */
static void Reflect(float* dx, float* dy, float nx, float ny) {
    float dot = (*dx) * nx + (*dy) * ny;
    *dx -= 2.0f * dot * nx;
    *dy -= 2.0f * dot * ny;
}

/* Ensure ball speed stays near target */
static void NormalizeSpeed(Ball* b, float target) {
    float spd = sqrtf(b->dx * b->dx + b->dy * b->dy);
    if (spd < 0.5f) spd = 0.5f;
    float scale = target / spd;
    b->dx *= scale;
    b->dy *= scale;
}

/* ── Power-up spawn ─────────────────────────────────────────── */
static void MaybeSpawnPowerUp(GameState* gs, float x, float y) {
    /* ~15% chance per brick */
    gs->rngState = gs->rngState * 1664525u + 1013904223u;
    if ((gs->rngState & 0xFF) > 38) return;   /* 38/255 ≈ 15% */

    for (int i = 0; i < MAX_POWERUPS; i++) {
        if (gs->powerups[i].active) continue;
        gs->powerups[i].active = 1;
        gs->powerups[i].x = x;
        gs->powerups[i].y = y;
        gs->powerups[i].dy = POWERUP_SPEED;
        /* random type */
        gs->rngState = gs->rngState * 1664525u + 1013904223u;
        int typeCount = 8; /* matches PowerUpType enum count */
        gs->powerups[i].type = (PowerUpType)((gs->rngState >> 16) % typeCount);
        break;
    }
}

/* ── Apply power-up effect ──────────────────────────────────── */
static void ApplyPowerUp(GameState* gs, PowerUpType t) {
    switch (t) {
    case PU_EXTRA_LIFE:
        if (gs->lives < MAX_LIVES) gs->lives++;
        break;
    case PU_WIDE_PADDLE:
        gs->paddle.w = (float)(PADDLE_WIDTH * 2);
        gs->widePaddleTimer = 600;
        break;
    case PU_MULTIBALL:
        /* Duplicate first active ball up to MAX_BALLS */
        for (int i = 1; i < MAX_BALLS && i < 3; i++) {
            if (!gs->balls[i].active) {
                gs->balls[i] = gs->balls[0];
                gs->balls[i].dx = -gs->balls[0].dx + (i == 1 ? 1.5f : -1.5f);
                gs->balls[i].active = 1;
                gs->ballCount++;
                break;
            }
        }
        break;
    case PU_GROW_BALL:
        for (int i = 0; i < MAX_BALLS; i++)
            if (gs->balls[i].active) {
                gs->balls[i].radius += 4;
                if (gs->balls[i].radius > MAX_BALL_RADIUS)
                    gs->balls[i].radius = MAX_BALL_RADIUS;
            }
        break;
    case PU_SHRINK_BALL:
        for (int i = 0; i < MAX_BALLS; i++)
            if (gs->balls[i].active) {
                gs->balls[i].radius -= 3;
                if (gs->balls[i].radius < MIN_BALL_RADIUS)
                    gs->balls[i].radius = MIN_BALL_RADIUS;
            }
        break;
    case PU_SLOW_BALL:
        for (int i = 0; i < MAX_BALLS; i++)
            if (gs->balls[i].active)
                NormalizeSpeed(&gs->balls[i], INITIAL_BALL_SPEED * 0.6f);
        break;
    case PU_FAST_BALL:
        for (int i = 0; i < MAX_BALLS; i++)
            if (gs->balls[i].active)
                NormalizeSpeed(&gs->balls[i], INITIAL_BALL_SPEED * 1.5f);
        break;
    case PU_FIREBALL:
        gs->fireballTimer = 300;
        break;
    }
    PlayTone(660, 120);
}

/* ── Particle spawn ─────────────────────────────────────────── */
static void SpawnParticles(GameState* gs, float x, float y, COLORREF col) {
    for (int n = 0; n < 6; n++) {
        for (int i = 0; i < MAX_PARTICLES; i++) {
            if (gs->particles[i].life > 0) continue;
            Particle* p = &gs->particles[i];
            p->x = x;
            p->y = y;
            p->color = col;
            /* deterministic scatter using RNG */
            gs->rngState = gs->rngState * 1664525u + 1013904223u;
            float angle = (float)(gs->rngState & 0xFFFF) / 65535.0f * 6.2832f;
            float spd = 1.5f + (float)((gs->rngState >> 8) & 0x7) * 0.4f;
            p->dx = spd * (float)(int)(100.0f * (float)(gs->rngState & 1 ? 1 : -1));
            /* simple: just set integer-style scatter */
            p->dx = ((int)((gs->rngState >> 4) & 0xF) - 7) * 0.4f;
            p->dy = ((int)((gs->rngState >> 8) & 0xF) - 5) * 0.5f - 1.0f;
            p->life = 30 + (int)((gs->rngState >> 12) & 0x1F);
            break;
        }
    }
}

/* ── Main physics step ──────────────────────────────────────── */
void Physics_Update(GameState* gs) {
    _gs = gs;

    /* ── Wide paddle timer ──────────────────────────────────── */
    if (gs->widePaddleTimer > 0) {
        gs->widePaddleTimer--;
        if (gs->widePaddleTimer == 0)
            gs->paddle.w = (float)PADDLE_WIDTH;
    }

    /* ── Fireball timer ─────────────────────────────────────── */
    if (gs->fireballTimer > 0)
        gs->fireballTimer--;

    /* ── Particles ──────────────────────────────────────────── */
    for (int i = 0; i < MAX_PARTICLES; i++) {
        Particle* p = &gs->particles[i];
        if (p->life <= 0) continue;
        p->x += p->dx;
        p->y += p->dy;
        p->dy += 0.15f;   /* gravity */
        p->life--;
    }

    /* ── Power-ups ──────────────────────────────────────────── */
    for (int i = 0; i < MAX_POWERUPS; i++) {
        PowerUp* pu = &gs->powerups[i];
        if (!pu->active) continue;
        pu->y += pu->dy;

        /* Fell off screen */
        if (pu->y > gs->canvasH + POWERUP_SIZE) {
            pu->active = 0;
            continue;
        }

        /* Paddle catch AABB */
        float pr = (float)POWERUP_SIZE / 2.0f;
        if (pu->x + pr > gs->paddle.x &&
            pu->x - pr < gs->paddle.x + gs->paddle.w &&
            pu->y + pr > gs->paddle.y &&
            pu->y - pr < gs->paddle.y + gs->paddle.h) {
            ApplyPowerUp(gs, pu->type);
            pu->active = 0;
        }
    }

    /* ── Balls ──────────────────────────────────────────────── */
    int activeBalls = 0;
    for (int i = 0; i < MAX_BALLS; i++) {
        Ball* b = &gs->balls[i];
        if (!b->active) continue;
        activeBalls++;

        float spd = INITIAL_BALL_SPEED;
        /* Increase speed by 10% per 5 levels */
        spd *= 1.0f + (gs->level / 5) * 0.10f;
        /* Clamp max speed */
        if (spd > 18.0f) spd = 18.0f;

        b->x += b->dx;
        b->y += b->dy;

        /* ── Wall collisions ────────────────────────────────── */
        if (b->x - b->radius < 0) {
            b->x = (float)b->radius;
            b->dx = fabsf(b->dx);
            PlayTone(300, 40);
        }
        if (b->x + b->radius > gs->canvasW) {
            b->x = (float)(gs->canvasW - b->radius);
            b->dx = -fabsf(b->dx);
            PlayTone(300, 40);
        }
        if (b->y - b->radius < 0) {
            b->y = (float)b->radius;
            b->dy = fabsf(b->dy);
            PlayTone(350, 40);
        }

        /* ── Ball lost ──────────────────────────────────────── */
        if (b->y - b->radius > gs->canvasH) {
            b->active = 0;
            gs->ballCount--;
            /* If last ball: signal game.c */
            int anyLeft = 0;
            for (int j = 0; j < MAX_BALLS; j++)
                if (gs->balls[j].active) { anyLeft = 1; break; }
            if (!anyLeft)
                Game_OnBallLost();
            continue;
        }

        /* ── Paddle collision ───────────────────────────────── */
        {
            float nx, ny;
            if (CircleAABB(b->x, b->y, (float)b->radius,
                           gs->paddle.x, gs->paddle.y,
                           gs->paddle.w, gs->paddle.h,
                           &nx, &ny)) {
                /* Reflect off top of paddle; add spin based on hit position */
                float hitT = (b->x - gs->paddle.x) / gs->paddle.w; /* 0..1 */
                b->dx = (hitT - 0.5f) * 2.0f * spd * 0.8f;
                b->dy = -fabsf(b->dy);
                b->y = gs->paddle.y - (float)b->radius - 0.5f;
                NormalizeSpeed(b, spd);
                PlayTone(500, 50);
            }
        }

        /* ── Brick collisions ───────────────────────────────── */
        for (int r = 0; r < BRICK_ROWS; r++) {
            for (int c = 0; c < BRICK_COLS; c++) {
                Brick* br = &gs->bricks[r][c];
                if (br->hp <= 0) continue;

                float bx, by, bw, bh;
                BrickRect(r, c, &bx, &by, &bw, &bh);

                float nx, ny;
                if (!CircleAABB(b->x, b->y, (float)b->radius,
                                bx, by, bw, bh, &nx, &ny)) continue;

                /* Fireball: destroy without bouncing */
                if (gs->fireballTimer > 0) {
                    SpawnParticles(gs, bx + bw / 2, by + bh / 2, br->color);
                    br->hp = 0;
                    MaybeSpawnPowerUp(gs, bx + bw / 2, by + bh / 2);
                    Game_OnBrickHit(r, c, 10 * br->maxHp);
                    continue;
                }

                Reflect(&b->dx, &b->dy, nx, ny);
                NormalizeSpeed(b, spd);
                /* Push ball out */
                b->x += nx * 1.5f;
                b->y += ny * 1.5f;

                br->hp--;
                SpawnParticles(gs, bx + bw / 2, by + bh / 2, br->color);

                if (br->hp <= 0) {
                    MaybeSpawnPowerUp(gs, bx + bw / 2, by + bh / 2);
                    Game_OnBrickHit(r, c, 10 * br->maxHp);
                } else {
                    /* Hit but not destroyed: smaller score */
                    Game_OnBrickHit(r, c, 5);
                }
                PlayTone(440, 30);
            }
        }
    }
}

