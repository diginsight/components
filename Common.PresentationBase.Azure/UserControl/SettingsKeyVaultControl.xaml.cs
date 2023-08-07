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
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Core;
using Azure.Identity;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using Azure.ResourceManager;
using Azure;
using System.Diagnostics;
using Microsoft.Rest.Azure.OData;
#endregion

namespace Common
{
    /// <summary>Interaction logic for SettingsDownloadControl.xaml</summary>
    public partial class SettingsKeyVaultControl : UserControl
    {
        private static readonly Type T = typeof(SettingsKeyVaultControl);
        private ILogger<SettingsKeyVaultControl> logger;
        private IConfiguration configuration;
        private AuthenticationService authenticationService;
        private IParallelService parallelService;

        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(SettingsKeyVaultControl), new PropertyMetadata(false));
        #endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SettingsKeyVaultControl), new PropertyMetadata());
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
        #region Tenant
        public TenantData Tenant
        {
            get { return (TenantData)GetValue(TenantProperty); }
            set { SetValue(TenantProperty, value); }
        }
        public static readonly DependencyProperty TenantProperty = DependencyProperty.Register("Tenant", typeof(TenantData), T, new PropertyMetadata());
        #endregion
        #region KeyVaults
        public IList<GenericResourceData> KeyVaults
        {
            get { return (IList<GenericResourceData>)GetValue(KeyVaultsProperty); }
            set { SetValue(KeyVaultsProperty, value); }
        }
        public static readonly DependencyProperty KeyVaultsProperty = DependencyProperty.Register("KeyVaults", typeof(IList<GenericResourceData>), T, new PropertyMetadata());
        #endregion
        #region KeyVault
        public GenericResourceData KeyVault
        {
            get { return (GenericResourceData)GetValue(KeyVaultProperty); }
            set { SetValue(KeyVaultProperty, value); }
        }
        public static readonly DependencyProperty KeyVaultProperty = DependencyProperty.Register("KeyVault", typeof(GenericResourceData), T, new PropertyMetadata());
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
        static SettingsKeyVaultControl()
        {
        }
        public SettingsKeyVaultControl()
        {
            using var scope = logger.BeginMethodScope();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            var app = ApplicationBase.Current as ApplicationBase;
            this.App = app;

            this.configuration = app.Host.Services.GetService<IConfiguration>();
            this.logger = app.Host.Services.GetService<ILogger<SettingsKeyVaultControl>>();

            var authenticationService = this.App.Host.Services.GetService<AuthenticationService>();
            this.authenticationService = authenticationService;
            var parallelService = this.App.Host.Services.GetService<IParallelService>();
            this.parallelService = parallelService;

            scope.LogDebug(new { authenticationService = authenticationService.GetLogString() });

            InitializeComponent();
        }
        #endregion

