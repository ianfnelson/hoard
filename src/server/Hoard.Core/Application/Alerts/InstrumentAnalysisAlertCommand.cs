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
    IAlertService alertService,
    TimeProvider timeProvider)
    : ICommandHandler<InstrumentAnalysisAlertCommand>
{
    public async Task HandleAsync(InstrumentAnalysisAlertCommand command, CancellationToken ct = default)
    {
        if (command.AsOfDate != DateOnlyHelper.TodayLocal(timeProvider))
        {
            return;
        }

        if (!command.AsOfDate.IsWeekday())
        {
            return;
        }

        var instrument = await context.Instruments
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == command.InstrumentId, ct);

        if (instrument == null)
            return;

        await EvaluateFiftyTwoWeekHigh(command, instrument, ct);
        await EvaluateFiftyTwoWeekLow(command, instrument, ct);
        await EvaluateAbnormalVolume(command, instrument, ct);
    }

    private async Task EvaluateFiftyTwoWeekHigh(
        InstrumentAnalysisAlertCommand command, Instrument instrument, CancellationToken ct)
    {
        if (!command.IsNewFiftyTwoWeekHigh)
            return;

        await alertService.TryRaiseAsync(new AlertRequest
        {
            AlertTypeId = AlertType.FiftyTwoWeekHigh,
            TriggeredDate = command.AsOfDate,
            InstrumentId = command.InstrumentId,
            SuppressionKey = $"{AlertType.FiftyTwoWeekHigh}:{command.InstrumentId}:{command.AsOfDate.ToIsoDateString()}",
            Title = $"{instrument.Ticker} — New 52-Week High",
            Message = $"{instrument.Name} closed at a new 52-week high.",
            Url = HoardUrls.Instrument(command.InstrumentId),
            UrlTitle = instrument.Name
        }, ct);
    }

    private async Task EvaluateFiftyTwoWeekLow(
        InstrumentAnalysisAlertCommand command, Instrument instrument, CancellationToken ct)
    {
        if (!command.IsNewFiftyTwoWeekLow)
            return;

        await alertService.TryRaiseAsync(new AlertRequest
        {
            AlertTypeId = AlertType.FiftyTwoWeekLow,
            TriggeredDate = command.AsOfDate,
            InstrumentId = command.InstrumentId,
            SuppressionKey = $"{AlertType.FiftyTwoWeekLow}:{command.InstrumentId}:{command.AsOfDate.ToIsoDateString()}",
            Title = $"{instrument.Ticker} — New 52-Week Low",
            Message = $"{instrument.Name} closed at a new 52-week low.",
            Url = HoardUrls.Instrument(command.InstrumentId),
            UrlTitle = instrument.Name
        }, ct);
    }

    private async Task EvaluateAbnormalVolume(
        InstrumentAnalysisAlertCommand command, Instrument instrument, CancellationToken ct)
    {
        if (!command.IsAbnormalVolume)
            return;

        await alertService.TryRaiseAsync(new AlertRequest
        {
            AlertTypeId = AlertType.AbnormalVolume,
            TriggeredDate = command.AsOfDate,
            InstrumentId = command.InstrumentId,
            SuppressionKey = $"{AlertType.AbnormalVolume}:{command.InstrumentId}:{command.AsOfDate.ToIsoDateString()}",
            Title = $"{instrument.Ticker} — Abnormal Volume",
            Message = $"{instrument.Name} traded with abnormally high volume.",
            Url = HoardUrls.Instrument(command.InstrumentId),
            UrlTitle = instrument.Name
        }, ct);
    }
}
