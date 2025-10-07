using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Diginsight.Components.Configuration;

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
                    logger.LogTrace("Request body: {RequestBody}", logString);
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
