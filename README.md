# Jellyfin Plugin: Valid Media Home Sections

A Jellyfin plugin that creates custom home sections displaying **only media with valid, accessible files**. This plugin filters out broken symlinks, missing files, and items with corrupt metadata - ensuring your home screen only shows content you can actually watch/listen to.

## What This Plugin Does

This plugin creates home sections that display **media with verified file access**:

- ✅ Shows only items with **valid file sizes** (filters out broken/missing files)
- 🎯 **Content Type Selection**: Choose to include movies, TV series, music albums, or any combination
- 🔄 **Multiple Sorting Options**: Sort by date added, date played, release date, name, or random
- ⚙️ **Easy Configuration**: Configure everything through the Jellyfin dashboard
- 🚀 **Auto-Registration**: Automatically registers with Home Screen Sections plugin on Jellyfin startup
- 🏠 **Seamless Integration**: Works with the Home Screen Sections (Modular Home) plugin
- 🔍 **Comprehensive Logging**: All logs prefixed with `[ExcludedLibraries]` for easy debugging

## Why Use This Plugin?

Perfect for users who:
- Have broken symlinks or missing files in their libraries
- Want to see only watchable/playable content on the home screen
- Need to filter out items with corrupt metadata
- Share libraries with others and want a clean experience

## Prerequisites

1. **Jellyfin 10.10.7 or later** running in Docker
2. **.NET 8.0 SDK** (for building the plugin)
3. **Required Plugins** (install these first):
   - Home Screen Sections (Modular Home)
   - File Transformation
   - Plugin Pages

### Installing Required Plugins

1. In Jellyfin, go to **Dashboard** → **Plugins** → **Repositories**
2. Click **+** and add: `https://www.iamparadox.dev/jellyfin/plugins/manifest.json`
3. Go to **Plugins** → **Catalog**
4. Install: **Home Screen Sections**, **File Transformation**, **Plugin Pages**
5. Restart Jellyfin

## Installation

### Method 1: Install from Repository (Recommended)

The easiest way to install and receive automatic updates:

1. **Add the plugin repository:**
   - Go to **Dashboard** → **Plugins** → **Repositories**
   - Click **+** (Add Repository)
   - Enter:
     - **Repository Name**: `Valid Media Sections`
     - **Repository URL**: `https://sraja7272.github.io/Jellyfin-Hide-Empty-Files/manifest.json`
   - Click **Save**

2. **Install the plugin:**
   - Go to **Plugins** → **Catalog**
   - Find **"Excluded Libraries Home Sections"**
   - Click **Install**
   - Restart Jellyfin

3. **Done!** The plugin will automatically update when new versions are released.

### Method 2: Manual Installation (For Development)

If you want to build from source or test modifications:

#### Step 1: Build the Plugin

```bash
cd src
dotnet clean -c Release
dotnet restore
dotnet build -c Release
```

The DLL will be in: `src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll`

#### Step 2: Copy to Docker Container

```bash
# Copy DLL to your Jellyfin config volume
docker cp src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll \
  jellyfin:/config/plugins/ExcludedLibraries/

# Or if using docker-compose with volumes:
mkdir -p /path/to/jellyfin/config/plugins/ExcludedLibraries
cp src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll \
  /path/to/jellyfin/config/plugins/ExcludedLibraries/
```

#### Step 3: Restart Jellyfin

```bash
docker restart jellyfin

# Or with docker-compose:
docker-compose restart jellyfin
```

#### Step 4: Verify Installation

1. Go to **Dashboard** → **Plugins**
2. Look for **"Excluded Libraries Home Sections"**
3. If it appears, installation was successful! 🎉

## Configuration

### Step 1: Configure the Plugin

1. Go to **Dashboard** → **Plugins** → **Excluded Libraries Home Sections**
2. Configure your preferences:

   **Display Settings:**
   - **Section Display Name**: What to call this section (e.g., "Valid Content", "Watchable Media")

   **Content Types:**
   - ☑ **Include Movies**: Show movies with valid files
   - ☑ **Include TV Series**: Show TV series with valid episodes
   - ☐ **Include Music Albums**: Show music albums with valid tracks

   **Sorting:**
   - **Sort By**: Date Added, Date Played, Release Date, Name, or Random
   - **Sort Descending**: Check for newest first

3. Click **Save**

**Note:** The plugin automatically filters out items with missing or corrupt files - you don't need to configure anything else!

### Step 2: Restart Jellyfin

After saving your section configuration, **restart Jellyfin** to register the section:

```bash
docker restart jellyfin
# Or run the scheduled task: "Excluded Libraries - Register Home Sections"
```