        private void ctlSettingsKeyVaultControl_Initialized(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            using var scope = logger.BeginMethodScope(new { sender, e });

            Task.Run(async () => await ctlSettingsKeyVaultControl_InitializedAsync(sender, e));

            //var app = Application.Current as ApplicationBase;
            //this.AIApplicationID = app.Properties["AIApplicationID"] as string;
            //this.AIApiKey = app.Properties["AIApiKey"] as string;
        }
        private async Task ctlSettingsKeyVaultControl_InitializedAsync(object sender, EventArgs e)
        {
            var isInDesignMode = false;
            this.Dispatcher.Invoke(() => { DesignerProperties.GetIsInDesignMode(this); });
            if (isInDesignMode) { return; }

            using var scope = logger.BeginMethodScope(new { sender, e });

            try
            {
                var tenantId = default(string);
                var authTenantId = default(string);
                var clientId = default(string);
                var clientSecret = default(string);
                var keyVaultAddress = default(string);
                var identity = await authenticationService.LoginSilentAsync();
                authTenantId = authenticationService.AuthenticationResult.TenantId;

                // LoadState(this) 

                this.Dispatcher.Invoke(() =>
                {
                    this.Identity = identity;
                    this.App.SetProperty("Identity", identity);
                    tenantId = this.TenantId = ApplicationBase.Current.Properties["TenantId"] as string;
                    clientId = this.ClientId = ApplicationBase.Current.Properties["ClientId"] as string;
                    clientSecret = this.ClientSecret = ApplicationBase.Current.Properties["ClientSecret"] as string;
                    keyVaultAddress = ApplicationBase.Current.Properties["KeyVaultAddress"] as string;
                });
                scope.LogDebug(new { identity = identity.GetLogString() });
                scope.LogDebug(new { tenantId, authTenantId, clientId, clientSecret, keyVaultAddress });

                TokenCredential credential = null;
                if (!string.IsNullOrEmpty(clientSecret)) { credential = new ClientSecretCredential(tenantId, clientId, clientSecret); }
                if (identity != null && credential == null)
                {
                    var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                    credential = new DefaultAzureCredential(credentialOptions);
                }
                if (credential == null) { return; }

                //var configuration = new TenantConfiguration();
                //var configurations = GetTenantConfigurations();
                //this.Dispatcher.Invoke(() =>
                //{
                //    scope.LogDebug(new { configurations });
                //    this.Configurations = configurations;
                //    if (configurations?.Count == 1) { this.Configuration = configuration = configurations.FirstOrDefault(); }
                //});

                var tenants = GetUserTenants(credential);
                this.Dispatcher.Invoke(() =>
                {
                    this.Tenants = tenants;
                });

                // get the configuration TenantId 
                Microsoft.Graph.Models.Application application = null;
                clientId = authenticationService.ApplicationId;
                var applicationTenantId = default(string);
                if (!string.IsNullOrEmpty(applicationTenantId)) { application = await GetOwnedApplicationAsync(identity, applicationTenantId, clientId); }
                if (applicationTenantId == null || application == null)
                {
                    var options = new ParallelOptions() { MaxDegreeOfParallelism = parallelService.MediumConcurrency };

                    await parallelService.ForEachAsync(tenants ?? new List<TenantData>(), options, async (tenant) =>
                    {
                        applicationTenantId = tenant.TenantId.ToString();
                        application = await GetOwnedApplicationAsync(identity, applicationTenantId, clientId);
                        if (application != null) { var breakException = new BreakLoopException(); breakException.Data["item"] = tenant; throw breakException; }
                    });
                }

                var clientTenantId = default(string);
                if (application != null)
                {
                    var applicationTenant = tenants.FirstOrDefault(t => t.DefaultDomain == application.PublisherDomain);
                    clientTenantId = applicationTenant?.TenantId?.ToString();
                }
                //if (string.IsNullOrEmpty(clientTenantId)) { clientTenantId = configuration?.ClientTenantId; }
                if (string.IsNullOrEmpty(clientTenantId)) { clientTenantId = authenticationService?.AuthenticationResult?.TenantId; }

                this.Dispatcher.Invoke(() =>
                {
                    var tenant = this.Tenants.FirstOrDefault(t => t.TenantId == Guid.Parse(clientTenantId));
                    this.Tenant = tenant;
                    //tenantId = tenant.TenantId?.ToString();
                });


                //var tenants = new List<TenantData>();
                var credentialOptions1 = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                credentialOptions1.TenantId = clientTenantId;
                credential = new DefaultAzureCredential(credentialOptions1);

                var armClient = new ArmClient(credential);
                var subscription = await armClient.GetDefaultSubscriptionAsync(); // .GetResourceGroups();

                string filterQuery = "resourceType eq 'Microsoft.KeyVault/vaults'"; // Modify this filter as needed
                var resourcesPageable = subscription.GetGenericResourcesAsync(filterQuery);

                var keyVaults = new List<GenericResourceData>();
                await foreach (Page<GenericResource> page in resourcesPageable.AsPages())
                {
                    foreach (GenericResource akvResource in page.Values)
                    {
                        var akvResourceData = akvResource.Data;
                        keyVaults.Add(akvResourceData);
                        // akvData.CreatedOn
                        // akvData.ChangedOn
                        // akvData.Id
                        // akvData.Location
                        // akvData.Name
                        // akvData.ResourceType
                    }
                }

                this.Dispatcher.Invoke(() =>
                {
                    this.KeyVaults = keyVaults;
                });


                //scope.LogDebug($"configurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());");

            }
            catch (Exception ex)
            {
                //var message = this.GetResourceValue<string>("Info.ExceptionLoadingSecrets", "Cannot load secrets from Key Vault");
                //var ex1 = new ClientException(message, ex) { Code = ExceptionCodes.PRESSLOGIN };
                //ExceptionManager.RaiseException(this, ex1);
                //CommandManager.InvalidateRequerySuggested();
                ExceptionManager.RaiseException(this, ex);
                scope.LogException(ex);
                // CommandManager.InvalidateRequerySuggested();
                // RegreshCount += 1;
            }
        }

