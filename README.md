![main build](https://github.com/diginsight/components/actions/workflows/v2_99.Package.CICD.yml/badge.svg?branch=main)<br>
![main build](https://github.com/diginsight/components/actions/workflows/quarto-publish.yml/badge.svg?branch=main)

# 📖 Introduction

**Diginsight Components** are **observable-first** .NET libraries that bring **built-in observability** to common development scenarios like Azure integrations, configuration management, and data access.

Instead of adding observability as an afterthought, these components are designed from the ground up with **OpenTelemetry integration**, **structured logging**, and **performance metrics** built-in.

> 🎯 **The Problem**: Traditional libraries require you to manually add logging, tracing, and monitoring to understand what's happening in production.  
> ✨ **The Solution**: Diginsight Components provide the same functionality with observability **already integrated**.

## 🚀 **Quick Value Proposition**

- **🔍 Zero-Config Observability**: Get detailed telemetry without writing boilerplate logging code
- **📊 Built-in Metrics**: Automatic performance counters and business metrics (e.g., CosmosDB RU consumption)
- **🔗 Distributed Tracing**: Full request tracing across components with OpenTelemetry
- **🛠️ Drop-in Replacements**: Use familiar APIs with enhanced observability features
- **☁️ Cloud-Ready**: Optimized for Azure services with built-in monitoring integration

# 📋 Table of Contents

- [💡 Why Choose Diginsight Components](#-why-choose-diginsight-components)
- [🏗️ Architecture Overview](#️-architecture-overview)
- [🚀 Getting Started](#-getting-started)
- [📦 Available Components](#-available-components)
  - [⚙️ Configuration Components](#️-configuration-components)
  - [🎯 Core Components](#-core-components)
  - [🌐 Azure Components](#-azure-components)
  - [🎨 Presentation Components](#-presentation-components)
- [💼 Use Cases & Examples](#-use-cases--examples)
- [📚 References](#-references)

# 💡 Why Choose Diginsight Components

## 🔍 **Traditional Approach vs. Diginsight Components**

### Without Diginsight Components:
```csharp
// Manual logging and monitoring
_logger.LogInformation("Starting CosmosDB query");
var stopwatch = Stopwatch.StartNew();
try 
{
    var results = await container.GetItemQueryIterator<Product>(query).ReadNextAsync();
    _logger.LogInformation("Query completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
    // Manual RU consumption tracking...
}
catch (Exception ex)
{
    _logger.LogError(ex, "Query failed");
    throw;
}
```

### With Diginsight Components:
```csharp
// Automatic observability built-in
var results = await container.GetItemQueryIteratorObservable<Product>(query).ReadNextAsync();
// ✅ Automatic logging, tracing, RU metrics, error handling
```

## 🎯 **Key Benefits**

- **⚡ Faster Development**: Focus on business logic, not observability plumbing
- **🔍 Better Debugging**: Rich telemetry helps identify issues quickly
- **📊 Production Insights**: Built-in metrics reveal performance bottlenecks
- **🛡️ Reliability**: Consistent error handling and retry patterns
- **🔄 Maintainability**: Standardized observability across your entire application

# 🏗️ Architecture Overview

**Diginsight Components** follow a **modular architecture** with **selective dependencies** based on architectural roles:

```
┌─────────────────────────────────────────────────────────────────┐
│                      Your Application                           │
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ ⚙️ Configuration │  │  � Core       │  │ 🌐 Azure       │  │
│  │   Components    │  │  Components    │  │  Components    │  │
│  │                 │  │                │  │                │  │
│  │ ┌─────────────┐ │  │ ┌─────────────┐ │  │ ┌─────────────┐ │  │
│  │ │.Abstractions│ │  │ │.Abstractions│ │  │ │.Abstractions│ │  │
│  │ └─────────────┘ │  │ └─────────────┘ │  │ └─────────────┘ │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│                                                                 │
│  ┌─────────────────┐                                            │
│  │ 🎨 Presentation │     ← Choose only what you need            │
│  │   Components    │                                            │
│  │                 │                                            │
│  │ ┌─────────────┐ │                                            │
│  │ │.Abstractions│ │                                            │
│  │ └─────────────┘ │                                            │
│  └─────────────────┘                                            │
└─────────────────────────────────────────────────────────────────┘
```

### **Dependency Architecture**

The diagram above shows a **simplified view**. In reality, the architecture has **strategic dependencies**:

```
⚙️ Configuration Components (Orchestrator)
├── 🌐 Azure Components (domain-specific, independent from each other)
├── 🎨 Presentation Components (domain-specific, independent from each other)
└── 🎯 Core Components (Foundation)
       (all components depend on Core)
```

### **Key Architectural Principles**

1. **🧩 Selective Modularity**: Components have **strategic dependencies** based on their architectural role
2. **🎯 Core Foundation**: `Diginsight.Components` provides foundational utilities that **all components can use**
3. **⚙️ Configuration Orchestration**: `Configuration` components can **use and configure all other components**
4. **🌐 Domain Independence**: Domain-specific components (Azure, Presentation) are **independent from each other**
5. **🔌 Abstractions Pattern**: Every component comes with its `.Abstractions` assembly for clean dependency injection
6. **📊 Observable by Default**: Every component includes built-in telemetry and metrics

**⚙️ Configuration Components Orchestration**

**`Diginsight.Components.Configuration`** provides **configurable activation capabilities**:

- **🌐 Cross-Component Configuration**: Can configure and activate features across different component assemblies
- **📋 Centralized Activation**: Provides configurable activation capabilities for various components
- **🔧 Component Orchestration**: Manages startup, initialization, and configuration of other components
- **🎛️ Feature Toggles**: Enables/disables component features through configuration

```csharp
// based on appsettings-json configuration, 
// Configuration Components activate observability for all other components
services.AddObservability(configuration, hostEnvironment, out IOpenTelemetryOptions openTelemetryOptions);
```


### **Component Assembly Pattern**

Each functional area provides **two assemblies**:

- **`Diginsight.Components.{Area}`** - Implementation with observable extensions
- **`Diginsight.Components.{Area}.Abstractions`** - Interfaces and contracts for DI

This allows you to:

- Reference only abstractions in your domain layer
- Reference implementations in your composition root
- Test easily with clean interfaces
- Avoid circular dependencies

# 🚀 Getting Started

## 📦 **Installation**

Install the components you need via NuGet:

```bash
# For enhanced configuration
dotnet add package Diginsight.Components.Configuration

# For Azure services (CosmosDB, Tables, etc.)
dotnet add package Diginsight.Components.Azure

# For core utilities
dotnet add package Diginsight.Components
```

Every version is also published as a GitHub Release carrying the same `.nupkg` and `.snupkg` bytes,
plus `SHA256SUMS` and `release-manifest.json`. Use those assets when a version has not yet
propagated to your NuGet feed.

## 🔧 **Building this repository**

```powershell
./eng/Download-PackageRelease.ps1
dotnet restore src/Diginsight.Components.Build.slnx
```

The first command downloads and verifies the pinned `diginsight/telemetry` and
`diginsight/smartcache` releases into `artifacts/packages`, which is a declared package source. It
is only needed while a pinned upstream version has not reached the corporate proxy. See
[eng/README.md](eng/README.md) for the full runbook.

## ⚙️ **Basic Setup**

```csharp
// Program.cs or Startup.cs
builder.Services.AddDiginsightConfiguration()
                .AddDiginsightAzure()
                .AddOpenTelemetry(); // Automatic observability
```

## 🎯 **Your First Observable Operation**

```csharp
// Instead of regular CosmosDB client
var results = await container.GetItemQueryIteratorObservable<Product>(
    "SELECT * FROM c WHERE c.category = @category"
).ReadNextAsync();

// ✅ Automatically provides:
// - Distributed tracing spans
// - RU consumption metrics  
// - Query performance logging
// - Error tracking and correlation
```

# 📦 Available Components

Components are organized by functionality and include both implementation and abstraction packages for clean dependency injection.

## ⚙️ **Configuration Components**

**Cross-cutting configuration management with component activation capabilities**

### `Diginsight.Components.Configuration`

- **Purpose**: **Cross-component configuration orchestrator** that provides enhanced .NET configuration with observability and component activation
- **Key Features**:
  - 🔐 **Azure Key Vault Integration**: Automatic authentication and secret management
  - 🎛️ **Component Activation**: Configurable activation of other Diginsight components
  - 📋 **Startup Diagnostics**: Configuration troubleshooting and validation
  - 🌍 **Environment-Specific Config**: Support for `.local`, `.environment` files
  - 📊 **Configuration Observability**: Full visibility into configuration sources and values
  - 🔧 **Cross-Component Settings**: Centralized configuration for Azure, Core, and Presentation components
- **Use Cases**: 
  - Applications requiring centralized configuration management
  - Multi-component applications needing coordinated activation
  - Secure configuration with Azure Key Vault integration
  - Development environments with local configuration overrides

### `Diginsight.Components.Configuration.Abstractions`

- **Purpose**: Interfaces and contracts for configuration orchestration
- **Key Features**: Configuration provider interfaces, component activation contracts, cross-cutting configuration patterns

## 🎯 **Core Components**

**Essential functionality with built-in observability**

### `Diginsight.Components`
- **Purpose**: Common utilities with observable patterns
- **Key Features**:
  - 🌐 Observable HTTP client configuration
  - 🔐 Authentication and JWT token handling with telemetry
  - 🛡️ Cryptography support with operation tracking
  - ⚙️ Microsoft Identity integration
- **Use Cases**: Any application needing HTTP clients, authentication, or security features

### `Diginsight.Components.Abstractions`
- **Purpose**: Core interfaces for all components
- **Key Features**: `IDebugService` interface, base contracts, common patterns

## 🌐 **Azure Components**

**Azure services with built-in observability and advanced metrics**

### `Diginsight.Components.Azure`
- **Purpose**: Observable Azure service integrations
- **Key Features**:
  - 📊 **CosmosDB Extensions**: Query operations with RU consumption metrics, performance tracking
  - 🗄️ **Azure Table Extensions**: Table operations with built-in observability
  - ☁️ **Other Azure Services**: Observable patterns for various Azure APIs
- **Use Cases**: Applications using Azure data services needing performance insights

### `Diginsight.Components.Azure.Abstractions`
- **Purpose**: Interfaces for Azure service integrations
- **Key Features**: Base contracts for Azure observable extensions

## 🎨 **Presentation Components**

**UI and API layer components** *(In Development)*

### `Diginsight.Components.Presentation`
- **Purpose**: UI/API specific observability features
- **Status**: Currently in development
- **Planned Features**: MVC/API telemetry, request/response tracking, user journey analytics

# 💼 Use Cases & Examples

## 🏢 **Enterprise Application**
```csharp
// Full stack observability
builder.Services
    .AddDiginsightConfiguration()  // 📋 Observable configuration
    .AddDiginsightAzure()          // ☁️ Azure service telemetry
    .AddDiginsightPresentation();  // 🎨 API/UI observability
```

## 🚀 **Microservice with CosmosDB**
```csharp
// Focus on Azure data services
builder.Services
    .AddDiginsightConfiguration()
    .AddDiginsightAzure();

// Your service gets automatic observability
public class ProductService
{
    public async Task<Product> GetProductAsync(string id)
    {
        // Automatic: tracing, logging, RU metrics, error handling
        return await _container.ReadItemObservableAsync<Product>(id, new PartitionKey(id));
    }
}
```

## 🛠️ **Development & Debugging**
```csharp
// Enhanced configuration for local development
builder.Configuration
    .AddDiginsightConfiguration()  // Loads .local files, enables debug logging
    .AddKeyVault();                // Connects to dev Key Vault automatically
```

# 📚 References

### 📘 Diginsight Documentation
- [Diginsight Telemetry](https://diginsight.github.io/telemetry/) - Core observability platform
- [Diginsight Smartcache](https://diginsight.github.io/smartcache/) - Hybrid caching strategies
- [Diginsight GitHub](https://github.com/diginsight) - Source code and examples

### 🔭 OpenTelemetry & Observability
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/) - .NET observability standards
- [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/) - Azure observability platform
- [Distributed Tracing](https://opentelemetry.io/docs/concepts/observability-primer/#distributed-traces) - Tracing concepts

### ☁️ Azure & .NET Technologies
- [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/) - Document database service
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/) - Secrets management
- [.NET Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) - Configuration patterns
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Web framework
