using System.Net.Http.Headers;

namespace Diginsight.Components;

internal static class HttpHelper
{
    private static readonly Type T = typeof(HttpHelper);
    //private ILogger<HttpHelper> logger;

    public static HttpClient GetHttpClient(string mediaType, string authToken = null)
    {
        //using (var scope = TraceLogger.BeginMethodScope(T, new { mediaType }))
        //{
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        if (authToken is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        //scope.Result = client;
        return client;
        //}
    }
}
