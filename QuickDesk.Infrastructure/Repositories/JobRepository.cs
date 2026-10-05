using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.DbConnection;
using QuickDesk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

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

                var result = await connectionString.QuerySingleAsync<Job>(
                    DBQueries.CreateJob,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure);
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
    }
}
