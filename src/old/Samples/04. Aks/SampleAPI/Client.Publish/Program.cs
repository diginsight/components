#region using
using Common;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 
#endregion

namespace SampleAPI.Client.Publish
{
    public class Program
    {
        private static Type T = typeof(Program);

        public static void Main(string[] args)
        {
            using var scope = TraceLogger.BeginMethodScope(T);

            var builder = CreateHostBuilder(args)
                          .ConfigureLogging((context, loggingBuilder) =>
                           {
                               var scopeInner = TraceLogger.BeginNamedScope(T, "ConfigureLoggingCallback");

                               //loggingBuilder.ClearProviders();
                               loggingBuilder.ClearProviders(); scopeInner.LogDebug($"loggingBuilder.ClearProviders();");

                               // if local => 
                               //var traceLoggerConsoleProviderIsEnabledString = context.Configuration["AppSettings:TraceLoggerConsoleProvider.IsEnabled"];
                               //var ok = bool.TryParse(traceLoggerConsoleProviderIsEnabledString, out var traceLoggerConsoleProviderIsEnabled);
                               //if (traceLoggerConsoleProviderIsEnabled)
                               //{
                               var consoleProvider = new TraceLoggerConsoleProvider();
                               var traceLoggerProvider = new TraceLoggerFormatProvider(context.Configuration) { ConfigurationSuffix = "Console" };
                               traceLoggerProvider.AddProvider(consoleProvider);
                               loggingBuilder.AddProvider(traceLoggerProvider); // i.e. builder.Services.AddSingleton(traceLoggerProvider);
                                                                                //}

                               // LOG4NET
                               var log4NetProviderIsEnabledString = context.Configuration["AppSettings:Log4NetProvider.IsEnabled"];
                               var ok = bool.TryParse(log4NetProviderIsEnabledString, out var log4NetProviderIsEnabled);
                               if (log4NetProviderIsEnabled)
                               {
                                   var options = new Log4NetProviderOptions();
                                   options.Log4NetConfigFileName = "log4net.config";
                                   var log4NetProvider = new Log4NetProvider(options);
                                   loggingBuilder.AddDiginsightFormatted(log4NetProvider, context.Configuration); scopeInner.LogDebug($"loggingBuilder.AddDiginsightFormatted(log4NetProvider, context.Configuration);");
                               }

                               // AppInsight
                               var applicationInsightsLoggerProviderIsEnabledString = context.Configuration["AppSettings:ApplicationInsightsLoggerProvider.IsEnabled"];
                               ok = bool.TryParse(applicationInsightsLoggerProviderIsEnabledString, out var applicationInsightsLoggerProviderIsEnabled);
                               if (applicationInsightsLoggerProviderIsEnabled)
                               {
                                   var appInsightsKey = context.Configuration["AppSettings:AppInsightsKey"]; scopeInner.LogDebug(new { appInsightsKey });
                                   if (!string.IsNullOrEmpty(appInsightsKey))
                                   {
                                       TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(appInsightsKey);
                                       ApplicationInsightsLoggerOptions appinsightOptions = new ApplicationInsightsLoggerOptions();
                                       var tco = Options.Create<TelemetryConfiguration>(telemetryConfiguration);
                                       var aio = Options.Create<ApplicationInsightsLoggerOptions>(appinsightOptions);
                                       loggingBuilder.AddDiginsightJson(new ApplicationInsightsLoggerProvider(tco, aio), context.Configuration);
                                   }
                                   // loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Debug);
                               }

                               // DEBUG
                               var traceLoggerDebugProviderIsEnabledString = context.Configuration["AppSettings:TraceLoggerDebugProvider.IsEnabled"];
                               ok = bool.TryParse(traceLoggerDebugProviderIsEnabledString, out var traceLoggerDebugProviderIsEnabled);
                               if (traceLoggerDebugProviderIsEnabled)
                               {
                                   var debugProvider = new TraceLoggerDebugProvider();
                                   traceLoggerProvider = new TraceLoggerFormatProvider(context.Configuration) { ConfigurationSuffix = "Debug" };
                                   traceLoggerProvider.AddProvider(debugProvider);
                                   loggingBuilder.AddProvider(traceLoggerProvider); // i.e. builder.Services.AddSingleton(traceLoggerProvider);
                                   scopeInner.LogDebug($"loggingBuilder.AddProvider(traceLoggerProvider);");
                               }

                               var debugLoggerProviderIsEnabledString = context.Configuration["AppSettings:DebugLoggerProvider.IsEnabled"];
                               ok = bool.TryParse(debugLoggerProviderIsEnabledString, out var debugLoggerProviderIsEnabled);
                               if (debugLoggerProviderIsEnabled)
                               {
                                   var debugProvider = new DebugLoggerProvider();
                                   var traceLoggerProviderDebug = new TraceLoggerFormatProvider(context.Configuration) { ConfigurationSuffix = "Debug" };
                                   traceLoggerProviderDebug.AddProvider(debugProvider);
                                   loggingBuilder.AddProvider(traceLoggerProviderDebug); // i.e. builder.Services.AddSingleton(traceLoggerProvider);
                               }

                               // loggingBuilder.AddAzureWebAppDiagnostics(); // STREAMING LOG not working ?
                           });

            var host = builder.Build(); scope.LogDebug($"var host = builder.Build();");

            host.InitTraceLogger(); scope.LogDebug($"host.InitTraceLogger();");

            host.Run(); scope.LogDebug($"host.Run();");
        }

        #region CreateHostBuilder
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            using var scope = TraceLogger.BeginMethodScope(T);

            var builder = Host.CreateDefaultBuilder(args)
                              .ConfigureWebHostDefaults(webBuilder =>
                              {
                                  using (var scopeInner = TraceLogger.BeginNamedScope(T, "ConfigureWebHostDefaultsCallback"))
                                  {
                                      webBuilder.UseStartup<Startup>(); scopeInner.LogDebug($"webBuilder.UseStartup<Startup>(); completed");
                                      //webBuilder.UseUrls("http://localhost:8081/");
                                      //webBuilder.UseUrls("http://[::]:80/"); //, "https://localhost:44381"
                                      //webBuilder.Start();
                                  }
                              });

            return builder;
        }
        #endregion
    }
}
