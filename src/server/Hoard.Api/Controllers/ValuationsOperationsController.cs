using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/operations/valuations/")]
[Produces("application/json")]
[Tags("Operations")]
public class ValuationsOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillValuationsAsync([FromBody] TriggerBackfillValuationsCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Backfill valuations triggered." });
    }

    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateValuationsAsync([FromBody] TriggerCalculateValuationsCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Calculate valuations triggered." });
    }
}