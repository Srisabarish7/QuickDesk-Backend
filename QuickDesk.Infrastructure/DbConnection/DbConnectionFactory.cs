using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace QuickDesk.Infrastructure.DbConnection
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> GetQuickDeskConnection(CancellationToken cancellationToken = default);
    }
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _connectionString;
        private readonly ILogger<DbConnectionFactory> _logger;

        public DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger)
        {
            _connectionString = configuration;
            _logger = logger;
        }

        public async Task<IDbConnection> GetQuickDeskConnection(CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _connectionString.GetConnectionString("QuickDesk");
                var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in DbConnectionFactory::GetQuickDeskConnection while creating the connection");
                throw;
            }
        }
    }
}
