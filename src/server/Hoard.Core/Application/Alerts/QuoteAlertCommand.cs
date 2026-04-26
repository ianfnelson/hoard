using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Alerts;

public record QuoteAlertCommand(int InstrumentId, decimal RegularMarketChangePercent) : ICommand;

public class QuoteAlertHandler(
    HoardContext context,
    IAlertService alertService,
    TimeProvider timeProvider)
    : ICommandHandler<QuoteAlertCommand>
{
    public async Task HandleAsync(QuoteAlertCommand command, CancellationToken ct = default)
    {
        var nowUk = DateOnlyHelper.NowUk(timeProvider);
        var today = DateOnly.FromDateTime(nowUk);

        if (!today.IsWeekday())
            return;

        var ukTime = TimeOnly.FromDateTime(nowUk);
        if (ukTime < new TimeOnly(9, 0) || ukTime >= new TimeOnly(16, 30))
            return;

        var instrument = await context.Instruments
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == command.InstrumentId, ct);

        if (instrument == null)
            return;

        var analysis = await context.InstrumentAnalyses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.InstrumentId == command.InstrumentId, ct);

        if (analysis?.FiftyTwoWeekVolatility is not > 0)
            return;

        if (Math.Abs(command.RegularMarketChangePercent) <= 2.5m * analysis.FiftyTwoWeekVolatility.Value)
            return;

        var direction = command.RegularMarketChangePercent >= 0 ? "up" : "down";
        var absPct = Math.Abs(command.RegularMarketChangePercent);

        await alertService.TryRaiseAsync(new AlertRequest
        {
            AlertTypeId = AlertType.QuoteMovement,
            TriggeredDate = today,
            InstrumentId = command.InstrumentId,
            SuppressionKey = $"{AlertType.QuoteMovement}:{command.InstrumentId}:{today.ToIsoDateString()}",
            Title = $"{instrument.Ticker} {direction} {absPct:F2}%",
            Message = $"{instrument.Name} is {direction} {absPct:F2}%",
            Url = HoardUrls.Instrument(command.InstrumentId),
            UrlTitle = instrument.Name
        }, ct);
    }
}
