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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Diginsight.Components.Configuration;

using Options = Microsoft.Extensions.Options.Options;


public static partial class ObservabilityExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        bool configureDefaults = true
    )
    {
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
        const string diginsightConfKey = "Diginsight";
        const string observabilityConfKey = "Observability";

        bool isLocal = hostEnvironment.IsDevelopment();
        string assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;

        IConfiguration openTelemetryConfiguration = configuration.GetSection("OpenTelemetry");

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

        openTelemetryConfiguration.Bind(mutableOpenTelemetryOptions);
        services.Configure<OpenTelemetryOptions>(openTelemetryConfiguration);

        services.TryAddSingleton<IActivityLoggingSampler, NameBasedActivityLoggingSampler>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, ActivitySourceDetectorRegistration>());

        string? azureMonitorConnectionString = openTelemetryOptions.AzureMonitorConnectionString;

        services.AddLogging(
            loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                if (configuration.GetValue(ConfigurationPath.Combine(observabilityConfKey, "ConsoleEnabled"), true))
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
                }

                if (configuration.GetValue(ConfigurationPath.Combine(observabilityConfKey, "Log4NetEnabled"), false))
                {
                    loggingBuilder.AddDiginsightLog4Net(
                        sp =>
                        {
                            IHostEnvironment env = sp.GetRequiredService<IHostEnvironment>();
                            string fileBaseDir = env.IsDevelopment()
                                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)
                                : $"{Path.DirectorySeparatorChar}home";

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
                }

                if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                {
                    loggingBuilder.AddDiginsightOpenTelemetry(
                        otlo => otlo.AddAzureMonitorLogExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        )
                    );
                }
            }
        );

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

        IOpenTelemetryBuilder openTelemetryBuilder = services.AddDiginsightOpenTelemetry();

        if (openTelemetryOptions.EnableMetrics)
        {
            services.AddSpanDurationMetricRecorder();
            services.TryAddSingleton<ISpanDurationMetricRecorderSettings, NameBasedSpanDurationMetricRecorderSettings>();

            if (!services.Any(static x => x.ServiceType == typeof(DecoratedSpanDurationMetricRecorderSettingsMarker)))
            {
                services.AddSingleton<DecoratedSpanDurationMetricRecorderSettingsMarker>();
                services.Decorate<ISpanDurationMetricRecorderSettings, DecoratorTagsSpanDurationMetricRecorderSettings>();
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

        if (openTelemetryOptions.EnableTraces)
        {
            openTelemetryBuilder.WithTracing(
                tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddDiginsight()
                        .AddSource(openTelemetryOptions.ActivitySources.ToArray())
                        .SetErrorStatusOnException();

                    if (!string.IsNullOrEmpty(azureMonitorConnectionString))
                    {
                        tracerProviderBuilder.AddAzureMonitorTraceExporter(
                            exporterOptions => { exporterOptions.ConnectionString = azureMonitorConnectionString; }
                        );
                    }

                    tracerProviderBuilder.SetSampler(
                        static sp =>
                        {
                            IOpenTelemetryOptions openTelemetryOptions = sp.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;
                            return new ParentBasedSampler(new TraceIdRatioBasedSampler(openTelemetryOptions.TracingSamplingRatio));
                        }
                    );
                }
            );
        }

        return services;
    }

}
