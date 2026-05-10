# BrickBlast — Electron Wrappers

Three folders contain identical source structure, differing only in their
`package.json` build targets:

| Folder | Target |
|--------|--------|
| `electron-windows/` | Windows x64 + ARM64 → NSIS installer `.exe` |
| `electron-linux/`   | Linux x64 + ARM64  → AppImage `.AppImage`  |
| `electron-macos/`   | macOS x64 + ARM64  → DMG `.dmg`           |

## Build Steps (any folder)

```bash
cd electron-<platform>
npm install
npm run build
```

Output lands in `dist/`.

## Requirements
- Node 18+ / npm 9+
- For macOS DMG: must build on macOS
- For Windows code signing: supply `--win --certificateFile` or use EV cert
- For Linux AppImage: build on Linux or use Docker

## Keyboard Controls (in-app)
Arrow keys / A D — move paddle  
F — speed boost  P — pause  S — store  H — options
