using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.RunLog;

public sealed record CompleteRunCommand(Guid RunId, int RunTypeId, DateTime CompletedAtUtc) : ICommand;

public sealed class CompleteRunHandler(HoardContext context) : ICommandHandler<CompleteRunCommand>
{
    public async Task HandleAsync(CompleteRunCommand command, CancellationToken ct = default)
    {
        var entry = await context.RunLogs.FirstOrDefaultAsync(
            r => r.RunId == command.RunId && r.RunTypeId == command.RunTypeId, ct);
        if (entry is null)
            return;

        entry.CompletedAtUtc = command.CompletedAtUtc;
        await context.SaveChangesAsync(ct);
    }
}
