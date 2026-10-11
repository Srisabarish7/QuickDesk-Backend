using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Infrastructure.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string to, string subject, string message, CancellationToken cancellationToken);
    }
}
