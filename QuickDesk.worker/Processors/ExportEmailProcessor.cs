using Newtonsoft.Json.Linq;
using QuickDesk.worker.Application.Interfaces;
using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Infrastructure.Interfaces;
using QuickDesk.worker.Models.Pipeline;
using QuickDesk.worker.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Processors
{
    public sealed class ExportEmailProcessor : BaseJobTypeProcessor
    {
        private readonly ILogger<ExportEmailProcessor> _logger;
        private readonly ValidateJobStep _validateJobStep;
        private readonly JobRunningStep _jobRunStep;
        private readonly ICsvExportService _csvExportService;
        private readonly ISqlQueueRepository _sqlQueueRepository;
        private readonly IQueryService _queryService;

        public ExportEmailProcessor(ILogger<ExportEmailProcessor> logger, ValidateJobStep validateJobStep, JobRunningStep jobRunningStep, ICsvExportService csvExportService, ISqlQueueRepository sqlQueueRepository, ILogger<BaseJobTypeProcessor> baseLogger, IJobValidateService jobValidateService, IQueryService queryService) : base(baseLogger, jobValidateService)
        {
            _logger = logger;
            _validateJobStep = validateJobStep;
            _jobRunStep = jobRunningStep;
            _csvExportService = csvExportService;
            _sqlQueueRepository = sqlQueueRepository;
            _queryService = queryService;
        }

        public override JobTypeEnum JobTypeId => JobTypeEnum.ExportEmail;

        public override async Task<bool> ProcessJobAsync(ProcessingContext context, CancellationToken ct)
        {
            if (context == null)
                return false;

            try
            {
                if(Enum.IsDefined(typeof(JobTypeEnum), (int)context.JobTypeId))
                {
                    context.JobType = (JobTypeEnum)context.JobTypeId;
                }

                var validationResult = await ValidateJobStep(context, ct);
                if (!validationResult.IsSuccess)
                {
                    return false;
                }

                var jobRunResult = await JobRunningStep(context, ct);
                if (!jobRunResult.IsSuccess)
                {
                    return false;
                }

                JObject details = context.Details;                      

                DateTime? startDate = details.Value<DateTime>("startDate");
                DateTime? endDate = details.Value<DateTime>("endDate");

                if(startDate == null || endDate == null)
                {
                    throw new InvalidOperationException("StartDate and EndDate is Required.");
                }

                if(startDate > endDate)
                {
                    throw new InvalidOperationException("StartDate must be lesser than EndDate");
                }

                var emailLogs = await _sqlQueueRepository.GetEmailLogs(context.UserId, startDate, endDate, ct);

                var userDetail = await _queryService.GetUserDetail(context.UserId, ct);

                var filePath = await _csvExportService.ExportEmailLogsAsync(emailLogs, userDetail.UserName ,context.JobId, ct);

                context.ExtraState["ExportEmailLog"] = filePath;

                return true;
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in ExportEmailProcessor: ProcessJobAsync InputParameters: {context}", context);
                return false;
            }
        }

        private async Task<ProcessingResult> ValidateJobStep(ProcessingContext context, CancellationToken cancellationToken)
        {
            await _validateJobStep.ExecuteAsync(context, cancellationToken);
            if (context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }

        private async Task<ProcessingResult> JobRunningStep(ProcessingContext context, CancellationToken cancellationToken)
        {
            await _jobRunStep.ExecuteAsync(context, cancellationToken);
            if (context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }
    }
}
