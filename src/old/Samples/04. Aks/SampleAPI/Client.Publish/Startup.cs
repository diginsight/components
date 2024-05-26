#region using
using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq; 
#endregion

namespace SampleAPI.Client.Publish
{
    public class Startup
    {
        ILogger<Startup> _logger;

        public Startup(IConfiguration configuration)
        {
            using var scope = _logger.BeginMethodScope(new { configuration = configuration.GetLogString() });

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            using var scope = _logger.BeginMethodScope(new { services = services.GetLogString() });

            //services.AddControllersWithViews();
            //services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using var scope = _logger.BeginMethodScope(new { app = app.GetLogString(), env = env.GetLogString() });

            var isDevelopment = env.IsDevelopment();
            scope.LogDebug($"env.IsDevelopment(); returned {isDevelopment}");
            if (true || isDevelopment)
            {
                app.UseDeveloperExceptionPage(); scope.LogDebug($"app.UseDeveloperExceptionPage();");
                app.UseWebAssemblyDebugging(); scope.LogDebug($"app.UseWebAssemblyDebugging();");
            }
            else
            {
                //app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(); scope.LogDebug($"app.UseHsts();");
            }

            //app.UseHttpsRedirection(); scope.LogDebug($"app.UseHttpsRedirection();");
            app.UseBlazorFrameworkFiles(); scope.LogDebug($"app.UseBlazorFrameworkFiles();");
            app.UseStaticFiles(); scope.LogDebug($"app.UseStaticFiles();");

            app.UseRouting(); scope.LogDebug($"app.UseRouting();");

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                //endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
            scope.LogDebug($"app.UseEndpoints();");
        }
    }
}
