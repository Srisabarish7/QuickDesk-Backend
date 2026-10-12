using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Domain.Entities
{
    public class GetJobByIdRequest
    {
        public long JobId { get; set; }
        public long UserId { get; set; }
    }
}
