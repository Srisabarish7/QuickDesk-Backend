using System.Text.Json;

namespace QuickDesk.Domain.Entities
{
    public class CreateJob
    {
        public long UserId { get; set; }
        public long JobTypeId { get; set; }
        public long JobStatusId { get; set; }
        public DateTime? ScheduledAt { get; set; } = null;
        public JsonElement DetailsJson { get; set; }
    }
}
