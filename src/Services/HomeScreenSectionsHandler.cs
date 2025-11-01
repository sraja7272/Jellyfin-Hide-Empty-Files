using System;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ExcludedLibraries.Services;

/// <summary>
/// Handler for Home Screen Sections integration.
/// This class is invoked via reflection by the Home Screen Sections plugin.
/// </summary>
public class HomeScreenSectionsHandler
{
    private readonly ExcludedLibrariesSection _section;
    private readonly ILogger<HomeScreenSectionsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeScreenSectionsHandler"/> class.
    /// </summary>
    /// <param name="section">Instance of the <see cref="ExcludedLibrariesSection"/> class.</param>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    public HomeScreenSectionsHandler(
        ExcludedLibrariesSection section,
        ILogger<HomeScreenSectionsHandler> logger)
    {
        _section = section;
        _logger = logger;
        _logger.LogInformation("[ExcludedLibraries] HomeScreenSectionsHandler instance created via DI");
    }

    /// <summary>
    /// Gets filtered results for a specific section.
    /// This method is called via reflection by Home Screen Sections plugin.
    /// The payload parameter will be deserialized to match HomeScreenSectionPayload structure.
    /// </summary>
    /// <param name="payload">The section payload containing UserId and AdditionalData (sectionId).</param>
    /// <returns>Query result with filtered items.</returns>
    public QueryResult<BaseItemDto> GetResults(HomeScreenSectionPayload payload)
    {
        _logger.LogInformation(
            "[ExcludedLibraries] ========== GetResults CALLED ==========");
        _logger.LogInformation(
            "[ExcludedLibraries] User ID: {UserId}",
            payload.UserId);
        _logger.LogInformation(
            "[ExcludedLibraries] Section ID (AdditionalData): {SectionId}",
            payload.AdditionalData);

        try
        {
            // Validate payload
            if (payload.UserId == Guid.Empty)
            {
                _logger.LogWarning("[ExcludedLibraries] Invalid UserId (empty GUID)");
                return new QueryResult<BaseItemDto>();
            }

            if (string.IsNullOrEmpty(payload.AdditionalData))
            {
                _logger.LogWarning("[ExcludedLibraries] Invalid AdditionalData (null or empty)");
                return new QueryResult<BaseItemDto>();
            }

            _logger.LogInformation("[ExcludedLibraries] Calling GetFilteredItems...");
            
            // AdditionalData contains the section ID
            var result = _section.GetFilteredItems(payload.UserId, payload.AdditionalData);
            
            _logger.LogInformation(
                "[ExcludedLibraries] ✓ Successfully retrieved {Count} items for section {SectionId}",
                result.TotalRecordCount,
                payload.AdditionalData);
            _logger.LogInformation(
                "[ExcludedLibraries] ========================================");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[ExcludedLibraries] ✗ Error getting results for user {UserId}, section {SectionId}",
                payload.UserId,
                payload.AdditionalData);
            _logger.LogError(
                "[ExcludedLibraries] Exception type: {ExceptionType}",
                ex.GetType().Name);
            _logger.LogError(
                "[ExcludedLibraries] ========================================");
            
            return new QueryResult<BaseItemDto>();
        }
    }
}

/// <summary>
/// Payload structure that matches Home Screen Sections plugin's HomeScreenSectionPayload.
/// This is used for deserialization when the method is invoked via reflection.
/// </summary>
public class HomeScreenSectionPayload
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets additional data (in our case, the section ID).
    /// </summary>
    public string? AdditionalData { get; set; }
}

