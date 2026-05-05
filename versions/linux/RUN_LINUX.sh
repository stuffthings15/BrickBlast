#!/usr/bin/env bash
# RUN_LINUX.sh — BrickBlast: Velocity Market launcher for Linux
# Usage: chmod +x RUN_LINUX.sh && ./RUN_LINUX.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_FILE="$SCRIPT_DIR/index.html"

if [ ! -f "$GAME_FILE" ]; then
    echo "ERROR: index.html not found in $SCRIPT_DIR"
    exit 1
fi

echo "🎮 Launching BrickBlast: Velocity Market..."

# Try browsers in order of preference
launch_browser() {
    local browsers=(xdg-open chromium-browser chromium google-chrome firefox firefox-esr brave-browser epiphany)
    for b in "${browsers[@]}"; do
        if command -v "$b" &>/dev/null; then
            echo "Opening with: $b"
            "$b" "$GAME_FILE" &
            return 0
        fi
    done
    return 1
}

if ! launch_browser; then
    echo ""
    echo "No browser found. Please open the file manually:"
    echo "  $GAME_FILE"
    echo ""
    echo "Install a browser:"
    echo "  Ubuntu/Debian: sudo apt install firefox"
    echo "  Fedora:        sudo dnf install firefox"
    echo "  Arch:          sudo pacman -S firefox"
    exit 1
fi

echo "BrickBlast is running in your browser."
