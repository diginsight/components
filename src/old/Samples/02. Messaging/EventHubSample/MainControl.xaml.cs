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
#endregion

namespace EventHubSample
{
    /// <summary>Interaction logic for MainControl.xaml</summary>
    public partial class MainControl : UserControl
    {
        #region const
        const string S_CONNECTIONSTRING_DEFAULT = @"";
        const string S_BLOBSTORAGECONNECTIONSTRING_DEFAULT = "";
        const string S_CHECKPOINTSCONTAINER_DEFAULT = "";
        const string S_EVENTHUBNAME_DEFAULT = "";
        const string CONFIGVALUE_KEYVAULTADDRESS = "KeyVaultAddress", DEFAULTVALUE_KEYVAULTADDRESS = "";
        const string CONFIGVALUE_APPINSIGHTSKEY = "AppInsightsKey", DEFAULTVALUE_APPINSIGHTSKEY = "";
        const string CONFIGVALUE_TENANTID = "TenantId"; const string DEFAULTVALUE_TENANTID = "";
        const string CONFIGVALUE_CLIENTID = "ClientId"; const string DEFAULTVALUE_CLIENTID = "";
        const string CONFIGVALUE_APPNAME = "AppName"; const string DEFAULTVALUE_APPNAME = "";
        const string CONFIGVALUE_APPVERSION = "AppVersion"; const string DEFAULTVALUE_APPVERSION = "";
        const string CONFIGVALUE_REDIRECTURI = "RedirectUri"; const string DEFAULTVALUE_REDIRECTURI = "";
        const string CONFIGVALUE_SCOPES = "Scopes"; const string DEFAULTVALUE_SCOPES = "";
        const string CONFIGVALUE_OAUTHVERSIONSUFFIX = "OauthVersionSuffix"; const string DEFAULTVALUE_OAUTHVERSIONSUFFIX = "/2.0";
        #endregion

        private IConfiguration configuration;
        private ILogger<MainControl> logger;
        private AuthenticationService authenticationService;
        private string tenantId;
        private string clientId;
        private string appName;
        private string appVersion;
        private string redirectUri;
        private string scopes;
        private string oauthVersionSuffix;

        #region .ctor
        public MainControl()
        {
            using var scope = logger.BeginMethodScope();

            var app = App.Current as App;
            this.App = app;

            this.configuration = app.Host.Services.GetService<IConfiguration>();
            this.logger = app.Host.Services.GetService<ILogger<MainControl>>();

            //var configuration = app.Host.Services.GetService<IConfiguration>();

            InitializeComponent();
        }
        #endregion

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
        #region RegreshCount
        public int RegreshCount
        {
            get { return (int)GetValue(RegreshCountProperty); }
            set { SetValue(RegreshCountProperty, value); }
        }
        public static readonly DependencyProperty RegreshCountProperty = DependencyProperty.Register("RegreshCount", typeof(int), typeof(MainControl), new PropertyMetadata(0));
        #endregion

