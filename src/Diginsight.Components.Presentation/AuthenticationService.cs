/*
*
* Copyright (c) Microsoft Corporation.
* All rights reserved.
*
* This code is licensed under the MIT License.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files(the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions :
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*
*/
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Windows;
using System.Windows.Interop;
//using Microsoft.InformationProtection;
//using System.Windows.Interop;
//using Microsoft.Extensions.Logging;
//using System.Windows.Interop;
//using Microsoft.Identity.Client.Broker;

namespace Diginsight.Components.Presentation;

public class AuthenticationService //: IAuthDelegate
{
    private static readonly Type T = typeof(AuthenticationService);
    private const string CONFIGVALUE_TENANTID = "TenantId";
    private const string DEFAULTVALUE_TENANTID = "common";
    private const string CONFIGVALUE_CLIENTID = "ClientId";
    private const string DEFAULTVALUE_CLIENTID = "";
    private const string CONFIGVALUE_APPNAME = "AppName";
    private const string DEFAULTVALUE_APPNAME = "";
    private const string CONFIGVALUE_APPVERSION = "AppVersion";
    private const string DEFAULTVALUE_APPVERSION = "";
    private const string CONFIGVALUE_REDIRECTURI = "RedirectUri";
    private const string DEFAULTVALUE_REDIRECTURI = "";
    private const string CONFIGVALUE_SCOPES = "Scopes";
    private const string DEFAULTVALUE_SCOPES = "";
    private const string CONFIGVALUE_DATADECLASSGROUPNAME = "GFSCL_DATADECLASS_USERS";

    private ILogger<AuthenticationService> logger;
    private string tenantId;
    private string applicationId;
    private string appName;
    private string appVersion;
    private string redirectUri;
    private Identity identity;

    private AuthenticationResult authenticationResult;
    // MSAL: Tenant is important for the quickstart. We'd need to check with Andre/Portal if we want to change to the AadAuthorityAudience.
    private Window window;
    //private string tenantId = "common";
    private string Instance = "https://login.microsoftonline.com/";

    //Set the scope for API call to user.read
    private string[] scopes = new string[] { "openid", "user.read", "user.read.all", "profile" }; // , "email", "groupmember.read.all", "group.read.all", "https://aadrm.com/user_impersonation", "https://psor.o365syncservice.com/UnifiedPolicy.User.Read"
    private string[] scopesAADRM = [ "https://aadrm.com/user_impersonation" ]; // , , "https://psor.o365syncservice.com/UnifiedPolicy.User.Read"
    private string[] scopesO365syncservice = [ "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" ];

    public AuthenticationResult AuthenticationResult { get => authenticationResult; set => authenticationResult = value; }
    public Identity Identity { get => identity; set => identity = value; }
    public string TenantId { get => tenantId; set => tenantId = value; }
    public string ApplicationId { get => applicationId; set => applicationId = value; }
    public IPublicClientApplication App { get; set; }

    public AuthenticationService(string tenantId,
        string applicationId,
        string appName,
        string appVersion,
        string redirectUri,
        string[] scopes,
        string oauthVersion,
        Window window) // ApplicationInfo appInfo,
    {
        // using (var scope = logger.BeginMethodScope(new { tenantId, applicationId, appName, appVersion, redirectUri, scopes, oauthVersion, window = window }))
        // {
        //this.logger = Factor;
        this.tenantId = tenantId;
        //if (string.IsNullOrEmpty(this.tenantId)) { this.tenantId = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID); } // , CultureInfo.InvariantCulture
        this.applicationId = applicationId;
        //if (string.IsNullOrEmpty(this.applicationId)) { this.applicationId = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID); } // , CultureInfo.InvariantCulture
        this.appName = appName;
        //if (string.IsNullOrEmpty(this.appName)) { this.appName = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_APPNAME, DEFAULTVALUE_APPNAME); } // , CultureInfo.InvariantCulture
        this.appVersion = appVersion;
        //if (string.IsNullOrEmpty(this.appVersion)) { this.appVersion = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_APPVERSION, DEFAULTVALUE_APPVERSION); } // , CultureInfo.InvariantCulture
        this.redirectUri = redirectUri;
        //if (string.IsNullOrEmpty(this.redirectUri)) { this.redirectUri = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_REDIRECTURI, DEFAULTVALUE_REDIRECTURI); } // , CultureInfo.InvariantCulture
        this.scopes = scopes;
        if (this.scopes is null)
        {
            //var scopesString = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_SCOPES, DEFAULTVALUE_SCOPES);
            //scopes = scopesString?.Split(',');
        }

        this.window = window;

        var builder = PublicClientApplicationBuilder.Create(applicationId)
            .WithAuthority($"https://login.microsoftonline.com/{tenantId}{oauthVersion}", true) //$"{Instance}{tenantId}"
            .WithDefaultRedirectUri();

        //// Use of Broker Requires redirect URI "ms-appx-web://microsoft.aad.brokerplugin/{client_id}" in app registration
        //var useWam = false;
        //if (useWam)
        //{
        //    BrokerOptions brokerOptions = new BrokerOptions(BrokerOptions.OperatingSystems.Windows);
        //    brokerOptions.ListOperatingSystemAccounts = true;
        //    builder.WithBroker(brokerOptions); //
        //}

        //.WithRedirectUri(this.redirectUri)
        App = builder.Build(); logger.LogDebug("_clientApp = PublicClientApplicationBuilder.Create(applicationId).WithAuthority($\'{Instance}{TenantId}\', true).WithRedirectUri({RedirectUri}).Build();", Instance, tenantId, this.redirectUri);

        //.WithAuthority(AzureCloudInstance.AzurePublic, "common") //$"{Instance}{tenantId}"
        TokenCacheHelper.EnableSerialization(App.UserTokenCache); logger.LogDebug($"TokenCacheHelper.EnableSerialization({App.UserTokenCache}); completed");
        //}
    }

