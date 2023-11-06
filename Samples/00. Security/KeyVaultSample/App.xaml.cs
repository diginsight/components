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
using Azure.Messaging.EventHubs;
using System.ComponentModel;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Azure.ResourceManager.Resources;
using Common.PresentationBase;
using Common.SmartCache;
using System.Security.Cryptography;
using System.Windows.Input;
using Microsoft.Identity.Client;
using ApplicationBase = Common.ApplicationBase;
using System.Diagnostics;
using Microsoft.Azure.Amqp.Framing;
using System.Text.RegularExpressions;
//using Microsoft.Graph.Models.ExternalConnectors;
//using Azure.Extensions.AspNetCore.Configuration.Secrets;
#endregion

namespace KeyVaultSample
{
    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App : ApplicationBase, IProvideLogString
    {
        static Type T = typeof(App);
        #region constants
        const string S_PATH_DEFAULT = @"";
        const string S_MESSAGE_DEFAULT = @"This is a test message.";
        const string S_DESCRIPTION_DEFAULT = @"This is a test message description.";
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
        //public static readonly ActivitySource ActivitySource = new("KeyVaultSample");

        private ILogger<App> logger;
        //public IHost Host { get; set; }
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
        #region Scopes
        public string Scopes
        {
            get { return GetValue(() => Scopes); }
            set { SetValue(() => Scopes, value); }
        }
        #endregion
        #region AppVersion
        public string AppVersion
        {
            get { return GetValue(() => AppVersion); }
            set { SetValue(() => AppVersion, value); }
        }
        #endregion
        #region OauthVersionSuffix
        public string OauthVersionSuffix
        {
            get { return GetValue(() => OauthVersionSuffix); }
            set { SetValue(() => OauthVersionSuffix, value); }
        }
        #endregion

        #region AppInsightKey
        public string AppInsightKey
        {
            get { return GetValue(() => AppInsightKey); }
            set { SetValue(() => AppInsightKey, value); }
        }
        #endregion
        #region KeyVaultAddress
        public string KeyVaultAddress
        {
            get { return GetValue(() => KeyVaultAddress); }
            set { SetValue(() => KeyVaultAddress, value); }
        }
        #endregion

        #region .ctor
        public App()
        {
            //using var scope = logger.BeginMethodScope();
            using var scope = TraceLogger.ActivitySource.StartMethodActivity(logger);
            //this.Activated += App_Activated;
            //this.LoadCompleted += App_LoadCompleted;
            LogStringExtensions.RegisterLogstringProvider(this);
            LogStringExtensions.RegisterLogstringProvider(new LogStringProviderWpf());
        }
        #endregion

        protected override async void OnStartup(StartupEventArgs e)
        {
            //using var scope = logger.BeginMethodScope(new { e });
            using var scope = TraceLogger.ActivitySource.StartMethodActivity(logger, new { e });

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

            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        var environmentName = context?.HostingEnvironment?.EnvironmentName;
                        scope.LogDebug(new { environmentName }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("User", App.GetUser()), ("MaxMessageLen", 0) }));
                        if (environmentName != null) { configurationManager.AddJsonFile($"appsettings.{environmentName}.json", true, reloadOnChange: true); }

                        builder.Sources.Clear();
                        builder.AddConfiguration(configuration);

                        //builder.UseObservability();

