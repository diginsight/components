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
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;
using Hyak.Common;
using System.Net.NetworkInformation;
#endregion

namespace IotHubSample
{
    /// <summary>Interaction logic for MainControl.xaml</summary>
    public partial class MainControl : UserControl
    {
        #region const
        const string S_CONNECTIONSTRING_DEFAULT = @"";
        const string S_BLOBSTORAGECONNECTIONSTRING_DEFAULT = "";
        const string S_CHECKPOINTSCONTAINER_DEFAULT = "";
        const string S_QUEUENAME_DEFAULT = "samplequeue";
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
        private AuthenticationHelper authenticationHelper;
        private string tenantId;
        private string clientId;
        private string appName;
        private string appVersion;
        private string redirectUri;
        private string scopes;
        private string oauthVersionSuffix;
        private DeviceClient deviceClient;

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
        #region TopicOutput
        public string TopicOutput
        {
            get { return (string)GetValue(TopicOutputProperty); }
            set { SetValue(TopicOutputProperty, value); }
        }
        public static readonly DependencyProperty TopicOutputProperty = DependencyProperty.Register("TopicOutput", typeof(string), typeof(MainControl), new PropertyMetadata(null));
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

                var authenticationHelper = new AuthenticationHelper(tenantId, clientId, appName, appVersion, redirectUri, scopes?.Split(','), oauthVersionSuffix, Application.Current.MainWindow);
                scope.LogDebug(new { _authenticationHelper = authenticationHelper.GetLogString() });
                this.authenticationHelper = authenticationHelper;

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
                //var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS);

                var identity = await authenticationHelper.LoginSilentAsync();
                this.Dispatcher.Invoke(() =>
                {
                    this.Identity = identity;
                    //tenantId = App.TenantId; clientId = App.ClientId; clientSecret = App.ClientSecret;
                    //App.ConnectionString = App.KeyVaultAddress = keyVaultAddress;
                });


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
                        SharedTokenCacheUsername = identity.Upn,
                        ExcludeInteractiveBrowserCredential = false,
                        ExcludeSharedTokenCacheCredential = false,
                        ExcludeAzureCliCredential = false,
                        ExcludeEnvironmentCredential = true,
                        ExcludeManagedIdentityCredential = true,
                        ExcludeVisualStudioCodeCredential = true,
                        ExcludeVisualStudioCredential = true
                    };
                    var credential = new DefaultAzureCredential(credentialOptions);

