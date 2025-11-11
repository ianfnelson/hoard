using Hoard.Api.Models.Valuations;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ValuationsController(IMediator mediator, ILogger<ValuationsController> logger) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillHoldingsAsync([FromBody] BackfillValuationsRequest model)
    {
        logger.LogInformation("Received request to backfill valuations.");
        
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Backfill valuations triggered." });
    }

    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateValuationsAsync([FromBody] CalculateValuationsRequest model)
    {
        logger.LogInformation("Received request to calculate valuations.");

        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Calculate valuations triggered." });
    }
}