The plugin automatically registers sections with Home Screen Sections on startup.

### Step 3: Enable the Section

1. Go to your Jellyfin home page
2. Click the **hamburger menu** (☰)
3. Click **Modular Home**
4. Toggle your section (e.g., "Movies for Adults") **ON**
5. Refresh the page (F5)

Your custom section should now appear! 🎉

## Logging and Debugging

All plugin operations are logged with the **`[ExcludedLibraries]`** prefix for easy filtering.

### View Logs in Docker
```bash
# Show all plugin logs
docker logs jellyfin 2>&1 | grep "\[ExcludedLibraries\]"

# Follow in real-time
docker logs -f jellyfin 2>&1 | grep "\[ExcludedLibraries\]"

# Show only errors
docker logs jellyfin 2>&1 | grep "\[ExcludedLibraries\].*Error"
```

### View Logs in Browser Console
Open Developer Tools (F12) → Console tab. All JavaScript operations log with `[ExcludedLibraries]` prefix.

### Check Version
The configuration page displays the current version in the top-right corner.

## Troubleshooting

### Plugin Doesn't Appear After Installation

**Check:**
1. Verify DLL is in the correct location inside container:
   ```bash
   docker exec jellyfin ls -la /config/plugins/ExcludedLibraries/
   ```
2. Check Jellyfin logs:
   ```bash
   docker logs jellyfin | tail -100
   ```
3. Ensure .NET 8.0 compatible build
4. Restart container again

### Section Not Appearing on Home Page

**Solutions:**
1. Verify Home Screen Sections plugin is installed and enabled
2. Check you enabled the section in Modular Home settings
3. Hard refresh the page (Ctrl+F5)
4. Test the API endpoint directly:
   ```
   http://your-jellyfin-url:8096/ExcludedLibraries/FilteredItems?userId=YOUR_USER_ID
   ```

### Section is Empty

**Check:**
1. At least one content type is enabled in configuration
2. Verify you have media files with valid file sizes (the plugin filters out items with broken/missing files)
3. Check your media files aren't broken symlinks
4. Check Jellyfin logs for file access errors

### Section Not Appearing in Modular Home

**If your section doesn't show up:**
1. Verify you restarted Jellyfin after saving the section
2. Check if Home Screen Sections plugin is installed
3. Check Jellyfin logs for `[ExcludedLibraries]` messages
4. Try running the scheduled task: "Excluded Libraries - Register Home Sections"
5. Verify the section is enabled in Modular Home settings

### Build Errors

**Solutions:**
1. Ensure .NET 8.0 SDK is installed: `dotnet --version`
2. Clean and rebuild:
   ```bash
   dotnet clean
   rm -rf bin/ obj/
   dotnet restore
   dotnet build -c Release
   ```

## Configuration Options

All configurable through the web UI:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| SectionDisplayName | string | "Filtered Content" | Name shown on home screen |
| IncludeMovies | bool | true | Include movies with valid files |
| IncludeSeries | bool | true | Include TV series with valid episodes |
| IncludeMusic | bool | false | Include music albums with valid tracks |
| SortBy | string | "DateCreated" | Sort field (DateCreated, DatePlayed, PremiereDate, Name, Random) |
| SortDescending | bool | true | Sort direction |

## How It Works

```
User Configures Plugin
        ↓
Clicks Save → Restarts Jellyfin
        ↓
Sections auto-register on startup
        ↓
User enables section in Modular Home
        ↓
Home Screen Sections calls plugin
        ↓
Plugin queries all media files of selected types
        ↓
Filters out items with invalid/missing files (Size = null or 0)
        ↓
Groups episodes by series, tracks by albums
        ↓
Sorts and returns valid content
        ↓
Displays on home screen
```

## Technical Details

The plugin uses an efficient filtering approach:

1. **Single Query**: Fetches all media files (Movies, Episodes, Audio) in one database query
2. **File Validation**: Filters items where `Size != null && Size > 0` to ensure files exist and are accessible
3. **Smart Grouping**: 
   - Movies are shown as-is
   - Episodes are grouped by their parent Series
   - Audio tracks are grouped by their parent Albums
4. **No N+1 Problem**: Uses a single query then groups in memory for optimal performance

This ensures you only see content that actually works!

## Credits

- Built for [Jellyfin](https://jellyfin.org/)
- Integrates with [Home Screen Sections](https://github.com/IAmParadox27/jellyfin-plugin-home-sections) by IAmParadox27

## Support

For issues:
1. Check this README's Troubleshooting section
2. Review Docker logs: `docker logs jellyfin`
3. Verify all prerequisite plugins are installed
4. Ensure Jellyfin version is 10.10.7 or later
