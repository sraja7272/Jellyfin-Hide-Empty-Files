#!/bin/bash
# Automated release script for Jellyfin Plugin

set -e

# Configuration
REPO="sraja7272/Jellyfin-Hide-Empty-Files"
DLL_PATH="src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll"
MANIFEST_FILE="manifest.json"

echo "=== Jellyfin Plugin Release Automation ==="
echo ""

# Get the latest tag and increment minor version
echo "Fetching latest tag..."
LATEST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "v1.0.0")
echo "Latest tag: $LATEST_TAG"

# Parse version numbers
if [[ $LATEST_TAG =~ ^v([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
    MAJOR="${BASH_REMATCH[1]}"
    MINOR="${BASH_REMATCH[2]}"
    PATCH="${BASH_REMATCH[3]}"
    
    # Increment minor version
    MINOR=$((MINOR + 1))
    PATCH=0
else
    echo "Warning: Could not parse tag '$LATEST_TAG', starting from v1.0.0"
    MAJOR=1
    MINOR=0
    PATCH=0
fi

VERSION="${MAJOR}.${MINOR}.${PATCH}"
TAG="v${VERSION}"

echo "New version: $TAG"
echo ""

# Check if gh CLI is installed
if ! command -v gh &> /dev/null; then
    echo "ERROR: GitHub CLI (gh) is not installed"
    echo "Install it with: brew install gh"
    echo "Then authenticate with: gh auth login"
    exit 1
fi

# Check if authenticated
if ! gh auth status &> /dev/null; then
    echo "ERROR: Not authenticated with GitHub"
    echo "Run: gh auth login"
    exit 1
fi

# Build the plugin
echo "Building plugin..."
cd src
dotnet clean -c Release > /dev/null 2>&1
dotnet build -c Release --no-restore
cd ..

if [ ! -f "$DLL_PATH" ]; then
    echo "ERROR: DLL not found at $DLL_PATH"
    exit 1
fi

echo "✓ Build successful!"
echo ""

# Calculate DLL checksum (for reference only)
echo "Calculating DLL checksum..."
if command -v md5sum &> /dev/null; then
    CHECKSUM=$(md5sum "$DLL_PATH" | awk '{print $1}')
elif command -v md5 &> /dev/null; then
    CHECKSUM=$(md5 -q "$DLL_PATH")
else
    echo "ERROR: Neither md5sum nor md5 found"
    exit 1
fi

echo "DLL MD5: $CHECKSUM"
echo ""

# Create ZIP package
echo "Creating ZIP package..."
ZIP_NAME="jellyfin-plugin-excludedlibraries_${VERSION}.zip"
ZIP_PATH="$ZIP_NAME"

# Use -j to junk paths (don't include directory structure)
zip -j -q "$ZIP_PATH" "$DLL_PATH"

if [ ! -f "$ZIP_PATH" ]; then
    echo "ERROR: ZIP file not found at $ZIP_PATH"
    exit 1
fi

echo "✓ ZIP package created: $ZIP_PATH"
echo ""

# Calculate ZIP checksum (MD5 as required by Jellyfin)
echo "Calculating ZIP MD5 checksum..."
if command -v md5sum &> /dev/null; then
    ZIP_CHECKSUM=$(md5sum "$ZIP_PATH" | awk '{print $1}')
elif command -v md5 &> /dev/null; then
    ZIP_CHECKSUM=$(md5 -q "$ZIP_PATH")
else
    echo "ERROR: Neither md5sum nor md5 found"
    exit 1
fi

echo "ZIP MD5: $ZIP_CHECKSUM"
echo ""

# Prompt for release notes
echo "Enter release notes (press Ctrl+D when done):"
echo "--------------------------------------------"
RELEASE_NOTES=$(cat)
echo ""

# If no release notes provided, use a default message
if [ -z "$RELEASE_NOTES" ]; then
    RELEASE_NOTES="Release $TAG"
fi

# Create GitHub release
echo "Creating GitHub release $TAG..."
if gh release view "$TAG" &> /dev/null; then
    echo "Release $TAG already exists. Deleting and recreating..."
    gh release delete "$TAG" -y
fi

gh release create "$TAG" "$ZIP_PATH" \
    --title "$TAG" \
    --notes "$RELEASE_NOTES"

echo "✓ GitHub release created!"
echo ""

# Clean up ZIP file
rm -f "$ZIP_PATH"

# Update version numbers in all files
echo "Updating version numbers in project files..."

if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    # Update manifest.json - version and checksum
    sed -i '' "s/\"version\": \"[^\"]*\"/\"version\": \"${VERSION}.0\"/" "$MANIFEST_FILE"
    sed -i '' "s/\"checksum\": \"[^\"]*\"/\"checksum\": \"$ZIP_CHECKSUM\"/" "$MANIFEST_FILE"
    sed -i '' "s|releases/download/v[0-9.]*/[^\"]*|releases/download/$TAG/jellyfin-plugin-excludedlibraries_${VERSION}.zip|" "$MANIFEST_FILE"
    
    # Update build.yaml
    sed -i '' "s/^version: .*/version: \"${VERSION}.0\"/" build.yaml
    
    # Update .csproj file
    sed -i '' "s/<Version>[^<]*<\/Version>/<Version>${VERSION}.0<\/Version>/" src/Jellyfin.Plugin.ExcludedLibraries.csproj
    sed -i '' "s/<AssemblyVersion>[^<]*<\/AssemblyVersion>/<AssemblyVersion>${VERSION}.0<\/AssemblyVersion>/" src/Jellyfin.Plugin.ExcludedLibraries.csproj
    sed -i '' "s/<FileVersion>[^<]*<\/FileVersion>/<FileVersion>${VERSION}.0<\/FileVersion>/" src/Jellyfin.Plugin.ExcludedLibraries.csproj
    
    # Update Plugin.cs
    sed -i '' "s/Version [0-9.]*/Version ${VERSION}/" src/Plugin.cs
    
    # Update configPage.html
    sed -i '' "s/v[0-9.]*<\/span>/v${VERSION}<\/span>/" src/Configuration/configPage.html
    sed -i '' "s/version: '[0-9.]*'/version: '${VERSION}'/" src/Configuration/configPage.html
else
    # Linux
    sed -i "s/\"version\": \"[^\"]*\"/\"version\": \"${VERSION}.0\"/" "$MANIFEST_FILE"
    sed -i "s/\"checksum\": \"[^\"]*\"/\"checksum\": \"$ZIP_CHECKSUM\"/" "$MANIFEST_FILE"
    sed -i "s|releases/download/v[0-9.]*/[^\"]*|releases/download/$TAG/jellyfin-plugin-excludedlibraries_${VERSION}.zip|" "$MANIFEST_FILE"
    
    sed -i "s/^version: .*/version: \"${VERSION}.0\"/" build.yaml
    
    sed -i "s/<Version>[^<]*<\/Version>/<Version>${VERSION}.0<\/Version>/" src/Jellyfin.Plugin.ExcludedLibraries.csproj
    sed -i "s/<AssemblyVersion>[^<]*<\/AssemblyVersion>/<AssemblyVersion>${VERSION}.0<\/AssemblyVersion>/" src/Jellyfin.Plugin.ExcludedLibraries.csproj
    sed -i "s/<FileVersion>[^<]*<\/FileVersion>/<FileVersion>${VERSION}.0<\/FileVersion>/" src/Jellyfin.Plugin.ExcludedLibraries.csproj
    
    sed -i "s/Version [0-9.]*/Version ${VERSION}/" src/Plugin.cs
    
    sed -i "s/v[0-9.]*<\/span>/v${VERSION}<\/span>/" src/Configuration/configPage.html
    sed -i "s/version: '[0-9.]*'/version: '${VERSION}'/" src/Configuration/configPage.html
fi

echo "✓ Version numbers updated in all files!"
echo ""

# Commit and push all version changes
echo "Committing and pushing version updates..."
git add "$MANIFEST_FILE" build.yaml src/Jellyfin.Plugin.ExcludedLibraries.csproj src/Plugin.cs src/Configuration/configPage.html
git commit -m "Release $TAG"
git push origin main

echo ""
echo "=== Release Complete! ==="
echo ""
echo "Repository URL: https://${REPO%/*}.github.io/${REPO#*/}/manifest.json"
echo "Release page: https://github.com/$REPO/releases/tag/$TAG"
echo ""
echo "Users can now install the plugin from the repository!"
echo ""

