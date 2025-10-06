using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Diginsight.Components.Configuration;

public static class HttpHostingExtensions
{
    public static IServiceCollection AddGlobalBodyLoggingHandler(this IServiceCollection services)
    {
        AddBodyLoggingHandlerCore(services);
        services.Configure<BodyLoggingHandlerClientNames>(static names => { names.Add("\0"); });

        return services;
    }

    public static IHttpClientBuilder AddBodyLoggingHandler(this IHttpClientBuilder builder)
    {
        IServiceCollection services = builder.Services;
        AddBodyLoggingHandlerCore(services);
        services.Configure<BodyLoggingHandlerClientNames>(names => { names.Add(builder.Name); });

        return builder;
    }

    private static void AddBodyLoggingHandlerCore(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, BodyLoggingHandlerBuilderFilter>());
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static bool HasTokenAcquirer(IServiceCollection services, TokenAcquirerKey serviceKey)
    //{
    //    return services.Any(
    //        x =>
    //            (x.ServiceType == typeof(ApplicationTokenAcquirer) || x.ServiceType == typeof(DelegatedTokenAcquirer)) &&
    //            serviceKey.Equals(x.ServiceKey)
    //    );
    //}

    //private sealed record TokenAcquirerKey(string ClientName);

    //public static IHttpClientBuilder AddApplicationPermissionAuthentication(
    //    this IHttpClientBuilder builder, Action<AuthenticatedClientOptions>? configureOptions = null
    //)
    //{
    //    string clientName = builder.Name;
    //    TokenAcquirerKey serviceKey = new(clientName);
    //    if (HasTokenAcquirer(builder.Services, serviceKey))
    //    {
    //        return builder;
    //    }

    //    builder.Services.AddKeyedSingleton(serviceKey, (sp, _) => ActivatorUtilities.CreateInstance<ApplicationTokenAcquirer>(sp, clientName));
    //    builder.AddHttpMessageHandler(sp => new ApplicationAuthenticationHandler(sp.GetRequiredKeyedService<ApplicationTokenAcquirer>(serviceKey)));
    //    if (configureOptions is not null)
    //    {
    //        builder.Services.Configure(clientName, configureOptions);
    //    }

    //    return builder;
    //}

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
