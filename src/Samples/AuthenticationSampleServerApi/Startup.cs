using Diginsight;
using Diginsight.AspNetCore;
//using Diginsight.Strings;
using RestSharp;
//using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.Json.Serialization;
using Diginsight.SmartCache.Externalization.ServiceBus;
using Diginsight.SmartCache;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Diginsight.SmartCache.Externalization.AspNetCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using System.Reflection;
using Microsoft.Identity.Web;
using Diginsight.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Diginsight.Stringify;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Diginsight.SmartCache.Externalization.Http;
using Diginsight.Components.Configuration;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Options;

namespace AuthenticationSampleServerApi
{
    public class Startup
    {
        private static readonly string SmartCacheServiceBusSubscriptionName = Guid.NewGuid().ToString("N");
        private readonly IConfiguration configuration;
        private readonly IHostEnvironment hostEnvironment;
        private readonly EarlyLoggingManager observabilityManager;
        private readonly ILoggerFactory loggerFactory;

        public Startup(IConfiguration configuration,
            IHostEnvironment hostEnvironment, EarlyLoggingManager observabilityManager)
        {
            this.configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.observabilityManager = observabilityManager;
            loggerFactory = observabilityManager.LoggerFactory;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            using var activity = Observability.ActivitySource.StartMethodActivity(logger, new { services });

            services.AddAspNetCoreObservability(configuration, hostEnvironment, out IOpenTelemetryOptions openTelemetryOptions);
            observabilityManager.AttachTo(services);

            services.AddHttpObservability(openTelemetryOptions);

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            if (openTelemetryOptions.EnableTraces)
            {
                services.AddDiginsightOpenTelemetry()
                    .WithTracing(
                        static tracerProviderBuilder =>
                        {
                            tracerProviderBuilder.AddHttpClientInstrumentation(
                                static options =>
                                {
                                    var enrichWithHttpRequestMessage = options.EnrichWithHttpRequestMessage;
                                    options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                                    {
                                        enrichWithHttpRequestMessage?.Invoke(activity, httpRequestMessage);

                                        if (activity?.FindLabeledParent("Controller")?.GetTagItem("widget_template") is { } widgetTemplateName)
                                        {
                                            activity.SetTag("widget_template", widgetTemplateName);
                                        }
                                    };
                                }
                            );
                        }
                    );
            }

            services.AddDiginsightOpenTelemetry().WithTracing(b => b.SetSampler(new AlwaysOnSampler()));
            //services.TryAddEnumerable(ServiceDescriptor.Singleton<IActivityListenerRegistration, ControllerActivityTaggerRegistration>());

            services.AddDynamicLogLevel<DefaultDynamicLogLevelInjector>();

            //services.AddMicrosoftIdentityWebApiAuthentication(this.configuration);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(configuration); //.AddJwtBearer() 
            services.AddAuthorization();

            IdentityModelEventSource.ShowPII = true;

            services.ConfigureClassAware<ConcurrencyOptions>(configuration.GetSection("FeatureManagement"))
                .DynamicallyConfigure<ConcurrencyOptions>()
                .VolatilelyConfigure<ConcurrencyOptions>();

            //services.ConfigureClassAware<GeneralOptions>(this.configuration.GetSection("AppSettings"))
            //    .VolatilelyConfigure<GeneralOptions>()
            //    .DynamicallyConfigureClassAware<GeneralOptions>();

            services.ConfigureClassAware<FeatureFlagOptions>(configuration.GetSection("FeatureManagement"))
                    .DynamicallyConfigureClassAware<FeatureFlagOptions>()
                    .VolatilelyConfigure<ConcurrencyOptions>();

            // configure type contracts for log string rendering
            static void ConfigureTypeContracts(StringifyTypeContractAccessor accessor)
            {
                accessor.GetOrAdd<RestResponse>(
                    static typeContract =>
                    {
                        typeContract.GetOrAdd(static x => x.Request, static mc => mc.Included = false);
                        typeContract.GetOrAdd(static x => x.ResponseStatus, static mc => mc.Order = 1);
                        //typeContract.GetOrAdd(static x => x.Content, static mc => mc.Order = 1);
                    }
                );
            }
            StringifyContextFactoryBuilder.DefaultBuilder.ConfigureContracts(ConfigureTypeContracts);
            services.Configure<StringifyTypeContractAccessor>(ConfigureTypeContracts);

            //services
            //.AddAnalysis()
            //.Configure<AnalysisOptions>(configuration.GetSection("Observability:Analysis"));

            //services.Configure<LogStringTypeContractAccessor>(ConfigureTypeContracts);
            services.ConfigureRedisCacheSettings(configuration);

            services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = ApiVersions.V_2024_04_26.Version;
                opt.AssumeDefaultVersionWhenUnspecified = true;

                // ToDo: add error response (opt.ErrorResponses)
            });