    /// <summary>AcquireToken is called by the SDK when auth is required for an operation.
    /// Adding or loading an IFileEngine is typically where this will occur first.
    /// The SDK provides all three parameters below.Identity from the EngineSettings.
    /// Authority and resource are provided from the 401 challenge.
    /// The SDK cares only that an OAuth2 token is returned.How it's fetched isn't important.
    /// In this sample, we fetch the token using Active Directory Authentication Library(ADAL).</summary>
    /// <param name="identity"></param>
    /// <param name="authority"></param>
    /// <param name="resource"></param>
    /// <returns>The OAuth2 token for the user</returns>
    public string AcquireToken(Identity identity, string authority, string resource, string claims)
    {
        //using var scope = logger.BeginMethodScope(new { identity = identity, authority, resource, claims });

        return Task.Run(() => AcquireTokenAsync(identity, authority, resource, claims)).GetAwaiter().GetResult();
    }

    public async Task<string> AcquireTokenAsync(Identity identity, string authority, string resource, string claims)
    {
        //using var scope = logger.BeginMethodScope(new { identity = identity, authority, resource, claims });

        AuthenticationResult authResult;
        var app = App;

        var accounts = await App.GetAccountsAsync(); logger.LogDebug($"await app.GetAccountsAsync(); returned {accounts}");
        var firstAccount = accounts.FirstOrDefault();
        //logger.LogDebug(new { firstAccount = firstAccount });

        try
        {
            var scopes = this.scopes;
            if (resource.StartsWith("https://aadrm.com", StringComparison.InvariantCultureIgnoreCase)) { scopes = scopesAADRM; }
            if (resource.StartsWith("https://syncservice.o365syncservice.com/", StringComparison.InvariantCultureIgnoreCase)) { scopes = scopesO365syncservice; }

            authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync(); logger.LogDebug($"await app.AcquireTokenSilent({scopesAADRM}, firstAccount).ExecuteAsync(); returned {authResult}");
        }
        catch (MsalUiRequiredException ex)
        {
            // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
            logger.LogDebug($"MsalUiRequiredException: {ex.Message}");

            try
            {
                if (window is not null && false)
                {
                    IntPtr handle = new WindowInteropHelper(window).Handle;

                    authResult = await app.AcquireTokenInteractive(scopesAADRM).WithAccount(firstAccount)
                        .WithParentActivityOrWindow(handle)
                        .WithPrompt(Prompt.SelectAccount).ExecuteAsync();
                }
                else
                {
                    //var handle = new WindowInteropHelper(_window).Handle;
                    authResult = await app.AcquireTokenInteractive(scopesAADRM).WithAccount(firstAccount)
                        .WithPrompt(Prompt.SelectAccount).ExecuteAsync();
                }
                logger.LogDebug($"await app.AcquireTokenInteractive({scopesAADRM}).WithAccount(firstAccount).WithParentActivityOrWindow(new WindowInteropHelper(this).Handle).WithPrompt({Prompt.SelectAccount}).ExecuteAsync(); returned {authResult}");
            }
            catch (MsalException msalex)
            {
                //logger.LogException(msalex);
                throw;
            }
        }
        catch (Exception ex)
        {
            //logger.LogException(ex);
            throw;
        }

        // Return the token. The token is sent to the resource.
        //logger.Result = authResult.AccessToken;
        return authResult.AccessToken;
    }

