# Diginsight Configuration Extensions

## 📑 Table of Contents

- [📋 Overview](#-overview)
- [✨ Key Features](#-key-features)
- [📁 Enhanced Configuration File Hierarchy](#-enhanced-configuration-file-hierarchy)
  - [Standard .NET Core Configuration Files](#standard-net-core-configuration-files)
  - [Extended Diginsight Configuration Files](#extended-diginsight-configuration-files)
  - [Configuration Loading Order (Precedence)](#configuration-loading-order-precedence)
- [🔧 Local Development Support](#-local-development-support)
- [🔐 Azure Key Vault Integration](#-azure-key-vault-integration)
  - [AzureKeyVault Configuration Section](#azurekeyvault-configuration-section)
  - [Authentication Methods](#authentication-methods)
- [🏗️ Core Components](#️-core-components)
  - [1. HostBuilderExtensions](#1-hostbuilderextensions)
  - [2. ConfigureAppConfiguration2 Method](#2-configureappconfiguration2-method)
- [⚙️ Configuration Loading Process](#️-configuration-loading-process)
  - [Phase 1: Environment Detection](#phase-1-environment-detection)
  - [Phase 2: Base Configuration Files](#phase-2-base-configuration-files)
  - [Phase 3: External Configuration Resolution](#phase-3-external-configuration-resolution)
  - [Phase 4: Environment Variable Overrides](#phase-4-environment-variable-overrides)
  - [Phase 5: Azure Key Vault Integration](#phase-5-azure-key-vault-integration)
  - [Phase 6: Configuration Source Ordering](#phase-6-configuration-source-ordering)
- [🛠️ Helper Components](#️-helper-components)
  - [DirectoryHelper](#directoryhelper)
  - [ApplicationCredentialProvider](#applicationcredentialprovider)
  - [KeyVaultSecretManager2](#keyvaultsecretmanager2)
- [🌍 Environment Variables](#-environment-variables)
  - [Core Variables](#core-variables)
  - [Azure Key Vault Variables](#azure-key-vault-variables)
- [💻 Usage Examples](#-usage-examples)
  - [Basic Usage](#basic-usage)
  - [With Tag Filtering](#with-tag-filtering)
  - [For Web Applications](#for-web-applications)
- [📄 Configuration File Examples](#-configuration-file-examples)
  - [appsettings.json](#appsettingsjson)
  - [appsettings.Development.json](#appsettingsdevelopmentjson)
  - [appsettings.local.json (Development Only)](#appsettingslocaljson-development-only)
  - [appsettings.Development.local.json (Development Only)](#appsettingsdevelopmentlocaljson-development-only)
- [✅ Best Practices](#-best-practices)
- [🔒 Security Considerations](#-security-considerations)
- [🐛 Troubleshooting](#-troubleshooting)
  - [Common Issues](#common-issues)
  - [Debug Logging](#debug-logging)
- [🚀 Migration from Standard .NET Core Configuration](#-migration-from-standard-net-core-configuration)

---

## 📋 Overview

The `HostBuilderExtensions` class extends the standard .NET Core configuration building logic with an **enhanced appsettings.json hierarchy** and **configuration capabilities**. <br>
This includes advanced **configuration loading with external folder support** with intelligent file resolution and **automatic Azure Key Vault integration** for .NET applications.

## ✨ Key Features

- **Extended Configuration Hierarchy**: Extends standard .NET Core configuration with additional file layers
- **Multi-Environment Configuration**: Supports different environments (Development, Production, etc.)
- **Local Development Support**: Special `.local.json` files for local debugging scenarios
- **External Configuration Folder**: Allows configurations to be stored outside the application directory
- **Azure Key Vault Integration**: Automatically loads secrets from Azure Key Vault using standard AKV section
- **Intelligent File Resolution**: Hierarchical search for configuration files
- **Tag-Based Filtering**: Supports filtering Key Vault secrets by tags

## 📁 Enhanced Configuration File Hierarchy

The `HostBuilderExtensions` class extends the standard .NET Core configuration hierarchy with additional layers:

**Standard .NET Core Configuration Files**:

standard .NET Core configuration files are normally organized according to the following structure:

└─── **appsettings.json** - Base configuration file<br>
└─── **appsettings.\{Environment\}.json** - Environment-specific configuration<br>
└─── **user secrets** (highest priority)<br>
└─── **Environment variables** (highest priority)<br>

diginsight configuration extensions builds application configuration from the following structure:

**Extended.NET Core Configuration Files**:

└─── `appsettings.json` (lowest priority)<br>
└─── `appsettings.{Environment}.json`<br>
└─── <mark>**`appsettings.local.json`**</mark> (Development only)<br>
└─── <mark>**`appsettings.{Environment}.local.json`**</mark> (Development only)<br>
└─── <mark>**Azure Key Vault secrets**</mark> (if configured)<br>
└─── **user secrets** (highest priority)<br>
└─── **Environment variables** (highest priority)<br>

**Local Development Support**

The <mark>`.local.json`</mark> files are specifically designed for local debugging scenarios:

- **Purpose**: Store local development overrides without affecting source control
- **Environment**: Only loaded in Development environment
- **Security**: Should be added to `.gitignore` to prevent accidental commits
- **Usage**: Override connection strings, API endpoints, and other environment-specific values

**Azure Key Vault Integration**

The system provides seamless Azure Key Vault integration through a standard configuration section:

```json
{
  "AzureKeyVault": {
    "TenantId": "c8f97966-df69-480f-a690-072314b06f83", // Environment specific
    "ClientId": "9c90b0e2-405f-44c1-9610-e7803621e68a",
    // "ManagedIdentityClientId": "9c90b0e2-405f-44c1-9610-e7803621e68a",
    "Uri": "https://dev-dgw-003-kv.vault.azure.net/", // Environment specific
    "ClientSecret": "" // Key Vault secret or empty for managed identity
  }
}
```

Authentication is handled by the `ApplicationCredentialProvider` class, which supports multiple authentication methods based on the environment (Development or Production).

The `ApplicationCredentialProvider` supports multiple authentication methods:

**Development Environment:**
- `AzureCliCredential`: Uses Azure CLI authentication
- `VisualStudioCodeCredential`: Uses VS Code authentication
- `VisualStudioCredential`: Uses Visual Studio authentication
- `ClientSecretCredential`: Uses client secret if provided

**Production Environment:**
- `ManagedIdentityCredential`: Uses managed identity (recommended)
- `ClientSecretCredential`: Uses client secret if provided

## 🏗️ Core Components

### 1. HostBuilderExtensions

The main entry point for configuration setup. Provides extension methods for `IHostBuilder` and `IWebHostBuilder`.
public static IHostBuilder ConfigureAppConfiguration2(
    this IHostBuilder hostBuilder, 
    ILoggerFactory loggerFactory, 
    Func<IDictionary<string, string>, bool>? tagsMatch = null)
### 2. ConfigureAppConfiguration2 Method

The core configuration method that orchestrates the entire configuration loading process.

## ⚙️ Configuration Loading Process

### Phase 1: Environment Detection
1. Determines if running in Development environment (`isLocal`)
2. Checks if debugger is attached (`isDebuggerAttached`)
3. Gets environment name from `IHostEnvironment.EnvironmentName`

### Phase 2: Base Configuration Files
1. **appsettings.json**: Base configuration file
2. **appsettings.local.json**: Local override (Development only)
3. **appsettings.{Environment}.json**: Environment-specific configuration

### Phase 3: External Configuration Resolution

The system supports loading configurations from an external folder specified by the `ExternalConfigurationFolder` environment variable.

#### External Configuration Search Algorithm

When `ExternalConfigurationFolder` is set and exists, the system performs a hierarchical search:

1. **Repository Root Detection**: Uses `DirectoryHelper.GetRepositoryRoot()` to find the git repository root
2. **Path Decomposition**: Breaks down the current directory path relative to the repository root
3. **Hierarchical Search**: Searches for configuration files in the following order:
Example: If current directory is:
E:\dev.darioa.live\Diginsight\samples\src\03.00 AzureStorage\01 TableStorage\bin\Debug

And external folder is:
E:\dev.darioa.live\Diginsight\samples.internal

The search order is:
1. E:\dev.darioa.live\Diginsight\samples.internal\src\03.00 AzureStorage\01 TableStorage\bin\Debug
2. E:\dev.darioa.live\Diginsight\samples.internal\src\03.00 AzureStorage\01 TableStorage\bin
3. E:\dev.darioa.live\Diginsight\samples.internal\src\03.00 AzureStorage\01 TableStorage
4. E:\dev.darioa.live\Diginsight\samples.internal\src\03.00 AzureStorage
5. E:\dev.darioa.live\Diginsight\samples.internal\src
6. E:\dev.darioa.live\Diginsight\samples.internal
### Phase 4: Environment Variable Overrides

The system supports custom environment names through the `AppsettingsEnvironmentName` environment variable:
string? appsettingsEnvironmentName = Environment.GetEnvironmentVariable("AppsettingsEnvironmentName") ?? environmentName;
This allows using different configuration files than the default environment name.

### Phase 5: Azure Key Vault Integration

If Azure Key Vault configuration is present, the system automatically integrates it using the standard AKV section format.

#### Authority Host Support:
The system automatically detects China regions and sets the appropriate authority host:if (appsettingsEnvName.EndsWith("cn", StringComparison.OrdinalIgnoreCase))
{
    credentialOptions.AuthorityHost = AzureAuthorityHosts.AzureChina;
}
### Phase 6: Configuration Source Ordering

The system ensures proper precedence by reordering configuration sources:
1. JSON files (in order of importance)
2. Azure Key Vault secrets
3. Environment variables (highest priority)

## 🛠️ Helper Components

### DirectoryHelper
Provides utility methods for finding repository roots:public static string? GetRepositoryRoot(string currentDirectory)
### ApplicationCredentialProvider
Manages Azure authentication credentials with fallback chains for different environments.

### KeyVaultSecretManager2
Custom Key Vault secret manager that:
- Converts secret names to configuration keys
- Supports tag-based filtering
- Handles secret expiration and activation dates
- Converts Key Vault naming conventions to .NET configuration format

#### Key Name Conversion:
- `--` ? `:`
- `-x{hex}` ? Unicode character
- `-u{hex}` ? Unicode character

Example: `MyApp--Database--ConnectionString` becomes `MyApp:Database:ConnectionString`

## 🌍 Environment Variables

### Core Variables:
- `DOTNET_ENVIRONMENT`: Sets the application environment
- `ExternalConfigurationFolder`: Path to external configuration folder
- `AppsettingsEnvironmentName`: Override for environment-specific configuration file names

### Azure Key Vault Variables:
- `AzureKeyVault:Uri`: Key Vault URI
- `AzureKeyVault:ClientId`: Azure AD Client ID
- `AzureKeyVault:TenantId`: Azure AD Tenant ID
- `AzureKeyVault:ClientSecret`: Azure AD Client Secret

## 💻 Usage Examples

### Basic Usage:Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration2(loggerFactory)
    .Build();
### With Tag Filtering:Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration2(loggerFactory, tags => tags.ContainsKey("AppSettings"))
    .Build();
### For Web Applications:var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration2(loggerFactory);
## 📄 Configuration File Examples

### appsettings.json{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AzureKeyVault": {
    "Uri": "https://myvault.vault.azure.net/"
  }
}
### appsettings.Development.json{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "AzureKeyVault": {
    "TenantId": "c8f97966-df69-480f-a690-072314b06f83",
    "ClientId": "9c90b0e2-405f-44c1-9610-e7803621e68a",
    "Uri": "https://dev-dgw-003-kv.vault.azure.net/",
    "ClientSecret": "" // Retrieved from Key Vault or managed identity
  }
}
### appsettings.local.json (Development Only){
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=true;"
  },
  "AzureKeyVault": {
    "ClientSecret": "your-local-client-secret"
  }
}
### appsettings.Development.local.json (Development Only){
  "Logging": {
    "LogLevel": {
      "Diginsight.Components.Configuration": "Debug"
    }
  },
  "LocalDebugSettings": {
    "EnableDetailedLogging": true,
    "MockExternalServices": true
  }
}
## ✅ Best Practices

1. **Environment Variables**: Use environment variables for sensitive configuration in production
2. **External Folders**: Keep sensitive configurations in external folders for security
3. **Tag Filtering**: Use Key Vault tags to organize and filter secrets
4. **Local Development**: Use `.local.json` files for local development overrides
5. **Logging**: Enable detailed logging during development for configuration troubleshooting
6. **Source Control**: Add `*.local.json` to `.gitignore` to prevent accidental commits
7. **Key Vault**: Use managed identities in production environments

## 🔒 Security Considerations

1. **Secret Management**: Never commit secrets to source control
2. **External Folders**: Ensure external configuration folders have proper access controls
3. **Key Vault**: Use managed identities in production environments
4. **Local Files**: Add `*.local.json` to `.gitignore`
5. **Environment Variables**: Secure environment variable access in production

## 🐛 Troubleshooting

### Common Issues:
1. **Configuration Not Found**: Check external folder path and permissions
2. **Key Vault Access**: Verify authentication credentials and permissions
3. **File Resolution**: Enable debug logging to trace file search paths
4. **Environment Variables**: Ensure all required environment variables are set
5. **Local Files**: Verify `.local.json` files are not committed to source control

### Debug Logging:
The system provides extensive debug logging. Enable it in your configuration:{
  "Logging": {
    "LogLevel": {
      "Diginsight.Components.Configuration": "Debug"
    }
  }
}
## 🚀 Migration from Standard .NET Core Configuration

To migrate from standard .NET Core configuration to Diginsight configuration:

1. **Replace ConfigureAppConfiguration** with `ConfigureAppConfiguration2`
2. **Add local configuration files** for development overrides
3. **Configure Azure Key Vault section** if using AKV
4. **Set up external configuration folder** if needed
5. **Update .gitignore** to exclude `*.local.json` files

This enhanced configuration system provides flexibility, security, and maintainability for complex application deployment scenarios while maintaining compatibility with standard .NET Core configuration patterns.
