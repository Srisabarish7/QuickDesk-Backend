using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Models
{
    public class AddEmailLog
    {
        public long UserId { get; set; }
        public long JobId { get; set; }
        public long JobTypeId { get; set; }
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Title { get; set;}
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? CreatedAt { get; set; }       
    }
}
