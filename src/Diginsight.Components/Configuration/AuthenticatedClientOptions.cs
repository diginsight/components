namespace Diginsight.Components;

public sealed class AuthenticatedClientOptions : IAuthenticatedClientOptions
{
    private string? scope;
    private string? tenantId;
    private string? appRegistrationClientId;
    private string? appRegistrationClientSecret;
    private string? managedIdentityClientId;

    public string? Scope
    {
        get => scope;
        set => scope = value.HardTrim();
    }

    public string? TenantId
    {
        get => tenantId;
        set => tenantId = value.HardTrim();
    }

    public string? ClientId
    {
        get => appRegistrationClientId;
        set => appRegistrationClientId = value.HardTrim();
    }

    public string? ClientSecret
    {
        get => appRegistrationClientSecret;
        set => appRegistrationClientSecret = value.HardTrim();
    }

    public string? ManagedIdentityClientId
    {
        get => managedIdentityClientId;
        set => managedIdentityClientId = value.HardTrim();
    }

    public bool UseFederatedConfidentialClient { get; set; }
}
