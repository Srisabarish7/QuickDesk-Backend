using QuickDesk.Application.Extensions;
using QuickDesk.Infrastructure.Extensions;

namespace QuickDesk.Api.Extensions
{
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add API services here
            services.AddApplicationServices();
            services.AddInfrastructureExtensions();
            services.AddSwaggerExtension();
            services.AddAuthentication(configuration);

            return services;
        }
    }
}
