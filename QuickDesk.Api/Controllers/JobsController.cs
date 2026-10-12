using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickDesk.Application.Operations.Commands.Requests;
using QuickDesk.Application.Operations.Queries.Handlers;
using QuickDesk.Application.Operations.Queries.Requests;

namespace QuickDesk.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        public IMediator _meditor;

        public JobsController(IMediator meditor)
        {
            _meditor = meditor;
        }

        [HttpPost("CreateJob")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobCommand command, CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }
            command.UserId = long.Parse(userId);
            var result = await _meditor.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpGet("GetNotes")]
        public async Task<IActionResult> GetNotes([FromQuery] GetNotesQuery query, CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }
            query.UserId = long.Parse(userId);
            var result = await _meditor.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("GetJobs")]
        public async Task<IActionResult> GetJobs([FromQuery] GetJobsQuery query, CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }
            query.UserId = long.Parse(userId);
            var result = await _meditor.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("GetJobById")]
        public async Task<IActionResult> GetJobById([FromQuery] GetJobByIdQuery query, CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }
            query.UserId = long.Parse(userId);
            var result = await _meditor.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
