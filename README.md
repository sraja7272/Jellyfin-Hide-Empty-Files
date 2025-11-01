# Jellyfin Plugin: Excluded Libraries Home Sections

A Jellyfin plugin that creates custom home sections displaying content from your libraries while excluding specific libraries you specify. Perfect for filtering out kids content, documentaries, or any other libraries from your main home screen.

## Features

- 🎬 **Flexible Content Filtering**: Exclude any libraries from your home sections
- 🎯 **Content Type Selection**: Choose to include movies, TV series, music albums, or any combination
- 🔄 **Multiple Sorting Options**: Sort by date added, date played, release date, name, or random
- ⚙️ **Easy Configuration**: Configure everything through the Jellyfin dashboard
- 🚀 **Auto-Registration**: Automatically registers with Home Screen Sections plugin on Jellyfin startup (v2.0+)
- 🏠 **Seamless Integration**: Works with the Home Screen Sections (Modular Home) plugin
- 🔍 **Comprehensive Logging**: All logs prefixed with `[ExcludedLibraries]` for easy debugging
- 📊 **Version Tracking**: Config page displays version number for cache-busting

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

### Step 1: Build the Plugin

```bash
cd src
dotnet clean -c Release
dotnet restore
dotnet build -c Release
```

The DLL will be in: `src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll`

### Step 2: Copy to Docker Container

```bash
# Copy DLL to your Jellyfin config volume
docker cp src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll \
  jellyfin:/config/plugins/ExcludedLibraries/

# Or if using docker-compose with volumes:
mkdir -p /path/to/jellyfin/config/plugins/ExcludedLibraries
cp src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll \
  /path/to/jellyfin/config/plugins/ExcludedLibraries/
```

### Step 3: Restart Jellyfin

```bash
docker restart jellyfin

# Or with docker-compose:
docker-compose restart jellyfin
```

### Step 4: Verify Installation

1. Go to **Dashboard** → **Plugins**
2. Look for **"Excluded Libraries Home Sections"**
3. If it appears, installation was successful! 🎉

## Configuration

### Step 1: Configure the Plugin

1. Go to **Dashboard** → **Plugins** → **Excluded Libraries Home Sections**
2. Configure your preferences:

   **Display Settings:**
   - **Section Display Name**: What to call this section (e.g., "Movies for Adults")
   - **Maximum Items**: How many items to show (1-100, recommended: 20-30)

   **Content Types:**
   - ☑ **Include Movies**: Show movies
   - ☑ **Include TV Series**: Show TV shows
   - ☐ **Include Music Albums**: Show music

   **Sorting:**
   - **Sort By**: Date Added, Date Played, Release Date, Name, or Random
   - **Sort Descending**: Check for newest first

   **Excluded Libraries:**
   - Check the libraries you want to **EXCLUDE** from this section

3. Click **Save**

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

## Usage Examples

### Example 1: Family-Friendly Main Screen

**Goal:** Show all movies except adult content

**Configuration:**
- Section Display Name: `"Movies for Adults"`
- Content Types: ✅ Movies
- Excluded Libraries: ✅ "Kids Movies"
- Sort: Date Added (Descending)

### Example 2: Entertainment Only

**Goal:** Show movies and TV but not documentaries

**Configuration:**
- Section Display Name: `"Entertainment"`
- Content Types: ✅ Movies, ✅ TV Series
- Excluded Libraries: ✅ "Documentaries"
- Sort: Date Added (Descending)

### Example 3: Recently Added (Filtered)

**Goal:** Show recent additions except kids content

**Configuration:**
- Section Display Name: `"New Arrivals"`
- Content Types: ✅ Movies, ✅ TV Series, ✅ Music
- Excluded Libraries: ✅ "Kids Movies", ✅ "Kids Shows", ✅ "Kids Music"
- Sort: Date Added (Descending)

### Example 4: Random Discovery

**Goal:** Random content excluding specific genres

**Configuration:**
- Section Display Name: `"Discover"`
- Content Types: ✅ Movies, ✅ TV Series
- Excluded Libraries: ✅ "Horror", ✅ "Reality TV"
- Sort: Random

## API Endpoints

The plugin exposes these REST endpoints:

### `GET /ExcludedLibraries/FilteredItems?userId={id}`
Returns filtered items based on plugin configuration.

**Response:** `QueryResult<BaseItemDto>`

### `GET /ExcludedLibraries/Libraries`
Returns list of all available libraries.

**Response:**
```json
{
  "Libraries": ["Movies", "TV Shows", "Kids", "Music"]
}
```

### `GET /ExcludedLibraries/Configuration`
Returns current plugin configuration.

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
The configuration page displays the current version (e.g., `v2.0.2`) in the top-right corner.

See [VERSION_AND_LOGGING.md](VERSION_AND_LOGGING.md) for detailed debugging guides.

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
2. You haven't excluded ALL your libraries
3. You have items in non-excluded libraries
4. Try unchecking all exclusions temporarily to test

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
| ExcludedLibraryNames | string[] | [] | Libraries to exclude |
| SectionDisplayName | string | "Filtered Content" | Name shown on home screen |
| MaxItems | int | 20 | Number of items to display |
| IncludeMovies | bool | true | Include movies |
| IncludeSeries | bool | true | Include TV series |
| IncludeMusic | bool | false | Include music albums |
| SortBy | string | "DateCreated" | Sort field |
| SortDescending | bool | true | Sort direction |

## How It Works

```
User Configures Plugin
        ↓
Clicks Save
        ↓
Configuration Saved
        ↓
Restarts Jellyfin
        ↓
Sections auto-register on startup
        ↓
User enables section in Modular Home
        ↓
Home Screen Sections calls plugin via reflection
        ↓
Plugin filters items by library
        ↓
Returns filtered results
        ↓
Displays on home screen
```

## Updating the Plugin

To update after making changes:

```bash
# Rebuild
cd src
dotnet build -c Release

# Copy to container
docker cp src/bin/Release/net8.0/Jellyfin.Plugin.ExcludedLibraries.dll \
  jellyfin:/config/plugins/ExcludedLibraries/

# Restart to register sections
docker restart jellyfin
```

**Note:** After creating or modifying sections, restart Jellyfin to register them with Home Screen Sections.

## Security

- All endpoints require authentication (`[Authorize]`)
- Uses Jellyfin's built-in user management
- Respects library access permissions
- No sensitive data exposed

## Performance

- Queries are limited to avoid server overload
- Efficient filtering using LINQ
- Proper DTO options minimize data transfer
- Logging for debugging without performance impact

## Contributing

Contributions welcome! Feel free to submit issues or pull requests.

## License

MIT License - see LICENSE file for details

## Credits

- Built for [Jellyfin](https://jellyfin.org/)
- Integrates with [Home Screen Sections](https://github.com/IAmParadox27/jellyfin-plugin-home-sections) by IAmParadox27

## Support

For issues:
1. Check this README's Troubleshooting section
2. Review Docker logs: `docker logs jellyfin`
3. Verify all prerequisite plugins are installed
4. Ensure Jellyfin version is 10.10.7 or later
