![main build](https://github.com/diginsight/components/actions/workflows/v2_99.Package.CICD.yml/badge.svg?branch=main)<br>
![main build](https://github.com/diginsight/components/actions/workflows/quarto-publish.yml/badge.svg?branch=main)

# 📖 Introduction

`Diginsight Components` include <mark>**observable extensions** for Azure technologies</mark>, achieved through integration with the **Diginsight** observability.

Also, <mark>utility components</mark> are provided to enhance **application observability** and **troubleshooting** in those technologies.

Each component is designed to address specific functionality areas (e.g., authentication, data access, parallel processing) while maintaining **consistent observability and diagnostics capabilities**.

# 📋 Table of Contents

- [💡 Why Diginsight Components](#-why-diginsight-components)
- [📦 What components are available](#-what-components-are-available)
  - [⚙️ Diginsight.Components.Configuration](#️-diginsightcomponentsconfiguration)
  - [🎯 Diginsight.Components](#-diginsightcomponents-core)
  - [🌐 Diginsight.Components.Azure](#-diginsightcomponentsazure)
  - [🎨 Diginsight.Components.Presentation](#-diginsightcomponentspresentation)
- [🏛️ Architecture Characteristics](#️-architecture-characteristics)
- [📚 References](#-references)
  - [📘 Diginsight Documentation](#-diginsight-documentation)
  - [🔭 OpenTelemetry](#-opentelemetry)
  - [☁️ Azure Services](#️-azure-services)
  - [⚡ .NET Technologies](#-net-technologies)
  - [📊 Observability & Monitoring](#-observability--monitoring)

# 💡 Why Diginsight Components

**[Diginsight Telemetry](https://diginsight.github.io/telemetry/)** implements observability and diagnostics capabilities for .NET applications using **OpenTelemetry** and **Azure Monitor**.
![alt text](<./src/docs/001.01a diginsight telemetry repo.png>)
**Diginsight telemetry** can be integrated into applications to ensure **observability of their** (business) **logic**.

**[Diginsight Components](https://diginsight.github.io/components/)** applies the observability model to **database access**, **authentication**, **api and http invocations** etc. so that we can integrate them without worrying about making them **observable** and **easy to troubleshoot**.
![alt text](<./src/docs/001.01b diginsight components repo.png>)


**Diginsight Components** are built with a modular architecture so different assemblies are provided with helpers for different technologies.
For every assembly, components are normally provided as **extension methods** or **services** that can be obtained by dependency injection.
For every component, an associated **abstractions package** defines its core interfaces and contracts.

# 📦 What components are available

The **core components** provide essential functionality like authentication and HTTP client configuration. 

**Technology-specific components** (like Azure integrations) extend the core capabilities for particular platforms. Finally, **presentation components** handle UI-related concerns.

This modular approach allows you to compose exactly the functionality your application needs—from a minimal observability setup using just the core components, to a full-featured enterprise solution with Azure integrations, advanced caching, and presentation layers.

## ⚙️ **Diginsight.Components.Configuration**

- **Purpose**: Observable extensions for Diginsight configuration with Azure Key Vault, Console, Log4Net, and OpenTelemetry.
- **Key features**:
  - Azure Key Vault integration.
  - Activity source detection and registration.
  - OpenTelemetry runtime instrumentation.
  - Azure Monitor integration.
  - Dependencies: Azure.Core, Azure.Identity, Azure.Extensions.AspNetCore.Configuration.Secrets.
- **Abstractions**: `Diginsight.Components.Configuration.Abstractions` provides configuration provider interfaces, Azure Key Vault integration contracts, and foundation interfaces for observable configuration patterns.

## 🎯 **Diginsight.Components** (Core)

- **Purpose**: Main component library with common functionality.
- **Key features**:
  - HTTP client configuration (`HttpClientOptions`).
  - Authentication and security features.
  - JWT token handling.
  - Microsoft Identity Client integration.
  - Cryptography support.
- **Abstractions**: `Diginsight.Components.Abstractions` provides core interfaces like `IDebugService` for conditional debug operations and base contracts for other components.

## 🌐 **Diginsight.Components.Azure**

- **Purpose**: Azure-specific functionality and integrations.
- **Key features**:
  - CosmosDB Observable extensions: CosmosDB Container query extensions integrated with Diginsight observability and exposing advanced metrics (e.g., query_cost).
  - Azure Table Observable extensions: Azure Table query extensions integrated with Diginsight observability.
  - Other Azure service observable extensions.
- **Abstractions**: `Diginsight.Components.Azure.Abstractions` provides interfaces and base contracts for Azure service integrations.

## 🎨 **Diginsight.Components.Presentation**

- **Purpose**: UI/Presentation layer components.
- **Status**: Currently appears to be a placeholder (contains only `Class1`).
- **Abstractions**: `Diginsight.Components.Presentation.Abstractions` provides interfaces and contracts for presentation layer functionality.


# 🏛️ Architecture Characteristics

Each component follows these patterns:

1. **Observability Integration**: Every component includes an `Observability` class that:
   - Creates an `ActivitySource` for distributed tracing.
   - Registers with the `ObservabilityRegistry`.
   - Provides consistent logging infrastructure.

2. **Modular Design**: Components can be used independently or together, with clear separation of concerns.

3. **Azure Integration**: Strong focus on Azure cloud services with built-in monitoring and configuration support.

4. **OpenTelemetry Standards**: Uses industry-standard observability patterns for metrics, traces, and logs.

## 📚 References

### 📘 Diginsight Documentation

- [Diginsight Telemetry](https://diginsight.github.io/telemetry/) - Official documentation for Diginsight observability platform
- [Diginsight Smartcache](https://diginsight.github.io/smartcache/) - Documentation for hybrid caching strategies
- [Diginsight GitHub Organization](https://github.com/diginsight) - Source code and additional resources

### 🔭 OpenTelemetry

- [OpenTelemetry Official Documentation](https://opentelemetry.io/docs/) - Comprehensive guide to OpenTelemetry standards
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/) - .NET-specific OpenTelemetry implementation
- [OpenTelemetry Instrumentation](https://opentelemetry.io/docs/concepts/instrumentation/) - Instrumentation concepts and best practices

### ☁️ Azure Services

- [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/) - Azure's observability and monitoring platform
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/) - Secure secrets management service
- [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/) - Globally distributed database service
- [Azure Table Storage](https://docs.microsoft.com/en-us/azure/storage/tables/) - NoSQL structured data storage

### ⚡ .NET Technologies

- [.NET Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) - Configuration providers and patterns
- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/) - Authentication and authorization platform
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Cross-platform web framework

### 📊 Observability & Monitoring

- [Distributed Tracing](https://opentelemetry.io/docs/concepts/observability-primer/#distributed-traces) - Understanding distributed tracing concepts
- [Metrics Collection](https://opentelemetry.io/docs/concepts/observability-primer/#metrics) - Metrics and measurement best practices
- [Logging Best Practices](https://opentelemetry.io/docs/concepts/observability-primer/#logs) - Structured logging approaches
