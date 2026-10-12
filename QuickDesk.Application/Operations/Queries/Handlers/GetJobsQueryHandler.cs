using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QuickDesk.Application.Operations.Queries.Requests;
using QuickDesk.Application.ResponseDtos;
using QuickDesk.Domain.Common.ExceptionHandling;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Operations.Queries.Handlers
{
    public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, List<GetJobsDto>>
    {
        private readonly ILogger<GetJobsQueryHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IJobRepository _jobRepository;

        public GetJobsQueryHandler(ILogger<GetJobsQueryHandler> logger, IMapper mapper, IJobRepository jobRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _jobRepository = jobRepository;
        }

        public async Task<List<GetJobsDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                await CheckValidUser(request.UserId, cancellationToken);
                var jobs = await GetJobs(request, cancellationToken);
                return CreateSuccessMessage(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetJobsQueryHandler:Handle Input:{request}", request);
                throw;
            }
        }

        public async Task CheckValidUser(long userId, CancellationToken cancellationToken)
        {
            if (userId > 0)
            {
                var userExists = await _jobRepository.CheckUserExist(userId, cancellationToken);
                if (!userExists)
                {
                    throw new UnauthorizedCustomException(new List<string> { "User does not exist" });
                }
            }
        }

        public async Task<List<GetJobsResponse>> GetJobs(GetJobsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                request.StartDate ??= DateTime.Today.AddDays(-1);
                request.EndDate ??= DateTime.Today;

                request.JobTypeId = request.JobTypeId == 4 ? null : request.JobTypeId;
                request.JobStatusId = request.JobStatusId == 7 ? null : request.JobStatusId;
                var getJobs = _mapper.Map<GetJobsRequest>(request);
                var result = await _jobRepository.GetJobs(getJobs, cancellationToken);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetJobsQueryHandler:GetJobs Input:{request}", request);
                throw;
            }
        }

        private List<GetJobsDto> CreateSuccessMessage(List<GetJobsResponse> jobs)
        {
            if (jobs == null || jobs.Count == 0)
            {
                throw new BadRequestCustomException(new List<string> { "No jobs found." });
            }
            return jobs.Select(job => new GetJobsDto
            {
                JobId = job.JobId,
                JobName = job.JobName,
                JobStatus = job.JobStatus,
                Progress = (int)job.Progress,
                ScheduledAt = job.ScheduledAt ?? DateTime.MinValue,
                CreatedAt = job.CreatedAt
            }).ToList();
        }
    }
}
