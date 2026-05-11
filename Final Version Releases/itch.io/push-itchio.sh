#!/usr/bin/env bash
# push-itchio.sh — upload the itch.io release using Butler
# Install Butler: https://itchio.itch.io/butler
#   Windows: scoop install butler   OR download from https://itchio.itch.io/butler
#   Mac/Linux: brew install butler
#
# Run: ./push-itchio.sh [itchio-username]
# Default username: stuffthings15

set -e
USER=${1:-"stuffthings15"}
GAME="brickblast-velocity-market"
DIR="$(cd "$(dirname "$0")" && pwd)"

echo "=== Pushing BrickBlast: Velocity Market to itch.io as $USER/$GAME ==="

# HTML5 browser play
butler push "$DIR/index.html" "$USER/$GAME:html" --userversion 1.0.0

# Windows installer
butler push "$DIR/BrickBlast-Windows-Setup.exe" "$USER/$GAME:windows" --userversion 1.0.0

# Linux x64 (zip lives in linux-x64/ release folder)
butler push "$DIR/../linux-x64/BrickBlast-Linux-x64.zip" "$USER/$GAME:linux-x64" --userversion 1.0.0

# Linux arm64 (zip lives in linux-arm64/ release folder)
butler push "$DIR/../linux-arm64/BrickBlast-Linux-arm64.zip" "$USER/$GAME:linux-arm64" --userversion 1.0.0

# macOS — run this on a Mac after building the DMG
# butler push "path/to/BrickBlast-macOS.dmg" "$USER/$GAME:osx" --userversion 1.0.0

echo ""
echo "=== Done! Visit https://itch.io/dashboard to publish. ==="
echo "Channels: html | windows | linux-x64 | linux-arm64"
