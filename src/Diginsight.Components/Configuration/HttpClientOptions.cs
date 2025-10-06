namespace Diginsight.Components;

public sealed class HttpClientOptions : IHttpClientOptions
{
    public Uri BaseUrl { get; init; }

    public string? ApimSubscriptionKey { get; init; }
}
