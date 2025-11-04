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
    /// Triggers a fetch of prices for active instruments and currencies.
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="FetchPricesCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("fetch")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> FetchPricesAsync([FromBody] FetchPricesRequest model)
    {
        _logger.LogInformation("Received request to fetch prices.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Prices fetch triggered." });
    }
}