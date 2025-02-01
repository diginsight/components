#region using
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#endregion

namespace Diginsight.Components.Extensions;

public static class HttpExtensions
{
    public static async Task<HttpResponseMessage?> HttpSendAsync(
        this HttpClient httpClient,
        HttpMethod method, string url, object? requestBody, string description, CancellationToken cancellationToken
    )
    {
        // using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { method, url, requestBody, description } );

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

public static class HttpHostingExtensions
{
    public static IHttpClientBuilder AddApplicationPermissionAuthentication(
        this IHttpClientBuilder builder, Action<AuthenticatedClientOptions>? configureOptions = null
    )
    {
        string clientName = builder.Name;

        builder.Services.AddKeyedScoped(clientName, (sp, _) => ActivatorUtilities.CreateInstance<ApplicationAuthenticationHandler>(sp, clientName));
        builder.AddHttpMessageHandler(sp => sp.GetRequiredKeyedService<ApplicationAuthenticationHandler>(clientName));
        
        if (configureOptions is not null)
        {
            builder.Services.Configure(clientName, configureOptions);
        }

        return builder;
    }

    //public static IHttpClientBuilder AddDelegatedPermissionAuthentication(
    //    this IHttpClientBuilder builder, Action<AuthenticatedClientOptions>? configureOptions = null
    //)
    //{
    //    string clientName = builder.Name;
    //    TokenAcquirerKey serviceKey = new(clientName);
    //    if (HasTokenAcquirer(builder.Services, serviceKey))
    //    {
    //        return builder;
    //    }

    //    builder.Services.AddKeyedSingleton(serviceKey, (sp, _) => ActivatorUtilities.CreateInstance<DelegatedTokenAcquirer>(sp, clientName));
    //    builder.AddHttpMessageHandler(sp => new DelegatedAuthenticationHandler(sp.GetRequiredKeyedService<DelegatedTokenAcquirer>(serviceKey)));
    //    if (configureOptions is not null)
    //    {
    //        builder.Services.Configure(clientName, configureOptions);
    //    }

    //    return builder;
    //}
}