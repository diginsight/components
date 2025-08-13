using Diginsight.Components.Azure.Metrics;
using Diginsight.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Diginsight.Components.Azure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CosmosDB query cost metric recording to the service collection.
    /// This will register a metric recorder that captures query costs from CosmosDB operations.
    /// </summary>
    /// <param name="services">The service collection to add the metric recorder to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCosmosDbQueryCostMetrics(this IServiceCollection services)
    {
        services.AddSingleton<QueryCostMetricRecorder>();
        services.AddSingleton<IActivityListenerRegistration, QueryCostMetricRecorderRegistration>();
        
        return services;
    }
}
