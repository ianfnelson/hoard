using Hoard.Core.Data;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.RunLog;

public class RunLogService(HoardContext context, ILogger<RunLogService> logger) : IRunLogService
{
    public async Task RecordStartAsync(int runTypeId, Guid runId, DateTime startedAtUtc, CancellationToken ct = default)
    {
        var entry = new Domain.Entities.RunLog
        {
            RunTypeId = runTypeId,
            RunId = runId,
            StartedAtUtc = startedAtUtc,
        };
        context.RunLogs.Add(entry);
        await context.SaveChangesAsync(ct);
        logger.LogDebug("Run started: RunTypeId={RunTypeId} {RunId}", runTypeId, runId);
    }
}
