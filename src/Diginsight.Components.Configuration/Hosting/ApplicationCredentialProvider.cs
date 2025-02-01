using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Diginsight.Components.Configuration;

public sealed class ApplicationCredentialProvider //: ICredentialProvider
{
    private readonly IHostEnvironment environment;

    public ApplicationCredentialProvider(IHostEnvironment environment)
    {
        this.environment = environment;
    }

    public TokenCredential Get(string? tenantId, string? clientId, string? clientSecret)
    {
        ICollection<TokenCredential> credentials = new List<TokenCredential>();

        if (tenantId is not null && clientId is not null && clientSecret is not null)
        {
            ClientSecretCredentialOptions credentialOptions3 = new();
            SetAuthorityHost(credentialOptions3);
            credentials.Add(new ClientSecretCredential(tenantId, clientId, clientSecret, credentialOptions3));
        }

        //if (!credentials.Any())
        //{
        if (environment.IsDevelopment())
        {
            AzureCliCredentialOptions credentialOptions4 = new();
            SetAuthorityHost(credentialOptions4);
            credentials.Add(new AzureCliCredential(credentialOptions4));

            VisualStudioCodeCredentialOptions credentialOptions5 = new();
            SetAuthorityHost(credentialOptions5);
            credentials.Add(new VisualStudioCodeCredential(credentialOptions5));

            VisualStudioCredentialOptions credentialOptions6 = new();
            SetAuthorityHost(credentialOptions6);
            credentials.Add(new VisualStudioCredential(credentialOptions6));
        }
        else
        {
            //WorkloadIdentityCredentialOptions credentialOptions1 = new();
            //SetAuthorityHost(credentialOptions1);
            //credentials.Add(new WorkloadIdentityCredential(credentialOptions1));

            TokenCredentialOptions credentialOptions2 = new();
            SetAuthorityHost(credentialOptions2);
            credentials.Add(new ManagedIdentityCredential(clientId, credentialOptions2));
        }
        //}
        return new ChainedTokenCredential(credentials.ToArray());
    }

    private void SetAuthorityHost(TokenCredentialOptions credentialOptions)
    {
        string appsettingsEnvName = Environment.GetEnvironmentVariable("AppsettingsEnvironmentName") ?? environment.EnvironmentName;
        if (appsettingsEnvName.EndsWith("cn", StringComparison.OrdinalIgnoreCase))
        {
            credentialOptions.AuthorityHost = AzureAuthorityHosts.AzureChina;
        }
    }
}



