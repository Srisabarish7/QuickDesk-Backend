using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Infrastructure;
using QuickDesk.worker.Models;
using QuickDesk.worker.Models.Pipeline;

namespace QuickDesk.worker.Pipeline
{
    public sealed class JobRunningStep : IPipelineStep
    {
        private readonly ILogger<JobRunningStep> _logger;
        private readonly ISqlQueueRepository _sqlQueueRepository;
        public JobRunningStep(ILogger<JobRunningStep> logger, ISqlQueueRepository sqlQueueRepository)
        {
            _logger = logger;
            _sqlQueueRepository = sqlQueueRepository;
        }

        public async Task ExecuteAsync(ProcessingContext context, CancellationToken cancellationToken)
        {
            if (context.Errors.Any()) 
            {
                _logger.LogWarning("Job {JobId} has errors, skipping execution.", context.JobId);
                return;
            }

            var updateResult = await _sqlQueueRepository.UpdateJobStatusAsync(context.JobId, (int)JobStatusEnum.Running,errorMessage: null, cancellationToken);

            SyncContextItem(context.Item, updateResult);

            context.ExtraState["UpdatedItem"] = true;
        }

        private static void SyncContextItem(QuickDeskWorkerItem item, QuickDeskWorkerItem updatedItem)
        {
            item.JobStatusId = updatedItem.JobStatusId;
            item.JobId = updatedItem.JobId;
            item.JobStatus = updatedItem.JobStatus;
            item.StartedAt = updatedItem.StartedAt;
            item.ModifiedAt = updatedItem.ModifiedAt;
            item.ErrorMessage = updatedItem.ErrorMessage;
        }
    }
}
