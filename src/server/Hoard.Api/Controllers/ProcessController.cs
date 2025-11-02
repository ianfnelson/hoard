using Hoard.Api.Models.Process;
using Hoard.Core.Messages;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProcessController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger<ProcessController> _logger;

    public ProcessController(IBus bus, ILogger<ProcessController> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    
    /// <summary>
    /// Triggers a batch refresh of all quotes.
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="RefreshQuotesCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("refresh-quotes")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshQuotesAsync()
    {
        _logger.LogInformation("Received request to refresh quotes.");

        await _bus.Send(new RefreshQuotesCommand());
        return Accepted(new { message = "Quote refresh triggered." });
    }

    /// <summary>
    /// Triggers a fetch of all daily prices
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="FetchDailyPricesCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("fetch-daily-prices")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> FetchDailyPricesAsync([FromBody] FetchDailyPricesRequest model)
    {
        _logger.LogInformation("Received request to fetch daily prices.");

        await _bus.Send(new FetchDailyPricesCommand(model.AsOfDate));
        return Accepted(new { message = "Daily prices fetch triggered." });
    }
    
    /// <summary>
    /// Triggers a recalculation of all holdings
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="RecalculateHoldingsCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("recalculate-holdings")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RecalculateHoldingsAsync([FromBody] RecalculateHoldingsRequest model)
    {
        _logger.LogInformation("Received request to recalculate holdings.");

        await _bus.Send(new RecalculateHoldingsCommand(model.AsOfDate));
        return Accepted(new { message = "Recalculate holdings triggered." });
    }
    
    /// <summary>
    /// Triggers a recalculation of all holdings for a given date range
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="BackfillHistoricalHoldingsCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("backfill-holdings")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillHoldingsAsync([FromBody] BackfillHoldingsRequest model)
    {
        _logger.LogInformation("Received request to backfill holdings.");

        await _bus.Send(new BackfillHistoricalHoldingsCommand(Guid.NewGuid(), model.StartDate, model.EndDate));
        return Accepted(new { message = "Backfill holdings triggered." });
    }
    
    /// <summary>
    /// Triggers a recalculation of all valuations
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="RecalculateValuationsCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("recalculate-valuations")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RecalculateValuationsAsync([FromBody] RecalculateValuationsRequest model)
    {
        _logger.LogInformation("Received request to recalculate valuations.");

        await _bus.Send(new RecalculateValuationsCommand(model.AsOfDate));
        return Accepted(new { message = "Recalculate valuations triggered." });
    }
}