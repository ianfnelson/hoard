namespace Hoard.Core.Domain.Entities;

public class RunLog : Entity<int>
{
    public int RunTypeId { get; set; }
    public RunType RunType { get; set; } = null!;
    public Guid RunId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
