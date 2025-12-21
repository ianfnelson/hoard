using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/positions/")]
[Tags("Operations")]
[Produces("application/json")]
public class PositionsOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculatePositionsAsync()
    {
        var command = new TriggerCalculatePositionsCommand();
        
        await mediator.SendAsync(command);

        return Accepted(new { message = "Calculate positions triggered." });
    }
}