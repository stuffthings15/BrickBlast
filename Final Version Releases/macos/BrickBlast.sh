#!/usr/bin/env bash
# BrickBlast macOS launcher
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
APPIMAGE="$SCRIPT_DIR/BrickBlast.dmg"
GAME="$SCRIPT_DIR/game/index.html"
if [ -f "$GAME" ]; then
  python3 -m http.server 7777 --directory "$SCRIPT_DIR/game" &
  SERVER=$!
  sleep 1
  open http://localhost:7777
  wait $SERVER
else
  echo "game/ folder not found."
fi
