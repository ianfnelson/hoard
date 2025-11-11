using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Core.Application;

public class TriggerCommandHandler<TCommand>(
    IBus bus,
    ILogger<TriggerCommandHandler<TCommand>> logger)
    : ICommandHandler<TCommand>
    where TCommand : ITriggerCommand
{
    public async Task HandleAsync(TCommand command, CancellationToken ct = default)
    {
        logger.LogDebug("Triggering {Command})", typeof(TCommand).Name);

        await bus.Send(command.ToBusCommand());
    }
}