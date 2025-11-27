using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Hoard.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PositionsController(IMediator mediator, ILogger<PositionsController> logger) : ControllerBase
{
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculatePositionsAsync()
    {
        logger.LogInformation("Received request to calculate positions.");

        await mediator.SendAsync(new TriggerCalculatePositionsCommand(Guid.NewGuid(), PipelineMode.DaytimeReactive));

        return Accepted(new { message = "Calculate positions triggered." });
    }
}