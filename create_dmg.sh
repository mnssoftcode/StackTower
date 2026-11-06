#!/bin/bash
# ==============================================================================
# Stack Tower — Universal macOS DMG Packager
# Supports both Intel (x86_64) and Apple Silicon (M1/M2/M3/M4)
# ==============================================================================

set -e

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_PATH="$PROJECT_DIR/Builds/macOS/StackTower.app"
DMG_PATH="$PROJECT_DIR/Builds/macOS/StackTower.dmg"
STAGING_DIR="$PROJECT_DIR/Builds/macOS/dmg_staging"

if [ ! -d "$APP_PATH" ]; then
    echo "❌ Error: $APP_PATH not found."
    echo "👉 Please build the macOS application first from Unity:"
    echo "   Option 1: In Unity top menu, click 'Stack Tower > Build macOS Universal App (.app)'"
    echo "   Option 2: Go to File > Build Settings > select macOS > Architecture: 'Intel 64-bit + Apple Silicon' > Build."
    exit 1
fi

echo "📦 Staging Universal macOS application..."
rm -rf "$STAGING_DIR"
mkdir -p "$STAGING_DIR"

# Copy the .app bundle
cp -R "$APP_PATH" "$STAGING_DIR/"

# Create symlink to /Applications for easy drag-and-drop installation
ln -s /Applications "$STAGING_DIR/Applications"

echo "💿 Creating compressed DMG image..."
rm -f "$DMG_PATH"
hdiutil create -volname "Stack Tower" -srcfolder "$STAGING_DIR" -ov -format UDZO "$DMG_PATH"

# Cleanup staging directory
rm -rf "$STAGING_DIR"

echo "=============================================================================="
echo "✅ SUCCESS! DMG created at: $DMG_PATH"
echo "=============================================================================="

# Reveal in Finder
if command -v open >/dev/null 2>&1; then
    open -R "$DMG_PATH"
fi

