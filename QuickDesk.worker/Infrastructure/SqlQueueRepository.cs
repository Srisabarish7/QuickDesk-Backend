using Dapper;
using Microsoft.Data.SqlClient;
using QuickDesk.worker.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public class SqlQueueRepository : ISqlQueueRepository
    {
        private readonly ILogger<SqlQueueRepository> _logger;
        private readonly string _connectionString;

        public SqlQueueRepository(ILogger<SqlQueueRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("QuickDesk") ?? throw new ArgumentNullException("Connection string 'QuickDesk' not found.");
        }

        public async Task<IEnumerable<QuickDeskWorkerItem>> ClaimPendingItemsAsync(int batchSize, CancellationToken cancellationToken)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                var result = await conn.QueryAsync<QuickDeskWorkerItem>(
                    new CommandDefinition("usp_Job_GetPending", new { BatchSize = batchSize }, cancellationToken: cancellationToken));
                return result ?? Enumerable.Empty<QuickDeskWorkerItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SqlQueueRepository:ClaimPendingItemsAsync");
                throw;
            }
        }

        public async Task<int> ResetStuckItemsAsync(int timeoutInMinutes, CancellationToken cancellationToken)
        {
            try
            {
                const string query = """                    
                    UPDATE Jobs
                    SET  JobStatusId = 2
                    WHERE JobStatusId = 3 AND StartedAt < DATEADD(MINUTE, -@TimeoutInMinutes, SYSUTCDATETIME());                    
                    """;

                await using var conn = new SqlConnection(_connectionString);
                return await conn.ExecuteAsync(
                    new CommandDefinition(query, new { TimeoutInMinutes = timeoutInMinutes }, cancellationToken: cancellationToken));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SqlQueueRepository:ResetStuckItemsAsync with timeoutInMinutes: {timeoutInMinutes}", timeoutInMinutes);
                throw;
            }
        }

        public async Task<QuickDeskWorkerItem> UpdateJobStatusAsync(long jobId, int jobStatusId, string? errorMessage, CancellationToken cancellationToken)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                var result = await conn.QuerySingleOrDefaultAsync<QuickDeskWorkerItem>(
                    new CommandDefinition("usp_Job_UpdateStatus", new { JobId = jobId, JobStatusId = jobStatusId, ErrorMessage = errorMessage }, cancellationToken: cancellationToken));
                if (result == null)
                {
                    throw new InvalidOperationException($"No job found with JobId: {jobId}");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SqlQueueRepository:UpdateJobStatusAsync for JobId: {jobId}, JobStatusId: {jobStatusId}", jobId, jobStatusId);
                throw;
            }
        }
    }
}
