using MediatR;
using QuickDesk.Application.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Operations.Commands.Requests
{
    public class SignInCommand : IRequest<UserDto>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
