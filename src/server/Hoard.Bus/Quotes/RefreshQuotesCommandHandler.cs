using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Messages.Quotes;
using Rebus.Bus;
using Rebus.Handlers;

namespace Hoard.Bus.Quotes;

public class RefreshQuotesCommandHandler(IMediator mediator, IBus bus) :
    IHandleMessages<RefreshQuotesBusCommand>,
    IHandleMessages<RefreshQuotesBatchBusCommand>
{
    public async Task Handle(RefreshQuotesBusCommand message)
    {
        var applicationCommand = new ProcessRefreshQuotesCommand();

        await mediator.SendAsync(applicationCommand);

        await bus.Publish(new QuotesRefreshedEvent(message.QuotesRunId));
    }

    public async Task Handle(RefreshQuotesBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshQuotesBatchCommand(message.InstrumentIds);

        await mediator.SendAsync(appCommand);
    }
}
