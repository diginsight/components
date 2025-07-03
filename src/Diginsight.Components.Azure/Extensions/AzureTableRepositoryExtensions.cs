using Azure.Data.Tables;
using Diginsight.Components.Azure.Abstractions;
using Diginsight.Components.Azure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Diginsight.Components.Azure.Extensions
{
    /// <summary>
    /// Extension methods for configuring Azure Table Storage repositories in the dependency injection container.
    /// </summary>
    public static class AzureTableRepositoryExtensions
    {
        /// <summary>
        /// Registers the Azure Table Repository for a specific entity type in the service collection.
        /// </summary>
        /// <typeparam name="T">The entity type that implements ITableEntity.</typeparam>
        /// <param name="services">The service collection to add the repository to.</param>
        /// <param name="tableName">The name of the Azure table to use for this entity type.</param>
        /// <param name="lifetime">The service lifetime for the repository (default: Scoped).</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddAzureTableRepository&lt;ProductEntity&gt;("ProductsTable");
        /// </code>
        /// </example>
        public static IServiceCollection AddAzureTableRepository<T>(
            this IServiceCollection services, 
            string tableName,
            ServiceLifetime lifetime = ServiceLifetime.Scoped) 
            where T : class, ITableEntity, new()
        {
            var serviceDescriptor = new ServiceDescriptor(
                typeof(IAzureTableRepository<T>),
                serviceProvider => new AzureTableRepository<T>(
                    serviceProvider.GetRequiredService<TableServiceClient>(),
                    serviceProvider.GetRequiredService<ILogger<AzureTableRepository<T>>>(),
                    tableName),
                lifetime);

            services.Add(serviceDescriptor);
            return services;
        }

        /// <summary>
        /// Registers the Azure Table Repository for a specific entity type in the service collection
        /// using a factory function for advanced configuration.
        /// </summary>
        /// <typeparam name="T">The entity type that implements ITableEntity.</typeparam>
        /// <param name="services">The service collection to add the repository to.</param>
        /// <param name="factory">A factory function to create the repository instance.</param>
        /// <param name="lifetime">The service lifetime for the repository (default: Scoped).</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddAzureTableRepository&lt;ProductEntity&gt;(sp => 
        ///     new AzureTableRepository&lt;ProductEntity&gt;(
        ///         sp.GetRequiredService&lt;TableServiceClient&gt;(),
        ///         sp.GetRequiredService&lt;ILogger&lt;AzureTableRepository&lt;ProductEntity&gt;&gt;&gt;(),
        ///         "CustomProductsTable"));
        /// </code>
        /// </example>
        public static IServiceCollection AddAzureTableRepository<T>(
            this IServiceCollection services,
            Func<IServiceProvider, AzureTableRepository<T>> factory,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where T : class, ITableEntity, new()
        {
            var serviceDescriptor = new ServiceDescriptor(
                typeof(IAzureTableRepository<T>),
                factory,
                lifetime);

            services.Add(serviceDescriptor);
            return services;
        }

        /// <summary>
        /// Registers multiple Azure Table Repositories for different entity types in a single call.
        /// All repositories will use the same table name pattern based on the entity type name.
        /// </summary>
        /// <param name="services">The service collection to add the repositories to.</param>
        /// <param name="entityTypes">The entity types to register repositories for.</param>
        /// <param name="tableNamePattern">
        /// Pattern for generating table names. Use {0} as placeholder for entity type name.
        /// Default pattern: "{0}Table" (e.g., ProductTable for Product entity).
        /// </param>
        /// <param name="lifetime">The service lifetime for all repositories (default: Scoped).</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddAzureTableRepositories(
        ///     new[] { typeof(ProductEntity), typeof(UserEntity) },
        ///     "{0}s"); // Results in "Products" and "Users" tables
        /// </code>
        /// </example>
        public static IServiceCollection AddAzureTableRepositories(
            this IServiceCollection services,
            Type[] entityTypes,
            string tableNamePattern = "{0}Table",
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            foreach (var entityType in entityTypes)
            {
                if (!entityType.IsAssignableTo(typeof(ITableEntity)) || entityType.IsAbstract || entityType.IsInterface)
                {
                    throw new ArgumentException($"Type {entityType.Name} must be a concrete class implementing ITableEntity", nameof(entityTypes));
                }

                var tableName = string.Format(tableNamePattern, entityType.Name);
                var repositoryType = typeof(AzureTableRepository<>).MakeGenericType(entityType);
                var interfaceType = typeof(IAzureTableRepository<>).MakeGenericType(entityType);

                var serviceDescriptor = new ServiceDescriptor(
                    interfaceType,
                    serviceProvider =>
                    {
                        var tableServiceClient = serviceProvider.GetRequiredService<TableServiceClient>();
                        var loggerType = typeof(ILogger<>).MakeGenericType(repositoryType);
                        var logger = serviceProvider.GetRequiredService(loggerType);
                        return Activator.CreateInstance(repositoryType, tableServiceClient, logger, tableName)!;
                    },
                    lifetime);

                services.Add(serviceDescriptor);
            }

            return services;
        }
    }
}