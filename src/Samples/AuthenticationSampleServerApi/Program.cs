using Diginsight.AspNetCore;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace AuthenticationSampleServerApi;

public class Program
{
    public static IDeferredLoggerFactory DeferredLoggerFactory = default!;

    public static void Main(string[] args)
    {
        var activitiesOptions = new DiginsightActivitiesOptions() { LogActivities = true };
        DeferredLoggerFactory = new DeferredLoggerFactory(activitiesOptions: activitiesOptions);
        DeferredLoggerFactory.ActivitySourceFilter = (activitySource) => true; // activitySource.Name.StartsWith($"AuthenticationSampleServerApi") || activitySource.Name.StartsWith($"Diginsight.Components")
        var logger = DeferredLoggerFactory.CreateLogger<Program>();

        IWebHost host;
        using (var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args }))
        {
            host = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration2(DeferredLoggerFactory)
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var logger = DeferredLoggerFactory.CreateLogger<Startup>();
                    using var innerActivity = Observability.ActivitySource.StartRichActivity(logger, "ConfigureServicesCallback", new { services });

                    services.TryAddSingleton(DeferredLoggerFactory);
                })
                .UseDiginsightServiceProvider()
                .Build();

            logger.LogDebug("Host built");
        }

        host.Run();
    }
}