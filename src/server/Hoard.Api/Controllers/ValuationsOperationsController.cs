using Hoard.Api.Models.Valuations;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/valuations/")]
[Produces("application/json")]
[Tags("Operations")]
public class ValuationsOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillValuationsAsync([FromBody] BackfillValuationsRequest model)
    {
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Backfill valuations triggered." });
    }

    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateValuationsAsync([FromBody] CalculateValuationsRequest model)
    {
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Calculate valuations triggered." });
    }
}