using Azure.Core;
using Azure.Identity;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography.X509Certificates;

namespace Diginsight.Components.Configuration;

/// <summary>
/// Default implementation of ICredentialProvider that creates a chained token credential
/// with multiple authentication methods based on environment and configuration.
/// 
/// This provider implements a fallback chain approach:
/// - For Development: Client credentials (secret/certificate) → Azure CLI → VS Code → Visual Studio
/// - For Production: Client credentials → Workload Identity → Client Assertion → Managed Identity
/// </summary>
public sealed class DefaultCredentialProvider : ICredentialProvider
{
    private Type T = typeof(DefaultCredentialProvider);
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the DefaultCredentialProvider.
    /// </summary>
    /// <param name="environment">The host environment to determine development vs production behavior</param>
    public DefaultCredentialProvider(IHostEnvironment environment)
    {
        this.environment = environment;
    }

    /// <summary>
    /// Creates a chained token credential based on the provided configuration and environment.
    /// 
    /// The method builds a credential chain with different priorities:
    /// - Development: Prioritizes developer tools (CLI, VS, VS Code) for local development
    /// - Production: Prioritizes Azure-native authentication (Workload Identity, Managed Identity)
    /// </summary>
    /// <param name="configuration">Configuration containing authentication parameters</param>
    /// <returns>A ChainedTokenCredential that tries multiple authentication methods in order</returns>
    public TokenCredential Get(IConfiguration configuration)
    {
        var logger = Observability.LoggerFactory?.CreateLogger(T) ?? NullLogger.Instance;
        using var activity = Observability.ActivitySource?.StartMethodActivity(logger, () => new { configuration });

        ICollection<TokenCredential> credentials = new List<TokenCredential>();

        // Extract authentication configuration values
        var clientId = configuration["ClientId"].HardTrim();
        var managedIdentityClientId = configuration["ManagedIdentityClientId"].HardTrim();
        var tenantId = configuration["TenantId"].HardTrim();
        var clientSecret = configuration["ClientSecret"].HardTrim();
        var certificateThumbprint = configuration["CertificateThumbprint"].HardTrim();
        
        logger.LogDebug($"tenantId:{tenantId},clientId:{clientId},clientSecret:{clientSecret},managedIdentityClientId:{managedIdentityClientId},certificateThumbprint:{certificateThumbprint}");
        
        var authorityHost = GetAuthorityHost();

        // Add client credentials (highest priority for configured authentication)
        if (tenantId is not null && clientId is not null)
        {
            // Client Secret Credential - uses application secret for authentication
            if (clientSecret is not null)
            {
                ClientSecretCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
                credentials.Add(new ClientSecretCredential(tenantId, clientId, clientSecret, credentialOptions1));
            }

            // Client Certificate Credential - uses X.509 certificate for authentication
            if (certificateThumbprint is not null)
            {
                ClientCertificateCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
                X509Certificate2 certificate = GetStoredCertificate(certificateThumbprint);
                credentials.Add(new ClientCertificateCredential(tenantId, clientId, certificate, credentialOptions1));
            }
        }

        // Environment-specific credential chain
        if (environment.IsDevelopment())
        {
            // Development environment: Add developer tools for local authentication
            
            // Azure CLI Credential - uses Azure CLI login for developers
            AzureCliCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
            credentials.Add(new AzureCliCredential(credentialOptions1));

            // Visual Studio Code Credential - uses VS Code Azure extension authentication
            VisualStudioCodeCredentialOptions credentialOptions2 = new() { AuthorityHost = authorityHost };
            credentials.Add(new VisualStudioCodeCredential(credentialOptions2));

            // Visual Studio Credential - uses Visual Studio Azure Service Authentication
            VisualStudioCredentialOptions credentialOptions3 = new() { AuthorityHost = authorityHost };
            credentials.Add(new VisualStudioCredential(credentialOptions3));
        }
        else
        {
            // Production environment: Add Azure-native authentication methods
            
            // Workload Identity Credential - for Kubernetes workload identity
            WorkloadIdentityCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
            credentials.Add(new WorkloadIdentityCredential(credentialOptions1));

            // Managed Identity setup for Client Assertion and direct usage
            TokenCredentialOptions credentialOptions2 = new() { AuthorityHost = authorityHost };
            ManagedIdentityCredential managedIdentityCredential = new(managedIdentityClientId, credentialOptions2);

            // Client Assertion Credential - uses managed identity to get assertion token
            if (tenantId is not null && clientId is not null)
            {
                // Function to get assertion token from managed identity for token exchange
                async Task<string> GetAssertionAsync(CancellationToken ct)
                {
                    return (await managedIdentityCredential.GetTokenAsync(new TokenRequestContext(["api://AzureADTokenExchange/.default"]), ct)).Token;
                }

                ClientAssertionCredentialOptions credentialOptions3 = new() { AuthorityHost = authorityHost };
                credentials.Add(new ClientAssertionCredential(tenantId, clientId, GetAssertionAsync, credentialOptions3));
            }

            // Managed Identity Credential - direct managed identity authentication (fallback)
            credentials.Add(managedIdentityCredential);
        }

        // Return chained credential that tries each method in order until one succeeds
        return new ChainedTokenCredential(credentials.ToArray());
    }

    /// <summary>
    /// Retrieves an X.509 certificate from the current user's certificate store by thumbprint.
    /// </summary>
    /// <param name="thumbprint">The thumbprint of the certificate to retrieve</param>
    /// <returns>The X.509 certificate matching the thumbprint</returns>
    /// <exception cref="InvalidOperationException">Thrown when no certificate is found with the specified thumbprint</exception>
    public static X509Certificate2 GetStoredCertificate(string thumbprint)
    {
        using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.OpenExistingOnly);
        return store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false).First();
    }

    /// <summary>
    /// Determines the appropriate Azure authority host based on the environment configuration.
    /// 
    /// Checks the appsettings environment name to determine if this is a China region deployment,
    /// which requires using the Azure China authority host instead of the public cloud.
    /// </summary>
    /// <returns>The appropriate Azure authority host URI</returns>
    Uri GetAuthorityHost()
    {
        bool? isChina = null;
        if (isChina is not { } isChina0)
        {
            // Check if environment name indicates China region (ends with "cn")
            string? appsettingsEnvName = EnvironmentVariables.AppsettingsEnvironmentName;
            isChina = isChina0 = appsettingsEnvName?.EndsWith("cn", StringComparison.OrdinalIgnoreCase) == true;
        }

        // Default to Azure Public Cloud, switch to China if detected
        var authorityHost = AzureAuthorityHosts.AzurePublicCloud;
        if (isChina0)
        {
            authorityHost = AzureAuthorityHosts.AzureChina;
        }
        return authorityHost;
    }
}



