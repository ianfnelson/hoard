namespace Hoard.Core.Application.RunLog;

public class RunLogDto
{
    public int RunLogId { get; init; }
    public int RunTypeId { get; init; }
    public string RunTypeName { get; init; } = null!;
    public Guid RunId { get; init; }
    public DateTime StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}
