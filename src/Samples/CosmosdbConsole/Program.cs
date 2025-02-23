using Cocona;
using Cocona.Builder;
using CosmosdbConsole;
using Diginsight;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CosmosdbConsole;

internal class Program
{
    private static async Task Main(string[] args)
    {
        AppContext.SetSwitch("Azure.Experimental.EnableActivitySource", true);

        CoconaAppBuilder appBuilder = CoconaApp.CreateBuilder(args);

        IConfiguration configuration = appBuilder.Configuration;
        IServiceCollection services = appBuilder.Services;
        IHostEnvironment hostEnvironment = appBuilder.Environment;

        services.AddObservability(configuration, hostEnvironment);

        services.AddSingleton<Executor>();

        appBuilder.Host.UseDiginsightServiceProvider(true);
        using CoconaApp app = appBuilder.Build();

        Executor executor = app.Services.GetRequiredService<Executor>();
        app.AddCommand("loadjson", executor.StreamDocumentsJsonAsync);
        app.AddCommand("query", executor.QueryAsync);
        app.AddCommand("uploadjson", executor.UploadDocumentsJsonAsync);
        app.AddCommand("deletefromjson", executor.DeleteDocumentsFromJsonAsync);

        await app.RunAsync();
    }
}
