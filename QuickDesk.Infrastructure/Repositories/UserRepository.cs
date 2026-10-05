using Dapper;
using Microsoft.Extensions.Logging;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.DbConnection;
using QuickDesk.Infrastructure.Interfaces;
using System.Data;
using System.Collections.Generic;
using System.Text;
using QuickDesk.Infrastructure.Services;

namespace QuickDesk.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<UserRepository> _logger;
        private readonly IPasswordHasherService _passwordHasherService;

        public UserRepository(IDbConnectionFactory dbConnectionFactory, ILogger<UserRepository> logger, IPasswordHasherService passwordHasherService)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
            _passwordHasherService = passwordHasherService;
        }

        public async Task<User> AddUser(AddUser user, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _dbConnectionFactory.GetQuickDeskConnection(cancellationToken);

                var hashedPassword = _passwordHasherService.HashPassword(user.Password);

                var parameters = new DynamicParameters();
                parameters.Add("@UserName", user.UserName);
                parameters.Add("@FirstName", user.FirstName);
                parameters.Add("@LastName", user.LastName);
                parameters.Add("@Email", user.Email);
                parameters.Add("@HashedPassword", hashedPassword);
                parameters.Add("@IsActive", 1);

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
                    commandType: CommandType.Text);
                return result ?? new CheckUserExists();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserRepository::CheckUserExists InputParameters: UserName={UserName}, Email={Email}", userName, email);
                throw;
            }
        }

        public async Task<string?> ValidateUser(string userName, string password, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _dbConnectionFactory.GetQuickDeskConnection(cancellationToken);
                var parameters = new DynamicParameters();
                parameters.Add("@UserName", userName);
                var result = await connection.QueryFirstOrDefaultAsync<(string UserName, string PasswordHash)>(
                    DBQueries.CheckValidUser,
                    parameters,
                    commandType: CommandType.Text);
                if (result.UserName == null || result.PasswordHash == null)
                {
                    return null;
                }

                var isValidPassword = _passwordHasherService.VerifyPassword(password, result.PasswordHash);
                if (!isValidPassword)
                {
                    _logger.LogWarning("Invalid password attempt for user: {UserName}", userName);
                    throw new UnauthorizedAccessException("Invalid password.");
                }
                return isValidPassword ? result.UserName : null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserRepository::ValidateUser InputParameters: UserName={UserName}", userName);
                throw;
            }
        }

        public async Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = await _dbConnectionFactory.GetQuickDeskConnection(cancellationToken);
                var parameters = new DynamicParameters();
                parameters.Add("@UserName", userName);
                var result = await connection.QueryFirstOrDefaultAsync<User>(
                    DBQueries.GetUserByUsername,
                    parameters,
                    commandType: CommandType.Text);
                return result ?? new User();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserRepository::GetUserByUsername InputParameters: UserName={UserName}", userName);
                throw;
            }
        }
    }
}
