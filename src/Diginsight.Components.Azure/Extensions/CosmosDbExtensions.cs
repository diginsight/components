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

    public static async IAsyncEnumerable<T> GetAsyncItems<T>(this FeedIterator<T> feedIterator)
    {
        while (feedIterator.HasMoreResults)
        {
            foreach (var item in await feedIterator.ReadNextAsync())
            {
                yield return item;
            }
        }
    }

    public static async Task<IEnumerable<T>> GetItemsAsync<T>(this FeedIterator<T> feedIterator)
    {
        var items = await feedIterator.GetAsyncItems().ToListAsync(); 
        return items;
    }

}
