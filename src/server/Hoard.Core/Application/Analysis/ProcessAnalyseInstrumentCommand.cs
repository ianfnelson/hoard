using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application.Analysis;

public record ProcessAnalyseInstrumentCommand(
    Guid AnalysisRunId,
    PipelineMode PipelineMode,
    int InstrumentId,
    DateOnly AsOfDate) : ICommand;

public class ProcessAnalyseInstrumentHandler(
    IBus bus,
    HoardContext context,
    ILogger<ProcessAnalyseInstrumentHandler> logger)
    : ICommandHandler<ProcessAnalyseInstrumentCommand>
{
    public async Task HandleAsync(ProcessAnalyseInstrumentCommand command, CancellationToken ct = default)
    {
        var prices = await LoadPricesAsync(command.InstrumentId, command.AsOfDate, ct);

        if (prices.Count == 0)
        {
            logger.LogWarning("No prices found for instrument {InstrumentId} in 52-week window", command.InstrumentId);
            await PublishEventAsync(command, AnalysisFlags.None);
            return;
        }

        var result = Analyse(prices);

        await UpsertAnalysisAsync(command.InstrumentId, command.AsOfDate, result, ct);
        await PublishEventAsync(command, result.Flags);

        logger.LogInformation(
            "Analysis complete for instrument {InstrumentId}: High={IsHigh}, Low={IsLow}, AbnVol={IsAbnVol}",
            command.InstrumentId, result.Flags.IsNewHigh, result.Flags.IsNewLow, result.Flags.IsAbnormalVolume);
    }

    private async Task<List<Price>> LoadPricesAsync(int instrumentId, DateOnly asOfDate, CancellationToken ct)
    {
        var fiftyTwoWeeksAgo = asOfDate.AddDays(-364);

        return await context.Prices
            .Where(p => p.InstrumentId == instrumentId)
            .Where(p => p.AsOfDate >= fiftyTwoWeeksAgo && p.AsOfDate <= asOfDate)
            .OrderBy(p => p.AsOfDate)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    private static AnalysisResult Analyse(List<Price> prices)
    {
        var today = prices.Last();
        var prior = prices.Where(p => p.AsOfDate < today.AsOfDate).ToList();
        var hasSufficientHistory = prices.Count >= 250;

        decimal? fiftyTwoWeekHigh = null;
        decimal? fiftyTwoWeekLow = null;
        bool isNewHigh = false;
        bool isNewLow = false;

        if (hasSufficientHistory)
        {
            var prior52WHigh = prior.Max(p => p.Close);
            var prior52WLow = prior.Min(p => p.Close);

            fiftyTwoWeekHigh = Math.Max(prior52WHigh, today.Close);
            fiftyTwoWeekLow = Math.Min(prior52WLow, today.Close);
            isNewHigh = today.Close > prior52WHigh;
            isNewLow = today.Close < prior52WLow;
        }

        var last30Prior = prior.TakeLast(30).ToList();
        var medianVolume = CalculateMedianVolume(last30Prior);
        var volatility = CalculateVolatility(prices);

        var metrics = new AnalysisMetrics(
            FiftyTwoWeekHighClose: fiftyTwoWeekHigh,
            FiftyTwoWeekLowClose: fiftyTwoWeekLow,
            ThirtyDayMedianVolume: medianVolume,
            FiftyTwoWeekVolatility: volatility);

        var flags = new AnalysisFlags(
            IsNewHigh: isNewHigh,
            IsNewLow: isNewLow,
            IsAbnormalVolume: medianVolume.HasValue && today.Volume.HasValue
                && medianVolume.Value > 0 && today.Volume.Value > 3 * medianVolume.Value);

        return new AnalysisResult(metrics, flags);
    }

    private async Task UpsertAnalysisAsync(int instrumentId, DateOnly asOfDate,
        AnalysisResult result, CancellationToken ct)
    {
        var existing = await context.InstrumentAnalyses
            .FirstOrDefaultAsync(a => a.InstrumentId == instrumentId, ct);

        var m = result.Metrics;

        if (existing == null)
        {
            context.InstrumentAnalyses.Add(new InstrumentAnalysis
            {
                InstrumentId = instrumentId,
                AsOfDate = asOfDate,
                FiftyTwoWeekHighClose = m.FiftyTwoWeekHighClose,
                FiftyTwoWeekLowClose = m.FiftyTwoWeekLowClose,
                ThirtyDayMedianVolume = m.ThirtyDayMedianVolume,
                FiftyTwoWeekVolatility = m.FiftyTwoWeekVolatility,
                UpdatedUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.AsOfDate = asOfDate;
            existing.FiftyTwoWeekHighClose = m.FiftyTwoWeekHighClose;
            existing.FiftyTwoWeekLowClose = m.FiftyTwoWeekLowClose;
            existing.ThirtyDayMedianVolume = m.ThirtyDayMedianVolume;
            existing.FiftyTwoWeekVolatility = m.FiftyTwoWeekVolatility;
            existing.UpdatedUtc = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task PublishEventAsync(ProcessAnalyseInstrumentCommand command, AnalysisFlags flags)
    {
        await bus.Publish(new InstrumentAnalysedEvent(
            command.AnalysisRunId,
            command.PipelineMode,
            command.InstrumentId,
            command.AsOfDate,
            flags.IsNewHigh,
            flags.IsNewLow,
            flags.IsAbnormalVolume));
    }

    public static long? CalculateMedianVolume(List<Price> prices)
    {
        var volumes = prices
            .Where(p => p.Volume.HasValue)
            .Select(p => p.Volume!.Value)
            .OrderBy(v => v)
            .ToList();

        if (volumes.Count == 0)
            return null;

        var mid = volumes.Count / 2;
        return volumes.Count % 2 == 0
            ? (volumes[mid - 1] + volumes[mid]) / 2
            : volumes[mid];
    }

    public static decimal? CalculateVolatility(List<Price> prices)
    {
        if (prices.Count < 250)
            return null;

        var returns = new List<decimal>();
        for (var i = 1; i < prices.Count; i++)
        {
            if (prices[i - 1].Close != 0)
            {
                var dailyReturn = (prices[i].Close - prices[i - 1].Close) / prices[i - 1].Close;
                returns.Add(dailyReturn);
            }
        }

        if (returns.Count == 0)
            return 0m;

        var mean = returns.Average();
        var sumSquaredDiffs = returns.Sum(r => (r - mean) * (r - mean));
        var variance = sumSquaredDiffs / (returns.Count - 1);

        return (decimal)Math.Sqrt((double)variance) * 100m;
    }
}

internal record AnalysisMetrics(
    decimal? FiftyTwoWeekHighClose,
    decimal? FiftyTwoWeekLowClose,
    long? ThirtyDayMedianVolume,
    decimal? FiftyTwoWeekVolatility);

internal record AnalysisFlags(
    bool IsNewHigh,
    bool IsNewLow,
    bool IsAbnormalVolume)
{
    public static AnalysisFlags None => new(false, false, false);
}

internal record AnalysisResult(
    AnalysisMetrics Metrics,
    AnalysisFlags Flags);
