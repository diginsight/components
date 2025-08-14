![main build](https://github.com/diginsight/components/actions/workflows/v2_99.Package.CICD.yml/badge.svg?branch=main)<br>
![main build](https://github.com/diginsight/components/actions/workflows/quarto-publish.yml/badge.svg?branch=main)

# Introduction

`Diginsight Components` include **observable** or **optimized extensions** for other technologies, achieved through integration with the **Diginsight** observability platform and **Diginsight Smartcache**.

Each component is designed to address specific functionality areas (e.g., authentication, database access) while maintaining consistent observability and diagnostics capabilities.

# Table of Contents

- [Why Diginsight Components](#why-diginsight-components)
- [What components are available](#what-components-are-available)
  - [Diginsight.Components.Configuration](#diginsightcomponentsconfiguration)
  - [Diginsight.Components.Abstractions](#diginsightcomponentsabstractions)
  - [Diginsight.Components](#diginsightcomponents-core)
  - [Diginsight.Components.Azure.Abstractions](#diginsightcomponentsazureabstractions)
  - [Diginsight.Components.Azure](#diginsightcomponentsazure)
  - [Diginsight.Components.Presentation.Abstractions](#diginsightcomponentspresentationabstractions)
  - [Diginsight.Components.Presentation](#diginsightcomponentspresentation)
- [Architecture Characteristics](#architecture-characteristics)
- [References](#references)
  - [Diginsight Documentation](#diginsight-documentation)
  - [OpenTelemetry](#opentelemetry)
  - [Azure Services](#azure-services)
  - [.NET Technologies](#net-technologies)
  - [Observability & Monitoring](#observability--monitoring)

# Why Diginsight Components

**[Diginsight Telemetry](https://diginsight.github.io/telemetry/)** implements observability and diagnostics capabilities for .NET applications using **OpenTelemetry** and **Azure Monitor**. 

**[Diginsight Smartcache](https://diginsight.github.io/smartcache/)** implements optimized hybrid caching for .NET applications, combining in-memory and distributed caching strategies.

![Diginsight repositories overview](./images/diginsight-repositories-overview.png)

**Diginsight Components** are built to take advantage of these capabilities, enhancing existing libraries with observability capabilities and performance optimization, where possible.

**Diginsight Components** are built with a layered architecture that promotes modularity and flexibility. 

At the foundation, **abstractions packages** define core interfaces and contracts. 

The **core components** provide essential functionality like authentication and HTTP client configuration. 

**Technology-specific components** (like Azure integrations) extend the core capabilities for particular platforms. Finally, **presentation components** handle UI-related concerns.

This modular approach allows you to compose exactly the functionality your application needs—from a minimal observability setup using just the core components, to a full-featured enterprise solution with Azure integrations, advanced caching, and presentation layers.

# What components are available

## **Diginsight.Components.Configuration**
- **Purpose**: Observable extensions for Diginsight configuration with Azure Key Vault, Console, Log4Net, and OpenTelemetry.
- **Key features**:
  - Azure Key Vault integration.
  - Activity source detection and registration.
  - OpenTelemetry runtime instrumentation.
  - Azure Monitor integration.
  - Dependencies: Azure.Core, Azure.Identity, Azure.Extensions.AspNetCore.Configuration.Secrets.

## **Diginsight.Components.Abstractions**

- **Purpose**: Core abstractions and interfaces for Diginsight.Components assembly.
- **Key features**:
  - `IDebugService` interface for conditional debug operations.
  - Base contracts for other components.

## **Diginsight.Components** (Core)

- **Purpose**: Main component library with common functionality.
- **Key features**:
  - HTTP client configuration (`HttpClientOptions`).
  - Authentication and security features.
  - JWT token handling.
  - Microsoft Identity Client integration.
  - Cryptography support.

## **Diginsight.Components.Azure.Abstractions**

- **Purpose**: Abstractions and interfaces for Diginsight.Components.Azure assembly.
- **Key features**:
  - `IDebugService` interface for conditional debug operations.
  - Base contracts for other components.

## **Diginsight.Components.Azure**
- **Purpose**: Azure-specific functionality and integrations.
- **Key features**:
  - CosmosDB Observable extensions: CosmosDB Container query extensions integrated with Diginsight observability and exposing advanced metrics (e.g., query_cost).
  - Azure Table Observable extensions: Azure Table query extensions integrated with Diginsight observability.
  - Other Azure service observable extensions.

## **Diginsight.Components.Presentation.Abstractions**

- **Purpose**: Abstractions and interfaces for Diginsight.Components.Presentation assembly.
- **Key features**:
  - `IDebugService` interface for conditional debug operations.
  - Base contracts for other components.

## **Diginsight.Components.Presentation**

- **Purpose**: UI/Presentation layer components.
- **Status**: Currently appears to be a placeholder (contains only `Class1`).


# Architecture Characteristics

Each component follows these patterns:

1. **Observability Integration**: Every component includes an `Observability` class that:
   - Creates an `ActivitySource` for distributed tracing.
   - Registers with the `ObservabilityRegistry`.
   - Provides consistent logging infrastructure.

2. **Modular Design**: Components can be used independently or together, with clear separation of concerns.

3. **Azure Integration**: Strong focus on Azure cloud services with built-in monitoring and configuration support.

4. **OpenTelemetry Standards**: Uses industry-standard observability patterns for metrics, traces, and logs.

## References

### Diginsight Documentation

- [Diginsight Telemetry](https://diginsight.github.io/telemetry/) - Official documentation for Diginsight observability platform
- [Diginsight Smartcache](https://diginsight.github.io/smartcache/) - Documentation for hybrid caching strategies
- [Diginsight GitHub Organization](https://github.com/diginsight) - Source code and additional resources

### OpenTelemetry

- [OpenTelemetry Official Documentation](https://opentelemetry.io/docs/) - Comprehensive guide to OpenTelemetry standards
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/) - .NET-specific OpenTelemetry implementation
- [OpenTelemetry Instrumentation](https://opentelemetry.io/docs/concepts/instrumentation/) - Instrumentation concepts and best practices

### Azure Services

- [Azure Monitor](https://docs.microsoft.com/en-us/azure/azure-monitor/) - Azure's observability and monitoring platform
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/) - Secure secrets management service
- [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/) - Globally distributed database service
- [Azure Table Storage](https://docs.microsoft.com/en-us/azure/storage/tables/) - NoSQL structured data storage

### .NET Technologies

- [.NET Configuration](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration) - Configuration providers and patterns
- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/) - Authentication and authorization platform
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Cross-platform web framework

### Observability & Monitoring

- [Distributed Tracing](https://opentelemetry.io/docs/concepts/observability-primer/#distributed-traces) - Understanding distributed tracing concepts
- [Metrics Collection](https://opentelemetry.io/docs/concepts/observability-primer/#metrics) - Metrics and measurement best practices
- [Logging Best Practices](https://opentelemetry.io/docs/concepts/observability-primer/#logs) - Structured logging approaches