#region using
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
using System;
using System.Configuration;
//using Microsoft.InformationProtection;
using Microsoft.Identity.Client;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
//using System.Windows.Interop;
using Microsoft.Identity.Client.Extensibility;
using Microsoft.Extensions.Logging;
//using System.Windows.Interop;
//using Microsoft.Identity.Client.Broker;
using System.Security.Principal;
using System.Windows.Interop;
//using Microsoft.Identity.Client.Broker;
#endregion
namespace Diginsight.Components
{
    public class AuthenticationService //: IAuthDelegate
    {
        #region constants
        private static readonly Type T = typeof(AuthenticationService);
        const string CONFIGVALUE_TENANTID = "TenantId"; const string DEFAULTVALUE_TENANTID = "common";
        const string CONFIGVALUE_CLIENTID = "ClientId"; const string DEFAULTVALUE_CLIENTID = "";
        const string CONFIGVALUE_APPNAME = "AppName"; const string DEFAULTVALUE_APPNAME = "";
        const string CONFIGVALUE_APPVERSION = "AppVersion"; const string DEFAULTVALUE_APPVERSION = "";
        const string CONFIGVALUE_REDIRECTURI = "RedirectUri"; const string DEFAULTVALUE_REDIRECTURI = "";
        const string CONFIGVALUE_SCOPES = "Scopes"; const string DEFAULTVALUE_SCOPES = "";
        const string CONFIGVALUE_DATADECLASSGROUPNAME = "GFSCL_DATADECLASS_USERS";
        #endregion

        #region internal state
        private ILogger<AuthenticationService> logger;
        string tenantId;
        string applicationId;
        string appName;
        string appVersion;
        string redirectUri;
        Identity identity;
        AuthenticationResult authenticationResult;
        // MSAL: Tenant is important for the quickstart. We'd need to check with Andre/Portal if we want to change to the AadAuthorityAudience.
        Window window;
        //private string tenantId = "common";
        private string Instance = "https://login.microsoftonline.com/";
        private IPublicClientApplication publicClientApp;
        //Set the scope for API call to user.read
        string[] scopes = new string[] { "openid", "user.read", "user.read.all", "profile" }; // , "email", "groupmember.read.all", "group.read.all", "https://aadrm.com/user_impersonation", "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" 
        string[] scopesAADRM = new string[] { "https://aadrm.com/user_impersonation" }; // , , "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" 
        string[] scopesO365syncservice = new string[] { "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" };
        #endregion

        public AuthenticationResult AuthenticationResult { get => authenticationResult; set => authenticationResult = value; }
        public Identity Identity { get => identity; set => identity = value; }
        public string TenantId { get => tenantId; set => tenantId = value; }
        public string ApplicationId { get => applicationId; set => applicationId = value; }
        public IPublicClientApplication App { get => publicClientApp; set => publicClientApp = value; }

        #region .ctor
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
            if (this.scopes == null)
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
            publicClientApp = builder.Build(); logger.LogDebug($@"_clientApp = PublicClientApplicationBuilder.Create(applicationId).WithAuthority($'{Instance}{tenantId}', true).WithRedirectUri({this.redirectUri}).Build();");

            //.WithAuthority(AzureCloudInstance.AzurePublic, "common") //$"{Instance}{tenantId}"
            TokenCacheHelper.EnableSerialization(publicClientApp.UserTokenCache); logger.LogDebug($"TokenCacheHelper.EnableSerialization({publicClientApp.UserTokenCache}); completed");
            //}

        }
        #endregion

        #region AcquireToken
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

