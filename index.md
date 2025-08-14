![main build](https://github.com/diginsight/components/actions/workflows/v2_99.Package.CICD.yml/badge.svg?branch=main)<br>
![main build](https://github.com/diginsight/components/actions/workflows/quarto-publish.yml/badge.svg?branch=main)

# INTRODUCTUCTION

`Diginsight Components` include **Observable** or **Optimized extensions** for other technologies obtained with integration of **Diginsight** observability platform and **Diginsight Smartcache**. 

Each component is designed to address specific functionality areas (e.g. Authentication, database access...) while maintaining consistent observability and diagnostics capabilities.

## Why Diginsight Components

**Diginsight Telemetry** implements observability and diagnostics capabilities for .NET applications using **OpenTelemetry** and **Azure Monitor**. 

**Diginsight Smartcache** implements optimized hybrid caching for .NET applications, combining in-memory and distributed caching strategies.

Available Diginsight Components include:

<img src="images/001.01 diginsight repositories.png" alt="Diginsight repositories overview" style="max-width: 400px; width: 100%;" />

### 1. **Diginsight.Components.Configuration**
- **Purpose**: Observable extensions for diginsight configuration with Azure Key Vault, Console, Log4Net, and OpenTelemetry etc, 
- **Key features**:
  - Azure Key Vault integration
  - Activity source detection and registration
  - OpenTelemetry runtime instrumentation
  - Azure Monitor integration
  - Dependencies: Azure.Core, Azure.Identity, Azure.Extensions.AspNetCore.Configuration.Secrets

### 2. **Diginsight.Components.Azure**
- **Purpose**: Azure-specific functionality and integrations
- **Current file context**: Contains metrics for Azure services like CosmosDB
- **Key features**:
  - CosmosDB Observable extensions: CosmosDB Container query extensions integrated with diginsight observability and exposing advanced metrics (e.g. query_cost))
  - Azure Table Observable extensions: Azure Table query extensions integrated with diginsight observability 
  - Other Azure service observable extensions

### 3. **Diginsight.Components.Abstractions**
- **Purpose**: Core abstractions and interfaces for the Diginsight.Components 
- **Key features**:
  - `IDebugService` interface for conditional debug operations
  - Base contracts for other components

### 4. **Diginsight.Components** (Core)
- **Purpose**: Main component library with common functionality
- **Key features**:
  - HTTP client configuration (`HttpClientOptions`)
  - Authentication and security features
  - JWT token handling
  - Microsoft Identity Client integration
  - Cryptography support

### 5. **Diginsight.Components.Azure.Abstractions**
- **Purpose**: Abstractions and interfaces for the Diginsight.Components.Azure
- **Key features**:
  - `IDebugService` interface for conditional debug operations
  - Base contracts for other components

### 6. **Diginsight.Components.Presentation.Abstractions**
- **Purpose**: Abstractions and interfaces for the Diginsight.Components.Presentation
- **Key features**:
  - `IDebugService` interface for conditional debug operations
  - Base contracts for other components

### 7. **Diginsight.Components.Azure**
- **Purpose**: Azure technology observable extensions components
- **Status**: Currently appears to be a placeholder (contains only `Class1`)

### 8. **Diginsight.Components.Presentation**
- **Purpose**: UI/Presentation layer components
- **Status**: Currently appears to be a placeholder (contains only `Class1`)


## Architecture Characteristics

Each component follows these patterns:

1. **Observability Integration**: Every component includes an `Observability` class that:
   - Creates an `ActivitySource` for distributed tracing
   - Registers with the `ObservabilityRegistry`
   - Provides consistent logging infrastructure

2. **Modular Design**: Components can be used independently or together, with clear separation of concerns

3. **Azure Integration**: Strong focus on Azure cloud services with built-in monitoring and configuration support

4. **OpenTelemetry Standards**: Uses industry-standard observability patterns for metrics, traces, and logs

The component you're currently viewing (`Diginsight.Components.Azure.Metrics.QueryMetrics`) is part of the Azure component and specifically provides CosmosDB query cost tracking using OpenTelemetry histograms.






