using Hoard.Api.Models.Prices;
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
    
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillPricesAsync([FromBody] BackfillPricesRequest model)
    {
        _logger.LogInformation("Received request to backfill prices.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Prices backfill triggered." });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshPricesAsync([FromBody] RefreshPricesRequest model)
    {
        _logger.LogInformation("Received request to refresh prices.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Prices refresh triggered." });
    }
}