            services.AddControllers()
                .AddControllersAsServices()
                .ConfigureApiBehaviorOptions(opt =>
                {
                    opt.SuppressModelStateInvalidFilter = true;
                })
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    opt.JsonSerializerOptions.WriteIndented = true;

                    //opt.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseJsonNamingPolicy();
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .AddMvcOptions(opt =>
                {
                    opt.MaxModelValidationErrors = 25;
                    //opt.Conventions.Add(new DataExportConvention() as IControllerModelConvention);
                    //opt.Conventions.Add(new DataExportConvention() as IActionModelConvention);
                });

            //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            SmartCacheBuilder smartCacheBuilder = services.AddSmartCache(configuration, hostEnvironment, loggerFactory)
                            .AddHttp();

            IConfigurationSection smartCacheServiceBusConfiguration = configuration.GetSection("Diginsight:SmartCache:ServiceBus");
            if (!string.IsNullOrEmpty(smartCacheServiceBusConfiguration[nameof(SmartCacheServiceBusOptions.ConnectionString)]) &&
                !string.IsNullOrEmpty(smartCacheServiceBusConfiguration[nameof(SmartCacheServiceBusOptions.TopicName)]))
            {
                smartCacheBuilder.SetServiceBusCompanion(
                    static (c, _) =>
                    {
                        IConfiguration sbc = c.GetSection("Diginsight:SmartCache:ServiceBus");
                        return !string.IsNullOrEmpty(sbc[nameof(SmartCacheServiceBusOptions.ConnectionString)])
                            && !string.IsNullOrEmpty(sbc[nameof(SmartCacheServiceBusOptions.TopicName)]);
                    },
                    sbo =>
                    {
                        configuration.GetSection("Diginsight:SmartCache:ServiceBus").Bind(sbo);
                        sbo.SubscriptionName = SmartCacheServiceBusSubscriptionName;
                    });
            }
            services.TryAddSingleton<ICacheKeyProvider, MyCacheKeyProvider>();
            //services.TryAddSingleton<IActivityTagger, ActivityTagger>();

            IsSwaggerEnabled = configuration.GetValue<bool>("IsSwaggerEnabled");
            if (IsSwaggerEnabled)
            {
                services.AddSwaggerDocumentation();
            }
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            using var innerActivity = Observability.ActivitySource.StartMethodActivity(logger, new { app, env });

            if (env.IsDevelopment())
            {
                //IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //app.UseAnalysis();

            //app.UseOpenTelemetryPrometheusScrapingEndpoint();

            if (IsSwaggerEnabled)
            {
                app.UseSwaggerDocumentation();
                //app.UseSwagger(); scope.LogDebug($"app.UseSwagger();");
                //app.UseSwaggerUI(options => options.OAuthClientId(builder.Configuration["SwaggerAuthentication:WebAppClientId"])); scope.LogDebug($"app.UseSwaggerUI(options => options.OAuthClientId(builder.Configuration[\"SwaggerAuthentication:WebAppClientId\"]));");
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            //app.UseCors();

            app.UseAuthentication(); // If you have this, it should be before UseAuthorization
            app.UseAuthorization();  // Make sure this is between UseRouting and UseEndpoints

            //app.UseMiddleware<HandleExceptionsMiddleware>(); scope.LogDebug($"app.UseMiddleware<HandleExceptionsMiddleware>();");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapVolatileConfiguration();
                endpoints.MapControllers();
            });

        }
        private bool IsSwaggerEnabled { get; set; }

    }
}
