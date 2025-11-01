# Quick Start Guide

Get your custom filtered home section running in 5 minutes!

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

### 1. Build

```bash
cd src
dotnet build -c Release
```

### 2. Copy to Docker

```bash
docker cp src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll \
  jellyfin:/config/plugins/ExcludedLibraries/
```

### 3. Restart

```bash
docker restart jellyfin
```

## Setup

### 1. Configure

- Go to **Dashboard** → **Plugins** → **Excluded Libraries Home Sections**
- Enter section name (e.g., "Filtered Content")
- Check content types you want (Movies, TV Series, Music)
- **Check libraries to EXCLUDE** (e.g., check "Kids Movies" to exclude it)
- Choose sorting preference
- Click **Save**

✨ Section automatically registers with Home Screen Sections plugin!

### 2. Enable

- Go to home page → hamburger menu (☰) → **Modular Home**
- Toggle your section **ON**
- Refresh page

Done! 🎉

## Common Setups

**No Kids Content:**
```
Name: "Movies for Adults"
Include: ✅ Movies
Exclude: ✅ "Kids Movies"
```

**Entertainment Only:**
```
Name: "Entertainment"
Include: ✅ Movies, ✅ TV Series
Exclude: ✅ "Documentaries"
```

**Recently Added (Filtered):**
```
Name: "New Arrivals"  
Include: ✅ Movies, ✅ TV Series
Sort: Date Added (Descending)
Exclude: ✅ "Kids Content", ✅ "Educational"
```

## Troubleshooting

**Plugin not showing?**
```bash
docker exec jellyfin ls -la /config/plugins/ExcludedLibraries/
docker restart jellyfin
```

**Section empty?**
- Check at least one content type is enabled
- Verify you haven't excluded all libraries

**Need more help?**
See [README.md](README.md) for detailed documentation.
