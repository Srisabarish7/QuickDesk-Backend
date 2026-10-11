using QuickDesk.worker.Domain.Enum;
using QuickDesk.worker.Infrastructure.Interfaces;
using QuickDesk.worker.Models;
using QuickDesk.worker.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Pipeline
{
    public sealed class AddEmailLogStep : IPipelineStep
    {
        private readonly ILogger<AddEmailLogStep> _logger;
        private readonly ISqlQueueRepository _sqlQueueRepository;
        private readonly IQueryService _queryService;

        public AddEmailLogStep(ILogger<AddEmailLogStep> logger, ISqlQueueRepository sqlQueueRepository, IQueryService queryService)
        {
            _logger = logger;
            _sqlQueueRepository = sqlQueueRepository;
            _queryService = queryService;
        }

        public async Task ExecuteAsync(ProcessingContext context, CancellationToken ct)
        {
            if (context.Errors.Any())
            {
                _logger.LogWarning("AddEmail Log Failed for the JobId: {jobId}", context.JobId);
            }


            try
            {
                var userEmail = await GetUserEmail(context.UserId, ct);
                var emailLog = new AddEmailLog
                {
                    UserId = context.UserId,
                    JobId = context.JobId,
                    JobTypeId = context.JobTypeId,
                    Recipient = context.Details.Value<string>("to") ?? userEmail.Email ?? string.Empty,
                    Subject = context.Details.Value<string>("subject") ?? string.Empty,
                    Title = context.Details.Value<string>("title") ?? string.Empty,
                    Description = context.Details.Value<string>("description") ?? string.Empty,
                    ErrorMessage = context.Errors.Any() ? string.Join("; ", context.Errors) : null,
                    Status = context.JobStatus.ToString(),
                };

                await _sqlQueueRepository.AddEmailLog(emailLog, ct);
            }catch(Exception ex)
            {
                _logger.LogError(ex,"Exception thrown in AddEmailLogStep:ExecuteAsync JobId: {jobId}", context.JobId);
                throw;
            }
        }

        private async Task<(string? UserName, string? Email)> GetUserEmail(long userId, CancellationToken ct)
        {
            return await _queryService.GetUserDetail(userId, ct);
        }
    }
}
