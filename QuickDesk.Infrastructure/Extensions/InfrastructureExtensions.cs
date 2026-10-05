using Microsoft.Extensions.DependencyInjection;
using QuickDesk.Infrastructure.DbConnection;
using QuickDesk.Infrastructure.Interfaces;
using QuickDesk.Infrastructure.Repositories;
using QuickDesk.Infrastructure.Services;
namespace QuickDesk.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services)
        {
            //Register Dependencies
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<IUserRepository, UserRepository>();

            //Password Dependency Injection
            services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IJobRepository, JobRepository>();
            return services;
        }
    }
}
