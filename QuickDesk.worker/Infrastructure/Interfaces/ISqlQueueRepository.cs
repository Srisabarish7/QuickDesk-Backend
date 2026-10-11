using QuickDesk.worker.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure.Interfaces
{
    public interface ISqlQueueRepository
    {
        Task<IEnumerable<QuickDeskWorkerItem>> ClaimPendingItemsAsync(int batchSize, CancellationToken cancellationToken);
        Task<int> ResetStuckItemsAsync(int timeoutInMinutes, CancellationToken cancellationToken);
        Task<QuickDeskWorkerItem> UpdateJobStatusAsync(long jobId, int jobStatusId,int progress, string? errorMessage, CancellationToken cancellationToken);
        Task<int> AddEmailLog(AddEmailLog addEmailLog, CancellationToken ct);
        Task<int> UpdateEmailLog(long jobId, string status, DateTime? sentAt, string? errorMessage, CancellationToken ct);
        Task<IReadOnlyList<EmailLogs>> GetEmailLogs(long userId, DateTime? startDate, DateTime? endDate, CancellationToken ct);
    }
}
