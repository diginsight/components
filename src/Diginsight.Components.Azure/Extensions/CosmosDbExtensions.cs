using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Diginsight.Components.Azure;


public static class CosmosDbExtensions
{
    public static FeedIterator<T> GetItemQueryIteratorObservable<T>(this Container container, QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
    {
        //var loggerFactory = ObservabilityHelper.LoggerFactory;
        //var logger = loggerFactory.CreateLogger(T);
        //using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { method, url, requestBody, description });

        return container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
    }
}
