using Diginsight.Components.Azure.Metrics;
using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Runtime.CompilerServices;

namespace Diginsight.Components.Azure;

public static class ServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddCosmosDbQueryCostMetricRecorder(this IServiceCollection services)
    {
        return services.AddCosmosDbQueryCostMetricRecorder<QueryCostMetricRecorderRegistration>();
    }

    /// <summary>
    /// Adds CosmosDB query cost metric recording to the service collection.
    /// This will register a metric recorder that captures query costs from CosmosDB operations.
    /// </summary>
    /// <param name="services">The service collection to add the metric recorder to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCosmosDbQueryCostMetricRecorder<TRegistration>(this IServiceCollection services)
        where TRegistration : QueryCostMetricRecorderRegistration
    {
        services
            .AddClassAwareOptions()
            .AddActivityListenersAdder()
            .AddMetrics();

        services.TryAddSingleton<QueryCostMetricRecorder>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, TRegistration>());

        return services;
    }
}
