---
title: "Getting Started"
description: "Quick start guide for Diginsight Components - Installation, basic usage, and configuration examples"
author: "Diginsight Team"
date: "2024-01-20"
categories: [getting-started, installation, configuration]
format:
  html:
    toc: true
    toc-depth: 3
    number-sections: false
    code-copy: true
    code-fold: false
  pdf:
    toc: true
    number-sections: false
execute:
  echo: true
  eval: false
---

## Table of Contents
- [📦 Installation](#-installation)
  - [Minimal Setup (Basic observability)](#minimal-setup-basic-observability)
  - [Azure Applications](#azure-applications)
  - [Full Enterprise Setup](#full-enterprise-setup)
- [💡 Basic Usage](#-basic-usage)
  - [Configure Your Application](#configure-your-application)
  - [Add Observability to Your Services](#add-observability-to-your-services)
  - [Use Azure Extensions](#use-azure-extensions-if-using-azure-components)
- [⚙️ Configuration](#️-configuration)

## 📦 Installation

Choose the components that fit your needs:

### **Minimal Setup** (Basic observability)
```bash
dotnet add package Diginsight.Components.Configuration
```

### **Azure Applications**
```bash
dotnet add package Diginsight.Components.Configuration
dotnet add package Diginsight.Components.Azure
```

### **Full Enterprise Setup**
```bash
dotnet add package Diginsight.Components.Configuration
dotnet add package Diginsight.Components.Azure
dotnet add package Diginsight.Components
```

## 💡 Basic Usage

### **Configure Your Application**

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

### **Add Observability to Your Services**

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

### **Use Azure Extensions** (if using Azure components)

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

## ⚙️ Configuration

### **appsettings.json Example**
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
