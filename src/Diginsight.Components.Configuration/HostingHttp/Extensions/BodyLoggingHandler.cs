using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Diginsight.Components.Configuration;

public sealed class BodyLoggingHandlerClientNames : HashSet<string>
{
    public BodyLoggingHandlerClientNames()
        : base(StringComparer.OrdinalIgnoreCase) { }
}

public sealed class BodyLoggingHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly IServiceProvider serviceProvider;
    private readonly HashSet<string> clientNames;

    public BodyLoggingHandlerBuilderFilter(IServiceProvider serviceProvider, IOptions<BodyLoggingHandlerClientNames> clientNames)
    {
        this.serviceProvider = serviceProvider;
        this.clientNames = clientNames.Value;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);

            string clientName = builder.Name ?? Microsoft.Extensions.Options.Options.DefaultName;
            if (clientNames.Contains("\0") || clientNames.Contains(clientName))
            {
                builder.AdditionalHandlers.Add(ActivatorUtilities.CreateInstance<BodyLoggingHandler>(serviceProvider, clientName));
            }
        };
    }
}

public sealed class BodyLoggingHandler : DelegatingHandler
{
    private readonly ILoggerFactory loggerFactory;
    private readonly string clientName;

    public BodyLoggingHandler(
        ILoggerFactory loggerFactory,
        string clientName
    )
    {
        this.loggerFactory = loggerFactory;
        this.clientName = clientName;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{clientName}.BodyLoggingHandler");

        if (request.Content is { } requestContent)
        {
            string requestString = await requestContent.ReadAsStringAsync(cancellationToken);
            int requestStringLength = requestString.Length;

            logger.LogDebug("Request body length: {RequestBodyLength:#,##0.00} KB", (double)requestStringLength / 1024);
            if (requestStringLength > 0)
            {
                if (logger.IsEnabled(LogLevel.Trace))
                {
                    var mediaType = request.Content?.Headers?.ContentType?.MediaType;
                    var logString = requestString;
                    if (mediaType == "application/json")
                    {
                        using var jsonDocument = JsonDocument.Parse(requestString);
                        logString = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = false });
                    }
                    logger.LogTrace("Request body: {RequestBody}", requestString);
                }
            }
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        HttpContent responseContent = response.Content;

        string responseString = await responseContent.ReadAsStringAsync(cancellationToken);
        int responseStringLength = responseString.Length;

        logger.LogDebug("Response body length: {ResponseBodyLength:#,##0.00} KB", (double)responseStringLength / 1024);
        if (responseStringLength > 0)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                var mediaType = response.Content?.Headers?.ContentType?.MediaType;
                var logString = responseString;
                if (mediaType == "application/json")
                {
                    using var jsonDocument = JsonDocument.Parse(responseString);
                    logString = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions { WriteIndented = false });
                }
                logger.LogTrace("Response body: {ResponseBody}", logString);
            }
        }

        return response;
    }
}
