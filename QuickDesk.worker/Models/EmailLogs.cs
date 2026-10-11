using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Models
{
    public class EmailLogs
    {
        public long EmailLogId { get; set; }
        public long UserId { get; set; }
        public long JobId { get; set; }
        public string JobName { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
