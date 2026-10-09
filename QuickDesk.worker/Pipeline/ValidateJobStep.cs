using Newtonsoft.Json.Linq;
using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Models.Pipeline;

namespace QuickDesk.worker.Pipeline
{
    public sealed class ValidateJobStep : IPipelineStep
    {
        private readonly ILogger<ValidateJobStep> _logger;
        public ValidateJobStep(ILogger<ValidateJobStep> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(ProcessingContext context, CancellationToken cancellationToken)
        {
            await ValidateJobDetails(context, cancellationToken);
            await ValidateUserId(context, cancellationToken);
        }

        private  Task ValidateJobDetails(ProcessingContext context, CancellationToken cancellationToken)
        {
            if(context.JobId <= 0)
            {
                _logger.LogWarning("Invalid JobId {JobId} for validation.", context.JobId);
                return Fail(context, $"Invalid JobId {context.JobId}");
            }

            if (context.JobTypeId <= 0)
            {
                _logger.LogWarning("Invalid JobTypeId {JobTypeId} for validation.", context.JobTypeId);
                return Fail(context, $"Invalid JobTypeId {context.JobTypeId}");
            }

            if(!Enum.IsDefined(typeof(JobTypeEnum), context.JobType))
            {
                _logger.LogWarning("Invalid JobType {JobType} for validation.", context.JobType);
                return Fail(context, $"Invalid JobType {context.JobType} for validation.");
            }

            if (string.IsNullOrWhiteSpace(context.DetailsJson))
            {
                _logger.LogWarning("Invalid DetailsJson for validation.");
                return Fail(context, "Invalid DetailsJson for validation.");
            }

            try
            {
                context.Details = JObject.Parse(context.DetailsJson);
                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse DetailsJson for validation.");
                return Fail(context, "Failed to parse DetailsJson for validation.");
            }
        }

        private Task ValidateUserId(ProcessingContext context, CancellationToken cancellationToken)
        {
            if (context.UserId <= 0)
            {
                _logger.LogWarning("Invalid UserId {UserId} for validation.", context.UserId);
                return Fail(context, $"Invalid UserId {context.UserId} for validation.");
            }
            return Task.CompletedTask;
        }

        private Task Fail(ProcessingContext context, string message)
        { 
            _logger.LogError("Validation failed for JobId {JobId}, JobTypeId {JobTypeId}, UserId {UserId}.", context.JobId, context.JobTypeId, context.UserId);
            context.Errors.Add(message);
            context.ExtraState["ValidationFailed"] = true;
            context.ExtraState["ValidationErrorMessage"] = message;
            return Task.CompletedTask;
        }
    }
}
