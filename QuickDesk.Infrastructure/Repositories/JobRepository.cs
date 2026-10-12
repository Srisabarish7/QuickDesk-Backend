using Dapper;
using Microsoft.Extensions.Logging;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.DbConnection;
using QuickDesk.Infrastructure.Interfaces;
using System.Data;

namespace QuickDesk.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(IDbConnectionFactory connectionFactory, ILogger<JobRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<Job> CreateJob(CreateJob request, CancellationToken cancellationToken)
        {
            try
            {
                using var connectionString = await _connectionFactory.GetQuickDeskConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.UserId);
                parameters.Add("@JobTypeId", request.JobTypeId);
                parameters.Add("@JobStatusId", request.JobStatusId);
                parameters.Add("@ScheduledAt", request.ScheduledAt);
                parameters.Add("@DetailsJson", request.DetailsJson.GetRawText());

                var result = await connectionString.QuerySingleAsync<Job>(
                    DBQueries.CreateJob,
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result ?? new Job();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobRepository:CreateJob Input:{request}", request);
                throw;
            }
        }

        public async Task<bool> CheckUserExist(long userId, CancellationToken cancellationToken)
        {
            try
            {
                using var connectionString = await _connectionFactory.GetQuickDeskConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                var result = await connectionString.QueryFirstOrDefaultAsync<bool>(
                    DBQueries.CheckUserExist,
                    parameters,
                    commandType: CommandType.Text);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobRepository:CheckUserExist Input:{userId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Notes>> GetNotes(GetNotes request, CancellationToken cancellationToken)
        {
            try
            {
                using var connectionString = await _connectionFactory.GetQuickDeskConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.UserId);
                parameters.Add("@Page", request.Page);
                parameters.Add("@PageSize", request.PageSize);
                parameters.Add("@SearchString", request.SearchString);
                parameters.Add("@JobTypeId", 2);
                var result = await connectionString.QueryAsync<Notes>(
                    DBQueries.GetNotes,
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobRepository:GetNotes Input:{request}", request);
                throw;
            }
        }

        public async Task<IEnumerable<GetJobsResponse>> GetJobs(GetJobsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var connectionString = await _connectionFactory.GetQuickDeskConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.UserId);
                parameters.Add("@JobTypeId", request.JobTypeId);
                parameters.Add("@JobStatusId", request.JobStatusId);
                parameters.Add("@Page", request.Page);
                parameters.Add("@PageSize", request.PageSize);
                parameters.Add("@StartDate", request.StartDate);
                parameters.Add("@EndDate", request.EndDate);
                var result = await connectionString.QueryAsync<GetJobsResponse>(
                    DBQueries.GetJobs,
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobRepository:GetJobs Input:{request}", request);
                throw;
            }
        }

        public async Task<GetJobById> GetJobById(GetJobByIdRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var connectionString = await _connectionFactory.GetQuickDeskConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.UserId);
                parameters.Add("@JobId", request.JobId);
                var result = await connectionString.QuerySingleOrDefaultAsync<GetJobById>(
                    DBQueries.GetJobById,
                    parameters,
                    commandType: CommandType.Text);
                return result ?? new GetJobById();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobRepository:GetJobById Input:{request}", request);
                throw;
            }
        }
    }
}
