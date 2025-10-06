using Diginsight;
using Diginsight.Components;
using Diginsight.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

//namespace Diginsight.Components.Configuration;


public sealed class ApplicationAuthenticationHandler : DelegatingHandler
{
    private readonly string clientName;
    private readonly ILogger<ApplicationAuthenticationHandler> logger;
    private readonly IOptionsMonitor<AuthenticatedClientOptions> authenticatedClientOptionsMonitor;
    private readonly IHttpClientFactory httpClientFactory;

    public ApplicationAuthenticationHandler(
        ILogger<ApplicationAuthenticationHandler> logger,
        IOptionsMonitor<AuthenticatedClientOptions> authenticatedClientOptionsMonitor,
        IHttpClientFactory httpClientFactory,
        string clientName)
    {
        this.clientName = clientName;
        this.logger = logger;
        this.authenticatedClientOptionsMonitor = authenticatedClientOptionsMonitor;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<string?> GetManagedIdentityClientIdAsync(CancellationToken cancellationToken)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { clientName });

        try
        {
            using var httpClient = httpClientFactory.CreateClient("Unauthenticated");
            httpClient.DefaultRequestHeaders.Add("Metadata", "true");
            var response = await httpClient.GetAsync("http://169.254.169.254/metadata/identity/oauth2/token?api-version=2019-08-01&resource=https://management.azure.com/", cancellationToken); logger.LogDebug(@"response = await httpClient.GetAsync(""http://169.254.169.254/metadata/identity/oauth2/token?api-version=2019-08-01&resource=https://management.azure.com/"", cancellationToken);");
            logger.LogDebug("response = {response}", response);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("client_id", out var clientIdElement))
                {
                    return clientIdElement.GetString();
                }
            }
        }
        catch (Exception ex)
        {
            var message = $"Unhandled exception '{ex.GetType().Name}' occurred processing {activity.OperationName}.";
            logger.LogError(ex, message);
        }

        return null;
    }
    public async Task<AuthenticationResult> AcquiresTokenForClientAsync(CancellationToken cancellationToken)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { clientName });

        AuthenticationResult result = default!;
        try
        {
            IAuthenticatedClientOptions options = authenticatedClientOptionsMonitor.Get(clientName);
            if (options.Scope is not { } scope) { throw new InvalidOperationException($"{nameof(IAuthenticatedClientOptions.Scope)} is empty"); }

            var tenantId = options.TenantId.HardTrim();
            var clientId = options.ClientId.HardTrim();
            var clientSecret = options.ClientSecret.HardTrim();
            logger.LogDebug("tenantId = {tenantId}, clientId = {clientId}, clientSecret = {clientSecret}", tenantId, clientId, clientSecret.Mask());
            if (result is null && tenantId is not null && clientId is not null && clientSecret is not null)
            {
                var confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithTenantId(tenantId)
                    .WithClientSecret(clientSecret)
                    .Build(); logger.LogDebug($"var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create({clientId}).WithTenantId({tenantId}).WithClientSecret({clientSecret.Mask()}).Build();");

                var builder = confidentialClientApplication.AcquireTokenForClient([scope]); logger.LogDebug($"var builder = confidentialClientApplication.AcquireTokenForClient([{scope}]);");
                result = await builder.ExecuteAsync(cancellationToken); logger.LogDebug("result = await builder.ExecuteAsync(cancellationToken);");
            }

            if (result is null)
            {
                tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID").HardTrim();
                clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID").HardTrim();
                var authorityHost = Environment.GetEnvironmentVariable("AZURE_AUTHORITY_HOST").HardTrim();
                logger.LogDebug("tenantId = {tenantId}, clientId = {clientId}, authorityHost = {authorityHost}", tenantId, clientId, authorityHost);
                if (tenantId is not null && clientId is not null && authorityHost is not null)
                {
                    IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                        .Create(clientId)
                        .WithTenantId(tenantId)
                        .WithAuthority(authorityHost)
                        .WithClientAssertion(static ct =>
                        {
                            var token = File.ReadAllTextAsync(Environment.GetEnvironmentVariable("AZURE_FEDERATED_TOKEN_FILE")!, ct);
                            return token;
                        })
                        .Build();

                    var builder = confidentialClientApplication.AcquireTokenForClient([scope]);
                    result = await builder.ExecuteAsync(cancellationToken);
                }
            }

            if (result is null)
            {
                //tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID").HardTrim();
                clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID").HardTrim();
                var msiEndpoint = Environment.GetEnvironmentVariable("MSI_ENDPOINT").HardTrim();
                var msiSecret = Environment.GetEnvironmentVariable("MSI_SECRET").HardTrim();
                logger.LogDebug("tenantId = {tenantId}, clientId = {clientId}, msiEndpoint = {msiEndpoint}, msiSecret = {msiSecret}", tenantId, clientId, msiEndpoint, msiSecret.Mask());
                if (msiEndpoint is not null && msiSecret is not null)
                {
                    var managedIdentityClientId = options.ManagedIdentityClientId.HardTrim(); logger.LogDebug("managedIdentityClientId: {managedIdentityClientId}", managedIdentityClientId);
                    //if (managedIdentityClientId is null)
                    //{
                    //    managedIdentityClientId = (await GetManagedIdentityClientIdAsync(cancellationToken)).HardTrim();
                    //}
                    ManagedIdentityId managedIdentityId = managedIdentityClientId is not null ? ManagedIdentityId.WithUserAssignedClientId(managedIdentityClientId) : ManagedIdentityId.SystemAssigned;
                    IManagedIdentityApplication managedIdentityApplication = ManagedIdentityApplicationBuilder
                        .Create(managedIdentityId)
                        .Build();

                    var builder = managedIdentityApplication.AcquireTokenForManagedIdentity(scope);
                    result = await builder.ExecuteAsync(cancellationToken);
                }
            }

            if (result is null && options.UseFederatedConfidentialClient)
            {
                ManagedIdentityId managedIdentityId = options.ManagedIdentityClientId is { } managedIdentityClientId ? ManagedIdentityId.WithUserAssignedClientId(managedIdentityClientId) : ManagedIdentityId.SystemAssigned;
                IManagedIdentityApplication managedIdentityApplication = ManagedIdentityApplicationBuilder
                    .Create(managedIdentityId)
                    .Build();

                IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithTenantId(tenantId)
                    .WithClientAssertion(async ct =>
                    {
                        AuthenticationResult authenticationResult = await managedIdentityApplication
                            .AcquireTokenForManagedIdentity("api://AzureADTokenExchange")
                            .ExecuteAsync(ct);
                        return authenticationResult.AccessToken;
                    })
                    .Build();

                var builder = confidentialClientApplication.AcquireTokenForClient([scope]);
                result = await builder.ExecuteAsync(cancellationToken);
            }
        }
        catch (Exception ex) { logger.LogError(ex, $"Exception {ex.GetType().Name} occured in {activity?.DisplayName}"); }

        activity?.SetOutput(result);
        return result;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { request });

        if (request.Headers.Authorization is null)
        {
            AuthenticationResult authenticationResult = await AcquiresTokenForClientAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue(authenticationResult.TokenType, authenticationResult.AccessToken);
        }

        var result = await base.SendAsync(request, cancellationToken);
        activity?.SetOutput(result);
        return result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IManagedIdentityApplication CreateManagedIdentityApplication(string clientId)
    {
        return ManagedIdentityApplicationBuilder.Create(
                clientId is { } managedIdentityClientId ? ManagedIdentityId.WithUserAssignedClientId(managedIdentityClientId) : ManagedIdentityId.SystemAssigned
            ).Build();
    }

}
