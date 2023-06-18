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
        private ILogger<SettingsAccountControl> logger;
        private AuthenticationHelper authenticationHelper;
        private string tenantId;
        private string clientId;
        private string appName;
        private string appVersion;
        private string redirectUri;
        private string scopes;
        private string oauthVersionSuffix;
        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(SettingsAccountControl), new PropertyMetadata(false));
        #endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SettingsAccountControl), new PropertyMetadata());
        #endregion
        #region Identity
        public Identity Identity
        {
            get { return (Identity)GetValue(IdentityProperty); }
            set { SetValue(IdentityProperty, value); }
        }
        public static readonly DependencyProperty IdentityProperty = DependencyProperty.Register("Identity", typeof(Identity), typeof(SettingsAccountControl), new PropertyMetadata());
        #endregion
        #region App
        public ApplicationBase App
        {
            get { return (ApplicationBase)GetValue(AppProperty); }
            set { SetValue(AppProperty, value); }
        }
        public static readonly DependencyProperty AppProperty = DependencyProperty.Register("App", typeof(ApplicationBase), typeof(SettingsAccountControl), new PropertyMetadata(null));
        #endregion

        #region .ctor
        static SettingsAccountControl()
        {
        }
        public SettingsAccountControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            //cmbLanguages.ItemsSource = LocalizationExtensions.GetAvailableCultures();
            //cmbLanguages.SelectedValue =  Thread.CurrentThread.CurrentCulture;
        }

        #endregion
        private async void ctlSettingsAccountControl_Initialized(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            using var scope = logger.BeginMethodScope(new { sender, e });

            //this.ExceptionEvent += SettingsAccountControl_ExceptionEvent;
            try
            {
                var tenantId = default(string);
                var clientId = default(string);
                var clientSecret = default(string);
                var keyVaultAddress = default(string);
                var identity = await authenticationHelper.LoginSilentAsync();
                this.Dispatcher.Invoke(() =>
                {
                    this.Identity = identity;
                    tenantId = ApplicationBase.Current.Properties["TenantId"] as string;
                    clientId = ApplicationBase.Current.Properties["ClientId"] as string;
                    clientSecret = ApplicationBase.Current.Properties["ClientSecret"] as string;
                    keyVaultAddress = ApplicationBase.Current.Properties["KeyVaultAddress"] as string;
                    scope.LogDebug(new { this.Identity });
                });

                TokenCredential credential = null;
                if (!string.IsNullOrEmpty(clientSecret)) { credential = new ClientSecretCredential(tenantId, clientId, clientSecret); }

                if (credential == null)
                {
                    var credentialOptions = new DefaultAzureCredentialOptions
                    {
                        SharedTokenCacheUsername = identity.Upn,
                        ExcludeInteractiveBrowserCredential = false,
                        ExcludeSharedTokenCacheCredential = false,
                        ExcludeAzureCliCredential = false,
                        ExcludeEnvironmentCredential = true,
                        ExcludeManagedIdentityCredential = true,
                        ExcludeVisualStudioCodeCredential = true,
                        ExcludeVisualStudioCredential = true
                    };
                    credential = new DefaultAzureCredential(credentialOptions);
                }

                if (credential != null)
                {
                    ConfigurationManager configurationManager = null;
                    this.Dispatcher.Invoke(() =>
                    {
                        configurationManager = ApplicationBase.Current.Properties["ConfigurationManager"] as ConfigurationManager;
                        scope.LogDebug($"configurationManager");
                    });

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

