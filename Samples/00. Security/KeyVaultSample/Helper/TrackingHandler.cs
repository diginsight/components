using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultSample
{
    internal sealed class LatencyTrackingHandler : DelegatingHandler
    {
        public static readonly HttpRequestOptionsKey<Options> LatencyTrackingOptionKey = new("latencyTracking");

        //private readonly ITestService testService;
        private readonly ILogger<LatencyTrackingHandler> logger;
        private readonly IConfiguration configuration;

        public LatencyTrackingHandler(
            ILogger<LatencyTrackingHandler> logger,
            //ITestService testService,
            IConfiguration configuration)
        {
            //this.testService = testService;
            this.configuration = configuration;
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var localScope = logger.BeginMethodScope(() => new { request });

            if (!request.Options.TryGetValue(LatencyTrackingOptionKey, out Options? lto))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            (StrongBox<float?> latencyWarningThresholdBox, CodeSectionScope scope) = lto;
            localScope.LogDebug(() => new { latencyWarningThreshold = latencyWarningThresholdBox.Value });

            //Action<TestResult> setResult = tr => { tr.Action = $"{request.Method} {request.RequestUri}"; };

            var traceRequestBody = configuration.GetValue("AppSettings:TraceRequestBody", true);
            if (traceRequestBody)
            {
                string? requestString = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
                if (requestString is not null)
                {
                    localScope.LogDebug($"Request body ({(double)requestString.Length / 1024:#,##0} KB): {requestString}", null, new Dictionary<string, object>() { { "MaxMessageLen", 0 } });
                }

                //setResult += tr =>
                //{
                //    tr.Payload = requestString;
                //    tr.PayloadSize = (requestString?.Length ?? 0).ToString();
                //};
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                var responseContent = response.Content;
                var traceResponseBody = configuration.GetValue("AppSettings:TraceResponseBody", !response.IsSuccessStatusCode);
                //if (traceResponseBody)
                //{

                string? responseString = traceResponseBody ? await responseContent.ReadAsStringAsync(cancellationToken) : null;
                if (response.IsSuccessStatusCode)
                {
                    if (traceResponseBody)
                    {
                        localScope.LogDebug($"Response body ({(responseString?.Length ?? 0) / 1024:#,##0} KB): {responseString}", null, new Dictionary<string, object>() { { "MaxMessageLen", 0 } });
                    }
                    else
                    {
                        var contentLength = response.Content.Headers.ContentLength ?? 0;
                        localScope.LogDebug($"Response body length ({(double)contentLength / 1024:#,##0} KB)", null, new Dictionary<string, object>() { { "MaxMessageLen", 0 } });
                    }
                }
                else
                {
                    responseString ??= await responseContent.ReadAsStringAsync(cancellationToken);
                    localScope.LogDebug($"Response body ({(double)responseString.Length / 1024:#,##0} KB): {responseString}", null, new Dictionary<string, object>() { { "MaxMessageLen", 0 } });
                }

                //setResult += tr => { tr.Response = responseString; };
                //}
                //else
                //{
                //    var contentLength = response?.Content?.Headers?.ContentLength ?? 0;
                //    if (response.IsSuccessStatusCode)
                //    {
                //        scopeLocal.LogDebug($"Response body ({(double)contentLength / 1024:#,##0} KB)", null, new Dictionary<string, object>() { { "MaxMessageLen", 0 } });
                //    }
                //    else
                //    {
                //        string responseString = await responseContent.ReadAsStringAsync(cancellationToken);
                //        scopeLocal.LogDebug($"Response body ({(double)responseString.Length / 1024:#,##0} KB): {responseString}", null, new Dictionary<string, object>() { { "MaxMessageLen", 0 } });
                //    }
                //}

                //setResult += tr =>
                //{
                //    tr.Result = response.StatusCode.ToString("G");
                //    tr.ResponseSize = response.Content.Headers.ContentLength ?? 0;
                //};

                return response;
            }
            catch (Exception exception)
            {
                //setResult += tr => { tr.Exception = exception.ToString(); };

                throw;
            }
            finally
            {
                //latencyWarningThresholdBox.Value = await testService
                //    .CheckCallResultAsync(scope, latencyWarningThresholdBox.Value, setResult)
                //    .ConfigureAwait(false);
            }
        }

        public sealed record Options(StrongBox<float?> LatencyWarningThresholdBox, CodeSectionScope Scope);
    }
}
