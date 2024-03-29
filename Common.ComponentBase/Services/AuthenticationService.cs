﻿#region using
using System;
using System.Configuration;
//using Microsoft.InformationProtection;
using Common;
using Microsoft.Identity.Client;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Windows.Interop;
using Microsoft.Identity.Client.Extensibility;
using Microsoft.Extensions.Logging;
using Common.Abstractions;
using MessageBox = System.Windows.MessageBox;
//using static System.Formats.Asn1.AsnWriter;
//using Microsoft.Identity.Client.Broker;
#endregion

namespace Common
{
    public class AuthenticationService : IAuthenticationService, ISupportLogString
    {
        #region constants
        private static readonly Type T = typeof(AuthenticationService);
        const string CONFIGVALUE_TENANTID = "TenantId"; const string DEFAULTVALUE_TENANTID = "common";
        const string CONFIGVALUE_CLIENTID = "ClientId"; const string DEFAULTVALUE_CLIENTID = "";
        const string CONFIGVALUE_APPNAME = "AppName"; const string DEFAULTVALUE_APPNAME = "";
        const string CONFIGVALUE_APPVERSION = "AppVersion"; const string DEFAULTVALUE_APPVERSION = "";
        const string CONFIGVALUE_REDIRECTURI = "RedirectUri"; const string DEFAULTVALUE_REDIRECTURI = "";
        const string CONFIGVALUE_SCOPES = "Scopes"; const string DEFAULTVALUE_SCOPES = "";
        const string CONFIGVALUE_OAUTHVERSIONSUFFIX = "OauthVersionSuffix"; const string DEFAULTVALUE_OAUTHVERSIONSUFFIX = "/2.0";
        //const string CONFIGVALUE_TENANTID = "TenantId"; const string DEFAULTVALUE_TENANTID = "common";
        //const string CONFIGVALUE_REDIRECTURI = "RedirectUri"; const string DEFAULTVALUE_REDIRECTURI = "";
        const string CONFIGVALUE_DATADECLASSGROUPNAME = "GFSCL_DATADECLASS_USERS";
        #endregion

        #region internal state
        private ILogger<AuthenticationService> logger;
        // ADAL: Set the redirect URI from the AAD Application Registration.
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
        string[] scopes = new string[] { "openid", "user.read", "profile", "https://vault.azure.net/.default" }; // "user.read.all", "openid", "user.read", "user.read.all", "profile" "https://aadrm.com/user_impersonation", "email", "groupmember.read.all", "group.read.all", "https://aadrm.com/user_impersonation", "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" 
        string[] scopesAADRM = new string[] { "https://aadrm.com/user_impersonation" }; // ,, "https://aadrm.com/user_impersonation" "https://vault.azure.net/.default"  , "https://vault.azure.net/.default", "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" 
        string[] scopesAKV = new string[] { "https://vault.azure.net/user_impersonation" }; // ,, "https://aadrm.com/user_impersonation" "https://vault.azure.net/.default"  , "https://vault.azure.net/.default", "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" 
        string[] scopesO365syncservice = new string[] { "https://psor.o365syncservice.com/UnifiedPolicy.User.Read" };
        #endregion

