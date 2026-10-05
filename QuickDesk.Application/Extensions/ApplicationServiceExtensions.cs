using Microsoft.Extensions.DependencyInjection;
using QuickDesk.Application.Mapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QuickDesk.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            //Register AutoMapper
            services.AddAutoMapper(typeof(MapperProfile).Assembly);

            // Add application services here
            return services;
        }
    }
}
