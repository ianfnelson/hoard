using Hoard.Core.Application;
using Hoard.Core.Application.Cashflows;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CashflowsController(IMediator mediator, ILogger<CashflowsController> logger) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillHoldingsAsync()
    {
        logger.LogInformation("Received request to backfill cashflow.");
        
        await mediator.SendAsync(new TriggerBackfillCashflowCommand(Guid.NewGuid()));
        
        return Accepted(new { message = "Backfill cashflow triggered." });
    }
}