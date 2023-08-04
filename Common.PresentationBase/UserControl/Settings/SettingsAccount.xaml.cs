#region using
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Resources;
using Common.Properties;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using System.IdentityModel;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using Path = System.IO.Path;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Graph.Applications.GetByIds;
using Microsoft.Graph.Models;
using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Kiota.Abstractions;
using System.Windows.Media.TextFormatting;
//using Azure.Management.Resources;
//using Azure.Management.Resources.Models;
#endregion

namespace Common
{

    /// <summary>Interaction logic for SettingsDownloadControl.xaml</summary>
    public partial class SettingsAccountControl : UserControl
    {
        #region const
        const string S_CONNECTIONSTRING_DEFAULT = @"";
        const string S_BLOBSTORAGECONNECTIONSTRING_DEFAULT = "";
        const string S_CHECKPOINTSCONTAINER_DEFAULT = "";
        const string S_EVENTHUBNAME_DEFAULT = "";
        const string CONFIGVALUE_KEYVAULTADDRESS = "KeyVaultAddress", DEFAULTVALUE_KEYVAULTADDRESS = "";
        const string CONFIGVALUE_CLIENTSECRET = "ClientSecret", DEFAULTVALUE_CLIENTSECRET = "";
        const string CONFIGVALUE_APPINSIGHTSKEY = "AppInsightsKey", DEFAULTVALUE_APPINSIGHTSKEY = "";
        const string CONFIGVALUE_TENANTID = "TenantId"; const string DEFAULTVALUE_TENANTID = "";
        const string CONFIGVALUE_CLIENTID = "ClientId"; const string DEFAULTVALUE_CLIENTID = "";
        const string CONFIGVALUE_APPNAME = "AppName"; const string DEFAULTVALUE_APPNAME = "";
        const string CONFIGVALUE_APPVERSION = "AppVersion"; const string DEFAULTVALUE_APPVERSION = "";
        const string CONFIGVALUE_REDIRECTURI = "RedirectUri"; const string DEFAULTVALUE_REDIRECTURI = "";
        const string CONFIGVALUE_SCOPES = "Scopes"; const string DEFAULTVALUE_SCOPES = "";
        const string CONFIGVALUE_OAUTHVERSIONSUFFIX = "OauthVersionSuffix"; const string DEFAULTVALUE_OAUTHVERSIONSUFFIX = "/2.0";
        #endregion
        private static readonly Type T = typeof(SettingsAccountControl);
        private IConfiguration configuration;
        private ILogger<SettingsAccountControl> logger;
        private AuthenticationHelper authenticationHelper;

        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), T, new PropertyMetadata(false));
        #endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), T, new PropertyMetadata());
        #endregion
        #region Identity
        public Identity Identity
        {
            get { return (Identity)GetValue(IdentityProperty); }
            set { SetValue(IdentityProperty, value); }
        }
        public static readonly DependencyProperty IdentityProperty = DependencyProperty.Register("Identity", typeof(Identity), T, new PropertyMetadata());
        #endregion

        #region TenantId
        public string TenantId
        {
            get { return (string)GetValue(TenantIdProperty); }
            set { SetValue(TenantIdProperty, value); }
        }
        public static readonly DependencyProperty TenantIdProperty = DependencyProperty.Register("TenantId", typeof(string), T, new PropertyMetadata());
        #endregion
        #region ClientId
        public string ClientId
        {
            get { return (string)GetValue(ClientIdProperty); }
            set { SetValue(ClientIdProperty, value); }
        }
        public static readonly DependencyProperty ClientIdProperty = DependencyProperty.Register("ClientId", typeof(string), T, new PropertyMetadata());
        #endregion
        #region ClientSecret
        public string ClientSecret
        {
            get { return (string)GetValue(ClientSecretProperty); }
            set { SetValue(ClientSecretProperty, value); }
        }
        public static readonly DependencyProperty ClientSecretProperty = DependencyProperty.Register("ClientSecret", typeof(string), T, new PropertyMetadata());
        #endregion
        #region UseManagedIdentity
        public bool UseManagedIdentity
        {
            get { return (bool)GetValue(UseManagedIdentityProperty); }
            set { SetValue(UseManagedIdentityProperty, value); }
        }
        public static readonly DependencyProperty UseManagedIdentityProperty = DependencyProperty.Register("UseManagedIdentity", typeof(bool), T, new PropertyMetadata());
        #endregion

        #region Configuration
        public TenantConfiguration Configuration
        {
            get { return (TenantConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }
        public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.Register("Configuration", typeof(TenantConfiguration), T, new PropertyMetadata());
        #endregion
        #region Configurations
        public IList<TenantConfiguration> Configurations
        {
            get { return (IList<TenantConfiguration>)GetValue(ConfigurationsProperty); }
            set { SetValue(ConfigurationsProperty, value); }
        }
        public static readonly DependencyProperty ConfigurationsProperty = DependencyProperty.Register("Configurations", typeof(IList<TenantConfiguration>), T, new PropertyMetadata());
        #endregion
        #region Tenants
        public IList<TenantData> Tenants
        {
            get { return (IList<TenantData>)GetValue(TenantsProperty); }
            set { SetValue(TenantsProperty, value); }
        }
        public static readonly DependencyProperty TenantsProperty = DependencyProperty.Register("Tenants", typeof(IList<TenantData>), T, new PropertyMetadata());
        #endregion
        #region Applications
        public IList<Microsoft.Graph.Models.Application> Applications
        {
            get { return (IList<Microsoft.Graph.Models.Application>)GetValue(ApplicationsProperty); }
            set { SetValue(ApplicationsProperty, value); }
        }
        public static readonly DependencyProperty ApplicationsProperty = DependencyProperty.Register("Applications", typeof(IList<Microsoft.Graph.Models.Application>), T, new PropertyMetadata());
        #endregion
        #region Organizations
        public IList<Microsoft.Graph.Models.Organization> Organizations
        {
            get { return (IList<Microsoft.Graph.Models.Organization>)GetValue(OrganizationsProperty); }
            set { SetValue(OrganizationsProperty, value); }
        }
        public static readonly DependencyProperty OrganizationsProperty = DependencyProperty.Register("Organizations", typeof(IList<Microsoft.Graph.Models.Organization>), T, new PropertyMetadata());
        #endregion

        #region App
        public ApplicationBase App
        {
            get { return (ApplicationBase)GetValue(AppProperty); }
            set { SetValue(AppProperty, value); }
        }
        public static readonly DependencyProperty AppProperty = DependencyProperty.Register("App", typeof(ApplicationBase), T, new PropertyMetadata(null));
        #endregion

        #region .ctor
        static SettingsAccountControl()
        {
        }
        public SettingsAccountControl()
        {
            using var scope = logger.BeginMethodScope();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            var app = ApplicationBase.Current as ApplicationBase;
            this.App = app;

            this.configuration = app.Host.Services.GetService<IConfiguration>();
            this.logger = app.Host.Services.GetService<ILogger<SettingsAccountControl>>();
            this.authenticationHelper = this.App.Host.Services.GetService<AuthenticationHelper>();
            scope.LogDebug(new { authenticationHelper = authenticationHelper.GetLogString() });

            InitializeComponent();
        }

        #endregion
        private async void ctlSettingsAccountControl_Initialized(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            using var scope = logger.BeginMethodScope(new { sender, e });

            //this.ExceptionEvent += SettingsAccountControl_ExceptionEvent;
            try
            {
                var keyVaultAddress = default(string);
                var identity = await authenticationHelper.LoginSilentAsync();
                this.Dispatcher.Invoke(() =>
                {
                    this.Identity = identity;
                    this.App.SetProperty("Identity", identity);
                    this.TenantId = ApplicationBase.Current.Properties["TenantId"] as string;
                    this.ClientId = ApplicationBase.Current.Properties["ClientId"] as string;
                    this.ClientSecret = ApplicationBase.Current.Properties["ClientSecret"] as string;
                    keyVaultAddress = ApplicationBase.Current.Properties["KeyVaultAddress"] as string;
                    scope.LogDebug(new { this.Identity });
                });

                TokenCredential credential = null;
                if (!string.IsNullOrEmpty(this.ClientSecret)) { credential = new ClientSecretCredential(this.TenantId, this.ClientId, this.ClientSecret); }
                if (identity != null && credential == null)
                {
                    var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                    credential = new DefaultAzureCredential(credentialOptions);
                }

                if (credential != null)
                {
                    var profileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    var currentProcessFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    var executableName = Path.GetFileNameWithoutExtension(currentProcessFileName);
                    var configurationFilePath = Path.Combine(profileFolder, executableName, "Configuration");
                    scope.LogDebug(new { configurationFilePath });

                    var configurations = new List<TenantConfiguration>();
                    string[] fileNames = Directory.GetFiles(configurationFilePath); scope.LogDebug($"Directory.GetFiles(configurationFilePath); returned {fileNames.GetLogString()}");
                    foreach (string fileName in fileNames)
                    {
                        try
                        {
                            scope.LogDebug(new { fileName });
                            var json = File.ReadAllText(fileName);
                            var tenantConfiguration = JsonConvert.DeserializeObject<TenantConfiguration>(json);

                            configurations.Add(tenantConfiguration);
                        }
                        catch (Exception ex) { }
                    }
                    this.Configurations = configurations;
                    scope.LogDebug(new { configurations });
                    if (configurations?.Count == 1) { this.Configuration = configurations.FirstOrDefault(); }

                    var tenantId = authenticationHelper.TenantId;
                    var clientId = authenticationHelper.ApplicationId;

                    //var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://management.azure.com/.default" }), new CancellationToken());
                    //var client = new HttpClient();
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                    //// Call the REST API to list the tenants
                    //var response = await client.GetAsync("https://management.azure.com/tenants?api-version=2020-01-01");
                    //// Print the response content
                    //var content = await response.Content.ReadAsStringAsync();

                    var tenants = new List<TenantData>();
                    var armClient = new ArmClient(credential);
                    var tenantObjects = armClient.GetTenants();
                    foreach (var tenantObj in tenantObjects)
                    {
                        var tenantItem = tenantObj.Data;
                        scope.LogDebug($"Id:{tenantItem.Id},TenantId:{tenantItem.TenantId},DisplayName:{tenantItem.DisplayName},DefaultDomain:{tenantItem.DefaultDomain},TenantCategory:{tenantItem.TenantCategory},TenantType:{tenantItem.TenantType},Country:{tenantItem.Country},CountryCode:{tenantItem.CountryCode}");
                        tenants.Add(tenantItem);
                    }
                    this.Tenants = tenants;

                    var tenant = this.Tenants.FirstOrDefault();
                    tenantId = tenant.TenantId?.ToString();
                    
                    var client = new GraphServiceClient(credential);
                    //var user = await client.Users["me"].GetAsync(); 
                    //var accountId = user.Id;
                    var me = await client.Me.GetAsync();
                    var accountId = me.Id;

                    var ownedObjects =  client.Me.OwnedObjects.GetAsync();
                    //var onenote = await client.Me.Onenote.GetAsync();
                    
                    var pageSize = 30;
                    var applications = new List<Microsoft.Graph.Models.Application>();
                    string responseAppsContent = null;
                    try
                    {
                        //var requestUri = $"https://graph.microsoft.com/beta/applications?$top={pageSize}&$filter=owners/$count eq 1&$count=true";
                        var requestUri = $"https://graph.microsoft.com/v1.0/me/ownedObjects/microsoft.graph.application?$top=10&$count=true";
                        // /$count ne 0
                        // https://graph.microsoft.com/v1.0/me/ownedObjects/microsoft.graph.application?$top=10&$count=true
                        //$expand=owners& &$select=appId,identifierUris,displayName,publisherDomain,signInAudience,owners $filter=owners/any(x:x/id eq '{accountId}') $select=appId,identifierUris,displayName,publisherDomain,signInAudience,owners
                        //GET https://graph.microsoft.com/v1.0/applications?$filter=owners/$count eq 0 or owners/$count eq 1&$count=true&$select=id,displayName
                        //ConsistencyLevel: eventual
                        for (var page = 0; ; page++)
                        {
                            if (page > 10) { scope.LogDebug($"Page limit reached: {page-1}"); break; }
                            if (page > 0 && string.IsNullOrEmpty(requestUri)) { scope.LogDebug($"Enumeration completed: {page-1}"); break; }
                            var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }), new CancellationToken());
                            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                            {
                                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                                request.Headers.Add("ConsistencyLevel", "eventual");

                                using (var clientApps = new HttpClient())
                                using (var responseApps = await clientApps.SendAsync(request))
                                {
                                    responseApps.EnsureSuccessStatusCode();

                                    responseAppsContent = await responseApps.Content.ReadAsStringAsync();
                                    JObject responseAppsOjbect = JObject.Parse(responseAppsContent);
                                    var nextLink = responseAppsOjbect["@odata.nextLink"]?.Value<string>();
                                    requestUri = nextLink;
                                    scope.LogDebug(new { page, nextLink });

                                    var applicationList = responseAppsOjbect["value"];
                                    foreach (var item in applicationList)
                                    {
                                        var id = item["id"]?.Value<string>();
                                        var appId = item["appId"]?.Value<string>();
                                        var displayName = item["displayName"]?.Value<string>();
                                        var description = item["description"]?.Value<string>();
                                        var createdDateTime = item["createdDateTime"]?.Value<DateTime?>();
                                        var signInAudience = item["signInAudience"]?.Value<string>();
                                        var identifierUris = item["identifierUris"]?.Select(uri=> uri?.Value<string>()).ToList();
                                        var publisherDomain = item["publisherDomain"]?.Value<string>();
                                        //var owners = item["owners"];
                                        //appId,identifierUris,displayName,publisherDomain,signInAudience

                                        var application = new Microsoft.Graph.Models.Application()
                                        {
                                            Id = id,
                                            AppId = appId,
                                            DisplayName = displayName,
                                            Description = description,
                                            CreatedDateTime = createdDateTime,
                                            SignInAudience = signInAudience,
                                            PublisherDomain = publisherDomain,
                                            IdentifierUris = identifierUris
                                        };
                                        scope.LogDebug(new { application = application.GetLogString() });
                                        applications.Add(application);
                                    }
                                }
                            }
                        }

                        ApplicationCollectionResponse response;
                        //for (var page = 0; ; page++)
                        //{
                        //    var appRegistrations = response = await client.Applications.GetAsync(requestConfiguration =>
                        //    {
                        //        requestConfiguration.QueryParameters.Count = true;
                        //        requestConfiguration.QueryParameters.Top = pageSize;
                        //        if (page > 0) { requestConfiguration.QueryParameters.Skip = page * pageSize; }
                        //        requestConfiguration.QueryParameters.Select = new string[] { "AppId", "Id", "DisplayName", "Owners" };
                        //        //requestConfiguration.QueryParameters.Filter = $"appOwnerOrganizationId eq '{tenantId}'";  // Filter = "Owners/any()"; "Owners ne null"
                        //        // requestConfiguration.Headers.Add("Prefer", "outlook.body-content-type=\"text\""); 		+		Owners	null	System.Collections.Generic.List<Microsoft.Graph.Models.DirectoryObject>
                        //    }); 

                        //    var applicationsPage = appRegistrations.Value;
                        //    applications.AddRange(applicationsPage);
                        //}
                    }
                    catch (Exception ex) { scope.LogException(ex); }

                    this.Applications = applications;



                    ////// get the list of owned appregistrations 
                    //var graphClient = new GraphServiceClient(credential);

                    ////// Get the tenants
                    ////// var tenants = await graphClient.TenantRelationships.GetAsync();
                    ////   var tenants = await graphClient.Organization.GetAsync();
                    ////   this.Organizations = tenants.Value;
                    //var appRegistrations = await graphClient.Applications.GetAsync(requestConfiguration =>
                    //{
                    //    requestConfiguration.QueryParameters.Top = 10;
                    //    requestConfiguration.QueryParameters.Select = new string[] { "AppId", "Id", "DisplayName", "Owners" };
                    //    // requestConfiguration.QueryParameters.Filter = "Owners/$count ge 1";  // Filter = "Owners/any()"; "Owners ne null"
                    //    // requestConfiguration.Headers.Add("Prefer", "outlook.body-content-type=\"text\""); 		+		Owners	null	System.Collections.Generic.List<Microsoft.Graph.Models.DirectoryObject>
                    //});
                    //var applications = appRegistrations.Value;
                    //this.Applications = applications;


                    //scope.LogDebug($"configurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());");

                }
            }
            catch (Exception ex)
            {
                //var message = this.GetResourceValue<string>("Info.ExceptionLoadingSecrets", "Cannot load secrets from Key Vault");
                //var ex1 = new ClientException(message, ex) { Code = ExceptionCodes.PRESSLOGIN };
                //ExceptionManager.RaiseException(this, ex1);
                //CommandManager.InvalidateRequerySuggested();
                ExceptionManager.RaiseException(this, ex);
                scope.LogException(ex);
                //CommandManager.InvalidateRequerySuggested();
                //RegreshCount += 1;
            }
        }

        static async Task<string> GetAccessTokenAsync(string tenantId, string clientId)
        {
            // Create an HTTP client for Azure AD
            var client = new HttpClient();

            // Set the request parameters
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{tenantId}/oauth2/token");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("resource", "https://management.core.windows.net/")
            });

            // Send the request and get the response
            var response = await client.SendAsync(request);

            // Parse the response content as JSON
            var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            // Return the access token
            return json.RootElement.GetProperty("access_token").GetString();
        }

        // Commands
        void ToggleIsCollapsedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        private void ToggleIsCollapsedCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();
            IsCollapsed = !IsCollapsed;
        }
        private void LoginToggleCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void LoginToggleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();
            if (Identity == null) { Commands.Login.Execute(null, this); }
            else { Commands.Logout.Execute(null, this); }
        }
        private void LoginCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void LoginExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            scope.LogDebug(new { authenticationHelper = authenticationHelper.GetLogString() });
            if (authenticationHelper == null)
            {
                var message = this.GetResourceValue<string>("Info.AuthHelperNotAvailable", "Authentication helper is not available");
                var ex = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, ex);
                //CommandManager.InvalidateRequerySuggested();
                return;
            }

            await authenticationHelper.LogoutAsync();
            var identity = await authenticationHelper.LoginAsync();
            this.Identity = identity;
            this.App.SetProperty("Identity", identity);

            //TokenCredential credential = null;
            //var clientSecret = ConfigurationHelper.GetClassSetting<SettingsAccountControl, string>(CONFIGVALUE_CLIENTSECRET, DEFAULTVALUE_CLIENTSECRET);
            //if (!string.IsNullOrEmpty(clientSecret)) { credential = new ClientSecretCredential(this.TenantId, this.ClientId, this.ClientSecret); }
            //if (identity == null && credential == null)
            //{
            //    var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
            //    var ex = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
            //    ExceptionManager.RaiseException(this, ex);
            //    return;
            //}
            //if (credential == null)
            //{
            //    var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
            //    credential = new DefaultAzureCredential(credentialOptions);
            //}
        }
        private void LogoutCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void LogoutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            if (authenticationHelper == null) { return; }
            await authenticationHelper.LogoutAsync(); scope.LogDebug($"await authenticationHelper.LogoutAsync(); completed");
            this.Identity = null;
            this.App.SetProperty("Identity", default(Identity));

            var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
            var exception = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
            ExceptionManager.RaiseException(this, exception);
            //CommandManager.InvalidateRequerySuggested();
        }
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TenantId) || string.IsNullOrEmpty(ClientId)) { return; }
                if (this.Configurations == null) { e.CanExecute = true; return; }

                var configurations = this.Configurations.ToArray();
                if (configurations.Any(configuration => configuration != null && configuration.TenantId == TenantId && configuration.ClientId == ClientId && configuration.ClientSecret == ClientSecret))
                {
                    return;
                }
            }
            catch (Exception ex) { }

            e.CanExecute = true;
        }
        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            var configuration = new TenantConfiguration()
            {
                TenantId = TenantId,
                ClientId = ClientId,
                ClientSecret = ClientSecret
            };

            var inputBox = new InputBoxControl()
            {
                Title = "Save configuration...",
                Label = "Choose a configuration name:",
                Text = "Samplename"
            };

            inputBox.OnOK += (sender, e) =>
            {
                using var scope = TraceLogger.BeginNamedScope(T, "OnOK");

                var inputBox = e as InputBoxControl;
                var profileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var chosenName = inputBox.Text;
                var fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                var executableName = Path.GetFileNameWithoutExtension(fileName);
                configuration.Name = chosenName;

                var configurationFilePath = Path.Combine(profileFolder, executableName, "Configuration", $"{chosenName}.json");
                if (!Directory.Exists(Path.GetDirectoryName(configurationFilePath))) { Directory.CreateDirectory(Path.GetDirectoryName(configurationFilePath)); }

                File.WriteAllText(configurationFilePath, JsonConvert.SerializeObject(configuration));
                scope.LogDebug($"File.WriteAllText({configurationFilePath}, JsonConvert.SerializeObject(configuration)); completed");
            };

            Common.Commands.AddItem.Execute(inputBox, App.MainWindow);

            var configurations = this.Configurations is null ? new List<TenantConfiguration>() : new List<TenantConfiguration>(this.Configurations);
            configurations.Add(configuration);
            this.Configurations = configurations;
        }

        private void CmbLanguages_DropDownClosed(object sender, EventArgs e)
        {
            if (sender is ComboBox cb)
            {
                //CultureInfo.CurrentCulture = (CultureInfo)cb.SelectedItem;
                Commands.ChangeCulture.Execute((CultureInfo)cb.SelectedItem, this);
                //Thread.CurrentThread.CurrentCulture = (CultureInfo)cb.SelectedItem;

            }
        }
    }
}

