#!/usr/bin/env bash
# RUN_LINUX.sh — Brick Blast native launcher for Linux

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BINARY="$SCRIPT_DIR/bin/anime finder macos"

if [ ! -f "$BINARY" ]; then
    echo "ERROR: Native binary not found at: $BINARY"
    exit 1
fi

chmod +x "$BINARY"
echo "Launching Brick Blast..."
cd "$SCRIPT_DIR"
exec "$BINARY"
