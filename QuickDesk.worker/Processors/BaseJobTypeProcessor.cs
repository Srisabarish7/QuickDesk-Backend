using QuickDesk.worker.Dispatching;
using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Infrastructure;
using QuickDesk.worker.Models;
using QuickDesk.worker.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Processors
{
    public abstract class BaseJobTypeProcessor : IJobTypeProcessor
    {
        protected readonly ILogger<BaseJobTypeProcessor> _logger;
        protected readonly IJobValidateService _jobValidateService;

        protected BaseJobTypeProcessor(ILogger<BaseJobTypeProcessor> logger, IJobValidateService jobValidateService)
        {
            _logger = logger;
            _jobValidateService = jobValidateService;
        }

        public abstract JobTypeEnum JobTypeId { get; }
        public abstract Task<bool> ProcessJobAsync(ProcessingContext jobItem, CancellationToken cancellationToken);

        protected async Task ValidateJobs(ProcessingContext jobItem, CancellationToken cancellationToken) 
        {
            var isValidJobId = await _jobValidateService.ValidateJobAsync(jobItem.JobId, cancellationToken);

            var isValidJobType = await _jobValidateService.ValidateJobTypeAsync(jobItem.JobTypeId, cancellationToken);

            if (!isValidJobId)
            {
                _logger.LogError($"The given JobId: {jobItem.JobId} is invalid.");
                throw new InvalidOperationException($"The given JobId: {jobItem.JobId} is invalid.");
            }

            if (!isValidJobType)
            {
                _logger.LogError($"The given JobTypeId: {jobItem.JobTypeId} is invalid.");
                throw new InvalidOperationException($"The given JobTypeId: {jobItem.JobTypeId} is invalid.");
            }
        }
    }
}
