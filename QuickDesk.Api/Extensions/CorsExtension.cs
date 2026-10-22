namespace QuickDesk.Api.Extensions
{
    public static class CorsExtension
    {
        public static IServiceCollection AddCorsExtension(this IServiceCollection services, IConfiguration configuration)
        {            
            services.AddCors(options =>
            {
                options.AddPolicy("AllowedOrigins", builder =>
                {
                    var allowedOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
            return services;
        }
    }
}
