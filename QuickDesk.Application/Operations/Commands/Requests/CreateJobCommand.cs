using MediatR;
using QuickDesk.Application.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Operations.Commands.Requests
{
    public class CreateJobCommand : IRequest<JobDto>
    {
        public long? UserId { get; set; }
        public long JobTypeId { get; set; }
        public long? JobStatusId { get; set; }
        public DateTime? ScheduledAt { get; set; } = null;
    }
}
