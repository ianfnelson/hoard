using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Messages.Holdings;
using Rebus.Handlers;

namespace Hoard.Bus.Holdings;

public class CalculateHoldingsCommandHandler(IMediator mediator)
    : IHandleMessages<CalculateHoldingsBusCommand>
{
    public async Task Handle(CalculateHoldingsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingsCommand(message.HoldingsRunId, message.PipelineMode, message.AsOfDate);
        await mediator.SendAsync(appCommand);
    }
}
