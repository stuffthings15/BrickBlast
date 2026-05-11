#!/usr/bin/env bash
# BUILD_IOS.sh — Brick Blast iPhone native build
# Team Fast Talk
#
# REQUIREMENTS: macOS 13+, Xcode 15+, Node.js 18+, CocoaPods
# SOURCE: Syncs canonical game from mobile/www/index.html (built from web/index.html)
#
# Usage: cd versions/iphone && bash BUILD_IOS.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
MOBILE_DIR="$PROJECT_ROOT/mobile"
CANONICAL_HTML="$PROJECT_ROOT/web/index.html"

echo "============================================"
echo "  BRICK BLAST — iPhone Native Build"
echo "  Team Fast Talk"
echo "============================================"
echo ""

# Validate tools
for tool in xcodebuild npm node pod; do
    if ! command -v "$tool" &>/dev/null; then
        echo "ERROR: '$tool' is not installed."
        case $tool in
            xcodebuild) echo "  Install Xcode from the Mac App Store." ;;
            npm|node)   echo "  Install Node.js from https://nodejs.org" ;;
            pod)        echo "  Run: sudo gem install cocoapods" ;;
        esac
        exit 1
    fi
done

# Step 1: Sync canonical game source into mobile/www
echo "Step 1 — Syncing canonical game source..."
if [ ! -f "$CANONICAL_HTML" ]; then
    echo "ERROR: Canonical source not found: $CANONICAL_HTML"
    exit 1
fi
mkdir -p "$MOBILE_DIR/www"
cp "$CANONICAL_HTML"                  "$MOBILE_DIR/www/index.html"
cp "$PROJECT_ROOT/web/manifest.json"  "$MOBILE_DIR/www/manifest.json"
cp -r "$PROJECT_ROOT/web/icons"       "$MOBILE_DIR/www/icons" 2>/dev/null || true
echo "  Copied: $CANONICAL_HTML -> mobile/www/index.html"
echo "  Size: $(wc -c < "$MOBILE_DIR/www/index.html") bytes"

# Step 2: Install npm packages and sync Capacitor
echo ""
echo "Step 2 — Installing Capacitor packages and syncing iOS project..."
cd "$MOBILE_DIR"
npm install
npx cap sync ios

# Step 3: Install CocoaPods
echo ""
echo "Step 3 — Installing CocoaPods dependencies..."
cd "$MOBILE_DIR/ios/App"
pod install --repo-update

# Step 4: Archive
echo ""
echo "Step 4 — Archiving for App Store submission..."
xcodebuild \
    -workspace "$MOBILE_DIR/ios/App/App.xcworkspace" \
    -scheme App \
    -configuration Release \
    -destination 'generic/platform=iOS' \
    -archivePath "$SCRIPT_DIR/build/BrickBlast-iPhone.xcarchive" \
    archive

echo ""
echo "============================================"
echo "  BUILD COMPLETE!"
echo "============================================"
echo ""
echo "Archive: $SCRIPT_DIR/build/BrickBlast-iPhone.xcarchive"
echo ""
echo "Next steps:"
echo "  1. Open the archive in Xcode Organizer"
echo "  2. Distribute App -> App Store Connect -> Upload"
echo ""
echo "See PUBLISHING.md for the full App Store submission checklist."
