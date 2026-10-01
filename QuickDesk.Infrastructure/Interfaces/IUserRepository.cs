using QuickDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddUser(AddUser user, CancellationToken cancellationToken);
        Task<CheckUserExists> CheckUserExists(string userName, string email, CancellationToken cancellationToken);
    }
}