            var ret = "";
            ret = Task.Run(async () => { return await AcquireTokenAsync(identity, authority, resource, claims); }).GetAwaiter().GetResult();
            return ret;
        }
        #endregion
        #region AcquireTokenAsync
        public async Task<string> AcquireTokenAsync(Identity identity, string authority, string resource, string claims)
        {
            //using var scope = logger.BeginMethodScope(new { identity = identity, authority, resource, claims });

            try
            {
                var authResult = default(Microsoft.Identity.Client.AuthenticationResult);
                var app = publicClientApp;

                var accounts = await publicClientApp.GetAccountsAsync(); logger.LogDebug($"await app.GetAccountsAsync(); returned {accounts}");
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
                        if (window != null && false)
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
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        public async Task<Identity> GetUserIdentitySylentAsync(Action<Identity> onComplete) // , PromptBehavior promptBehavior = PromptBehavior.Auto
        {
            //using (var scope = logger.BeginMethodScope(new { onComplete })) // , promptBehavior
            //{
            try
            {
                Identity identity = null;

                var authResult = default(Microsoft.Identity.Client.AuthenticationResult);
                var app = publicClientApp;

                var accounts = await publicClientApp.GetAccountsAsync(); logger.LogDebug($"await app.GetAccountsAsync(); returned {accounts}");
                var firstAccount = accounts.FirstOrDefault();
                //logger.LogDebug(new { firstAccount = firstAccount });

                var scopes = this.scopes;
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

                    this.Identity = identity = authResult != null && authResult.Account != null ? new Identity(upn, email, name ?? authResult.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                    this.TenantId = tid;
                }
                catch (MsalUiRequiredException ex)
                {
                    this.Identity = null;
                    this.TenantId = null;
                    //logger.LogException(ex);
                    logger.LogInformation($"MsalUiRequiredException: {ex.Message}");
                    // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
                }
                catch (Exception ex)
                {
                    this.Identity = null;
                    this.TenantId = null;
                    //logger.LogException(ex);
                    throw;
                }

                //logger.Result = identity;
                return identity;
            }
            catch (Exception ex)
            {
                this.Identity = null;
                this.TenantId = null;
                //logger.LogException(ex);
            }
            finally
            {
                if (onComplete != null) { onComplete(this.Identity); }
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

                var authResult = default(Microsoft.Identity.Client.AuthenticationResult);
                var app = publicClientApp;

                var accounts = await publicClientApp.GetAccountsAsync(); logger.LogDebug($"await app.GetAccountsAsync(); returned {accounts}");
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

                        this.Identity = identity = authResult != null && authResult.Account != null ? new Identity(upn, email, name ?? authResult.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                        this.TenantId = tid;
                        this.AuthenticationResult = authResult;
                    }
                    catch (MsalUiRequiredException ex)
                    {
                        this.Identity = null;
                        this.AuthenticationResult = null;
                        this.TenantId = null;
                        //logger.LogException(ex);
                        // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
                        logger.LogInformation($"MsalUiRequiredException: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        this.Identity = null;
                        this.AuthenticationResult = null;
                        this.TenantId = null;
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

                            this.Identity = identity = authResult != null && authResult.Account != null ? new Identity(upn, email, name ?? authResult.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                            this.TenantId = tid;
                            return authResult;
                            //}
                        });

                        this.Identity = identity;
                        this.AuthenticationResult = authResult;

                        // TODO: await task; not working https://stackoverflow.com/questions/62658840/msal-application-freeze-on-wpf
                        //while (result1 == null) { System.Threading.Thread.Sleep(20); }
                        //authResult = result1;
                    }
                    catch (MsalException msalex)
                    {
                        this.Identity = null;
                        this.AuthenticationResult = null;
                        this.TenantId = null;
                        //logger.LogException(msalex);
                        throw;
                    }
                }

                //logger.Result = identity;
                return identity;
            }
            catch (Exception ex)
            {
                this.Identity = null;
                this.AuthenticationResult = null;
                this.TenantId = null;
                //logger.LogException(ex);
            }
            finally
            {
                if (onComplete != null) { onComplete(this.Identity); }
            }
            return this.Identity;
            //}
        }

        public void Logout()
        {
            //using var scope = logger.BeginMethodScope();

            Task.Run(async () => { await LogoutAsync(); }).GetAwaiter().GetResult();
            this.Identity = null;
            this.TenantId = null;
        }
        public async Task LogoutAsync()
        {
            //using var scope = logger.BeginMethodScope();

            var accounts = await publicClientApp.GetAccountsAsync(); logger.LogDebug($"await publicClientApp.GetAccountsAsync(); return {accounts}");
            logger.LogInformation($"Clears the library cache. Does not affect the browser cookies.");
            logger.LogDebug($"while (accounts.Any()) {accounts?.Count()}");

            while (accounts.Any())
            {
                var account = accounts.First();
                await publicClientApp.RemoveAsync(account); logger.LogDebug($"await _app.RemoveAsync({account});");
                accounts = await publicClientApp.GetAccountsAsync(); logger.LogDebug($"await publicClientApp.GetAccountsAsync(); returned {accounts}");
                this.Identity = null;
                //    this.TenantId = null;
            }
        }

        public async Task<Identity> LoginSilentAsync()
        {
            //using var scope = logger.BeginMethodScope();

            var accounts = await publicClientApp.GetAccountsAsync();
            logger.LogDebug($"await publicClientApp.GetAccountsAsync(); returned {accounts}");

            if (accounts.Any())
            {
                try
                {
                    var result = await publicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true);
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
                    var identity = result != null && result.Account != null ? new Identity(upn, email, name ?? result.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                    return identity;
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

            var accounts = await publicClientApp.GetAccountsAsync();
            logger.LogDebug($"(await publicClientApp.GetAccountsAsync()).ToList(); returned {accounts}");

            try
            {
                var result = await publicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true);
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
                var identity = result != null && result.Account != null ? new Identity(upn, email, name ?? result.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                return identity;
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                    // and we don't necessarily want to re-sign-in the same user
                    var builder = publicClientApp.AcquireTokenInteractive(scopes).WithAccount(accounts.FirstOrDefault()).WithPrompt(Prompt.SelectAccount);
                    logger.LogDebug($"_app.AcquireTokenInteractive({scopes}).WithAccount({accounts.FirstOrDefault()}).WithPrompt({Prompt.SelectAccount}); returned {builder}");

                    var isEmbeddedWebViewAvailable = publicClientApp.IsEmbeddedWebViewAvailable(); logger.LogDebug($"_app.IsEmbeddedWebViewAvailable() returned {isEmbeddedWebViewAvailable}");
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
                    var identity = account != null ? new Identity(upn, email, name ?? result.Account.Username) : null; // , email ?? upn, manager, IsDeclassifier
                    return identity;
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
}
