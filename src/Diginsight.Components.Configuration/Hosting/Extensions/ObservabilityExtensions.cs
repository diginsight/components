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
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Diginsight.Components.Configuration;


public static partial class ObservabilityExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment
    )
    {
        return services.AddObservability(configuration, hostEnvironment, out OpenTelemetryOptions _);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        out IOpenTelemetryOptions openTelemetryOptions
    )
    {
        services.AddObservability(configuration, hostEnvironment, out OpenTelemetryOptions mutableOpenTelemetryOptions);

        openTelemetryOptions = mutableOpenTelemetryOptions;
        return services;
    }

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        out OpenTelemetryOptions mutableOpenTelemetryOptions
    )
    {
        const string diginsightConfKey = "Diginsight";
        const string observabilityConfKey = "Observability";

        bool isLocal = hostEnvironment.IsDevelopment();
        string assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;

        mutableOpenTelemetryOptions = new OpenTelemetryOptions() {
            EnableTraces = true,
            EnableMetrics = true,
            TracingSamplingRatio = isLocal ? 1 : 0.1
        };
        IOpenTelemetryOptions openTelemetryOptions = mutableOpenTelemetryOptions;

        services.Configure<OpenTelemetryOptions>((o) => {
            o.EnableTraces = true;
            o.EnableMetrics = true;
            o.TracingSamplingRatio = isLocal ? 1 : 0.1;
        });

        IConfiguration openTelemetryConfiguration = configuration.GetSection("OpenTelemetry");
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
                            fo.TotalWidth = isLocal ? -1 : 0;
                            fo.UseColor = isLocal;
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

        services.Configure<DiginsightActivitiesOptions>(
            dao =>
            {
                dao.LogActivities = true;
                dao.MeterName = assemblyName;
            }
        );

        //services.AddSingleton<IConfigureClassAwareOptions<DiginsightActivitiesOptions>>(
        //    new ConfigureClassAwareOptions<DiginsightActivitiesOptions>(
        //        Microsoft.Extensions.Options.Options.DefaultName,
        //        static (t, dao) =>
        //        {
        //            IReadOnlyList<string> markers = ClassConfigurationMarkers.For(t);
        //            if (markers.Contains("ABC.*") || markers.Contains("NH.*") || markers.Contains("Diginsight.*"))
        //            {
        //                dao.RecordSpanDurations = true;
        //            }
        //        }
        //    )
        //);

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
                }
            );
        }

        return services;
    }

}
