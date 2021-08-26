using Doppler.AccountPlans.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAccountPlansRepository, AccountPlansRepository>();
            services.AddScoped<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
            return services;
        }
    }
}
