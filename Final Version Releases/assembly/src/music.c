/*
 * music.c — PCM WAV SFX synthesis + MIDI-file music generation
 * Ported from Form1.vb: GenerateWav, GetMusicData, GenerateMidiBytes,
 * PreGenerateAllMusic, StartMusic, etc.
 * No CRT. Win32 heap only.
 */

#include "BrickBlast.h"
#include "bbutil.h"

/* ── Win32 heap ───────────────────────────────────────────────── */
static HANDLE _heap;

static void* HAlloc(SIZE_T n) {
    if (!_heap) _heap = GetProcessHeap();
    return HeapAlloc(_heap, HEAP_ZERO_MEMORY, n);
}
static void HFree(void* p) {
    if (p) HeapFree(_heap, 0, p);
}

/* ── Math helpers (no libm) ─────────────────────────────────── */
#pragma intrinsic(sin)
static double MySin(double x) { return sin(x); }

#define PI    3.14159265358979323846
#define TWOPI (2.0 * PI)

/* ── WAV header builder ─────────────────────────────────────── */
/* Returns heap-allocated buffer; caller must NOT free (stored in gs) */
static void WriteLE16(BYTE* p, WORD v) {
    p[0] = (BYTE)(v & 0xFF);
    p[1] = (BYTE)(v >> 8);
}
static void WriteLE32(BYTE* p, DWORD v) {
    p[0] = (BYTE)(v & 0xFF);
    p[1] = (BYTE)((v >> 8) & 0xFF);
    p[2] = (BYTE)((v >> 16) & 0xFF);
    p[3] = (BYTE)((v >> 24) & 0xFF);
}

/* ── PCM WAV SFX generation ─────────────────────────────────── */
/*
 * Music_GenerateSFX(freqHz, durationMs, vol 0-100, *outLen)
 * Returns pointer to raw WAV bytes (RIFF header + PCM samples).
 * Generates a simple sine-wave tone with linear decay envelope.
 */
BYTE* Music_GenerateSFX(int freqHz, int durationMs, int vol, int* outLen) {
    if (!_heap) _heap = GetProcessHeap();

    const int sampleRate = 44100;
    const int channels   = 1;
    const int bitsPerSmp = 16;
    const int bytePerSmp = bitsPerSmp / 8;

    int numSamples = (sampleRate * durationMs) / 1000;
    int dataBytes  = numSamples * bytePerSmp * channels;
    int totalBytes = 44 + dataBytes;    /* 44-byte RIFF/WAVE/fmt /data header */

    BYTE* buf = (BYTE*)HAlloc(totalBytes);
    if (!buf) { *outLen = 0; return 0; }

    /* RIFF header */
    buf[0]='R'; buf[1]='I'; buf[2]='F'; buf[3]='F';
    WriteLE32(buf+4,  (DWORD)(totalBytes - 8));
    buf[8]='W'; buf[9]='A'; buf[10]='V'; buf[11]='E';

    /* fmt  chunk */
    buf[12]='f'; buf[13]='m'; buf[14]='t'; buf[15]=' ';
    WriteLE32(buf+16, 16);              /* chunk size */
    WriteLE16(buf+20, 1);              /* PCM */
    WriteLE16(buf+22, (WORD)channels);
    WriteLE32(buf+24, (DWORD)sampleRate);
    WriteLE32(buf+28, (DWORD)(sampleRate * channels * bytePerSmp));
    WriteLE16(buf+32, (WORD)(channels * bytePerSmp));
    WriteLE16(buf+34, (WORD)bitsPerSmp);

    /* data chunk */
    buf[36]='d'; buf[37]='a'; buf[38]='t'; buf[39]='a';
    WriteLE32(buf+40, (DWORD)dataBytes);

    /* PCM samples */
    SHORT* pcm = (SHORT*)(buf + 44);
    double volScale = (vol / 100.0) * 32767.0;
    double phaseInc = TWOPI * freqHz / sampleRate;
    double phase = 0.0;

    for (int i = 0; i < numSamples; i++) {
        /* Linear decay envelope */
        double env = 1.0 - (double)i / numSamples;
        double sample = MySin(phase) * env * volScale;
        /* Clamp */
        int s = (int)sample;
        if (s >  32767) s =  32767;
        if (s < -32768) s = -32768;
        pcm[i] = (SHORT)s;
        phase += phaseInc;
        if (phase >= TWOPI) phase -= TWOPI;
    }

    *outLen = totalBytes;
    return buf;
}

/* ── Music track catalog (6 styles, matches Form1.vb _musicStyleNames) ─── */
/*
 * Each style is a melody as (freq,dur) pairs.
 * 0 = Brick Blast (original),  1 = Calculated Impact, 2 = Machine Precision,
 * 3 = Machine, 4 = Pinball Dream, 5 = Pinball
 */
typedef struct { int freq; int dur; } Note;

