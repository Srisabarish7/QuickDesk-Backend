using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QuickDesk.Application.Operations.Commands.Requests;
using QuickDesk.Application.ResponseDtos;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuickDesk.Application.Operations.Commands.Handlers
{
    public class SignInCommandHandler : IRequestHandler<SignInCommand, UserDto>
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SignInCommandHandler> _logger;
        private readonly IConfiguration _configuration;

        public SignInCommandHandler(IMapper mapper, IUserRepository userRepository, ILogger<SignInCommandHandler> logger, IConfiguration configuration)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<UserDto> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateUser(request, cancellationToken);
                var user = await GetUserByUserName(request.UserName, cancellationToken);
                var token = GenerateToken(user, cancellationToken);
                return CreateSuccessResponseMessage(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SignInCommandHandler:Handle, Input: {Input}", request);
                throw;
            }
        }

        public async Task ValidateUser(SignInCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.ValidateUser(request.UserName, request.Password, cancellationToken);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SignInCommandHandler:ValidateUser, Input: {Input}", request);
                throw;
            }
        }

        public async Task<User> GetUserByUserName(string userName, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetUserByUserName(userName, cancellationToken);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid username or password.");
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in SignInCommandHandler:GetUserByUserName, Input: {Input}", userName);
                throw;
            }
        }

        private string GenerateToken(User user, CancellationToken cancellationToken)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim("UserName", user.UserName),
                new Claim("Email", user.Email),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserDto CreateSuccessResponseMessage(string token)
        {
            return new UserDto
            {
                accessToken = token,
                RequestId = Guid.NewGuid().ToString(),
                RequestMessage = "Sign-in successful."
            };
        }
    }
}