        #region .ctor
        public AuthenticationService()
        {
            using var scope = logger.BeginMethodScope();

            this.tenantId = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID);
            this.applicationId = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID);
            this.appName = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_APPNAME, DEFAULTVALUE_APPNAME);
            this.appVersion = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_APPVERSION, DEFAULTVALUE_APPVERSION);
            this.redirectUri = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_REDIRECTURI, DEFAULTVALUE_REDIRECTURI);
            var scopesString = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_SCOPES, DEFAULTVALUE_SCOPES);
            this.scopes = scopesString?.Split(',');
            var oauthVersionSuffix = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_OAUTHVERSIONSUFFIX, DEFAULTVALUE_OAUTHVERSIONSUFFIX); // , CultureInfo.InvariantCulture


            //this.window = window;
            var builder = PublicClientApplicationBuilder.Create(applicationId)
                                .WithAuthority($"https://login.microsoftonline.com/{tenantId}{oauthVersionSuffix}", true) //$"{Instance}{tenantId}"
                                .WithDefaultRedirectUri();

            publicClientApp = builder.Build(); scope.LogDebug($@"_clientApp = PublicClientApplicationBuilder.Create({applicationId}).WithAuthority($'{Instance}{tenantId}', true).WithRedirectUri({this.redirectUri}).Build();");

            TokenCacheHelper.EnableSerialization(publicClientApp.UserTokenCache); scope.LogDebug($"TokenCacheHelper.EnableSerialization({publicClientApp.UserTokenCache}); completed");
        }
        public AuthenticationService(string tenantId, string applicationId, string appName, string appVersion, string redirectUri, string[] scopes, string oauthVersion, Window window) // ApplicationInfo appInfo, 
        {
            using var scope = logger.BeginMethodScope(new { tenantId, applicationId, appName, appVersion, redirectUri, scopes, oauthVersion, window }); // new { appInfo = appInfo.GetLogString() }

            this.tenantId = tenantId;
            if (string.IsNullOrEmpty(this.tenantId)) { this.tenantId = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID); } // , CultureInfo.InvariantCulture
            this.applicationId = applicationId;
            if (string.IsNullOrEmpty(this.applicationId)) { this.applicationId = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID); } // , CultureInfo.InvariantCulture
            this.appName = appName;
            if (string.IsNullOrEmpty(this.appName)) { this.appName = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_APPNAME, DEFAULTVALUE_APPNAME); } // , CultureInfo.InvariantCulture
            this.appVersion = appVersion;
            if (string.IsNullOrEmpty(this.appVersion)) { this.appVersion = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_APPVERSION, DEFAULTVALUE_APPVERSION); } // , CultureInfo.InvariantCulture
            this.redirectUri = redirectUri;
            if (string.IsNullOrEmpty(this.redirectUri)) { this.redirectUri = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_REDIRECTURI, DEFAULTVALUE_REDIRECTURI); } // , CultureInfo.InvariantCulture
            this.scopes = scopes;
            if (this.scopes == null)
            {
                var scopesString = ConfigurationHelper.GetClassSetting<AuthenticationService, string>(CONFIGVALUE_SCOPES, DEFAULTVALUE_SCOPES);
                this.scopes = scopesString?.Split(',');
                scope.LogDebug(() => new { this.scopes });
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
            publicClientApp = builder.Build(); scope.LogDebug($@"_clientApp = PublicClientApplicationBuilder.Create(applicationId).WithAuthority($'{Instance}{tenantId}', true).WithRedirectUri({this.redirectUri}).Build();");

            //.WithAuthority(AzureCloudInstance.AzurePublic, "common") //$"{Instance}{tenantId}"
            TokenCacheHelper.EnableSerialization(publicClientApp.UserTokenCache); scope.LogDebug($"TokenCacheHelper.EnableSerialization({publicClientApp.UserTokenCache}); completed");
        }
        #endregion

        public AuthenticationResult AuthenticationResult { get => authenticationResult; set => authenticationResult = value; }
        public Identity Identity { get => identity; set => identity = value; }
        public string TenantId { get => tenantId; set => tenantId = value; }
        public string ApplicationId { get => applicationId; set => applicationId = value; }
        public IPublicClientApplication App { get => publicClientApp; set => publicClientApp = value; }

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
            using var scope = logger.BeginMethodScope(new { identity, authority, resource, claims });

            var ret = "";
            ret = Task.Run(async () => { return await AcquireTokenAsync(identity, authority, resource, claims); }).GetAwaiter().GetResult();
            return ret;
        }
        #endregion
        #region AcquireTokenAsync
        public async Task<string> AcquireTokenAsync(Identity identity, string authority, string resource, string claims)
        {
            using var scope = logger.BeginMethodScope(new { identity, authority, resource, claims });

            try
            {
                var authResult = default(Microsoft.Identity.Client.AuthenticationResult);
                var app = publicClientApp;

                var accounts = await publicClientApp.GetAccountsAsync(); scope.LogDebug($"await app.GetAccountsAsync(); returned {accounts.GetLogString()}");
                var firstAccount = accounts.FirstOrDefault();
                scope.LogDebug(new { firstAccount });

                try
                {
                    var scopes = this.scopes;
                    if (resource.StartsWith("https://aadrm.com", StringComparison.InvariantCultureIgnoreCase)) { scopes = scopesAADRM; }
                    if (resource.StartsWith("https://syncservice.o365syncservice.com/", StringComparison.InvariantCultureIgnoreCase)) { scopes = scopesO365syncservice; }

                    authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync(); scope.LogDebug($"await app.AcquireTokenSilent({scopes.GetLogString()}, firstAccount).ExecuteAsync(); returned {authResult.GetLogString()}");
                }
                catch (MsalUiRequiredException ex)
                {
                    // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
                    scope.LogDebug($"MsalUiRequiredException: {ex.Message}");

                    try
                    {
                        if (window != null && false)
                        {
                            IntPtr handle = new WindowInteropHelper(window).Handle;

                            authResult = await app.AcquireTokenInteractive(scopes).WithAccount(firstAccount)
                                                  .WithParentActivityOrWindow(handle)
                                                  .WithPrompt(Prompt.SelectAccount).ExecuteAsync();
                        }
                        else
                        {
                            //var handle = new WindowInteropHelper(_window).Handle;
                            authResult = await app.AcquireTokenInteractive(scopes).WithAccount(firstAccount)
                                                  .WithPrompt(Prompt.SelectAccount).ExecuteAsync();
                        }
                        scope.LogDebug($"await app.AcquireTokenInteractive({scopes.GetLogString()}).WithAccount(firstAccount).WithParentActivityOrWindow(new WindowInteropHelper(this).Handle).WithPrompt({Prompt.SelectAccount.GetLogString()}).ExecuteAsync(); returned {authResult.GetLogString()}");
                    }
                    catch (MsalException msalex)
                    {
                        scope.LogException(msalex);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    scope.LogException(ex);
                    throw;
                }

                // Return the token. The token is sent to the resource.
                scope.Result = authResult.AccessToken;
                return authResult.AccessToken;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public async Task<Identity> GetUserIdentitySylentAsync(Action<Identity> onComplete) // , PromptBehavior promptBehavior = PromptBehavior.Auto
        {
            using (var scope = logger.BeginMethodScope(new { onComplete })) // , promptBehavior
            {
                try
                {
                    Identity identity = null;

                    var authResult = default(Microsoft.Identity.Client.AuthenticationResult);
                    var app = publicClientApp;

                    var accounts = await publicClientApp.GetAccountsAsync(); scope.LogDebug($"await app.GetAccountsAsync(); returned {accounts.GetLogString()}");
                    var firstAccount = accounts.FirstOrDefault();
                    scope.LogDebug(new { firstAccount });

                    var scopes = this.scopes;
                    try
                    {
                        authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync(); scope.LogDebug($"await app.AcquireTokenSilent({scopes.GetLogString()}, firstAccount).ExecuteAsync(); returned {authResult.GetLogString()}");

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
                        scope.LogException(ex);
                        scope.LogInformation($"MsalUiRequiredException: {ex.Message}");
                        // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
                    }
                    catch (Exception ex)
                    {
                        this.Identity = null;
                        this.TenantId = null;
                        scope.LogException(ex);
                        throw;
                    }

                    scope.Result = identity;
                    return identity;
                }
                catch (Exception ex)
                {
                    this.Identity = null;
                    this.TenantId = null;
                    scope.LogException(ex);
                }
                finally
                {
                    if (onComplete != null) { onComplete(this.Identity); }
                }
                return null;
            }
        }
        public async Task<Identity> GetUserIdentityAsync(Action<Identity> onComplete) // , PromptBehavior promptBehavior = PromptBehavior.Auto
        {
            using (var scope = logger.BeginMethodScope(new { onComplete })) // , promptBehavior
            {
                try
                {
                    Identity identity = null;
                    //Identity manager = null;

                    var authResult = default(Microsoft.Identity.Client.AuthenticationResult);
                    var app = publicClientApp;

                    var accounts = await publicClientApp.GetAccountsAsync(); scope.LogDebug($"await app.GetAccountsAsync(); returned {accounts.GetLogString()}");
                    var firstAccount = accounts.FirstOrDefault();
                    scope.LogDebug(new { firstAccount });

                    var scopes = this.scopes;
                    if (true)
                    {
                        try
                        {
                            authResult = await app.AcquireTokenSilent(scopes, firstAccount).ExecuteAsync(); scope.LogDebug($"await app.AcquireTokenSilent({scopes.GetLogString()}, firstAccount).ExecuteAsync(); returned {authResult.GetLogString()}");

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
                            scope.LogException(ex);
                            // A MsalUiRequiredException happened on AcquireTokenSilent. This indicates you need to call AcquireTokenInteractive to acquire a token
                            scope.LogInformation($"MsalUiRequiredException: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            this.Identity = null;
                            this.AuthenticationResult = null;
                            this.TenantId = null;
                            scope.LogException(ex);
                            throw;
                        }
                    }

                    if (string.IsNullOrEmpty(authResult?.AccessToken)) // && promptBehavior != PromptBehavior.Auto
                    {
                        try
                        {
                            await Task.Run(async () =>
                            {
                                using (var sec1 = TraceLogger.BeginMethodScope(T, "AcquireTokenInteractive"))
                                {   // WithParentActivityOrWindow(handle).
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
                                }
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
                            scope.LogException(msalex);
                            throw;
                        }
                    }

                    scope.Result = identity;
                    return identity;
                }
                catch (Exception ex)
                {
                    this.Identity = null;
                    this.AuthenticationResult = null;
                    this.TenantId = null;
                    scope.LogException(ex);
                }
                finally
                {
                    if (onComplete != null) { onComplete(this.Identity); }
                }
                return this.Identity;
            }
        }

        public async Task Logout()
        {
            using var scope = logger.BeginMethodScope();

            Task.Run(async () => { await LogoutAsync(); }).GetAwaiter().GetResult();
            this.Identity = null;
            this.TenantId = null;
        }
        public async Task LogoutAsync()
        {
            using var scope = logger.BeginMethodScope();

            var accounts = await publicClientApp.GetAccountsAsync(); scope.LogDebug($"await publicClientApp.GetAccountsAsync(); return {accounts.GetLogString()}");
            scope.LogInformation($"Clears the library cache. Does not affect the browser cookies.");
            scope.LogDebug($"while (accounts.Any()) {accounts?.Count()}");

            while (accounts.Any())
            {
                var account = accounts.First();
                await publicClientApp.RemoveAsync(account); scope.LogDebug($"await _app.RemoveAsync({account.GetLogString()});");
                accounts = await publicClientApp.GetAccountsAsync(); scope.LogDebug($"await publicClientApp.GetAccountsAsync(); returned {accounts.GetLogString()}");
                this.Identity = null;
                //    this.TenantId = null;
            }
        }

        public async Task<Identity> LoginSilentAsync()
        {
            using var scope = logger.BeginMethodScope();

            var accounts = await publicClientApp.GetAccountsAsync();
            scope.LogDebug($"await publicClientApp.GetAccountsAsync(); returned {accounts.GetLogString()}");

            if (accounts.Any())
            {
                try
                {
                    var result = await publicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true);
                    scope.LogDebug($"await _app.AcquireTokenSilent({scopes.GetLogString()}, {accounts.FirstOrDefault().GetLogString()}).ExecuteAsync().ConfigureAwait(true); returned {result.GetLogString()}");

                    this.authenticationResult = result;
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

                    scope.Result = identity;
                    return identity;
                }
                catch (MsalUiRequiredException ex)
                {
                    scope.LogException(ex);
                    throw;
                }
            }

            return null;
        }
        public async Task<Identity> LoginAsync()
        {
            using var scope = logger.BeginMethodScope();

            var accounts = await publicClientApp.GetAccountsAsync();
            scope.LogDebug(new { accounts });
            scope.LogDebug($"(await publicClientApp.GetAccountsAsync()).ToList(); returned {accounts.GetLogString()}");

            try
            {
                //var scopeTemp = new List<string>(scopes);
                //scopeTemp.AddRange(scopesAADRM);
                //scopes = scopeTemp.ToArray();

                var result = await publicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync().ConfigureAwait(true);
                scope.LogDebug($"await _app.AcquireTokenSilent({scopes.GetLogString()}, {accounts.FirstOrDefault().GetLogString()}).ExecuteAsync().ConfigureAwait(true); returned {result.GetLogString()}");

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

                scope.Result = identity;
                return identity;
            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    // Force a sign-in (Prompt.SelectAccount), as the MSAL web browser might contain cookies for the current user
                    // and we don't necessarily want to re-sign-in the same user
                    var scopes = new List<string> { "openid", "user.read", "profile" }; // , "https://vault.azure.net/.default"
                    var builder = publicClientApp.AcquireTokenInteractive(scopes)
                                                 //.WithExtraScopesToConsent(new[] { "https://vault.azure.net/.default" }) // https://vault.azure.net/user_impersonation"
                                                 .WithAccount(accounts.FirstOrDefault()).WithPrompt(Prompt.SelectAccount);
                    scope.LogDebug($"_app.AcquireTokenInteractive({scopes.GetLogString()}).WithAccount({accounts.FirstOrDefault().GetLogString()}).WithPrompt({Prompt.SelectAccount}); returned {builder.GetLogString()}");

                    var isEmbeddedWebViewAvailable = publicClientApp.IsEmbeddedWebViewAvailable(); scope.LogDebug($"_app.IsEmbeddedWebViewAvailable() returned {isEmbeddedWebViewAvailable}");
                    if (!isEmbeddedWebViewAvailable)
                    {
                        // You app should install the embedded browser WebView2 https://aka.ms/msal-net-webview2
                        // but if for some reason this is not possible, you can fall back to the system browser 
                        // in this case, the redirect uri needs to be set to "http://localhost"
                        builder = builder.WithUseEmbeddedWebView(false); logger.LogDebug($"builder.WithUseEmbeddedWebView(false); returned {builder.GetLogString()}");
                    }

                    scope.LogDebug($"calling await builder.ExecuteAsync().ConfigureAwait(true);");
                    var result = await builder.ExecuteAsync().ConfigureAwait(true);
                    scope.LogDebug($"await builder.ExecuteAsync().ConfigureAwait(true); returned {result.GetLogString()}");
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

                    this.AuthenticationResult = result;
                    scope.Result = identity;
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

                        MessageBox.Show(message);
                    }

                    //Dispatcher.Invoke(() =>
                    //{
                    //    // UserName.Content = Properties.Resources.UserNotSignedIn;
                    //});
                }
            }

            return null;
        }

        public string ToLogString()
        {
            var logString = $"{{{nameof(AuthenticationService)}:{{Identity:{this.Identity.GetLogString()},ApplicationId:{this.ApplicationId},TenantId:{this.TenantId},AuthenticationResult:{this.AuthenticationResult.GetLogString()} }}}}";
            return logString;
        }
    }
}
