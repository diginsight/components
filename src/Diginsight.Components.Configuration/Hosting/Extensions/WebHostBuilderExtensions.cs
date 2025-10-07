using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

//using IHostBuilder = Microsoft.Extensions.Hosting.IHostBuilder;

namespace Diginsight.Components.Configuration;

public static class WebHostBuilderExtensions
{
    private static readonly Type T = typeof(WebHostBuilderExtensions);
    public static IWebHostBuilder ConfigureAppConfiguration2(this IWebHostBuilder hostBuilder, ILoggerFactory loggerFactory, Func<IDictionary<string, string>, bool>? tagsMatch = null)
    {
        Console.WriteLine("Starting ConfigureAppConfiguration2...");

        var logger = Observability.LoggerFactory.CreateLogger(T);
        using var activity = Observability.ActivitySource.StartMethodActivity(logger);

        return hostBuilder.ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) => HostBuilderExtensions.ConfigureAppConfiguration2(webHostBuilderContext.HostingEnvironment, configurationBuilder, tagsMatch));
    }
}


