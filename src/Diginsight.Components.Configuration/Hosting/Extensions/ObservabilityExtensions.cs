using Azure.Monitor.OpenTelemetry.Exporter;
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
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Diginsight.Diagnostics;
using Diginsight.Diagnostics.Log4Net;
using Diginsight.Options;
using Diginsight.Components.Azure;

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
        using var activity = Observability.ActivitySource.StartMethodActivity(T, logger, () => new { services, configuration, hostEnvironment, configureDefaults });

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
        using var activity = Observability.ActivitySource.StartMethodActivity(T, logger, () => new { services, configuration, hostEnvironment, configureDefaults });

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
        using var activity = Observability.ActivitySource.StartMethodActivity(T, logger, () => new { services, configuration, hostEnvironment, configureDefaults });

        const string diginsightConfKey = "Diginsight";

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

        services.TryAddSingleton<IActivityLoggingFilter, OptionsBasedActivityLoggingFilter>(); logger.LogDebug("services.TryAddSingleton<IActivityLoggingFilter, OptionsBasedActivityLoggingFilter>();");

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, ActivitySourceDetectorRegistration>()); logger.LogDebug("services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, ActivitySourceDetectorRegistration>());");

        string? azureMonitorConnectionString = openTelemetryOptions.AzureMonitorConnectionString; logger.LogDebug("azureMonitorConnectionString: {azureMonitorConnectionString}", azureMonitorConnectionString);

        services.AddLogging(
            loggingBuilder =>
            {
                using var activityInner = Observability.ActivitySource.StartRichActivity(logger, "AddLogging.Configure", () => new { loggingBuilder });

                loggingBuilder.ClearProviders(); logger.LogDebug("loggingBuilder.ClearProviders();");

                var debugEnabled = configuration.GetValue(ConfigurationPath.Combine("Observability", "DebugEnabled"), false); logger.LogDebug("debugEnabled: {debugEnabled}", debugEnabled);
                if (debugEnabled)
                {
                    loggingBuilder.AddDiginsightDebug();
                    //co => 
                    //{
                    //    if (configureDefaults)
                    //    {
                    //        //co.Pattern = "{Timestamp} {Category} {LogLevel} {Message}";
                    //        //co.TimeZone = TimeZoneInfo.Local;
                    //    }

                    //});
                    logger.LogDebug("loggingBuilder.AddDiginsightDebug();");
                }

                var consoleEnabled = configuration.GetValue(ConfigurationPath.Combine("Observability", "ConsoleEnabled"), true); logger.LogDebug("consoleEnabled: {consoleEnabled}", consoleEnabled);
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

                var log4NetEnabled = configuration.GetValue(ConfigurationPath.Combine("Observability", "Log4NetEnabled"), true); logger.LogDebug("log4NetEnabled: {log4NetEnabled}", log4NetEnabled);
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
                    dao.EnablePayloadTagging = openTelemetryOptions.EnablePayloadTagging;
                    dao.RecordSpanDuration = true;             // Singular
                    dao.SpanDurationMeterName = assemblyName;  // SpanDuration prefix
                    dao.SpanDurationMetricName = "diginsight.span_duration";
                    dao.SpanDurationMetricDescription = "Duration of application spans";
                    // dao.MetricUnit removed - no longer used
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
                            dao.RecordSpanDuration = true;
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

        // Configure QueryCostMetricRecorderOptions from "Diginsight:QueryCostMetricRecorder" section
        services.ConfigureClassAware<Diginsight.Components.Azure.Metrics.QueryCostMetricRecorderOptions>(configuration.GetSection(ConfigurationPath.Combine(diginsightConfKey, "QueryCostMetricRecorder")));
        services
            .VolatilelyConfigureClassAware<Diginsight.Components.Azure.Metrics.QueryCostMetricRecorderOptions>()
            .DynamicallyConfigureClassAware<Diginsight.Components.Azure.Metrics.QueryCostMetricRecorderOptions>();
        logger.LogDebug("services.ConfigureClassAware<QueryCostMetricRecorderOptions>();");

        IOpenTelemetryBuilder openTelemetryBuilder = services.AddDiginsightOpenTelemetry(); logger.LogDebug("services.AddDiginsightOpenTelemetry();");

        logger.LogDebug("openTelemetryOptions.EnableMetrics: {openTelemetryOptions.EnableMetrics}", openTelemetryOptions.EnableMetrics);
        if (openTelemetryOptions.EnableMetrics)
        {
            var diginsightConfig = configuration.GetSection(ConfigurationPath.Combine(diginsightConfKey, "Activities"));

            var defaultMetricActivities = diginsightConfig.GetSection("SpanMeasuredActivityNames").Get<IDictionary<string, bool>>() ?? new Dictionary<string, bool>();
            logger.LogDebug("Found {Count} default metric activity configurations", defaultMetricActivities.Count);

            var metricSpecificActivitiesSection = diginsightConfig.GetSection("MetricSpecificSpanMeasuredActivityNames");
            var metricSpecificActivities = metricSpecificActivitiesSection.Get<MetricRecordingFilterConfiguration[]>() ?? [];
            logger.LogDebug("Found {Count} metric-specific activity configurations", metricSpecificActivities.Length);

            var defaultMetricTags = diginsightConfig.GetSection("MetricTags").Get<string[]>() ?? Array.Empty<string>();
            logger.LogDebug("Default MetricTags: {Tags}", string.Join(", ", defaultMetricTags));

            var metricSpecificTagsSection = diginsightConfig.GetSection("MetricSpecificTags");
            var metricSpecificTags = metricSpecificTagsSection.Get<MetricRecordingEnricherConfiguration[]>() ?? [];
            logger.LogDebug("Found {Count} metric-specific tag configurations", metricSpecificTags.Length);

            services.Configure<OptionsBasedMetricRecordingFilterOptions>(
                options =>
                {
                    foreach (var (activityName, record) in defaultMetricActivities)
                    {
                        options.ActivityNames[activityName] = record;
                    }
                }
            );

            foreach (var metricConfig in metricSpecificActivities)
            {
                string metricName = GetRequiredMetricName(metricConfig.MetricName, metricSpecificActivitiesSection.Path);
                services.Configure<OptionsBasedMetricRecordingFilterOptions>(
                    metricName,
                    options =>
                    {
                        foreach (var (activityName, record) in metricConfig.ActivityNames)
                        {
                            options.ActivityNames[activityName] = record;
                        }
                    }
                );
            }

            services.Configure<OptionsBasedMetricRecordingEnricherOptions>(
                options =>
                {
                    foreach (string metricTag in defaultMetricTags)
                    {
                        options.MetricTags.Add(metricTag);
                    }
                }
            );

            foreach (var metricConfig in metricSpecificTags)
            {
                string metricName = GetRequiredMetricName(metricConfig.MetricName, metricSpecificTagsSection.Path);
                services.Configure<OptionsBasedMetricRecordingEnricherOptions>(
                    metricName,
                    options =>
                    {
                        foreach (string metricTag in metricConfig.MetricTags)
                        {
                            options.MetricTags.Add(metricTag);
                        }
                    }
                );
            }

            services.TryAddSingleton<IMetricRecordingFilter, OptionsBasedMetricRecordingFilter>();
            services.TryAddSingleton<IMetricRecordingEnricher, OptionsBasedMetricRecordingEnricher>();

            services.AddSpanDurationMetricRecorder(); logger.LogDebug("services.AddSpanDurationMetricRecorder();");
            services.AddCosmosDbQueryCostMetricRecorder(); logger.LogDebug("services.AddCosmosDbQueryCostMetricRecorder();");

            logger.LogDebug("Configured metric recorders with default and metric-specific filtering and enrichment");

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

    private static string GetRequiredMetricName(string? metricName, string configurationSectionPath)
    {
        if (string.IsNullOrWhiteSpace(metricName))
        {
            throw new InvalidOperationException(
                $"Configuration section '{configurationSectionPath}' contains an entry with a missing or whitespace-only MetricName."
            );
        }

        return metricName;
    }

}
