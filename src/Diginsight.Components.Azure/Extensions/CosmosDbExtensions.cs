using Diginsight.Diagnostics;
using Diginsight.Stringify;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Diginsight.Components.Azure;

public static class CosmosDbExtensions
{
    public static FeedIterator GetItemQueryStreamIteratorObservable(this Container container, QueryDefinition queryDefinition, string? continuationToken = null, QueryRequestOptions? requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { queryDefinition, continuationToken, requestOptions });

        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class 'Object' in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", queryDefinition.QueryText);

            return container.GetItemQueryStreamIterator(queryDefinition, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB stream query iterator");
            throw;
        }
    }

    public static FeedIterator<T> GetItemQueryIteratorObservable<T>(this Container container, QueryDefinition queryDefinition, string? continuationToken = null, QueryRequestOptions? requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { queryDefinition, continuationToken, requestOptions });

        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class '{Type}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Database.Id, container.Id);
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
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { query, continuationToken, requestOptions });

        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class 'Object' in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", query);

            return container.GetItemQueryStreamIterator(query, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB stream query iterator");
            throw;
        }
    }
    public static FeedIterator<T> GetItemQueryIteratorObservable<T>(this Container container, string query = null, string continuationToken = null, QueryRequestOptions requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { query, continuationToken, requestOptions });
        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class '{Type}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", query);
            return container.GetItemQueryIterator<T>(query, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB query iterator for type {Type}", typeof(T).Name);
            throw;
        }
    }

    public static FeedIterator GetItemQueryStreamIteratorObservable(this Container container, FeedRange feedRange, QueryDefinition queryDefinition, string continuationToken, QueryRequestOptions requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { feedRange, queryDefinition, continuationToken, requestOptions });
        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class 'Object' in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", queryDefinition.QueryText);
            return container.GetItemQueryStreamIterator(feedRange, queryDefinition, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB stream query iterator");
            throw;
        }
    }

    public static FeedIterator<T> GetItemQueryIteratorObservable<T>(this Container container, FeedRange feedRange, QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { feedRange, queryDefinition, continuationToken, requestOptions });
        try
        {
            // Log connection and query information
            logger.LogDebug("🔍 CosmosDB query for class 'Object' in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 Query: \"{Query}\"", queryDefinition.QueryText);
            return container.GetItemQueryIterator<T>(feedRange, queryDefinition, continuationToken, requestOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB stream query iterator");
            throw;
        }
    }

    public static IOrderedQueryable<T> GetItemLinqQueryableObservable<T>(this Container container, bool allowSynchronousQueryExecution = false, string continuationToken = null, QueryRequestOptions requestOptions = null, CosmosLinqSerializerOptions linqSerializerOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { allowSynchronousQueryExecution, continuationToken, requestOptions, linqSerializerOptions });
        try
        {
            var queryable = container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution, continuationToken, requestOptions, linqSerializerOptions);
            logger.LogDebug("🔍 CosmosDB LINQ query for class '{Type}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 Query container: \"{Container}\"", queryable.ToString());

            return queryable;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB LINQ query for type {Type}", typeof(T).Name);
            throw;
        }
    }

    public static IQueryable<T> GetItemLinqQueryableObservable<T>(this Container container, Func<IQueryable<T>, IQueryable<T>> trasform, bool allowSynchronousQueryExecution = false, string continuationToken = null, QueryRequestOptions requestOptions = null, CosmosLinqSerializerOptions linqSerializerOptions = null)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { allowSynchronousQueryExecution, continuationToken, requestOptions, linqSerializerOptions });
        try
        {
            var collectionQueryable = container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution, continuationToken, requestOptions, linqSerializerOptions);
            var queryable = trasform(collectionQueryable);

            logger.LogDebug("🔍 CosmosDB LINQ query for class '{Type}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 Query: {Query}", queryable.ToString());

            return queryable;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB LINQ query for type {Type}", typeof(T).Name);
            throw;
        }
    }

    public static async Task<ItemResponse<T>> UpsertItemObservableAsync<T>(this Container container, T item, PartitionKey? partitionKey = null, ItemRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { item, partitionKey, requestOptions });

        try
        {
            // Log connection and upsert item information
            logger.LogDebug("🔄 CosmosDB upsert for class '{Type}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Database.Id, container.Id);
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

    public static async Task<ResponseMessage> UpsertItemStreamObservableAsync(this Container container, Stream streamPayload, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { streamPayload, partitionKey, requestOptions });
        try
        {
            // Log connection and upsert item information
            logger.LogDebug("🔄 CosmosDB upsert item in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔄 partitionKey:{partitionKey}", partitionKey.ToString());
            var response = await container.UpsertItemStreamAsync(streamPayload, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error upserting item in CosmosDB");
            throw;
        }
    }

    public static async Task<ResponseMessage> CreateItemStreamObservableAsync(this Container container, Stream streamPayload, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { streamPayload, partitionKey, requestOptions });
        try
        {
            // Log connection and create item information
            logger.LogDebug("📦 CosmosDB create item in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("📦 partitionKey:{partitionKey}", partitionKey.ToString());
            var response = await container.CreateItemStreamAsync(streamPayload, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating item in CosmosDB");
            throw;
        }
    }
    public static async Task<ItemResponse<T>> CreateItemObservableAsync<T>(this Container container, T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { item, partitionKey, requestOptions });
        try
        {
            // Log connection and create item information
            logger.LogDebug("📦 CosmosDB create item for class '{Type}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("📦 entity:{entity}", item.Stringify());
            var response = await container.CreateItemAsync(item, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating item in CosmosDB for type {Type}", typeof(T).Name);
            throw;
        }
    }

    public static async Task<ResponseMessage> ReadItemStreamObservableAsync(this Container container, string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { id, partitionKey, requestOptions });
        try
        {
            // Log connection and read item information
            logger.LogDebug("🔍 CosmosDB read item for id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 partitionKey:{partitionKey}", partitionKey.ToString());
            var response = await container.ReadItemStreamAsync(id, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error reading item from CosmosDB for id {Id}", id);
            throw;
        }
    }

    public static async Task<ItemResponse<T>> ReadItemObservableAsync<T>(this Container container, string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { id, partitionKey, requestOptions });
        try
        {
            // Log connection and read item information
            logger.LogDebug("🔍 CosmosDB read item for id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔍 partitionKey:{partitionKey}", partitionKey.ToString());
            var response = await container.ReadItemAsync<T>(id, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error reading item from CosmosDB for id {Id}", id);
            throw;
        }
    }

    public static async Task<ResponseMessage> ReadManyItemsStreamObservableAsync(this Container container, IReadOnlyList<(string id, PartitionKey partitionKey)> items, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { items, readManyRequestOptions });
        try
        {
            // Log connection and read many items information
            logger.LogDebug("🔍 CosmosDB read many items in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            var response = await container.ReadManyItemsStreamAsync(items, readManyRequestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error reading many items from CosmosDB");
            throw;
        }
    }
    public static async Task<FeedResponse<T>> ReadManyItemsObservableAsync<T>(this Container container, IReadOnlyList<(string id, PartitionKey partitionKey)> items, ReadManyRequestOptions readManyRequestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { items, readManyRequestOptions });

        try
        {
            // Log connection and read many items information
            logger.LogDebug("🔍 CosmosDB read many items for class '{Type}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            var response = await container.ReadManyItemsAsync<T>(items, readManyRequestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error reading many items from CosmosDB for type {Type}", typeof(T).Name);
            throw;
        }
    }

    public static async Task<ItemResponse<T>> PatchItemObservableAsync<T>(this Container container, string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { id, partitionKey, patchOperations, requestOptions });
        try
        {
            // Log connection and patch item information
            logger.LogDebug("✂️ CosmosDB patch item for class '{Type}' with id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("✂️ partitionKey:{partitionKey}", partitionKey.ToString());
            logger.LogDebug("✂️ patchOperations:{patchOperations}", string.Join(", ", patchOperations.Select(po => po.ToString())));
            var response = await container.PatchItemAsync<T>(id, partitionKey, patchOperations, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error patching item in CosmosDB for type {Type} with id {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public static async Task<ResponseMessage> PatchItemStreamObservableAsync(this Container container, string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations, PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { id, partitionKey, patchOperations, requestOptions });
        try
        {
            // Log connection and patch item information
            logger.LogDebug("✂️ CosmosDB patch item for id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("✂️ partitionKey:{partitionKey}", partitionKey.ToString());
            logger.LogDebug("✂️ patchOperations:{patchOperations}", string.Join(", ", patchOperations.Select(po => po.ToString())));
            var response = await container.PatchItemStreamAsync(id, partitionKey, patchOperations, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error patching item in CosmosDB for id {Id}", id);
            throw;
        }
    }

    public static async Task<ResponseMessage> DeleteItemStreamObservableAsync(this Container container, string id, PartitionKey partitionKey, ItemRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { id, partitionKey, requestOptions });

        try
        {
            // Log connection and delete item information
            logger.LogDebug("🗑️ CosmosDB delete item for id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🗑️ partitionKey:{partitionKey}", partitionKey.ToString());

            var response = await container.DeleteItemStreamAsync(id, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error deleting item from CosmosDB for id {Id}", id);
            throw;
        }
    }

    public static async Task<ItemResponse<T>> DeleteItemObservableAsync<T>(this Container container, string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { id, partitionKey, requestOptions });
        try
        {
            // Log connection and delete item information
            logger.LogDebug("🗑️ CosmosDB delete item for class '{Type}' with id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🗑️ partitionKey:{partitionKey}", partitionKey.ToString());
            var response = await container.DeleteItemAsync<T>(id, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error deleting item from CosmosDB for type {Type} with id {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public static async Task<ResponseMessage> DeleteAllItemsByPartitionKeyStreamObservableAsync(this Container container, PartitionKey partitionKey, RequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, requestOptions });
        try
        {
            // Log connection and delete all items by partition key information
            logger.LogDebug("🗑️ CosmosDB delete all items by partition key in database {Endpoint}, container {Container}, collection '{Collection}'", partitionKey.ToString(), requestOptions?.ToString());
            var response = await container.DeleteAllItemsByPartitionKeyStreamAsync(partitionKey, requestOptions, cancellationToken);
            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error deleting all items by partition key from CosmosDB");
            throw;
        }
    }

    public static async Task<ResponseMessage> ReplaceItemStreamObservableAsync(this Container container, Stream streamPayload, string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { streamPayload, id, partitionKey, requestOptions });
        try
        {
            // Log connection and replace item information
            logger.LogDebug("🔄 CosmosDB replace item for id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔄 partitionKey:{partitionKey}", partitionKey.ToString());
            var response = await container.ReplaceItemStreamAsync(streamPayload, id, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error replacing item in CosmosDB for id {Id}", id);
            throw;
        }
    }
    public static async Task<ItemResponse<T>> ReplaceItemObservableAsync<T>(this Container container, T item, string id, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { item, id, partitionKey, requestOptions });
        try
        {
            // Log connection and replace item information
            logger.LogDebug("🔄 CosmosDB replace item for class '{Type}' with id '{Id}' in database {Endpoint}, container {Container}, collection '{Collection}'", typeof(T).Name, id, container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("🔄 entity:{entity}", item.Stringify());
            var response = await container.ReplaceItemAsync(item, id, partitionKey, requestOptions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error replacing item in CosmosDB for type {Type} with id {Id}", typeof(T).Name, id);
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

    public static async Task<FeedResponse<T>> ReadNextObservableAsync<T>(this FeedIterator<T> feedIterator, CancellationToken cancellationToken = default(CancellationToken))
    {
        var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger(typeof(CosmosDbExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { feedIterator }, logLevel: LogLevel.Trace);
        try
        {
            var feedResponse = await feedIterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            logger.LogDebug("Query executed successfully. Retrieved {Count} documents of type '{Type}', RU consumed: {RequestCharge}", feedResponse.Count, typeof(T).Name, feedResponse.RequestCharge);

            return feedResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creating CosmosDB stream query iterator");
            throw;
        }
    }


}
