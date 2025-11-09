using Hoard.Core.Data;
using Hoard.Core.Extensions;
using Hoard.Messages.Valuations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateValuationsSaga :
    Saga<CalculateValuationsSagaData>,
    IAmInitiatedBy<StartCalculateValuationsSagaCommand>,
    IHandleMessages<HoldingValuationCalculatedEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<CalculateValuationsSaga> _logger;
    private readonly HoardContext _context;

    public CalculateValuationsSaga(IBus bus, ILogger<CalculateValuationsSaga> logger, HoardContext context)
    {
        _bus = bus;
        _logger = logger;
        _context = context;
    }

    protected override void CorrelateMessages(ICorrelationConfig<CalculateValuationsSagaData> cfg)
    {
        cfg.Correlate<StartCalculateValuationsSagaCommand>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);

        cfg.Correlate<HoldingValuationCalculatedEvent>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);
    }

    public async Task Handle(StartCalculateValuationsSagaCommand message)
    {
        Data.CorrelationId = message.CorrelationId;

        var asOfDate = message.AsOfDate.OrToday();
        Data.AsOfDate = asOfDate;

        var holdingIds = await GetHoldingIdsAsync(asOfDate);

        if (holdingIds.Count == 0)
        {
            MarkAsComplete();
            return;
        }

        _logger.LogInformation("Started calculate valuations saga {CorrelationKey} for {Count} holdings",
            Data.CorrelationKey, holdingIds.Count);

        Data.PendingHoldings = holdingIds.ToHashSet();

        foreach (var holdingId in holdingIds)
        {
            var command = new CalculateHoldingValuationBusCommand(Data.CorrelationId, holdingId);
            await _bus.SendLocal(command);
        }
    }

    private async Task<List<int>> GetHoldingIdsAsync(DateOnly asOfDate)
    {
        return await _context.Holdings
            .Where(x => x.AsOfDate == asOfDate)
            .Select(x => x.Id)
            .ToListAsync();
    }

    public async Task Handle(HoldingValuationCalculatedEvent message)
    {
        Data.PendingHoldings.Remove(message.HoldingId);
        if (Data.PendingHoldings.Count == 0)
        {
            await CalculatePortfolioValuations();

            await _bus.Publish(new ValuationsCalculatedEvent(Data.CorrelationId, Data.AsOfDate));

            _logger.LogInformation("Calculate valuations saga {CorrelationKey} complete", Data.CorrelationKey);
            MarkAsComplete();
        }
    }

    private async Task CalculatePortfolioValuations()
    {
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var parameters = new[]
            {
                new SqlParameter("@AsOfDate", Data.AsOfDate.ToDateTime(TimeOnly.MinValue))
            };

            var result =
                await _context.Database.ExecuteSqlRawAsync("EXEC CalculatePortfolioValuations @AsOfDate", parameters);

            sw.Stop();

            _logger.LogInformation(
                "Portfolio valuations calculated for {Date} ({Count} rows affected) in {Elapsed} ms",
                Data.AsOfDate.ToIsoDateString(), result, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate Portfolio valuations for {Date}",
                Data.AsOfDate.ToIsoDateString());
            throw;
        }
    }
}

public class CalculateValuationsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public DateOnly AsOfDate { get; set; }
    
    public string CorrelationKey => $"{CorrelationId:N}:{AsOfDate}";
    public HashSet<int> PendingHoldings { get; set; } = new();
}