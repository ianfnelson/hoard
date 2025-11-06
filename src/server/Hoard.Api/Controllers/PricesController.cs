using Hoard.Api.Models.Prices;
using Hoard.Core.Messages.Prices;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PricesController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger<PricesController> _logger;

    public PricesController(IBus bus, ILogger<PricesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }
    
    /// <summary>
    /// Triggers a backfill of prices.
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="BackfillPricesCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillPricesAsync([FromBody] BackfillPricesRequest model)
    {
        _logger.LogInformation("Received request to backfill prices.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Prices backfill triggered." });
    }
    
    /// <summary>
    /// Triggers a refresh of prices for active instruments and currencies.
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="RefreshPricesCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshPricesAsync([FromBody] RefreshPricesRequest model)
    {
        _logger.LogInformation("Received request to refresh prices.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Prices refresh triggered." });
    }
}