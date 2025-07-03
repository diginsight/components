using Azure.Data.Tables;

namespace Diginsight.Components.Azure.Abstractions
{
    /// <summary>
    /// Interface for Azure Table Storage repository operations.
    /// Provides comprehensive CRUD operations for both strongly-typed and dynamic entities.
    /// </summary>
    /// <typeparam name="T">The entity type that implements ITableEntity.</typeparam>
    public interface IAzureTableRepository<T> where T : class, ITableEntity, new()
    {
        /// <summary>
        /// Retrieves a collection of records from Azure Table Storage using strongly-typed entities.
        /// </summary>
        /// <param name="filter">Optional OData filter expression to filter results.</param>
        /// <param name="top">Optional maximum number of records to return.</param>
        /// <param name="select">Optional list of properties to select.</param>
        /// <returns>A collection of T entities.</returns>
        Task<IEnumerable<T>> QueryAsync(string? filter = null, int? top = null, IEnumerable<string>? select = null);

        /// <summary>
        /// Retrieves a collection of records from Azure Table Storage as dynamic objects with configurable property naming.
        /// </summary>
        /// <param name="filter">Optional OData filter expression to filter results.</param>
        /// <param name="top">Optional maximum number of records to return.</param>
        /// <param name="select">Optional list of properties to select.</param>
        /// <param name="namingPolicy">Optional property naming policy for response formatting.</param>
        /// <returns>A collection of dynamic objects with formatted property names.</returns>
        Task<IEnumerable<object>> QueryAsJsonAsync(string? filter = null, int? top = null, IEnumerable<string>? select = null, PropertyNamingPolicy? namingPolicy = null);

        /// <summary>
        /// Retrieves a single record from Azure Table Storage by its partition key and row key.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to retrieve.</param>
        /// <param name="rowKey">The row key of the record to retrieve.</param>
        /// <returns>The requested T entity, or null if not found.</returns>
        Task<T?> GetAsync(string partitionKey, string rowKey);

        /// <summary>
        /// Creates a new record in Azure Table Storage using a strongly-typed entity.
        /// </summary>
        /// <param name="entity">The T entity to create.</param>
        /// <returns>The created entity with generated keys and timestamps.</returns>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Creates a new dynamic record in Azure Table Storage from a dictionary of key-value pairs.
        /// </summary>
        /// <param name="entityData">Dictionary containing the properties and values for the new entity.</param>
        /// <returns>The created entity as a dynamic object with camelCase property names.</returns>
        Task<Dictionary<string, object?>> CreateDynamicAsync(Dictionary<string, object> entityData);

        /// <summary>
        /// Creates a new record in Azure Table Storage from a raw JSON string with configurable property naming.
        /// </summary>
        /// <param name="jsonString">The JSON string containing the entity data to create.</param>
        /// <param name="namingPolicy">Optional property naming policy for response formatting.</param>
        /// <returns>The created entity as a dynamic object with formatted property names.</returns>
        Task<Dictionary<string, object?>> CreateAsJsonAsync(string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase);

        /// <summary>
        /// Creates multiple records in Azure Table Storage using strongly-typed entities in a single transaction.
        /// </summary>
        /// <param name="entities">The collection of T entities to create.</param>
        /// <returns>The created entities with generated keys and timestamps.</returns>
        Task<IEnumerable<T>> CreateBatchAsync(IEnumerable<T> entities);

        /// <summary>
        /// Creates multiple records in Azure Table Storage from a JSON array string in a single transaction.
        /// </summary>
        /// <param name="jsonString">The JSON array string containing the entity data to create.</param>
        /// <param name="namingPolicy">Optional property naming policy for response formatting.</param>
        /// <returns>The created entities as dynamic objects with formatted property names.</returns>
        Task<IEnumerable<Dictionary<string, object?>>> CreateAsJsonBatchAsync(string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase);

        /// <summary>
        /// Updates an existing record in Azure Table Storage using a strongly-typed entity.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to update.</param>
        /// <param name="rowKey">The row key of the record to update.</param>
        /// <param name="entity">The updated T entity.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        Task UpdateAsync(string partitionKey, string rowKey, T entity);

        /// <summary>
        /// Updates an existing record in Azure Table Storage using dynamic data from a dictionary.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to update.</param>
        /// <param name="rowKey">The row key of the record to update.</param>
        /// <param name="entityData">Dictionary containing the properties and values to update.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        Task UpdateDynamicAsync(string partitionKey, string rowKey, Dictionary<string, object> entityData);

        /// <summary>
        /// Updates an existing record in Azure Table Storage using data from a raw JSON string.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to update.</param>
        /// <param name="rowKey">The row key of the record to update.</param>
        /// <param name="jsonString">The JSON string containing the properties and values to update.</param>
        /// <param name="namingPolicy">Optional property naming policy for processing.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        Task UpdateAsJsonAsync(string partitionKey, string rowKey, string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase);