    public async Task<Identity> GetUserIdentitySylentAsync(Action<Identity> onComplete) // , PromptBehavior promptBehavior = PromptBehavior.Auto
    {
        //using (var scope = logger.BeginMethodScope(new { onComplete })) // , promptBehavior
        //{
        try
        {
            Identity identity = null;

            var app = App;

            var accounts = await App.GetAccountsAsync(); logger.LogDebug($"await app.GetAccountsAsync(); returned {accounts}");
            var firstAccount = accounts.FirstOrDefault();
            //logger.LogDebug(new { firstAccount = firstAccount });

            var scopes = this.scopes;
            try
            {
                AuthenticationResult authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync(); logger.LogDebug($"await app.AcquireTokenSilent({scopes}, firstAccount).ExecuteAsync(); returned {authResult}");

                var account = authResult.Account;
                var claims = authResult.ClaimsPrincipal.Claims;

                var upn = account.Username;
                var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                var tid = claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                //var handler = new JwtSecurityTokenHandler();
                //var token = handler.ReadJwtToken(authResult.AccessToken);
                //var upn = token?.Claims?.FirstOrDefault(c => c.Type == "upn")?.Value;
                //var name = token?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                //var email = token?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                //var tid = token?.Claims?.FirstOrDefault(c => c.Type == "tid")?.Value;

                Identity = identity = authResult.Account is not null ? new Identity(upn, email, name ?? authResult.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                TenantId = tid;
            }
            catch (MsalUiRequiredException ex)
            {
                Identity = null;
                TenantId = null;
                //logger.LogException(ex);
                logger.LogInformation($"MsalUiRequiredException: {ex.Message}");
                // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
            }
            catch (Exception ex)
            {
                Identity = null;
                TenantId = null;
                //logger.LogException(ex);
                throw;
            }

            //logger.Result = identity;
            return identity;
        }
        catch (Exception ex)
        {
            Identity = null;
            TenantId = null;
            //logger.LogException(ex);
        }
        finally
        {
            if (onComplete is not null) { onComplete(Identity); }
        }
        return null;
        //}
    }
    public async Task<Identity> GetUserIdentityAsync(Action<Identity> onComplete) // , PromptBehavior promptBehavior = PromptBehavior.Auto
    {
        //using (var scope = logger.BeginMethodScope(new { onComplete })) // , promptBehavior
        //{
        try
        {
            Identity identity = null;
            //Identity manager = null;

            var authResult = default(AuthenticationResult);
            var app = App;

            var accounts = await App.GetAccountsAsync(); logger.LogDebug($"await app.GetAccountsAsync(); returned {accounts}");
            var firstAccount = accounts.FirstOrDefault();
            //logger.LogDebug(new { firstAccount = firstAccount });

            var scopes = this.scopes;
            if (true)
            {
                try
                {
                    authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync(); logger.LogDebug($"await app.AcquireTokenSilent({scopes}, firstAccount).ExecuteAsync(); returned {authResult}");

                    var account = authResult.Account;
                    var claims = authResult.ClaimsPrincipal.Claims;

                    var upn = account.Username;
                    var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                    var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                    var tid = claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                    //var handler = new JwtSecurityTokenHandler();
                    //var token = handler.ReadJwtToken(authResult.AccessToken);
                    //var upn = token?.Claims?.FirstOrDefault(c => c.Type == "upn")?.Value;
                    //var name = token?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                    //var email = token?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                    //var tid = token?.Claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                    bool IsDeclassifier = false;

                    //if (string.IsNullOrEmpty(email))
                    //{
                    //    using (var client = HttpHelper.GetHttpClient("application/json", authResult.AccessToken))
                    //    {
                    //        // GetMe
                    //        var profile = await GraphHelper.GetMe(client);
                    //        email = profile?.Mail;

                    //        // GetManager
                    //        profile = await GraphHelper.GetManager(upn, client);
                    //        if (profile != null)
                    //        {
                    //            manager = new Identity(profile?.UserPrincipalName, profile?.DisplayName, profile?.Mail);
                    //        }

                    //        // IsDeclassifier
                    //        IsDeclassifier = await GraphHelper.IsDeclassifier(upn, CONFIGVALUE_DATADECLASSGROUPNAME, client);
                    //    }
                    //}

                    Identity = identity = authResult.Account is not null ? new Identity(upn, email, name ?? authResult.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                    TenantId = tid;
                    AuthenticationResult = authResult;
                }
                catch (MsalUiRequiredException ex)
                {
                    Identity = null;
                    AuthenticationResult = null;
                    TenantId = null;
                    //logger.LogException(ex);
                    // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
                    logger.LogInformation($"MsalUiRequiredException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Identity = null;
                    AuthenticationResult = null;
                    TenantId = null;
                    //logger.LogException(ex);
                    throw;
                }
            }

            if (string.IsNullOrEmpty(authResult?.AccessToken)) // && promptBehavior != PromptBehavior.Auto
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        //using (var sec1 = TraceLogger.BeginMethodScope(T, "AcquireTokenInteractive"))
                        //{   // WithParentActivityOrWindow(handle).
                        authResult = await app.AcquireTokenInteractive(scopes).WithAccount(firstAccount).WithPrompt(Prompt.SelectAccount).ExecuteAsync();

                        var account = authResult.Account;
                        var claims = authResult.ClaimsPrincipal.Claims;

                        var upn = account.Username;
                        var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                        var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                        var tid = claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                        //var handler = new JwtSecurityTokenHandler();
                        //var token = handler.ReadJwtToken(authResult.AccessToken);
                        //var upn = token?.Claims?.FirstOrDefault(c => c.Type == "upn")?.Value;
                        //var name = token?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                        //var email = token?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                        //var tid = token?.Claims?.FirstOrDefault(c => c.Type == "tid")?.Value;

                        //if (string.IsNullOrEmpty(email))
                        //{
                        //    // server.Execute<UserProfile>(GET, GRAPHAPI_ME, request) UPSGuard
                        //    using (var client = HttpHelper.GetHttpClient("application/json", authResult.AccessToken))
                        //    {
                        //        // GetMe
                        //        var profile = await GraphHelper.GetMe(client);
                        //        email = profile?.Mail;

                        //        // GetManager
                        //        profile = await GraphHelper.GetManager(upn, client);
                        //        if (profile != null)
                        //        {
                        //            manager = new Identity(profile?.UserPrincipalName, profile?.DisplayName, profile?.Mail);
                        //        }

                        //        // IsDeclassifier
                        //        IsDeclassifier = await GraphHelper.IsDeclassifier(upn, CONFIGVALUE_DATADECLASSGROUPNAME, client);
                        //    }
                        //}

                        Identity = identity = authResult?.Account != null ? new Identity(upn, email, name ?? authResult.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                        TenantId = tid;
                        return authResult;
                        //}
                    });

                    Identity = identity;
                    AuthenticationResult = authResult;

                    // TODO: await task; not working https://stackoverflow.com/questions/62658840/msal-application-freeze-on-wpf
                    //while (result1 == null) { System.Threading.Thread.Sleep(20); }
                    //authResult = result1;
                }
                catch (MsalException msalex)
                {
                    Identity = null;
                    AuthenticationResult = null;
                    TenantId = null;
                    //logger.LogException(msalex);
                    throw;
                }
            }

            //logger.Result = identity;
            return identity;
        }
        catch (Exception ex)
        {
            Identity = null;
            AuthenticationResult = null;
            TenantId = null;
            //logger.LogException(ex);
        }
        finally
        {
            onComplete?.Invoke(Identity);
        }
        return Identity;
        //}
    }

