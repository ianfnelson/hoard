using Hoard.Api.Models.Holdings;
using Hoard.Core.Messages.Holdings;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HoldingsController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger<ValuationsController> _logger;
    
    public HoldingsController(IBus bus, ILogger<ValuationsController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Triggers a calculation of all holdings for a given date range
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="BackfillHoldingsCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillHoldingsAsync([FromBody] BackfillHoldingsRequest model)
    {
        _logger.LogInformation("Received request to backfill holdings.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Backfill holdings triggered." });
    }

    /// <summary>
    /// Triggers calculation of all holdings
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="CalculateHoldingsCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateHoldingsAsync([FromBody] CalculateHoldingsRequest model)
    {
        _logger.LogInformation("Received request to calculate holdings.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Calculate holdings triggered." });
    }
}