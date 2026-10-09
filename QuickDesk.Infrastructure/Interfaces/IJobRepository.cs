using QuickDesk.Domain.Entities;

namespace QuickDesk.Infrastructure.Interfaces
{
    public interface IJobRepository
    {
        Task<Job> CreateJob(CreateJob request, CancellationToken cancellationToken);
        Task<bool> CheckUserExist(long userId, CancellationToken cancellationToken);
    }
}
