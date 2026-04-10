#!/bin/bash
# ============================================
#   BRICK BLAST - iPhone Native Build
#   Team Fast Talk
# ============================================
# Run this script on a Mac with Xcode installed.

set -e
cd "$(dirname "$0")/xcode-project/App"

echo "============================================"
echo "  BRICK BLAST - iPhone Native Build"
echo "  Team Fast Talk"
echo "============================================"
echo ""

# Check for Xcode
if ! command -v xcodebuild &> /dev/null; then
    echo "ERROR: Xcode is not installed."
    echo "Install Xcode from the Mac App Store first."
    exit 1
fi

# Install CocoaPods if needed
if ! command -v pod &> /dev/null; then
    echo "Installing CocoaPods..."
    sudo gem install cocoapods
fi

# Install pods
echo "Installing dependencies..."
pod install 2>/dev/null || echo "Note: pod install had warnings (this is usually OK)"

# Build the app
echo ""
echo "Building for iPhone..."
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
echo "To install on your iPhone:"
echo "  1. Open BrickBlast.xcarchive in Xcode Organizer"
echo "  2. Click 'Distribute App' > 'Ad Hoc' or 'Development'"
echo "  3. Connect iPhone and install via Xcode"
echo ""
echo "Or open the project in Xcode:"
echo "  open xcode-project/App/App.xcworkspace"
echo ""
