using QuickDesk.Application.ResponseDtos;
using QuickDesk.worker.Application.Interfaces;
using QuickDesk.worker.Dispatching;
using QuickDesk.worker.Infrastructure;
using QuickDesk.worker.Infrastructure.Interfaces;
using QuickDesk.worker.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Application.UseCases
{
    public class ProcessJobItemUseCase : IProcessJobItemUseCase
    {
        private readonly IJobItemValidator _validator;
        private readonly ProcessorDispatcher _processorDispatcher;
        private readonly ILogger<ProcessJobItemUseCase> _logger;
        private readonly ISqlQueueRepository _sqlQueueRepository;
        private readonly SignalRConnectionService _signalRConnectionService;

        public ProcessJobItemUseCase(IJobItemValidator validator, ProcessorDispatcher processorDispatcher, ILogger<ProcessJobItemUseCase> logger, ISqlQueueRepository sqlQueueRepository, SignalRConnectionService signalRConnectionService)
        {
            _validator = validator;
            _processorDispatcher = processorDispatcher;
            _logger = logger;
            _sqlQueueRepository = sqlQueueRepository;
            _signalRConnectionService = signalRConnectionService;
        }

        public async Task<ProcessingResult> ExecuteAsync(QuickDeskWorkerItem jobItem, CancellationToken ct)
        {
            try
            {
                var validationResult = _validator.Validate(jobItem);

                if (!validationResult.IsValid)
                {
                   var errorMessage = string.Join(';', validationResult.Errors);

                    _logger.LogError("Validation failed for job item {JobId}: {ErrorMessage}", jobItem.JobId, errorMessage);

                    var failedJob = await _sqlQueueRepository.UpdateJobStatusAsync(jobItem.JobId, 5, 0, errorMessage, ct);
                    await NotifyJobUpdateAsync(failedJob, ct);

                    return ProcessingResult.Failure(errorMessage);
                }

                // Process the job item
                var success = await _processorDispatcher.DispatchAsync(jobItem, ct);

                QuickDeskWorkerItem workerItem;
                if (success)
                {
                   workerItem = await _sqlQueueRepository.UpdateJobStatusAsync(jobItem.JobId, 4, 100, errorMessage: null, ct);
                   await NotifyJobUpdateAsync(workerItem, ct);
                }
                else
                {
                   workerItem = await _sqlQueueRepository.UpdateJobStatusAsync(jobItem.JobId, 5, 0, "Process failed.", ct);
                   await NotifyJobUpdateAsync(workerItem, ct);
                }

                var status = success ? "Completed" : "Failed";
                DateTime? sentAt = string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ? DateTime.Now : null;
                string emailErrorMessage = string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ? string.Empty : "Update EmailLog Failed";
                await _sqlQueueRepository.UpdateEmailLog(jobItem.JobId, status, sentAt, emailErrorMessage, ct);
                return ProcessingResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job item.");
                return ProcessingResult.Failure(ex.Message);
            }
        }

        private async Task NotifyJobUpdateAsync(QuickDeskWorkerItem jobItem, CancellationToken cancellationToken)
        {
            var dto = new JobUpdateDto
            {
                JobId = jobItem.JobId,
                Status = jobItem.JobStatus,
                Progress = jobItem.Progress,
                Message = string.Empty,
                ErrorMessage = jobItem.ErrorMessage ?? string.Empty,
                UpdatedAt = DateTime.Now.ToString("o")
            };

            await _signalRConnectionService.PublishJobUpdateAsync(dto, cancellationToken);
        }
    }
}
