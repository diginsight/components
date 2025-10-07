using Diginsight.Diagnostics;
using Diginsight.Stringify;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;

namespace Diginsight.Components.Azure;

public class ObservableTransactionalBatch : TransactionalBatch
{
    private readonly TransactionalBatch innerBatch;
    private readonly Container? container;
    private readonly PartitionKey? partitionKey;
    private readonly ILogger logger;

    internal ObservableTransactionalBatch(TransactionalBatch batch, PartitionKey? partitionKey, Container? container)
    {
        innerBatch = batch ?? throw new ArgumentNullException(nameof(batch));
        this.container = container;
        this.partitionKey = partitionKey;

        var loggerFactory = Observability.LoggerFactory;
        logger = loggerFactory.CreateLogger(typeof(ObservableTransactionalBatch));
    }

    public override TransactionalBatch CreateItem<T>(T item, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB CreateItem for transactionalBatch with item type '{Item}'", item.Stringify());
        return innerBatch.CreateItem(item, requestOptions);
    }

    public override TransactionalBatch CreateItemStream(Stream streamPayload, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB CreateItemStream for transactionalBatch with stream payload");
        return innerBatch.CreateItemStream(streamPayload, requestOptions);
    }

    public override TransactionalBatch ReadItem(string id, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB ReadItem for transactionalBatch with id '{Id}'", id);
        return innerBatch.ReadItem(id, requestOptions);
    }

    public override TransactionalBatch UpsertItem<T>(T item, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB UpsertItem for transactionalBatch with item type '{Item}'", item.Stringify());
        return innerBatch.UpsertItem(item, requestOptions);
    }

    public override TransactionalBatch UpsertItemStream(Stream streamPayload, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB UpsertItemStream for transactionalBatch with stream payload");
        return innerBatch.UpsertItemStream(streamPayload, requestOptions);
    }

    public override TransactionalBatch ReplaceItem<T>(string id, T item, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB ReplaceItem for transactionalBatch with id '{Id}' and item type '{Item}'", id, item.Stringify());
        return innerBatch.ReplaceItem(id, item, requestOptions);
    }

    public override TransactionalBatch ReplaceItemStream(string id, Stream streamPayload, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB ReplaceItemStream for transactionalBatch with id '{Id}' and stream payload", id);
        return innerBatch.ReplaceItemStream(id, streamPayload, requestOptions);
    }

    public override TransactionalBatch DeleteItem(string id, TransactionalBatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB DeleteItem for transactionalBatch with id '{Id}'", id);
        return innerBatch.DeleteItem(id, requestOptions);
    }

    public override TransactionalBatch PatchItem(string id, IReadOnlyList<PatchOperation> patchOperations, TransactionalBatchPatchItemRequestOptions? requestOptions = null)
    {
        logger.LogDebug("📦 CosmosDB PatchItem for transactionalBatch with id '{Id}' and {PatchCount} patch operations", id, patchOperations?.Count ?? 0);
        return innerBatch.PatchItem(id, patchOperations, requestOptions);
    }

    public override async Task<TransactionalBatchResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new {
            container = container?.Id,
            database = container?.Database?.Id
        });

        try
        {
            SetActivityTags(activity);

            logger.LogDebug("⚡ CosmosDB Execute for 'transactionalBatch' in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("⚡ transactionalBatch: PartitionKey '{PartitionKey}'", partitionKey?.ToString()); // , Operations '{Operations}'

            var response = await innerBatch.ExecuteAsync(cancellationToken);

            logger.LogDebug("⚡✅ CosmosDB ExecuteAsync transactionalBatch completed. Status: {StatusCode}, RU consumed: {RequestCharge}, Items: {Count}", response.StatusCode, response.RequestCharge, response.Count);
            if (activity is not null && response.RequestCharge > 0)
            {
                activity.SetTag("query", $"Batch({partitionKey})");
                activity.SetTag("container", container.Id);
                activity.SetTag("database", container.Database.Id);

                activity.SetTag("query_cost", response.RequestCharge);
                activity.SetTag("query_status", response.StatusCode.ToString());
                activity.SetTag("query_count", response.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error executing CosmosDB transactional batch");
            throw;
        }
    }

    public override async Task<TransactionalBatchResponse> ExecuteAsync(TransactionalBatchRequestOptions? requestOptions, CancellationToken cancellationToken = default)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new {
            container = container?.Id,
            database = container?.Database?.Id,
            hasRequestOptions = requestOptions is not null
        });

        try
        {
            SetActivityTags(activity);

            logger.LogDebug("⚡ CosmosDB Execute for 'transactionalBatch' in database {Endpoint}, container {Container}, collection '{Collection}'", container.Database.Client.Endpoint, container.Database.Id, container.Id);
            logger.LogDebug("⚡ CosmosDB transactionalBatch: PartitionKey '{PartitionKey}'", partitionKey?.ToString()); // , Operations '{Operations}'

            var response = await innerBatch.ExecuteAsync(requestOptions, cancellationToken);

            logger.LogDebug("⚡✅ CosmosDB ExecuteAsync transactionalBatch with options completed. Status: {StatusCode}, RU consumed: {RequestCharge}, Items: {Count}", response.StatusCode, response.RequestCharge, response.Count);
            if (activity is not null && response.RequestCharge > 0)
            {
                activity.SetTag("query", $"Batch({partitionKey})");
                activity.SetTag("container", container.Id);
                activity.SetTag("database", container.Database.Id);

                activity.SetTag("query_cost", response.RequestCharge);
                activity.SetTag("query_status", response.StatusCode.ToString());
                activity.SetTag("query_count", response.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error executing CosmosDB transactional batch with options");
            throw;
        }
    }

    private void SetActivityTags(Activity? activity)
    {
        if (activity is null) return;

        if (container is not null)
        {
            activity.SetTag("container", container.Id);

            if (container.Database is not null)
            {
                activity.SetTag("database", container.Database.Id);

                if (container.Database.Client is not null)
                {
                    activity.SetTag("endpoint", container.Database.Client.Endpoint?.ToString());
                }
            }
        }
    }
}
