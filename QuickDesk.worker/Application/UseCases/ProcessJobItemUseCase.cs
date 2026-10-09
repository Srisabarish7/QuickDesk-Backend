using QuickDesk.worker.Application.Interfaces;
using QuickDesk.worker.Dispatching;
using QuickDesk.worker.Infrastructure;
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

        public ProcessJobItemUseCase(IJobItemValidator validator, ProcessorDispatcher processorDispatcher, ILogger<ProcessJobItemUseCase> logger, ISqlQueueRepository sqlQueueRepository)
        {
            _validator = validator;
            _processorDispatcher = processorDispatcher;
            _logger = logger;
            _sqlQueueRepository = sqlQueueRepository;
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

                    return ProcessingResult.Failure(errorMessage);
                }

                // Process the job item
                var success = await _processorDispatcher.DispatchAsync(jobItem, ct);

                return ProcessingResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job item.");
                return ProcessingResult.Failure(ex.Message);
            }
        }
    }
}