        #region ToggleIsCollapsedCanExecute
        private void ToggleIsCollapsedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region ToggleIsCollapsedCommand
        private void ToggleIsCollapsedCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                IsCollapsed = !IsCollapsed;
            }
        }
        #endregion

        #region AIApplicationID
        public string AIApplicationID
        {
            get { return (string)GetValue(AIApplicationIDProperty); }
            set { SetValue(AIApplicationIDProperty, value); }
        }
        public static readonly DependencyProperty AIApplicationIDProperty = DependencyProperty.Register("AIApplicationID", typeof(string), typeof(SettingsKeyVaultControl), new PropertyMetadata(null, AIApplicationIDChanged));
        public static void AIApplicationIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pthis = d as SettingsKeyVaultControl;
            var app = Application.Current as ApplicationBase;
            app.SetProperty("AIApplicationID", pthis.AIApplicationID);
        }
        #endregion
        #region AIApiKey
        public string AIApiKey
        {
            get { return (string)GetValue(AIApiKeyProperty); }
            set { SetValue(AIApiKeyProperty, value); }
        }
        public static readonly DependencyProperty AIApiKeyProperty = DependencyProperty.Register("AIApiKey", typeof(string), typeof(SettingsKeyVaultControl), new PropertyMetadata(null, AIApiKeyChanged));
        public static void AIApiKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pthis = d as SettingsKeyVaultControl;
            var app = Application.Current as ApplicationBase;
            app.SetProperty("AIApiKey", pthis.AIApiKey);
        }
        #endregion

        #region GetUserTenants
        private List<TenantData> GetUserTenants(TokenCredential credential)
        {
            using var scope = logger.BeginMethodScope(new { credential = credential.GetLogString() });

            var tenants = new List<TenantData>();
            var armClient = new ArmClient(credential);
            var tenantObjects = armClient.GetTenants();
            foreach (var tenantObj in tenantObjects)
            {
                var tenantItem = tenantObj.Data;
                scope.LogDebug($"Id:{tenantItem.Id},TenantId:{tenantItem.TenantId},DisplayName:{tenantItem.DisplayName},DefaultDomain:{tenantItem.DefaultDomain},TenantCategory:{tenantItem.TenantCategory},TenantType:{tenantItem.TenantType},Country:{tenantItem.Country},CountryCode:{tenantItem.CountryCode}");
                tenants.Add(tenantItem);
            }

            return tenants;
        }
        #endregion
        #region GetOwnedApplicationAsync
        async Task<Microsoft.Graph.Models.Application> GetOwnedApplicationAsync(Identity identity, string tenantId, string clientId)
        {
            using var scope = logger.BeginMethodScope(new { identity = identity.GetLogString(), tenantId, clientId });

            try // GetApplication(tenantId, clientId)
            {
                var credentialOptions0 = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                credentialOptions0.TenantId = tenantId;
                var credential = new DefaultAzureCredential(credentialOptions0);

                var requestUri = $"https://graph.microsoft.com/v1.0/me/ownedObjects/microsoft.graph.application"; // ?$filter=appId eq '{clientId}'
                var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }), new CancellationToken());
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                    request.Headers.Add("ConsistencyLevel", "eventual");

                    using (var clientApp = new HttpClient())
                    using (var responseApp = await clientApp.SendAsync(request))
                    {
                        responseApp.EnsureSuccessStatusCode();

                        var responseAppContent = await responseApp.Content.ReadAsStringAsync();
                        var responseAppOjbect = JObject.Parse(responseAppContent);

                        var applicationList = responseAppOjbect["value"];
                        foreach (var item in applicationList)
                        {
                            var id = item["id"]?.Value<string>();
                            var appId = item["appId"]?.Value<string>();
                            var displayName = item["displayName"]?.Value<string>();
                            var description = item["description"]?.Value<string>();
                            var createdDateTime = item["createdDateTime"]?.Value<DateTime?>();
                            var signInAudience = item["signInAudience"]?.Value<string>();
                            var identifierUris = item["identifierUris"]?.Select(uri => uri?.Value<string>()).ToList();
                            var publisherDomain = item["publisherDomain"]?.Value<string>();

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

                            if (application.AppId == clientId) { return application; }
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return null;
        }
        #endregion
    }
}

