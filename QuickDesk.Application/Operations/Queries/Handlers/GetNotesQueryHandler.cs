using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using QuickDesk.Application.Operations.Queries.Requests;
using QuickDesk.Application.ResponseDtos;
using QuickDesk.Domain.Common.ExceptionHandling;
using QuickDesk.Domain.Entities;
using QuickDesk.Infrastructure.Interfaces;

namespace QuickDesk.Application.Operations.Queries.Handlers
{
    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, List<NotesDto>>
    {
        private readonly ILogger<GetNotesQueryHandler> _logger;
        private readonly IJobRepository _jobRepository;
        private readonly IMapper _mapper;

        public GetNotesQueryHandler(ILogger<GetNotesQueryHandler> logger, IJobRepository jobRepository, IMapper mapper)
        {
            _logger = logger;
            _jobRepository = jobRepository;
            _mapper = mapper;
        }

        public async Task<List<NotesDto>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            await CheckUserExistence(request.UserId, cancellationToken);
            var notes = await GetNotes(request, cancellationToken);
            return CreateSuccessMessage(notes);
        }

        public async Task CheckUserExistence(long userId, CancellationToken cancellationToken)
        {
            try
            {
                if (userId > 0)
                {
                    var userExists = await _jobRepository.CheckUserExist(userId, cancellationToken);
                    if (!userExists)
                    {
                        throw new UnauthorizedCustomException(["User does not exist"]);
                    }
                }
            }
            catch (Exception ex) { 
                _logger.LogError(ex, "Exception thrown in GetNotesQueryHandler:CheckUserExistence Input:{userId}", userId); 
                throw;
            }
        }

        public async Task<List<Notes>> GetNotes(GetNotesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mapper = _mapper.Map<GetNotes>(request);                
                var result =await _jobRepository.GetNotes(mapper, cancellationToken);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in GetNotesQueryHandler:GetNotes Input:{request}", request);
                throw;
            }
        }

        private List<NotesDto> CreateSuccessMessage(List<Notes> notes)
        {
            if (notes == null || notes.Count == 0)
            {
                throw new BadRequestCustomException(new List<string> { "No notes found." });
            }

            return notes.Select(notes => new NotesDto
            {
                RequestId =Guid.NewGuid().ToString(),
                RequestMessage = "Notes retrieved successfully.",
                Title = notes.Title,
                Description = notes.Description,
                CreatedAt = notes.CreatedAt,
                ScheduledAt = notes.ScheduledAt
            }).ToList();            
        }
    }
}
