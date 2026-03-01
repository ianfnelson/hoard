namespace Hoard.Core.Application.RunLog;

public interface IRunLogService
{
    Task RecordStartAsync(int runTypeId, Guid runId, DateTime startedAtUtc, CancellationToken ct = default);
}
