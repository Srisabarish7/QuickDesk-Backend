using MediatR;
using QuickDesk.Application.ResponseDtos;
using System.Text.Json;

namespace QuickDesk.Application.Operations.Commands.Requests
{
    public class CreateJobCommand : IRequest<JobDto>
    {
        public long? UserId { get; set; }
        public long JobTypeId { get; set; }
        public long? JobStatusId { get; set; }
        public JsonElement DetailsJson { get; set; }
        public DateTime? ScheduledAt { get; set; } = null;
    }
}
