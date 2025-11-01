using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ExcludedLibraries.Configuration;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.ExcludedLibraries.Services;

/// <summary>
/// Startup service that registers sections with Home Screen Sections plugin.
/// </summary>
public class StartupService : IScheduledTask
{
    private readonly ILogger<StartupService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupService"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    public StartupService(ILogger<StartupService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string Name => "Excluded Libraries - Register Home Sections";

    /// <inheritdoc/>
    public string Key => "ExcludedLibrariesRegisterHomeSections";

    /// <inheritdoc/>
    public string Description => "Registers Excluded Libraries sections with Home Screen Sections plugin on startup";

    /// <inheritdoc/>
    public string Category => "Library";

    /// <inheritdoc/>
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ExcludedLibraries] StartupService: Beginning section registration");
        
        // Wait a bit to ensure Home Screen Sections plugin is ready
        await Task.Delay(2000, cancellationToken);
        
        try
        {
            RegisterSections();
            _logger.LogInformation("[ExcludedLibraries] StartupService: Section registration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExcludedLibraries] StartupService: Error registering sections");
        }

        progress.Report(100);
    }

    /// <inheritdoc/>
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // Run on application startup
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerStartup
            }
        };
    }

    private void RegisterSections()
    {
        var config = Plugin.Instance?.Configuration;
        if (config?.Sections == null || config.Sections.Count == 0)
        {
            _logger.LogInformation("[ExcludedLibraries] No sections configured to register");
            return;
        }

        // Find the Home Screen Sections assembly using reflection
        Assembly? homeScreenSectionsAssembly = AssemblyLoadContext.All
            .SelectMany(x => x.Assemblies)
            .FirstOrDefault(x => x.FullName?.Contains(".HomeScreenSections") ?? false);

        if (homeScreenSectionsAssembly == null)
        {
            _logger.LogWarning("[ExcludedLibraries] Home Screen Sections plugin not found - sections will not be registered");
            return;
        }

        _logger.LogInformation("[ExcludedLibraries] Found Home Screen Sections assembly: {AssemblyName}", 
            homeScreenSectionsAssembly.FullName);

        // Get the PluginInterface type
        Type? pluginInterfaceType = homeScreenSectionsAssembly.GetType("Jellyfin.Plugin.HomeScreenSections.PluginInterface");
        if (pluginInterfaceType == null)
        {
            _logger.LogError("[ExcludedLibraries] Could not find PluginInterface type in Home Screen Sections");
            return;
        }

        // Get the RegisterSection method
        MethodInfo? registerMethod = pluginInterfaceType.GetMethod("RegisterSection");
        if (registerMethod == null)
        {
            _logger.LogError("[ExcludedLibraries] Could not find RegisterSection method");
            return;
        }

        // Register each configured section
        int registeredCount = 0;
        foreach (var section in config.Sections)
        {
            try
            {
                var payload = new JObject
                {
                    { "id", section.Id },
                    { "displayText", section.DisplayName },
                    { "limit", 1 },
                    { "route", "web/#/movies?topParentId=f137a2dd21bbc1b99aa5c0f6bf02a805&collectionType=movies" },
                    { "additionalData", section.Id },
                    { "resultsAssembly", GetType().Assembly.FullName },
                    { "resultsClass", typeof(HomeScreenSectionsHandler).FullName },
                    { "resultsMethod", nameof(HomeScreenSectionsHandler.GetResults) }
                };

                _logger.LogInformation("[ExcludedLibraries] Registering section: {SectionId} - {DisplayName}", 
                    section.Id, section.DisplayName);
                _logger.LogInformation("[ExcludedLibraries] Payload: {Payload}", payload.ToString());
                _logger.LogInformation("[ExcludedLibraries] Assembly: {Assembly}", GetType().Assembly.FullName);
                _logger.LogInformation("[ExcludedLibraries] Class: {Class}", typeof(HomeScreenSectionsHandler).FullName);
                _logger.LogInformation("[ExcludedLibraries] Method: {Method}", nameof(HomeScreenSectionsHandler.GetResults));

                registerMethod.Invoke(null, new object[] { payload });
                registeredCount++;

                _logger.LogInformation("[ExcludedLibraries] ✓ Successfully registered section: {DisplayName}", 
                    section.DisplayName);
                _logger.LogInformation("[ExcludedLibraries] NOTE: You must enable this section in Dashboard > Home Screen Sections > Modular Home settings to see it on the home screen");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ExcludedLibraries] ✗ Failed to register section: {SectionId}", section.Id);
            }
        }

        _logger.LogInformation("[ExcludedLibraries] Registered {Count} of {Total} sections", 
            registeredCount, config.Sections.Count);
    }
}

