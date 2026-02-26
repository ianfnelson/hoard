using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Alerts;

public record InstrumentAnalysisAlertCommand(
    PipelineMode PipelineMode,
    int InstrumentId,
    DateOnly AsOfDate,
    bool IsNewFiftyTwoWeekHigh,
    bool IsNewFiftyTwoWeekLow,
    bool IsAbnormalVolume) : ICommand;

public class InstrumentAnalysisAlertHandler(
    HoardContext context,
    IAlertService alertService)
    : ICommandHandler<InstrumentAnalysisAlertCommand>
{
    public async Task HandleAsync(InstrumentAnalysisAlertCommand command, CancellationToken ct = default)
    {
        var instrument = await context.Instruments
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == command.InstrumentId, ct);

        if (instrument == null)
            return;

        if (command.IsNewFiftyTwoWeekHigh)
        {
            await alertService.TryRaiseAsync(new AlertRequest
            {
                AlertTypeId = AlertType.FiftyTwoWeekHigh,
                TriggeredDate = command.AsOfDate,
                InstrumentId = command.InstrumentId,
                SuppressionKey = $"{AlertType.FiftyTwoWeekHigh}:{command.InstrumentId}:{command.AsOfDate}",
                Title = $"{instrument.Name} — New 52-Week High",
                Message = $"{instrument.Name} ({instrument.TickerDisplay}) closed at a new 52-week high on {command.AsOfDate.ToIsoDateString()}.",
                Url = $"http://hoard.iannelson.uk/instruments/{command.InstrumentId}",
                UrlTitle = instrument.Name
            }, ct);
        }

        if (command.IsNewFiftyTwoWeekLow)
        {
            await alertService.TryRaiseAsync(new AlertRequest
            {
                AlertTypeId = AlertType.FiftyTwoWeekLow,
                TriggeredDate = command.AsOfDate,
                InstrumentId = command.InstrumentId,
                SuppressionKey = $"{AlertType.FiftyTwoWeekLow}:{command.InstrumentId}:{command.AsOfDate}",
                Title = $"{instrument.Name} — New 52-Week Low",
                Message = $"{instrument.Name} ({instrument.TickerDisplay}) closed at a new 52-week low on {command.AsOfDate.ToIsoDateString()}.",
                Url = $"http://hoard.iannelson.uk/instruments/{command.InstrumentId}",
                UrlTitle = instrument.Name
            }, ct);
        }

        if (command.IsAbnormalVolume)
        {
            await alertService.TryRaiseAsync(new AlertRequest
            {
                AlertTypeId = AlertType.AbnormalVolume,
                TriggeredDate = command.AsOfDate,
                InstrumentId = command.InstrumentId,
                SuppressionKey = $"{AlertType.AbnormalVolume}:{command.InstrumentId}:{command.AsOfDate}",
                Title = $"{instrument.Name} — Abnormal Volume",
                Message = $"{instrument.Name} ({instrument.TickerDisplay}) traded with abnormally high volume on {command.AsOfDate.ToIsoDateString()}.",
                Url = $"http://hoard.iannelson.uk/instruments/{command.InstrumentId}",
                UrlTitle = instrument.Name
            }, ct);
        }
    }
}
