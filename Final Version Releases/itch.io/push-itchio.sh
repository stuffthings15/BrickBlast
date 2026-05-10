#!/usr/bin/env bash
# push-itchio.sh — upload the itch.io release using Butler
# Install Butler: https://itchio.itch.io/butler
# Usage: ./push-itchio.sh <itchio-username>
#
# Channel naming convention used here:
#   html       -> browser play (HTML5)
#   win-x64    -> Windows 64-bit
#   win-arm64  -> Windows ARM64
#   linux-x64  -> Linux 64-bit (Electron AppImage)
#   linux-arm64-> Linux ARM64  (Electron AppImage)
#   osx        -> macOS (Electron DMG)

set -e
USER=${1:-"stuffthings15"}
GAME="brickblast-velocity-market"
ROOT="$(dirname "$0")/.."

echo "=== Pushing BrickBlast: Velocity Market to itch.io as $USER/$GAME ==="

butler push "$ROOT/html"              "$USER/$GAME:html"        --userversion 1.0.0
butler push "$ROOT/windows-x64"       "$USER/$GAME:win-x64"     --userversion 1.0.0
butler push "$ROOT/windows-arm64"     "$USER/$GAME:win-arm64"   --userversion 1.0.0
butler push "$ROOT/electron-linux"    "$USER/$GAME:linux-x64"   --userversion 1.0.0
butler push "$ROOT/electron-macos"    "$USER/$GAME:osx"         --userversion 1.0.0

echo "=== Done! Visit https://itch.io/dashboard to publish the draft. ==="
