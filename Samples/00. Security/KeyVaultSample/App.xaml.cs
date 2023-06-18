#region using
//using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
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
using System.Configuration;
using System.Data;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
//using System.Drawing;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;
using Window = System.Windows.Window;
//using Azure.Extensions.AspNetCore.Configuration.Secrets;
#endregion

namespace KeyVaultSample
{
    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App : ApplicationBase
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

        #region TenantId
        public string TenantId
        {
            get { return GetValue(() => TenantId); }
            set { SetValue(() => TenantId, value); }
        }
        #endregion
        #region ClientId
        public string ClientId
        {
            get { return GetValue(() => ClientId); }
            set { SetValue(() => ClientId, value); }
        }
        #endregion
        #region ClientSecret
        public string ClientSecret
        {
            get { return GetValue(() => ClientSecret); }
            set { SetValue(() => ClientSecret, value); }
        }
        #endregion
        #region AppName
        public string AppName
        {
            get { return GetValue(() => AppName); }
            set { SetValue(() => AppName, value); }
        }
        #endregion
        #region RedirectUri
        public string RedirectUri
        {
            get { return GetValue(() => RedirectUri); }
            set { SetValue(() => RedirectUri, value); }
        }
        #endregion
        #region AppVersion
        public string AppVersion
        {
            get { return GetValue(() => AppVersion); }
            set { SetValue(() => AppVersion, value); }
        }
        #endregion
        #region KeyVaultAddress
        public string KeyVaultAddress
        {
            get { return GetValue(() => KeyVaultAddress); }
            set { SetValue(() => KeyVaultAddress, value); }
        }
        #endregion
        //#region BlobstorageConnectionString
        //public string BlobstorageConnectionString
        //{
        //    get { return GetValue(() => BlobstorageConnectionString); }
        //    set { SetValue(() => BlobstorageConnectionString, value); }
        //}
        //#endregion
        //#region CheckpointsContainer
        //public string CheckpointsContainer
        //{
        //    get { return GetValue(() => CheckpointsContainer); }
        //    set { SetValue(() => CheckpointsContainer, value); }
        //}
        //#endregion

        #region .ctor
        public App()
        {
            using var scope = logger.BeginMethodScope();
            //this.Activated += App_Activated;
            //this.LoadCompleted += App_LoadCompleted;
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

            //var keyVaultAddress = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_KEYVAULTADDRESS, DEFAULTVALUE_KEYVAULTADDRESS);
            //this.ConnectionString = this.KeyVaultAddress = keyVaultAddress;

            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        var environmentName = context?.HostingEnvironment?.EnvironmentName; scope.LogDebug(new { environmentName });
                        if (environmentName != null) { configurationManager.AddJsonFile($"appsettings.{environmentName}.json", true, reloadOnChange: true); }

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
                var assembly = typeof(ApplicationWindow).Assembly;
                var resourceName = "pack://application:,,,/Common.PresentationBase;component/Resources/Images/Azure/security/10245-icon-service-Key-Vaults.png";
                var icon = BitmapFrame.Create(Application.GetResourceStream(new Uri(resourceName, UriKind.RelativeOrAbsolute)).Stream);

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
                    },
                    Icon = icon
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
    }
}
