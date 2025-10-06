using Diginsight.Components;
using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

//namespace Diginsight.Components.Configuration;


public sealed class DelegatedAuthenticationHandler : DelegatingHandler
{
    private readonly ILogger<DelegatedAuthenticationHandler> logger;
    private readonly IOptionsMonitor<AuthenticatedClientOptions> authenticatedClientOptionsMonitor;

    public DelegatedAuthenticationHandler(ILogger<DelegatedAuthenticationHandler> logger,
        IOptionsMonitor<AuthenticatedClientOptions> authenticatedClientOptionsMonitor)
    {
        this.logger = logger;
        this.authenticatedClientOptionsMonitor = authenticatedClientOptionsMonitor;
    }

    public async Task<AuthenticationResult> AcquiresTokenForClientAsync(string clientName, CancellationToken cancellationToken)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { clientName });

        IAuthenticatedClientOptions aco = authenticatedClientOptionsMonitor.Get(clientName);
        if (aco.Scope is not { } joinedScopes ||
            joinedScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is not { Length: > 0 } scopes)
        {
            throw new InvalidOperationException($"{nameof(IAuthenticatedClientOptions.Scope)} is empty");
        }

        if (aco.TenantId is not { } tenantId)
        {
            throw new InvalidOperationException($"{nameof(IAuthenticatedClientOptions.TenantId)} is empty");
        }
        if (aco.ClientId is not { } appRegistrationClientId)
        {
            throw new InvalidOperationException($"{nameof(IAuthenticatedClientOptions.ClientId)} is empty");
        }
        if (aco.ClientSecret is not { } appRegistrationClientSecret)
        {
            throw new InvalidOperationException($"{nameof(IAuthenticatedClientOptions.ClientSecret)} is empty");
        }

        IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(appRegistrationClientId)
            .WithTenantId(tenantId)
            .WithClientSecret(appRegistrationClientSecret)
            .Build();


        var builder = confidentialClientApplication.AcquireTokenForClient(scopes);
        var result = await builder.ExecuteAsync(cancellationToken);
        return result;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { request });

        if (request.Headers.Authorization is null)
        {
            AuthenticationResult authenticationResult = await AcquiresTokenForClientAsync("", cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue(authenticationResult.TokenType, authenticationResult.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IManagedIdentityApplication CreateManagedIdentityApplication(string clientId)
    {
        return ManagedIdentityApplicationBuilder.Create(
                clientId is { } managedIdentityClientId ? ManagedIdentityId.WithUserAssignedClientId(managedIdentityClientId) : ManagedIdentityId.SystemAssigned
            ).Build();
    }



}
