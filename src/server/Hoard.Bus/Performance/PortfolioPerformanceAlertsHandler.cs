using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Core.Extensions;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Rebus.Handlers;

namespace Hoard.Bus.Performance;

public class PortfolioPerformanceAlertsHandler(IMediator mediator, TimeProvider timeProvider)
    : IHandleMessages<PortfolioPerformanceCalculatedEvent>
{
    public async Task Handle(PortfolioPerformanceCalculatedEvent message)
    {
        if (message.PipelineMode != PipelineMode.DaytimeReactive)
        {
            return;
        }

        var today = DateOnlyHelper.TodayLocal(timeProvider);

        if (!today.IsWeekday())
        {
            return;
        }

        await mediator.SendAsync(new PortfolioPerformanceAlertCommand(message.PortfolioId, today));
    }
}
