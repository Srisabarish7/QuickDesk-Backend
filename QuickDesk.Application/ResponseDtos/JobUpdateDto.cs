using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.ResponseDtos
{
    public class JobUpdateDto
    {
        public long JobId { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string UpdatedAt { get; set; }
    }
}
