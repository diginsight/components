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
using System.Reflection;
using System.Text;

namespace Diginsight.Components.Configuration;

public static class HostBuilderExtensions
{
    public static Type T = typeof(HostBuilderExtensions);
    public static IHostBuilder ConfigureAppConfiguration2(this IHostBuilder hostBuilder, ILoggerFactory loggerFactory, Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { hostBuilder, tagsMatch });
        if (ObservabilityHelper.LoggerFactory == null) { ObservabilityHelper.LoggerFactory = loggerFactory; }

        return hostBuilder.ConfigureAppConfiguration((hbc, cb) => ConfigureAppConfiguration2(hbc.HostingEnvironment, cb, loggerFactory, tagsMatch));
    }

    public static void ConfigureAppConfiguration2(IHostEnvironment environment, IConfigurationBuilder builder, ILoggerFactory loggerFactory, Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { environment, builder, tagsMatch });
        if (ObservabilityHelper.LoggerFactory == null) { ObservabilityHelper.LoggerFactory = loggerFactory; }

        bool isLocal = environment.IsDevelopment();

        if (isLocal)
        {
            int appsettingsIndex = GetJsonFileIndex("appsettings.json", builder);
            AppendLocalJsonFile("appsettings.local.json", appsettingsIndex, builder, isLocal);
        }

        var environmentName = environment.EnvironmentName;
        int appsettingsEnvironmentIndex = GetJsonFileIndex($"appsettings.{environmentName}.json", builder);

        string? appsettingsEnvironmentName = Environment.GetEnvironmentVariable("AppsettingsEnvironmentName") ?? environmentName;
        var appsettingsFileFolder = ".";
        var appsettingsFileName = $"appsettings.{appsettingsEnvironmentName}.json";
        var appsettingsFilePath = appsettingsFileName;

        if (!appsettingsEnvironmentName.Equals(environmentName, StringComparison.InvariantCultureIgnoreCase))
        {
            ((JsonConfigurationSource)builder.Sources[appsettingsEnvironmentIndex]).Path = appsettingsFilePath;
        }

        var externalConfigurationFolder = Environment.GetEnvironmentVariable("ExternalConfigurationFolder");
        var externalConfigurationFolderExists = externalConfigurationFolder is not null && Directory.Exists(externalConfigurationFolder);
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

        if (isLocal)
        {
            var appsettingsLocalFileFolder = appsettingsFileFolder;
            var appsettingsLocalFileName = $"appsettings.{appsettingsEnvironmentName}.local.json";
            var appsettingsLocalFilePath = appsettingsLocalFileFolder == "." ? appsettingsLocalFileName : Path.Combine(appsettingsLocalFileFolder, appsettingsLocalFileName);
            AppendLocalJsonFile(appsettingsLocalFilePath, appsettingsEnvironmentIndex, builder, isLocal);
        }


        IConfiguration configuration = builder.Build();

        var kvUri = configuration["AzureKeyVault:Uri"];
        if (!string.IsNullOrEmpty(kvUri))
        {
            var clientId = configuration["AzureKeyVault:ClientId"].HardTrim();
            var tenantId = configuration["AzureKeyVault:TenantId"].HardTrim();
            var clientSecret = configuration["AzureKeyVault:ClientSecret"].HardTrim();
            var applicationCredentialProvider = new ApplicationCredentialProvider(environment);

            var credential = applicationCredentialProvider.Get(tenantId, clientId, clientSecret);
            builder.AddAzureKeyVault(new Uri(kvUri), credential, new KeyVaultSecretManager2(DateTimeOffset.UtcNow, tagsMatch));
        }

        int environmentVariablesIndex = GetSourceIndex(builder.Sources, static x => x.Source is EnvironmentVariablesConfigurationSource) ?? -1;
        if (environmentVariablesIndex >= 0)
        {
            int sourcesCount = builder.Sources.Count;
            IConfigurationSource kvConfigurationSource = builder.Sources.Last();
            builder.Sources.RemoveAt(sourcesCount - 1);
            builder.Sources.Insert(environmentVariablesIndex, kvConfigurationSource);
        }
    }

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
    private static int GetJsonFileIndex(string path, IConfigurationBuilder builder)
    {
        return GetSourceIndex(builder.Sources, x => x.Source is JsonConfigurationSource jsonSource && string.Equals(jsonSource.Path, path, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("No such json configuration source");
    }
    private static int? GetSourceIndex(IList<IConfigurationSource> sources, Func<(IConfigurationSource Source, int Index), bool> predicate)
    {
        return sources
            .Select(static (source, index) => (Source: source, Index: index))
            .Where(predicate)
            .Select(static x => (int?)x.Index)
            .FirstOrDefault();
    }
}



