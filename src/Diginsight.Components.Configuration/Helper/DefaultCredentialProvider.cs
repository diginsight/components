using Azure.Core;
using Azure.Identity;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    private static readonly Type TClass = typeof(DefaultCredentialProvider);

    private readonly IHostEnvironment environment;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the DefaultCredentialProvider.
    /// </summary>
    /// <param name="environment">The host environment to determine development vs production behavior</param>
    /// <param name="logger">Logger instance for diagnostic output</param>
    public DefaultCredentialProvider(IHostEnvironment environment, ILogger<DefaultCredentialProvider> logger)
        : this(environment, (ILogger)logger) { }

    /// <summary>
    /// Initializes a new instance of the DefaultCredentialProvider with an untyped logger.
    /// Used in early host-building scenarios where DI is not yet available.
    /// </summary>
    public DefaultCredentialProvider(IHostEnvironment environment, ILogger logger)
    {
        this.environment = environment;
        this.logger = logger;
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
        using var activity = Observability.ActivitySource.StartMethodActivity(TClass, logger, () => new { configuration });

        ICollection<TokenCredential> credentials = new List<TokenCredential>();

        // Extract authentication configuration values
        var tenantId = configuration["TenantId"].HardTrim();
        var clientId = configuration["ClientId"].HardTrim();
        var managedIdentityClientId = configuration["ManagedIdentityClientId"].HardTrim();
        var clientSecret = configuration["ClientSecret"].HardTrim();
        var certificateThumbprint = configuration["CertificateThumbprint"].HardTrim();

        logger.LogDebug("tenantId:{TenantId},clientId:{ClientId},clientSecret:{ClientSecret},managedIdentityClientId:{ManagedIdentityClientId},certificateThumbprint:{CertificateThumbprint}", tenantId, clientId, Redact(clientSecret), managedIdentityClientId, certificateThumbprint);

        // A missing section binds to null, and every Apply below tolerates null, so an absent
        // configuration leaves the Azure SDK defaults in place.
        CredentialChainSettings chain = configuration.GetSection(CredentialChainSettings.SectionName).Get<CredentialChainSettings>() ?? new CredentialChainSettings();

        CredentialSettings? legacyAzureCliSettings = ReadLegacyAzureCliSettings(configuration, tenantId);

        var authorityHost = GetAuthorityHost();

        // Add client credentials (highest priority for configured authentication)
        if (tenantId is not null && clientId is not null)
        {
            // Client Secret Credential - uses application secret for authentication
            if (clientSecret is not null)
            {
                ClientSecretCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
                chain.Common.Apply(credentialOptions1);
                chain.ClientSecret.Apply(credentialOptions1);
                credentials.Add(DecorateWithGetTokenRetry(new ClientSecretCredential(tenantId, clientId, clientSecret, credentialOptions1), chain.Common, chain.ClientSecret));
            }

            // Client Certificate Credential - uses X.509 certificate for authentication
            if (certificateThumbprint is not null)
            {
                ClientCertificateCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
                chain.Common.Apply(credentialOptions1);
                chain.ClientCertificate.Apply(credentialOptions1);
                X509Certificate2 certificate = GetStoredCertificate(certificateThumbprint);
                credentials.Add(DecorateWithGetTokenRetry(new ClientCertificateCredential(tenantId, clientId, certificate, credentialOptions1), chain.Common, chain.ClientCertificate));
            }
        }

        // Environment-specific credential chain
        if (environment.IsDevelopment())
        {
            // Azure CLI Credential - uses Azure CLI login for developers
            AzureCliCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
            legacyAzureCliSettings.Apply(credentialOptions1);
            chain.Common.Apply(credentialOptions1);
            chain.AzureCli.Apply(credentialOptions1);
            credentials.Add(DecorateWithGetTokenRetry(new AzureCliCredential(credentialOptions1), chain.Common, chain.AzureCli));

            // Visual Studio Code Credential - uses VS Code Azure extension authentication
            VisualStudioCodeCredentialOptions credentialOptions2 = new() { AuthorityHost = authorityHost };
            chain.Common.Apply(credentialOptions2);
            chain.VisualStudioCode.Apply(credentialOptions2);
            credentials.Add(DecorateWithGetTokenRetry(new VisualStudioCodeCredential(credentialOptions2), chain.Common, chain.VisualStudioCode));

            // Visual Studio Credential - uses Visual Studio Azure Service Authentication
            VisualStudioCredentialOptions credentialOptions3 = new() { AuthorityHost = authorityHost };
            chain.Common.Apply(credentialOptions3);
            chain.VisualStudio.Apply(credentialOptions3);
            credentials.Add(DecorateWithGetTokenRetry(new VisualStudioCredential(credentialOptions3), chain.Common, chain.VisualStudio));
        }
        else
        {
            // Production environment: Add Azure-native authentication methods

            // Workload Identity Credential - for Kubernetes workload identity
            WorkloadIdentityCredentialOptions credentialOptions1 = new() { AuthorityHost = authorityHost };
            chain.Common.Apply(credentialOptions1);
            chain.WorkloadIdentity.Apply(credentialOptions1);
            credentials.Add(DecorateWithGetTokenRetry(new WorkloadIdentityCredential(credentialOptions1), chain.Common, chain.WorkloadIdentity));

            // Managed Identity setup for Client Assertion and direct usage
            TokenCredentialOptions credentialOptions2 = new() { AuthorityHost = authorityHost };
            chain.Common.Apply(credentialOptions2);
            chain.ManagedIdentity.Apply(credentialOptions2);
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
                chain.Common.Apply(credentialOptions3);
                chain.ClientAssertion.Apply(credentialOptions3);
                credentials.Add(DecorateWithGetTokenRetry(new ClientAssertionCredential(tenantId, clientId, GetAssertionAsync, credentialOptions3), chain.Common, chain.ClientAssertion));
            }

            // Managed Identity Credential - direct managed identity authentication (fallback)
            credentials.Add(DecorateWithGetTokenRetry(managedIdentityCredential, chain.Common, chain.ManagedIdentity));
        }

        // Return chained credential that tries each method in order until one succeeds
        return new ChainedTokenCredential(credentials.ToArray());
    }

    /// <summary>
    /// Shows enough of a secret to correlate it with a known value, never enough to use it.
    /// </summary>
    private static string? Redact(string? value) =>
        value is null ? null
        : value.Length <= 3 ? "..."
        : string.Concat(value.AsSpan(0, 3), "...");

    /// <summary>
    /// Reads the flat Azure CLI keys that predate the <c>Credential</c> section. They are applied before it,
    /// so the structured configuration wins where both are present.
    /// </summary>
    private CredentialSettings? ReadLegacyAzureCliSettings(IConfiguration configuration, string? tenantId)
    {
        var processTimeout = configuration["ProcessTimeout"].HardTrim();
        var additionallyAllowedTenants = configuration["AdditionallyAllowedTenants"].HardTrim();
        var subscriptionId = configuration["SubscriptionId"].HardTrim();

        if (tenantId is null && processTimeout is null && additionallyAllowedTenants is null && subscriptionId is null)
        {
            return null;
        }

        TimeSpan? parsedProcessTimeout = null;
        if (processTimeout is not null)
        {
            if (TimeSpan.TryParse(processTimeout, out TimeSpan value))
            {
                parsedProcessTimeout = value;
            }
            else
            {
                logger.LogWarning("Ignoring unparsable 'ProcessTimeout' value {ProcessTimeout}", processTimeout);
            }
        }

        return new ProcessCredentialSettings()
        {
            TenantId = tenantId,
            ProcessTimeout = parsedProcessTimeout,
            Subscription = subscriptionId,
            AdditionallyAllowedTenants = additionallyAllowedTenants?.Split(';', StringSplitOptions.RemoveEmptyEntries),
        };
    }

    /// <summary>
    /// Returns <paramref name="credential"/> decorated with the configured <c>GetTokenRetry</c> policy, taking
    /// the credential-specific setting when present and falling back to the shared one. Returns the credential
    /// unchanged when neither configures retries.
    /// </summary>
    private TokenCredential DecorateWithGetTokenRetry(TokenCredential credential, CredentialSettings? commonSettings, CredentialSettings? credentialSettings) =>
        RetryingTokenCredential.Create(credential, credentialSettings?.GetTokenRetry ?? commonSettings?.GetTokenRetry, logger);

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



