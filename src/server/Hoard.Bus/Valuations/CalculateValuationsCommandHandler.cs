using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.Valuations;

public class CalculateValuationsCommandHandler(IMediator mediator)
    : IHandleMessages<CalculateHoldingValuationsBusCommand>,
        IHandleMessages<CalculatePortfolioValuationBusCommand>
{
    public async Task Handle(CalculateHoldingValuationsBusCommand message)
    {
        var appCommand = new ProcessCalculateHoldingValuationsCommand(message.ValuationsRunId, message.PipelineMode, message.InstrumentId, message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }

    public async Task Handle(CalculatePortfolioValuationBusCommand message)
    {
        var appCommand = new ProcessCalculatePortfolioValuationCommand(message.ValuationsRunId, message.PipelineMode, message.PortfolioId, message.AsOfDate);

        await mediator.SendAsync(appCommand);
    }
}
