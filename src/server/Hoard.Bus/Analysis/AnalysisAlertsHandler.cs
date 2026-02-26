using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages.Analysis;
using Rebus.Handlers;

namespace Hoard.Bus.Analysis;

public class AnalysisAlertsHandler(IMediator mediator)
    : IHandleMessages<InstrumentAnalysedEvent>
{
    public async Task Handle(InstrumentAnalysedEvent message)
    {
        if (message is { IsNewFiftyTwoWeekHigh: false, IsNewFiftyTwoWeekLow: false, IsAbnormalVolume: false })
        {
            return;
        }

        await mediator.SendAsync(new InstrumentAnalysisAlertCommand(
            message.PipelineMode,
            message.InstrumentId,
            message.AsOfDate,
            message.IsNewFiftyTwoWeekHigh,
            message.IsNewFiftyTwoWeekLow,
            message.IsAbnormalVolume));
    }
}
