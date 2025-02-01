namespace Diginsight.Components;

public interface IHttpClientOptions
{
    Uri BaseUrl { get; }
    string? ApimSubscriptionKey { get; }
}
