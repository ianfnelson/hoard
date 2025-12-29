using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/operations/holdings/")]
[Tags("Operations")]
[Produces("application/json")]
public class HoldingsOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillHoldingsAsync([FromBody] TriggerBackfillHoldingsCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Backfill holdings triggered." });
    }
    
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateHoldingsAsync([FromBody] TriggerCalculateHoldingsCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Calculate holdings triggered." });
    }
}