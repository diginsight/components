#region using
//using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Diginsight.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
#endregion

namespace Diginsight.Components;

public static class HttpExtensions
{
    private static readonly Type T = typeof(HttpExtensions);

    private static readonly HttpRequestOptionsKey<(Type Type, string MemberName)> InvocationOptionKey = new("Invocation");
    private static readonly HttpRequestOptionsKey<UserAssertion> UserAssertionOptionKey = new("UserAssertion");

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetInvocation(this HttpRequestMessage request, [CallerMemberName] string callerMemberName = "")
    {
        request.SetInvocation(RuntimeUtils.GetCallerType(), callerMemberName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetInvocation(this HttpRequestMessage request, Type type, string memberName)
    {
        request.Options.Set(InvocationOptionKey, (type, memberName));
    }

    public static bool TryGetInvocation(
        this HttpRequestMessage request, [NotNullWhen(true)] out Type? invocationType, [NotNullWhen(true)] out string? invocationMemberName
    )
    {
        if (request.Options.TryGetValue(InvocationOptionKey, out var invocation))
        {
            invocationType = invocation.Type;
            invocationMemberName = invocation.MemberName;
            return true;
        }
        else
        {
            invocationType = null;
            invocationMemberName = null;
            return false;
        }
    }

    public static void SetUserAssertion(this HttpRequestMessage request, UserAssertion userAssertion)
    {
        request.Options.Set(UserAssertionOptionKey, userAssertion);
    }

    public static bool TryGetUserAssertion(this HttpRequestMessage request, [NotNullWhen(true)] out UserAssertion? userAssertion)
    {
        return request.Options.TryGetValue(UserAssertionOptionKey, out userAssertion);
    }

    public static async Task<HttpResponseMessage?> SendAsync(
        this HttpClient httpClient,
        HttpMethod method, string url, object? requestBody, string description, CancellationToken cancellationToken
    )
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { method, url, requestBody, description });

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

    public static async Task<TResult?> GetAsync<TResult>(
        this HttpResponseMessage responseMessage, 
        CancellationToken cancellationToken)
    {
        string responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (responseMessage.IsSuccessStatusCode == false || string.IsNullOrEmpty(responseBody)) { return default; }

        return JsonConvert.DeserializeObject<TResult>(responseBody);
    }

}
