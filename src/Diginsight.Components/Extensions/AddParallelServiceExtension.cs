using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Diginsight.Components
{
    public static class AddParallelServiceExtension
    {
        /// <summary>
        /// Extension method for registering cache related services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static IServiceCollection AddParallelService(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureClassAware<ParallelServiceOptions>(configuration.GetSection("Diginsight:Components"));
            services.AddSingleton<IParallelService, ParallelService>();

            return services;
        }
    }
}
