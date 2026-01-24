using Hoard.Core.Application;
using Hoard.Core.Application.News;
using Hoard.Messages.News;
using Rebus.Handlers;

namespace Hoard.Bus.News;

public class NewsEventHandler(IMediator mediator) : 
    IHandleMessages<RefreshNewsBatchBusCommand>
{
    public async Task Handle(RefreshNewsBatchBusCommand message)
    {
        var appCommand = new ProcessRefreshNewsBatchCommand(
            message.NewsRunId,
            message.PipelineMode,
            message.InstrumentId
        );

        await mediator.SendAsync(appCommand);
    }
}