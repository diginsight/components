using Diginsight.AspNetCore;
using Diginsight.Components.Configuration;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace AuthenticationSampleApi;

public class Program
{
    public static ObservabilityManager ObservabilityManager;

    public static void Main(string[] args)
    {
        using var observabilityManager = new ObservabilityManager();
        ObservabilityManager = observabilityManager;
        ILogger logger = ObservabilityManager.LoggerFactory.CreateLogger(typeof(Program));

        IWebHost host;
        using (var activity = Observability.ActivitySource.StartMethodActivity(logger, new { args }))
        {
            host = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration2(ObservabilityManager.LoggerFactory)
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    var logger = ObservabilityManager.LoggerFactory.CreateLogger<Startup>();
                    using var innerActivity = Observability.ActivitySource.StartRichActivity(logger, "ConfigureServicesCallback", new { services });

                    

                })
                .UseDiginsightServiceProvider(true)
                .Build();

            logger.LogDebug("Host built");
        }

        host.Run();
    }
}