using Hoard.Core;
using Hoard.Core.Data;
using Hoard.Core.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Prices;

public class BackfillPricesSaga :
    Saga<BackfillPricesSagaData>,
    IAmInitiatedBy<BackfillPricesCommand>,
    IHandleMessages<PriceRefreshedEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<BackfillPricesSaga> _logger;
    private readonly HoardContext _context;

    public BackfillPricesSaga(
        HoardContext context,
        IBus bus, 
        ILogger<BackfillPricesSaga> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }
    
    protected override void CorrelateMessages(ICorrelationConfig<BackfillPricesSagaData> cfg)
    {
        cfg.Correlate<BackfillPricesCommand>(m => m.CorrelationId, d => d.CorrelationId);
        cfg.Correlate<PriceRefreshedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(BackfillPricesCommand message)
    {
        Data.CorrelationId = message.CorrelationId;
        
        var instrumentIds = await GetTargetInstrumentIdsAsync(message.InstrumentId);

        _logger.LogInformation("Started backfill prices saga {CorrelationId} for {Count} instruments",
            Data.CorrelationId, instrumentIds.Count);
        
        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        var dateRange = GetDateRange(message);
        
        var delay = TimeSpan.Zero;
        
        foreach (var instrumentId in instrumentIds)
        {
            var command =
                new RefreshPricesBatchCommand(Data.CorrelationId, instrumentId, dateRange.StartDate, dateRange.EndDate);
            await _bus.DeferLocal(delay, command);
            delay+=TimeSpan.FromSeconds(5);
        }   
    }
    
    public Task Handle(PriceRefreshedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            _logger.LogInformation("Price backfill saga {CorrelationId} complete", Data.CorrelationId);
            MarkAsComplete();
        }

        return Task.CompletedTask;
    }

    private static DateRange GetDateRange(BackfillPricesCommand message)
    {
        var startDate = message.StartDate ?? DateOnlyHelper.EpochLocal();
        var endDate = message.EndDate ?? DateOnlyHelper.TodayLocal();
        
        return new DateRange(startDate, endDate);
    }
    
    private async Task<List<int>> GetTargetInstrumentIdsAsync(int? instrumentId)
    {
        if (!instrumentId.HasValue)
        {
            return await _context.Instruments
                .Include(x => x.InstrumentType)
                .Where(x => x.EnablePriceUpdates)
                .Where(x => x.TickerApi != null)
                .Select(x => x.Id)
                .ToListAsync();
        }
        
        var instrument = await _context.Instruments
            .Include(x => x.InstrumentType)
            .FirstOrDefaultAsync(x => x.Id == instrumentId.Value);

        if (instrument == null)
        {
            _logger.LogWarning("Instrument with id {InstrumentId} not found", instrumentId);
            return [];
        }
        
        return [instrumentId.Value];
    }
}

public class BackfillPricesSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}