    public void Logout()
    {
        //using var scope = logger.BeginMethodScope();

        Task.Run(async () => { await LogoutAsync(); }).GetAwaiter().GetResult();
        Identity = null;
        TenantId = null;
    }
    public async Task LogoutAsync()
    {
        //using var scope = logger.BeginMethodScope();

        var accounts = await App.GetAccountsAsync(); logger.LogDebug($"await publicClientApp.GetAccountsAsync(); return {accounts}");
        logger.LogInformation("Clears the library cache. Does not affect the browser cookies.");
        logger.LogDebug($"while (accounts.Any()) {accounts?.Count()}");

        while (accounts.Any())
        {
            var account = accounts.First();
            await App.RemoveAsync(account); logger.LogDebug($"await _app.RemoveAsync({account});");
            accounts = await App.GetAccountsAsync(); logger.LogDebug($"await publicClientApp.GetAccountsAsync(); returned {accounts}");
            Identity = null;
            //    this.TenantId = null;
        }
    }

    public async Task<Identity> LoginSilentAsync()
    {
        //using var scope = logger.BeginMethodScope();

        var accounts = await App.GetAccountsAsync();
        logger.LogDebug($"await publicClientApp.GetAccountsAsync(); returned {accounts}");

        if (accounts.Any())
        {
            try
            {
                var result = await App.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true);
                logger.LogDebug($"await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true); returned {result}");

                var account = result.Account;
                var claims = result.ClaimsPrincipal.Claims;

                var upn = account.Username;
                var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                var tid = claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                //var handler = new JwtSecurityTokenHandler();
                //var token = handler.ReadJwtToken(result.AccessToken);
                //var upn = token?.Claims?.FirstOrDefault(c => c.Type == "upn")?.Value;
                //var name = token?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                //var email = token?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                //var tid = token?.Claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                return result.Account != null ? new Identity(upn, email, name ?? result.Account.Username) : null;
            }
            catch (MsalUiRequiredException ex)
            {
                //logger.Log(ex);
                throw;
            }
        }

