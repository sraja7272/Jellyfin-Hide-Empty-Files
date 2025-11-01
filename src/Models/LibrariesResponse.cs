using System.Collections.Generic;

namespace Jellyfin.Plugin.ExcludedLibraries.Models;

/// <summary>
/// Response model for libraries list.
/// </summary>
public class LibrariesResponse
{
    /// <summary>
    /// Gets or sets the list of library names.
    /// </summary>
    public List<string> Libraries { get; set; } = new();
}

