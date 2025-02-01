using Diginsight.Options;

namespace AuthenticationSampleClient;

public class AuthenticationSampleApiOptions : IDynamicallyConfigurable
{
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Uri { get; set; }
}
