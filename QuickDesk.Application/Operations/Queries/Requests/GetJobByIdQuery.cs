using MediatR;
using QuickDesk.Application.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Operations.Queries.Requests
{
    public class GetJobByIdQuery : IRequest<JobByIdDto>
    {
        public long JobId { get; set; }
        public long UserId { get; set; }
    }
}
