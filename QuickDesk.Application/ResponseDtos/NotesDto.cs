using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.ResponseDtos
{
    public class NotesDto : BaseDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }
}
