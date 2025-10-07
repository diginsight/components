using Azure;
using Azure.Data.Tables;
using Diginsight.Components.Azure.Abstractions;
using Diginsight.Components.Azure.Extensions;
using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Diginsight.Components.Azure.Repositories;

/// <summary>
/// Generic repository implementation for Azure Table Storage operations.
/// Provides comprehensive CRUD operations for both strongly-typed and dynamic entities.
/// 
/// Log Message Icons:
/// 🔍 - Query operations
/// ➕ - Create operations
/// 🔄 - Update operations
/// 🗑️ - Delete operations
/// ❌ - Error operations
/// ⚠️ - Warning operations
/// </summary>
/// <typeparam name="T">The entity type that implements ITableEntity.</typeparam>
public class AzureTableRepository<T> : IAzureTableRepository<T>, IAzureTableRepository
    where T : class, ITableEntity, new()
{
    private readonly TableServiceClient tableServiceClient;
    private readonly ILogger<AzureTableRepository<T>> logger;
    private readonly string tableName;

    private readonly Lazy<Task<TableClient>> lazyTableClient;

    /// <summary>
    /// Initializes a new instance of the AzureTableRepository class.
    /// </summary>
    /// <param name="tableServiceClient">The Azure Table Storage service client.</param>
    /// <param name="logger">The logger instance for logging operations and errors.</param>
    /// <param name="tableName">The name of the table to operate on.</param>
    public AzureTableRepository(TableServiceClient tableServiceClient,
        ILogger<AzureTableRepository<T>> logger,
        string tableName = "DefaultTable")
    {
        this.tableServiceClient = tableServiceClient;
        this.logger = logger;
        this.tableName = tableName;

        // Initialize the lazy client
        lazyTableClient = new Lazy<Task<TableClient>>(InitializeTableClientAsync);
    }

    private async Task<TableClient> InitializeTableClientAsync()
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { tableName });

        var client = tableServiceClient.GetTableClientObservable(tableName);
        await client.CreateIfNotExistsObservableAsync();
        return client;
    }

    private async Task<TableClient> GetTableClientAsync() => await lazyTableClient.Value;

    /// <inheritdoc />
    public async Task<IEnumerable<T>> QueryAsync(string? filter = null, int? top = null, IEnumerable<string>? select = null)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { filter, top, select });

        var tableClient = await GetTableClientAsync();
        await tableClient.CreateIfNotExistsObservableAsync();

        var records = new List<T>();
        var asyncPageableRecords = tableClient.QueryObservableAsync<T>(
            filter: filter,
            maxPerPage: top,
            select: select);

        await foreach (var entity in asyncPageableRecords)
        {
            records.Add(entity);
        }

        activity?.SetOutput(records);
        return records;
    }

    /// <inheritdoc cref="QueryAsJsonAsync" />
    public async Task<IEnumerable<object>> QueryAsJsonAsync(string? filter = null, int? top = null, IEnumerable<string>? select = null, PropertyNamingPolicy? namingPolicy = null)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { filter, top, select, namingPolicy });

        var tableClient = await GetTableClientAsync();
        await tableClient.CreateIfNotExistsObservableAsync();

        var records = new List<object>();
        var asyncPageableRecords = tableClient.QueryObservableAsync<TableEntity>(
            filter: filter,
            maxPerPage: top,
            select: select);

        await foreach (var entity in asyncPageableRecords)
        {
            var record = new Dictionary<string, object?>();

            foreach (var kvp in entity)
            {
                string propertyName = namingPolicy switch
                {
                    PropertyNamingPolicy.CamelCase => ToCamelCase(kvp.Key),
                    PropertyNamingPolicy.KebabCaseLower => ToKebabCase(kvp.Key),
                    PropertyNamingPolicy.SnakeCaseLower => ToSnakeCase(kvp.Key),
                    PropertyNamingPolicy.KebabCaseUpper => ToKebabCase(kvp.Key).ToUpperInvariant(),
                    PropertyNamingPolicy.SnakeCaseUpper => ToSnakeCase(kvp.Key).ToUpperInvariant(),
                    _ => kvp.Key
                };
                record[propertyName] = kvp.Value;
            }

            records.Add(record);
        }

        activity?.SetOutput(records);
        return records;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync(string partitionKey, string rowKey)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { partitionKey, rowKey });
        try
        {
            var tableClient = await GetTableClientAsync();
            await tableClient.CreateIfNotExistsObservableAsync();

            var response = await tableClient.GetEntityObservableAsync<T>(partitionKey, rowKey);

            activity?.SetOutput(response?.Value);
            return response?.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            activity?.SetOutput(null);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<T> CreateAsync(T entity)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { entity });

        var tableClient = await GetTableClientAsync();
        await tableClient.CreateIfNotExistsObservableAsync();

        // Set default timestamps if entity supports them
        SetEntityTimestamps(entity, isCreate: true);

        // Generate RowKey if not provided
        if (string.IsNullOrEmpty(entity.RowKey))
        {
            entity.RowKey = Guid.NewGuid().ToString();
        }

        // Set default partition key if not provided
        if (string.IsNullOrEmpty(entity.PartitionKey))
        {
            entity.PartitionKey = "default";
        }

        await tableClient.AddEntityObservableAsync(entity);

        activity?.SetOutput(entity);
        return entity;
    }

    /// <inheritdoc cref="CreateDynamicAsync" />
    public async Task<Dictionary<string, object?>> CreateDynamicAsync(Dictionary<string, object> entityData)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { entityData });
        var tableClient = await GetTableClientAsync();
        await tableClient.CreateIfNotExistsObservableAsync();

        var entity = new TableEntity();
        string partitionKey = "default";
        string rowKey = Guid.NewGuid().ToString();

        foreach ((string key, var value) in entityData)
        {
            switch (key.ToLowerInvariant())
            {
                case "partitionkey":
                    partitionKey = value?.ToString() ?? "default";
                    break;
                case "rowkey":
                    rowKey = value?.ToString() ?? Guid.NewGuid().ToString();
                    break;
                case "timestamp":
                case "etag":
                    continue;
                default:
                    entity[key] = value;
                    break;
            }
        }

        entity.PartitionKey = partitionKey;
        entity.RowKey = rowKey;

        if (!entity.ContainsKey("CreatedAt"))
        {
            entity["CreatedAt"] = DateTime.UtcNow;
        }
        entity.TryAdd("UpdatedAt", null);

        await tableClient.AddEntityObservableAsync(entity);

        var responseEntity = new Dictionary<string, object?>();
        foreach (var kvp in entity)
        {
            string propertyName = ToCamelCase(kvp.Key);
            responseEntity[propertyName] = kvp.Value;
        }

        activity?.SetOutput(responseEntity);
        return responseEntity;
    }

    /// <inheritdoc cref="CreateAsJsonAsync" />
    public async Task<Dictionary<string, object?>> CreateAsJsonAsync(string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { jsonString, namingPolicy });

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new ArgumentException("JSON string cannot be null or empty", nameof(jsonString));
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonString);
            if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("JSON must be an object", nameof(jsonString));
            }

            var tableClient = await GetTableClientAsync();
            await tableClient.CreateIfNotExistsObservableAsync();

            var entity = new TableEntity();
            string partitionKey = "default";
            string rowKey = Guid.NewGuid().ToString();

            foreach (var property in jsonDocument.RootElement.EnumerateObject())
            {
                var key = property.Name;
                var value = ExtractJsonValue(property.Value);
                var normalizedKey = key.ToLowerInvariant();

                switch (normalizedKey)
                {
                    case "partitionkey":
                        partitionKey = value?.ToString() ?? "default";
                        break;
                    case "rowkey":
                        rowKey = value?.ToString() ?? Guid.NewGuid().ToString();
                        break;
                    case "timestamp":
                    case "etag":
                        continue;
                    default:
                        string storageKey = ToPascalCase(key);
                        entity[storageKey] = value;
                        break;
                }
            }

            entity.PartitionKey = partitionKey;
            entity.RowKey = rowKey;

            if (!entity.ContainsKey("CreatedAt"))
            {
                entity["CreatedAt"] = DateTime.UtcNow;
            }
            entity.TryAdd("UpdatedAt", null);

            await tableClient.AddEntityObservableAsync(entity);

            var responseEntity = new Dictionary<string, object?>();
            foreach (var kvp in entity)
            {
                string propertyName = namingPolicy switch
                {
                    PropertyNamingPolicy.CamelCase => ToCamelCase(kvp.Key),
                    PropertyNamingPolicy.KebabCaseLower => ToKebabCase(kvp.Key),
                    PropertyNamingPolicy.SnakeCaseLower => ToSnakeCase(kvp.Key),
                    PropertyNamingPolicy.KebabCaseUpper => ToKebabCase(kvp.Key).ToUpperInvariant(),
                    PropertyNamingPolicy.SnakeCaseUpper => ToSnakeCase(kvp.Key).ToUpperInvariant(),
                    _ => kvp.Key
                };
                responseEntity[propertyName] = kvp.Value;
            }

            activity?.SetOutput(responseEntity);
            return responseEntity;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(jsonString), ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> CreateBatchAsync(IEnumerable<T> entities)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { entities });

        if (entities is null || !entities.Any())
        {
            throw new ArgumentException("Entities collection cannot be null or empty", nameof(entities));
        }

        var entityList = entities.ToList();
        if (entityList.Count > 100)
        {
            throw new ArgumentException("Batch size cannot exceed 100 records", nameof(entities));
        }

        try
        {
            var tableClient = await GetTableClientAsync();
            await tableClient.CreateIfNotExistsObservableAsync();

            foreach (var entity in entityList)
            {
                SetEntityTimestamps(entity, isCreate: true);

                if (string.IsNullOrEmpty(entity.RowKey))
                {
                    entity.RowKey = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(entity.PartitionKey))
                {
                    entity.PartitionKey = "default";
                }
            }

            var partitionKeys = entityList.Select(static r => r.PartitionKey).Distinct().ToList();
            if (partitionKeys.Count > 1)
            {
                throw new ArgumentException("All entities in a batch must have the same partition key. Found partition keys: " + string.Join(", ", partitionKeys), nameof(entities));
            }

            var transactionActions = entityList.Select(static entity => new TableTransactionAction(TableTransactionActionType.Add, entity)).ToList();

            await tableClient.SubmitTransactionObservableAsync(transactionActions);

            activity?.SetOutput(entityList);
            return entityList;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw;
        }
    }

    /// <inheritdoc cref="CreateAsJsonBatchAsync" />
    public async Task<IEnumerable<Dictionary<string, object?>>> CreateAsJsonBatchAsync(string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { jsonString, namingPolicy });

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new ArgumentException("JSON string cannot be null or empty", nameof(jsonString));
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonString);
            if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("JSON must be an array", nameof(jsonString));
            }

            var jsonArray = jsonDocument.RootElement.EnumerateArray().ToList();
            if (!jsonArray.Any())
            {
                throw new ArgumentException("JSON array cannot be empty", nameof(jsonString));
            }

            if (jsonArray.Count > 100)
            {
                throw new ArgumentException("Batch size cannot exceed 100 records", nameof(jsonString));
            }

            var tableClient = await GetTableClientAsync();
            await tableClient.CreateIfNotExistsObservableAsync();

            var entities = new List<TableEntity>();

            foreach (var jsonElement in jsonArray)
            {
                if (jsonElement.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException("All items in the JSON array must be objects", nameof(jsonString));
                }

                var entity = new TableEntity();
                string partitionKey = "default";
                string rowKey = Guid.NewGuid().ToString();

                foreach (var property in jsonElement.EnumerateObject())
                {
                    var key = property.Name;
                    var value = ExtractJsonValue(property.Value);
                    var normalizedKey = key.ToLowerInvariant();

                    switch (normalizedKey)
                    {
                        case "partitionkey":
                            partitionKey = value?.ToString() ?? "default";
                            break;
                        case "rowkey":
                            rowKey = value?.ToString() ?? Guid.NewGuid().ToString();
                            break;
                        case "timestamp":
                        case "etag":
                            continue;
                        default:
                            string storageKey = ToPascalCase(key);
                            entity[storageKey] = value;
                            break;
                    }
                }

                entity.PartitionKey = partitionKey;
                entity.RowKey = rowKey;

                if (!entity.ContainsKey("CreatedAt"))
                {
                    entity["CreatedAt"] = DateTime.UtcNow;
                }
                entity.TryAdd("UpdatedAt", null);

                entities.Add(entity);
            }

            var partitionKeys = entities.Select(static e => e.PartitionKey).Distinct().ToList();
            if (partitionKeys.Count > 1)
            {
                throw new ArgumentException("All records in a batch must have the same partition key. Found partition keys: " + string.Join(", ", partitionKeys), nameof(jsonString));
            }

            var transactionActions = entities.Select(static entity => new TableTransactionAction(TableTransactionActionType.Add, entity)).ToList();

            await tableClient.SubmitTransactionObservableAsync(transactionActions);

            var responseEntities = new List<Dictionary<string, object?>>();
            foreach (var entity in entities)
            {
                var responseEntity = new Dictionary<string, object?>();
                foreach (var kvp in entity)
                {
                    string propertyName = namingPolicy switch
                    {
                        PropertyNamingPolicy.CamelCase => ToCamelCase(kvp.Key),
                        PropertyNamingPolicy.KebabCaseLower => ToKebabCase(kvp.Key),
                        PropertyNamingPolicy.SnakeCaseLower => ToSnakeCase(kvp.Key),
                        PropertyNamingPolicy.KebabCaseUpper => ToKebabCase(kvp.Key).ToUpperInvariant(),
                        PropertyNamingPolicy.SnakeCaseUpper => ToSnakeCase(kvp.Key).ToUpperInvariant(),
                        _ => kvp.Key
                    };
                    responseEntity[propertyName] = kvp.Value;
                }
                responseEntities.Add(responseEntity);
            }

            activity?.SetOutput(responseEntities);
            return responseEntities;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(jsonString), ex);
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(string partitionKey, string rowKey, T entity)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { partitionKey, rowKey, entity });

        var tableClient = await GetTableClientAsync();
        await tableClient.CreateIfNotExistsObservableAsync();

        entity.PartitionKey = partitionKey;
        entity.RowKey = rowKey;
        SetEntityTimestamps(entity, isCreate: false);

        await tableClient.UpdateEntityObservableAsync(entity, ETag.All);
    }

    /// <inheritdoc cref="UpdateDynamicAsync" />
    public async Task UpdateDynamicAsync(string partitionKey, string rowKey, Dictionary<string, object> entityData)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { partitionKey, rowKey, entityData });

        try
        {
            var tableClient = await GetTableClientAsync();
            await tableClient.CreateIfNotExistsObservableAsync();

            TableEntity existingEntity;
            try
            {
                var response = await tableClient.GetEntityObservableAsync<TableEntity>(partitionKey, rowKey);
                existingEntity = response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new InvalidOperationException("Entity not found", ex);
            }

            var updatedEntity = new TableEntity(partitionKey, rowKey)
            {
                ETag = existingEntity.ETag
            };

            foreach (var kvp in existingEntity)
            {
                if (kvp.Key != "PartitionKey" && kvp.Key != "RowKey" && kvp.Key != "Timestamp" && kvp.Key != "ETag")
                {
                    updatedEntity[kvp.Key] = kvp.Value;
                }
            }

            foreach ((string key, object value) in entityData)
            {
                switch (key.ToLowerInvariant())
                {
                    case "partitionkey":
                    case "rowkey":
                    case "timestamp":
                    case "etag":
                        continue;
                    default:
                        updatedEntity[key] = value;
                        break;
                }
            }

            updatedEntity["UpdatedAt"] = DateTime.UtcNow;

            await tableClient.UpdateEntityObservableAsync(updatedEntity, existingEntity.ETag);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw;
        }
    }

    /// <inheritdoc cref="UpdateAsJsonAsync" />
    public async Task UpdateAsJsonAsync(string partitionKey, string rowKey, string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { partitionKey, rowKey, jsonString, namingPolicy });

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new ArgumentException("JSON string cannot be null or empty", nameof(jsonString));
        }

        try
        {
            using var jsonDocument = JsonDocument.Parse(jsonString);
            if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("JSON must be an object", nameof(jsonString));
            }

            var tableClient = await GetTableClientAsync();
            await tableClient.CreateIfNotExistsObservableAsync();

            TableEntity existingEntity;
            try
            {
                var response = await tableClient.GetEntityObservableAsync<TableEntity>(partitionKey, rowKey);
                existingEntity = response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new InvalidOperationException("Entity not found", ex);
            }

            var updatedEntity = new TableEntity(partitionKey, rowKey)
            {
                ETag = existingEntity.ETag
            };

            foreach (var kvp in existingEntity)
            {
                if (kvp.Key != "PartitionKey" && kvp.Key != "RowKey" && kvp.Key != "Timestamp" && kvp.Key != "ETag")
                {
                    updatedEntity[kvp.Key] = kvp.Value;
                }
            }

            foreach (var property in jsonDocument.RootElement.EnumerateObject())
            {
                var key = property.Name;
                var value = ExtractJsonValue(property.Value);
                var normalizedKey = key.ToLowerInvariant();

                switch (normalizedKey)
                {
                    case "partitionkey":
                    case "rowkey":
                    case "timestamp":
                    case "etag":
                        continue;
                    default:
                        string storageKey = ToPascalCase(key);
                        updatedEntity[storageKey] = value;
                        break;
                }
            }

            updatedEntity["UpdatedAt"] = DateTime.UtcNow;

            await tableClient.UpdateEntityObservableAsync(updatedEntity, existingEntity.ETag);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(jsonString), ex);
        }
        catch (Exception ex) when (ex is not (ArgumentException or InvalidOperationException))
        {
            throw;
        }
    }

    /// <inheritdoc cref="DeleteAsync" />
    public async Task DeleteAsync(string partitionKey, string rowKey)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { partitionKey, rowKey });

        try
        {
            var tableClient = await GetTableClientAsync();
            await tableClient.CreateIfNotExistsObservableAsync();

            await tableClient.DeleteEntityObservableAsync(partitionKey, rowKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new InvalidOperationException("Entity not found", ex);
        }
    }

    /// <summary>
    /// Sets timestamps on the entity if it supports them through reflection.
    /// This allows the repository to work with any entity type that has CreatedAt/UpdatedAt properties.
    /// </summary>
    /// <param name="entity">The entity to set timestamps on.</param>
    /// <param name="isCreate">Whether this is a create operation (sets CreatedAt) or update operation (sets UpdatedAt).</param>
    private static void SetEntityTimestamps(T entity, bool isCreate)
    {
        var entityType = typeof(T);

        if (isCreate)
        {
            var createdAtProperty = entityType.GetProperty("CreatedAt");
            if (createdAtProperty is not null && createdAtProperty.CanWrite && createdAtProperty.PropertyType == typeof(DateTime))
            {
                createdAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            var updatedAtProperty = entityType.GetProperty("UpdatedAt");
            if (updatedAtProperty is not null && updatedAtProperty.CanWrite &&
                (updatedAtProperty.PropertyType == typeof(DateTime?) || updatedAtProperty.PropertyType == typeof(DateTime)))
            {
                updatedAtProperty.SetValue(entity, null);
            }
        }
        else
        {
            var updatedAtProperty = entityType.GetProperty("UpdatedAt");
            if (updatedAtProperty is not null && updatedAtProperty.CanWrite &&
                (updatedAtProperty.PropertyType == typeof(DateTime?) || updatedAtProperty.PropertyType == typeof(DateTime)))
            {
                updatedAtProperty.SetValue(entity, DateTime.UtcNow);
            }
        }
    }

    /// <summary>
    /// Converts a string from PascalCase to camelCase format.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string converted to camelCase format.</returns>
    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            return input;

        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    /// <summary>
    /// Converts a string from camelCase to PascalCase format.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string converted to PascalCase format.</returns>
    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (char.IsUpper(input[0]))
            return input;

        return char.ToUpperInvariant(input[0]) + input[1..];
    }

    /// <summary>
    /// Converts a string from PascalCase/camelCase to kebab-case format (lowercase with hyphens).
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string converted to kebab-case format.</returns>
    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return string.Concat(input.Select(static (x, i) => i > 0 && char.IsUpper(x) ? "-" + char.ToLowerInvariant(x) : char.ToLowerInvariant(x).ToString()));
    }

    /// <summary>
    /// Converts a string from PascalCase/camelCase to snake_case format (lowercase with underscores).
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string converted to snake_case format.</returns>
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return string.Concat(input.Select(static (x, i) => i > 0 && char.IsUpper(x) ? "_" + char.ToLowerInvariant(x) : char.ToLowerInvariant(x).ToString()));
    }

    /// <summary>
    /// Recursively extracts and converts JSON element values to appropriate .NET types.
    /// Handles all JSON value types including nested objects and arrays.
    /// </summary>
    /// <param name="element">The JsonElement to extract the value from.</param>
    /// <returns>The extracted value as the appropriate .NET type (string, number, bool, null, array, or dictionary).</returns>
    private static object? ExtractJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out int intValue) ? intValue :
                element.TryGetInt64(out long longValue) ? longValue :
                element.TryGetDouble(out double doubleValue) ? doubleValue :
                element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ExtractJsonValue).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(static p => p.Name, static p => ExtractJsonValue(p.Value)),
            _ => element.ToString()
        };
    }
}
