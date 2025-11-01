using Jellyfin.Plugin.ExcludedLibraries.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ExcludedLibraries;

/// <summary>
/// Plugin service registrator for dependency injection.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        System.Console.WriteLine("[ExcludedLibraries] Registering services");
        
        // Core service for filtering items
        serviceCollection.AddSingleton<ExcludedLibrariesSection>();
        
        // Handler for Home Screen Sections integration
        serviceCollection.AddSingleton<HomeScreenSectionsHandler>();
        
        // Scheduled task for registering sections on startup
        serviceCollection.AddSingleton<IScheduledTask, StartupService>();
    }
}

