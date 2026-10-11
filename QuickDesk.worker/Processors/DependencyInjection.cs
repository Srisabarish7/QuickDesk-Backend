using QuickDesk.worker.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Processors
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProcessors(this IServiceCollection services)
        {
            services.AddSingleton<IJobTypeProcessor, SendEmailProcessor>();
            services.AddSingleton<IJobTypeProcessor, AddNotesProcessor>();
            services.AddSingleton<IJobTypeProcessor, ExportEmailProcessor>();

            return services;
        }
    }
}
