using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure.Data.Tables.Sas;
using Diginsight.Diagnostics;
using Diginsight.Stringify;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq.Expressions;

namespace Diginsight.Components.Azure.Extensions;

public static class AzureTableObservableExtensions
{
    public static Response AddEntityObservable<T>(this TableClient tableClient, T entity, CancellationToken cancellationToken = default) where T : ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("➕ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("➕ partitionKey:{PartitionKey}, rowKey:{RowKey}, entity:{Entity}", entity.PartitionKey, entity.RowKey, entity.Stringify());

        var response = tableClient.AddEntity(entity, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response> AddEntityObservableAsync<T>(this TableClient tableClient, T entity, CancellationToken cancellationToken = default) where T : ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("➕ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("➕ partitionKey:{PartitionKey}, rowKey:{RowKey}, entity:{Entity}", entity.PartitionKey, entity.RowKey, entity.Stringify());

        var response = await tableClient.AddEntityAsync(entity, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static Response<TableItem> CreateObservable(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = tableClient.Create(cancellationToken);
        activity?.SetOutput(response);
        return response;
    }

    public static async Task<Response<TableItem>> CreateObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = await tableClient.CreateAsync(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static Response<TableItem> CreateIfNotExistsObservable(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = tableClient.CreateIfNotExists(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static async Task<Response<TableItem>> CreateIfNotExistsObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = await tableClient.CreateIfNotExistsAsync(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static Response DeleteObservable(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = tableClient.Delete(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static async Task<Response> DeleteObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = await tableClient.DeleteAsync(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static Response DeleteEntityObservable(this TableClient tableClient, string partitionKey, string rowKey, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, ifMatch });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, "object", tableName, tableUri, "Standard");
        logger.LogDebug("🗑️ partitionKey:{PartitionKey}, rowKey:{RowKey}", partitionKey, rowKey);

        var response = tableClient.DeleteEntity(partitionKey, rowKey, ifMatch, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static Response DeleteEntityObservable(this TableClient tableClient, ITableEntity entity, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, entity.GetType().Name, tableName, tableUri, "Standard");
        logger.LogDebug("🗑️ partitionKey:{PartitionKey}, rowKey:{RowKey}, entity:{Entity}", entity.PartitionKey, entity.RowKey, entity.Stringify());

        var response = tableClient.DeleteEntity(entity, ifMatch, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static async Task<Response> DeleteEntityObservableAsync(this TableClient tableClient, string partitionKey, string rowKey, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, ifMatch });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, "object", tableName, tableUri, "Standard");
        logger.LogDebug("🗑️ partitionKey:{PartitionKey}, rowKey:{RowKey}", partitionKey, rowKey);

        var response = await tableClient.DeleteEntityAsync(partitionKey, rowKey, ifMatch, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static async Task<Response> DeleteEntityObservableAsync(this TableClient tableClient, ITableEntity entity, ETag ifMatch = default, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, entity.GetType().Name, tableName, tableUri, "Standard");
        logger.LogDebug("🗑️ partitionKey:{PartitionKey}, rowKey:{RowKey}, entity:{Entity}", entity.PartitionKey, entity.RowKey, entity.Stringify());

        var response = await tableClient.DeleteEntityAsync(entity, ifMatch, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static Uri GenerateSasUriObservable(this TableClient tableClient, TableSasPermissions permissions, DateTimeOffset expiresOn)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, expiresOn });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, permissions:{Permissions}, expiresOn:{ExpiresOn}", operationName, tableName, tableUri, permissions, expiresOn);

        var response = tableClient.GenerateSasUri(permissions, expiresOn);

        activity?.SetOutput(response);
        return response;
    }

    public static Uri GenerateSasUriObservable(this TableClient tableClient, TableSasBuilder builder)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { builder });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, builder:{Builder}", operationName, tableName, tableUri, builder.Stringify());

        var response = tableClient.GenerateSasUri(builder);

        activity?.SetOutput(response);
        return response;
    }

    public static Response<IReadOnlyList<TableSignedIdentifier>> GetAccessPoliciesObservable(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("📋 AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = tableClient.GetAccessPolicies(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response<IReadOnlyList<TableSignedIdentifier>>> GetAccessPoliciesObservableAsync(this TableClient tableClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("📋 AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, tableUri, "Standard");

        var response = await tableClient.GetAccessPoliciesAsync(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response<T> GetEntityObservable<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 partitionKey:{PartitionKey}, rowKey:{RowKey}", partitionKey, rowKey);

        var response = tableClient.GetEntity<T>(partitionKey, rowKey, select, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response<T>> GetEntityObservableAsync<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 partitionKey:{PartitionKey}, rowKey:{RowKey}", partitionKey, rowKey);

        var response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey, select, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static NullableResponse<T> GetEntityIfExistsObservable<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 partitionKey:{PartitionKey}, rowKey:{RowKey}", partitionKey, rowKey);

        var response = tableClient.GetEntityIfExists<T>(partitionKey, rowKey, select, cancellationToken);
        activity?.SetOutput(response);
        return response;
    }
    public static async Task<NullableResponse<T>> GetEntityIfExistsObservableAsync<T>(this TableClient tableClient, string partitionKey, string rowKey, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { partitionKey, rowKey, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 partitionKey:{PartitionKey}, rowKey:{RowKey}", partitionKey, rowKey);

        var response = await tableClient.GetEntityIfExistsAsync<T>(partitionKey, rowKey, select, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static TableSasBuilder GetSasBuilderObservable(this TableClient tableClient, TableSasPermissions permissions, DateTimeOffset expiresOn)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, expiresOn });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, permissions:{Permissions}, expiresOn:{ExpiresOn}", operationName, tableName, tableUri, permissions, expiresOn);

        var response = tableClient.GetSasBuilder(permissions, expiresOn);

        activity?.SetOutput(response);
        return response;
    }
    public static TableSasBuilder GetSasBuilderObservable(this TableClient tableClient, string rawPermissions, DateTimeOffset expiresOn)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { rawPermissions, expiresOn });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, rawPermissions:{RawPermissions}, expiresOn:{ExpiresOn}", operationName, tableName, tableUri, rawPermissions, expiresOn);

        var response = tableClient.GetSasBuilder(rawPermissions, expiresOn);

        activity?.SetOutput(response);
        return response;
    }
    public static Pageable<T> QueryObservable<T>(this TableClient tableClient, Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}, select:{Select}", filter?.ToString() ?? "null", maxPerPage?.ToString() ?? "null", select is not null ? string.Join(",", select) : "null");

        var response = tableClient.Query(filter, maxPerPage, select, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Pageable<T> QueryObservable<T>(this TableClient tableClient, string filter = null, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}, select:{Select}", filter ?? "null", maxPerPage?.ToString() ?? "null", select is not null ? string.Join(",", select) : "null");

        var response = tableClient.Query<T>(filter, maxPerPage, select, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static AsyncPageable<T> QueryObservableAsync<T>(this TableClient tableClient, Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}, select:{Select}", filter?.ToString() ?? "null", maxPerPage?.ToString() ?? "null", select is not null ? string.Join(",", select) : "null");

        var response = tableClient.QueryAsync(filter, maxPerPage, select, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static AsyncPageable<T> QueryObservableAsync<T>(this TableClient tableClient, string filter = null, int? maxPerPage = null, IEnumerable<string> select = null, CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage, select });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;

        logger.LogDebug("🔍 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}, select:{Select}", filter ?? "null", maxPerPage?.ToString() ?? "null", select is not null ? string.Join(",", select) : "null");

        var response = tableClient.QueryAsync<T>(filter, maxPerPage, select, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response SetAccessPolicyObservable(this TableClient tableClient, IEnumerable<TableSignedIdentifier> tableAcl, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableAcl });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("⚙️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, tableAcl:{TableAcl}", operationName, tableName, tableUri, tableAcl.Stringify());

        var response = tableClient.SetAccessPolicy(tableAcl, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static async Task<Response> SetAccessPolicyObservableAsync(this TableClient tableClient, IEnumerable<TableSignedIdentifier> tableAcl, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableAcl });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("⚙️ AzureTable {OperationName} for table:{TableName}, StorageUri:{StorageUri}, tableAcl:{TableAcl}", operationName, tableName, tableUri, tableAcl.Stringify());

        var response = await tableClient.SetAccessPolicyAsync(tableAcl, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response<IReadOnlyList<Response>> SubmitTransactionObservable(this TableClient tableClient, IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { transactionActions });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("➕ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, transactionActions.FirstOrDefault()?.Entity?.GetType()?.Name, tableName, tableUri, "Standard");
        logger.LogDebug("➕ transactionActions:{TransactionActions}", transactionActions.Stringify());

        var response = tableClient.SubmitTransaction(transactionActions, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response<IReadOnlyList<Response>>> SubmitTransactionObservableAsync(this TableClient tableClient, IEnumerable<TableTransactionAction> transactionActions, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { transactionActions });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("➕ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, transactionActions.FirstOrDefault()?.Entity?.GetType()?.Name, tableName, tableUri, "Standard");
        logger.LogDebug("➕ transactionActions:{TransactionActions}", transactionActions.Stringify());

        var response = await tableClient.SubmitTransactionAsync(transactionActions, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response UpdateEntityObservable<T>(this TableClient tableClient, T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) where T : ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch, mode });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔄 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔄 existingEntity.ETag:{ETag}, entity:{Entity}", ifMatch, entity.Stringify());

        var response = tableClient.UpdateEntity(entity, ifMatch, mode, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response> UpdateEntityObservableAsync<T>(this TableClient tableClient, T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) where T : ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, ifMatch, mode });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔄 AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("🔄 existingEntity.ETag:{ETag}, entity:{Entity}", ETag.All, entity.Stringify());

        var response = await tableClient.UpdateEntityAsync(entity, ifMatch, mode, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response UpsertEntityObservable<T>(this TableClient tableClient, T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) where T : ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, mode });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("⬆️ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("⬆️ partitionKey:{PartitionKey}, rowKey:{RowKey}, entity:{Entity}", entity.PartitionKey, entity.RowKey, entity.Stringify());

        var response = tableClient.UpsertEntity(entity, mode, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response> UpsertEntityObservableAsync<T>(this TableClient tableClient, T entity, TableUpdateMode mode = TableUpdateMode.Merge, CancellationToken cancellationToken = default) where T : ITableEntity
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity, mode });

        var tableUri = tableClient.Uri;
        var tableName = tableClient.Name;
        var operationName = activity?.OperationName;
        logger.LogDebug("⬆️ AzureTable {OperationName} for type:{Type}, table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, typeof(T).Name, tableName, tableUri, "Standard");
        logger.LogDebug("⬆️ partitionKey:{PartitionKey}, rowKey:{RowKey}, entity:{Entity}", entity.PartitionKey, entity.RowKey, entity.Stringify());

        var response = await tableClient.UpsertEntityAsync(entity, mode, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }

    public static Response<TableItem> CreateTableObservable(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTableService {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, serviceUri, "Standard");

        var response = tableServiceClient.CreateTable(tableName, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response<TableItem>> CreateTableObservableAsync(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTableService {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, serviceUri, "Standard");

        var response = await tableServiceClient.CreateTableAsync(tableName, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response<TableItem> CreateTableIfNotExistsObservable(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTableService {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, serviceUri, "Standard");

        var response = tableServiceClient.CreateTableIfNotExists(tableName, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response<TableItem>> CreateTableIfNotExistsObservableAsync(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🏗️ AzureTableService {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, serviceUri, "Standard");

        var response = await tableServiceClient.CreateTableIfNotExistsAsync(tableName, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response DeleteTableObservable(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTableService {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, serviceUri, "Standard");

        var response = tableServiceClient.DeleteTable(tableName, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response> DeleteTableObservableAsync(this TableServiceClient tableServiceClient, string tableName, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🗑️ AzureTableService {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, serviceUri, "Standard");

        var response = await tableServiceClient.DeleteTableAsync(tableName, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Uri GenerateSasUriObservable(this TableServiceClient tableServiceClient, TableAccountSasPermissions permissions, TableAccountSasResourceTypes resourceTypes, DateTimeOffset expiresOn)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, resourceTypes, expiresOn });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTableService {OperationName} for StorageUri:{StorageUri}, permissions:{Permissions}, resourceTypes:{ResourceTypes}, expiresOn:{ExpiresOn}", operationName, serviceUri, permissions, resourceTypes, expiresOn);

        var response = tableServiceClient.GenerateSasUri(permissions, resourceTypes, expiresOn);

        activity?.SetOutput(response);
        return response;
    }
    public static Uri GenerateSasUriObservable(this TableServiceClient tableServiceClient, TableAccountSasBuilder builder)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { builder });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTableService {OperationName} for StorageUri:{StorageUri}, builder:{Builder}", operationName, serviceUri, builder.Stringify());

        var response = tableServiceClient.GenerateSasUri(builder);

        activity?.SetOutput(response);
        return response;
    }
    public static Response<TableServiceProperties> GetPropertiesObservable(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("📋 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");

        var response = tableServiceClient.GetProperties(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response<TableServiceProperties>> GetPropertiesObservableAsync(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("📋 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");

        var response = await tableServiceClient.GetPropertiesAsync(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static TableAccountSasBuilder GetSasBuilderObservable(this TableServiceClient tableServiceClient, TableAccountSasPermissions permissions, TableAccountSasResourceTypes resourceTypes, DateTimeOffset expiresOn)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { permissions, resourceTypes, expiresOn });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTableService {OperationName} for StorageUri:{StorageUri}, permissions:{Permissions}, resourceTypes:{ResourceTypes}, expiresOn:{ExpiresOn}", operationName, serviceUri, permissions, resourceTypes, expiresOn);

        var response = tableServiceClient.GetSasBuilder(permissions, resourceTypes, expiresOn);

        activity?.SetOutput(response);
        return response;
    }
    public static TableAccountSasBuilder GetSasBuilderObservable(this TableServiceClient tableServiceClient, string rawPermissions, TableAccountSasResourceTypes resourceTypes, DateTimeOffset expiresOn)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { rawPermissions, resourceTypes, expiresOn });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔑 AzureTableService {OperationName} for StorageUri:{StorageUri}, rawPermissions:{RawPermissions}, resourceTypes:{ResourceTypes}, expiresOn:{ExpiresOn}", operationName, serviceUri, rawPermissions, resourceTypes, expiresOn);

        var response = tableServiceClient.GetSasBuilder(rawPermissions, resourceTypes, expiresOn);

        activity?.SetOutput(response);
        return response;
    }
    public static Response<TableServiceStatistics> GetStatisticsObservable(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("📈 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");

        var response = tableServiceClient.GetStatistics(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response<TableServiceStatistics>> GetStatisticsObservableAsync(this TableServiceClient tableServiceClient, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("📈 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");

        var response = await tableServiceClient.GetStatisticsAsync(cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static TableClient GetTableClientObservable(this TableServiceClient tableServiceClient, string tableName)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { tableName });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🎯 AzureTableService {OperationName} for table:{TableName}, StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, tableName, serviceUri, "Standard");

        var response = tableServiceClient.GetTableClient(tableName);

        activity?.SetOutput(response);
        return response;
    }
    public static Pageable<TableItem> QueryObservable(this TableServiceClient tableServiceClient, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}", filter ?? "null", maxPerPage?.ToString() ?? "null");

        var response = tableServiceClient.Query(filter, maxPerPage, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Pageable<TableItem> QueryObservable(this TableServiceClient tableServiceClient, FormattableString filter, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}", filter?.ToString() ?? "null", maxPerPage?.ToString() ?? "null");

        var response = tableServiceClient.Query(filter, maxPerPage, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Pageable<TableItem> QueryObservable(this TableServiceClient tableServiceClient, Expression<Func<TableItem, bool>> filter, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}", filter?.ToString() ?? "null", maxPerPage?.ToString() ?? "null");

        var response = tableServiceClient.Query(filter, maxPerPage, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static AsyncPageable<TableItem> QueryObservableAsync(this TableServiceClient tableServiceClient, string filter = null, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}", filter ?? "null", maxPerPage?.ToString() ?? "null");

        var response = tableServiceClient.QueryAsync(filter, maxPerPage, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static AsyncPageable<TableItem> QueryObservableAsync(this TableServiceClient tableServiceClient, FormattableString filter, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}", filter?.ToString() ?? "null", maxPerPage?.ToString() ?? "null");

        var response = tableServiceClient.QueryAsync(filter, maxPerPage, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static AsyncPageable<TableItem> QueryObservableAsync(this TableServiceClient tableServiceClient, Expression<Func<TableItem, bool>> filter, int? maxPerPage = null, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { filter, maxPerPage });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("🔍 AzureTableService {OperationName} for StorageUri:{StorageUri}, DefaultRequestOptions:{DefaultRequestOptions}", operationName, serviceUri, "Standard");
        logger.LogDebug("🔍 filter:{Filter}, top:{Top}", filter?.ToString() ?? "null", maxPerPage?.ToString() ?? "null");

        var response = tableServiceClient.QueryAsync(filter, maxPerPage, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static Response SetPropertiesObservable(this TableServiceClient tableServiceClient, TableServiceProperties properties, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { properties });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("⚙️ AzureTableService {OperationName} for StorageUri:{StorageUri}, properties:{Properties}", operationName, serviceUri, properties.Stringify());

        var response = tableServiceClient.SetProperties(properties, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
    public static async Task<Response> SetPropertiesObservableAsync(this TableServiceClient tableServiceClient, TableServiceProperties properties, CancellationToken cancellationToken = default)
    {
        var loggerFactory = Observability.LoggerFactory;
        var logger = loggerFactory.CreateLogger(typeof(AzureTableObservableExtensions));
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { properties });

        var serviceUri = tableServiceClient.Uri;
        var operationName = activity?.OperationName;
        logger.LogDebug("⚙️ AzureTableService {OperationName} for StorageUri:{StorageUri}, properties:{Properties}", operationName, serviceUri, properties.Stringify());

        var response = await tableServiceClient.SetPropertiesAsync(properties, cancellationToken);

        activity?.SetOutput(response);
        return response;
    }
}
