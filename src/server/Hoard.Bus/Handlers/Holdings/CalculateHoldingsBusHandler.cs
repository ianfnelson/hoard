using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Messages.Holdings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Handlers.Holdings;

public class CalculateHoldingsBusHandler : IHandleMessages<CalculateHoldingsBusCommand>
{
    private readonly IBus _bus;
    private readonly HoardContext _context;
    private readonly ILogger<CalculateHoldingsBusHandler> _logger;

    public CalculateHoldingsBusHandler(IBus bus,  ILogger<CalculateHoldingsBusHandler> logger, HoardContext context)
    {
        _bus = bus;
        _logger = logger;
        _context = context;
    }
    
    public async Task Handle(CalculateHoldingsBusCommand message)
    {
        var asOfDate = message.AsOfDate.OrToday();
        
        _logger.LogInformation("Starting holdings calculation for {Date}", asOfDate.ToIsoDateString());

        await CalculateHoldings(asOfDate);

        await _bus.Publish(new HoldingsCalculatedEvent(message.CorrelationId, asOfDate));
    }

    private async Task CalculateHoldings(DateOnly asOfDate)
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var parameters = new[]
            {
                new SqlParameter("@AsOfDate", asOfDate.ToDateTime(TimeOnly.MinValue))
            };
                
            var result = await _context.Database.ExecuteSqlRawAsync("EXEC CalculateHoldings @AsOfDate", parameters);

            sw.Stop();

            _logger.LogInformation(
                "Holdings calculated for {Date} ({Count} rows affected) in {Elapsed} ms",
                asOfDate.ToIsoDateString(), result, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate holdings for {Date}", asOfDate.ToIsoDateString());
            throw;
        }
    }
}