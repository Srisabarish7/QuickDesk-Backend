using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Domain.Entities
{
    public class Notes
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
}
