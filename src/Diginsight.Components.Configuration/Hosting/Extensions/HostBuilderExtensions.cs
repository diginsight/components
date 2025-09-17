using Azure.Core;
using Azure.Identity;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Diginsight.Components.Configuration;


public static class HostBuilderExtensions
{
    public static Type T = typeof(HostBuilderExtensions);

    /// <summary>
    /// Configures the application configuration for the host builder using the specified logger factory and optional tags match function.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="loggerFactory">The logger factory to use for logging.</param>
    /// <param name="tagsMatch">An optional function to filter configuration tags.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder ConfigureAppConfiguration2(this IHostBuilder hostBuilder,
        ILoggerFactory loggerFactory,
        Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { hostBuilder, tagsMatch });
        if (ObservabilityHelper.LoggerFactory == null) { ObservabilityHelper.LoggerFactory = loggerFactory; }

        return hostBuilder.ConfigureAppConfiguration((hbc, cb) => ConfigureAppConfiguration2(hbc.HostingEnvironment, cb, loggerFactory, tagsMatch));
    }

    /// <summary>
    /// Configures the application configuration sources for the specified environment and configuration builder.
    /// </summary>
    /// <param name="environment">The hosting environment.</param>
    /// <param name="builder">The configuration builder to configure.</param>
    /// <param name="loggerFactory">The logger factory to use for logging.</param>
    /// <param name="tagsMatch">An optional function to filter configuration tags.</param>
    public static void ConfigureAppConfiguration2(IHostEnvironment environment,
        IConfigurationBuilder builder,
        ILoggerFactory loggerFactory,
        Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { environment, builder, tagsMatch });
        if (ObservabilityHelper.LoggerFactory == null) { ObservabilityHelper.LoggerFactory = loggerFactory; }

        bool isLocal = environment.IsDevelopment();
        bool isDebuggerAttached = Debugger.IsAttached;
        logger.LogDebug($"isLocal:{isLocal}, isDebuggerAttached:{isDebuggerAttached}");

        var runtimeEnvironmentName = environment.EnvironmentName;
        int appsettingsFileIndex = GetJsonFileIndex($"appsettings.json", builder); logger.LogDebug($"GetJsonFileIndex($\"appsettings.json\", builder); returned {appsettingsFileIndex}");
        int environmentAppsettingsFileIndex = GetJsonFileIndex($"appsettings.{runtimeEnvironmentName}.json", builder); logger.LogDebug($"GetJsonFileIndex($\"appsettings.{runtimeEnvironmentName}.json\", builder); returned {environmentAppsettingsFileIndex}");

        var appsettingsEnvironmentName = Environment.GetEnvironmentVariable("AppsettingsEnvironmentName");
        var environmentName = appsettingsEnvironmentName ?? runtimeEnvironmentName;
        logger.LogDebug($"runtimeEnvironmentName:{runtimeEnvironmentName},appsettingsEnvironmentName:{appsettingsEnvironmentName},environmentName:{environmentName}");

        var appsettingsFileFolder = ".";
        var appsettingsFileName = $"appsettings.{environmentName}.json";
        var appsettingsFilePath = appsettingsFileName;

        var lastAppsettingsFileIndex = environmentAppsettingsFileIndex >= 0 ? environmentAppsettingsFileIndex : appsettingsFileIndex;
        if (!environmentName.Equals(runtimeEnvironmentName, StringComparison.InvariantCultureIgnoreCase))
        {
            if (File.Exists(appsettingsFileName))
            {
                AppendJsonFile(appsettingsFileName, lastAppsettingsFileIndex, builder);
            }
            if (environmentAppsettingsFileIndex >= 0)
            {
                builder.Sources.RemoveAt(environmentAppsettingsFileIndex);
            }
        }

        if (isLocal)
        {
            var allConfigurationFiles = new[] { $"appsettings.json", $"appsettings.{environmentName}.json", $"appsettings.local.json", $"appsettings.{environmentName}.local.json" };
            var localConfigurationFiles = new[] { $"appsettings.{environmentName}.json", $"appsettings.local.json", $"appsettings.{environmentName}.local.json" };
            foreach (var configurationFile in localConfigurationFiles)
            {
                logger.LogDebug($"Checking for local file: {configurationFile}");
                var currentDirectory = Directory.GetCurrentDirectory();
                var currentDirectoryInfo = new DirectoryInfo(currentDirectory);
                var repositoryRoot = DirectoryHelper.GetRepositoryRoot(currentDirectory)!;
                var repositoryRootInfo = new DirectoryInfo(repositoryRoot);
                var currentDirectoryParts = GetCurrentDirectoryParts(currentDirectoryInfo, repositoryRootInfo);

                if (File.Exists(configurationFile))
                {
                    var lastAppsettingsFile = builder.Sources.LastOrDefault(cs => cs is FileConfigurationSource && (((FileConfigurationSource)cs)?.Path?.StartsWith("appsettings", StringComparison.InvariantCultureIgnoreCase) ?? false));
                    lastAppsettingsFileIndex = lastAppsettingsFile != null ? builder.Sources.IndexOf(lastAppsettingsFile) : -1;

                    if (lastAppsettingsFileIndex >= 0)
                    {
                        AppendLocalJsonFile(configurationFile, lastAppsettingsFileIndex, builder, isLocal);
                    }
                }

                var found = false; // look for the file in external folder, if found add into the builder.Source at end of ...
                var externalConfigurationFolder = Environment.GetEnvironmentVariable("ExternalConfigurationFolder");
                if (string.IsNullOrEmpty(externalConfigurationFolder)) { continue; }

                var externalConfigurationFolderDirectoryInfo = new DirectoryInfo(externalConfigurationFolder!);
                var potentialAppsettingsFolder = externalConfigurationFolderDirectoryInfo.FullName;
                while (currentDirectoryParts.Count() >= 0)
                {
                    var potentialSubfolder = currentDirectoryParts.Any() ? Path.Combine(currentDirectoryParts.ToArray()) : string.Empty;
                    var potentialFolder = Path.Combine(potentialAppsettingsFolder, potentialSubfolder);
                    var potentialFilePath = Path.Combine(potentialFolder, configurationFile);
                    if (File.Exists(potentialFilePath))
                    {
                        appsettingsFileFolder = potentialFolder;
                        appsettingsFilePath = potentialFilePath;
                        found = true;
                        break;
                    }
                    if (currentDirectoryParts.Any()) { currentDirectoryParts.RemoveAt(currentDirectoryParts.Count() - 1); } else { break; }
                }

                if (found)
                {
                    var lastAppsettingsFile = builder.Sources.LastOrDefault(cs => cs is FileConfigurationSource && (((FileConfigurationSource)cs)?.Path?.StartsWith("appsettings", StringComparison.InvariantCultureIgnoreCase) ?? false));
                    lastAppsettingsFileIndex = lastAppsettingsFile != null ? builder.Sources.IndexOf(lastAppsettingsFile) : -1;

                    AppendLocalJsonFile(appsettingsFilePath, lastAppsettingsFileIndex, builder, isLocal);
                    //builder.Sources.RemoveAt(lastAppsettingsFileIndex);
                }
            }
        }

        IConfiguration configuration = builder.Build();

        var kvUri = configuration["AzureKeyVault:Uri"];
        logger.LogDebug($"kvUri:{kvUri}");
        if (!string.IsNullOrEmpty(kvUri))
        {
            TokenCredential credential;
            var credentialProvider = new DefaultCredentialProvider(environment);
            credential = credentialProvider.Get(configuration.GetSection("AzureKeyVault"));

            builder.AddAzureKeyVault(new Uri(kvUri), credential, new KeyVaultSecretManagerWithTags(DateTimeOffset.UtcNow, tagsMatch));
            logger.LogDebug($"builder.AddAzureKeyVault({kvUri})");
        }

        int environmentVariablesIndex = GetSourceLastIndex(builder.Sources, static x => x.Source is EnvironmentVariablesConfigurationSource) ?? -1;
        if (environmentVariablesIndex >= 0)
        {
            int sourcesCount = builder.Sources.Count;
            IConfigurationSource kvConfigurationSource = builder.Sources.Last();
            builder.Sources.RemoveAt(sourcesCount - 1);
            builder.Sources.Insert(environmentVariablesIndex, kvConfigurationSource);
        }
    }

    private static List<string> GetCurrentDirectoryParts(DirectoryInfo? currentDirectoryInfo, DirectoryInfo repositoryRootInfo)
    {
        var currentDirectoryParts = new List<string>();
        while (currentDirectoryInfo != null && currentDirectoryInfo.Exists)
        {
            currentDirectoryParts.Insert(0, currentDirectoryInfo.Name);
            currentDirectoryInfo = currentDirectoryInfo.Parent;
            if (currentDirectoryInfo == null || currentDirectoryInfo.FullName.Equals(repositoryRootInfo.FullName)) { break; }
        }

        return currentDirectoryParts;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TBuilder ConfigureAppConfiguration2<TBuilder>(
        this TBuilder hostAppBuilder,
        ILoggerFactory loggerFactory = null,
        Func<IDictionary<string, string>, bool>? kvTagsMatch = null
    )
        where TBuilder : IHostApplicationBuilder
    {
        ConfigureAppConfiguration2(hostAppBuilder.Environment, hostAppBuilder.Configuration, loggerFactory, kvTagsMatch);
        return hostAppBuilder;
    }
    /// <summary>
    /// Appends a local JSON configuration file to the configuration builder at the specified index if running locally.
    /// </summary>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="index">The index at which to insert the configuration source.</param>
    /// <param name="builder">The configuration builder to modify.</param>
    /// <param name="isLocal">Indicates if the environment is local.</param>
    private static void AppendLocalJsonFile(string path, int index, IConfigurationBuilder builder, bool isLocal)
    {
        var loggerFactory = ObservabilityHelper.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { path, index, isLocal });

        if (!isLocal) { return; }

        //JsonConfigurationSource jsonSource = new JsonConfigurationSource()
        //{
        //    Path = path,
        //    Optional = true,
        //    ReloadOnChange = true,
        //};
        //builder.Sources.Insert(index + 1, jsonSource);
        builder.AddJsonFile(path, true, true);
        var lastSource = builder.Sources.Last();
        builder.Sources.Insert(index + 1, lastSource);
        builder.Sources.RemoveAt(builder.Sources.Count - 1);
    }
    private static void AppendJsonFile(string path, int index, IConfigurationBuilder builder)
    {
        var loggerFactory = ObservabilityHelper.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { path, index });

        //if (!isLocal) { return; }

        //JsonConfigurationSource jsonSource = new JsonConfigurationSource()
        //{
        //    Path = path,
        //    Optional = true,
        //    ReloadOnChange = true,
        //};
        //builder.Sources.Insert(index + 1, jsonSource);
        builder.AddJsonFile(path, true, true);
        var lastSource = builder.Sources.Last();
        builder.Sources.Insert(index + 1, lastSource);
        builder.Sources.RemoveAt(builder.Sources.Count - 1);
    }
    /// <summary>
    /// Retrieves the index of a JSON configuration source in the specified configuration builder.
    /// </summary>
    /// <param name="path">The file path of the JSON configuration source to locate. This comparison is case-insensitive.</param>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> containing the configuration sources to search.</param>
    /// <returns>The zero-based index of the JSON configuration source in the <paramref name="builder"/> sources collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no JSON configuration source with the specified <paramref name="path"/> exists in the <paramref
    /// name="builder"/>.</exception>
    private static int GetJsonFileIndex(string path, IConfigurationBuilder builder)
    {
        var loggerFactory = ObservabilityHelper.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { path, builder });

        var ret = GetSourceIndex(builder.Sources, x => x.Source is JsonConfigurationSource jsonSource &&
                                                       string.Equals(jsonSource.Path, path, StringComparison.OrdinalIgnoreCase))
            ?? -1;

        activity?.SetOutput(ret);
        return ret;
    }
    /// <summary>
    /// Retrieves the index of the first configuration source matching the predicate.
    /// </summary>
    /// <param name="sources">The list of configuration sources.</param>
    /// <param name="predicate">The predicate to match sources.</param>
    /// <returns>The index of the first matching source, or null if not found.</returns>
    private static int? GetSourceIndex(IList<IConfigurationSource> sources, Func<(IConfigurationSource Source, int Index), bool> predicate)
    {
        return sources.Select(static (source, index) => (Source: source, Index: index))
            .Where(predicate)
            .Select(static x => (int?)x.Index)
            .FirstOrDefault();
    }
    /// <summary>
    /// Retrieves the index of the last configuration source matching the predicate.
    /// </summary>
    /// <param name="sources">The list of configuration sources.</param>
    /// <param name="predicate">The predicate to match sources.</param>
    /// <returns>The index of the last matching source, or null if not found.</returns>
    private static int? GetSourceLastIndex(IList<IConfigurationSource> sources, Func<(IConfigurationSource Source, int Index), bool> predicate)
    {
        return sources.Select(static (source, index) => (Source: source, Index: index))
            .Where(predicate)
            .Select(static x => (int?)x.Index)
            .LastOrDefault();
    }
}



