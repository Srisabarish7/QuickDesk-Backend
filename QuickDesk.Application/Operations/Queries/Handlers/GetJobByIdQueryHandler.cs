using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QuickDesk.Application.Operations.Queries.Requests;
using QuickDesk.Application.ResponseDtos;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Operations.Queries.Handlers
{
    public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobByIdDto>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetJobByIdQueryHandler> _logger;

        public GetJobByIdQueryHandler(IJobRepository jobRepository, IMapper mapper, ILogger<GetJobByIdQueryHandler> logger)
        {
            _jobRepository = jobRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<JobByIdDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var job = await GetJobById(request, cancellationToken);
                return CreateSuccessMessage(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetJobByIdQueryHandler:Handle Input:{request}", request);
                throw;
            }
        }

        public async Task<GetJobById> GetJobById(GetJobByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var jobRequest = _mapper.Map<GetJobByIdRequest>(request);
                return await _jobRepository.GetJobById(jobRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetJobByIdQueryHandler:GetJobById Input:{request}", request);
                throw;
            }
        }

        private JobByIdDto CreateSuccessMessage(GetJobById job)
        {
            return new JobByIdDto
            {
                JobId = job.JobId,
                JobName = job.JobName,
                UserName = job.UserName,
                JobStatus = job.JobStatus,
                CreatedAt = job.CreatedAt,
                ScheduledAt = job.ScheduledAt,
                Progress = job.Progress,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                ErrorMessage = job.ErrorMessage,
                Receipent = job.Receipent,
                Subject = job.Subject,
                Title = job.Title,
                Description = job.Description,
                Message = job.Message,
                RequestId = Guid.NewGuid().ToString(),
                RequestMessage = "Job retrieved successfully."
            };
        }
    }
}