        private async void ctlMain_Initialized(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            using var scope = logger.BeginMethodScope(new { sender, e });

            // ExceptionManager.CatchExceptions()
            this.ExceptionEvent += MainControl_ExceptionEvent;

            try
            {
                this.tenantId = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID); // , CultureInfo.InvariantCulture
                this.clientId = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_CLIENTID, DEFAULTVALUE_CLIENTID); // , CultureInfo.InvariantCulture
                this.appName = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_APPNAME, DEFAULTVALUE_APPNAME); // , CultureInfo.InvariantCulture SettingAccessType.SecretWithCredential, 
                this.appVersion = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_APPVERSION, DEFAULTVALUE_APPVERSION); // , CultureInfo.InvariantCulture
                this.redirectUri = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_REDIRECTURI, DEFAULTVALUE_REDIRECTURI); // , CultureInfo.InvariantCulture
                this.scopes = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_SCOPES, DEFAULTVALUE_SCOPES); // , CultureInfo.InvariantCulture
                this.oauthVersionSuffix = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_OAUTHVERSIONSUFFIX, DEFAULTVALUE_OAUTHVERSIONSUFFIX); // , CultureInfo.InvariantCulture
                scope.LogDebug(new { clientId, appName, appVersion, oauthVersionSuffix, scopes = scopes.GetLogString() });

                var appInsightKey = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_APPINSIGHTSKEY, DEFAULTVALUE_APPINSIGHTSKEY); // , CultureInfo.InvariantCulture SettingAccessType.SecretWithCredential, 
                scope.LogDebug(new { appInsightKey });

                var authenticationService = new AuthenticationService(tenantId, clientId, appName, appVersion, redirectUri, scopes?.Split(','), oauthVersionSuffix, Application.Current.MainWindow);
                scope.LogDebug(new { _authenticationService = authenticationService.GetLogString() });
                this.authenticationService = authenticationService;

                await Task.Run(async () => await ctlMain_InitializedAsync(sender, e));
            }
            catch (Exception ex)
            {
                ExceptionManager.RaiseException(this, ex);
                scope.LogException(ex);
            }
        }
        private async Task ctlMain_InitializedAsync(object sender, EventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender, e });

            try
            {
                var identity = await authenticationService.LoginSilentAsync();

                this.Dispatcher.Invoke(() =>
                {
                    this.Identity = identity;
                    scope.LogDebug(new { this.Identity });
                });

                if (identity != null)
                {
                    //// Create DefaultAzureCredential with the token
                    var credentialOptions = new DefaultAzureCredentialOptions
                    {
                        SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false,
                        ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true
                    };
                    var credential = new DefaultAzureCredential(credentialOptions);

                    var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS); scope.LogDebug($"ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS); returned {keyVaultAddress}");
                    var secretClient = new SecretClient(new Uri(keyVaultAddress), credential);
                    this.Dispatcher.Invoke(() =>
                    {
                        App.ConfigurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                        scope.LogDebug($"App.ConfigurationManager.AddAzureKeyVault({secretClient}, new KeyVaultSecretManager());");
                    });
                    string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

                    var connectionString = ConfigurationHelper.GetClassSetting<App, string>("EventHubConnectionString", S_CONNECTIONSTRING_DEFAULT);
                    var eventHubName = ConfigurationHelper.GetClassSetting<App, string>("EventHubName", S_EVENTHUBNAME_DEFAULT);
                    var blobstorageConnectionString = ConfigurationHelper.GetClassSetting<App, string>("BlobstorageConnectionString", S_BLOBSTORAGECONNECTIONSTRING_DEFAULT);
                    var checkpointsContainer = ConfigurationHelper.GetClassSetting<App, string>("CheckpointsContainer", S_CHECKPOINTSCONTAINER_DEFAULT);
                    this.Dispatcher.Invoke(() =>
                    {
                        App.ConnectionString = connectionString;
                        App.EventHubName = eventHubName;
                        App.BlobstorageConnectionString = blobstorageConnectionString;
                        App.CheckpointsContainer = checkpointsContainer;
                        scope.LogDebug(new { App.ConnectionString, App.EventHubName, App.BlobstorageConnectionString, App.CheckpointsContainer });
                    });

                    // Create a blob container client that the event processor will use 
                    var storageClient = new BlobContainerClient(blobstorageConnectionString, checkpointsContainer);

                    // Create an event processor client to process events in the event hub
                    var processor = new EventProcessorClient(storageClient, consumerGroup, connectionString, eventHubName); // App.EventHubName
                    scope.LogDebug(new { processor = processor.GetLogString() });

                    //// Register handlers for processing events and handling errors
                    processor.ProcessEventAsync += Processor_ProcessEventAsync; // ProcessEventHandler;
                    processor.ProcessErrorAsync += Processor_ProcessErrorAsync; // ProcessErrorHandler;

                    //// Start the processing
                    processor.StartProcessingAsync().GetAwaiter().GetResult();
                    //processor.EventHubName
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Processor = processor;
                        scope.LogDebug(new { this.Processor });
                    });
                }
            }
            catch (Exception ex)
            {
                var message = this.GetResourceValue<string>("Info.ExceptionLoadingSecrets", "Cannot load secrets from Key Vault");
                var ex1 = new ClientException(message, ex) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, ex1);
            }
        }

        private void ctlMain_Loaded(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();
            //RegreshCount += 1;
            BindingOperations.GetMultiBindingExpression(grdErrors, AttachedProperties.IsVisible0Property);
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            await using var producerClient = new EventHubProducerClient(App.ConnectionString, App.EventHubName); scope.LogDebug($"await using var producerClient = new EventHubProducerClient({App.ConnectionString}, {App.EventHubName});");

            using var eventBatch = await producerClient.CreateBatchAsync(); scope.LogDebug($"using await producerClient.CreateBatchAsync(); returned {eventBatch.GetLogString()}");

            // Add events to the batch. An event is a represented by a collection of bytes and metadata. 
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("First event"))); scope.LogDebug($"eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes('First event')));");
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Second event"))); scope.LogDebug($"eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes('Second event')));");
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Third event"))); scope.LogDebug($"eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes('Third event')));");

            // Use the producer client to send the batch of events to the event hub
            await producerClient.SendAsync(eventBatch); scope.LogDebug($"await producerClient.SendAsync(eventBatch); completed");
            scope.LogDebug("A batch of 3 events has been published.");
        }

        #region Identity
        public Identity Identity
        {
            get { return (Identity)GetValue(IdentityProperty); }
            set { SetValue(IdentityProperty, value); }
        }
        public static readonly DependencyProperty IdentityProperty = DependencyProperty.Register("Identity", typeof(Identity), typeof(MainControl), new PropertyMetadata());
        #endregion
        #region Processor
        public EventProcessorClient Processor
        {
            get { return (EventProcessorClient)GetValue(ProcessorProperty); }
            set { SetValue(ProcessorProperty, value); }
        }
        public static readonly DependencyProperty ProcessorProperty = DependencyProperty.Register("Processor", typeof(EventProcessorClient), typeof(MainControl), new PropertyMetadata());
        #endregion



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
            if (authenticationService == null) { return; }

            await authenticationService.LogoutAsync();
            var identity = await authenticationService.LoginAsync();

            if (identity == null)
            {
                this.Identity = identity;
                var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
                var ex = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
                ExceptionManager.RaiseException(this, ex);
                CommandManager.InvalidateRequerySuggested();
                //this.RegreshCount += 1;
                return;
            }

            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                SharedTokenCacheUsername = identity.Upn, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false,
                ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true
            });
            var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS);
            var secretClient = new SecretClient(new Uri(keyVaultAddress), credential);
            App.ConfigurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            this.Identity = identity;

            var exceptions = ExceptionProperties.GetExceptions(this);
            var exception = exceptions.LastOrDefault();
            if (exception is ClientException clientException && clientException.Code == ExceptionCodes.PRESSLOGIN)
            {
                exception = null;
            }

            App.ConnectionString = ConfigurationHelper.GetClassSetting<App, string>("EventHubConnectionString", S_CONNECTIONSTRING_DEFAULT);
            App.EventHubName = ConfigurationHelper.GetClassSetting<App, string>("EventHubName", S_EVENTHUBNAME_DEFAULT);
            App.BlobstorageConnectionString = ConfigurationHelper.GetClassSetting<App, string>("BlobstorageConnectionString", S_BLOBSTORAGECONNECTIONSTRING_DEFAULT);
            App.CheckpointsContainer = ConfigurationHelper.GetClassSetting<App, string>("CheckpointsContainer", S_CHECKPOINTSCONTAINER_DEFAULT);

            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            // Create a blob container client that the event processor will use 
            BlobContainerClient storageClient = new BlobContainerClient(App.BlobstorageConnectionString, App.CheckpointsContainer);

            //// Create an event processor client to process events in the event hub
            var processor = new EventProcessorClient(storageClient, consumerGroup, App.ConnectionString, "samplehub"); // App.EventHubName

            //// Register handlers for processing events and handling errors
            processor.ProcessEventAsync += Processor_ProcessEventAsync; // ProcessEventHandler;
            processor.ProcessErrorAsync += Processor_ProcessErrorAsync; // ProcessErrorHandler;

            //// Start the processing
            processor.StartProcessingAsync().GetAwaiter().GetResult();


            CommandManager.InvalidateRequerySuggested();
        }
        private void LogoutCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void LogoutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            if (authenticationService == null) { return; }
            await authenticationService.LogoutAsync(); scope.LogDebug($"await authenticationService.LogoutAsync(); completed");
            this.Identity = null;

            var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
            var exception = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
            ExceptionManager.RaiseException(this, exception);
            CommandManager.InvalidateRequerySuggested();
        }

        // Events
        private object getIdentityDescription_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var identity = value as Identity;
            if (identity == null) { return "login"; }
            if (!string.IsNullOrEmpty(identity.Name)) { return identity.Name; }
            if (!string.IsNullOrEmpty(identity.Upn)) { return identity.Upn; }

            return identity.Email;
        }
        private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            using var scope = logger.BeginMethodScope(new { arg = arg.GetLogString() });
            // Write details about the error to the console window

            this.Dispatcher.Invoke(() =>
            {
                var output = $"\tPartition '{arg.PartitionId}': an unhandled exception was encountered.\r\n{arg.Exception.Message}.\r\n{this.Output}";
                this.Output = output;
            });

            //Console.WriteLine();
            return Task.CompletedTask;
        }

        private async Task Processor_ProcessEventAsync(ProcessEventArgs arg)
        {
            using var scope = logger.BeginMethodScope(new { arg = arg.GetLogString() });
            var now = DateTime.Now;
            // Write the body of the event to the console window
            var value = Encoding.UTF8.GetString(arg.Data.Body.ToArray());

            this.Dispatcher.Invoke(() =>
            {
                Console.WriteLine("\tReceived event: {0}", value);
                this.Output = $"{now:HH:mm:ss.fff} Received event: {value}\r\n{this.Output}";
            });


            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await arg.UpdateCheckpointAsync(arg.CancellationToken);
        }

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
            using var scope = logger.BeginMethodScope();

            var exArg = e as ExceptionEventArgs;
            var ex = exArg.Exception;
            var exceptions = ExceptionProperties.GetExceptions(this);
            if (exceptions == null) { exceptions = new ObservableCollection<Exception>(); ExceptionProperties.SetExceptions(this, exceptions); }
            exceptions.Add(ex);

            exArg.Caught = true;
            exArg.Handled = true;
        }

        private object firstOrDefaultLocal_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            using var scope = logger.BeginMethodScope();

            var exceptions = value != null && value is IList<Exception> ? value as IList<Exception> : null;
            var exception = exceptions.FirstOrDefault();

            return exception;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

        }

        private object firstOrDefaultLocal_ConvertEvent2(DependencyObject source, object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            using var scope = logger.BeginMethodScope();

            int i = 0;
            var exceptions = values != null && values.Length > i && values[i] is IList<Exception> ? values[i] as IList<Exception> : null; i++;
            var refreshCount = values != null && values.Length > i && values[i] is int ? (int)values[i] : 0; i++;
            if (exceptions == null) { return null; }

            var exception = exceptions?.FirstOrDefault();
            scope.LogDebug(new { exception = exception.GetLogString(), exceptions = exceptions.GetLogString(), refreshCount });

            return exception;
        }
    }
}
