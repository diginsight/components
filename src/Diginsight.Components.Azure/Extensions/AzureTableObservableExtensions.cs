using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure.Data.Tables.Sas;
using Diginsight.Diagnostics;
using Diginsight.Stringify;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq.Expressions;

namespace Diginsight.Components.Azure.Extensions
{
    public static class AzureTableObservableExtensions
    {
        public static Response AddEntityObservable<T>(this TableClient tableClient, T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity });

            var response = tableClient.AddEntity(entity, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response> AddEntityObservableAsync<T>(this TableClient tableClient, T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity });

            var tableUri = tableClient.Uri;
            var tableName = tableClient.Name;
            var operationName = activity?.OperationName;
            logger.LogDebug("➕ AzureTable {operationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
            logger.LogDebug("➕ partitionKey:{PartitionKey}, rowKey:{RowKey}, entity:{Entity}", entity.PartitionKey, entity.RowKey, entity.Stringify());

            var response = await tableClient.AddEntityAsync(entity, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static Response<TableItem> CreateObservable(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = tableClient.Create(cancellationToken);
            activity?.SetOutput(response);
            return response;
        }

        public static async Task<Response<TableItem>> CreateObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = await tableClient.CreateAsync(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static Response<TableItem> CreateIfNotExistsObservable(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = tableClient.CreateIfNotExists(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static async Task<Response<TableItem>> CreateIfNotExistsObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = await tableClient.CreateIfNotExistsAsync(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static Response DeleteObservable(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = tableClient.Delete(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static async Task<Response> DeleteObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = await tableClient.DeleteAsync(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static Response DeleteEntityObservable(this TableClient tableClient, string partitionKey, string rowKey, ETag ifMatch = default(ETag), CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, ifMatch });

            var response = tableClient.DeleteEntity(partitionKey, rowKey, ifMatch, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static Response DeleteEntityObservable(this TableClient tableClient, ITableEntity entity, ETag ifMatch = default(ETag), CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch });

            var response = tableClient.DeleteEntity(entity, ifMatch, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static async Task<Response> DeleteEntityObservableAsync(this TableClient tableClient, string partitionKey, string rowKey, ETag ifMatch = default(ETag), CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, ifMatch });

            var tableUri = tableClient.Uri;
            var tableName = tableClient.Name;
            var operationName = activity?.OperationName;
            logger.LogDebug("🗑️ AzureTable {operationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, "object", tableName, tableUri, "Standard");
            logger.LogDebug("🗑️ partitionKey:{partitionKey}, rowKey:{rowKey}", partitionKey, rowKey);

            var response = await tableClient.DeleteEntityAsync(partitionKey, rowKey, ifMatch, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static async Task<Response> DeleteEntityObservableAsync(this TableClient tableClient, ITableEntity entity, ETag ifMatch = default(ETag), CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch });

            var response = await tableClient.DeleteEntityAsync(entity, ifMatch, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static Uri GenerateSasUriObservable(this TableClient tableClient, TableSasPermissions permissions, DateTimeOffset expiresOn)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, expiresOn });

            var response = tableClient.GenerateSasUri(permissions, expiresOn);

            activity?.SetOutput(response);
            return response;
        }

        public static Uri GenerateSasUriObservable(this TableClient tableClient, TableSasBuilder builder)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { builder });

            var response = tableClient.GenerateSasUri(builder);

            activity?.SetOutput(response);
            return response;
        }

        public static Response<IReadOnlyList<TableSignedIdentifier>> GetAccessPoliciesObservable(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = tableClient.GetAccessPolicies(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response<IReadOnlyList<TableSignedIdentifier>>> GetAccessPoliciesObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = await tableClient.GetAccessPoliciesAsync(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Response<T> GetEntityObservable<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });

            var response = tableClient.GetEntity<T>(partitionKey, rowKey, select, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response<T>> GetEntityObservableAsync<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });

            var tableUri = tableClient.Uri;
            var tableName = tableClient.Name;
            var operationName = activity?.OperationName;
            logger.LogDebug("🔍 AzureTable {operationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
            logger.LogDebug("🔍 partitionKey:{PartitionKey}, rowKey:{RowKey}", partitionKey, rowKey);

            var response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey, select, cancellationToken);

            activity?.SetOutput(response);
            return response;

        }
        public static NullableResponse<T> GetEntityIfExistsObservable<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });
            var response = tableClient.GetEntityIfExists<T>(partitionKey, rowKey, select, cancellationToken);
            activity?.SetOutput(response);
            return response;
        }
        public static async Task<NullableResponse<T>> GetEntityIfExistsObservableAsync<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });

            var response = await tableClient.GetEntityIfExistsAsync<T>(partitionKey, rowKey, select, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static TableSasBuilder GetSasBuilderObservable(this TableClient tableClient, TableSasPermissions permissions, DateTimeOffset expiresOn)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, expiresOn });

            var response = tableClient.GetSasBuilder(permissions, expiresOn);

            activity?.SetOutput(response);
            return response;
        }
        public static TableSasBuilder GetSasBuilderObservable(this TableClient tableClient, string rawPermissions, DateTimeOffset expiresOn)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { rawPermissions, expiresOn });

            var response = tableClient.GetSasBuilder(rawPermissions, expiresOn);

            activity?.SetOutput(response);
            return response;
        }
        public static Pageable<T> QueryObservable<T>(this TableClient tableClient, Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

            var response = tableClient.Query<T>(filter, maxPerPage, select, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Pageable<T> QueryObservable<T>(this TableClient tableClient, string filter = null, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

            var response = tableClient.Query<T>(filter, maxPerPage, select, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static AsyncPageable<T> QueryObservableAsync<T>(this TableClient tableClient, Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

            var response = tableClient.QueryAsync<T>(filter, maxPerPage, select, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static AsyncPageable<T> QueryObservableAsync<T>(this TableClient tableClient, string filter = null, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

            var tableUri = tableClient.Uri;
            var tableName = tableClient.Name;
            var operationName = activity?.OperationName;

            logger.LogDebug("🔍 AzureTable {operationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
            logger.LogDebug("🔍 filter:{Filter}, top:{Top}, select:{Select}", filter ?? "null", maxPerPage?.ToString() ?? "null", select != null ? string.Join(",", select) : "null");

            var response = tableClient.QueryAsync<T>(filter, maxPerPage, select, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Response SetAccessPolicyObservable(this TableClient tableClient, IEnumerable<TableSignedIdentifier> tableAcl, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableAcl });

            var response = tableClient.SetAccessPolicy(tableAcl, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static async Task<Response> SetAccessPolicyObservableAsync(this TableClient tableClient, IEnumerable<TableSignedIdentifier> tableAcl, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableAcl });

            var response = await tableClient.SetAccessPolicyAsync(tableAcl, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Response<IReadOnlyList<Response>> SubmitTransactionObservable(this TableClient tableClient, IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { transactionActions });

            var response = tableClient.SubmitTransaction(transactionActions, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response<IReadOnlyList<Response>>> SubmitTransactionObservableAsync(this TableClient tableClient, IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { transactionActions });

            var tableUri = tableClient.Uri;
            var tableName = tableClient.Name;
            var operationName = activity?.OperationName;
            logger.LogDebug("➕ AzureTable {operationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, transactionActions.FirstOrDefault()?.Entity?.GetType()?.Name, tableName, tableUri, "Standard");
            logger.LogDebug("➕ transactionActions:{transactionActions}", transactionActions.Stringify());

            var response = await tableClient.SubmitTransactionAsync(transactionActions, cancellationToken);

            activity?.SetOutput(response);
            return response;

        }
        public static Response UpdateEntityObservable<T>(this TableClient tableClient, T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch, mode });

            var response = tableClient.UpdateEntity(entity, ifMatch, mode, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response> UpdateEntityObservableAsync<T>(this TableClient tableClient, T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch, mode });

            var tableUri = tableClient.Uri;
            var tableName = tableClient.Name;
            var operationName = activity?.OperationName;
            logger.LogDebug("🔄 AzureTable {operationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
            logger.LogDebug("🔄 existingEntity.ETag:{existingEntityETag}, entity:{entity}", ETag.All, entity.Stringify());


            var response = await tableClient.UpdateEntityAsync(entity, ifMatch, mode, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Response UpsertEntityObservable<T>(this TableClient tableClient, T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, mode });

            var response = tableClient.UpsertEntity(entity, mode, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response> UpsertEntityObservableAsync<T>(this TableClient tableClient, T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default(CancellationToken)) where T : ITableEntity
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, mode });

            var response = await tableClient.UpsertEntityAsync(entity, mode, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }

        public static Response<TableItem> CreateTableObservable(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

            var response = tableServiceClient.CreateTable(tableName, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response<TableItem>> CreateTableObservableAsync(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

            var response = await tableServiceClient.CreateTableAsync(tableName, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Response<TableItem> CreateTableIfNotExistsObservable(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

            var response = tableServiceClient.CreateTableIfNotExists(tableName, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response<TableItem>> CreateTableIfNotExistsObservableAsync(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

            var response = await tableServiceClient.CreateTableIfNotExistsAsync(tableName, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Response DeleteTableObservable(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

            var response = tableServiceClient.DeleteTable(tableName, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response> DeleteTableObservableAsync(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

            var response = await tableServiceClient.DeleteTableAsync(tableName, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Uri GenerateSasUriObservable(this TableServiceClient tableServiceClient, TableAccountSasPermissions permissions, TableAccountSasResourceTypes resourceTypes, DateTimeOffset expiresOn)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, resourceTypes, expiresOn });

            var response = tableServiceClient.GenerateSasUri(permissions, resourceTypes, expiresOn);

            activity?.SetOutput(response);
            return response;
        }
        public static Uri GenerateSasUriObservable(this TableServiceClient tableServiceClient, TableAccountSasBuilder builder)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { builder });

            var response = tableServiceClient.GenerateSasUri(builder);

            activity?.SetOutput(response);
            return response;
        }
        public static Response<TableServiceProperties> GetPropertiesObservable(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = tableServiceClient.GetProperties(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response<TableServiceProperties>> GetPropertiesObservableAsync(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = await tableServiceClient.GetPropertiesAsync(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static TableAccountSasBuilder GetSasBuilderObservable(this TableServiceClient tableServiceClient, TableAccountSasPermissions permissions, TableAccountSasResourceTypes resourceTypes, DateTimeOffset expiresOn)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, resourceTypes, expiresOn });

            var response = tableServiceClient.GetSasBuilder(permissions, resourceTypes, expiresOn);

            activity?.SetOutput(response);
            return response;
        }
        public static TableAccountSasBuilder GetSasBuilderObservable(this TableServiceClient tableServiceClient, string rawPermissions, TableAccountSasResourceTypes resourceTypes, DateTimeOffset expiresOn)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { rawPermissions, resourceTypes, expiresOn });

            var response = tableServiceClient.GetSasBuilder(rawPermissions, resourceTypes, expiresOn);

            activity?.SetOutput(response);
            return response;
        }
        public static Response<TableServiceStatistics> GetStatisticsObservable(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = tableServiceClient.GetStatistics(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response<TableServiceStatistics>> GetStatisticsObservableAsync(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger);

            var response = await tableServiceClient.GetStatisticsAsync(cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static TableClient GetTableClientObservable(this TableServiceClient tableServiceClient, string tableName)
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

            var response = tableServiceClient.GetTableClient(tableName);

            activity?.SetOutput(response);
            return response;
        }
        public static Pageable<TableItem> QueryObservable(this TableServiceClient tableServiceClient, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

            var response = tableServiceClient.Query(filter, maxPerPage, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Pageable<TableItem> QueryObservable(this TableServiceClient tableServiceClient, FormattableString filter, int? maxPerPage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

            var response = tableServiceClient.Query(filter, maxPerPage, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Pageable<TableItem> QueryObservable(this TableServiceClient tableServiceClient, Expression<Func<TableItem, bool>> filter, int? maxPerPage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

            var response = tableServiceClient.Query(filter, maxPerPage, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static AsyncPageable<TableItem> QueryObservableAsync(this TableServiceClient tableServiceClient, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

            var response = tableServiceClient.QueryAsync(filter, maxPerPage, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static AsyncPageable<TableItem> QueryObservableAsync(this TableServiceClient tableServiceClient, FormattableString filter, int? maxPerPage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

            var response = tableServiceClient.QueryAsync(filter, maxPerPage, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static AsyncPageable<TableItem> QueryObservableAsync(this TableServiceClient tableServiceClient, Expression<Func<TableItem, bool>> filter, int? maxPerPage = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

            var response = tableServiceClient.QueryAsync(filter, maxPerPage, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static Response SetPropertiesObservable(this TableServiceClient tableServiceClient, TableServiceProperties properties, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { properties });

            var response = tableServiceClient.SetProperties(properties, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
        public static async Task<Response> SetPropertiesObservableAsync(this TableServiceClient tableServiceClient, TableServiceProperties properties, CancellationToken cancellationToken = default(CancellationToken))
        {
            var loggerFactory = Observability.LoggerFactory ?? NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { properties });

            var response = await tableServiceClient.SetPropertiesAsync(properties, cancellationToken);

            activity?.SetOutput(response);
            return response;
        }
    }
}
