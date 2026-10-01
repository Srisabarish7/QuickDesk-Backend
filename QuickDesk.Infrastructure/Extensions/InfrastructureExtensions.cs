using Microsoft.Extensions.DependencyInjection;
using QuickDesk.Infrastructure.DbConnection;
using QuickDesk.Infrastructure.Interfaces;
using QuickDesk.Infrastructure.Repositories;
namespace QuickDesk.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services)
        {
            //Register Dependencies
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddTransient<IUserRepository, UserRepository>();

            return services;
        }
    }
}
