#region using
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using System.ComponentModel;
using ExceptionEventArgs = Common.ExceptionEventArgs;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Identity.Client;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Azure;
using System.Security.Claims;
using Microsoft.Identity.Client.NativeInterop;
using Microsoft.Azure.Amqp.Framing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Azure.Core;
using System.Net.Sockets;
#endregion

namespace KeyVaultSample
{
    /// <summary>Interaction logic for MainControl.xaml</summary>
    public partial class MainControl : UserControl
    {
        #region const
        const string CONFIGVALUE_KEYVAULTADDRESS = "KeyVaultAddress", DEFAULTVALUE_KEYVAULTADDRESS = "";
        const string CONFIGVALUE_CLIENTSECRET = "ClientSecret", DEFAULTVALUE_CLIENTSECRET = "";
        #endregion

        private IConfiguration configuration;
        private ILogger<MainControl> logger;
        private AuthenticationService authenticationService;
        private string tenantId;
        private string clientId;
        Dictionary<string, string> secretValues = new Dictionary<string, string>();

        #region .ctor
        public MainControl()
        {
            using var scope = logger.BeginMethodScope();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            var app = App.Current as App;
            this.App = app;

            this.App.PropertyChanged += App_PropertyChanged;

            this.configuration = app.Host.Services.GetService<IConfiguration>();
            this.logger = app.Host.Services.GetService<ILogger<MainControl>>();
            this.authenticationService = this.App.Host.Services.GetService<AuthenticationService>();
            scope.LogDebug(new { authenticationService = authenticationService.GetLogString() });

            InitializeComponent();
        }
        private void App_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });
            scope.LogDebug(new { e.PropertyName });

            if (new[] { "Identity" }.Contains(e.PropertyName)) { Identity_PropertyChanged(sender, e); return; }
            //if (new[] { "Identity" }.Contains(e.PropertyName)) { Identity_PropertyChanged(sender, e); return; }

        }
        #endregion
        Reference<bool> _lockIdentity_PropertyChanged = new Reference<bool>();
        private void Identity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

            if (this._lockIdentity_PropertyChanged.Value) { return; }
            using var switchLocal = new SwitchOnDispose(this._lockIdentity_PropertyChanged, true);

            this.Identity = App.Properties["Identity"] as Identity;
        }

        #region App
        public App App
        {
            get { return (App)GetValue(AppProperty); }
            set { SetValue(AppProperty, value); }
        }
        public static readonly DependencyProperty AppProperty = DependencyProperty.Register("App", typeof(App), typeof(MainControl), new PropertyMetadata(null));
        #endregion
        #region Output
        public string Output
        {
            get { return (string)GetValue(OutputProperty); }
            set { SetValue(OutputProperty, value); }
        }
        public static readonly DependencyProperty OutputProperty = DependencyProperty.Register("Output", typeof(string), typeof(MainControl), new PropertyMetadata(null));
        #endregion

        private async void ctlMain_Initialized(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            using var scope = logger.BeginMethodScope(new { sender, e });

            scope.LogDebug(new { authenticationService = authenticationService.GetLogString() });

            this.ExceptionEvent += MainControl_ExceptionEvent;

            try
            {
                await Task.Run(async () => await ctlMain_InitializedAsync(sender, e));

                var environment = TraceManager.EnvironmentName;
                var panelInfo = new SettingsPanelInfo<SettingsAppInsightKeyControl>() { Name = "AppInsight", Description = "AppInsight", InternalName = "", Position = 0, Type = null };
                Commands.RegisterPanel.Execute(panelInfo, settingsControl);
                var panelAkv = new SettingsPanelInfo<SettingsKeyVaultControl>() { Name = "KeyVault", Description = "Key Vault", InternalName = "", Position = 0, Type = null };
                Commands.RegisterPanel.Execute(panelAkv, settingsControl);
                var panelAccount = new SettingsPanelInfo<SettingsAccountControl>() { Name = "Account", Description = "Account", InternalName = "", Position = 0, Type = null };
                Commands.RegisterPanel.Execute(panelAccount, settingsControl);
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
        private async Task ctlMain_InitializedAsync(object sender, EventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender, e });

            try
            {
                var tenantId = default(string);
                var clientId = default(string);
                var clientSecret = default(string);
                var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS);

                var identity = await authenticationService.LoginSilentAsync();
                this.Dispatcher.Invoke(() =>
                {
                    this.Identity = identity;
                    tenantId = App.TenantId; clientId = App.ClientId; clientSecret = App.ClientSecret;
                    App.ConnectionString = App.KeyVaultAddress = keyVaultAddress;
                    
                    scope.LogDebug(new { this.Identity, tenantId, clientId, clientSecret, keyVaultAddress });
                });



                TokenCredential credential = null;
                if (!string.IsNullOrEmpty(clientSecret)) { credential = new ClientSecretCredential(tenantId, clientId, clientSecret); }
                if (identity == null && credential == null)
                {
                    var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                    credential = new DefaultAzureCredential(credentialOptions);
                }

                if (credential != null)
                {
                    ConfigurationManager configurationManager = null;
                    this.Dispatcher.Invoke(() =>
                    {
                        configurationManager = App.ConfigurationManager;
                        scope.LogDebug($"configurationManager");
                    });

                    var secretClient = new SecretClient(new Uri(keyVaultAddress), credential);
                    configurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                    scope.LogDebug($"configurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());");
                }
            }
            catch (Exception ex)
            {
                var message = this.GetResourceValue<string>("Info.ExceptionLoadingSecrets", "Cannot load secrets from Key Vault");
                var ex1 = new ClientException(message, ex) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, ex1);
                // CommandManager.InvalidateRequerySuggested();
                // RegreshCount += 1;
            }
        }
        private void ctlMain_Loaded(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();
            // CommandManager.InvalidateRequerySuggested();
            // bndSelf.UpdateTarget();
            // UserControlHelper.UpdateTargets(bndSelf);
            BindingOperations.GetMultiBindingExpression(grdErrors, AttachedProperties.IsVisible0Property);

            // RegreshCount += 1;
        }


        #region ShowSettingsPanel
        public bool ShowSettingsPanel
        {
            get { return (bool)GetValue(ShowSettingsPanelProperty); }
            set { SetValue(ShowSettingsPanelProperty, value); }
        }
        public static readonly DependencyProperty ShowSettingsPanelProperty = DependencyProperty.Register("ShowSettingsPanel", typeof(bool), typeof(MainControl), new PropertyMetadata(false));
        #endregion
        #region Identity
        public Identity Identity
        {
            get { return (Identity)GetValue(IdentityProperty); }
            set { SetValue(IdentityProperty, value); }
        }
        public static readonly DependencyProperty IdentityProperty = DependencyProperty.Register("Identity", typeof(Identity), typeof(MainControl), new PropertyMetadata());
        #endregion

        #region VaultUri
        public Uri VaultUri
        {
            get { return (Uri)GetValue(VaultUriProperty); }
            set { SetValue(VaultUriProperty, value); }
        }
        public static readonly DependencyProperty VaultUriProperty = DependencyProperty.Register("VaultUri", typeof(Uri), typeof(MainControl), new PropertyMetadata());
        #endregion
        #region Secrets
        public IEnumerable<SecretProperties> Secrets
        {
            get { return (IEnumerable<SecretProperties>)GetValue(SecretsProperty); }
            set { SetValue(SecretsProperty, value); }
        }
        public static readonly DependencyProperty SecretsProperty = DependencyProperty.Register("Secrets", typeof(IEnumerable<SecretProperties>), typeof(MainControl), new PropertyMetadata(null));
        #endregion

        // Commands
        private void RunCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void RunCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS);
            if (App.ConnectionString is not null) { keyVaultAddress = App.ConnectionString; }
            var clientSecret = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_CLIENTSECRET, DEFAULTVALUE_CLIENTSECRET);

            TokenCredential credential = null;
            if (!string.IsNullOrEmpty(clientSecret)) { credential = new ClientSecretCredential(tenantId, clientId, clientSecret); }
            if (this.Identity == null && credential == null)
            {
                var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
                var exception = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, exception);
                return;
            }
            if (credential == null)
            {
                var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = this.Identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                credential = new DefaultAzureCredential(credentialOptions);
            }


            if (credential != null)
            {
                var client = new SecretClient(new Uri(keyVaultAddress), credential); // , new SecretClientOptions()
                this.VaultUri = client.VaultUri;

                secretValues = new Dictionary<string, string>();
                IEnumerable<SecretProperties> secrets = client.GetPropertiesOfSecrets();
                this.Secrets = secrets;
                foreach (var secret in secrets)
                {
                    // Getting a disabled secret will fail, so skip disabled secrets.
                    if (!secret.Enabled.GetValueOrDefault()) { continue; }

                    KeyVaultSecret secretWithValue = client.GetSecret(secret.Name);
                    if (!secretValues.ContainsKey(secretWithValue.Name))
                    {
                        var message = $"Secret {secretWithValue.Name}: {secretWithValue.Value}";
                        this.Output += $"\r\n{message}";
                        secretValues.Add(secretWithValue.Name, secretWithValue.Value);
                    }
                }
            }
        }
        private void LoginToggleCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void LoginToggleCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();
            if (Identity == null) { Commands.Login.Execute(null, this); }
            else { Commands.Logout.Execute(null, this); }
        }
        private void LoginCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void LoginCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            scope.LogDebug(new { authenticationService = authenticationService.GetLogString() });
            if (authenticationService == null)
            {
                var message = this.GetResourceValue<string>("Info.AuthHelperNotAvailable", "Authentication helper is not available");
                var ex = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, ex);
                //CommandManager.InvalidateRequerySuggested();
                return;
            }

            await authenticationService.LogoutAsync();
            var identity = await authenticationService.LoginAsync();

            TokenCredential credential = null;
            var clientSecret = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_CLIENTSECRET, DEFAULTVALUE_CLIENTSECRET);
            if (!string.IsNullOrEmpty(clientSecret)) { credential = new ClientSecretCredential(tenantId, clientId, clientSecret); }
            if (identity == null && credential == null)
            {
                var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
                var ex = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, ex);
                return;
            }
            if (credential == null)
            {
                var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                credential = new DefaultAzureCredential(credentialOptions);
            }

            var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS); scope.LogDebug($"ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS); returned {keyVaultAddress}");
            var secretClient = new SecretClient(new Uri(keyVaultAddress), credential);
            App.ConfigurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            this.Identity = identity;

            var exceptions = ExceptionProperties.GetExceptions(this);
            var exception = exceptions.LastOrDefault();
            if (exception is ClientException clientException && clientException.Code == ExceptionCodes.PRESSLOGIN)
            {
                exception = null;
            }

            App.ConnectionString = keyVaultAddress;
            //App.EventHubName = ConfigurationHelper.GetClassSetting<App, string>("EventHubName", S_EVENTHUBNAME_DEFAULT);
            //App.BlobstorageConnectionString = ConfigurationHelper.GetClassSetting<App, string>("BlobstorageConnectionString", S_BLOBSTORAGECONNECTIONSTRING_DEFAULT);
            //App.CheckpointsContainer = ConfigurationHelper.GetClassSetting<App, string>("CheckpointsContainer", S_CHECKPOINTSCONTAINER_DEFAULT);

            scope.LogDebug(new { ConnectionString = App.ConnectionString }); // , App.EventHubName, App.BlobstorageConnectionString, App.CheckpointsContainer


            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
        }
        private void LogoutCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void LogoutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            if (authenticationService == null) { return; }
            await authenticationService.LogoutAsync(); scope.LogDebug($"await authenticationService.LogoutAsync(); completed");
            this.Identity = null;

            this.Secrets = null;
            this.Output = null;

            var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
            var exception = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
            ExceptionManager.RaiseException(this, exception);
            //CommandManager.InvalidateRequerySuggested();
        }
        private void WatchCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void WatchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

            var secret = e.Parameter as SecretProperties;

            var originalSource = e.OriginalSource as FrameworkElement;

            var listViewItem = UserControlHelper.FindAncestor<ListViewItem>(originalSource);
            scope.LogDebug(new { listViewItem = listViewItem.GetLogString() });

            var isVisible0 = listViewItem.GetValue(AttachedProperties.IsVisible0Property) ?? false;
            listViewItem.SetValue(AttachedProperties.IsVisible0Property, !(bool)isVisible0);
        }
        private void WatchAllCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void WatchAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

            var originalSource = e.OriginalSource as FrameworkElement;
            var listView = UserControlHelper.FindAncestor<ListView>(originalSource);
            scope.LogDebug(new { listView = listView.GetLogString() });

            var itemContainerGenerator = listView.ItemContainerGenerator;

            var item = listView.Items.OfType<object>().FirstOrDefault();
            var listViewItem = (ListViewItem)itemContainerGenerator.ContainerFromItem(item);

            var isVisible0 = listViewItem.GetValue(AttachedProperties.IsVisible0Property) ?? false;
            foreach (var item1 in listView.Items)
            {
                var listViewItem1 = (ListViewItem)itemContainerGenerator.ContainerFromItem(item1);
                listViewItem1.SetValue(AttachedProperties.IsVisible0Property, !(bool)isVisible0);
            }
        }
        private void CopyCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void CopyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            var secret = e.Parameter as SecretProperties;
            if (!secret.Enabled.GetValueOrDefault()) { Clipboard.SetText($"secret '{secret.Name}' is disabled"); return; }

            var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS);
            if (App.ConnectionString is not null) { keyVaultAddress = App.ConnectionString; }
            var clientSecret = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_CLIENTSECRET, DEFAULTVALUE_CLIENTSECRET);

            TokenCredential credential = null;
            if (!string.IsNullOrEmpty(clientSecret)) { credential = new ClientSecretCredential(tenantId, clientId, clientSecret); }
            if (this.Identity == null && credential == null)
            {
                var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
                var exception = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, exception);
                return;
            }
            if (this.Identity != null && credential == null)
            {
                credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    SharedTokenCacheUsername = this.Identity.Upn,
                    ExcludeInteractiveBrowserCredential = false,
                    ExcludeSharedTokenCacheCredential = false,
                    ExcludeAzureCliCredential = false,
                    ExcludeEnvironmentCredential = true,
                    ExcludeManagedIdentityCredential = true,
                    ExcludeVisualStudioCodeCredential = true,
                    ExcludeVisualStudioCredential = true
                });
            }
            if (credential != null)
            {
                var client = new SecretClient(new Uri(keyVaultAddress), credential); // , new SecretClientOptions()
                this.VaultUri = client.VaultUri;

                KeyVaultSecret secretWithValue = client.GetSecret(secret.Name);
                var message = $"Secret {secretWithValue.Name}: {secretWithValue.Value}";
                this.Output += $"\r\n{message}";

                Clipboard.SetText(secretWithValue.Value);
            }
        }
        private void SettingsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Application.Current.Windows.Count
            e.CanExecute = true;
            e.Handled = true;
            return;
        }
        private void SettingsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            // TODO: new SettingsControl,
            // TODO: AddItem

            ShowSettingsPanel = !ShowSettingsPanel;
            if (ShowSettingsPanel)
            {
                Commands.Reset.Execute(null, settingsControl);
            }
        }
        #region HideSettingsCanExecute
        private void HideSettingsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
            return;
        }
        #endregion
        #region HideSettingsCommand
        private void HideSettingsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();
            ShowSettingsPanel = false;
        }
        #endregion
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

        }


        // Exception event
        public event RoutedEventHandler PreviewExceptionEvent
        {
            add { AddHandler(ExceptionManager.PreviewExceptionEvent, value); }
            remove { RemoveHandler(ExceptionManager.PreviewExceptionEvent, value); }
        }
        public event RoutedEventHandler ExceptionEvent
        {
            add { AddHandler(ExceptionManager.ExceptionEvent, value); }
            remove { RemoveHandler(ExceptionManager.ExceptionEvent, value); }
        }
        private void MainControl_ExceptionEvent(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

            var exArg = e as ExceptionEventArgs;
            var ex = exArg.Exception;
            var exceptions = ExceptionProperties.GetExceptions(this);
            if (exceptions == null) { exceptions = new ObservableCollection<Exception>(); ExceptionProperties.SetExceptions(this, exceptions); }
            exceptions.Add(ex);

            exArg.Caught = true;
            exArg.Handled = true;
        }

        // Conversion Events
        private object getIdentityDescription_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var identity = value as Identity;
            if (identity == null) { return "login"; }
            if (!string.IsNullOrEmpty(identity.Name)) { return identity.Name; }
            if (!string.IsNullOrEmpty(identity.Upn)) { return identity.Upn; }

            return identity.Email;
        }
        private object starColumnSize_ConvertEvent(DependencyObject source, object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int i = 0;
            var listView = values != null && values.Length > i && values[i] is ListView ? values[i] as ListView : null; i++;
            var width = values != null && values.Length > i && values[i] is ListView ? values[i] as object : null; i++;

            if (listView == null) { return 150; }
            var gridView = listView.View as GridView;
            var gridViewColumnsWidth = gridView.Columns.Sum(c => c.ActualWidth);
            var valueColum = gridView.Columns.FirstOrDefault(c => string.IsNullOrEmpty(c.Header as string));
            double otherColumnsWidth = gridViewColumnsWidth - valueColum.ActualWidth;
            double starWidth = listView.ActualWidth - otherColumnsWidth;

            TraceLogger.LogDebug($"listView.ActualWidth:{listView.ActualWidth:0.00},otherColumnsWidth:{otherColumnsWidth:0.00},starWidth:{starWidth:0.00},gridViewColumnsWidth:{gridViewColumnsWidth:0.00},valueColum.ActualWidth:{valueColum.ActualWidth:0.00}");
            return starWidth - listView.BorderThickness.Left - listView.BorderThickness.Right;
        }
        private object getSecretValue_ConvertEvent(DependencyObject source, object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int i = 0;
            var secret = values != null && values.Length > i && values[i] is SecretProperties ? values[i] as SecretProperties : null; i++;
            var isVisible0 = values != null && values.Length > i && values[i] is bool ? (bool)values[i] : false; i++;
            //var listViewItem = values != null && values.Length > i && values[i] is ListViewItem ? values[i] as ListViewItem : null; i++;

            if (!secret.Enabled.GetValueOrDefault()) { return "secret is disabled"; }

            var name = secret.Name;
            var kvp = secretValues.FirstOrDefault(kvp => kvp.Key == name);
            var value = kvp.Value as string;
            if (isVisible0 == false)
            {
                var length = value != null ? value.Length : 0;
                if (length < 5) { length = 5; }
                return new string('*', length);
            }
            //if (isVisible0 == false) { return DependencyProperty.UnsetValue; }

            TraceLogger.LogDebug($"name:{name},value:{value}");
            return value;
        }
        private object firstOrDefaultLocal_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            using var scope = logger.BeginMethodScope(new { value = value.GetLogString() });

            var exceptions = value != null && value is IList<Exception> ? value as IList<Exception> : null;
            var exception = exceptions.LastOrDefault();

            return exception;
        }

        private object firstOrDefaultLocal_ConvertEvent2(DependencyObject source, object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            using var scope = logger.BeginMethodScope(new { values = values.GetLogString() });

            int i = 0;
            var exceptions = values != null && values.Length > i && values[i] is IList<Exception> ? values[i] as IList<Exception> : null; i++;
            var refreshCount = values != null && values.Length > i && values[i] is int ? (int)values[i] : 0; i++;
            if (exceptions == null) { return null; }

            var exception = exceptions?.LastOrDefault();
            scope.LogDebug(new { exception = exception.GetLogString(), exceptions = exceptions.GetLogString(), refreshCount });

            return exception;
            // return new Exception(exception.Message, exception);
        }
        private object onChangeIsMouseOver_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isMouseOver = (bool)value;
            if (isMouseOver == false)
            {
                Rect areaBounds = new Rect(0, 0, this.ActualWidth * 0.9, this.ActualHeight * 0.9);
                Point mousePosition = Mouse.GetPosition(Application.Current.MainWindow);

                if (areaBounds.Contains(mousePosition))
                {
                    Commands.HideSettings.Execute(null, this);
                }
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
