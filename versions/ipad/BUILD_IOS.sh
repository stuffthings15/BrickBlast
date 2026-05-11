#!/bin/bash
# ============================================
#   BRICK BLAST - iPad Native Build
#   Team Fast Talk
# ============================================
# Run this script on a Mac with Xcode installed.

set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

echo "============================================"
echo "  BRICK BLAST - iPad Native Build"
echo "  Team Fast Talk"
echo "============================================"
echo ""

# Check for Xcode
if ! command -v xcodebuild &> /dev/null; then
    echo "ERROR: Xcode is not installed."
    echo "Install Xcode from the Mac App Store first."
    exit 1
fi

# Install Capacitor npm dependencies (Podfile depends on node_modules)
if ! command -v npm &> /dev/null; then
    echo "ERROR: Node.js / npm is not installed."
    echo "Install from https://nodejs.org and re-run this script."
    exit 1
fi

echo "Installing Capacitor npm packages..."
npm install

# Install CocoaPods if needed
if ! command -v pod &> /dev/null; then
    echo "Installing CocoaPods..."
    sudo gem install cocoapods
fi

# Install pods
echo "Installing CocoaPods dependencies..."
cd "$SCRIPT_DIR/xcode-project/App"
pod install 2>/dev/null || echo "Note: pod install had warnings (this is usually OK)"

# Build the app
echo ""
echo "Building for iPad..."
xcodebuild -workspace App.xcworkspace \
    -scheme App \
    -configuration Release \
    -destination 'generic/platform=iOS' \
    -archivePath ./build/BrickBlast.xcarchive \
    archive

echo ""
echo "============================================"
echo "  BUILD COMPLETE!"
echo "============================================"
echo ""
echo "Archive: xcode-project/App/build/BrickBlast.xcarchive"
echo ""
echo "To install on your iPad:"
echo "  1. Open BrickBlast.xcarchive in Xcode Organizer"
echo "  2. Click 'Distribute App' > 'Ad Hoc' or 'Development'"
echo "  3. Connect iPad and install via Xcode"
echo ""
echo "Or open the project in Xcode:"
echo "  open xcode-project/App/App.xcworkspace"
echo ""
