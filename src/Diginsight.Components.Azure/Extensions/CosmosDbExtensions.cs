using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Diginsight.Diagnostics;
using Diginsight.Stringify;
using System.Diagnostics;

namespace Diginsight.Components.Azure;

public static class CosmosDbExtensions
{
    public static FeedIterator<T> GetItemQueryIteratorObservable<T>(this Container container, QueryDefinition queryDefinition, string? continuationToken = null, QueryRequestOptions? requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(T));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { queryDefinition, continuationToken, requestOptions });

        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class '{Type}' in database {Endpoint}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", queryDefinition.QueryText);

            return container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB query iterator for type {Type}", typeof(T).Name);
            throw;
        }
    }

    public static FeedIterator GetItemQueryStreamIteratorObservable(this Container container, string query, string? continuationToken = null, QueryRequestOptions? requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { query, continuationToken, requestOptions });

        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class 'Object' in database {Endpoint}, collection '{Collection}'", container.Database.Client.Endpoint, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", query);

            return container.GetItemQueryStreamIterator(query, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB stream query iterator");
            throw;
        }
    }

    public static FeedIterator GetItemQueryStreamIteratorObservable(this Container container, QueryDefinition queryDefinition, string? continuationToken = null, QueryRequestOptions? requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { queryDefinition, continuationToken, requestOptions });

        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class 'Object' in database {Endpoint}, collection '{Collection}'", container.Database.Client.Endpoint, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", queryDefinition.QueryText);

            return container.GetItemQueryStreamIterator(queryDefinition, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB stream query iterator");
            throw;
        }
    }

    public static async Task<ItemResponse<T>> UpsertItemObservableAsync<T>(this Container container, T item, PartitionKey? partitionKey = null, ItemRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(T));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { item, partitionKey, requestOptions });

        try
        {
            // Log connection and upsert item information
            logger.LogDebug("🔄 CosmosDB upsert for class '{Type}' in database {Endpoint}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Id);
            logger.LogDebug("🔄 entity:{entity}", item.Stringify());

            var response = await container.UpsertItemAsync(item, partitionKey, requestOptions, cancellationToken);
            
            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error upserting item in CosmosDB for type {Type}", typeof(T).Name);
            throw;
        }
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
