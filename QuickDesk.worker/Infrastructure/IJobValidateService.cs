using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public interface IJobValidateService
    {
        Task<bool> ValidateJobAsync(long jobId, CancellationToken cancellationToken); // ValidateJobId
        Task<bool> ValidateJobTypeAsync(long jobTypeId, CancellationToken cancellationToken); // ValidateJobItem
    }
}
