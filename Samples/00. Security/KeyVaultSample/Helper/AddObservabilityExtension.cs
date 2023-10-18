using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KeyVaultSample
{
    public static class AddOpenTelemetryExtension
    {
        internal const string ENERGY_MANAGER_SERVICE = "energy-manager";

        public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
        {
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            return app;
        }

        public static IServiceCollection AddObservability(this IServiceCollection services, string aiConnectionString)
        {
            // https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-configuration?tabs=aspnetcore
            // Create a dictionary of resource attributes.
            var resourceAttributes = new Dictionary<string, object> {
                { "service.name", "KeyVaultSample" },
                { "service.namespace", "KeyVaultSampleNamespace" },
                { "service.instance.id", "KeyVaultSample-ID" }};

            services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
            {
                builder.AddSource(App.ActivitySource.Name);
                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                builder.AddConsoleExporter();
                //builder.AddRedisInstrumentation();
                builder.AddSource("Azure.*");
                //builder.SetSampler(serviceProvider => new HttpHeaderSampler(serviceProvider, new ParentBasedSampler(new TraceIdRatioBasedSampler(openTelemetryOptions.TracingSamplingRatio))));
                //builder.AddOtlpExporter(options =>
                //    {
                //        options.Endpoint = new Uri(openTelemetryOptions.OltpEndpoint);
                //    });
                builder.AddSource(KeyVaultSampleMetrics.Instance.ObservabilityName);
                builder.AddAzureMonitorTraceExporter();
                builder.ConfigureResource(resourceBuilder => resourceBuilder.AddAttributes(resourceAttributes));
            });

            services.ConfigureOpenTelemetryMeterProvider((sp, builder) =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                builder.AddConsoleExporter();
                builder.AddPrometheusExporter();
                //builder.AddOtlpExporter();
                builder.AddMetrics<KeyVaultSampleMetrics>();
                builder.AddMeter(KeyVaultSampleMetrics.StaticObservabilityName);
                builder.AddAzureMonitorMetricExporter();
                builder.ConfigureResource(resourceBuilder => resourceBuilder.AddAttributes(resourceAttributes));
            });

            var builder = services.AddOpenTelemetry();

            builder.ConfigureResource(builder => builder
                .AddService(serviceName: ENERGY_MANAGER_SERVICE));

            //builder.WithMetrics(builder => builder
            //    .AddMeter(AverageLoadAdapter.ObservabilityName)
            //    .AddMeter(NextMaintenanceDateAdapter.ObservabilityName)
            //    .AddMeter(WatticsService.ObservabilityName)
            //    .AddPrometheusExporter());

            //builder.WithTracing(tracing => tracing.AddSource(Startup.ActivitySource.Name));

            builder.UseAzureMonitor(options =>
            {
                options.ConnectionString = aiConnectionString;
            });

            return services;
        }
    }
}
