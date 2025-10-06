using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

//using IHostBuilder = Microsoft.Extensions.Hosting.IHostBuilder;

namespace Diginsight.Components.Configuration;

public static class WebHostBuilderExtensions
{
    public static Type T = typeof(WebHostBuilderExtensions);
    public static IWebHostBuilder ConfigureAppConfiguration2(this IWebHostBuilder hostBuilder, ILoggerFactory loggerFactory, Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        Console.WriteLine("Starting ConfigureAppConfiguration2...");

        var logger = loggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);
        if (ObservabilityHelper.LoggerFactory == null) { ObservabilityHelper.LoggerFactory = loggerFactory; }

        return hostBuilder.ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) => HostBuilderExtensions.ConfigureAppConfiguration2(webHostBuilderContext.HostingEnvironment, configurationBuilder, ObservabilityHelper.LoggerFactory, tagsMatch));
    }
}


