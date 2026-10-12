using QuickDesk.Domain.Entities;

namespace QuickDesk.Infrastructure.Interfaces
{
    public interface IJobRepository
    {
        Task<Job> CreateJob(CreateJob request, CancellationToken cancellationToken);
        Task<bool> CheckUserExist(long userId, CancellationToken cancellationToken);
        Task<IEnumerable<Notes>> GetNotes (GetNotes request, CancellationToken cancellationToken);
        Task<IEnumerable<GetJobsResponse>> GetJobs (GetJobsRequest request, CancellationToken cancellationToken);
        Task<GetJobById> GetJobById(GetJobByIdRequest request, CancellationToken cancellationToken);
    }
}