static const Note _style0[] = {
    {523,200},{587,200},{659,200},{698,400},{0,100},
    {784,200},{880,200},{988,200},{1047,400},{0,200},
    {988,150},{880,150},{784,150},{698,300},{0,100},
    {659,150},{587,150},{523,150},{494,300},{0,300},
    {0,0}
};
static const Note _style1[] = {
    {440,150},{494,150},{523,300},{587,150},{659,300},{0,150},
    {784,150},{880,300},{988,150},{1047,300},{0,200},
    {0,0}
};
static const Note _style2[] = {
    {349,100},{392,100},{440,200},{494,100},{523,200},{0,100},
    {587,100},{659,100},{698,200},{784,100},{880,200},{0,200},
    {0,0}
};
static const Note _style3[] = {
    {220,200},{247,200},{262,400},{294,200},{330,400},{0,200},
    {370,200},{415,200},{440,400},{0,300},{0,0}
};
static const Note _style4[] = {
    {392,150},{440,150},{494,300},{440,150},{392,300},{0,100},
    {349,150},{392,150},{440,300},{494,150},{523,400},{0,200},
    {0,0}
};
static const Note _style5[] = {
    {262,100},{294,100},{330,200},{349,100},{392,200},{0,100},
    {440,100},{494,100},{523,200},{0,200},{0,0}
};

static const Note* _styles[] = {
    _style0, _style1, _style2, _style3, _style4, _style5
};
static const int   _styleCount = 6;

/* ── MIDI byte helpers ─────────────────────────────────────── */
static void AppendByte(BYTE** buf, int* cap, int* len, BYTE b) {
    if (*len >= *cap) {
        *cap = (*cap) ? (*cap) * 2 : 512;
        BYTE* nb = (BYTE*)HAlloc(*cap);
        if (*buf) {
            /* copy old */
            for (int i = 0; i < *len; i++) nb[i] = (*buf)[i];
            HFree(*buf);
        }
        *buf = nb;
    }
    (*buf)[(*len)++] = b;
}
#define EMIT(b) AppendByte(&midi, &cap, &len, (BYTE)(b))

static void EmitVLQ(BYTE** buf, int* cap, int* len, int v) {
    /* Variable-length MIDI quantity */
    if (v < 128) {
        AppendByte(buf, cap, len, (BYTE)v);
        return;
    }
    BYTE tmp[4];
    int n = 0;
    tmp[n++] = v & 0x7F;
    v >>= 7;
    while (v) { tmp[n++] = (v & 0x7F) | 0x80; v >>= 7; }
    for (int i = n-1; i >= 0; i--)
        AppendByte(buf, cap, len, tmp[i]);
}
#define VLQ(v) EmitVLQ(&midi, &cap, &len, (v))

/* ── Frequency → MIDI note number (A4=440=69) ────────────────── */
static int FreqToMidi(int freq) {
    if (freq <= 0) return -1;
    /* note = 69 + 12*log2(freq/440) */
    double r = (double)freq / 440.0;
    /* log2 via integer approximation loop */
    int oct = 0;
    while (r >= 2.0) { r /= 2.0; oct++; }
    while (r < 1.0)  { r *= 2.0; oct--; }
    /* r is in [1,2), compute fractional semitones */
    /* ln(r)/ln(2)*12 ≈ */
    double semi = 0.0;
    double step = 1.0;
    for (int i = 0; i < 12; i++) {
        step *= 1.0594630943592952; /* 2^(1/12) */
        if (r < step) break;
        semi += 1.0;
    }
    int note = 69 + oct * 12 + (int)semi;
    if (note < 0)   note = 0;
    if (note > 127) note = 127;
    return note;
}

