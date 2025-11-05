using Hoard.Core;
using Hoard.Core.Data;
using Hoard.Core.Domain;
using Hoard.Core.Extensions;
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
    IHandleMessages<PricesBackfilledEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<BackfillPricesSaga> _logger;
    private readonly HoardContext _context;

    private const int DaysPerChunk = 90;

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
        cfg.Correlate<BackfillPricesCommand>(m => m.BatchId, d => d.BatchId);
        cfg.Correlate<PricesBackfilledEvent>(m => m.BatchId, d => d.BatchId);
    }

    public async Task Handle(BackfillPricesCommand message)
    {
        var instruments = await GetTargetInstrumentsAsync(message.InstrumentId);

        foreach (var instrument in instruments)
        {
            var dateRange = await GetDateRange(instrument, message);
            var rangeChunks = dateRange.ChunkByDays(DaysPerChunk).ToList();
            Data.PendingRanges[instrument.Id] = rangeChunks;

            var delay = TimeSpan.Zero;
            foreach (var chunk in rangeChunks)
            {
                var command = new BackfillPricesBatchCommand(
                    Data.BatchId, instrument.Id, chunk.StartDate, chunk.EndDate);
                await _bus.DeferLocal(delay, command);
                delay+=TimeSpan.FromSeconds(5);
            }
        }   
        _logger.LogInformation("Started backfill prices saga {BatchId} for {Count} instruments",
            Data.BatchId, instruments.Count);
    }
    
    public Task Handle(PricesBackfilledEvent message)
    {
        if (!Data.PendingRanges.TryGetValue(message.InstrumentId, out var ranges))
        {
            return Task.CompletedTask;
        }
        
        ranges.RemoveAll(r => r.StartDate == message.StartDate && r.EndDate == message.EndDate);

        if (ranges.Count == 0)
        {
            Data.PendingRanges.Remove(message.InstrumentId);
        }
        
        if (Data.PendingRanges.Count == 0)
        {
            _logger.LogInformation("Price backfill saga {BatchId} complete", Data.BatchId);
            MarkAsComplete();
        }

        return Task.CompletedTask;
    }

    private async Task<DateRange> GetDateRange(Instrument instrument, BackfillPricesCommand message)
    {
        var startDate = message.StartDate ?? await GetStartDate(instrument);
        var endDate = message.EndDate ?? await GetEndDate(instrument);
        
        return new DateRange(startDate, endDate);
    }

    private async Task<DateOnly> GetStartDate(Instrument instrument)
    {
        if (instrument.InstrumentType.IsFxPair)
        {
            var earliestTradeDate = await _context.Transactions
                .OrderBy(t => t.Date)
                .Select(x => x.Date)
                .FirstOrDefaultAsync();

            return earliestTradeDate.OrOneYearAgo();
        }
        
        var earliestHoldingDate = await _context.Holdings
            .Where(x => x.InstrumentId == instrument.Id)
            .OrderBy(x => x.AsOfDate)
            .Select(x => x.AsOfDate)
            .FirstOrDefaultAsync();

        return earliestHoldingDate.OrOneYearAgo();
    }

    private async Task<DateOnly> GetEndDate(Instrument instrument)
    {
        if (instrument.InstrumentType.IsFxPair)
        {
            return DateOnlyHelper.TodayLocal();
        }
        
        var latestHoldingDate = await _context.Holdings
            .Where(x => x.InstrumentId == instrument.Id)
            .OrderByDescending(x => x.AsOfDate)
            .Select(x => x.AsOfDate)
            .FirstOrDefaultAsync();

        return latestHoldingDate.OrToday();
    }
    
    private async Task<List<Instrument>> GetTargetInstrumentsAsync(int? instrumentId)
    {
        if (!instrumentId.HasValue)
        {
            return await _context.Instruments
                .Include(x => x.InstrumentType)
                .Where(x => x.EnablePriceUpdates)
                .Where(x => x.TickerApi != null)
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
        
        return [instrument];
    }
}

public class BackfillPricesSagaData : SagaData
{
    public Guid BatchId { get; set; }
    public Dictionary<int, List<DateRange>> PendingRanges { get; set; } = new();
}