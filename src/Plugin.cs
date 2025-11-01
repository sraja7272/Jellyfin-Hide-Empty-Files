using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Jellyfin.Plugin.ExcludedLibraries.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.ExcludedLibraries;

/// <summary>
/// The main plugin class for Excluded Libraries.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
                System.Console.WriteLine("[ExcludedLibraries] Plugin initialized - Version 1.1.0");
    }

    /// <summary>
    /// Module initializer to handle assembly resolution.
    /// </summary>
    [ModuleInitializer]
    public static void Init()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            return AssemblyLoadContext.All
                .FirstOrDefault(x => x.Name?.Contains("Referenceable") ?? false)?
                .Assemblies?
                .FirstOrDefault(x => x.FullName == args.Name);
        };
    }

    /// <inheritdoc />
    public override string Name => "Excluded Libraries Home Sections";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
            }
        };
    }
}

