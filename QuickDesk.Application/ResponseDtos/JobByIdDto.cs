using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.ResponseDtos
{
    public class JobByIdDto : BaseDto
    {
        public long JobId { get; set; }
        public string JobName { get; set; }
        public string UserName { get; set; }
        public string JobStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public int Progress { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Receipent { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
