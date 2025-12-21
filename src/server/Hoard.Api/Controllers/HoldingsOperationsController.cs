using Hoard.Api.Models.Holdings;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/holdings/")]
[Tags("Operations")]
[Produces("application/json")]
public class HoldingsOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillHoldingsAsync([FromBody] BackfillHoldingsRequest model)
    {
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Backfill holdings triggered." });
    }
    
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateHoldingsAsync([FromBody] CalculateHoldingsRequest model)
    {
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Calculate holdings triggered." });
    }
}