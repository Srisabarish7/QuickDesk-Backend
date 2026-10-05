using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickDesk.Application.Operations.Commands.Requests;

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
    }
}
