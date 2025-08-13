using Diginsight.Diagnostics;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;

namespace Diginsight.Components.Azure.Metrics
{
    public class ObservableFeedIterator<T> : FeedIterator<T>, IDisposable
    {
        private readonly FeedIterator<T> innerIterator;
        private readonly Container? container;
        private readonly QueryDefinition? queryDefinition;
        private readonly string? queryText; // For string-based queries
        //private readonly string? application;

        internal ObservableFeedIterator(FeedIterator<T> iterator, Container? container, QueryDefinition? queryDefinition = null) // string? queryText = null
        {
            innerIterator = iterator ?? throw new ArgumentNullException(nameof(iterator));
            this.container = container;
            this.queryDefinition = queryDefinition;
        }

        internal ObservableFeedIterator(FeedIterator<T> iterator, Container? container, string queryText = null) 
        {
            innerIterator = iterator ?? throw new ArgumentNullException(nameof(iterator));
            this.container = container;
            this.queryText = queryText;
        }

        public override bool HasMoreResults => innerIterator.HasMoreResults;

        public override async Task<FeedResponse<T>> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(ObservableFeedIterator<T>));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger,
                () => new {
                    query = queryDefinition?.QueryText ?? queryText, 
                    container = container?.Id,
                    database = container?.Database?.Id
                },
                logLevel: LogLevel.Trace);

            try
            {
                SetActivityTags(activity);

                var feedResponse = await innerIterator.ReadNextAsync(cancellationToken);
                logger.LogDebug("Query executed successfully. Retrieved {Count} documents of type '{Type}', RU consumed: {RequestCharge}", feedResponse.Count, typeof(T).Name, feedResponse.RequestCharge);

                if (activity != null && feedResponse.RequestCharge > 0)
                {
                    activity.SetTag("query_cost", feedResponse.RequestCharge);
                }

                return feedResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error executing CosmosDB query");
                throw;
            }
        }

        private void SetActivityTags(Activity? activity)
        {
            if (activity == null) return;

            var query = queryDefinition?.QueryText ?? queryText; 
            if (!string.IsNullOrEmpty(query)) { activity.SetTag("query", query); }

            if (container != null)
            {
                activity.SetTag("container", container.Id);

                if (container.Database != null)
                {
                    activity.SetTag("database", container.Database.Id);

                    if (container.Database.Client != null) { activity.SetTag("endpoint", container.Database.Client.Endpoint?.ToString()); }
                }
            }

            //// Set application information
            //if (!string.IsNullOrEmpty(application))
            //{
            //    activity.SetTag("application", application);
            //}
        }

        void IDisposable.Dispose() => innerIterator?.Dispose();
    }

    public class ObservableFeedIterator : FeedIterator, IDisposable
    {
        private readonly FeedIterator innerIterator;
        private readonly Container? container;
        private readonly QueryDefinition? queryDefinition;
        private readonly string? queryText; // For string-based queries

        internal ObservableFeedIterator( FeedIterator iterator, Container? container, QueryDefinition? queryDefinition = null)
        {
            innerIterator = iterator ?? throw new ArgumentNullException(nameof(iterator));
            this.container = container;
            this.queryDefinition = queryDefinition;
        }

        internal ObservableFeedIterator(FeedIterator iterator, Container? container, string queryText = null)
        {
            innerIterator = iterator ?? throw new ArgumentNullException(nameof(iterator));
            this.container = container;
            this.queryText = queryText;
        }

        public override bool HasMoreResults => innerIterator.HasMoreResults;

        public override async Task<ResponseMessage> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(ObservableFeedIterator));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger,
                () => new {
                    query = queryDefinition?.QueryText ?? queryText, 
                    container = container?.Id,
                    database = container?.Database?.Id
                },
                logLevel: LogLevel.Trace);

            try
            {
                SetActivityTags(activity);

                var responseMessage = await innerIterator.ReadNextAsync(cancellationToken);
                double requestCharge = 0;
                if (responseMessage.Headers?.RequestCharge != null) { requestCharge = responseMessage.Headers.RequestCharge; }

                logger.LogDebug("Query executed successfully. Retrieved documents, RU consumed: {RequestCharge}", requestCharge);

                // Set query cost tag for metric collection
                if (activity != null && requestCharge > 0)
                {
                    activity.SetTag("query_cost", requestCharge);
                }

                return responseMessage;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error executing CosmosDB query");
                throw;
            }
        }

        private void SetActivityTags(Activity? activity)
        {
            if (activity == null) return;

            var query = queryDefinition?.QueryText ?? queryText; 
            if (!string.IsNullOrEmpty(query)) { activity.SetTag("query", query); }

            if (container != null)
            {
                activity.SetTag("container", container.Id);

                if (container.Database != null)
                {
                    activity.SetTag("database", container.Database.Id);

                    if (container.Database.Client != null) { activity.SetTag("endpoint", container.Database.Client.Endpoint?.ToString()); }
                }
            }
        }

        void IDisposable.Dispose() => innerIterator?.Dispose();
    }
}
