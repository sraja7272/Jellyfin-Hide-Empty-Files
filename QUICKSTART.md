# Quick Start Guide

Get your valid media home section running in 5 minutes!

This plugin shows only media with **valid, accessible files** - perfect for hiding broken symlinks and missing files.

## Prerequisites

Before starting, install these Jellyfin plugins:

```
Dashboard → Plugins → Repositories → Add:
https://www.iamparadox.dev/jellyfin/plugins/manifest.json

Then install from Catalog:
- Home Screen Sections
- File Transformation  
- Plugin Pages

Restart Jellyfin
```

## Installation

### Option A: Via Repository (Easiest!)

1. **Add Repository:**
   - Dashboard → Plugins → Repositories → **+**
   - Name: `Valid Media Sections`
   - URL: `https://sraja7272.github.io/Jellyfin-Hide-Empty-Files/manifest.json`
   - Save

2. **Install Plugin:**
   - Plugins → Catalog
   - Find "Excluded Libraries Home Sections"
   - Install

3. **Restart Jellyfin**

### Option B: Manual Build

1. **Build:**
   ```bash
   cd src
   dotnet build -c Release
   ```

2. **Copy to Docker:**
   ```bash
   docker cp src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll \
     jellyfin:/config/plugins/ExcludedLibraries/
   ```

3. **Restart:**
   ```bash
   docker restart jellyfin
   ```

## Setup

### 1. Configure

- Go to **Dashboard** → **Plugins** → **Excluded Libraries Home Sections**
- Enter section name (e.g., "Valid Content", "Watchable Media")
- Check content types you want (Movies, TV Series, Music)
- Choose sorting preference
- Click **Save**

✨ Plugin automatically filters out items with missing/broken files!
✨ Section automatically registers with Home Screen Sections plugin!

### 2. Enable

- Go to home page → hamburger menu (☰) → **Modular Home**
- Toggle your section **ON**
- Refresh page

Done! 🎉

## Common Setups

**Valid Movies:**
```
Name: "Watchable Movies"
Include: ✅ Movies
Sort: Date Added (Descending)
```

**All Valid Content:**
```
Name: "Valid Media"
Include: ✅ Movies, ✅ TV Series, ✅ Music
Sort: Date Added (Descending)
```

**Recently Added (Valid Only):**
```
Name: "New Arrivals - Working Files"  
Include: ✅ Movies, ✅ TV Series
Sort: Date Added (Descending)
```

**Random Valid Content:**
```
Name: "Discover"
Include: ✅ Movies, ✅ TV Series
Sort: Random
```

## Troubleshooting

**Plugin not showing?**
```bash
docker exec jellyfin ls -la /config/plugins/ExcludedLibraries/
docker restart jellyfin
```

**Section empty?**
- Check at least one content type is enabled
- Verify you have media files with valid sizes (plugin filters broken files)
- Check for broken symlinks or missing files

**Need more help?**
See [README.md](README.md) for detailed documentation.
