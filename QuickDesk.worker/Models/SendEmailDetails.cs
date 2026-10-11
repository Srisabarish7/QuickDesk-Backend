using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Models
{
    public class SendEmailDetails
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

    }
}
