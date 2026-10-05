using QuickDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Infrastructure.Interfaces
{
    public interface IJobRepository
    {
        Task<Job> CreateJob(CreateJob request, CancellationToken cancellationToken);
        Task<bool> CheckUserExist(long userId, CancellationToken cancellationToken);
    }
}
