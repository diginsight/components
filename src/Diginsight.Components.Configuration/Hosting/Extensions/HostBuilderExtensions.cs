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

        if (isLocal)
        {
            int appsettingsIndex = GetJsonFileIndex("appsettings.json", builder);
            AppendLocalJsonFile("appsettings.local.json", appsettingsIndex, builder, isLocal);
        }

        //if (isDebuggerAttached)
        //{
        //    var entryAssembly = Assembly.GetEntryAssembly();
        //    builder.AddUserSecrets(entryAssembly);
        //}

        var environmentName = environment.EnvironmentName;
        int appsettingsEnvironmentIndex = GetJsonFileIndex($"appsettings.{environmentName}.json", builder); logger.LogDebug($"GetJsonFileIndex($\"appsettings.{environmentName}.json\", builder); returned {appsettingsEnvironmentIndex}");

        string? appsettingsEnvironmentName = Environment.GetEnvironmentVariable("AppsettingsEnvironmentName") ?? environmentName;
        var appsettingsFileFolder = ".";
        var appsettingsFileName = $"appsettings.{appsettingsEnvironmentName}.json";
        var appsettingsFilePath = appsettingsFileName;
        logger.LogDebug($"appsettingsEnvironmentName:{appsettingsEnvironmentName},appsettingsFileName:{appsettingsFileName}");

        if (!appsettingsEnvironmentName.Equals(environmentName, StringComparison.InvariantCultureIgnoreCase))
        {
            AppendJsonFile(appsettingsFilePath, appsettingsEnvironmentIndex, builder);
            builder.Sources.RemoveAt(appsettingsEnvironmentIndex);
        }

        var externalConfigurationFolder = Environment.GetEnvironmentVariable("ExternalConfigurationFolder");
        var externalConfigurationFolderExists = externalConfigurationFolder is not null && Directory.Exists(externalConfigurationFolder);
        logger.LogDebug($"isLocal:{isLocal},externalConfigurationFolder:{externalConfigurationFolder},externalConfigurationFolderExists:{externalConfigurationFolderExists}");
        if (isLocal && externalConfigurationFolderExists && !File.Exists(appsettingsFilePath))
        {
            var externalConfigurationFolderDirectoryInfo = new DirectoryInfo(externalConfigurationFolder!);
            var currentDirectory = Directory.GetCurrentDirectory();
            var currentDirectoryInfo = new DirectoryInfo(currentDirectory);
            var repositoryRoot = DirectoryHelper.GetRepositoryRoot(currentDirectory)!;
            var repositoryRootInfo = new DirectoryInfo(repositoryRoot);

            var currentDirectoryParts = new List<string>();
            while (currentDirectoryInfo != null && currentDirectoryInfo.Exists)
            {
                currentDirectoryParts.Insert(0, currentDirectoryInfo.Name);
                currentDirectoryInfo = currentDirectoryInfo.Parent;
                if (currentDirectoryInfo == null || currentDirectoryInfo.FullName.Equals(repositoryRootInfo.FullName)) { break; }
            }

            var found = false;
            var potentialAppsettingsFolder = externalConfigurationFolderDirectoryInfo.FullName;
            while (currentDirectoryParts.Count() >= 0)
            {
                var potentialSubfolder = currentDirectoryParts.Any() ? Path.Combine(currentDirectoryParts.ToArray()) : string.Empty;
                var potentialFolder = Path.Combine(potentialAppsettingsFolder, potentialSubfolder);
                var potentialFilePath = Path.Combine(potentialFolder, appsettingsFileName);
                if (File.Exists(potentialFilePath))
                {
                    appsettingsFileFolder = potentialFolder;
                    appsettingsFilePath = potentialFilePath;
                    found = true;
                    break;
                }
                if (currentDirectoryParts.Any()) { currentDirectoryParts.RemoveAt(currentDirectoryParts.Count() - 1); }
            }

            if (found)
            {
                AppendLocalJsonFile(appsettingsFilePath, appsettingsEnvironmentIndex, builder, isLocal);
                builder.Sources.RemoveAt(appsettingsEnvironmentIndex);
            }
        }

        var appsettingsLocalFileFolder = appsettingsFileFolder;
        var appsettingsLocalFileName = $"appsettings.{appsettingsEnvironmentName}.local.json";
        var appsettingsLocalFilePath = appsettingsLocalFileName;
        var appsettingsLocalEnvironmentIndex = GetJsonFileIndex($"appsettings.{appsettingsEnvironmentName}.json", builder);
        //var appsettingsLocalFilePath = appsettingsLocalFileFolder == "." ? appsettingsLocalFileName : Path.Combine(appsettingsLocalFileFolder, appsettingsLocalFileName);
        //if (isLocal)
        //{
        //    AppendLocalJsonFile(appsettingsLocalFilePath, appsettingsEnvironmentIndex, builder, isLocal);
        //}

        if (isLocal)
        {
            if (externalConfigurationFolderExists && !File.Exists(appsettingsLocalFilePath))
            {
                var externalConfigurationFolderDirectoryInfo = new DirectoryInfo(externalConfigurationFolder!);
                var currentDirectory = Directory.GetCurrentDirectory();
                var currentDirectoryInfo = new DirectoryInfo(currentDirectory);
                var repositoryRoot = DirectoryHelper.GetRepositoryRoot(currentDirectory)!;
                var repositoryRootInfo = new DirectoryInfo(repositoryRoot);

                var currentDirectoryParts = new List<string>();
                while (currentDirectoryInfo != null && currentDirectoryInfo.Exists)
                {
                    currentDirectoryParts.Insert(0, currentDirectoryInfo.Name);
                    currentDirectoryInfo = currentDirectoryInfo.Parent;
                    if (currentDirectoryInfo == null || currentDirectoryInfo.FullName.Equals(repositoryRootInfo.FullName)) { break; }
                }

                var found = false;
                var potentialAppsettingsFolder = externalConfigurationFolderDirectoryInfo.FullName;
                while (currentDirectoryParts.Count() >= 0)
                {
                    var potentialSubfolder = currentDirectoryParts.Any() ? Path.Combine(currentDirectoryParts.ToArray()) : string.Empty;
                    var potentialFolder = Path.Combine(potentialAppsettingsFolder, potentialSubfolder);
                    var potentialFilePath = Path.Combine(potentialFolder, appsettingsLocalFileName);
                    if (File.Exists(potentialFilePath))
                    {
                        appsettingsFileFolder = potentialFolder;
                        appsettingsLocalFilePath = potentialFilePath;
                        found = true;
                        break;
                    }
                    if (currentDirectoryParts.Any()) { currentDirectoryParts.RemoveAt(currentDirectoryParts.Count() - 1); }
                }

                if (found)
                {
                    AppendLocalJsonFile(appsettingsLocalFilePath, appsettingsEnvironmentIndex, builder, isLocal);
                    //builder.Sources.RemoveAt(appsettingsEnvironmentIndex);
                }
            }
            else
            {
                AppendLocalJsonFile(appsettingsLocalFilePath, appsettingsEnvironmentIndex, builder, isLocal);
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

        var ret = GetSourceIndex(builder.Sources, x => x.Source is JsonConfigurationSource jsonSource && string.Equals(jsonSource.Path, path, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("No such json configuration source");

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



