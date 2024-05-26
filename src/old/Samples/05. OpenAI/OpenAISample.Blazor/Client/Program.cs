using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Hosting;
using Common.Core.Blazor;
using Newtonsoft.Json;
using System.Text.Json;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace OpenAISampleBlazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine($"currentDirectory:{currentDirectory}");
            builder.Configuration.SetBasePath(currentDirectory);
            builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

            builder.RootComponents.Add<App>("#app");

            builder.Logging.SetMinimumLevel(LogLevel.Trace);
            //// Gets the standard ILoggerProvider (i.e. the console provider)
            //var consoleProvider = serviceProvider.GetRequiredService<ILoggerProvider>();
            //Console.WriteLine($"loggerProvider: '{consoleProvider}'"); // default logger is WebAssemblyConsoleLogger

            // Creates a Trace logger provider
            var consoleProvider = new TraceLoggerConsoleProvider();
            var traceLoggerProvider = new TraceLoggerFormatProvider(builder.Configuration) { ConfigurationSuffix = "Console" };
            traceLoggerProvider.AddProvider(consoleProvider);
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(traceLoggerProvider); //i.e. builder.Services.AddSingleton(traceLoggerProvider);

            var serviceProvider = builder.Services.BuildServiceProvider();
            //serviceProvider = builder.Services.BuildServiceProvider();

            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            Console.WriteLine($"loggerFactory: '{loggerFactory}'");
            var logger = loggerFactory.CreateLogger<Program>();// gets a logger from the ILoggerFactory
            Console.WriteLine($"logger: '{logger}'");

            builder.Services.AddMsalAuthentication(options =>
            {
                using var scope = logger.BeginNamedScope("AddMsalAuthentication");

                var configuration = builder.Configuration;
                configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                
                var scopeForAccessToken = configuration["CallApi:ScopeForAccessToken"];
                scope.LogDebug(new { scopeForAccessToken });
                
                options.ProviderOptions.DefaultAccessTokenScopes.Add(scopeForAccessToken);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("User.Read");
            });

            var configuration = builder.Configuration;
            ConfigureServices(builder.Services, configuration);

            using (var scope = logger.BeginMethodScope())
            {
                builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

                var host = builder.Build();
                var ihost = new WebAssemblyIHostAdapter(host) as IHost;
                ihost.InitTraceLogger(); scope.LogDebug($"ihost.InitTraceLogger();");
                ihost.UseCommandManager(); // gives the application host to CommandManager
                //Common.ActivityExtensions.InitTraceLogger(host.Services);

                await host.RunAsync();
            }
        }

        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Application state.
            var applicationState = new ApplicationState();
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            };
            applicationState.JsonSerializerSettings = jsonSerializerSettings;
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true
            };
            applicationState.JsonSerializerOptions = options;
            var commandManager = new CommandManager();
            applicationState.CommandManager = commandManager;

            services.AddSingleton(typeof(IApplicationState), applicationState);
            services.AddSingleton(typeof(IConfiguration), configuration);
            var apiUrl = configuration["apiUrl"];

            services.AddScoped<CustomAuthorizationMessageHandler>();
            //services.AddHttpClient<ServerManager>(
            //    "RoleManagerApi",
            //    client => client.BaseAddress = new Uri(apiUrl)
            //).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

            //services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("RoleManagerApi"));
            //services.AddScoped<ServerManager>();


            //services.AddScoped(sp =>
            //{
            //    var client = new HttpClient { BaseAddress = new Uri(apiUrl) };
            //    return new ServerManager(null, client, configuration);
            //});
            //    s => new ServerManager(
            //        "RoleManagerApi",
            //        s.GetRequiredService<ILoggerFactory>().CreateLogger<ServerManager>(),
            //        s.GetRequiredService<IHttpClientFactory>().CreateClient("RoleManagerApi"),
            //        s.GetRequiredService<IConfiguration>()
            //));

            services.AddScoped<BrowserService>();
            services.AddBlazorise(options =>
            {
                //options.cha
                //options.ChangeTextOnKeyPress = true;
            }).AddBootstrapProviders()
            .AddFontAwesomeIcons();
        }
    }
}
