using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KeyVaultSample
{
    public static class AddOpenTelemetryExtension
    {
        internal const string SERVICE_NAME = "KeyVaultSample";

        public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
        {
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            return app;
        }

        public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            var cloudRoleName = typeof(App).Assembly.GetName().Name;
            var cloudRoleNamespace = typeof(App).Assembly.GetName().FullName;
            var cloudRoleInstance = typeof(App).Assembly.GetName().FullName;
            var aiConnectionString = configuration["ApplicationInsights:ConnectionString"];

            // https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-configuration?tabs=aspnetcore
            // Create a dictionary of resource attributes.
            var resourceAttributes = new Dictionary<string, object> {
                { "service.name", cloudRoleName },
                { "service.namespace", cloudRoleNamespace },
                { "service.instance.id", cloudRoleInstance }};

            ObservabilityDefaults.ActivitySource = TraceLogger.ActivitySource;
            ObservabilityDefaults.Meter = KeyVaultSampleMetrics.Instance.Meter;  // KeyVaultSample / SpanDuration

            //services.AddOpenTelemetryTracing((builder) => builder
            //    // Configure the resource attribute `service.name` to MyServiceName
            //    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyServiceName"))
            //    // Add tracing of the AspNetCore instrumentation library
            //    .AddAspNetCoreInstrumentation()
            //    .AddConsoleExporter()
            //);

            services.ConfigureOpenTelemetryMeterProvider((sp, builder) =>
            {
                //builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                builder.AddConsoleExporter();
                builder.AddPrometheusExporter();

                //builder.AddOtlpExporter();
                builder.AddMetrics<KeyVaultSampleMetrics>();
                builder.AddMeter(KeyVaultSampleMetrics.StaticObservabilityName);
                
                builder.AddAzureMonitorMetricExporter();
                builder.ConfigureResource(resourceBuilder => resourceBuilder.AddAttributes(resourceAttributes));
            });

            //services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
            //{
            //    builder.ConfigureResource(resourceBuilder => resourceBuilder.AddAttributes(resourceAttributes));

            //    //builder.AddAspNetCoreInstrumentation();
            //    builder.AddHttpClientInstrumentation();
            //    //builder.AddConsoleExporter();
            //    builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Debug); // 
            //    builder.AddAzureMonitorTraceExporter();
            //    // builder.AddRedisInstrumentation();

            //    builder.AddSource("Azure.*");
            //    builder.AddSource(TraceLogger.ActivitySource.Name);
            //    builder.AddSource(KeyVaultSampleMetrics.Instance.ObservabilityName);
            //    //builder.SetSampler(serviceProvider => new HttpHeaderSampler(serviceProvider, new ParentBasedSampler(new TraceIdRatioBasedSampler(openTelemetryOptions.TracingSamplingRatio))));
            //    //builder.AddOtlpExporter(options =>
            //    //    {
            //    //        options.Endpoint = new Uri(openTelemetryOptions.OltpEndpoint);
            //    //    });
            //});

            var builder = services.AddOpenTelemetry().WithTracing(builder =>
            {
                builder.ConfigureResource(resourceBuilder => resourceBuilder.AddAttributes(resourceAttributes));

                builder.AddProcessor<ObservabilityLogProcessor>();
                builder.AddProcessor<DurationMetricProcessor>();

                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                //builder.AddConsoleExporter();
                builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Debug);
                builder.AddAzureMonitorTraceExporter();
                // builder.AddRedisInstrumentation();

                builder.AddSource("Azure.*");
                builder.AddSource(TraceLogger.ActivitySource.Name);
                builder.AddSource(KeyVaultSampleMetrics.Instance.ObservabilityName);
                //builder.SetSampler(serviceProvider => new HttpHeaderSampler(serviceProvider, new ParentBasedSampler(new TraceIdRatioBasedSampler(openTelemetryOptions.TracingSamplingRatio))));
                //builder.AddOtlpExporter(options =>
                //    {
                //        options.Endpoint = new Uri(openTelemetryOptions.OltpEndpoint);
                //    }); 
            });

            builder.ConfigureResource(builder => builder.AddService(serviceName: cloudRoleName));

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
