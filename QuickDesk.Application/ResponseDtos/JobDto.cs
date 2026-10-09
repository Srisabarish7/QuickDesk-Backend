using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace QuickDesk.Application.ResponseDtos
{
    public class JobDto : BaseDto
    {
        public long JobId { get; set; }
        public long UserId { get; set; }
        public string JobName { get; set; }
        public string JobStatus { get; set; }
        public int Progress { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ErrorMessage { get; set; }
        public string DetailsJson { get; set; }
    }
}
