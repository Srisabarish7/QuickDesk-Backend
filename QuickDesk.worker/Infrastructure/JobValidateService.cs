using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public class JobValidateService : IJobValidateService
    {
        private readonly ILogger<JobValidateService> _logger;
        private readonly string _connectionString;
        public JobValidateService(ILogger<JobValidateService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("QuickDesk");
        }

        public async Task<bool> ValidateJobAsync(long jobId, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var query = await connection.QueryFirstOrDefaultAsync<int>(
                    new CommandDefinition("SELECT COUNT(1) FROM Jobs WITH(NOLOCK) WHERE JobId = @JobId", new { JobId = jobId }, cancellationToken: cancellationToken));
                return query > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobValidateService.ValidateJobAsync for job {JobId}", jobId);
                throw;
            }
        }

        public async Task<bool> ValidateJobTypeAsync(long jobTypeId, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = await connection.QueryFirstOrDefaultAsync<int>(
                    new CommandDefinition("SELECT COUNT(1) FROM JobTypes WITH(NOLOCK) WHERE JobTypeId = @JobTypeId", new { JobTypeId = jobTypeId }, cancellationToken: cancellationToken));
                return query > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in JobValidateService.ValidateJobTypeAsync for job type {JobTypeId}", jobTypeId);
                throw;
            }
        }
    }
}
