using Azure.Monitor.OpenTelemetry.Exporter;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Diginsight.Diagnostics.Log4Net;
using Diginsight.Options;
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.IO;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Diginsight.Components.Configuration;

using Options = Microsoft.Extensions.Options.Options;


public static partial class ObservabilityExtensions
{
    static Type T = typeof(ObservabilityExtensions);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        bool configureDefaults = true
    )
    {
        var loggerFactory = ObservabilityHelper.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { services, configuration, hostEnvironment, configureDefaults });

        return services.AddObservability(configuration, hostEnvironment, out OpenTelemetryOptions _, configureDefaults);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        out IOpenTelemetryOptions openTelemetryOptions,
        bool configureDefaults = true
    )
    {
        var loggerFactory = ObservabilityHelper.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { services, configuration, hostEnvironment, configureDefaults });

        services.AddObservability(configuration, hostEnvironment, out OpenTelemetryOptions mutableOpenTelemetryOptions, configureDefaults);

        openTelemetryOptions = mutableOpenTelemetryOptions;
        return services;
    }

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        out OpenTelemetryOptions mutableOpenTelemetryOptions,
        bool configureDefaults = true
    )
    {
        var loggerFactory = ObservabilityHelper.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { services, configuration, hostEnvironment, configureDefaults });

        const string diginsightConfKey = "Diginsight";
        const string observabilityConfKey = "Observability";

        bool isLocal = hostEnvironment.IsDevelopment(); logger.LogDebug("isLocal: {isLocal}", isLocal);
        string assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!; logger.LogDebug("assemblyName: {assemblyName}", assemblyName);

        IConfiguration openTelemetryConfiguration = configuration.GetSection("OpenTelemetry"); logger.LogDebug("openTelemetryConfiguration: {openTelemetryConfiguration}", openTelemetryConfiguration);

        mutableOpenTelemetryOptions = new OpenTelemetryOptions();
        IOpenTelemetryOptions openTelemetryOptions = mutableOpenTelemetryOptions;
        if (configureDefaults)
        {
            void ConfigureOpenTelemetryDefaults(OpenTelemetryOptions o)
            {
                o.EnableTraces = true;
                o.EnableMetrics = true;
                o.TracingSamplingRatio = isLocal ? 1 : 0.1;
            }

            ConfigureOpenTelemetryDefaults(mutableOpenTelemetryOptions);
            services.Configure<OpenTelemetryOptions>(ConfigureOpenTelemetryDefaults);
        }

        openTelemetryConfiguration.Bind(mutableOpenTelemetryOptions); logger.LogDebug("openTelemetryConfiguration.Bind(mutableOpenTelemetryOptions);");
        services.Configure<OpenTelemetryOptions>(openTelemetryConfiguration); logger.LogDebug("services.Configure<OpenTelemetryOptions>(openTelemetryConfiguration);");

        services.TryAddSingleton<IActivityLoggingSampler, NameBasedActivityLoggingSampler>(); logger.LogDebug("services.TryAddSingleton<IActivityLoggingSampler, NameBasedActivityLoggingSampler>();");

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, ActivitySourceDetectorRegistration>()); logger.LogDebug("services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, ActivitySourceDetectorRegistration>());");

        string? azureMonitorConnectionString = openTelemetryOptions.AzureMonitorConnectionString; logger.LogDebug("azureMonitorConnectionString: {azureMonitorConnectionString}", azureMonitorConnectionString);

        services.AddLogging(
            loggingBuilder =>
            {
                using var activityInner = Observability.ActivitySource.StartRichActivity(logger, "AddLogging.Configure", () => new { loggingBuilder });
                
                loggingBuilder.ClearProviders(); logger.LogDebug("loggingBuilder.ClearProviders();");

                var consoleEnabled = configuration.GetValue(ConfigurationPath.Combine(observabilityConfKey, "ConsoleEnabled"), true); logger.LogDebug("consoleEnabled: {consoleEnabled}", consoleEnabled);
                if (consoleEnabled)
                {
                    loggingBuilder.AddDiginsightConsole(
                        fo =>
                        {
                            if (configureDefaults)
                            {
                                fo.TotalWidth = isLocal ? -1 : 0;
                                fo.UseColor = isLocal;
                            }
                            configuration.GetSection(ConfigurationPath.Combine(diginsightConfKey, "Console")).Bind(fo);
                        }
                    );
                    logger.LogDebug("loggingBuilder.AddDiginsightConsole();");
                }

                var log4NetEnabled = configuration.GetValue(ConfigurationPath.Combine(observabilityConfKey, "Log4NetEnabled"), true); logger.LogDebug("log4NetEnabled: {log4NetEnabled}", log4NetEnabled);
                if (log4NetEnabled)
                {
                    loggingBuilder.AddDiginsightLog4Net(
                        sp =>
                        {
                            IHostEnvironment env = sp.GetRequiredService<IHostEnvironment>();
                            string fileBaseDir = env.IsDevelopment()
                                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)
                                : $"{Path.DirectorySeparatorChar}home";
                            logger.LogDebug("fileBaseDir: {fileBaseDir}", fileBaseDir);

                            return
                            [
                                new RollingFileAppender()
                                {
                                    File = Path.Combine(fileBaseDir, "LogFiles", "Diginsight", assemblyName),
                                    AppendToFile = true,
                                    StaticLogFileName = false,
                                    RollingStyle = RollingFileAppender.RollingMode.Composite,
                                    DatePattern = @".yyyyMMdd.\l\o\g",
                                    MaxSizeRollBackups = 1000,
                                    MaximumFileSize = "100MB",
                                    Encoding = System.Text.Encoding.UTF8,
                                    LockingModel = new FileAppender.MinimalLock(),
                                    Layout = new DiginsightLayout()
                                    {
                                        Pattern = "{Timestamp} {Category} {LogLevel} {TraceId} {Delta} {Duration} {Depth} {Indentation|-1} {Message}",
                                    },
                                },
                            ];
                        },
                        static _ => Level.All
                    );
                    logger.LogDebug("loggingBuilder.AddDiginsightLog4Net();");
                }

                if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                {
                    loggingBuilder.AddDiginsightOpenTelemetry(
                        otlo => otlo.AddAzureMonitorLogExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        )
                    );
                    logger.LogDebug("loggingBuilder.AddDiginsightOpenTelemetry();");
                }
            }
        );

        logger.LogDebug("services.Logging();");
        logger.LogDebug("configureDefaults: {configureDefaults}", configureDefaults);
        if (configureDefaults)
        {
            services.Configure<DiginsightActivitiesOptions>(
                dao =>
                {
                    dao.LogBehavior = LogBehavior.Show;
                    dao.MeterName = assemblyName;
                }
            );

            services.AddSingleton<IConfigureClassAwareOptions<DiginsightActivitiesOptions>>(
                new ConfigureClassAwareOptions<DiginsightActivitiesOptions>(
                    Options.DefaultName,
                    static (t, dao) =>
                    {
                        IReadOnlyList<string> markers = ClassConfigurationMarkers.For(t);
                        if (markers.Contains("Diginsight.*"))
                        {
                            dao.RecordSpanDurations = true;
                        }
                    }
                )
            );
        }

        services.ConfigureClassAware<DiginsightActivitiesOptions>(configuration.GetSection(ConfigurationPath.Combine(diginsightConfKey, "Activities")));
        services
            .VolatilelyConfigureClassAware<DiginsightActivitiesOptions>()
            .DynamicallyConfigureClassAware<DiginsightActivitiesOptions>();
        logger.LogDebug("services.ConfigureClassAware<DiginsightActivitiesOptions>();");

        IOpenTelemetryBuilder openTelemetryBuilder = services.AddDiginsightOpenTelemetry(); logger.LogDebug("services.AddDiginsightOpenTelemetry();");

        logger.LogDebug("openTelemetryOptions.EnableMetrics: {openTelemetryOptions.EnableMetrics}", openTelemetryOptions.EnableMetrics);
        if (openTelemetryOptions.EnableMetrics)
        {
            services.AddSpanDurationMetricRecorder(); logger.LogDebug("services.AddSpanDurationMetricRecorder();");
            services.TryAddSingleton<ISpanDurationMetricRecorderSettings, NameBasedSpanDurationMetricRecorderSettings>(); logger.LogDebug("services.TryAddSingleton<ISpanDurationMetricRecorderSettings, NameBasedSpanDurationMetricRecorderSettings>();");

            if (!services.Any(static x => x.ServiceType == typeof(DecoratedSpanDurationMetricRecorderSettingsMarker)))
            {
                services.AddSingleton<DecoratedSpanDurationMetricRecorderSettingsMarker>(); logger.LogDebug("services.AddSingleton<DecoratedSpanDurationMetricRecorderSettingsMarker>();");
                services.Decorate<ISpanDurationMetricRecorderSettings, DecoratorTagsSpanDurationMetricRecorderSettings>(); logger.LogDebug("services.Decorate<ISpanDurationMetricRecorderSettings, DecoratorTagsSpanDurationMetricRecorderSettings>();");
            }

            openTelemetryBuilder.WithMetrics(
                meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .AddDiginsight()
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddMeter(openTelemetryOptions.Meters.ToArray());

                    if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                    {
                        meterProviderBuilder.AddAzureMonitorMetricExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        );
                    }
                }
            );
        }

        logger.LogDebug("openTelemetryOptions.EnableTraces: {openTelemetryOptions.EnableTraces}", openTelemetryOptions.EnableTraces);
        if (openTelemetryOptions.EnableTraces)
        {
            openTelemetryBuilder.WithTracing(
                tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddDiginsight()
                        .AddSource(openTelemetryOptions.ActivitySources.ToArray())
                        .SetErrorStatusOnException(); logger.LogDebug("tracerProviderBuilder.AddDiginsight();");

                    if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                    {
                        tracerProviderBuilder.AddAzureMonitorTraceExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        ); logger.LogDebug("tracerProviderBuilder.AddAzureMonitorTraceExporter();");
                    }

                    tracerProviderBuilder.SetSampler(
                        static sp =>
                        {
                            IOpenTelemetryOptions openTelemetryOptions = sp.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;
                            return new ParentBasedSampler(new TraceIdRatioBasedSampler(openTelemetryOptions.TracingSamplingRatio));
                        }
                    ); logger.LogDebug("tracerProviderBuilder.SetSampler();");
                }
            );
        }

        return services;
    }

}
