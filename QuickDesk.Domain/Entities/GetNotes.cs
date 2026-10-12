using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Domain.Entities
{
    public class GetNotes
    {
        public long UserId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? SearchString { get; set; }
    }
}
