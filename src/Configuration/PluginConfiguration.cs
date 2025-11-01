using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ExcludedLibraries.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        Sections = new List<SectionConfig>();
    }

    /// <summary>
    /// Gets or sets the list of configured sections.
    /// </summary>
    public List<SectionConfig> Sections { get; set; }
}

/// <summary>
/// Configuration for a single home section.
/// </summary>
public class SectionConfig
{
    /// <summary>
    /// Gets or sets the unique ID for this section.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the display name for the section on the home screen.
    /// </summary>
    public string DisplayName { get; set; } = "Filtered Content";

    /// <summary>
    /// Gets or sets the list of library names to exclude.
    /// </summary>
    public List<string> ExcludedLibraryNames { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to include movies.
    /// </summary>
    public bool IncludeMovies { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include TV series.
    /// </summary>
    public bool IncludeSeries { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include music.
    /// </summary>
    public bool IncludeMusic { get; set; } = false;

    /// <summary>
    /// Gets or sets the sort field (DateCreated, DatePlayed, Name, PremiereDate, etc).
    /// </summary>
    public string SortBy { get; set; } = "DateCreated";

    /// <summary>
    /// Gets or sets a value indicating whether to sort in descending order.
    /// </summary>
    public bool SortDescending { get; set; } = true;
}

