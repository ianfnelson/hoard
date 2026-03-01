using Hoard.Core.Application.RunLog;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application;

public class TriggerCommandHandler<TCommand>(
    IBus bus,
    IRunLogService runLogService,
    ILogger<TriggerCommandHandler<TCommand>> logger)
    : ICommandHandler<TCommand>
    where TCommand : ITriggerCommand
{
    public async Task HandleAsync(TCommand command, CancellationToken ct = default)
    {
        logger.LogDebug("Triggering {Command})", typeof(TCommand).Name);

        await runLogService.RecordStartAsync(command.RunTypeId, command.RunId, DateTime.UtcNow, ct);

        await bus.Send(command.ToBusCommand());
    }
}