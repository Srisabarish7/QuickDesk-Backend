using MediatR;
using QuickDesk.Application.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Operations.Queries.Requests
{
    public class GetJobsQuery : IRequest<List<GetJobsDto>>
    {
        public long UserId { get; set; }
        public long? JobTypeId { get; set; }
        public long? JobStatusId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
