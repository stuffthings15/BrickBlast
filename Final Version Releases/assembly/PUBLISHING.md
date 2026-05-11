# Publishing Guide — Assembly Launcher (Windows x64)

**Target:** Windows x64 — minimal Win32 assembly launcher stub  
**Artifact:** `BrickBlast-Launcher.exe` (MASM/Win32, no .NET required)  
**Status:** ✅ BUILT

---

## What's in This Folder

| File/Folder | Description |
|-------------|-------------|
| `BrickBlast-Launcher.exe` | Compiled Win32 ASM launcher |
| `BrickBlast64.asm` | MASM source — `MessageBoxW` + `ShellExecuteW` stub |
| `build.bat` | Build script (requires MASM + Windows SDK) |

---

## What It Does

The launcher is a minimal Win32 executable (~5 KB) that:
1. Displays a splash `MessageBox` ("Launching Brick Blast…")
2. Calls `ShellExecuteW` to launch the full `BrickBlast.exe` from the `windows-x64/` folder
3. Exits immediately

It demonstrates a native ASM entry point without any managed runtime dependency.

---

## Rebuild from Source

Requires Visual Studio with MASM tools or standalone ML64.exe + Windows SDK:

```bat
build.bat
```

Or manually:
```bat
ml64 /c BrickBlast64.asm
link /SUBSYSTEM:WINDOWS /ENTRY:WinMain /OUT:BrickBlast-Launcher.exe BrickBlast64.obj kernel32.lib user32.lib shell32.lib
```

---

## Distribution

This artifact is a supplementary launcher — it is not the primary distribution artifact. Include it with the Windows x64 release as a curiosity/bonus:

1. Copy `BrickBlast-Launcher.exe` into the `windows-x64/` release folder
2. Include a note in README: *"BrickBlast-Launcher.exe is a native Win32 ASM launcher stub."*

It does **not** need its own store listing — it is bundled inside the Windows release.

---

## Testing Before Publish

- [ ] Double-click `BrickBlast-Launcher.exe`
- [ ] MessageBox appears with launch message
- [ ] `BrickBlast.exe` opens after dismissing dialog
- [ ] No UAC prompt (no elevation required)
- [ ] File size is under 10 KB