        return null;
    }
    public async Task<Identity> LoginAsync()
    {
        //using var scope = logger.BeginMethodScope();

        var accounts = await App.GetAccountsAsync();
        logger.LogDebug($"(await publicClientApp.GetAccountsAsync()).ToList(); returned {accounts}");

        try
        {
            var result = await App.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true);
            logger.LogDebug($"await _app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true); returned {result}");

            var account = result.Account;
            var claims = result.ClaimsPrincipal.Claims;

            var upn = account.Username;
            var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
            var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value;
            var tid = claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
            //var handler = new JwtSecurityTokenHandler();
            //var token = handler.ReadJwtToken(result.AccessToken);
            //var upn = token?.Claims?.FirstOrDefault(c => c.Type == "upn")?.Value;
            //var name = token?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
            //var email = token?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
            //var tid = token?.Claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
            return result.Account != null ? new Identity(upn, email, name ?? result.Account.Username) : null;
        }
        catch (MsalUiRequiredException)
        {
            try
            {
                // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                // and we don't necessarily want to re-sign-in the same user
                var builder = App.AcquireTokenInteractive(scopes).WithAccount(accounts.FirstOrDefault()).WithPrompt(Prompt.SelectAccount);
                logger.LogDebug($"_app.AcquireTokenInteractive({scopes}).WithAccount({accounts.FirstOrDefault()}).WithPrompt({Prompt.SelectAccount}); returned {builder}");

                var isEmbeddedWebViewAvailable = App.IsEmbeddedWebViewAvailable(); logger.LogDebug($"_app.IsEmbeddedWebViewAvailable() returned {isEmbeddedWebViewAvailable}");
                if (!isEmbeddedWebViewAvailable)
                {
                    // You app should install the embedded browser WebView2 https://aka.ms/msal-net-webview2
                    // but if for some reason this is not possible, you can fall back to the system browser
                    // in this case, the redirect uri needs to be set to "http://localhost"
                    builder = builder.WithUseEmbeddedWebView(false); logger.LogDebug($"builder.WithUseEmbeddedWebView(false); returned {builder}");
                }

                var result = await builder.ExecuteAsync().ConfigureAwait(true);
                logger.LogDebug($"await builder.ExecuteAsync().ConfigureAwait(true); returned {result}");
                var account = result.Account;
                var claims = result.ClaimsPrincipal.Claims;

                var upn = account.Username;
                var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                var email = claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                var tid = claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                //var handler = new JwtSecurityTokenHandler();
                //var token = handler.ReadJwtToken(result.AccessToken);
                //var upn = token?.Claims?.FirstOrDefault(c => c.Type == "upn")?.Value;
                //var name = token?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                //var email = token?.Claims?.FirstOrDefault(c => c.Type == "email")?.Value;
                //var tid = token?.Claims?.FirstOrDefault(c => c.Type == "tid")?.Value;
                return new Identity(upn, email, name ?? result.Account.Username);
            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == "access_denied")
                {
                    // The user canceled sign in, take no action.
                }
                else
                {
                    // An unexpected error occurred.
                    string message = ex.Message;
                    if (ex.InnerException != null)
                    {
                        message += "Error Code: " + ex.ErrorCode + "Inner Exception : " + ex.InnerException.Message;
                    }

                    //MessageBox.Show(message);
                }

                //Dispatcher.Invoke(() =>
                //{
                //    // UserName.Content = Properties.Resources.UserNotSignedIn;
                //});
            }
        }

        return null;
    }
}
