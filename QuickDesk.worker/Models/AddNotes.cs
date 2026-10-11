using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Models
{
    public class AddNotes
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
