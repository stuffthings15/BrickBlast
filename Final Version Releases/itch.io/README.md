# BrickBlast — itch.io Release

## Upload (HTML5)

1. Zip the `assets/` folder (it contains the full HTML5 game).
2. On your itch.io project page set **Kind of project** → `HTML`.
3. Upload the zip and tick **This file will be played in the browser**.
4. Set **Viewport dimensions** to `1200 × 867`.

## Upload (Desktop channels via Butler)

Install [Butler](https://itch.io/docs/butler/) then run:

```bash
bash push-itchio.sh
```

The script pushes:
| Channel | Source |
|---------|--------|
| `html` | `assets/` |
| `win-x64` | `../windows-x64/` |
| `win-arm64` | `../windows-arm64/` |
| `linux-x64` | `../linux-x64/` |
| `osx` | `../macos/` |

Edit `push-itchio.sh` to set your itch.io username and game slug before running.