        /// <summary>
        /// Deletes a record from Azure Table Storage by its partition key and row key.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to delete.</param>
        /// <param name="rowKey">The row key of the record to delete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteAsync(string partitionKey, string rowKey);
    }

    /// <summary>
    /// Non-generic interface for Azure Table Storage repository operations that provides access to common operations
    /// without requiring a specific entity type. This is useful for dependency injection scenarios where the entity type
    /// is not known at compile time.
    /// </summary>
    public interface IAzureTableRepository
    {
        /// <summary>
        /// Retrieves a collection of records from Azure Table Storage as dynamic objects with configurable property naming.
        /// </summary>
        /// <param name="filter">Optional OData filter expression to filter results.</param>
        /// <param name="top">Optional maximum number of records to return.</param>
        /// <param name="select">Optional list of properties to select.</param>
        /// <param name="namingPolicy">Optional property naming policy for response formatting.</param>
        /// <returns>A collection of dynamic objects with formatted property names.</returns>
        Task<IEnumerable<object>> QueryAsJsonAsync(string? filter = null, int? top = null, IEnumerable<string>? select = null, PropertyNamingPolicy? namingPolicy = null);

        /// <summary>
        /// Creates a new dynamic record in Azure Table Storage from a dictionary of key-value pairs.
        /// </summary>
        /// <param name="entityData">Dictionary containing the properties and values for the new entity.</param>
        /// <returns>The created entity as a dynamic object with camelCase property names.</returns>
        Task<Dictionary<string, object?>> CreateDynamicAsync(Dictionary<string, object> entityData);

        /// <summary>
        /// Creates a new record in Azure Table Storage from a raw JSON string with configurable property naming.
        /// </summary>
        /// <param name="jsonString">The JSON string containing the entity data to create.</param>
        /// <param name="namingPolicy">Optional property naming policy for response formatting.</param>
        /// <returns>The created entity as a dynamic object with formatted property names.</returns>
        Task<Dictionary<string, object?>> CreateAsJsonAsync(string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase);

        /// <summary>
        /// Creates multiple records in Azure Table Storage from a JSON array string in a single transaction.
        /// </summary>
        /// <param name="jsonString">The JSON array string containing the entity data to create.</param>
        /// <param name="namingPolicy">Optional property naming policy for response formatting.</param>
        /// <returns>The created entities as dynamic objects with formatted property names.</returns>
        Task<IEnumerable<Dictionary<string, object?>>> CreateAsJsonBatchAsync(string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase);

        /// <summary>
        /// Updates an existing record in Azure Table Storage using dynamic data from a dictionary.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to update.</param>
        /// <param name="rowKey">The row key of the record to update.</param>
        /// <param name="entityData">Dictionary containing the properties and values to update.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        Task UpdateDynamicAsync(string partitionKey, string rowKey, Dictionary<string, object> entityData);

        /// <summary>
        /// Updates an existing record in Azure Table Storage using data from a raw JSON string.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to update.</param>
        /// <param name="rowKey">The row key of the record to update.</param>
        /// <param name="jsonString">The JSON string containing the properties and values to update.</param>
        /// <param name="namingPolicy">Optional property naming policy for processing.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        Task UpdateAsJsonAsync(string partitionKey, string rowKey, string jsonString, PropertyNamingPolicy? namingPolicy = PropertyNamingPolicy.CamelCase);

        /// <summary>
        /// Deletes a record from Azure Table Storage by its partition key and row key.
        /// </summary>
        /// <param name="partitionKey">The partition key of the record to delete.</param>
        /// <param name="rowKey">The row key of the record to delete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteAsync(string partitionKey, string rowKey);
    }

    /// <summary>
    /// Enumeration defining the available property naming policies for response formatting.
    /// Used to control how property names are formatted in API responses.
    /// </summary>
    public enum PropertyNamingPolicy
    {
        /// <summary>
        /// Uses the original property names without any transformation.
        /// </summary>
        Original,
        
        /// <summary>
        /// Converts property names to camelCase format (e.g., "firstName").
        /// </summary>
        CamelCase,
        
        /// <summary>
        /// Converts property names to lowercase kebab-case format (e.g., "first-name").
        /// </summary>
        KebabCaseLower,
        
        /// <summary>
        /// Converts property names to uppercase kebab-case format (e.g., "FIRST-NAME").
        /// </summary>
        KebabCaseUpper,
        
        /// <summary>
        /// Converts property names to lowercase snake_case format (e.g., "first_name").
        /// </summary>
        SnakeCaseLower,
        
        /// <summary>
        /// Converts property names to uppercase snake_case format (e.g., "FIRST_NAME").
        /// </summary>
        SnakeCaseUpper
    }
}