                    var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS); scope.LogDebug($"ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS); returned {keyVaultAddress}");
                    var secretClient = new SecretClient(new Uri(keyVaultAddress), credential);

                    ConfigurationManager configurationManager = null;
                    this.Dispatcher.Invoke(() =>
                    {
                        configurationManager = App.ConfigurationManager;
                        scope.LogDebug($"configurationManager");
                    });

                    configurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                    scope.LogDebug($"configurationManager.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());");

                    // string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
                }

                if (identity != null)
                {
                    // connect to the queue
                    var connectionString = ConfigurationHelper.GetClassSetting<App, string>("IotHubConnectionString", S_CONNECTIONSTRING_DEFAULT);
                    var queueName = ConfigurationHelper.GetClassSetting<App, string>("QueueName", S_QUEUENAME_DEFAULT);
                    var blobstorageConnectionString = ConfigurationHelper.GetClassSetting<App, string>("BlobstorageConnectionString", S_BLOBSTORAGECONNECTIONSTRING_DEFAULT);
                    var checkpointsContainer = ConfigurationHelper.GetClassSetting<App, string>("CheckpointsContainer", S_CHECKPOINTSCONTAINER_DEFAULT);
                    this.Dispatcher.Invoke(() =>
                    {
                        App.ConnectionString = connectionString;
                        //App.QueueName = queueName;
                        App.BlobstorageConnectionString = blobstorageConnectionString;
                        App.CheckpointsContainer = checkpointsContainer;
                        scope.LogDebug(new { App.ConnectionString, App.BlobstorageConnectionString, App.CheckpointsContainer });
                    });

                    //ServiceBusClient client = new ServiceBusClient(connectionString); // using 
                    //                                                                  // Create a Service Bus processor for the queue
                    //ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions()); // using 
                    //                                                                                                     // Register a message handler to process incoming messages
                    //processor.ProcessMessageAsync += ProcessMessageHandler;
                    //// Register an error handler to handle any errors that occur during message processing
                    //processor.ProcessErrorAsync += ErrorHandler;
                    //// Start processing messages
                    //await processor.StartProcessingAsync();

                    this.Dispatcher.Invoke(() =>
                    {
                        //this.Processor = processor;
                        //scope.LogDebug(new { this.Processor });
                    });
                }

                // connect to a topic
                if (identity != null)
                {
                    // Create a new instance of SubscriptionClient
                    var connectionString = default(string);
                    var topicName = "sampletopic";
                    var subscriptionName = "samplesubscription";

                    this.Dispatcher.Invoke(() =>
                    {
                        connectionString = App.ConnectionString;
                    });

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
            //RegreshCount += 1;
            BindingOperations.GetMultiBindingExpression(grdErrors, AttachedProperties.IsVisible0Property);
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

            //await using var producerClient = new EventHubProducerClient(App.ConnectionString, App.EventHubName); scope.LogDebug($"await using var producerClient = new EventHubProducerClient({App.ConnectionString}, {App.EventHubName});");
            var now = DateTime.Now;

            var connectionString = App.ConnectionString;
            string deviceId = "sampledevice_001";


            //var queueName = App.QueueName;
            if (deviceClient == null)
            {
                deviceClient = DeviceClient.CreateFromConnectionString(connectionString, deviceId);
                scope.LogDebug($"DeviceClient.CreateFromConnectionString(connectionString, deviceId); returned {deviceClient.GetLogString()}");
            }

            //var messageString = $"{now:HH:mm:ss.fff}: Sample message from {deviceId} to IotHub";
            //var message = new Message(Encoding.ASCII.GetBytes(messageString));
            //await deviceClient.SendEventAsync(message); scope.LogDebug($"await deviceClient.SendEventAsync({message});");
            //await deviceClient.CloseAsync();
            while (true)
            {
                var receiveMessage = await deviceClient.ReceiveAsync();
                if (receiveMessage == null) { continue; }

                var message = Encoding.ASCII.GetString(receiveMessage.GetBytes());
                scope.LogDebug(message);
                this.Dispatcher.Invoke(() =>
                {
                    this.Output = $"{now:HH:mm:ss.fff} Received message: {message}\r\n{this.Output}";
                });
                await deviceClient.CompleteAsync(receiveMessage);


            }


        }

        #region Identity
        public Identity Identity
        {
            get { return (Identity)GetValue(IdentityProperty); }
            set { SetValue(IdentityProperty, value); }
        }
        public static readonly DependencyProperty IdentityProperty = DependencyProperty.Register("Identity", typeof(Identity), typeof(MainControl), new PropertyMetadata());
        #endregion
        //#region Processor
        //public ServiceBusProcessor Processor
        //{
        //    get { return (ServiceBusProcessor)GetValue(ProcessorProperty); }
        //    set { SetValue(ProcessorProperty, value); }
        //}
        //public static readonly DependencyProperty ProcessorProperty = DependencyProperty.Register("Processor", typeof(ServiceBusProcessor), typeof(MainControl), new PropertyMetadata());
        //#endregion

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
            if (authenticationHelper == null) { return; }

            await authenticationHelper.LogoutAsync();
            var identity = await authenticationHelper.LoginAsync();

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
                SharedTokenCacheUsername = identity.Upn,
                ExcludeInteractiveBrowserCredential = false,
                ExcludeSharedTokenCacheCredential = false,
                ExcludeAzureCliCredential = false,
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true
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

            //App.ConnectionString = ConfigurationHelper.GetClassSetting<App, string>("ServiceBusConnectionString", S_CONNECTIONSTRING_DEFAULT);
            //App.QueueName = ConfigurationHelper.GetClassSetting<App, string>("QueueName", S_QUEUENAME_DEFAULT);
            //App.BlobstorageConnectionString = ConfigurationHelper.GetClassSetting<App, string>("BlobstorageConnectionString", S_BLOBSTORAGECONNECTIONSTRING_DEFAULT);
            //App.CheckpointsContainer = ConfigurationHelper.GetClassSetting<App, string>("CheckpointsContainer", S_CHECKPOINTSCONTAINER_DEFAULT);

            ////string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            //// Create a blob container client that the event processor will use 
            //BlobContainerClient storageClient = new BlobContainerClient(App.BlobstorageConnectionString, App.CheckpointsContainer);

            ////// Create an event processor client to process events in the event hub

            ////// Create a Service Bus processor options object
            ////var connectionString = App.ConnectionString;
            ////var queueName = App.QueueName;

            ////await using ServiceBusClient client = new ServiceBusClient(connectionString);
            ////// Create a Service Bus processor for the queue
            ////await using ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
            ////// Register a message handler to process incoming messages
            ////processor.ProcessMessageAsync += ProcessMessageHandler;
            ////// Register an error handler to handle any errors that occur during message processing
            ////processor.ProcessErrorAsync += ErrorHandler;

            ////// Start processing messages
            ////await processor.StartProcessingAsync();

            CommandManager.InvalidateRequerySuggested();
        }
        private void LogoutCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private async void LogoutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope();

            if (authenticationHelper == null) { return; }
            await authenticationHelper.LogoutAsync(); scope.LogDebug($"await authenticationHelper.LogoutAsync(); completed");
            this.Identity = null;

            var message = this.GetResourceValue<string>("Info.PressLogin", "Press Login to enter your credentials");
            var exception = new ClientException(message) { Code = ExceptionCodes.PRESSLOGIN };
            ExceptionManager.RaiseException(this, exception);
            CommandManager.InvalidateRequerySuggested();
        }

        // Events
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { sender = sender.GetLogString(), e = e.GetLogString() });

        }
        // Message handler method
        //async Task ProcessMessageHandler(ProcessMessageEventArgs args)
        //{
        //    using var scope = logger.BeginMethodScope(new { args = args.GetLogString() });
        //    var now = DateTime.Now;

        //    // Get the received message
        //    ServiceBusReceivedMessage message = args.Message;

        //    this.Dispatcher.Invoke(() =>
        //    {
        //        Console.WriteLine("\tReceived message: {0}", message);
        //        this.Output = $"{now:HH:mm:ss.fff} Received message: {message.Body}\r\n{this.Output}";
        //    });

        //    // Process the message
        //    Console.WriteLine($"Received message: {message.Body}");

        //    // Complete the message to remove it from the queue
        //    await args.CompleteMessageAsync(message);
        //}
        //// Error handler method
        //Task ErrorHandler(ProcessErrorEventArgs args)
        //{
        //    using var scope = logger.BeginMethodScope(new { args = args.GetLogString() });

        //    this.Dispatcher.Invoke(() =>
        //    {
        //        var output = $"\tAn unhandled exception was encountered.\r\n{args.Exception.Message}.\r\n{this.Output}";
        //        this.Output = output;
        //    });

        //    // Handle the error
        //    //Console.WriteLine($"Error occurred: {args.Exception}");

        //    return Task.CompletedTask;
        //}
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
        private object getIdentityDescription_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var identity = value as Identity;
            if (identity == null) { return "login"; }
            if (!string.IsNullOrEmpty(identity.Name)) { return identity.Name; }
            if (!string.IsNullOrEmpty(identity.Upn)) { return identity.Upn; }

            return identity.Email;
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
