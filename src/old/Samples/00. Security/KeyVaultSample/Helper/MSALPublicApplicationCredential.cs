using Azure.Core;
using Common;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyVaultSample
{
    public class MSALPublicApplicationCredential : TokenCredential
    {
        private readonly AuthenticationService authenticationService;
        private readonly IPublicClientApplication publicClientApp;
        private IAccount account;
        private SystemWebViewOptions options;

        public MSALPublicApplicationCredential(AuthenticationService authenticationService, SystemWebViewOptions options)
        {
            this.authenticationService = authenticationService;
            this.publicClientApp = authenticationService.App;
            this.options = options;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var result = authenticationService.AuthenticationResult;
            if (result != null && requestContext.Scopes.All(scope => result.Scopes.Contains(scope)) && result.ExpiresOn > DateTime.Now)
            {
                return new AccessToken(result.AccessToken, result.ExpiresOn);
            }

            return GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
        }

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var result = authenticationService.AuthenticationResult;
            if (result != null && requestContext.Scopes.All(scope => result.Scopes.Contains(scope)) && result.ExpiresOn > DateTime.Now)
            {
                return new AccessToken(result.AccessToken, result.ExpiresOn);
            }

            if (this.account == null) { var accounts = await publicClientApp.GetAccountsAsync(); this.account = accounts?.FirstOrDefault(); }
            if (this.account != null)
            {
                try
                {
                    result = await publicClientApp.AcquireTokenSilent(requestContext.Scopes, account).ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    return new AccessToken(result.AccessToken, result.ExpiresOn);
                }
                catch (MsalUiRequiredException)
                {
                    return await GetTokenInteractiveAsync(requestContext.Scopes, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                return await GetTokenInteractiveAsync(requestContext.Scopes, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<AccessToken> GetTokenInteractiveAsync(string[] scopes, CancellationToken cancellationToken)
        {
            var authResult = await publicClientApp.AcquireTokenInteractive(scopes)
                         .WithAccount(account)
                         .WithPrompt(Prompt.SelectAccount)
                         //.WithUseEmbeddedWebView(false)
                         //.WithSystemWebViewOptions(options)
                         .ExecuteAsync(cancellationToken);

            account = authResult.Account;
            return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
        }
    }
}