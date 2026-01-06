# Diginsight Components - Copilot Instructions

## Repository Overview

This repository contains **Diginsight Components** - observable-first .NET libraries that provide built-in observability for common development scenarios like Azure integrations, configuration management, and data access.

**Architecture Philosophy**: Rather than adding observability as an afterthought, these components are designed from the ground up with OpenTelemetry integration, structured logging, and performance metrics built-in. They serve as drop-in replacements for common .NET patterns (Azure SDK operations, configuration, data access) with automatic telemetry.

**Relation to Diginsight Telemetry**: These components build upon and integrate with the core Diginsight Telemetry library (in a separate repository), which provides the foundational activity-based tracing, class-aware options, and dynamic configuration capabilities.

## Architecture

### Two-Tier Component Structure

**Layer 1: Core Telemetry Foundation** (from separate Diginsight Telemetry repository)
- **Diginsight.Core**: Foundation - class-aware options, enhanced DI, logging infrastructure, volatile configuration
- **Diginsight.Diagnostics**: Activity lifecycle logging, console formatters, metrics collection, timing infrastructure
- **Diginsight.Stringify**: Object rendering for logs - customizable stringification with depth control
- **Diginsight.Diagnostics.OpenTelemetry**: OpenTelemetry bridge - tracers, meters, exporters

**Layer 2: Observable-First Domain Components** (this repository)

**Core Foundation:**
- **Diginsight.Components**: Base utilities (HTTP extensions, parallel processing, property change handlers) - foundation that all components build upon
- **Diginsight.Components.Abstractions**: Core interfaces and contracts

**Configuration Orchestration** (can configure all other components):
- **Diginsight.Components.Configuration**: Cross-component configuration, centralized activation via `AddObservability()`, feature toggles
- **Diginsight.Components.Configuration.Abstractions**: Configuration interfaces

**Domain-Specific Components** (independent from each other):
- **Diginsight.Components.Azure**: CosmosDB, Azure Tables with automatic RU metrics, query logging
- **Diginsight.Components.Azure.Abstractions**: Azure service interfaces
- **Diginsight.Components.Presentation**: UI/presentation helpers with observability
- **Diginsight.Components.Presentation.Abstractions**: Presentation interfaces

### Strategic Dependency Architecture

```
Configuration Components (Orchestrator)
├── Azure Components (domain-specific)
├── Presentation Components (domain-specific)
└── Core Components (Foundation)
    └── Diginsight Telemetry (External dependency)
```

**Key Principles:**
1. **Configuration as Orchestrator**: `Diginsight.Components.Configuration` can reference and configure all other components
2. **Domain Independence**: Azure and Presentation components are isolated - they don't reference each other
3. **Core Foundation**: `Diginsight.Components` provides shared utilities usable by all components
4. **Abstractions Pattern**: Every component has a `.Abstractions` package for clean DI and testing
5. **Direct vs Package References**: Uses conditional imports - can reference Diginsight Telemetry packages from NuGet OR directly from local solution via `Directory.Build.props.user`

### Key Architectural Patterns

**1. Observable Extension Pattern (Core Innovation):**
Every Azure SDK operation is wrapped with an `*Observable*` extension method that adds automatic tracing:
```csharp
// Original Azure SDK
var response = await tableClient.AddEntityAsync(entity);

// Observable wrapper adds telemetry
var response = await tableClient.AddEntityObservableAsync(entity);
// ✅ Automatic: activity creation, logging with emojis (➕ for add), exception handling, response tracking
```

Extensions follow strict naming: `{OriginalMethodName}Observable[Async]`. Located in `*Extensions/*ObservableExtensions.cs` files.

**2. Observability Static Helper:**
Each component package has an `Observability.cs` with:
```csharp
internal static class Observability
{
    public static readonly ActivitySource ActivitySource = new(Assembly.GetExecutingAssembly().GetName().Name!);
    public static ILoggerFactory? LoggerFactory => LoggerFactoryStaticAccessor.LoggerFactory;
}
```
All observable extensions use this to create activities and loggers.

**3. Configuration-Driven Setup:**
`AddObservability()` in Configuration package orchestrates all components based on `appsettings.json`:
```csharp
// Single call configures: OpenTelemetry, metrics, console logging, Azure Monitor exporters
services.AddObservability(configuration, hostEnvironment, out IOpenTelemetryOptions options);
```

**4. Metric Collection via Activity Listeners:**
Azure components register `IActivityListenerRegistration` to capture metrics (e.g., CosmosDB RU costs) from activity tags:
```csharp
// In ServiceCollectionExtensions
services.AddCosmosDbQueryCostMetricRecorder<QueryCostMetricRecorderRegistration>();
// Hooks into activities tagged with query costs, emits OpenTelemetry metrics
```

