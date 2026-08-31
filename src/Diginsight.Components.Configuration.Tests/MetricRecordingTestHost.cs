using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Diginsight.Components.Configuration.Tests;

internal static class MetricRecordingTestHost
{
    public static ServiceCollection CreateServices(
        IReadOnlyDictionary<string, string?>? configurationOverrides = null,
        Action<IServiceCollection>? configureBeforeObservability = null
    )
    {
        Dictionary<string, string?> configurationValues = new()
        {
            ["OpenTelemetry:EnableMetrics"] = "true",
            ["OpenTelemetry:EnableTraces"] = "false",
            ["Observability:ConsoleEnabled"] = "false",
            ["Observability:DebugEnabled"] = "false",
            ["Observability:Log4NetEnabled"] = "false",
        };

        if (configurationOverrides is not null)
        {
            foreach (var (key, value) in configurationOverrides)
            {
                configurationValues[key] = value;
            }
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
        IHostEnvironment hostEnvironment = new TestHostEnvironment();

        ServiceCollection services = new();
        services.AddSingleton(configuration);
        services.AddSingleton(hostEnvironment);
        configureBeforeObservability?.Invoke(services);
        new HostBuilder().ConfigureAppConfiguration2(NullLoggerFactory.Instance);
        services.AddObservability(configuration, hostEnvironment);

        return services;
    }

    public static ServiceProvider BuildServiceProvider(
        IReadOnlyDictionary<string, string?>? configurationOverrides = null,
        Action<IServiceCollection>? configureBeforeObservability = null
    )
    {
        return CreateServices(configurationOverrides, configureBeforeObservability)
            .BuildServiceProvider();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = typeof(MetricRecordingTestHost).Assembly.GetName().Name!;
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}