                        // builder.AddUserSecrets(this.GetType().Assembly);
                        // var secretClient = new SecretClient(new Uri(keyVaultAddress), new DefaultAzureCredential());
                        // builder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                    }).ConfigureServices((context, services) =>
                    {
                        ConfigureServices(context.Configuration, services, context.HostingEnvironment);
                    })
                    .ConfigureLogging((context, loggingBuilder) =>
                    {
                        loggingBuilder.ClearProviders();

                        var options = new Log4NetProviderOptions();
                        options.Log4NetConfigFileName = "log4net.config";
                        var log4NetProvider = new Log4NetProvider(options);
                        loggingBuilder.AddDiginsightFormatted(log4NetProvider, configuration);

                        //var options1 = new Log4NetProviderOptions();
                        //options1.Log4NetConfigFileName = "log4netJson.config";
                        //var log4NetProvider1 = new Log4NetProvider(options1);
                        //loggingBuilder.AddDiginsightJson(log4NetProvider1, configuration);

                        // add training logic
                        configuration = context.Configuration;
                        bool enablePreloading = configuration.GetValue<bool>("AppSettings:EnablePreloading", false);
                        if (enablePreloading)
                        {
                            var trainingProvider = new SmartCacheTrainingProvider();
                            loggingBuilder.AddProvider(trainingProvider);

                            var preloadingProvider = new SmartCachePreloadingProvider();
                            loggingBuilder.AddProvider(preloadingProvider);
                        }

                        // TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(appInsightKey);
                        // ApplicationInsightsLoggerOptions appinsightOptions = new ApplicationInsightsLoggerOptions();
                        // var tco = Options.Create<TelemetryConfiguration>(telemetryConfiguration);
                        // var aio = Options.Create<ApplicationInsightsLoggerOptions>(appinsightOptions);
                        // loggingBuilder.AddDiginsightJson(new ApplicationInsightsLoggerProvider(tco, aio), configuration);

                        // loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
                    });

            Host = hostBuilder.Build();

            Host.InitTraceLogger();

            logger = Host.GetLogger<App>();

            var tl = Host.Services.GetService<TraceLogger>();

            // LogStringExtensions.RegisterLogstringProvider(this);
            await Host.StartAsync(); scope.LogDebug($"await Host.StartAsync();");

            // registers 
            var mainWindow = Host.Services.GetRequiredService<Window>(); scope.LogDebug($"Host.Services.GetRequiredService<Window>(); returned {mainWindow}");
            this.MainWindow = mainWindow;

            // Restore Window State 
            mainWindow.Show(); scope.LogDebug($"mainWindow.Show();");

            base.OnStartup(e); scope.LogDebug($"base.OnStartup(e);");
        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services, IHostEnvironment hostEnvironment)
        {
            //using var scope = logger.BeginMethodScope(new { configuration, services });
            using var scope = TraceLogger.ActivitySource.StartMethodActivity(logger, new { configuration, services });

            ConfigurationHelper.Init(configuration); // set full configuration with secrets 
                                                     //var logger = Host.GetLogger<App>();

            //object value = services.AddHttpContextAccessor();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IParallelService, ParallelService>();
            services.AddClassConfiguration();

            var aiConnectionString = configuration.GetValue<string>(Constants.APPINSIGHTSCONNECTIONSTRING);
            var assembly = this.GetType().Assembly;
            //services.AddObservability(aiConnectionString, assembly.GetName().Name, $"{assembly.GetName().Name}.{assembly.GetName().Version}", assembly.FullName);


            services.AddCacheService(configuration, hostEnvironment);

            services.AddSingleton<IGraphAPIClientHttp, GraphAPIClientHttp>();
            services.AddSingleton<IArmAPIClient, ArmAPIClient>();

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

            services.AddSingleton<AuthenticationService>((IServiceProvider provider) =>
            {

                TenantId = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_TENANTID, DEFAULTVALUE_TENANTID); // , CultureInfo.InvariantCulture
                ClientId = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_CLIENTID, DEFAULTVALUE_CLIENTID); // , CultureInfo.InvariantCulture
                AppName = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_APPNAME, DEFAULTVALUE_APPNAME); // , CultureInfo.InvariantCulture SettingAccessType.SecretWithCredential, 
                AppVersion = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_APPVERSION, DEFAULTVALUE_APPVERSION); // , CultureInfo.InvariantCulture
                RedirectUri = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_REDIRECTURI, DEFAULTVALUE_REDIRECTURI); // , CultureInfo.InvariantCulture
                Scopes = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_SCOPES, DEFAULTVALUE_SCOPES); // , CultureInfo.InvariantCulture
                OauthVersionSuffix = ConfigurationHelper.GetClassSetting<MainControl, string>(CONFIGVALUE_OAUTHVERSIONSUFFIX, DEFAULTVALUE_OAUTHVERSIONSUFFIX); // , CultureInfo.InvariantCulture
                AppInsightKey = ConfigurationHelper.GetClassSetting<App, string>(CONFIGVALUE_APPINSIGHTSKEY, DEFAULTVALUE_APPINSIGHTSKEY); // , CultureInfo.InvariantCulture SettingAccessType.SecretWithCredential, 

                //scope.LogDebug(new { ClientId, AppName, AppVersion, Scopes = Scopes.GetLogString(), OauthVersionSuffix, AppInsightKey }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("MaxMessageLen", 0) }));
                scope.LogDebug(new { ClientId }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("User", App.GetUser()), ("MaxMessageLen", 0) }));
                scope.LogDebug(new { AppName }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("User", App.GetUser()), ("MaxMessageLen", 0) }));
                scope.LogDebug(new { AppVersion }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("User", App.GetUser()), ("MaxMessageLen", 0) }));
                scope.LogDebug(new { Scopes = Scopes.GetLogString() }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("User", App.GetUser()), ("MaxMessageLen", 0) }));
                scope.LogDebug(new { OauthVersionSuffix }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("User", App.GetUser()), ("MaxMessageLen", 0) }));
                scope.LogDebug(new { AppInsightKey }, properties: PROPS.Get(new[] { ("Tags", "Event,Variables" as object), ("User", App.GetUser()), ("MaxMessageLen", 0) }));

                var scopesArray = Scopes?.Split(',');

                var authenticationService = new AuthenticationService(TenantId, ClientId, AppName, AppVersion, RedirectUri, scopesArray, OauthVersionSuffix, Application.Current.MainWindow);
                return authenticationService;
            });


        }

        protected override async void OnExit(ExitEventArgs e)
        {
            //using var scope = logger.BeginMethodScope(new { e });
            using var scope = TraceLogger.ActivitySource.StartMethodActivity(logger, new { e });

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
            //using var scope = logger.BeginMethodScope(new { sender, e });
            using var scope = TraceLogger.ActivitySource.StartMethodActivity(logger, new { sender, e });

        }

        public string ToLogString(object t, HandledEventArgs arg)
        {
            switch (t)
            {
                //case Window w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                //case System.Windows.Controls.Button w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                //case PropertyChangedEventArgs w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                //case ExecutedRoutedEventArgs w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                //case RoutedUICommand w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case Thread w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case Microsoft.Graph.Models.Application w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case Identity w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case TenantData w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case TenantResource w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                case TokenCacheNotificationArgs w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);

                //case EventProcessorClient w: arg.Handled = true; return LogstringHelper.ToLogStringInternal(w);
                default:
                    break;
            }
            return null;
        }
    }
}
