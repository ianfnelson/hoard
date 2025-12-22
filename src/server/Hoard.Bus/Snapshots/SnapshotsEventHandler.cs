using Hoard.Core.Application;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages.Snapshots;
using Rebus.Handlers;

namespace Hoard.Bus.Snapshots;

public class SnapshotsEventHandler(IMediator mediator)
    : IHandleMessages<CalculateSnapshotBusCommand>
{
    public Task Handle(CalculateSnapshotBusCommand message)
    {
        var appCommand = new ProcessCalculateSnapshotCommand(message.SnapshotsRunId, message.PipelineMode,
            message.PortfolioId, message.Year);
        
        return mediator.SendAsync(appCommand);
    }
}