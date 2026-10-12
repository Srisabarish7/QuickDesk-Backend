using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.ResponseDtos
{
    public class GetJobsDto : BaseDto
    {
        public long JobId { get; set; }
        public string JobName { get; set; }
        public string JobStatus { get; set; }
        public long Progress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
}
