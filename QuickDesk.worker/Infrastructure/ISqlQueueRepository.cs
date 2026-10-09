using QuickDesk.worker.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public interface ISqlQueueRepository
    {
        Task<IEnumerable<QuickDeskWorkerItem>> ClaimPendingItemsAsync(int batchSize, CancellationToken cancellationToken);
        Task<int> ResetStuckItemsAsync(int timeoutInMinutes, CancellationToken cancellationToken);
        Task<QuickDeskWorkerItem> UpdateJobStatusAsync(long jobId, int jobStatusId, string? errorMessage, CancellationToken cancellationToken);
    }
}
