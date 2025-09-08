![main build](https://github.com/diginsight/components/actions/workflows/v2_99.Package.CICD.yml/badge.svg?branch=main)<br>
![main build](https://github.com/diginsight/components/actions/workflows/quarto-publish.yml/badge.svg?branch=main)


`Diginsight Components` include **observable** or **optimized extensions** for other technologies, achieved through integration with the **Diginsight** observability platform and **Diginsight Smartcache**.

Each component is designed to address specific functionality areas (e.g., authentication, database access) while maintaining consistent observability and diagnostics capabilities.

## Table of Contents

- [🚀 Quick Start](#-quick-start)
- [❓ Why Diginsight Components](#-why-diginsight-components)
- [📦 What components are available](#-what-components-are-available)
  - [Core Components](#core-components)
  - [Configuration Components](#configuration-components)
  - [Azure Integration Components](#azure-integration-components)
  - [Presentation Components](#presentation-components)
- [🏁 Getting Started](#-getting-started)
  - [Installation](#installation)
  - [Basic Usage](#basic-usage)
  - [Configuration](#configuration)
- [💼 Use Cases & Examples](#-use-cases--examples)
- [🏗️ Architecture Characteristics](#️-architecture-characteristics)
- [🔄 Migration Guide](#-migration-guide)
- [🤝 Contributing](#-contributing)
- [📖 References](#-references)

## 🚀 Quick Start

Get started with Diginsight Components in 3 simple steps:

### 1. Install the core package

```bash
dotnet add package Diginsight.Components.Configuration
```

### 2. Configure your application

```csharp
// Program.cs (ASP.NET Core)
var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration2(loggerFactory);

// Or for Generic Host
Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration2(loggerFactory)
    .Build();
```

### 3. Add observability to your classes

```csharp
using Diginsight.Components;

public class MyService
{
    private static readonly ActivitySource ActivitySource = ObservabilityRegistry.ActivitySource;
    
    public async Task<string> ProcessDataAsync()
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);
        // Your business logic here
        return "processed";
    }
}
```

➡️ **Continue to [Getting Started](#-getting-started) for detailed setup instructions**

# ❓ Why Diginsight Components

**[Diginsight Telemetry](https://diginsight.github.io/telemetry/)** implements observability and diagnostics capabilities for .NET applications using **OpenTelemetry** and **Azure Monitor**.

**[Diginsight Smartcache](https://diginsight.github.io/smartcache/)** implements optimized hybrid caching for .NET applications, combining in-memory and distributed caching strategies.

![Diginsight repositories overview](diginsight-repos-overview.png)

**Diginsight Components** are built to take advantage of these capabilities, enhancing existing libraries with **observability** capabilities and **performance** optimization, where possible.

**Diginsight Components** are built with a layered architecture that promotes **modularity** and **flexibility**.  
All components are available with **abstractions packages** to allow easy integration by means of Dependency Injection.

This modular approach allows you to compose exactly the functionality your application needs—from a minimal observability setup using just the core components, to a full-featured enterprise solution with Azure integrations, advanced caching, and presentation layers.

## 📦 What components are available

Diginsight Components are organized into logical groups, each serving specific architectural concerns:

### **Diginsight.Components.Configuration**

**Provides configuration extensions for application startup and observability.**

Configuration extensions include:

- ⚡ **Enhanced appsettings.json hierarchy** with local overrides
- 🔑 **Azure Key Vault integration** for secure secrets management
- 📈 **OpenTelemetry configuration** helpers
- 🔍 **Activity source detection** and automatic registration
- ☁️ **Azure Monitor integration** out-of-the-box

```bash
dotnet add package Diginsight.Components.Configuration
```

**Key Features:**
- External configuration folder support
- Multi-environment configuration (Development, Production, etc.)
- Tag-based Key Vault secret filtering
- Intelligent file resolution hierarchy

### **Diginsight.Components.Configuration.Abstractions**

**Configuration contracts and options classes.**

- 🛠️ **IOpenTelemetryOptions** interface for configuration
- ⚙️ **OpenTelemetryOptions** implementation with Azure Monitor support
- 🔄 **ConcurrencyOptions** for managing concurrent operations
- 📝 **Dynamic configuration** contracts for runtime updates

### **Diginsight.Components** (Main Library)

Implements:
- .NET applications **observability** for http and database access.
- concurrency control for debugging (eg. ParallelService, IParallelService).
- Helpers for authentication and serialization.

- 🚀 **HTTP client configuration** with built-in observability
- 🔐 **Authentication and security features** (JWT, Microsoft Identity)
- 🔒 **Cryptography support** for secure operations
- 📊 **Performance monitoring** and metrics collection

```bash
dotnet add package Diginsight.Components
```

### **Diginsight.Components.Abstractions**

**Core contracts and interfaces for the components ecosystem.**

- 📋 **Base interfaces** for all Diginsight components
- 🔧 **IDebugService** for conditional debug operations
- 🏗️ **Foundation contracts** for building custom extensions

```bash
dotnet add package Diginsight.Components.Abstractions
```

### **Diginsight.Components.Azure**

Production-ready observability for access to Azure services.

- 🌟 **CosmosDB Observable Extensions**
  - Query extensions with observability integration
  - Advanced metrics (query cost, RU consumption)
  - Automatic performance tracking

- 📋 **Azure Table Observable Extensions**
  - Query extensions with built-in observability
  - Performance metrics and diagnostics

- 🔧 **Other Azure service extensions** (expanding)

```bash
dotnet add package Diginsight.Components.Azure
```

**Perfect for:**
- High-performance Azure applications
- Cost optimization (RU monitoring)
- Production observability requirements

### **Diginsight.Components.Azure.Abstractions**

**Contracts and interfaces for Azure components.**

- 📋 **Base contracts** for Azure service extensions
- 🔧 **IDebugService** specialized for Azure scenarios

### **Diginsight.Components.Presentation** 

**UI and presentation layer components.** (🚧 Under Development)

*Currently in development - watch this space for updates!*

### **Diginsight.Components.Presentation.Abstractions**

**Presentation layer contracts.** (🚧 Under Development)

*Foundational contracts for future UI components.*

---

## 🎯 **Choose Your Components**

| **Scenario** | **Recommended Packages** |
|-------------|-------------------------|
| **Basic observability** | `Diginsight.Components.Configuration` |
| **Azure applications** | `Diginsight.Components.Configuration` + `Diginsight.Components.Azure` |
| **Enterprise applications** | All configuration and Azure components |
| **Custom extensions** | Add corresponding `.Abstractions` packages |

## 🏁 Getting Started

### Installation

Choose the components that fit your needs:

#### **Minimal Setup** (Basic observability)
```bash
dotnet add package Diginsight.Components.Configuration
```

#### **Azure Applications**
```bash
dotnet add package Diginsight.Components.Configuration
dotnet add package Diginsight.Components.Azure
```

#### **Full Enterprise Setup**
```bash
dotnet add package Diginsight.Components.Configuration
dotnet add package Diginsight.Components.Azure
dotnet add package Diginsight.Components
```

### Basic Usage

#### 1. **Configure Your Application**

For **ASP.NET Core**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add enhanced configuration support
builder.Host.ConfigureAppConfiguration2(loggerFactory);

var app = builder.Build();
```

For **Generic Host**:
```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration2(loggerFactory)
    .Build();
```

#### 2. **Add Observability to Your Services**

```csharp
using Diginsight.Components;

public class OrderService
{
    private static readonly ActivitySource ActivitySource = ObservabilityRegistry.ActivitySource;
    
    public async Task<Order> ProcessOrderAsync(int orderId)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { orderId });
        activity?.SetTag("order.id", orderId);
        
        // Your business logic with automatic observability
        var order = await _repository.GetOrderAsync(orderId);
        await _paymentService.ProcessPaymentAsync(order);
        
        return order;
    }
}
```

#### 3. **Use Azure Extensions** (if using Azure components)

```csharp
using Diginsight.Components.Azure;

public class ProductService
{
    private readonly Container _container;
    
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        // CosmosDB query with automatic observability and metrics
        var query = _container.GetItemQueryIterator<Product>(
            "SELECT * FROM c WHERE c.category = 'electronics'"
        );
        
        // Query cost and performance metrics automatically recorded
        var products = new List<Product>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            products.AddRange(response);
        }
        
        return products;
    }
}
```

### Configuration

#### **appsettings.json Example**
```json
{
  "OpenTelemetry": {
    "TracingEnabled": true,
    "MetricsEnabled": true,
    "AzureMonitor": {
      "ConnectionString": "your-application-insights-connection-string"
    }
  },
  "AzureKeyVault": {
    "Uri": "https://your-keyvault.vault.azure.net/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id"
  }
}
```

#### **Environment-Specific Configuration**

The system automatically loads configuration files in this order:
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. `appsettings.local.json` (Development only)
4. `appsettings.{Environment}.local.json` (Development only)
5. Azure Key Vault secrets
6. Environment variables

## 💼 Use Cases & Examples

### 🏢 **Enterprise Application**
Build a full-stack enterprise application with complete observability:

```csharp
// Startup configuration
builder.Host.ConfigureAppConfiguration2(loggerFactory);

// Service with observability
public class CustomerService
{
    private static readonly ActivitySource ActivitySource = ObservabilityRegistry.ActivitySource;
    
    public async Task<Customer> GetCustomerAsync(int customerId)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { customerId });

        activity?.SetTag("customer.id", customerId);
        
        var customer = await _cosmosContainer.ReadItemAsync<Customer>(
            customerId.ToString(), 
            new PartitionKey(customerId)
        );
        
        return customer.Resource;
    }
}
```

### ☁️ **Azure-First Application**
Leverage Azure services with built-in cost optimization:

```csharp
// CosmosDB with automatic RU monitoring
var queryIterator = _container.GetItemQueryIterator<Product>(queryDefinition);
while (queryIterator.HasMoreResults)
{
    // Request units and performance metrics automatically captured
    var response = await queryIterator.ReadNextAsync();
    // Process results...
}
```

### 🔧 **Microservice Architecture**
Build observable microservices with consistent telemetry:

```csharp
// Each service automatically contributes to distributed tracing
public class OrderProcessingService
{
    public async Task ProcessAsync(Order order)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, () => new { order });

        // Calls to other services automatically traced
        await _inventoryService.ReserveItemsAsync(order.Items);
        await _paymentService.ProcessPaymentAsync(order.Payment);
        await _shippingService.CreateShipmentAsync(order);
    }
}
```

## 🔄 Migration Guide

### **From Basic .NET Configuration**

**Before:**
```csharp
var builder = WebApplication.CreateBuilder(args);
// Basic configuration only
```

**After:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration2(loggerFactory); // Enhanced configuration
```

### **From Manual OpenTelemetry Setup**

**Before:**
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder.AddAspNetCoreInstrumentation())
    .WithMetrics(builder => builder.AddAspNetCoreInstrumentation());
```

**After:**
```csharp
// Configuration handled automatically through appsettings.json
builder.Host.ConfigureAppConfiguration2(loggerFactory);
```

## 🤝 Contributing

We welcome contributions! Here's how you can help:

1. 🐛 **Report Issues**: Found a bug? [Open an issue](https://github.com/diginsight/components/issues)
2. 💡 **Suggest Features**: Have ideas? [Start a discussion](https://github.com/diginsight/components/discussions)
3. 🔧 **Submit PRs**: Ready to contribute code? [Submit a pull request](https://github.com/diginsight/components/pulls)
4. 📖 **Improve Documentation**: Help make our docs better

### Development Setup
```bash
git clone https://github.com/diginsight/components.git
cd components
dotnet build
dotnet test
```


# 🏗️ Architecture Characteristics

Diginsight Components follow consistent architectural patterns:

## 🏗️ **Design Principles**

1. **🔍 Observability-First**: Every component includes built-in ActivitySource creation and ObservabilityRegistry integration
2. **🧩 Modular Architecture**: Use components independently or together with clear separation of concerns  
3. **☁️ Azure-Native**: Deep integration with Azure services and monitoring capabilities
4. **📊 OpenTelemetry Standards**: Industry-standard observability patterns for metrics, traces, and logs
5. **💉 Dependency Injection Ready**: All components provide abstractions for easy IoC integration

## 📋 **Common Patterns**

Every Diginsight component follows these patterns:

- **ObservabilityRegistry Integration**: Automatic ActivitySource creation and registration
- **Consistent Logging**: Structured logging infrastructure across all components  
- **Azure Monitor Ready**: Built-in support for Azure Application Insights
- **Performance Optimized**: Smart caching and performance monitoring capabilities

## References

### 📚 **Diginsight Ecosystem**

- [Diginsight Telemetry](https://diginsight.github.io/telemetry/) - Core observability platform documentation
- [Diginsight Smartcache](https://diginsight.github.io/smartcache/) - Hybrid caching strategies and implementation
- [Diginsight GitHub Organization](https://github.com/diginsight) - Source code and community resources
- [Components Repository](https://github.com/diginsight/components) - This repository's source code

### 🛠️ **OpenTelemetry & Observability**

- [OpenTelemetry Documentation](https://opentelemetry.io/docs/) - Official OpenTelemetry standards and guides
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/) - .NET-specific implementation guides  
- [OpenTelemetry Instrumentation](https://opentelemetry.io/docs/concepts/instrumentation/) - Instrumentation best practices
- [Distributed Tracing Concepts](https://opentelemetry.io/docs/concepts/observability-primer/#distributed-traces) - Understanding distributed systems observability

### ☁️ **Azure Platform**

- [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/) - Azure's comprehensive observability platform
- [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) - Application performance monitoring
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/) - Secrets and key management service
- [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/) - Globally distributed, multi-model database
- [Azure Table Storage](https://docs.microsoft.com/en-us/azure/storage/tables/) - NoSQL structured data storage service

### 🔧 **.NET Platform & Tools**

- [.NET Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) - Configuration providers and patterns
- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/) - Authentication and authorization
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Cross-platform web application framework
- [Generic Host](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host) - .NET application hosting model

### 📊 **Monitoring & Performance**

- [Metrics Collection Best Practices](https://opentelemetry.io/docs/concepts/observability-primer/#metrics) - Understanding application metrics
- [Structured Logging](https://opentelemetry.io/docs/concepts/observability-primer/#logs) - Modern logging approaches
- [Azure Monitor Metrics](https://docs.microsoft.com/en-us/azure/azure-monitor/platform/metrics-supported) - Available Azure platform metrics

---

## 💬 **Community & Support**

- **GitHub Issues**: [Report bugs and request features](https://github.com/diginsight/components/issues)
- **Discussions**: [Community discussions and Q&A](https://github.com/diginsight/components/discussions)  
- **Diginsight Organization**: [Explore all Diginsight projects](https://github.com/diginsight)

---

*📅 **Last Updated**: August 2025 | ⭐ **Version**: Compatible with .NET 8.0+ | 📜 **License**: [View License](LICENSE.md)*
