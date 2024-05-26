#region using
//using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Security.KeyVault.Secrets;
using Common;
//using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;
//using Azure.Extensions.AspNetCore.Configuration.Secrets;
#endregion

namespace IotHubSample
{
    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App : ApplicationBase, IProvideLogString
    {
        static Type T = typeof(App);
        #region constants
        const string CONFIGVALUE_KEYVAULTADDRESS = "KeyVaultAddress", DEFAULTVALUE_KEYVAULTADDRESS = "";
        const string S_PATH_DEFAULT = @"";
        const string S_MESSAGE_DEFAULT = @"This is a test message.";
        const string S_DESCRIPTION_DEFAULT = @"This is a test message description.";
        #endregion

        private ILogger<App> logger;
        public IHost Host { get; set; }
        public ConfigurationManager ConfigurationManager { get; set; }

        #region Message
        public string Message
        {
            get { return GetValue(() => Message); }
            set { SetValue(() => Message, value); }
        }
        #endregion
        #region Description
        public string Description
        {
            get { return GetValue(() => Description); }
            set { SetValue(() => Description, value); }
        }
        #endregion
        #region Path
        public string Path
        {
            get { return GetValue(() => Path); }
            set { SetValue(() => Path, value); }
        }
        #endregion
        #region ConnectionString
        public string ConnectionString
        {
            get { return GetValue(() => ConnectionString); }
            set { SetValue(() => ConnectionString, value); }
        }
        #endregion
        #region IotHubName
        public string IotHubName
        {
            get { return GetValue(() => IotHubName); }
            set { SetValue(() => IotHubName, value); }
        }
        #endregion
        #region BlobstorageConnectionString
        public string BlobstorageConnectionString
        {
            get { return GetValue(() => BlobstorageConnectionString); }
            set { SetValue(() => BlobstorageConnectionString, value); }
        }
        #endregion
        #region CheckpointsContainer
        public string CheckpointsContainer
        {
            get { return GetValue(() => CheckpointsContainer); }
            set { SetValue(() => CheckpointsContainer, value); }
        }
        #endregion

        #region .ctor
        public App()
        {
            using var scope = logger.BeginMethodScope();
            //this.Activated += App_Activated;
            //this.LoadCompleted += App_LoadCompleted;
            
            LogStringExtensions.RegisterLogstringProvider(this);
        }
        #endregion

        protected override async void OnStartup(StartupEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { e });

            var configurationManager = new ConfigurationManager(); // use .net 6 configuration manager to support partial configuration load
            var configurationBuilder = configurationManager as IConfigurationBuilder;
            configurationManager.AddJsonFile("appsettings.json", true, reloadOnChange: true);
            configurationManager.AddUserSecrets(this.GetType().Assembly);
            IConfiguration configuration = configurationBuilder.Build();
            this.ConfigurationManager = configurationManager;

            TraceLogger.InitConfiguration(configuration);
            ConfigurationHelper.Init(configuration);

            this.Message = ConfigurationHelper.GetClassSetting<App, string>("Message", S_MESSAGE_DEFAULT);
            this.Description = ConfigurationHelper.GetClassSetting<App, string>("Description", S_DESCRIPTION_DEFAULT);
            this.Path = ConfigurationHelper.GetClassSetting<App, string>("Path", S_PATH_DEFAULT);
            var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS);
            

            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(builder =>
                    {
                        builder.Sources.Clear();
                        builder.AddConfiguration(configuration);
                        // builder.AddUserSecrets(this.GetType().Assembly);
                        // var secretClient = new SecretClient(new Uri(keyVaultAddress), new DefaultAzureCredential());
                        // builder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                    }).ConfigureServices((context, services) =>
                    {
                        ConfigureServices(context.Configuration, services);
                    })
                    .ConfigureLogging((context, loggingBuilder) =>
                    {
                        loggingBuilder.ClearProviders();

                        var options = new Log4NetProviderOptions();
                        options.Log4NetConfigFileName = "log4net.config";
                        var log4NetProvider = new Log4NetProvider(options);
                        loggingBuilder.AddDiginsightFormatted(log4NetProvider, configuration);

                        // TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(appInsightKey);
                        // ApplicationInsightsLoggerOptions appinsightOptions = new ApplicationInsightsLoggerOptions();
                        // var tco = Options.Create<TelemetryConfiguration>(telemetryConfiguration);
                        // var aio = Options.Create<ApplicationInsightsLoggerOptions>(appinsightOptions);
                        // loggingBuilder.AddDiginsightJson(new ApplicationInsightsLoggerProvider(tco, aio), configuration);

                        // loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
                    }).Build();

            Host.InitTraceLogger();

            logger = Host.GetLogger<App>();

            // LogStringExtensions.RegisterLogstringProvider(this);
            await Host.StartAsync(); scope.LogDebug($"await Host.StartAsync();");

            var mainWindow = Host.Services.GetRequiredService<Window>(); scope.LogDebug($"Host.Services.GetRequiredService<Window>(); returned {mainWindow}");
            this.MainWindow = mainWindow;

            // Restore Window State 
            mainWindow.Show(); scope.LogDebug($"mainWindow.Show();");

            base.OnStartup(e); scope.LogDebug($"base.OnStartup(e);");
        }
        
        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            using var scope = logger.BeginMethodScope(new { configuration, services });
            ConfigurationHelper.Init(configuration); // set full configuration with secrets 
            //var logger = Host.GetLogger<App>();

            services.AddSingleton<Window>((IServiceProvider provider) =>
            {
                var mainControl = new MainControl();
                var applicationWindow = new ApplicationWindow(mainControl)
                {
                    Width = 800,
                    Height = 450,
                    //ResizeMode = ResizeMode.CanResize,
                    //WindowState = WindowState.Normal,
                    //WindowStyle = WindowStyle.None,
                    DragMode = WindowBase.WindowDragMode.Full,
                    WindowChrome = new System.Windows.Shell.WindowChrome()
                    {
                        CaptionHeight = 0,
                        ResizeBorderThickness = new Thickness(5)
                    }
                };
                applicationWindow.Name = mainControl.Name;
                return applicationWindow;
            });


        }
        
        protected override async void OnExit(ExitEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { e });
            //var logger = Host.GetLogger<App>();
            using (Host)
            {
                await Host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //var logger = Host.GetLogger<App>();
            using var scope = logger.BeginMethodScope(new { sender, e });

        }

        public string ToLogString(object t, HandledEventArgs arg)
        {
            switch (t)
            {
                case Window w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case Button w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case PropertyChangedEventArgs w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case Thread w:arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case EventProcessorClient w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case Identity w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                default:
                    break;
            }
            return null;
        }
    }
}
