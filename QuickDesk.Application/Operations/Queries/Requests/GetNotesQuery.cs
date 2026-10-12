using MediatR;
using QuickDesk.Application.ResponseDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Application.Operations.Queries.Requests
{
    public class GetNotesQuery : IRequest<List<NotesDto>>
    {
        public long UserId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? SearchString { get; set; }
    }
}
