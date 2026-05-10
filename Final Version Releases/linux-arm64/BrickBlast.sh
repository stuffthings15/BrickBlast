#!/usr/bin/env bash
# BrickBlast native Linux launcher
# Run this script from the directory containing it.
# It prefers the pre-built Electron AppImage; falls back to a simple browser launch.
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

APPIMAGE="$SCRIPT_DIR/BrickBlast-1.0.0.AppImage"
if [ -f "$APPIMAGE" ]; then
  chmod +x "$APPIMAGE"
  exec "$APPIMAGE" "$@"
fi

# Fallback: serve the HTML5 game in the default browser using Python
GAME_DIR="$SCRIPT_DIR/game"
if [ -d "$GAME_DIR" ]; then
  echo "AppImage not found; launching browser fallback on http://localhost:7777"
  cd "$GAME_DIR"
  if command -v python3 &>/dev/null; then
    python3 -m http.server 7777 &
  else
    python -m SimpleHTTPServer 7777 &
  fi
  SERVER_PID=$!
  sleep 1
  if command -v xdg-open &>/dev/null; then
    xdg-open http://localhost:7777
  fi
  wait $SERVER_PID
else
  echo "ERROR: Neither AppImage nor game/ folder found."
  exit 1
fi
