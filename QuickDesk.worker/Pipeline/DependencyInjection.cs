using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Pipeline
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPipeline(this IServiceCollection services)
        {
            services.AddSingleton<ValidateJobStep>();
            services.AddSingleton<JobRunningStep>();
            services.AddSingleton<AddEmailLogStep>();
            return services;
        }   
    }
}
