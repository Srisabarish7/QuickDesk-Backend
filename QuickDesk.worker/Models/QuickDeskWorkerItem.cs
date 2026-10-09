using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Models
{
    public class QuickDeskWorkerItem
    {
        public long JobId { get; set; }
        public long UserId { get; set; }
        public long JobTypeId { get; set; }
        public string? JobName { get; set; }
        public int JobStatusId { get; set; }
        public string? JobStatus { get; set; }
        public int Progress { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public long? JobDetailId { get; set; }
        public string? DetailsJson { get; set; }
    }
}
