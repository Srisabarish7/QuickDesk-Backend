using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure.Interfaces
{
    public interface IQueryService
    {
        Task<(string? UserName, string? Email)> GetUserDetail(long userId, CancellationToken ct);
    }
}
