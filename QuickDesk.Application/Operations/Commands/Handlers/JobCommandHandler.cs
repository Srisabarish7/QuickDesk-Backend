using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuickDesk.Application.Operations.Commands.Requests;
using QuickDesk.Application.ResponseDtos;
using QuickDesk.Domain.Common.ExceptionHandling;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.Interfaces;
using System.Text.Json;

namespace QuickDesk.Application.Operations.Commands.Handlers
{
    public class JobCommandHandler : IRequestHandler<CreateJobCommand, JobDto>
    {
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<JobCommandHandler> _logger;
        private readonly IMapper _mapper;

        public JobCommandHandler(IJobRepository jobRepository, ILogger<JobCommandHandler> logger, IMapper mapper)
        {
            _jobRepository = jobRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<JobDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await CheckUserExist(request.UserId, cancellationToken);
                var job = await CreateJob(request, cancellationToken);
                return CreatesuccessMessage(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobCommandHandler:Handle Input:{request}", request);
                throw;
            }
        }

        public async Task CheckUserExist(long? userId, CancellationToken cancellationToken)
        {
            if (userId.HasValue)
            {
                var userExists = await _jobRepository.CheckUserExist(userId.Value, cancellationToken);
                if (!userExists)
                {
                    throw new UnauthorizedCustomException(["UserID does not exist."]);
                }
            }
        }

        public async Task<Job> CreateJob(CreateJobCommand request, CancellationToken cancellationToken)
        {
            try
            {  
                request.JobStatusId = request.ScheduledAt.HasValue && request.ScheduledAt.Value > DateTime.UtcNow ? 2 : 1;
                var isValidJson = JsonDocument.Parse(request.DetailsJson.GetRawText());
                if(!isValidJson.RootElement.ValueKind.Equals(JsonValueKind.Object))
                {
                    throw new BadRequestCustomException(new List<string> { "DetailsJson must be a valid JSON object." });
                }
                var job = _mapper.Map<CreateJob>(request);
                return await _jobRepository.CreateJob(job, cancellationToken);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobCommandHandler:CreateJob Input:{request}", request);
                throw;
            }
        }

        private JobDto CreatesuccessMessage(Job job)
        {
            return new JobDto
            {
                UserId = job.UserId,
                JobId = job.JobId,
                JobName = job.JobName,
                JobStatus = job.JobStatus,
                ScheduledAt = job.ScheduledAt,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                CreatedAt = job.CreatedAt,
                ErrorMessage = job.ErrorMessage,
                DetailsJson = job.DetailsJson,
                RequestId = Guid.NewGuid().ToString(),
                RequestMessage = "Job created successfully"
            };
        }
    }
}
