using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Messages.Holdings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Holdings;

public record ProcessCalculateHoldingsCommand(Guid CorrelationId, DateOnly? AsOfDate) : ICommand;

public class ProcessCalculateHoldingsHandler(
    IBus bus,
    ILogger<ProcessCalculateHoldingsHandler> logger,
    HoardContext context)
    : ICommandHandler<ProcessCalculateHoldingsCommand>
{
    public async Task HandleAsync(ProcessCalculateHoldingsCommand command, CancellationToken ct = default)
    {
        var asOfDate = command.AsOfDate.OrToday();
        
        logger.LogInformation("Starting holdings calculation for {Date}", asOfDate.ToIsoDateString());

        await CalculateHoldings(asOfDate);

        await bus.Publish(new HoldingsCalculatedEvent(command.CorrelationId, asOfDate));
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
                
            var result = await context.Database.ExecuteSqlRawAsync("EXEC CalculateHoldings @AsOfDate", parameters);

            sw.Stop();

            logger.LogInformation(
                "Holdings calculated for {Date} ({Count} rows affected) in {Elapsed} ms",
                asOfDate.ToIsoDateString(), result, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate holdings for {Date}", asOfDate.ToIsoDateString());
            throw;
        }
    }
}