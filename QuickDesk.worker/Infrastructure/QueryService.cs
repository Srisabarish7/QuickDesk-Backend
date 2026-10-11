using Dapper;
using Microsoft.Data.SqlClient;
using QuickDesk.worker.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure
{
    public class QueryService : IQueryService
    {
        private readonly ILogger<QueryService> _logger;
        private string _connectionString;

        public QueryService(ILogger<QueryService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("QuickDesk") ?? throw new ArgumentException();
        }

        public async Task<(string? UserName, string? Email)> GetUserDetail(long userId, CancellationToken ct)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);

                string query = """SELECT UserName,Email FROM dbo.Users WITH(NOLOCK) WHERE UserId = @UserId""";

                var result = await conn.QueryFirstOrDefaultAsync<(string? UserName, string? Email)>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: ct));

                return result;
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in QueryService:GetUserName UserId: {userId}", userId);
                throw;
            }
        }
    }
}