**5. Activity-Based Tracing:**
All component operations use standard pattern from Diginsight Telemetry:
```csharp
using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { param1, param2 });
```

**6. Class-Aware Configuration:**
Options can vary by calling class context via `IClassAwareOptions<T>`. Enable with `services.AddClassAwareOptions()`. This powers component-level feature flags and per-class log levels.

**7. Deferred/Early Logging:**
Before DI container is built, use `DeferredLoggerFactory` and `DeferredActivityLifecycleLogEmitter`. They buffer events and flush to real implementations once available via `FlushOnCreateServiceProvider()`.

**8. Dynamic Configuration:**
Options implementing `IDynamicallyConfigurable` can change at runtime without app restart. Paired with `VolatileConfiguration` system for HTTP header-driven config overrides.

## Essential Setup Patterns

### Centralized Observability Activation
```csharp
// Program.cs - Configuration component orchestrates everything
var builder = WebApplication.CreateBuilder(args);

// Single call activates: Diginsight Core + Diagnostics, OpenTelemetry, Azure Monitor, Console logging
builder.Services.AddObservability(
    builder.Configuration, 
    builder.Environment,
    out IOpenTelemetryOptions openTelemetryOptions
);

// Add component-specific metrics
builder.Services.AddCosmosDbQueryCostMetricRecorder();
```

### Using Observable Azure Components
```csharp
// CosmosDB with automatic RU metrics and query tracing
var iterator = container.GetItemQueryIteratorObservable<Product>(queryDefinition);
await foreach (var response in iterator)
{
    // Each read automatically logged with: query text, RU cost, duration, container/database names
}

// Azure Tables with operation tracing
await tableClient.AddEntityObservableAsync(entity);
// Logs: ➕ operation, partition/row keys, entity details
```

### Dynamic Logging via HTTP Headers
```csharp
// In ASP.NET Core projects, enable log level overrides:
services.Configure<DiginsightDistributedContextOptions>(options =>
{
    // Exclude these from baggage propagation:
    options.NonBaggageKeys.Add("Log-Level");
    options.NonBaggageKeys.Add("Metric-Recording-Enabled");
});
```
Clients can then send `Log-Level: Debug` headers to get detailed traces for specific requests on live environments.

## Development Workflows

### Building
Standard .NET SDK commands from `src/`:
```bash
dotnet build Diginsight.Components.sln
dotnet test
```

Three solution files available:
- `Diginsight.Components.sln` - Full solution
- `Diginsight.Components.Build.sln` - Build-optimized subset
- `Diginsight.Components.Debug.sln` - Debug-optimized subset

Build configuration in `Directory.Build.props`:
- LangVersion: `13` (C# 13)
- ImplicitUsings: `enable`
- Nullable: `enable` with warnings as errors
- Package lock files enabled
- Strong-name signing via `diginsight.snk`
- OpenTelemetry version references managed centrally

### Local Development with Diginsight Telemetry
Create `Directory.Build.props.user` to reference local Diginsight Telemetry source:
```xml
<PropertyGroup>
  <DiginsightCoreSolutionDirectory>..\..\telemetry\src\</DiginsightCoreSolutionDirectory>
  <DiginsightCoreDirectImport>true</DiginsightCoreDirectImport>
</PropertyGroup>
```
This switches from NuGet packages to project references for easier debugging.

### Testing
Test projects follow convention: `<PackageName>.Tests` (not shown in structure but standard pattern).

### Debugging
Enable full observability during development:
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Diginsight": "Debug"
    }
  },
  "Diginsight": {
    "Activities": {
      "LogActivities": true,
      "ActivitySources": { "*": true }
    }
  }
}
```

### Documentation Site
Root-level Quarto site (`docs/` output from `*.md` sources). Build with:
```bash
quarto render
```

Markdown articles in `src/docs/` with dual metadata structure (top YAML for Quarto, bottom HTML comment for validation tracking).

## Coding Conventions

### Observable Extension Pattern
When wrapping Azure SDK or other operations:
```csharp
// Pattern: {OriginalMethodName}Observable[Async]
public static async Task<Response> AddEntityObservableAsync<T>(
    this TableClient tableClient, T entity, CancellationToken ct = default) 
    where T : ITableEntity
{
    var logger = Observability.LoggerFactory?.CreateLogger(typeof(AzureTableObservableExtensions));
    using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { entity });

    logger.LogDebug("➕ AzureTable add for type:{Type}, table:{TableName}", typeof(T).Name, tableClient.Name);
    logger.LogDebug("➕ partitionKey:{PartitionKey}, rowKey:{RowKey}", entity.PartitionKey, entity.RowKey);

    var response = await tableClient.AddEntityAsync(entity, ct);
    
    activity?.SetOutput(response);
    return response;
}
```

**Naming conventions:**
- Sync method: `{MethodName}Observable`
- Async method: `{MethodName}ObservableAsync`
- Use emojis consistently: 🔍 query, ➕ add, 🔄 update, 🗑️ delete, ❌ error, ⚠️ warning

### Dependency Injection Extensions
- Place in `ServiceCollectionExtensions` classes
- Use `TryAdd*` for non-destructive registration
- Chain setup methods: `AddClassAwareOptions()` → `AddActivityListenersAdder()` → component-specific setup
- Mark internal setup helpers with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`

