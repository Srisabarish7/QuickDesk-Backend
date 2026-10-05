using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QuickDesk.Application.Operations.Commands.Requests;
using QuickDesk.Application.ResponseDtos;
using QuickDesk.Domain.Common.ExceptionHandling;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuickDesk.Application.Operations.Commands.Handlers
{
    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddUserCommandHandler> _logger;
        private readonly IConfiguration _configuration;

        public AddUserCommandHandler(IUserRepository userRepository, IMapper mapper, ILogger<AddUserCommandHandler> logger, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<UserDto> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await CheckUserExist(request, cancellationToken);
                var user = await AddNewUser(request, cancellationToken);
                var token = GenerateToken(user, cancellationToken);
                return CreateSuccessResponseMessage(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserCommandHandler:Handle, Input: {Input}", request);
                throw;
            }
        }

        public async Task CheckUserExist(AddUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userExists = await _userRepository.CheckUserExists(request.UserName, request.Email, cancellationToken);
                if (userExists.UserName)
                {
                    throw new BadRequestCustomException(["UserName already exists."]);
                }
                else if (userExists.Email)
                {
                    throw new BadRequestCustomException(["Email already exists."]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserCommandHandler:CheckUserExist, Input: {Input}", request);
                throw;
            }
        }

        public async Task<User> AddNewUser(AddUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = _mapper.Map<AddUser>(request);
                return await _userRepository.AddUser(user, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserCommandHandler:AddNewUser, Input: {Input}", request);
                throw;
            }
        }

        private string GenerateToken(User user, CancellationToken cancellationToken)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in UserCommandHandler:GenerateToken, Input: {Input}", user);
                throw;
            }
        }

        private UserDto CreateSuccessResponseMessage(string token)
        {
            return new UserDto
            {
                accessToken = token,
                RequestId = Guid.NewGuid().ToString(),
                RequestMessage = "User created successfully.",
            };
        }
    }
}
