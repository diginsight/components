namespace Diginsight.Components;

public interface IAuthenticatedClientOptions
{
    string? Scope { get; }
    string? TenantId { get; }
    string? AppRegistrationClientId { get; }
    string? AppRegistrationClientSecret { get; }
    string? ManagedIdentityClientId { get; }
    bool UseFederatedConfidentialClient { get; }
}