### Observability.cs Pattern
Every component assembly includes this static helper:
```csharp
internal static class Observability
{
    public static readonly ActivitySource ActivitySource = new(Assembly.GetExecutingAssembly().GetName().Name!);
    public static ILoggerFactory? LoggerFactory => LoggerFactoryStaticAccessor.LoggerFactory;
}
```
Never instantiate ActivitySource multiple times - use this singleton.

### Activity Logging Pattern
```csharp
// Standard method activity wrapper:
using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { param1, param2 });
try
{
    // Work here
    activity?.SetOutput(result); // Optional - captures return values
    return result;
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    throw;
}
```

For repository patterns, see [`AzureTableRepository.cs`](src/Diginsight.Components.Azure/Repositories/AzureTableRepository.cs) - comprehensive example with lazy initialization, error handling, and consistent logging.

### Metric Collection Registration
Azure components use activity listeners to collect metrics:
```csharp
public static IServiceCollection AddCosmosDbQueryCostMetricRecorder<TRegistration>(this IServiceCollection services)
    where TRegistration : QueryCostMetricRecorderRegistration
{
    services
        .AddClassAwareOptions()
        .AddActivityListenersAdder()
        .AddMetrics();

    services.TryAddSingleton<QueryCostMetricRecorder>();
    services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, TRegistration>());
    
    return services;
}
```

Metrics are captured from activity tags set in observable extensions.

### Options Classes
- Implement interfaces from `Diginsight.Options` namespace
- Volatile options: implement `IVolatileConfiguration`
- Class-aware: no special interface, just configure with `services.Configure<TOptions>()`
- Dynamic: implement `IDynamicallyConfigurable` + call `services.FlagAsDynamic<TOptions>(name)`

### Logging
Use interpolated string handlers for structured logging (net7.0+):
```csharp
logger.LogDebug($"Processing order {orderId} for user {userId}");
// Auto-captures as structured properties, not concatenated string
```

## Key Files and References

- [`src/Diginsight.Components/Observability.cs`](src/Diginsight.Components/Observability.cs) - Observability pattern template
- [`src/Diginsight.Components.Azure/ServiceCollectionExtensions.cs`](src/Diginsight.Components.Azure/ServiceCollectionExtensions.cs) - DI setup for Azure components
- [`src/Diginsight.Components.Configuration/Hosting/Extensions/ObservabilityExtensions.cs`](src/Diginsight.Components.Configuration/Hosting/Extensions/ObservabilityExtensions.cs) - `AddObservability()` orchestration
- [`src/Diginsight.Components.Azure/Extensions/CosmosDbObservableExtensions.cs`](src/Diginsight.Components.Azure/Extensions/CosmosDbObservableExtensions.cs) - CosmosDB observable wrappers
- [`src/Diginsight.Components.Azure/Extensions/AzureTableObservableExtensions.cs`](src/Diginsight.Components.Azure/Extensions/AzureTableObservableExtensions.cs) - Azure Table observable wrappers
- [`src/Diginsight.Components.Azure/Repositories/AzureTableRepository.cs`](src/Diginsight.Components.Azure/Repositories/AzureTableRepository.cs) - Repository pattern implementation
- [`src/Directory.Build.props`](src/Directory.Build.props) - Build configuration
- [`README.md`](README.md) - High-level project overview and usage examples

## Cross-Cutting Concerns

- **W3C Trace Context**: Full support via OpenTelemetry integration; TraceIds preserved across services
- **Performance**: Dynamic compilation, intelligent sampling, automatic truncation to minimize overhead
- **Multi-Framework**: Single codebase targets net8.0 and net9.0
- **Backward Compat**: Can reference Diginsight Telemetry directly for latest features or via NuGet for stability

## Documentation Articles (Dual Metadata)

Articles in `src/docs/` use dual YAML metadata blocks:
1. **Top YAML** (lines 1-X): Quarto frontmatter (`title`, `author`, `date`, `categories`) - edit manually only
2. **Bottom HTML Comment** (after References): Validation tracking (`validations`, `article_metadata`) - updated by validation tools

When working with documentation:
- Never modify top YAML block via automated tools
- Update bottom metadata block for validation results only
- Maintain consistent article structure (TOC, body, references)

