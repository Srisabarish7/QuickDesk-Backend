using QuickDesk.worker.Models;

namespace QuickDesk.worker.Infrastructure.Interfaces
{
    public interface ICsvExportService
    {
        Task<string> ExportEmailLogsAsync(IEnumerable<EmailLogs> emailLogs, string userName,long jobId, CancellationToken ct);
    }
}
