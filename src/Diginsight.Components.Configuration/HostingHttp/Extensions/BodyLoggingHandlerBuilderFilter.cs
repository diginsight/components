using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace Diginsight.Components.Configuration;

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