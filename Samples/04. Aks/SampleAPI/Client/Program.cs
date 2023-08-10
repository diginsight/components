#region using
using BlazorApplicationInsights;
using Common;
using dbo.app.Handlers;
using mcs.core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace SampleAPI.Client
{
    public class Program
    {
        private static Type T = typeof(Program);

        public static async Task Main(string[] args)
        {
            using var scope = TraceLogger.BeginMethodScope(T);

            var builder = WebAssemblyHostBuilder.CreateDefault(args); scope.LogDebug($"WebAssemblyHostBuilder.CreateDefault(args); returned {builder.GetLogString()}");
            var configuration = builder.Configuration; scope.LogDebug(new { configuration = configuration.GetLogString() });

            builder.RootComponents.Add<App>("#app"); scope.LogDebug($"builder.RootComponents.Add<App>('#app');");

            var serviceProvider = builder.Services.BuildServiceProvider(); scope.LogDebug($"builder.Services.BuildServiceProvider();");

            // Configuration.
            builder.Services.AddSingleton(typeof(IConfiguration), configuration); scope.LogDebug($"builder.Services.AddSingleton(typeof(IConfiguration), configuration);");

            builder.Logging.SetMinimumLevel(LogLevel.Debug); scope.LogDebug($"builder.Logging.SetMinimumLevel(LogLevel.Debug);");
            builder.Logging.ClearProviders(); scope.LogDebug($"builder.Logging.ClearProviders();");

            var consoleProvider = new TraceLoggerConsoleProvider();
            var traceLoggerProvider = new TraceLoggerFormatProvider(builder.Configuration) { ConfigurationSuffix = "Console" };
            traceLoggerProvider.AddProvider(consoleProvider);
            builder.Logging.AddProvider(traceLoggerProvider); scope.LogDebug($"builder.Logging.AddProvider(traceLoggerProvider);");

            var appInsightsKey = builder.Configuration["AzureMonitor:AppInsightsKey"]; scope.LogDebug($"appInsightsKey:{appInsightsKey}");
            var appInsights = new ApplicationInsights(async applicationInsights =>
            {
                await applicationInsights.SetInstrumentationKey(appInsightsKey);
                await applicationInsights.LoadAppInsights();
                var telemetryItem = new TelemetryItem()
                {
                    Tags = new Dictionary<string, object>()
                        {
                            { "ai.cloud.role", "SPA" },
                            { "ai.cloud.roleInstance", "Blazor Wasm" },
                        }
                };
                await applicationInsights.AddTelemetryInitializer(telemetryItem);
            });
            var appinsightProvider = new ApplicationInsightsLoggerProvider(appInsights);
            var appinsightJsonLoggerProvider = new TraceLoggerJsonProvider(builder.Configuration) { ConfigurationSuffix = "Appinsights" };
            var appinsightFormatLoggerProvider = new TraceLoggerFormatProvider(builder.Configuration) { ConfigurationSuffix = "Appinsights" };

            appinsightFormatLoggerProvider.AddProvider(appinsightProvider);
            builder.Logging.AddProvider(appinsightFormatLoggerProvider); scope.LogDebug($"builder.Logging.AddProvider(appinsightFormatLoggerProvider);");
            builder.Services.AddSingleton<IApplicationInsights, ApplicationInsights>(sp => appInsights); scope.LogDebug($"builder.Services.AddSingleton<IApplicationInsights, ApplicationInsights>(sp => appInsights);");

            serviceProvider = builder.Services.BuildServiceProvider(); scope.LogDebug($"builder.Services.BuildServiceProvider();");
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>(); scope.LogDebug($"serviceProvider.GetRequiredService<ILoggerFactory>(); returned {loggerFactory.GetLogString()}");

            // gets a logger from the ILoggerFactory
            var logger = loggerFactory.CreateLogger<Program>(); scope.LogDebug($"loggerFactory.CreateLogger<Program>(); returned {logger.GetLogString()}");

            await InitConfiguration(builder, logger); scope.LogDebug($"await InitConfiguration(builder, logger);");

            var logStringProvider = new LogStringProvider();
            builder.Services.AddSingleton(typeof(LogStringProvider), logStringProvider); scope.LogDebug($"builder.Services.AddSingleton(typeof(LogStringProvider), logStringProvider);");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); scope.LogDebug($"builder.Services.AddScoped(sp => new HttpClient ({ builder.HostEnvironment.BaseAddress }));");

            // MSAL authentication.
            builder.Services.AddMsalAuthentication(options =>
            {
                configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                // Add scopes.
                foreach (var scope in configuration["RoleManagerApi:Scopes"]?.Split(","))
                {
                    options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
                }
                options.ProviderOptions.LoginMode = "redirect";
            }); scope.LogDebug($"builder.Services.AddMsalAuthentication()");
            //builder.Services.AddMsalAuthentication(options =>
            //{
            //    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            //});

            //services.AddSingleton(typeof(ComponentHelper),
            //    sp => new ComponentHelper(sp.GetRequiredService<IState<IBlazorApplicationState>>())
            //);
            builder.Services.AddHttpContextAccessor(); scope.LogDebug($"builder.Services.AddHttpContextAccessor();");

            builder.Services.AddScoped<RoleManagerAuthorizationMessageHandler>(); scope.LogDebug($"builder.Services.AddScoped<RoleManagerAuthorizationMessageHandler>();");

            //// RoleManagerService
            var isAuthenticationEnabledString = configuration["AzureAd:Enabled"]; scope.LogDebug(new { isAuthenticationEnabledString });
            var isAuthenticationEnabled = !string.IsNullOrEmpty(isAuthenticationEnabledString) && bool.Parse(isAuthenticationEnabledString); scope.LogDebug(new { isAuthenticationEnabled });

            var httpBuilder = builder.Services.AddHttpClient<IServiceManager, RoleManagerService>(); scope.LogDebug($"builder.Services.AddHttpClient<IServiceManager, RoleManagerService>(); returned {httpBuilder.GetLogString()}");
            if (isAuthenticationEnabled) { httpBuilder.AddHttpMessageHandler<RoleManagerAuthorizationMessageHandler>(); scope.LogDebug($"httpBuilder.AddHttpMessageHandler<RoleManagerAuthorizationMessageHandler>();"); }

            //services.AddScoped<BrowserService>();
            //services.AddBlazorise(options =>
            //{
            //    options.ChangeTextOnKeyPress = true;
            //}).AddBootstrapProviders()
            //  .AddFontAwesomeIcons();


            var host = builder.Build(); scope.LogDebug($"builder.Build(); returned {host.GetLogString()}");
            var ihost = new WebAssemblyIHostAdapter(host) as IHost;
            //ihost.UseWebCommands();
            ihost.InitTraceLogger(); scope.LogDebug($"ihost.InitTraceLogger();");

            await host.RunAsync(); scope.LogDebug($"await host.RunAsync();");
            //}
        }

        private static async Task InitConfiguration(WebAssemblyHostBuilder builder, ILogger logger)
        {
            using var scope = TraceLogger.BeginMethodScope(T);

            var baseAddress = builder.HostEnvironment.BaseAddress; scope.LogDebug(new { baseAddress });
            var environment = builder.HostEnvironment.Environment; scope.LogDebug(new { environment });

            var http = new HttpClient() { BaseAddress = new Uri(baseAddress) };

            builder.Services.AddScoped(sp => http); scope.LogDebug($"builder.Services.AddScoped(sp => http);");

            try
            {
                using var response = await http.GetAsync($"appsettings.{environment}.json"); scope.LogDebug($"using var response = await http.GetAsync($'appsettings.{environment}.json');");
                using var stream = await response.Content.ReadAsStreamAsync();
                logger.LogDebug($"appsettings.{environment}.json downloaded.{Environment.NewLine}Size: {stream.Length} bytes.");
                builder.Configuration.AddJsonStream(stream); scope.LogDebug($"builder.Configuration.AddJsonStream(stream);");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
    public class LogStringProvider : IProvideLogString
    {
        public string ToLogString(object source, HandledEventArgs arg)
        {
            switch (source)
            {
                case ChangeEventArgs w: arg.Handled = true; return ToLogStringInternal(w);
                default:
                    break;
            }
            return null;
        }
        public static string ToLogStringInternal(ChangeEventArgs pthis)
        {
            string logString = $"{{{nameof(ChangeEventArgs)}:{{Value:{pthis.Value.GetLogString()}}}}}";
            return logString;
        }
    }
}
