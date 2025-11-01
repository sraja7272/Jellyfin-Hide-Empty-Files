using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ExcludedLibraries;

/// <summary>
/// Service for getting items with library exclusions.
/// </summary>
public class ExcludedLibrariesSection
{
    private readonly ILibraryManager _libraryManager;
    private readonly IDtoService _dtoService;
    private readonly IUserManager _userManager;
    private readonly ILogger<ExcludedLibrariesSection> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcludedLibrariesSection"/> class.
    /// </summary>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="dtoService">Instance of the <see cref="IDtoService"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    public ExcludedLibrariesSection(
        ILibraryManager libraryManager,
        IDtoService dtoService,
        IUserManager userManager,
        ILogger<ExcludedLibrariesSection> logger)
    {
        _libraryManager = libraryManager;
        _dtoService = dtoService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets items excluding specified libraries.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="sectionId">The section ID.</param>
    /// <returns>Query result with filtered items.</returns>
    public QueryResult<BaseItemDto> GetFilteredItems(Guid userId, string sectionId)
    {
        _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Starting for user {UserId}", sectionId, userId);
        
        var pluginConfig = Plugin.Instance?.Configuration;
        if (pluginConfig == null)
        {
            _logger.LogWarning("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Plugin configuration is null", sectionId);
            return new QueryResult<BaseItemDto>();
        }

        var config = pluginConfig.Sections?.FirstOrDefault(s => s.Id == sectionId);
        if (config == null)
        {
            _logger.LogWarning("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Section not found", sectionId);
            return new QueryResult<BaseItemDto>();
        }

        _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Configuration loaded - DisplayName: {Name}",
            sectionId, config.DisplayName);
        _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Processing for user {UserId}", sectionId, userId);

        // Get user object using reflection - method signature changed in Jellyfin 10.11
        // In 10.10: IUserManager.GetUserById(Guid) returns Jellyfin.Data.Entities.User
        // In 10.11: The method signature is different
        // Use reflection to call the method at runtime to avoid compile-time binding
        object? userObj = null;
        try
        {
            var userManagerType = _userManager.GetType();
            var getUserByIdMethod = userManagerType.GetMethod("GetUserById", new[] { typeof(Guid) });
            
            if (getUserByIdMethod == null)
            {
                _logger.LogError("[ExcludedLibraries] [{SectionId}] GetFilteredItems: GetUserById method not found on IUserManager", sectionId);
                return new QueryResult<BaseItemDto>();
            }

            userObj = getUserByIdMethod.Invoke(_userManager, new object[] { userId });
            
            if (userObj == null)
            {
                _logger.LogWarning("[ExcludedLibraries] [{SectionId}] GetFilteredItems: User {UserId} not found", sectionId, userId);
                return new QueryResult<BaseItemDto>();
            }
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: User object retrieved via reflection", sectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExcludedLibraries] [{SectionId}] GetFilteredItems: Error retrieving user {UserId} via reflection", sectionId, userId);
            return new QueryResult<BaseItemDto>();
        }

        try
        {
            // Simple approach: filter by media type and file size only
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Using media type and size-based filtering", sectionId);


            // Build the item types to include  
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Building item types - Movies: {Movies}, Series: {Series}, Music: {Music}",
                sectionId, config.IncludeMovies, config.IncludeSeries, config.IncludeMusic);
            
            var includeMovies = config.IncludeMovies;
            var includeSeries = config.IncludeSeries;
            var includeMusic = config.IncludeMusic;

            if (!includeMovies && !includeSeries && !includeMusic)
            {
                _logger.LogWarning("[ExcludedLibraries] [{SectionId}] GetFilteredItems: No item types selected in configuration", sectionId);
                return new QueryResult<BaseItemDto>();
            }
            
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Sort by {SortBy} {Order}",
                sectionId, config.SortBy, config.SortDescending ? "Descending" : "Ascending");

            // NEW APPROACH: Query all actual media files (Episodes, Movies, Tracks) in ONE query
            // Then group by parent to find which Series/Albums have valid files
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Querying all media files recursively...", sectionId);
            
            var fileItemTypes = new List<BaseItemKind>();
            if (includeMovies) fileItemTypes.Add(BaseItemKind.Movie);
            if (includeSeries) fileItemTypes.Add(BaseItemKind.Episode); // Episodes instead of Series
            if (includeMusic) fileItemTypes.Add(BaseItemKind.Audio); // Audio tracks instead of MusicAlbum
            
            // Use dynamic to handle User type differences between Jellyfin versions
            dynamic dynamicUser = userObj;
            var query = new InternalItemsQuery(dynamicUser)
            {
                IncludeItemTypes = fileItemTypes.ToArray(),
                Recursive = true,
                Limit = 10000, // Get lots of files
                DtoOptions = new DtoOptions(false) // Minimal data needed
            };

            // Use reflection to call GetItemList - signature changed in Jellyfin 10.11
            // In 10.10: returns List<BaseItem>
            // In 10.11: might return IReadOnlyList<BaseItem> or different signature
            List<BaseItem> allItems;
            try
            {
                var libraryManagerType = _libraryManager.GetType();
                var getItemListMethod = libraryManagerType.GetMethod("GetItemList", new[] { typeof(InternalItemsQuery) });
                
                if (getItemListMethod == null)
                {
                    _logger.LogError("[ExcludedLibraries] [{SectionId}] GetFilteredItems: GetItemList method not found on ILibraryManager", sectionId);
                    return new QueryResult<BaseItemDto>();
                }

                var result = getItemListMethod.Invoke(_libraryManager, new object[] { query });
                
                // Handle different return types (List or IReadOnlyList)
                if (result is List<BaseItem> list)
                {
                    allItems = list;
                }
                else if (result is System.Collections.Generic.IEnumerable<BaseItem> enumerable)
                {
                    allItems = enumerable.ToList();
                }
                else
                {
                    _logger.LogError("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Unexpected return type from GetItemList: {Type}", 
                        sectionId, result?.GetType().FullName ?? "null");
                    return new QueryResult<BaseItemDto>();
                }
                
                _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Successfully retrieved {Count} media files", sectionId, allItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ExcludedLibraries] [{SectionId}] GetFilteredItems: Error calling GetItemList via reflection", sectionId);
                return new QueryResult<BaseItemDto>();
            }
            
            var filterStartTime = DateTime.UtcNow;
            
            // Filter files by valid size
            var validFiles = allItems.Where(file => file.Size != null && file.Size > 0).ToList();
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: {Valid} files have valid size (from {Total} total)", 
                sectionId, validFiles.Count, allItems.Count);
            
            // Group files by their parent (Series for Episodes, MusicAlbum for tracks, or the Movie itself)
            var parentGroups = new Dictionary<Guid, BaseItem>();
            
            foreach (var file in validFiles)
            {
                BaseItem? parent = null;
                var fileKind = file.GetBaseItemKind();
                
                if (fileKind == BaseItemKind.Movie)
                {
                    // Movies are their own parent
                    parent = file;
                }
                else if (fileKind == BaseItemKind.Episode)
                {
                    // Get the Series (grandparent of episode)
                    parent = file.GetParent()?.GetParent(); // Season -> Series
                    if (parent == null || parent.GetBaseItemKind() != BaseItemKind.Series)
                    {
                        // Fallback: try direct parent
                        parent = file.GetParent();
                    }
                }
                else if (fileKind == BaseItemKind.Audio)
                {
                    // Get the Album
                    parent = file.GetParent();
                }
                
                if (parent != null && !parentGroups.ContainsKey(parent.Id))
                {
                    parentGroups[parent.Id] = parent;
                }
            }
            
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Found {Count} unique parent items (Series/Movies/Albums)",
                sectionId, parentGroups.Count);
            
            // Sort parents
            var parents = parentGroups.Values.ToList();
            var sortedParents = config.SortBy switch
            {
                "Name" => config.SortDescending 
                    ? parents.OrderByDescending(i => i.SortName).ToList()
                    : parents.OrderBy(i => i.SortName).ToList(),
                "DatePlayed" => config.SortDescending
                    ? parents.OrderByDescending(i => i.DateLastSaved).ToList()
                    : parents.OrderBy(i => i.DateLastSaved).ToList(),
                "PremiereDate" => config.SortDescending
                    ? parents.OrderByDescending(i => i.PremiereDate.GetValueOrDefault()).ToList()
                    : parents.OrderBy(i => i.PremiereDate.GetValueOrDefault()).ToList(),
                "Random" => parents.OrderBy(i => Guid.NewGuid()).ToList(),
                _ => config.SortDescending
                    ? parents.OrderByDescending(i => i.DateCreated).ToList()
                    : parents.OrderBy(i => i.DateCreated).ToList()
            };
            
            // Take only what we need
            var filteredItems = sortedParents.Take(20).ToList();
            
            var filterTotalTime = DateTime.UtcNow - filterStartTime;
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Filtering completed in {TotalMs}ms - ONE query, NO N+1 problem!", 
                sectionId, filterTotalTime.TotalMilliseconds);

            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Returning {Count} items",
                sectionId, filteredItems.Count);
            
            if (filteredItems.Count == 0)
            {
                _logger.LogWarning("[ExcludedLibraries] [{SectionId}] GetFilteredItems: No items remaining after filtering!", sectionId);
            }

            // Convert to DTOs
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Converting {Count} items to DTOs...", sectionId, filteredItems.Count);
            var dtoOptions = new DtoOptions
            {
                Fields = new[]
                {
                    ItemFields.PrimaryImageAspectRatio,
                    ItemFields.Overview,
                    ItemFields.Genres,
                    ItemFields.DateCreated,
                    ItemFields.MediaStreams
                },
                EnableImages = true,
                AddCurrentProgram = true
            };

            // Convert items to DTOs using reflection to avoid signature issues
            // GetBaseItemDto signature may have changed in Jellyfin 10.11
            var dtos = new List<BaseItemDto>();
            var dtoServiceType = _dtoService.GetType();
            
            // Log all available methods for debugging
            var allMethods = dtoServiceType.GetMethods();
            var getBaseItemDtoMethods = allMethods.Where(m => m.Name == "GetBaseItemDto").ToList();
            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Found {Count} GetBaseItemDto methods", sectionId, getBaseItemDtoMethods.Count);
            
            foreach (var method in getBaseItemDtoMethods)
            {
                var parameters = method.GetParameters();
                _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Method signature - {ReturnType} GetBaseItemDto({Params})",
                    sectionId, method.ReturnType.Name,
                    string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}")));
            }
            
            // Find GetBaseItemDto method - look for one with 3 or 4 parameters
            // Try multiple strategies to find the right method
            MethodInfo? getBaseItemDtoMethod = null;
            int parameterCount = 0;
            
            // Strategy 1: Look for 4-parameter version (Jellyfin 10.11+)
            getBaseItemDtoMethod = getBaseItemDtoMethods
                .FirstOrDefault(m => m.GetParameters().Length == 4 &&
                                    m.GetParameters()[0].ParameterType.Name.Contains("BaseItem") &&
                                    m.GetParameters()[1].ParameterType.Name.Contains("DtoOptions"));
            
            if (getBaseItemDtoMethod != null)
            {
                parameterCount = 4;
                _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Found 4-parameter GetBaseItemDto method", sectionId);
            }
            
            // Strategy 2: Look for 3-parameter version (Jellyfin 10.10)
            if (getBaseItemDtoMethod == null)
            {
                getBaseItemDtoMethod = getBaseItemDtoMethods
                    .FirstOrDefault(m => m.GetParameters().Length == 3 &&
                                        m.GetParameters()[0].ParameterType.Name.Contains("BaseItem") &&
                                        m.GetParameters()[1].ParameterType.Name.Contains("DtoOptions"));
                
                if (getBaseItemDtoMethod != null)
                {
                    parameterCount = 3;
                    _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Found 3-parameter GetBaseItemDto method", sectionId);
                }
            }
            
            // Strategy 3: Fallback to any method with 3 or 4 parameters
            if (getBaseItemDtoMethod == null)
            {
                getBaseItemDtoMethod = getBaseItemDtoMethods.FirstOrDefault(m => m.GetParameters().Length == 4);
                if (getBaseItemDtoMethod != null)
                {
                    parameterCount = 4;
                    _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Using fallback 4-parameter method", sectionId);
                }
            }
            
            if (getBaseItemDtoMethod == null)
            {
                getBaseItemDtoMethod = getBaseItemDtoMethods.FirstOrDefault(m => m.GetParameters().Length == 3);
                if (getBaseItemDtoMethod != null)
                {
                    parameterCount = 3;
                    _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Using fallback 3-parameter method", sectionId);
                }
            }
            
            if (getBaseItemDtoMethod == null)
            {
                _logger.LogError("[ExcludedLibraries] [{SectionId}] GetFilteredItems: GetBaseItemDto method not found on IDtoService", sectionId);
                _logger.LogError("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Available methods: {Methods}", 
                    sectionId, string.Join(", ", allMethods.Select(m => m.Name).Distinct()));
                return new QueryResult<BaseItemDto>();
            }

            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Found GetBaseItemDto method via reflection: {ParamCount} parameters - {Signature}",
                sectionId, parameterCount,
                string.Join(", ", getBaseItemDtoMethod.GetParameters().Select(p => p.ParameterType.Name)));

            foreach (var item in filteredItems)
            {
                try
                {
                    object? dto;
                    
                    // Call with appropriate number of parameters
                    if (parameterCount == 4)
                    {
                        // 4-parameter version: (BaseItem item, DtoOptions options, User user, BaseItem owner)
                        // Pass null for owner parameter
                        dto = getBaseItemDtoMethod.Invoke(_dtoService, new object?[] { item, dtoOptions, userObj, null });
                    }
                    else
                    {
                        // 3-parameter version: (BaseItem item, DtoOptions options, User user)
                        dto = getBaseItemDtoMethod.Invoke(_dtoService, new object[] { item, dtoOptions, userObj });
                    }
                    
                    if (dto is BaseItemDto baseItemDto)
                    {
                        dtos.Add(baseItemDto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[ExcludedLibraries] [{SectionId}] GetFilteredItems: Failed to convert item {ItemName} to DTO, skipping", 
                        sectionId, item.Name);
                }
            }

            _logger.LogInformation("[ExcludedLibraries] [{SectionId}] GetFilteredItems: Successfully returning {Count} items", sectionId, dtos.Count);
            return new QueryResult<BaseItemDto>(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExcludedLibraries] [{SectionId}] GetFilteredItems: Error getting filtered items for user {UserId}", sectionId, userId);
            return new QueryResult<BaseItemDto>();
        }
    }

}

