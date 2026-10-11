using QuickDesk.worker.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ISqlQueueRepository, SqlQueueRepository>();

            services.AddSingleton<IJobValidateService, JobValidateService>();

            services.AddSingleton<IEmailSenderService, EmailSenderService>();
            services.AddSingleton<IQueryService, QueryService>();
            services.AddSingleton<ICsvExportService, CsvExportService>();
            return services;
        }
    }
}
