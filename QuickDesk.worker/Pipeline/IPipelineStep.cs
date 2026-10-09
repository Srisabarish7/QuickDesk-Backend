using QuickDesk.worker.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Pipeline
{
    public interface IPipelineStep
    {
        Task ExecuteAsync(ProcessingContext context, CancellationToken cancellationToken);
    }
}
