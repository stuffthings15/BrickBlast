# Copilot Instructions — Brick Blast

## ⚠️ MAIN PROJECT — Always work here by default

| File | Project |
|------|---------|
| `Form1.vb` | ✅ MAIN — edit this unless told otherwise |
| `Form1.Designer.vb` | ✅ MAIN |
| `anime finder.vbproj` | ✅ MAIN project file |

## 🚫 SUB-PROJECTS — Only touch if explicitly asked

| Path | What it is |
|------|-----------|
| `anime finder wpf/` | WPF sub-project — DO NOT edit unless user says "WPF" |
| `anime finder macos/` | macOS sub-project — DO NOT edit unless user says "macOS" |
| `web/` | HTML5 browser version — DO NOT edit unless user says "HTML" or "browser" |
| `versions/` | Pre-built platform distributions — DO NOT edit source here |
| `mobile/` | Capacitor mobile build — DO NOT edit unless user says "mobile" or "Android/iOS" |

## Rules

1. If the user says "the game", "the project", or "main project" → `Form1.vb` only.
2. If a bug exists in both main and a sub-project, fix main first and ask before touching the sub-project.
3. Never publish or rebuild a sub-project unless explicitly requested.
4. `docs/` edits are always fine from the workspace root.
5. The WinForms executable lives at `versions\windows\BrickBlast.exe`.
6. The WPF executable lives at `versions\windows-wpf\BrickBlast.exe` — only rebuild it when asked.
