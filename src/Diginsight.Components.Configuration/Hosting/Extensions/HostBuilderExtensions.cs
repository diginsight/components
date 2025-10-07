using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Diginsight.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Diginsight.Components.Configuration;

public static class HostBuilderExtensions
{
    private static readonly Type T = typeof(HostBuilderExtensions);

    /// <summary>
    /// Configures the application configuration for the host builder using the specified logger factory and optional tags match function.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="tagsMatch">An optional function to filter configuration tags.</param>
    /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder ConfigureAppConfiguration2(this IHostBuilder hostBuilder,
        Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        var logger = Observability.LoggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { hostBuilder, tagsMatch });

        return hostBuilder.ConfigureAppConfiguration((hbc, cb) => ConfigureAppConfiguration2(hbc.HostingEnvironment, cb, tagsMatch));
    }

    /// <summary>
    /// Configures the application configuration sources for the specified environment and configuration builder.
    /// </summary>
    /// <param name="environment">The hosting environment.</param>
    /// <param name="builder">The configuration builder to configure.</param>
    /// <param name="tagsMatch">An optional function to filter configuration tags.</param>
    public static void ConfigureAppConfiguration2(IHostEnvironment environment,
        IConfigurationBuilder builder,
        Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        var logger = Observability.LoggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { environment, builder, tagsMatch });

        bool isLocal = environment.IsDevelopment();
        bool isDebuggerAttached = Debugger.IsAttached;
        logger.LogDebug("isLocal:{IsLocal}, isDebuggerAttached:{IsDebuggerAttached}", isLocal, isDebuggerAttached);

        var runtimeEnvironmentName = environment.EnvironmentName;
        int appsettingsFileIndex = GetJsonFileIndex("appsettings.json", builder); logger.LogDebug($"GetJsonFileIndex($\"appsettings.json\", builder); returned {appsettingsFileIndex}");
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
            var allConfigurationFiles = new[] { "appsettings.json", $"appsettings.{environmentName}.json", "appsettings.local.json", $"appsettings.{environmentName}.local.json" };
            var localConfigurationFiles = new[] { $"appsettings.{environmentName}.json", "appsettings.local.json", $"appsettings.{environmentName}.local.json" };
            var currentDirectory = Directory.GetCurrentDirectory();
            var currentDirectoryInfo = new DirectoryInfo(currentDirectory);
            var repositoryRoot = DirectoryHelper.GetRepositoryRoot(currentDirectory)!;
            var repositoryRootInfo = new DirectoryInfo(repositoryRoot);
            var currentDirectoryParts = GetCurrentDirectoryParts(currentDirectoryInfo, repositoryRootInfo);

            foreach (var configurationFile in localConfigurationFiles)
            {
                logger.LogDebug($"Checking for local file: {configurationFile}");
                if (File.Exists(configurationFile) && !builder.Sources.Any(cs => cs is FileConfigurationSource fcs && (fcs.Path?.Equals(configurationFile, StringComparison.InvariantCultureIgnoreCase) ?? false)))
                {
                    var lastAppsettingsFile = builder.Sources.LastOrDefault(static cs => cs is FileConfigurationSource fcs && (fcs.Path?.StartsWith("appsettings", StringComparison.InvariantCultureIgnoreCase) ?? false));
                    lastAppsettingsFileIndex = lastAppsettingsFile is not null ? builder.Sources.IndexOf(lastAppsettingsFile) : -1;

                    if (lastAppsettingsFileIndex >= 0)
                    {
                        AppendLocalJsonFile(configurationFile, lastAppsettingsFileIndex, builder, isLocal);
                    }
                }

                var found = false; // look for the file in external folder, if found add into the builder.Source at end of ...
                var externalConfigurationFolder = Environment.GetEnvironmentVariable("ExternalConfigurationFolder");
                if (string.IsNullOrEmpty(externalConfigurationFolder)) { continue; }

                var externalConfigurationFolderDirectoryInfo = new DirectoryInfo(externalConfigurationFolder);
                var potentialAppsettingsFolder = externalConfigurationFolderDirectoryInfo.FullName;
                while (currentDirectoryParts.Count >= 0)
                {
                    var potentialSubfolder = currentDirectoryParts.Any() ? Path.Combine(currentDirectoryParts.ToArray()) : string.Empty;
                    var potentialFolder = Path.Combine(potentialAppsettingsFolder, potentialSubfolder);
                    var potentialFilePath = Path.Combine(potentialFolder, configurationFile);
                    if (File.Exists(potentialFilePath))
                    {
                        appsettingsFilePath = potentialFilePath;
                        found = true;
                        break;
                    }
                    if (currentDirectoryParts.Any()) { currentDirectoryParts.RemoveAt(currentDirectoryParts.Count - 1); } else { break; }
                }

                if (found)
                {
                    var lastAppsettingsFile = builder.Sources.LastOrDefault(static cs => cs is FileConfigurationSource fcs && (fcs.Path?.StartsWith("appsettings", StringComparison.InvariantCultureIgnoreCase) ?? false));
                    lastAppsettingsFileIndex = lastAppsettingsFile is not null ? builder.Sources.IndexOf(lastAppsettingsFile) : -1;

                    AppendLocalJsonFile(appsettingsFilePath, lastAppsettingsFileIndex, builder, isLocal);
                }
            }
        }

        IConfiguration configuration = builder.Build();

        var dumpAllValuesString = configuration["AzureKeyVault:DumpValues"] ?? "true";
        _ = bool.TryParse(dumpAllValuesString, out var dumpAllValues);
        if (!dumpAllValues) { DumpConfigurationSources(configuration); }

        var kvUri = configuration["AzureKeyVault:Uri"];
        logger.LogDebug($"kvUri:{kvUri}");
        if (!string.IsNullOrEmpty(kvUri))
        {
            var credentialProvider = new DefaultCredentialProvider(environment);
            TokenCredential credential = credentialProvider.Get(configuration.GetSection("AzureKeyVault"));

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
        if (dumpAllValues) { DumpConfigurationSources(configuration); }
    }

    private static List<string> GetCurrentDirectoryParts(DirectoryInfo? currentDirectoryInfo, DirectoryInfo repositoryRootInfo)
    {
        var currentDirectoryParts = new List<string>();
        while (currentDirectoryInfo is not null && currentDirectoryInfo.Exists)
        {
            currentDirectoryParts.Insert(0, currentDirectoryInfo.Name);
            currentDirectoryInfo = currentDirectoryInfo.Parent;
            if (currentDirectoryInfo is null || currentDirectoryInfo.FullName.Equals(repositoryRootInfo.FullName)) { break; }
        }

        return currentDirectoryParts;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TBuilder ConfigureAppConfiguration2<TBuilder>(
        this TBuilder hostAppBuilder,
        Func<IDictionary<string, string>, bool>? kvTagsMatch = null
    )
        where TBuilder : IHostApplicationBuilder
    {
        ConfigureAppConfiguration2(hostAppBuilder.Environment, hostAppBuilder.Configuration, kvTagsMatch);
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
        var logger = Observability.LoggerFactory.CreateLogger(T);
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
        var logger = Observability.LoggerFactory.CreateLogger(T);
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
        var logger = Observability.LoggerFactory.CreateLogger(T);
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
    /// <summary>
    /// Dumps all configuration sources loaded into the configuration builder for debugging purposes.
    /// </summary>
    /// <param name="configuration">The configuration to inspect</param>
    public static void DumpConfigurationSources(IConfiguration configuration)
    {
        var logger = Observability.LoggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { configuration });

        if (configuration is not IConfigurationRoot configRoot) { logger.LogWarning("⚠️ Configuration is not IConfigurationRoot, cannot dump configuration sources"); return; }

        logger.LogDebug("📋 Configuration Sources Dump:");
        logger.LogDebug("═══════════════════════════════════════════════════════════════");

        var sources = configRoot.Providers.ToList();
        for (int i = 0; i < sources.Count; i++)
        {
            var provider = sources[i];

            var isSecretsConfig = false;
            var forceMask = false; var maxLen = -1;
            if (provider.GetType().Name.Equals(nameof(AzureKeyVaultConfigurationProvider))) { isSecretsConfig = true; }
            if (provider is FileConfigurationProvider fcp && (fcp.Source.Path?.Equals("secrets", StringComparison.InvariantCultureIgnoreCase) ?? false)) { isSecretsConfig = true; }
            if (isSecretsConfig) { forceMask = true; maxLen = 50; }

            DumpConfigurationProvider(logger, i, provider, forceMask, maxLen);
        }

        logger.LogDebug("✅ Total Configuration Sources: {TotalCount}", sources.Count);
        logger.LogDebug("═══════════════════════════════════════════════════════════════");

        activity?.SetOutput(new { TotalSources = sources.Count, Sources = sources.Select(GetConfigurationProviderInfo).ToArray() });
    }

    private static void DumpConfigurationProvider(ILogger logger, int i, IConfigurationProvider provider, bool forceMaskValues = false, int maxLen = -1)
    {
        var configInfo = GetConfigurationProviderInfo(provider);

        logger.LogDebug("🔧 [{Index:D2}] {ProviderType}: {Source}", i + 1, provider.GetType().Name, configInfo.Source);
        try
        {
            var root = new ConfigurationRoot(new List<IConfigurationProvider> { provider });
            var keys = root.AsEnumerable();
            foreach (var kvp in keys ?? [])
            {
                var isSensitiveValue = IsSensitiveValue(kvp.Key, kvp.Value);
                logger.LogDebug("🔑 {Key}: {Value}", kvp.Key, isSensitiveValue || forceMaskValues ? kvp.Value?.Mask(maxLen) : kvp.Value);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug("❌ Error reading keys from provider: {Error}", ex.Message);
        }

        logger.LogDebug("───────────────────────────────────────────────────────────────");
    }

    /// <summary>
    /// Masks sensitive configuration values for secure logging.
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <param name="value">The configuration value</param>
    /// <returns>The value or a masked version if sensitive</returns>
    private static bool IsSensitiveValue(string key, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var lowerKey = key.ToLowerInvariant();

        // List of patterns that indicate sensitive values
        var sensitivePatterns = new[]
        {
            "password", "secret", "key", "token", "connectionstring",
            "clientsecret", "apikey", "accesskey", "credential"
        };

        return sensitivePatterns.Any(pattern => lowerKey.Contains(pattern));
    }
    /// <summary>
    /// Extracts detailed information from a configuration provider.
    /// </summary>
    /// <param name="provider">The configuration provider to inspect</param>
    /// <returns>Configuration provider information</returns>
    private static ConfigurationProviderInfo GetConfigurationProviderInfo(IConfigurationProvider provider)
    {
        var providerType = provider.GetType();
        var configName = providerType.Name;
        var source = "Unknown";
        string additionalInfo;

        try
        {
            switch (provider)
            {
                case JsonConfigurationProvider jsonProvider:
                    configName = "JSON Configuration";
                    source = GetJsonProviderSource(jsonProvider);
                    additionalInfo = GetJsonProviderInfo(jsonProvider);
                    break;

                case EnvironmentVariablesConfigurationProvider envProvider:
                    configName = "Environment Variables";
                    source = "System Environment";
                    additionalInfo = GetEnvironmentProviderInfo(envProvider);
                    break;

                case CommandLineConfigurationProvider cmdProvider:
                    configName = "Command Line Arguments";
                    source = "Command Line";
                    additionalInfo = GetCommandLineProviderInfo(cmdProvider);
                    break;

                case MemoryConfigurationProvider memProvider:
                    configName = "In-Memory Configuration";
                    source = "Memory";
                    additionalInfo = GetMemoryProviderInfo(memProvider);
                    break;

                case ChainedConfigurationProvider chainedProvider:
                    configName = "Chained Configuration";
                    source = "Chained Sources";
                    additionalInfo = GetChainedProviderInfo(chainedProvider);
                    break;

                default:
                    // Handle Azure Key Vault and other providers
                    if (providerType.Name.Contains("KeyVault"))
                    {
                        configName = "Azure Key Vault";
                        source = GetKeyVaultProviderSource(provider);
                        additionalInfo = GetKeyVaultProviderInfo(provider);
                    }
                    else if (providerType.Name.Contains("UserSecrets"))
                    {
                        configName = "User Secrets";
                        source = GetUserSecretsProviderSource(provider);
                        additionalInfo = GetUserSecretsProviderInfo(provider);
                    }
                    else
                    {
                        configName = providerType.Name.Replace("ConfigurationProvider", "").Replace("Provider", "");
                        source = GetGenericProviderSource(provider);
                        additionalInfo = GetGenericProviderInfo(provider);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            additionalInfo = $"Error reading provider info: {ex.Message}";
        }

        return new ConfigurationProviderInfo
        {
            ConfigName = configName,
            ProviderType = providerType.FullName ?? providerType.Name,
            Source = source,
            AdditionalInfo = additionalInfo
        };
    }

    private static string GetJsonProviderSource(JsonConfigurationProvider provider)
    {
        try
        {
            var source = provider.Source;
            if (source is not null)
            {
                return source.Path ?? "Unknown JSON file";
            }
        }
        catch (Exception) { }

        return "JSON Configuration File";
    }

    private static string GetJsonProviderInfo(JsonConfigurationProvider provider)
    {
        try
        {
            var sourceField = provider.GetType().GetField("_source", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sourceField?.GetValue(provider) is JsonConfigurationSource source)
            {
                var info = new List<string>()
                {
                    $"Optional: {source.Optional}",
                    $"ReloadOnChange: {source.ReloadOnChange}",
                };
                return string.Join(", ", info);
            }
        }
        catch (Exception) { }

        return "";
    }

    private static string GetEnvironmentProviderInfo(EnvironmentVariablesConfigurationProvider provider)
    {
        try
        {
            var sourceField = provider.GetType().GetField("_source", BindingFlags.NonPublic | BindingFlags.Instance);
            if (sourceField?.GetValue(provider) is EnvironmentVariablesConfigurationSource source)
            {
                return !string.IsNullOrEmpty(source.Prefix) ? $"Prefix: '{source.Prefix}'" : "No prefix";
            }
        }
        catch (Exception) { }

        return "";
    }

    private static string GetCommandLineProviderInfo(CommandLineConfigurationProvider provider)
    {
        try
        {
            var argsField = provider.GetType().GetField("_args", BindingFlags.NonPublic | BindingFlags.Instance);
            if (argsField?.GetValue(provider) is string[] args)
            {
                return $"Arguments count: {args.Length}";
            }
        }
        catch (Exception) { }

        return "";
    }

    private static string GetMemoryProviderInfo(MemoryConfigurationProvider provider)
    {
        try
        {
            var dataField = provider.GetType().GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dataField?.GetValue(provider) is IDictionary<string, string> data)
            {
                return $"Keys count: {data.Count}";
            }
        }
        catch (Exception) { }

        return "";
    }

    private static string GetChainedProviderInfo(ChainedConfigurationProvider provider)
    {
        try
        {
            var configField = provider.GetType().GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance);
            if (configField?.GetValue(provider) is IConfiguration config)
            {
                return $"Chained configuration type: {config.GetType().Name}";
            }
        }
        catch (Exception) { }

        return "";
    }

    private static string GetKeyVaultProviderSource(IConfigurationProvider provider)
    {
        try
        {
            // Try to get Key Vault URI through reflection
            var clientField = provider.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(static f => f.Name.Contains("client", StringComparison.OrdinalIgnoreCase));

            if (clientField?.GetValue(provider) is { } client)
            {
                var vaultUriProperty = client.GetType().GetProperty("VaultUri");
                if (vaultUriProperty?.GetValue(client) is Uri vaultUri)
                {
                    return vaultUri.ToString();
                }
            }
        }
        catch (Exception) { }

        return "Azure Key Vault";
    }

    private static string GetKeyVaultProviderInfo(IConfigurationProvider provider)
    {
        return "Azure Key Vault configuration provider";
    }

    private static string GetUserSecretsProviderSource(IConfigurationProvider provider)
    {
        try
        {
            var pathField = provider.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(static f => f.Name.Contains("path", StringComparison.OrdinalIgnoreCase));

            if (pathField?.GetValue(provider) is string path)
            {
                return path;
            }
        }
        catch (Exception) { }

        return "User Secrets";
    }

    private static string GetUserSecretsProviderInfo(IConfigurationProvider provider)
    {
        return "Development user secrets";
    }

    private static string GetGenericProviderSource(IConfigurationProvider provider)
    {
        try
        {
            // Try common field names for source information
            var sourceFields = new[] { "_source", "_path", "_uri", "_endpoint", "_connectionString" };

            foreach (var fieldName in sourceFields)
            {
                var field = provider.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field?.GetValue(provider) is { } value)
                {
                    return value.ToString() ?? "Unknown";
                }
            }
        }
        catch (Exception) { }

        return provider.GetType().Name;
    }

    private static string GetGenericProviderInfo(IConfigurationProvider provider)
    {
        return $"Provider type: {provider.GetType().FullName}";
    }

    /// <summary>
    /// Configuration provider information container.
    /// </summary>
    private record ConfigurationProviderInfo
    {
        public string ConfigName { get; init; } = "";
        public string ProviderType { get; init; } = "";
        public string Source { get; init; } = "";
        public string AdditionalInfo { get; init; } = "";
    }
}




