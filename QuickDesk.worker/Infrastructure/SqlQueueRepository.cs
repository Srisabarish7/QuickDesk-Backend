using Dapper;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using QuickDesk.worker.Infrastructure.Interfaces;
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

        public async Task<QuickDeskWorkerItem> UpdateJobStatusAsync(long jobId, int jobStatusId,int progress, string? errorMessage, CancellationToken cancellationToken)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                var result = await conn.QuerySingleOrDefaultAsync<QuickDeskWorkerItem>(
                    new CommandDefinition("usp_Job_UpdateStatus", new { JobId = jobId, JobStatusId = jobStatusId, Progress = progress, ErrorMessage = errorMessage }, cancellationToken: cancellationToken));
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

        public async Task<int> AddEmailLog(AddEmailLog request, CancellationToken ct)
        {
            try
            {
                const string query = """
                        INSERT INTO dbo.EmailLogs (UserId, JobId, Recipient, Subject, Status, ErrorMessage, CreatedAt, JobTypeId, Title, Description)
                        VALUES (@UserId, @JobId, @Recipient, @Subject, @Status, @ErrorMessage, @CreatedAt, @JobTypeId, @Title, @Description)
                    """;

                using var conn = new SqlConnection(_connectionString);

                return await conn.ExecuteAsync(
                    new CommandDefinition(query, new { request.UserId, request.JobId, request.Recipient, request.Subject, request.Status, request.ErrorMessage, CreatedAt = DateTime.UtcNow, request.JobTypeId, request.Title, request.Description }, cancellationToken: ct));
            }catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SqlQueueRepository:AddEmailLog for JobId: {jobId}", request.JobId);
                throw;
            }
        }

        public async Task<int> UpdateEmailLog(long jobId, string status, DateTime? sentAt, string? errorMessage, CancellationToken ct)
        {
            try
            {
                const string query = """
                        UPDATE dbo.EmailLogs 
                        SET 
                            Status = @Status, 
                            SentAt = @SentAt,
                            ErrorMessage = @ErrorMessage
                        WHERE JobId = @JobId
                    """;

                using var conn = new SqlConnection(_connectionString);

                return await conn.ExecuteAsync(
                    new CommandDefinition(query, new { JobId = jobId, Status = status, SentAt = sentAt, ErrorMessage = errorMessage }, cancellationToken: ct));
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SqlQueueRepository:UpdateEmailLog for JobId: {jobId}", jobId);
                throw;
            }
        }

        public async Task<IReadOnlyList<EmailLogs>> GetEmailLogs(long userId, DateTime? startDate, DateTime? endDate, CancellationToken ct)
        {
            try
            {
                const int pageSize = 100;
                int page = 1;

                var logs = new List<EmailLogs>();

                using var conn = new SqlConnection(_connectionString);

                while(true){
                    var pageLogs = (await conn.QueryAsync<EmailLogs>(
                        new CommandDefinition("usp_EmailLog_GetCurrentMonth", new
                        {
                            UserId = userId,
                            Page = page,
                            PageSize = pageSize,
                            StartDate = startDate,
                            EndDate = endDate
                        }, commandType: System.Data.CommandType.StoredProcedure, cancellationToken: ct))).AsList();

                    logs.AddRange(pageLogs);

                    if(pageLogs.Count < pageSize)
                    {
                        break;
                    }

                    page++;
                }

                return logs;

            }catch(Exception ex)
            {
                var inputParams = new { userId, startDate, endDate };
                _logger.LogError(ex, "Exception thrown in SqlQueueRepository:GetEmailLogs InputParameters: {inputParams}", inputParams);
                throw;
            }
        }
    }
}
