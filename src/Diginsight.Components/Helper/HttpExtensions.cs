#region using
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
#endregion

namespace Diginsight.Components;

public static class HttpExtensions
{
    public static async Task<HttpResponseMessage?> HttpSendAsync(
        this HttpClient httpClient,
        HttpMethod method, string url, object? requestBody, string description, CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage requestMessage = new(method, url);
        if (requestBody != null)
        {
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        try
        {
            return await httpClient.SendAsync(requestMessage, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException oce || oce.CancellationToken != cancellationToken)
        {
            //logger.LogError(exception, "Error calling {Description}", description);
            return null;
        }

        //if (responseMessage is null) return default(T);
        //if (responseMessage.IsSuccessStatusCode == false)
        //{
        //    string responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        //    var error = JsonConvert.DeserializeObject<T>(responseBody);
        //    throw new InvalidOperationException($"http request failed with {responseMessage.StatusCode}");
        //}
    }

    public static async Task<TResult?> GetAsync<TResult>(this HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        string responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (responseMessage.IsSuccessStatusCode == false || string.IsNullOrEmpty(responseBody)) { return default; }

        return JsonConvert.DeserializeObject<TResult>(responseBody);
    }

}
