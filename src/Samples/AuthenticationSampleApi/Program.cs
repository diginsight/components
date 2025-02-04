using Diginsight.AspNetCore;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace AuthenticationSampleApi;

public class Program
{
    public static void Main(string[] args)
    {
        using EarlyLoggingManager observabilityManager = new ObservabilityManager();
        ILogger logger = observabilityManager.LoggerFactory.CreateLogger(typeof(Program));

        IWebHost host;
        using (var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args }))
        {
            host = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration2(observabilityManager.LoggerFactory)
                .ConfigureServices(services =>
                {
                    services.TryAddSingleton(observabilityManager);
                })
                .UseStartup<Startup>()
                .UseDiginsightServiceProvider(true)
                .Build();

            logger.LogDebug("Host built");
        }

        host.Run();
    }
}