using Diginsight.AspNetCore;
using Diginsight.Diagnostics;
using Diginsight.Diagnostics.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Runtime.CompilerServices;

namespace Diginsight.Components.Configuration;

public static class WebHostingExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddAspNetCoreObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        bool configureDefaults = true,
        TraceInstrumentationCallbacks? traceInstrumentationCallbacks = null
    )
    {
        return services.AddAspNetCoreObservability(
            configuration, hostEnvironment, out OpenTelemetryOptions _, traceInstrumentationCallbacks
        ); // configureDefaults,
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddAspNetCoreObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        out IOpenTelemetryOptions openTelemetryOptions,
        bool configureDefaults = true,
        TraceInstrumentationCallbacks? traceInstrumentationCallbacks = null
    )
    {
        services.AddAspNetCoreObservability(
            configuration, hostEnvironment, out OpenTelemetryOptions mutableOpenTelemetryOptions, traceInstrumentationCallbacks
        ); // configureDefaults,

        openTelemetryOptions = mutableOpenTelemetryOptions;
        return services;
    }

    public static IServiceCollection AddAspNetCoreObservability(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment,
            out OpenTelemetryOptions mutableOpenTelemetryOptions,
            TraceInstrumentationCallbacks? traceInstrumentationCallbacks = null
        )
        //bool configureDefaults = true,
    {
        traceInstrumentationCallbacks ??= new TraceInstrumentationCallbacks();

        services.AddObservability(configuration, hostEnvironment, out mutableOpenTelemetryOptions); //, configureDefaults)

        IOpenTelemetryOptions openTelemetryOptions = mutableOpenTelemetryOptions;

        services.AddHttpContextAccessor();

        services.AddVolatileConfiguration();

        DefaultDynamicConfigurationLoader.AddToServices(services);
        services.AddDynamicLogLevel<DefaultDynamicLogLevelInjector>();
        services.AddLogging(static lb => lb.AddVolatileConfiguration());

        if (!services.Any(static x => x.ServiceType == typeof(DecoratedActivityLoggingSamplerMarker)))
        {
            services.AddSingleton<DecoratedActivityLoggingSamplerMarker>();
            services.Decorate<IActivityLoggingFilter, DecoratorHttpHeadersActivityLoggingFilter>();
        }

        ConfigurationVolatileConfigurationLoader.AddToServices(services);
        LogLevelVolatileConfigurationLoader.AddToServices(services);

        services.Configure<DiginsightDistributedContextOptions>(
            static x =>
            {
                x.NonBaggageKeys.Add(HttpHeadersActivityLoggingFilter.HeaderName);
                x.NonBaggageKeys.Add(HttpHeadersSpanDurationMetricRecordingFilter.HeaderName);
            }
        );

        IOpenTelemetryBuilder openTelemetryBuilder = services.AddDiginsightOpenTelemetry();

        if (openTelemetryOptions.EnableMetrics)
        {
            if (!services.Any(static x => x.ServiceType == typeof(MetricRecordingDurationMetricTagsEnricherMarker)))
            {
                services.AddSingleton<MetricRecordingDurationMetricTagsEnricherMarker>();
                services.Decorate<IMetricRecordingFilter, DecoratorHttpHeadersSpanDurationMetricRecordingFilter>();
            }

            openTelemetryBuilder.WithMetrics(
                static meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .AddAspNetCoreInstrumentation();
                }
            );
        }

        if (openTelemetryOptions.EnableTraces)
        {
            openTelemetryBuilder.WithTracing(
                tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddAspNetCoreInstrumentation(
                            options =>
                            {
                                options.EnrichWithHttpRequest = static (activity, httpRequest) =>
                                {
                                    var context = httpRequest.HttpContext;

                                    activity.DisplayName = $"{context.Request.Method.ToUpperInvariant()} {context.Request.Path}";
                                    activity.SetTag("http.client_ip", context.Connection.RemoteIpAddress);
                                    activity.SetTag("http.request_content_length", httpRequest.ContentLength);
                                    activity.SetTag("http.request_content_type", httpRequest.ContentType);
                                };

                                options.EnrichWithHttpResponse = static (activity, httpResponse) =>
                                {
                                    activity.SetTag("http.response_content_length", httpResponse.ContentLength);
                                    activity.SetTag("http.response_content_type", httpResponse.ContentType);
                                };

                                options.EnrichWithException = (activity, exception) =>
                                {
                                    if (traceInstrumentationCallbacks.ShouldSetStackTraceTag(activity, exception))
                                    {
                                        activity.SetTag("stack_trace", exception.StackTrace);
                                    }
                                };
                            }
                        );

                    tracerProviderBuilder
                        .SetHttpHeadersSampler(
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

