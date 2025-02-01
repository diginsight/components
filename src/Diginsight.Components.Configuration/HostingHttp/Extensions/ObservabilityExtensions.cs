using Diginsight.Diagnostics;
using Diginsight.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Diginsight.Components.Configuration;

public static partial class ObservabilityExtensions
{
    public static IServiceCollection AddHttpObservability(
        this IServiceCollection services,
        IOpenTelemetryOptions openTelemetryOptions,
        TraceInstrumentationCallbacks? traceInstrumentationCallbacks = null
    )
    {
        traceInstrumentationCallbacks ??= new TraceInstrumentationCallbacks();

        IOpenTelemetryBuilder openTelemetryBuilder = services.AddDiginsightOpenTelemetry();

        if (openTelemetryOptions.EnableTraces)
        {
            openTelemetryBuilder.WithTracing(
                tracerProviderBuilder =>
                {
                    void ConfigureHttpClientTraceInstrumentationOptions(HttpClientTraceInstrumentationOptions options)
                    {
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            if (httpRequestMessage.Options.TryGetValue(HttpExtensions.InvocationOptionKey, out (Type Type, string MemberName) invocation))
                            {
                                activity.SetTag("invocation", $"{invocation.Type.Name}.{invocation.MemberName}");
                            }

                            if (httpRequestMessage.Content is not { } content)
                                return;

                            byte[] contentByteArray = content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                            int contentLength = contentByteArray.Length;
                            if (!(contentLength > 0))
                                return;

                            activity.SetTag("http.request_content_length", contentLength);
                            if (traceInstrumentationCallbacks.ShouldSetRequestContentTag(activity, httpRequestMessage))
                            {
                                activity.SetTag("http.response_content", Encoding.UTF8.GetString(contentByteArray));
                            }
                        };

                        options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                        {
                            byte[] contentByteArray = httpResponseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                            int contentLength = contentByteArray.Length;
                            activity.SetTag("http.response_content_length", contentLength);
                            if (!httpResponseMessage.IsSuccessStatusCode && contentLength > 0 && traceInstrumentationCallbacks.ShouldSetResponseContentTag(activity, httpResponseMessage))
                            {
                                activity.SetTag("http.response_content", Encoding.UTF8.GetString(contentByteArray));
                            }
                        };

                        options.EnrichWithException = (activity, exception) =>
                        {
                            if (traceInstrumentationCallbacks.ShouldSetStackTraceTag(activity, exception))
                            {
                                activity.SetTag("stack_trace", exception.StackTrace);
                            }
                        };

                        options.FilterHttpRequestMessage = httpRequestMessage =>
                        {
                            string requestHost = httpRequestMessage.RequestUri!.Host;

                            foreach (string excludedHost in openTelemetryOptions.ExcludedHttpHosts)
                            {
                                if (excludedHost[0] == '.' && requestHost.EndsWith(excludedHost, StringComparison.Ordinal))
                                    return false;
                                if (requestHost == excludedHost)
                                    return false;
                            }

                            return true;
                        };
                    }

                    tracerProviderBuilder
                        .AddDiginsight()
                        .AddHttpClientInstrumentation(ConfigureHttpClientTraceInstrumentationOptions);
                }
            );
        }

        return services;
    }
}
