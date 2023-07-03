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

                    //ConfigurationManager configurationManager = null;
                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    configurationManager = ApplicationBase.Current.Properties["ConfigurationManager"] as ConfigurationManager;
                    //    scope.LogDebug($"configurationManager");
                    //});

                    //var secretClient = new SecretClient(new Uri(keyVaultAddress), credential);
                    //configurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
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


        // Commands
        private void ToggleIsCollapsedCanExecute(object sender, CanExecuteRoutedEventArgs e)
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

