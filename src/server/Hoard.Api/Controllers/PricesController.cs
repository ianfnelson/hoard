using Hoard.Api.Models.Prices;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PricesController(IMediator mediator, ILogger<PricesController> logger) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillPricesAsync([FromBody] BackfillPricesRequest model)
    {
        logger.LogInformation("Received request to backfill prices.");
        
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Prices backfill triggered." });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshPricesAsync([FromBody] RefreshPricesRequest model)
    {
        logger.LogInformation("Received request to refresh prices.");

        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Prices refresh triggered." });
    }
}