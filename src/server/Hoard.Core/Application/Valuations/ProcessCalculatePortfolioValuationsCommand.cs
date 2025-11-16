using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Messages.Valuations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Valuations;

public record ProcessCalculatePortfolioValuationsCommand(Guid CorrelationId, DateOnly AsOfDate, bool IsBackfill)
: ICommand;

public class ProcessCalculatePortfolioValuationsHandler(
    HoardContext context, 
    IBus bus,
    ILogger<ProcessCalculatePortfolioValuationsHandler> logger)
    : ICommandHandler<ProcessCalculatePortfolioValuationsCommand>
{
    public async Task HandleAsync(ProcessCalculatePortfolioValuationsCommand command, CancellationToken ct = default)
    {
        await CalculatePortfolioValuations(command.AsOfDate, ct);
        
        await bus.Publish(new ValuationsCalculatedEvent(command.CorrelationId, command.AsOfDate, command.IsBackfill));
    }
    
    private async Task CalculatePortfolioValuations(DateOnly asOfDate, CancellationToken ct = default)
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var parameters = new[]
            {
                new SqlParameter("@AsOfDate", asOfDate.ToDateTime(TimeOnly.MinValue))
            };

            var result =
                await context.Database.ExecuteSqlRawAsync("EXEC CalculatePortfolioValuations @AsOfDate", parameters);

            sw.Stop();

            logger.LogInformation(
                "Portfolio valuations calculated for {Date} ({Count} rows affected) in {Elapsed} ms",
                asOfDate.ToIsoDateString(), result, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate Portfolio valuations for {Date}",
                asOfDate.ToIsoDateString());
            throw;
        }
    }
}