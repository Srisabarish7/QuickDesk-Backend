using Dapper;
using Microsoft.Extensions.Logging;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.DbConnection;
using QuickDesk.Infrastructure.Interfaces;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IDbConnectionFactory dbConnectionFactory, ILogger<UserRepository> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<User> AddUser(AddUser user, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _dbConnectionFactory.GetQuickDeskConnection(cancellationToken);
                
                var parameters = new DynamicParameters();
                parameters.Add("@UserName", user.UserName);
                parameters.Add("@FirstName", user.FirstName);
                parameters.Add("@LastName", user.LastName);
                parameters.Add("@Email", user.Email);
                parameters.Add("@HashedPassword", user.Password);

                var result = await connection.QueryFirstOrDefaultAsync<User>(
                    DBQueries.AddUser,
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result ?? new User();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserRepository::AddUser InputParameters: {@User}", user);
                throw;
            }
        }

        public async Task<CheckUserExists> CheckUserExists(string userName, string email, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _dbConnectionFactory.GetQuickDeskConnection(cancellationToken);
                var parameters = new DynamicParameters();
                parameters.Add("@UserName", userName);
                parameters.Add("@Email", email);
                var result = await connection.QueryFirstOrDefaultAsync<CheckUserExists>(
                    DBQueries.CheckUserExists,
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result ?? new CheckUserExists();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserRepository::CheckUserExists InputParameters: UserName={UserName}, Email={Email}", userName, email);
                throw;
            }
        }
    }
}
