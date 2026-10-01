using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuickDesk.Application.Operations.Commands.Requests;

namespace QuickDesk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] AddUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Failed to add user.");
            }
        }
    }
}
