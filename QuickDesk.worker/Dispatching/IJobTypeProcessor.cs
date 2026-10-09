using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Models;
using QuickDesk.worker.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Dispatching
{
    public interface IJobTypeProcessor
    {
        JobTypeEnum JobTypeId { get; }

        Task<bool> ProcessJobAsync(ProcessingContext jobItem, CancellationToken cancellationToken);
    }
}
