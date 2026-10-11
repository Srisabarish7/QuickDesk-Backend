using QuickDesk.worker.Application.Interfaces;
using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Infrastructure.Interfaces;
using QuickDesk.worker.Models;
using QuickDesk.worker.Models.Pipeline;
using QuickDesk.worker.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace QuickDesk.worker.Processors
{
    public sealed class SendEmailProcessor : BaseJobTypeProcessor
    {
        private readonly IEmailSenderService _emailSenderService;
        private readonly ILogger<SendEmailProcessor> _logger;
        private readonly ValidateJobStep _validateJobStep;
        private readonly JobRunningStep _jobRunningStep;
        private readonly AddEmailLogStep _addEmailLogStep;

        public SendEmailProcessor(IEmailSenderService emailSenderService, ILogger<SendEmailProcessor> logger, ValidateJobStep validateJobStep, JobRunningStep jobRunningStep, ILogger<BaseJobTypeProcessor> baseLogger, IJobValidateService jobValidateService, AddEmailLogStep addEmailLogStep) : base(baseLogger, jobValidateService)
        {
            _emailSenderService = emailSenderService;
            _logger = logger;
            _validateJobStep = validateJobStep;
            _jobRunningStep = jobRunningStep;
            _addEmailLogStep = addEmailLogStep;
        }

        public override JobTypeEnum JobTypeId => JobTypeEnum.SendEmail;

        public override async Task<bool> ProcessJobAsync(ProcessingContext context, CancellationToken cancellationToken)
        {
            if (context == null) return false;
            try
            {
                if (Enum.IsDefined(typeof(JobTypeEnum), (int)context.JobTypeId))
                {
                    context.JobType = (JobTypeEnum)context.JobTypeId;
                }

                var validationResult = await ValidateJobStep(context, cancellationToken);
                if (!validationResult.IsSuccess)
                {
                    return false;
                }

                var runningStep = await JobRunningStep(context, cancellationToken);
                if (!runningStep.IsSuccess)
                {
                    return false;
                }

                await AddEmailLogStep(context, cancellationToken);

                SendEmailDetails? emailDetails = null;

                if (context.Details != null && context.Details.HasValues)
                {
                    emailDetails = context.Details.ToObject<SendEmailDetails>();
                }
                else if(!string.IsNullOrEmpty(context.DetailsJson))
                {
                    emailDetails = JsonSerializer.Deserialize<SendEmailDetails>(context.DetailsJson);
                }

                if (emailDetails == null || string.IsNullOrWhiteSpace(emailDetails.To) || string.IsNullOrWhiteSpace(emailDetails.Subject) || string.IsNullOrWhiteSpace(emailDetails.Message))
                {
                    return false;
                }
                
                await _emailSenderService.SendEmailAsync(emailDetails.To, emailDetails.Subject, emailDetails.Message, cancellationToken);                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SendEmailProcessor for job {JobId}", context.JobId);
                return false;
            }
        }

        private async Task<ProcessingResult> ValidateJobStep(ProcessingContext context, CancellationToken cancellationToken)
        {
            await _validateJobStep.ExecuteAsync(context, cancellationToken);
            if(context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }

        private async Task<ProcessingResult> JobRunningStep(ProcessingContext context, CancellationToken cancellationToken)
        {
            await _jobRunningStep.ExecuteAsync(context, cancellationToken);
            if (context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }

        private async Task<ProcessingResult> AddEmailLogStep(ProcessingContext context, CancellationToken cancellation)
        {
            await _addEmailLogStep.ExecuteAsync(context, cancellation);
            if (context.Errors.Any())
            {
                return ProcessingResult.Failure(string.Join("; ", context.Errors));
            }
            return ProcessingResult.Success();
        }
    }
}
