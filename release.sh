#!/bin/bash
# Helper script to prepare a release

set -e

echo "=== Jellyfin Plugin Release Helper ==="
echo ""

# Build the plugin
echo "Building plugin..."
cd src
dotnet clean -c Release
dotnet build -c Release --no-restore
cd ..

DLL_PATH="src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll"

if [ ! -f "$DLL_PATH" ]; then
    echo "ERROR: DLL not found at $DLL_PATH"
    exit 1
fi

echo ""
echo "✓ Build successful!"
echo ""

# Calculate checksum
echo "Calculating SHA256 checksum..."
if command -v sha256sum &> /dev/null; then
    CHECKSUM=$(sha256sum "$DLL_PATH" | awk '{print $1}')
elif command -v shasum &> /dev/null; then
    CHECKSUM=$(shasum -a 256 "$DLL_PATH" | awk '{print $1}')
else
    echo "ERROR: Neither sha256sum nor shasum found"
    exit 1
fi

echo ""
echo "=== Release Information ==="
echo ""
echo "DLL Location: $DLL_PATH"
echo "SHA256: $CHECKSUM"
echo ""
echo "Next steps:"
echo "1. Create a GitHub release with tag v1.0.0"
echo "2. Upload the DLL file from: $DLL_PATH"
echo "3. Update manifest.json with this checksum:"
echo "   \"checksum\": \"$CHECKSUM\""
echo "4. Update sourceUrl in manifest.json to point to the release"
echo "5. Commit and push manifest.json to enable repository installation"
echo ""