/* ── Generate a simple MIDI type-0 file from a style ─────────── */
/* Returns heap buffer; caller owns it */
static BYTE* GenerateMidi(int style, int* outLen) {
    if (style < 0 || style >= _styleCount) {
        *outLen = 0; return 0;
    }
    const Note* melody = _styles[style];

    /* Tempo: 120 BPM = 500000 microseconds per beat
       Ticks per beat: 480 */
    const int TPB = 480;
    const int TEMPO = 500000;

    BYTE* midi = 0;
    int cap = 0, len = 0;

    /* MIDI header: MThd */
    EMIT('M');EMIT('T');EMIT('h');EMIT('d');
    EMIT(0);EMIT(0);EMIT(0);EMIT(6);   /* header length = 6 */
    EMIT(0);EMIT(0);                    /* format 0 */
    EMIT(0);EMIT(1);                    /* 1 track */
    EMIT((BYTE)(TPB >> 8));EMIT((BYTE)(TPB & 0xFF));

    /* Track chunk header — fill length after building body */
    int trackHdrPos = len;
    EMIT('M');EMIT('T');EMIT('r');EMIT('k');
    EMIT(0);EMIT(0);EMIT(0);EMIT(0);   /* placeholder length */

    int trackStart = len;

    /* Tempo meta-event: delta=0, FF 51 03 <3 bytes> */
    VLQ(0);
    EMIT(0xFF);EMIT(0x51);EMIT(0x03);
    EMIT((BYTE)((TEMPO >> 16) & 0xFF));
    EMIT((BYTE)((TEMPO >>  8) & 0xFF));
    EMIT((BYTE)( TEMPO        & 0xFF));

    /* Program change: delta=0, channel 0, instrument 80 (Lead Square) */
    VLQ(0); EMIT(0xC0); EMIT(80);

    /* Emit notes */
    int prevTick = 0;
    int curTick  = 0;
    const BYTE VEL_ON  = 96;
    const BYTE VEL_OFF = 0;
    const BYTE CH      = 0x90;  /* note on, channel 0 */
    const BYTE CHoff   = 0x80;  /* note off */

    for (int i = 0; melody[i].freq != 0 || melody[i].dur != 0; i++) {
        int freq = melody[i].freq;
        int durMs= melody[i].dur;
        /* Convert ms to ticks: ticks = ms * TPB / (TEMPO/1000) = ms * TPB * 1000 / TEMPO */
        int ticks = (int)((long long)durMs * TPB * 1000 / TEMPO);

        if (freq <= 0) {
            /* Rest: just advance time */
            curTick += ticks;
            continue;
        }

        int midiNote = FreqToMidi(freq);
        if (midiNote < 0) { curTick += ticks; continue; }

        /* Note ON */
        VLQ(curTick - prevTick);
        EMIT(CH); EMIT((BYTE)midiNote); EMIT(VEL_ON);
        prevTick = curTick;

        /* Note OFF at curTick + ticks - small gap */
        int offTick = curTick + ticks - (ticks / 10 + 1);
        curTick += ticks;

        VLQ(offTick - prevTick);
        EMIT(CHoff); EMIT((BYTE)midiNote); EMIT(VEL_OFF);
        prevTick = offTick;
    }

    /* End of track: delta=0, FF 2F 00 */
    VLQ(curTick - prevTick);
    EMIT(0xFF); EMIT(0x2F); EMIT(0x00);

    int trackEnd = len;
    int trackLen = trackEnd - trackStart;

    /* Patch track chunk length */
    midi[trackHdrPos + 4] = (BYTE)((trackLen >> 24) & 0xFF);
    midi[trackHdrPos + 5] = (BYTE)((trackLen >> 16) & 0xFF);
    midi[trackHdrPos + 6] = (BYTE)((trackLen >>  8) & 0xFF);
    midi[trackHdrPos + 7] = (BYTE)( trackLen         & 0xFF);

    *outLen = len;
    return midi;
}

/* ── Temp file management ─────────────────────────────────────── */
/* We write MIDI to %TEMP%\BrickBlast_trackN.mid and play via MCI */

static WCHAR _trackPaths[6][MAX_PATH];
static int   _trackGenerated[6];

static void GetTempMidiPath(int style, WCHAR* out) {
    WCHAR tmp[MAX_PATH];
    GetTempPathW(MAX_PATH, tmp);
    /* Append filename */
    const WCHAR prefix[] = L"BrickBlast_track";
    const WCHAR ext[]    = L".mid";
    int i = 0;
    while (tmp[i]) i++;
    /* copy prefix */
    for (int j = 0; prefix[j]; j++) tmp[i++] = prefix[j];
    /* append digit */
    tmp[i++] = (WCHAR)('0' + style);
    for (int j = 0; ext[j]; j++) tmp[i++] = ext[j];
    tmp[i] = 0;
    for (int j = 0; j <= i; j++) out[j] = tmp[j];
}

static int WriteFileFull(const WCHAR* path, const BYTE* data, int len) {
    HANDLE h = CreateFileW(path, GENERIC_WRITE, 0, 0,
                           CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
    if (h == INVALID_HANDLE_VALUE) return 0;
    DWORD written;
    WriteFile(h, data, len, &written, 0);
    CloseHandle(h);
    return (int)written == len;
}

/* ── Public API ──────────────────────────────────────────────── */

void Music_GenerateAll(GameState* gs) {
    (void)gs;
    for (int s = 0; s < _styleCount; s++) {
        if (_trackGenerated[s]) continue;
        int len = 0;
        BYTE* midi = GenerateMidi(s, &len);
        if (midi && len > 0) {
            GetTempMidiPath(s, _trackPaths[s]);
            WriteFileFull(_trackPaths[s], midi, len);
            HFree(midi);
            _trackGenerated[s] = 1;
        }
    }
}

BYTE* Music_GetTrack(int style, int* outLen) {
    /* Not used for file playback; kept for direct waveOut path if needed */
    *outLen = 0;
    return 0;
}

void Music_PlayTrack(int style, int vol) {
    extern void Sound_PlayMusic(const WCHAR* path);
    extern void Sound_SetMusicVol(int v);
    if (style < 0 || style >= _styleCount) return;
    if (!_trackGenerated[style]) {
        Music_GenerateAll(0);
    }
    Sound_PlayMusic(_trackPaths[style]);
    Sound_SetMusicVol(vol);
}

