namespace Diginsight.Components;

public interface IAuthenticatedClientOptions
{
    string? Scope { get; }
    string? TenantId { get; }
    string? ClientId { get; }
    string? ClientSecret { get; }
    string? ManagedIdentityClientId { get; }
    bool UseFederatedConfidentialClient { get; }
}
