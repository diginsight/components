#region using
using Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Swashbuckle.Application;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
#endregion

namespace SampleAPI
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
        public void ConfigureServices(IServiceCollection services)
        {
            using var scope = _logger.BeginMethodScope(new { services = services.GetLogString() });

            //services
            //    .Configure<HttpClientOptions>(Configuration.GetSection("HttpClientOptions"));
            services.AddHttpClient(); scope.LogDebug($"services.AddHttpClient();");

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin();
                                      builder.AllowAnyHeader();
                                      builder.AllowAnyMethod();
                                  });
            });
            scope.LogDebug($"services.AddCors();");

            services.AddResponseCompression(); scope.LogDebug($"services.AddResponseCompression();");
            services.AddHttpContextAccessor(); scope.LogDebug($"services.AddHttpContextAccessor();");

            var isAuthenticationEnabledString = Configuration["AzureAd:Enabled"];
            var isAuthenticationEnabled = !string.IsNullOrEmpty(isAuthenticationEnabledString) && bool.Parse(isAuthenticationEnabledString); scope.LogDebug(new { isAuthenticationEnabled });

            if (isAuthenticationEnabled)
            {
                //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));
                services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                        .AddJwtBearer(options =>
                        {
                            // The identifier of the app which has been registered at the authorization server
                            options.Audience = Configuration["AzureAd:ClientId"];
                            // The jwt token issuer
                            options.Authority = $"{Configuration["AzureAd:Instance"]}{Configuration["AzureAd:TenantId"]}";
                            options.TokenValidationParameters.ValidateLifetime = true;
                            options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
                            options.Validate();
                        }); scope.LogDebug($"services.AddAuthentication().AddJwtBearer();");

                services.AddAuthorization(); scope.LogDebug($"services.AddAuthorization();");
            }

            services.AddHttpContextAccessor(); scope.LogDebug($"services.AddHttpContextAccessor();");
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>(); scope.LogDebug($"services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();");

            services.AddControllers().AddNewtonsoftJson(); scope.LogDebug($"services.AddControllers().AddNewtonsoftJson();");

            //services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SampleAPI", Version = "v1" });
            });
            scope.LogDebug($"services.AddSwaggerGen();");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using var scope = _logger.BeginMethodScope(new { app = app.GetLogString(), env = env.GetLogString() });

            scope.LogDebug($"env.IsDevelopment();");
            if (true || env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); scope.LogDebug($"app.UseDeveloperExceptionPage();");

                string endpointPrefix = "api", endpointVersion = "v1", endpointTitle = "Sample Service";
                app.UseSwagger(o =>
                {
                    o.RouteTemplate = $"{endpointPrefix}/{{documentName}}/openapi.json";
                }).UseSwaggerUI(o =>
                    {
                        o.SwaggerEndpoint($"{endpointVersion}/openapi.json", $"{endpointTitle} {endpointVersion}"); // 
                        o.RoutePrefix = endpointPrefix;
                    }
                );
            }
            //else
            //{
            //    app.UseExceptionHandler("/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            app.UseCors(); scope.LogDebug($"app.UseCors();");

            // app.UseHttpsRedirection(); scope.LogDebug($"app.UseHttpsRedirection();");

            app.UseRouting(); scope.LogDebug($"app.UseRouting();");

            var isAuthenticationEnabledString = Configuration["AzureAd:Enabled"];
            var isAuthenticationEnabled = !string.IsNullOrEmpty(isAuthenticationEnabledString) && bool.Parse(isAuthenticationEnabledString); scope.LogDebug(new { isAuthenticationEnabled });

            if (isAuthenticationEnabled)
            {
                app.UseAuthentication(); scope.LogDebug($"app.UseAuthentication();");
                app.UseAuthorization(); scope.LogDebug($"app.UseAuthorization();");
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // var endpointappsconventionBuilder {}
                //endpoints.Select().Filter().OrderBy().Expand().Count().MaxTop(10);
                //endpoints.EnableDependencyInjection();
                //endpoints.MapFallbackToFile($"/api/index.html");
            });
            scope.LogDebug($"app.UseEndpoints();");
        }
    }
}
