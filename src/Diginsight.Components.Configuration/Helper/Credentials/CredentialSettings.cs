using Azure.Core;

namespace Diginsight.Components.Configuration;

/// <summary>
/// Configuration surface shared by every credential built by <see cref="DefaultCredentialProvider"/>.
/// Every member is nullable: an unset member leaves the Azure SDK default untouched.
/// </summary>
public class CredentialSettings
{
    /// <summary>
    /// Overrides the authority host derived from the environment. Leave unset to keep the derived value.
    /// </summary>
    public Uri? AuthorityHost { get; set; }

    /// <summary>
    /// The tenant the credential authenticates to by default. Honoured only by credentials whose
    /// options type declares it; credentials that take the tenant as a constructor argument ignore it.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Additional tenants the credential may acquire tokens for. Replaces the existing list rather than
    /// appending to it. Use <c>*</c> to allow any tenant.
    /// </summary>
    public IList<string>? AdditionallyAllowedTenants { get; set; }

    /// <summary>
    /// Enables ETW logging that may contain personally identifiable information.
    /// </summary>
    public bool? IsUnsafeSupportLoggingEnabled { get; set; }

    /// <summary>
    /// Skips Microsoft Entra instance discovery. Honoured only by credentials whose options type declares it.
    /// </summary>
    public bool? DisableInstanceDiscovery { get; set; }

    /// <summary>
    /// Retry options for the credential's HTTP pipeline. This does not retry token acquisition itself;
    /// see <see cref="GetTokenRetry"/> for that.
    /// </summary>
    public CredentialRetrySettings? Retry { get; set; }

    /// <summary>
    /// Logging, telemetry and distributed-tracing options for the credential.
    /// </summary>
    public CredentialDiagnosticsSettings? Diagnostics { get; set; }

    /// <summary>
    /// Retries around <c>GetToken</c> itself, which is the only layer that can recover from a subprocess
    /// timeout such as <c>Azure CLI authentication timed out</c>.
    /// </summary>
    public GetTokenRetrySettings? GetTokenRetry { get; set; }
}

/// <summary>
/// Settings for credentials that acquire tokens by launching an external process.
/// </summary>
public sealed class ProcessCredentialSettings : CredentialSettings
{
    /// <summary>
    /// How long the credential waits for the external process before failing.
    /// </summary>
    public TimeSpan? ProcessTimeout { get; set; }

    /// <summary>
    /// The subscription name or id passed to the Azure CLI. Honoured by the Azure CLI credential only.
    /// </summary>
    public string? Subscription { get; set; }
}

/// <summary>
/// Settings specific to the client certificate credential.
/// </summary>
public sealed class ClientCertificateCredentialSettings : CredentialSettings
{
    /// <summary>
    /// Includes the x5c header in client claims, enabling subject name / issuer authentication.
    /// </summary>
    public bool? SendCertificateChain { get; set; }
}

/// <summary>
/// Settings specific to the workload identity credential.
/// </summary>
public sealed class WorkloadIdentityCredentialSettings : CredentialSettings
{
    /// <summary>
    /// The client id of the service principal the federated token is exchanged for.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Path to the file holding the workload identity token.
    /// </summary>
    public string? TokenFilePath { get; set; }
}

/// <summary>
/// Mirrors the bindable members of <see cref="RetryOptions"/>.
/// </summary>
public sealed class CredentialRetrySettings
{
    public int? MaxRetries { get; set; }

    public TimeSpan? Delay { get; set; }

    public TimeSpan? MaxDelay { get; set; }

    public RetryMode? Mode { get; set; }

    public TimeSpan? NetworkTimeout { get; set; }
}

/// <summary>
/// Mirrors the bindable members of <see cref="Azure.Identity.TokenCredentialDiagnosticsOptions"/>.
/// </summary>
public sealed class CredentialDiagnosticsSettings
{
    public string? ApplicationId { get; set; }

    public bool? IsLoggingEnabled { get; set; }

    public bool? IsLoggingContentEnabled { get; set; }

    public bool? IsTelemetryEnabled { get; set; }

    public bool? IsDistributedTracingEnabled { get; set; }

    public bool? IsAccountIdentifierLoggingEnabled { get; set; }

    public int? LoggedContentSizeLimit { get; set; }
}

/// <summary>
/// Retry policy applied around token acquisition.
/// </summary>
public sealed class GetTokenRetrySettings
{
    /// <summary>
    /// Total number of attempts, including the first. A value of 1 or less disables the wrapper entirely.
    /// </summary>
    public int? MaxAttempts { get; set; }

    public TimeSpan? Delay { get; set; }

    public TimeSpan? MaxDelay { get; set; }

    public RetryMode? Mode { get; set; }
}

/// <summary>
/// The <c>Credential</c> subsection of the configuration section handed to
/// <see cref="ICredentialProvider.Get(Microsoft.Extensions.Configuration.IConfiguration)"/>.
/// <see cref="Common"/> applies to every credential in the chain; each named member overrides it for one credential.
/// </summary>
public sealed class CredentialChainSettings
{
    /// <summary>Name of the subsection holding these settings.</summary>
    public const string SectionName = "Credential";

    public CredentialSettings? Common { get; set; }

    public CredentialSettings? ClientSecret { get; set; }

    public ClientCertificateCredentialSettings? ClientCertificate { get; set; }

    public ProcessCredentialSettings? AzureCli { get; set; }

    public CredentialSettings? VisualStudioCode { get; set; }

    public CredentialSettings? VisualStudio { get; set; }

    public WorkloadIdentityCredentialSettings? WorkloadIdentity { get; set; }

    public CredentialSettings? ClientAssertion { get; set; }

    public CredentialSettings? ManagedIdentity { get; set; }
}
