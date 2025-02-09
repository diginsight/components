#region using
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#endregion

namespace Diginsight.Components;

public static class HttpHostingExtensions
{
    private static readonly Type T = typeof(HttpHostingExtensions);

    public static IHttpClientBuilder AddApplicationPermissionAuthentication(
        this IHttpClientBuilder builder, Action<AuthenticatedClientOptions>? configureOptions = null
    )
    {
        var loggerFactory = ObservabilityHelper.LoggerFactory;
        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { builder, configureOptions });

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