using Newtonsoft.Json.Linq;
using QuickDesk.worker.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.worker.Models.Pipeline
{
    public sealed class ProcessingContext
    {
        public ProcessingContext(QuickDeskWorkerItem item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public QuickDeskWorkerItem Item { get; }
        public long JobId => Item.JobId;
        public long JobTypeId => Item.JobTypeId;
        public long UserId => Item.UserId;
        public JobTypeEnum JobType { get; set; } = JobTypeEnum.Unknown;
        public string? DetailsJson => Item.DetailsJson;
        public JObject Details { get; set; } = new JObject();
        public List<string> Errors { get; } = new List<string>();
        public Dictionary<string, object> ExtraState { get; } = [];


    }
}
